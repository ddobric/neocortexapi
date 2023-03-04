// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NumSharp;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;
using System.Numerics;



namespace NeoCortexApi.Encoders
{


    /// <summary>
    /// Defines the <see cref="ScalarEncoderExperimental" />
    /// </summary>
    public class ScalarEncoder : EncoderBase
    {
        /// <summary>
        /// Gets a value indicating whether IsDelta
        /// </summary>
        public override bool IsDelta => throw new NotImplementedException();

        /// <summary>
        /// Gets the Width
        /// </summary>
        public override int Width => throw new NotImplementedException();

        //public int halfwidth { get; private set; }
        public static object DEFAULT_RADIUS { get; private set; }
        public static object DEFAULT_RESOLUTION { get; private set; }
        public static object ScalarEncoderProto { get; private set; }
        public int[] tmpOutput { get; private set; }

        public int GetWidth()
        {
            return this.N;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarEncoderExperimental"/> class.
        /// </summary>
        public ScalarEncoder()
        {

        }
        private const double SENTINEL_VALUE_FOR_MISSING_DATA = double.NaN;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarEncoderExperimental"/> class.
        /// </summary>
        /// <param name="encoderSettings">The encoderSettings<see cref="Dictionary{string, object}"/></param>
        public ScalarEncoder(Dictionary<string, object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }

        /// <summary>
        /// The AfterInitialize
        /// </summary>
        public override void AfterInitialize()
        {
            this.Encoders = null;
            this.verbosity = verbosity;
            this.W = W;
            if (W % 2 == 0)
            {
                throw new ArgumentException("W must be an odd number (to eliminate centering difficulty)");
            }

            this.MinVal = MinVal;
            this.MaxVal = MaxVal;

            this.Periodic = Periodic;
            this.ClipInput = ClipInput;


            this.HalfWidth = (W - 1) / 2;

            // For non-periodic inputs, padding is the number of bits "outside" the range,
            // on each side. I.e. the representation of minval is centered on some bit, and
            // there are "padding" bits to the left of that centered bit; similarly with
            // bits to the right of the center bit of maxval
            Padding = Periodic ? 0 : HalfWidth;

            if (double.NaN != MinVal && double.NaN != MaxVal)
            {
                if (MinVal >= MaxVal)
                {
                    throw new ArgumentException("maxVal must be > minVal");
                }

                this.RangeInternal = (float)(this.MaxVal - this.MinVal);
            }

            // There are three different ways of thinking about the representation. Handle
            // each case here.
            this.InitEncoder(W, MinVal, MaxVal, N, Radius, Resolution);

            //NInternal represents the output _area excluding the possible padding on each side
            this.NInternal = this.N - 2 * this.Padding;

            if (Name == null)
            {
                if ((MinVal % ((int)MinVal)) > 0 ||
                    (MaxVal % ((int)MaxVal)) > 0)
                {
                    Name = "[" + MinVal + ":" + MaxVal + "]";
                }
                else
                {
                    Name = "[" + (int)MinVal + ":" + (int)MaxVal + "]";
                }
            }

            //Checks for likely mistakes in encoder settings
            if (IsRealCortexModel)
            {
                if (W < 21 || W <= 2)
                {
                    throw new ArgumentException(
                        "Number of bits in the SDR (%d) must be greater than 2, and recommended >= 21 (use forced=True to override)");
                }
            }
            // Initialized bucketValues to null.
            this.bucketValues = null;



        }
        
         

        public void InitEncoder(int w, double minVal, double maxVal, int n, double radius, double resolution)
        {
            if (n != 0)
            {
                if (double.NaN != minVal && double.NaN != maxVal)
                {
                    if (!Periodic)
                    {
                        Resolution = RangeInternal / (N - W);
                    }
                    else
                    {
                        Resolution = RangeInternal / N;
                    }

                    Radius = W * Resolution;

                    if (Periodic)
                    {
                        Range = RangeInternal;
                    }
                    else
                    {
                        Range = RangeInternal + Resolution;
                    }
                }
            }
            else
            {
                if (radius != 0)
                {
                    Resolution = (float)(this.Radius) / W;
                }
                else if (resolution != 0)
                {
                    Radius = Resolution * W;
                }
                else
                {
                    throw new ArgumentException(
                        "One of n, radius, resolution must be specified for a ScalarEncoder");
                }

                if (Periodic)
                {
                    Range = RangeInternal;
                }
                else
                {
                    Range = RangeInternal + Resolution;
                }

                double nFloat = W * (this.Range / this.Radius) + 2 * this.Padding;
                N = (int)(nFloat);
            }



        }

        public void RecalcParams()
        {
            RangeInternal = (float)(this.MaxVal - this.MinVal);

            if (!Periodic)
            {
               
                Resolution = RangeInternal / (N - W);
            }
            else
            {
                
                Resolution = (float)RangeInternal / this.N;
            }
            Radius = W * Resolution;

            if (Periodic)
            {
                Range = RangeInternal;
            }
            else
            {
                Radius = this.RangeInternal * this.Resolution;
            }
            string Name = "[" + MinVal + ":" + MaxVal + "]";
        }




        /// <summary>
        /// Gets the index of the first non-zero bit.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Null in a case of an error.</returns>
        /// <exception cref="ArgumentException"></exception>
        protected int? GetFirstOnBit(double input)
        {
            if (input == double.NaN)
            {
                return null;
            }
            else
            {
                if (input < MinVal)
                {
                    if (ClipInput && !Periodic)
                    {
                        if (this.verbosity > 0)
                        {
                            Debug.WriteLine("Clipped input " + Name + "=" + input + " to minval " + MinVal);
                            input = MinVal;
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Input ({input}) less than range ({MinVal} - {MaxVal}");
                    }
                }
            }

            if (this.Periodic)
            {
                if (input >= this.MaxVal)
                {
                    throw new ArgumentException($"Input ({input}) greater than periodic range ({MinVal} - {MaxVal}");
                }
            }
            else
            {
                if (input > this.MaxVal)
                {
                    if (ClipInput)
                    {
                        if (this.verbosity >= 0)
                        {
                            Debug.WriteLine($"Clipped input {Name} = {input} to maxval MaxVal");
                            input = MaxVal;
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Input ({input}) greater than periodic range ({MinVal} - {MaxVal}");
                    }
                }
            }

            int centerbin;
            if (Periodic)
            {
                centerbin = (int)((input - this.MinVal) * this.NInternal / this.Range + this.Padding);
            }
            else
            {
                centerbin = ((int)(((input - this.MinVal) + this.Resolution / 2) / this.Resolution)) + this.Padding;
            }
            // We use the first bit to be set in the encoded output as the bucket index

            int minbin = centerbin - this.HalfWidth;

            return minbin;
        }


        /// <summary>
        /// Gets the bucket index of the given value.
        /// </summary>
        /// <param name="inputData">The data to be encoded. Must be of type double.</param>
        /// <param name="bucketIndex">The bucket index.</param>
        /// <returns></returns>
       /* public int? GetBucketIndex(object inputData)
        {
            double input = Convert.ToDouble(inputData, CultureInfo.InvariantCulture);
            if (input == double.NaN)
            {
                return null;
            }

            int? bucketVal = GetFirstOnBit(input);

            return bucketVal; 
        }
       */

        /// <summary>
        /// Encodes the given scalar value as SDR as defined by HTM.
        /// </summary>
        /// <param name="inputData">The inputData<see cref="object"/></param>
        /// <returns>The <see cref="int[]"/></returns>
        public override int[] Encode(object inputData)
        {
            int[] output = null;

            double input = Convert.ToDouble(inputData, CultureInfo.InvariantCulture);
            if (input == double.NaN)
            {
                return output;
            }

            int? bucketVal = GetFirstOnBit(input);
            if (bucketVal != null)
            {
                output = new int[N];

                int bucketIdx = bucketVal.Value;
                //Arrays.fill(output, 0);
                var minbin = bucketIdx;
                var maxbin = minbin + 2 * HalfWidth;
                if (Periodic)
                {
                    if (maxbin >= N)
                    {
                        int bottombins = maxbin - N + 1;
                        int[] range = ArrayUtils.Range(0, bottombins);
                        ArrayUtils.SetIndexesTo(output, range, 1);
                        maxbin = N - 1;
                    }
                    if (minbin < 0)
                    {
                        var topbins = -minbin;
                        ArrayUtils.SetIndexesTo(output, ArrayUtils.Range(N - topbins, N), 1);
                        minbin = 0;
                    }
                }

                ArrayUtils.SetIndexesTo(output, ArrayUtils.Range(minbin, maxbin + 1), 1);
            }

            // Output 1-D array of same length resulted in parameter N    
            return output;
        }


        /// <summary>
        /// This method enables running in the network.
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="learn"></param>
        /// <returns></returns>
        public int Compute(object inputData, bool learn, int v)
        {
            ScalarEncoder encoder = new ScalarEncoder();
            return v;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="List{T}"/></returns>
        ///  Vinay
        int bucketIdx;

        public int? GetBucketIndex(double input)
        {
            if (input is double && double.IsNaN((double)input))
            {
                input = double.NaN;
            }

            if (input == double.NaN)
            {
                return null;
            }

          

            minbin = this.GetFirstOnBit(input) ?? 0;
            int? bucketVal = GetFirstOnBit(input);
            // For periodic encoders, the bucket index is the index of the center bit
            if (this.Periodic)
            {
                bucketVal = minbin + this.HalfWidth;
                if (bucketVal < 0)
                {
                    bucketVal += this.N;
                }
                else
                {
                    /// for non-periodic encoders, the bucket index is the index of the left bit
                    bucketVal = minbin;
                }
                return bucketVal;
            }
            return 0;
        }



        public int EncodeIntoArray(int input, double[] output, int n, bool learn = true)
        {

            if (input != 0 && !typeof(System.IConvertible).IsAssignableFrom(input.GetType()))
            {
                throw new ArgumentException("Expected a scalar input but got input of type " + input.GetType().Name);
            }

            if ((!Double.IsNaN(input)) && float.IsNaN((float)input))
            {
                input = 0;
            }
            bucketVal = (int)GetFirstOnBit(input);

            if (bucketVal == 0)
            {
                // None is returned for missing value
                for (int i = 0; i < this.N; i++)
                {
                    output[i] = 0;
                }
                // TODO: should all 1s, or random SDR be returned instead?
            }

            else
            {
                // The bucket index is the index of the first bit to set in the output
                for (int i = 0; i < this.N; i++)
                {
                    output[i] = 0;
                }

                int minbin = (int)bucketVal;
                int maxbin = minbin + 2 * HalfWidth;

                if (this.Periodic)
                {
                    // Handle the edges by computing wrap-around
                    if (maxbin >= this.N)
                    {
                        int bottombins = maxbin - this.N + 1;
                        for (int i = 0; i < bottombins; i++)
                        {
                            output[i] = 1;
                        }
                        maxbin = this.N - 1;
                    }

                    if (minbin < 0)
                    {
                        int topbins = -minbin;
                        for (int i = this.N - topbins; i < this.N; i++)
                        {
                            output[i] = 1;
                        }
                        minbin = 0;
                    }
                }

                Debug.Assert(minbin >= 0);
                Debug.Assert(maxbin < this.N);

                // Set the output (except for periodic wraparound)
                for (int i = minbin; i <= maxbin; i++)
                {
                    output[i] = 1;
                }
            }
            // Debug the decode() method
            if (verbosity >= 2)
            {
                Console.WriteLine();
                Console.WriteLine("input: " + input);
                Console.WriteLine("range: " + MinVal + " - " + MaxVal);
                Console.WriteLine("n: " + this.N + " w: " + this.W + " resolution: " + this.Resolution +
                                  " radius: " + this.Radius + " periodic: " + this.Periodic);
                Console.Write("output: ");
                PPrint(output);
                Console.WriteLine("input desc: " + DecodedToStr(Decode(output)));
            }
            return 0;

        }

        private Tuple<Dictionary<string, Tuple<List<int>, string>>, List<string>> Decode(double[] output)
        {
            throw new NotImplementedException();
        }

       
        public int Decode(object encoded, object start, int newStruct, string parentFieldName = "")
        {
            (int[], (Dictionary<string, object>, List<object>)) Encode(int input)
            {
                double[] encodedArray = EncodeIntoArray(input);
                /// For now, we simply assume any top-down output greater than 0
                /// is ON. Eventually, we will probably want to incorporate the strength
                /// of each top-down output.
                tmpOutput = encodedArray.Take(N).Select(x => Convert.ToInt32(x > 0)).ToArray();
                if (!tmpOutput.Any())
                {
                    return (new int[0], (new Dictionary<string, object>(), new List<object>()));
                }

                var result = (tmpOutput, (new Dictionary<string, object>(), new List<object>()));
                return result;
            }
            


            // First, assume the input pool is not sampled 100%, and fill in the
            // "holes" in the encoded representation (which are likely to be present
            // if this is a coincidence that was learned by the SP).

            // Search for portions of the output that have "holes"
            maxZerosInARow = HalfWidth;
            for (int i = 0; i < maxZerosInARow; i++)
            {
                List<int> searchStr = Enumerable.Repeat(1, i + 3).ToList();
                searchStr[0] = 0;
                searchStr[searchStr.Count - 1] = 0;
                subLen = searchStr.Count;

                /// Does this search string appear in the output?

                if (Periodic)
                {
                    for (int j = 0; j < N; j++)
                    {
                        outputIndices = Enumerable.Range(j, subLen).Select(x => x % N).ToArray();
                        if (searchStr.SequenceEqual(tmpOutput.Where((value, index) => outputIndices.Contains(index))))
                        {
                            for (int k = 0; k < subLen; k++)
                            {
                                tmpOutput[outputIndices[k]] = 1;
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < N - subLen + 1; j++)
                    {
                        if (searchStr.SequenceEqual(tmpOutput.Skip(j).Take(subLen)))
                        {
                            for (int k = 0; k < subLen; k++)
                            {
                                tmpOutput[j + k] = 1;
                            }
                        }
                    }
                }
            }

            if (this.verbosity >= 2)
            {
                Console.WriteLine("raw output: " + string.Join(",", ((double[])encoded).Take(N).ToArray()));
                Console.WriteLine("filtered output: " + string.Join(",", tmpOutput));
            }



            // Find each run of 1's.
            nz = Array.FindAll(tmpOutput, x => x != 0);
            List<(int, int)> runs = new List<(int, int)>(); // will be tuples of (startIdx, runLength)
            if (nz.Length > 0)
            {
                run = new int[] { nz[0], 1 };
                int i = 1;
                while (i < nz.Length)
                {
                    if (nz[i] == run[0] + run[1])
                    {
                        run[1] += 1;
                    }
                    else
                    {
                        runs.Add((run[0], run[1]));
                        run = new int[] { nz[i], 1 };
                    }
                    i += 1;
                }
                runs.Add((run[0], run[1]));
            }

            // If we have a periodic encoder, merge the first and last run if they
            // both go all the way to the edges
            if (this.Periodic && runs.Count > 1)
            {
                if (runs[0].Item1 == 0 && runs[^1].Item1 + runs[^1].Item2 == this.N)
                {
                    runs[^1] = (runs[^1].Item1, runs[^1].Item2 + runs[0].Item2);
                    runs.RemoveAt(0);
                }
            }


            // ------------------------------------------------------------------------
            // Now, for each group of 1's, determine the "left" and "right" edges, where
            //  the "left" edge is inset by halfwidth and the "right" edge is inset by
            //  halfwidth.
            // For a group of width w or less, the "left" and "right" edge are both at
            //   the center position of the group.
            List<int[]> ranges = new List<int[]>();
            foreach (var run in runs)
            {
                Start = run.Item1;
                runLen = run.Item2;
                if (runLen <= this.W)
                {
                    left = right = Start + runLen / 2;
                }
                else
                {
                    left = Start + this.HalfWidth;
                    right = Start + runLen - 1 - this.HalfWidth;
                }

                ranges.Add(new int[] { left, right });

                if (!this.Periodic)
                {
                    // Convert to input space.
                    inMin = (left - Padding) * this.Resolution + this.MinVal;
                    inMax = (right - Padding) * this.Resolution + this.MinVal;
                }
                else
                {
                    // Convert to input space.
                    inMin = (left - this.Padding) * this.Range / this.NInternal + this.MinVal;
                    inMax = (right - this.Padding) * this.Range / this.NInternal + this.MinVal;
                }

                // Handle Wape-around if periodic 
                if (this.Periodic)
                {
                    if (inMin >= this.MaxVal)
                    {
                        inMin -= this.Range;
                        inMax -= this.Range;
                    }
                }


                // Clip low end
                if (inMin < this.MinVal)
                {
                    inMin = this.MinVal;
                }
                if (inMax < this.MinVal)
                {
                    inMax = this.MinVal;
                }

                /// If we have a periodic encoder, and the max is past the edge, break into
                ///  2 separate ranges
                if (this.Periodic && inMax >= this.MaxVal)
                {
                    ranges.Add(new int[] { (int)inMin, (int)this.MaxVal });
                    ranges.Add(new int[] { (int)this.MinVal, (int)(inMax - Range) });
                }
                else
                {
                    if (inMax > this.MaxVal)
                    {
                        inMax = this.MaxVal;
                    }
                    if (inMin > this.MaxVal)
                    {
                        inMin = this.MaxVal;
                    }
                    ranges.Add(new int[] { (int)inMin, (int)inMax });
                }

            }

            var desc = this.GenerateRangeDescription(ranges);
            // Return result
            if (parentFieldName != "")
            {
                fieldName = $"{parentFieldName}.{Name}";
            }
            else
            {
                fieldName = Name;
            }

            var result = new Dictionary<string, Tuple<List<double[]>, List<string>>>();
            List<double[]> doubleRanges = ranges.Select(r => r.Select(x => (double)x).ToArray()).ToList();
            result.Add(fieldName, new Tuple<List<double[]>, List<string>>(doubleRanges, new List<string>() { fieldName }));
            return result;

        }

        private object GenerateRangeDescription(List<int[]> ranges)
        {
            throw new NotImplementedException();
        }

        private double[] EncodeIntoArray(int input)
        {
            throw new NotImplementedException();
        }

        

        private double[] EncodeIntoArray(double value, int input)
        {
            throw new NotImplementedException();
        }



        public string GenerateRangeDescription(List<double[]> ranges)
        {
            string desc = "";
            int numRanges = ranges.Count;

            for (int i = 0; i < numRanges; i++)
            {
                if (ranges[i][0] != ranges[i][1])
                {
                    desc += $"{ranges[i][0]:#.##}-{ranges[i][1]:#.##}";
                }
                else
                {
                    desc += $"{ranges[i][0]:#.##}";
                }

                if (i < numRanges - 1)
                {
                    desc += ", ";
                }
            }

            return desc;
        }






        public object Get_topDownValues()
        {
            return _topDownValues;
        }

        //By defining _topDownMappingM as a member variable of the ScalarEncoder class,
        //you will be able to access it within the getTopDownMapping()

        public int[][] GetTopDownMapping()
        {
            if (_topDownMappingM == null)
            {
                if (Periodic)
                {
                    _topDownValues = Enumerable.Range(MinVal + Resolution / 2, (int)((MaxVal - MinVal) / Resolution))
                                                .Select(x => (double)x).ToArray();
                }
                else
                {
                    int numValues = (int)((maxval - minval) / resolution) + 1;
                    _topDownValues = Enumerable.Range(minval, numValues)
                                                .Select(x => x + resolution / 2.0).ToArray();
                }
                  
                int numCategories = _topDownValues.Length;
                _topDownMappingM = new int[numCategories][];
                for (int i = 0; i < numCategories; i++)
                {
                    double value = _topDownValues[i];
                    value = Math.Max(value, minval);
                    value = Math.Min(value, maxval);
                    int[] outputSpace = new int[n];
                    encodeIntoArray(value, outputSpace, learn: false);
                    _topDownMappingM[i] = outputSpace;
                }
            }

            return _topDownMappingM;
        }



        public List<double> GetBucketValues()
        {
            // Need to re-create?
            if (this.bucketValues == null)
            {
                int[,] topDownMappingM = this.GetTopDownMapping();
                int numBuckets = topDownMappingM.GetLength(0);
                List<object> list = new List<object>();
                this.bucketValues = list;
                for (int bucketIdx = 0; bucketIdx < numBuckets; bucketIdx++)
                {
                    int[] buckets = new int[] { bucketIdx };
                    List<BucketInfo> bucketInfoList = this.getBucketInfo(buckets);
                    this.bucketValues.Add((double)bucketInfoList[0].Value);
                }
            }

            return this.bucketValues;
        }

        private int[,] GetTopDownMapping()
        {
            throw new NotImplementedException();
        }

        public int getBucketInfo(object buckets)
        {
            object inputVal;
            // Get/generate the topDown mapping table
            //NOTE: although variable topDownMappingM is unused, some (bad-style) actions
            //are executed during _getTopDownMapping() so this line must stay here
            var topDownMappingM = this.GetTopDownMapping(this.Get_topDownValues());
            // The "category" is simply the bucket index
            var category = buckets[0];
            var encoding = this._topDownMappingM.getRow(category);
            // Which input value does this correspond to?
            if (this.Periodic)
            {
                inputVal = this.MinVal + this.Resolution / 2.0 + category * this.Resolution;
            }
            else
            {
                inputVal = this.MinVal + category * this.Resolution;
            }
            return new List<object>
            {
                EncoderResult(value: inputVal, scalar: inputVal, encoding: encoding)
            };
        }

        public int topDownCompute(object encoded)
        {
            // Get/generate the topDown mapping table
            var topDownMappingM = this.GetTopDownMapping(this.Get_topDownValues());
            // See which "category" we match the closest.
            var category = Matrix.rightVecProd(encoded).argmax();
            // Return that bucket info
            return this.getBucketInfo(new List<object> {
                    category
            });
        }

        public virtual object closenessScores(object expValues, object actValues, object fractional = null)
        {
            object closeness;
            var expValue = expValues[0];
            var actValue = actValues[0];
            if (this.Periodic)
            {
                expValue = expValue % this.MaxVal;
                actValue = actValue % this.MaxVal;
            }
            double err = Math.Abs(expValue - actValue);
            if (this.Periodic)
            {
                err = Math.Min(err, this.MaxVal - err);
            }
            if ((bool)fractional)
            {
                double pctErr = err / (this.MaxVal - this.MinVal);
                pctErr = Math.Min(1.0, pctErr);
                closeness = 1.0 - pctErr;
            }
            else
            {
                closeness = err;
            }
            return numpy.array(new List<object> {
                    closeness
            });
        }

       
        //     
        public virtual object closenessScoresNew(object expValues, object actValues, object fractional = null)
        {
            object closeness;
            var expValue = expValues[0];
            var actValue = actValues[0];
            if (this.Periodic)
            {
                expValue = expValue % this.MaxVal;
                actValue = actValue % this.MaxVal;
            }
            var err = Math.Abs(expValue - actValue);
            if (this.Periodic)
            {
                err = Math.Min(err, this.MaxVal - err);
            }
            if ((bool)fractional)
            {
                var pctErr = (float)err / (this.MaxVal - this.MinVal);
                pctErr = Math.Min(1.0, pctErr);
                closeness = 1.0 - pctErr;
            }   
            else
            {
                closeness = err;
            }
            return numpy.array(new List<object> {
                    closeness
                });
        }




        public override string ToString()
        {
            var @string = "ScalarEncoder:";
            @string += $" min: {this.MinVal}";
            @string += $" min: {this.MaxVal}";
            @string += $" min: {this.W}";
            @string += $" min: {this.N}";
            @string += $" min: {this.Resolution}";
            @string += $" min: {this.Radius}";
            @string += $" min: {this.Periodic}";
            @string += $" min: {this.NInternal}";
            @string += $" min: {this.rangeInternal}";
            @string += $" min: {this.Padding}";
            return @string;
        }
        public static object GetSchema(Type cls, object scalarEncoderProto)
        {
            return scalarEncoderProto;
        }

        
        public static object read(object cls, object proto)
        {
            object resolution;
            object radius;
            if (proto.N != null)
            {
                radius = DEFAULT_RADIUS;
                resolution = DEFAULT_RESOLUTION;
            }
            else
            {
                radius = proto.Radius;
                resolution = proto.Resolution;
            }
            return new cls(W: proto.W, minval: proto.MinVal, maxval: proto.MaxVal, periodic: proto.periodic, n: proto.n, name: proto.name, verbosity: proto.verbosity, ClipInput: Encoder.ClipInput(proto.ClipInput), forced: true);
        }

        public virtual object Write(object proto)
        {
            proto.W = this.W;
            proto.MinVal = this.MinVal;
            proto.MaxVal = this.MaxVal;
            proto.Periodic = this.Periodic;
            // Radius and resolution can be recalculated based on n
            proto.N = this.N;
            proto.Name = this.Name;
            proto.verbosity = this.verbosity;
            proto.ClipInput = this.ClipInput;
            return proto;
        }

        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }

        public override void EncodeIntoArray(object inputData, double[] output)
        {
            throw new NotImplementedException();
        }





        //public static object Deserialize<T>(StreamReader sr, string name)
        //{
        //    var excludeMembers = new List<string> { nameof(ScalarEncoder.Properties) };
        //    return HtmSerializer2.DeserializeObject<T>(sr, name, excludeMembers);
        //}

    }

    internal class BucketInfo
    {
        public double Value { get; internal set; }
    }

    internal class SM32
    {
        private int numCategories;
        private int n;

        public SM32(int numCategories, int n)
        {
            this.numCategories = numCategories;
            this.n = n;
        }
    }

    internal struct NewStruct
    {
        public Dictionary<object, object> Item1;
        public List<object> Item2;

        public NewStruct(Dictionary<object, object> item1, List<object> item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is NewStruct other &&
                   EqualityComparer<Dictionary<object, object>>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<List<object>>.Default.Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }

        public void Deconstruct(out Dictionary<object, object> item1, out List<object> item2)
        {
            item1 = Item1;
            item2 = Item2;
        }

        public static implicit operator (Dictionary<object, object>, List<object>)(NewStruct value)
        {
            return (value.Item1, value.Item2);
        }

        public static implicit operator NewStruct((Dictionary<object, object>, List<object>) value)
        {
            return new NewStruct(value.Item1, value.Item2);
        }

        
    }

    internal struct NewStruct1
    {
        public Dictionary<string, object> Item1;
        public List<object> Item2;

        public NewStruct1(Dictionary<string, object> item1, List<object> item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is NewStruct1 other &&
                   EqualityComparer<Dictionary<string, object>>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<List<object>>.Default.Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }

        public void Deconstruct(out Dictionary<string, object> item1, out List<object> item2)
        {
            item1 = Item1;
            item2 = Item2;
        }

        public static implicit operator (Dictionary<string, object>, List<object>)(NewStruct1 value)
        {
            return (value.Item1, value.Item2);
        }

        public static implicit operator NewStruct1((Dictionary<string, object>, List<object>) value)
        {
            return new NewStruct1(value.Item1, value.Item2);
        }
    }
}
