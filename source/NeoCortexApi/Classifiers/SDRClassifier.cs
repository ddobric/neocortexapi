using NeoCortexApi.Entities;
using NeoCortexApi.Exception;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Classifiers
{
    public class SdrClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
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
            int bucketIdx = (int)classification[0]; // gives bucket index
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
