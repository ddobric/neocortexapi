using NeoCortexApi.Entities;
using NeoCortexApi.Exception;
using NeoCortexApi.Utility;
using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Collections;

namespace NeoCortexApi.Classifiers 
{
    public class SdrClassifier<TIN, TOUT> : ScalarEncoder, IClassifier<TIN, TOUT> 
    {
        private double alpha = 0.001;
        private int learnIteration;
        private int recordNumMinusLearnIteration = -1;
        private int maxInputIdx;
        private int maxBucketIdx;


        /// <summary>
        /// Stores and updates the weight matrix, which is used in predicting the bucket number
        /// </summary>
        public FlexComRowMatrix<object> weightMatrix;

        /// <summary>
        /// Stores the patternHistory with respect to the iteration number
        /// e.g  1 : 1,2,3
        ///      2 : 1,3,4
        ///      3 : 1,2,5
        /// </summary>
        List<Tuple<int, object>> patternNzHistory;

        /// <summary>
        /// Stores the bucket number and its entries
        /// </summary>
        public Dictionary<int, List<object>> bucketEntries;

        /// <summary>
        /// Represents the default constructor
        /// </summary>
        public SdrClassifier() : this(0.001)
        {
         
        }

        /// <summary>
        /// Represents the constructor in which we can alpha can be assigned.
        /// By default value is take as 0.001. This value can be changed in order to increase or decrease the learning process
        /// Larger the value of alpha less iteration are required to learn and vice-versa.
        /// NOTE: alpha value should always be greater than 0 in order to make the learning happen
        /// </summary>
        /// <param name = "alpha"> represents some random value given by the user</param>>
        public SdrClassifier(double alpha)
        {
            this.alpha = alpha;
            InitializeEntries();
        }

        /// <summary>
        /// Initializes the global variables.
        /// </summary>
        private void InitializeEntries()
        {
            patternNzHistory = new List<Tuple<int, object>>();
            weightMatrix = new FlexComRowMatrix<object>();
            bucketEntries = new Dictionary<int, List<object>>();
        }

        /// <summary>
        /// Function to fetch bucket index from encoders and store it in our dictionary
        /// </summary>summary>
/*        private void getBucketEntries()
        {
            bucketEntries = GetBucketIndex(Dictionary<int, List<object>>);
        }
*/
        /// <summary>
        /// Method computes the result after the data is provided by the temporal memory.
        /// </summary>
        /// <param name="recordNum"> the nth number of the iteration </param>
        /// <param name = "classification"> represents list of object with 2 values one is bucket index and second is the actual value that came into the bucket
        /// This information is from the encoder itself and using this classifier checks the error. key is the bucket-index and value
        /// is the entry that went into the bucket</param>>
        /// <param name = "patternNz"> represents 1D array of the patterns. This input is from the temporal memory</param>>
        public void Compute(int recordNum, List<object> classification, int[] patternNz)
        {

            // throws object should not be null exception if classification object is null
            if (classification == null)
            {
                throw new ObjectShouldNotBeNUllException(ExceptionConstants.CLASSIFICATION_CANNOT_BE_NULL);
            }

            // throws object should not be null exception if patternNZ is null or its length is zero
            if (patternNz == null || patternNz.Length == 0)
            {
                throw new ObjectShouldNotBeNUllException(ExceptionConstants.PATTERN_NZ_CANNOT_BE_NULL);
            }

            if (recordNumMinusLearnIteration == -1)
            {
                recordNumMinusLearnIteration = recordNum - learnIteration;
            }
            learnIteration = recordNum - recordNumMinusLearnIteration;
            patternNzHistory.Add(Tuple.Create(learnIteration, (object)patternNz));

            if (ArrayUtils.Max(patternNz) > maxInputIdx)
            {
                int newMaxInputIdx = ArrayUtils.Max(patternNz);

                // Initializes the matrix with zero padding upto the maximum input
                GrowMatrixUptoMaximumInput(newMaxInputIdx);

                // Sets the maximum input index, basically is the maximum number of active bits used by the temporal memory.
                maxInputIdx = newMaxInputIdx;
            }

            Learn(classification);
        }

        /// <summary>
        /// Learns after the computation is done successfully.
        /// </summary>
        /// <param name = "classification"> represents list of object with 2 values one is bucket index and second is the actual value that came into the bucket
        /// </param>
        private void Learn(List<object> classification)
        {
            int bucketIdx = (int)GetBucketIndex(classification[0]); // gives bucket index
            object actValue = classification[1];// gives actual value in the bucket
            if (bucketIdx > maxBucketIdx)
            {
                AddBucketsToWeightMatrix(bucketIdx);
                maxBucketIdx = bucketIdx;
            }
            UpdateBucketEntries(bucketIdx, actValue);
            UpdateWeightMatrix(classification);
        }

        /// <summary>
        /// Method predicts the result by informing us in which bucket we should check for this particular pattern.
        /// </summary>
        /// <param name = "patternNz"> This input parameter patternNz is the input from the temporal memory.
        /// Gives information about the number of active bits</param>
        /// <returns>1-d array containing the probabilities of the bucket indexes that can come for the next pattern.
        /// The bucket with the highest probability will be chosen for the next prediction</returns>
        public double[] Predict(int[] patternNz)
        {
            // throws empty bucket exception if bucket entry count is zero
            if (bucketEntries.Count == 0)
            {
                throw new EmptyBucketException(ExceptionConstants.EMPTY_BUCKET_EXCEPTION);
            }
            double[] predictedValues = inferSingleStep(patternNz);
            return predictedValues;
        }

        /// <summary>
        /// Updates the Weight Matrix after the error calculation is done
        /// </summary>
        /// <param name = "classification"> represents list of object with 2 values one is bucket index and second is the actual value that came into the bucket
        /// </param>
        private void UpdateWeightMatrix(List<object> classification)
        {
            foreach (Tuple<int, object> t in patternNzHistory)
            {
                var learnPatternNz = (int[])t.Item2;
                double[] error = CalculateError(classification);

                for (int row = 0; row <= maxBucketIdx; row++)
                {
                    foreach (int bit in learnPatternNz)
                    {
                        weightMatrix.AddAndUpdate(row, bit, alpha * error[row]);
                    }
                }
            }
        }

        /// <summary>
        /// Method calculates the error
        /// </summary>
        /// <param name = "classification"> represents list of object with 2 values one is bucket index and second is the actual value that came into the bucket
        /// </param>
        /// <returns>1-d array of the errors calculated for each bucket.</returns>
        private double[] CalculateError(List<object> classification)
        {
            double[] error = new double[maxBucketIdx + 1];
            int[] targetDist = new int[maxBucketIdx + 1];

            // target bucket entry should approach to 1
            targetDist[Convert.ToInt32(classification[0])] = 1;

            foreach (Tuple<int, object> t in patternNzHistory)
            {
                var learnPatternNz = (int[])t.Item2;
                double[] predictDist = inferSingleStep(learnPatternNz);
                double[] targetDistMinusPredictDist = new double[maxBucketIdx + 1];
                for (int i = 0; i <= maxBucketIdx; i++)
                {
                    targetDistMinusPredictDist[i] = targetDist[i] - predictDist[i];
                }
                error = targetDistMinusPredictDist;
            }
            return error;
        }

        /// <summary>
        /// Method infers the next step by using mathematical tool known as soft max normalization.
        /// </summary>
        /// <param name = "patternNz"> represents 1-D array</param>>
        /// <returns>1-d array containing the prediction results after soft max normalization has been done</returns>
        private double[] inferSingleStep(int[] patternNz)
        {
            double[] outputActivationSum = new double[maxBucketIdx + 1];
            double[] predictDistribution = new double[outputActivationSum.Length];
            for (int row = 0; row <= maxBucketIdx; row++)
            {
                foreach (int bit in patternNz)
                {
                    outputActivationSum[row] += Convert.ToDouble(weightMatrix.Matrix[row][bit]);
                }
            }

            PerformSoftMaxNormalization(outputActivationSum, predictDistribution);
            return predictDistribution;
        }

        /// <summary>
        /// Increases the columns of the weight matrix. This should be matched with the number of active bits/ active pattern
        /// as per the temporal memory.
        /// </summary>
        /// <param name = "newMaxInputIdx"> maximum index value used by the temporal memory while forming active patterns</param>>
        private void GrowMatrixUptoMaximumInput(int newMaxInputIdx)
        {
            for (int i = 0; i < maxBucketIdx + 1; i++)
            {
                List<object> inputList;
                try
                {
                    inputList = weightMatrix.Matrix[i];

                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine("Caught an exception" + ex.Message + ", Unable to get the bucket adding a new bucket number :" + i);
                    inputList = new List<object>();
                    AddZeros(0, newMaxInputIdx, inputList);
                    weightMatrix.Matrix.Add(inputList);
                    continue;
                }

                AddZeros(maxInputIdx, newMaxInputIdx - 1, inputList);
                weightMatrix.Matrix[i] = inputList;
            }
        }

        /// <summary>
        /// Initializes the weight matrix with 0 values
        /// </summary>
        /// <param name="startingPoint"> represents the starting point of the matrix </param>>
        /// <param name="endingPoint"> represents the ending point of the matrix </param>>
        /// <param name="list"> represents the list of the object </param>>
        private void AddZeros(int startingPoint, int endingPoint, List<object> list)
        {
            for (int num = startingPoint; num < endingPoint + 1; num++)
            {
                list.Add(0);
            }
        }



        /// <summary>
        /// Increases the number of rows of the weight matrix in order to match with the maximum bucket index used by the encoder
        /// </summary>
        /// <param name="bucketIdx"> bucket index used by the encoder</param>
        private void AddBucketsToWeightMatrix(int bucketIdx)
        {
            for (int i = maxBucketIdx; i < bucketIdx; i++)
            {
                List<object> list = new List<object>();
                for (int j = 0; j < maxInputIdx + 1; j++)
                {
                    list.Add(0);
                }
                weightMatrix.Matrix.Add(list);
            }
        }


        /// <summary>
        /// Updates the bucket index with the actual value.
        /// This is our internal bucket entries stored which can be used to predict the values
        /// </summary>
        /// <param name="bucketIdx"> bucket index used by the encoder</param>
        /// <param name="actValue"> actual value set by the encoder</param>
        private void UpdateBucketEntries(int bucketIdx, object actValue)
        {
            List<object> inputEntries;
            if (bucketEntries.ContainsKey(bucketIdx))
            {
                inputEntries = bucketEntries[bucketIdx];
                inputEntries.Add(actValue);
            }
            else
            {
                inputEntries = new List<object> { actValue };
            }
            bucketEntries[bucketIdx] = inputEntries;
        }


        /// <summary>
        /// Performs SoftMaxNormalization using mathematical formulas.
        /// </summary>
        /// <param name="outputActivation"> represents 1D array of output activation</param>
        /// <param name="predictDist"> represents 1D array of predicted distribution</param>
        private void PerformSoftMaxNormalization(double[] outputActivation, double[] predictDist)
        {
            double[] expOutputActivation = new double[outputActivation.Length];
            for (int i = 0; i < expOutputActivation.Length; i++)
            {
                // to find the probability
                expOutputActivation[i] = Math.Exp(outputActivation[i]);
            }

            for (int i = 0; i < predictDist.Length; i++)
            {
                predictDist[i] = expOutputActivation[i] / ArrayUtils.Sum(expOutputActivation);
            }

        }

        /// <summary>
        /// Prints the weight Matrix
        /// </summary>
        public void PrintWeightMatrix()
        {
            Console.WriteLine("Maximum input index :" + maxInputIdx);
            Console.WriteLine("Maximum bucket index :" + maxBucketIdx);

            for (int i = 0; i < maxBucketIdx + 1; i++)
            {
                for (int j = 0; j < maxInputIdx + 1; j++)
                {
                    Console.Write(weightMatrix.Matrix[i][j] + " ");
                }

                Console.WriteLine("\n");
            }
        }

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }

        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            throw new NotImplementedException();
        }

        public void Learn(TIN input, Cell[] output)
        {
            throw new NotImplementedException();
        }

        public List<ClassifierResult<TIN>> GetPredictedInputValues(int[] cellIndicies, short howMany = 1)
        {
            throw new NotImplementedException();
        }
    }
}



//namespace namespace
//{

//    using deque = collections.deque;

//    using numpy;

//    using serializable = nupic.serializable.serializable;

//    using capnp;

//    using sdrclassifierproto = nupic.proto.sdrclassifier_capnp.sdrclassifierproto;

//    using system.collections.generic;

//    using system;

//    using system.linq;

//    public static class module
//    {

//        static module()
//        {

//        }

//        public static object capnp = null;


//        the sdr classifier accepts a binary input pattern from the
//           level below(the "activationpattern") and information from the sensor and
//           encoders(the "classification") describing the true (target) input.

//           the sdr classifier maps input patterns to class labels. there are as many
//           output units as the number of class labels or buckets(in the case of scalar
//           encoders). the output is a probabilistic distribution over all class labels.
         
//           during inference, the output is calculated by first doing a weighted summation
//           of all the inputs, and then perform a softmax nonlinear function to get
//           the predicted distribution of class labels


//           during learning, the connection weights between input units and output units
//           are adjusted to maximize the likelihood of the model

//           example usage:
         
         
//              c = sdrclassifier(steps=[1], alpha= 0.1, actvaluealpha= 0.1, verbosity= 0)

//              # learning
//              c.compute(recordnum=0, patternnz=[1, 5, 9],
//                        classification={"bucketidx": 4, "actvalue": 34.7},
//                        learn=true, infer=false)
         
//              # inference
//              result = c.compute(recordnum=1, patternnz=[1, 5, 9],
//                                 classification={"bucketidx": 4, "actvalue": 34.7},
//                                 learn=false, infer=true)
         
//              # print the top three predictions for 1 steps out.
//              toppredictions = sorted(zip(result[1],
//                                      result["actualvalues"]), reverse= true)[:3]
//              for probability, value in toppredictions:
//                print "prediction of {} has probability of {}.".format(value,
//                                                                       probability*100.0)


//           :param steps: (list) sequence of the different steps of multi-step predictions
//             to learn
//           :param alpha: (float) the alpha used to adapt the weight matrix during
//             learning.a larger alpha results in faster adaptation to the data.
//           :param actvaluealpha: (float) used to track the actual value within each
//             bucket.a lower actvaluealpha results in longer term memory
//           :param verbosity: (int) verbosity level, can be 0, 1, or 2
         
//           :raises: (valueerror) when record number does not increase monotonically.


//        public class sdrclassifier
//            : serializable
//        {

//            public object version = 1;

//            public sdrclassifier(object steps = new list<object> {
//                1
//            }, object alpha = 0.001, object actvaluealpha = 0.3, object verbosity = 0)
//            {
//                if (steps.count == 0)
//                {
//                    throw typeerror("steps cannot be empty");
//                }
//                if (!all(from item in steps
//                         select item is int))
//                {
//                    throw typeerror("steps must be a list of ints");
//                }
//                if (any(from item in steps
//                        select item < 0))
//                {
//                    throw valueerror("steps must be a list of non-negative ints");
//                }
//                if (alpha < 0)
//                {
//                    throw valueerror("alpha (learning rate) must be a positive number");
//                }
//                if (actvaluealpha < 0 || actvaluealpha >= 1)
//                {
//                    throw valueerror("actvaluealpha be a number between 0 and 1");
//                }
//                save constructor args
//                this.steps = steps;
//                this.alpha = alpha;
//                this.actvaluealpha = actvaluealpha;
//                this.verbosity = verbosity;
//                max # of steps of prediction we need to support
//                this._maxsteps = max(this.steps) + 1;
//                history of the last _maxsteps activation patterns.we need to keep
//                 these so that we can associate the current iteration's classification
//                 with the activationpattern from n steps ago
//                this._patternnzhistory = deque(maxlen: this._maxsteps);
//                this contains the value of the highest input number we've ever seen
//                 it is used to pre - allocate fixed size arrays that hold the weights
//                this._maxinputidx = 0;
//                this contains the value of the highest bucket index we've ever seen
//                 it is used to pre - allocate fixed size arrays that hold the weights of
//                 each bucket index during inference
//                this._maxbucketidx = 0;
//                the connection weight matrix
//                this._weightmatrix = new dictionary<object, object>();
//                foreach (var step in this.steps)
//                {
//                    this._weightmatrix[step] = numpy.zeros(shape: (this._maxinputidx + 1, this._maxbucketidx + 1));
//                }
//                this keeps track of the actual value to use for each bucket index.we

//                start with 1 bucket, no actual value so that the first infer has something

//                to return
//               this._actualvalues = new list<object> {
//                    null
//               };
//                set the version to the latest version.
//                this is used for serialization / deserialization

//               this._version = sdrclassifier.version;
//            }


//            process one input sample.

//            this method is called by outer loop code outside the nupic-engine.we
//            use this instead of the nupic engine compute() because our inputs and
//            outputs aren't fixed size vectors of reals.
             
             
//                 :param recordnum: record number of this input pattern. record numbers

//              normally increase sequentially by 1 each time unless there are missing

//              records in the dataset. knowing this information insures that we don't get

//              confused by missing records.
             
//                 :param patternnz: list of the active indices from the output below.when the

//              input is from temporalmemory, this list should be the indices of the
//              active cells.
             
//                 :param classification: dict of the classification information where:
             
//                   - bucketidx: list of indices of the encoder bucket
//                   - actvalue: list of actual values going into the encoder


//              classification could be none for inference mode.
//                 :param learn: (bool) if true, learn this sample
//                 :param infer: (bool) if true, perform inference
             
//                 :return:    dict containing inference results, there is one entry for each
//                        step in self.steps, where the key is the number of steps, and
//                        the value is an array containing the relative likelihood for

//                        each bucketidx starting from bucketidx 0.


//                        there is also an entry containing the average actual value to
//                        use for each bucket. the key is 'actualvalues'.
             
//                             for example:
             
//                             .. code-block::python


//                                {
//                1 :             [0.1, 0.3, 0.2, 0.7],
//                                  4 :             [0.2, 0.4, 0.3, 0.5],
//                                  'actualvalues': [1.5, 3,5, 5,5, 7.6],
//                                }

//            public virtual object compute(
//                object recordnum,
//                object patternnz,
//                object classification,
//                object learn,
//                object infer)
//            {
//                object nsteps;
//                object numcategory;
//                object actvaluelist;
//                object bucketidxlist;
//                if (this.verbosity >= 1)
//                {
//                    console.writeline("  learn:", learn);
//                    console.writeline("  recordnum:", recordnum);
//                    console.writeline(string.format("  patternnz (%d):", patternnz.count), patternnz);
//                    console.writeline("  classificationin:", classification);
//                }
//                ensures that recordnum increases monotonically
//                if (this._patternnzhistory.count > 0)
//                {
//                    if (recordnum < this._patternnzhistory[-1][0])
//                    {
//                        throw valueerror("the record number has to increase monotonically");
//                    }
//                }
//                store pattern in our history if this is a new record
//                if (this._patternnzhistory.count == 0 || recordnum > this._patternnzhistory[-1][0])
//                {
//                    this._patternnzhistory.append((recordnum, patternnz));
//                }
//                to allow multi -class classification, we need to be able to run learning
//                 without inference being on.so initialize retval outside
//                 of the inference block.
//                var retval = new dictionary<object, object>
//                {
//                };
//            update maxinputidx and augment weight matrix with zero padding
//                if (max(patternnz) > this._maxinputidx)
//                {
//                    var newmaxinputidx = max(patternnz);
//                    foreach (var nsteps in this.steps)
//                    {
//                        this._weightmatrix[nsteps] = numpy.concatenate((this._weightmatrix[nsteps], numpy.zeros(shape: (newmaxinputidx - this._maxinputidx, this._maxbucketidx + 1))), axis: 0);
//            }
//                    this._maxinputidx = convert.toint32(newmaxinputidx);
//                }
//        get classification info
//                if (classification != null)
//                {
//                    if (type(classification["bucketidx"]) != list)
//                    {
//                        bucketidxlist = new list<object> {
//                            classification["bucketidx"]
//    };
//    actvaluelist = new list<object> {
//                            classification["actvalue"]
//};
//numcategory = 1;
//                    }
//                    else
//{
//    bucketidxlist = classification["bucketidx"];
//    actvaluelist = classification["actvalue"];
//    numcategory = classification["bucketidx"].count;
//}
//                }
//                else
//{
//    if (learn)
//    {
//        throw valueerror("classification cannot be none when learn=true");
//    }
//    actvaluelist = null;
//    bucketidxlist = null;
//}
//------------------------------------------------------------------------
//inference:
//                 for each active bit in the activationpattern, get the classification
//                 votes
//                if (infer)
//                {
//    retval = this.infer(patternnz, actvaluelist);
//}
//if (learn && classification["bucketidx"] != null)
//{
//    foreach (var categoryi in enumerable.range(0, numcategory))
//    {
//        var bucketidx = bucketidxlist[categoryi];
//        var actvalue = actvaluelist[categoryi];
//        update maxbucketindex and augment weight matrix with zero padding
//                        if (bucketidx > this._maxbucketidx)
//        {
//            foreach (var nsteps in this.steps)
//            {
//                this._weightmatrix[nsteps] = numpy.concatenate((this._weightmatrix[nsteps], numpy.zeros(shape: (this._maxinputidx + 1, bucketidx - this._maxbucketidx))), axis: 1);
//            }
//            this._maxbucketidx = convert.toint32(bucketidx);
//        }
//        update rolling average of actual values if it's a scalar. if it's
//        not, it must be a category, in which case each bucket only ever
//                         sees one category so we don't need a running average.
//                        while (this._maxbucketidx > this._actualvalues.count - 1)
//        {
//            this._actualvalues.append(null);
//        }
//        if (this._actualvalues[bucketidx] == null)
//        {
//            this._actualvalues[bucketidx] = actvalue;
//        }
//        else if (actvalue is int || actvalue is float || actvalue is long)
//        {
//            this._actualvalues[bucketidx] = (1.0 - this.actvaluealpha) * this._actualvalues[bucketidx] + this.actvaluealpha * actvalue;
//        }
//        else
//        {
//            this._actualvalues[bucketidx] = actvalue;
//        }
//    }
//    foreach (var _tup_1 in this._patternnzhistory)
//    {
//        var learnrecordnum = _tup_1.item1;
//        var learnpatternnz = _tup_1.item2;
//        var error = this._calculateerror(recordnum, bucketidxlist);
//        nsteps = recordnum - learnrecordnum;
//        if (this.steps.contains(nsteps))
//        {
//            foreach (var bit in learnpatternnz)
//            {
//                this._weightmatrix[nsteps][bit, ":"] += this.alpha * error[nsteps];
//            }
//        }
//    }
//}
//------------------------------------------------------------------------
//verbose print
//                if (infer && this.verbosity >= 1)
//{
//    console.writeline("  inference: combined bucket likelihoods:");
//    console.writeline("    actual bucket values:", retval["actualvalues"]);
//    foreach (var _tup_2 in retval.items())
//    {
//        nsteps = _tup_2.item1;
//        var votes = _tup_2.item2;
//        if (nsteps == "actualvalues")
//        {
//            continue;
//        }
//        console.writeline(string.format("    %d steps: ", nsteps), _pformatarray(votes));
//        var bestbucketidx = votes.argmax();
//        console.writeline(string.format("      most likely bucket idx: %d, value: %s", bestbucketidx, retval["actualvalues"][bestbucketidx]));
//    }
//    console.writeline();
//}
//return retval;
//            }

             
//                 return the inference value from one input sample. the actual
//                 learning happens in compute().
             
//                 :param patternnz: list of the active indices from the output below
//                 :param classification: dict of the classification information:
//                                 bucketidx: index of the encoder bucket
//                                 actvalue:  actual value going into the encoder
             
//                 :return:    dict containing inference results, one entry for each step in
//                             self.steps. the key is the number of steps, the value is an
//                             array containing the relative likelihood for each bucketidx
//                             starting from bucketidx 0.
             
//                             for example:
             
//                             ..code - block::python


//                                {
//    'actualvalues': [0.0, 1.0, 2.0, 3.0]
//                                  1 : [0.1, 0.3, 0.2, 0.7]
//                                  4 : [0.2, 0.4, 0.3, 0.5]}

//public virtual object infer(object patternnz, object actvaluelist)
//{
//    object defaultvalue;
//    return value dict.for buckets which we don't have an actual value
//                 for yet, just plug in any valid actual value.it doesn't matter what
//                 we use because that bucket won't have non-zero likelihood anyways.
//                 note: if doing 0 - step prediction, we shouldn't use any knowledge
//                  of the classification input during inference.
//                if (this.steps[0] == 0 || actvaluelist == null)
//        {
//            defaultvalue = 0;
//        }
//        else
//        {
//            defaultvalue = actvaluelist[0];
//        }
//    var actvalues = (from x in this._actualvalues
//                     select x != null ? x : defaultvalue).tolist();
//    var retval = new dictionary<object, object> {
//                    {
//                        "actualvalues",
//                        actvalues}};
//    foreach (var nsteps in this.steps)
//    {
//        var predictdist = this.infersinglestep(patternnz, this._weightmatrix[nsteps]);
//        retval[nsteps] = predictdist;
//    }
//    return retval;
//}


//perform inference for a single step. given an sdr input and a weight
//                 matrix, return a predicted distribution.
             
//                 :param patternnz: list of the active indices from the output below
//                 :param weightmatrix: numpy array of the weight matrix
//                 :return: numpy array of the predicted class label distribution

//            public virtual object infersinglestep(object patternnz, object weightmatrix)
//{
//    var outputactivation = weightmatrix[patternnz].sum(axis: 0);
//    softmax normalization
//                outputactivation = outputactivation - numpy.max(outputactivation);
//    var expoutputactivation = numpy.exp(outputactivation);
//    var predictdist = expoutputactivation / numpy.sum(expoutputactivation);
//    return predictdist;
//}

//[classmethod]
//public static object getschema(object cls)
//{
//    return sdrclassifierproto;
//}

//[classmethod]
//public static object read(object cls, object proto)
//{
//    var classifier = object.@__new__(cls);
//    classifier.steps = (from step in proto.steps
//                        select step).tolist();
//    classifier.alpha = proto.alpha;
//    classifier.actvaluealpha = proto.actvaluealpha;
//    classifier._patternnzhistory = deque(maxlen: max(classifier.steps) + 1);
//    var patternnzhistoryproto = proto.patternnzhistory;
//    var recordnumhistoryproto = proto.recordnumhistory;
//    foreach (var i in xrange(patternnzhistoryproto.count))
//    {
//        classifier._patternnzhistory.append((recordnumhistoryproto[i], patternnzhistoryproto[i].tolist()));
//    }
//    classifier._maxsteps = proto.maxsteps;
//    classifier._maxbucketidx = proto.maxbucketidx;
//    classifier._maxinputidx = proto.maxinputidx;
//    classifier._weightmatrix = new dictionary<object, object>
//    {
//    };
//    var weightmatrixproto = proto.weightmatrix;
//    foreach (var i in xrange(weightmatrixproto.count))
//    {
//        classifier._weightmatrix[weightmatrixproto[i].steps] = numpy.reshape(weightmatrixproto[i].weight, newshape: (classifier._maxinputidx + 1, classifier._maxbucketidx + 1));
//    }
//    classifier._actualvalues = new list<object>();
//    foreach (var actvalue in proto.actualvalues)
//    {
//        if (actvalue == 0)
//        {
//            classifier._actualvalues.append(null);
//        }
//        else
//        {
//            classifier._actualvalues.append(actvalue);
//        }
//    }
//    classifier._version = proto.version;
//    classifier.verbosity = proto.verbosity;
//    return classifier;
//}

//public virtual object write(object proto)
//{
//    var stepsproto = proto.init("steps", this.steps.count);
//    foreach (var i in xrange(this.steps.count))
//    {
//        stepsproto[i] = this.steps[i];
//    }
//    proto.alpha = this.alpha;
//    proto.actvaluealpha = this.actvaluealpha;
//note: technically, saving `_maxsteps` is redundant, since it may be
//                 reconstructed from `self.steps` just as in the constructor. eliminating
//                 this attribute from the capnp scheme will involve coordination with
//                 nupic.core, where the `sdrclassifierproto` schema resides.
//                proto.maxsteps = this._maxsteps;
//note: size of history buffer may be less than `self._maxsteps` if fewer
//                 inputs had been processed
//                var patternproto = proto.init("patternnzhistory", this._patternnzhistory.count);
//    var recordnumhistoryproto = proto.init("recordnumhistory", this._patternnzhistory.count);
//    foreach (var i in xrange(this._patternnzhistory.count))
//    {
//        var subpatternproto = patternproto.init(i, this._patternnzhistory[i][1].count);
//        foreach (var j in xrange(this._patternnzhistory[i][1].count))
//        {
//            subpatternproto[j] = convert.toint32(this._patternnzhistory[i][1][j]);
//        }
//        recordnumhistoryproto[i] = convert.toint32(this._patternnzhistory[i][0]);
//    }
//    var weightmatrices = proto.init("weightmatrix", this._weightmatrix.count);
//    var i = 0;
//    foreach (var step in this.steps)
//    {
//        var stepweightmatrixproto = weightmatrices[i];
//        stepweightmatrixproto.steps = step;
//        stepweightmatrixproto.weight = this._weightmatrix[step].flatten().astype(type("float", valuetuple.create(float), new dictionary<object, object>
//        {
//        })).tolist();
//        i += 1;
//    }
//    proto.maxbucketidx = this._maxbucketidx;
//    proto.maxinputidx = this._maxinputidx;
//    var actualvaluesproto = proto.init("actualvalues", this._actualvalues.count);
//    foreach (var i in xrange(this._actualvalues.count))
//    {
//        if (this._actualvalues[i] != null)
//        {
//            actualvaluesproto[i] = this._actualvalues[i];
//        }
//        else
//        {
//            actualvaluesproto[i] = 0;
//        }
//    }
//    proto.version = this._version;
//    proto.verbosity = this.verbosity;
//}


//calculate error signal
             
//                 :param bucketidxlist: list of encoder buckets
             
//                 :return: dict containing error. the key is the number of steps
//                          the value is a numpy array of error at the output layer


//            public virtual object _calculateerror(object recordnum, object bucketidxlist)
//{
//    var error = new dictionary<object, object>();
//    var targetdist = numpy.zeros(this._maxbucketidx + 1);
//    var numcategories = bucketidxlist.count;
//    foreach (var bucketidx in bucketidxlist)
//    {
//        targetdist[bucketidx] = 1.0 / numcategories;
//    }
//    foreach (var _tup_1 in this._patternnzhistory)
//    {
//        var learnrecordnum = _tup_1.item1;
//        var learnpatternnz = _tup_1.item2;
//        var nsteps = recordnum - learnrecordnum;
//        if (this.steps.contains(nsteps))
//        {
//            var predictdist = this.infersinglestep(learnpatternnz, this._weightmatrix[nsteps]);
//            error[nsteps] = targetdist - predictdist;
//        }
//    }
//    return error;
//}
//        }

//        public static object _pformatarray(object array_, object fmt = "%.2f")
//{
//    return "[ " + " ".join(from x in array_
//                           select fmt % x) + " ]";
//}
//    }
//}