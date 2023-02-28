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
            if (W % 2 == 0)
            {
                throw new ArgumentException("W must be an odd number (to eliminate centering difficulty)");
            }

            HalfWidth = (W - 1) / 2;

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

                RangeInternal = MaxVal - MinVal;
            }

            // There are three different ways of thinking about the representation. Handle
            // each case here.
            InitEncoder(W, MinVal, MaxVal, N, Radius, Resolution);

            //nInternal represents the output _area excluding the possible padding on each side
            NInternal = N - 2 * Padding;

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
                    Resolution = Radius / w;
                }
                else if (resolution != 0)
                {
                    Radius = Resolution * w;
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

                double nFloat = w * (Range / Radius) + 2 * Padding;
                N = (int)(nFloat);
            }



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
                        Debug.WriteLine("Clipped input " + Name + "=" + input + " to minval " + MinVal);

                        input = MinVal;
                    }
                    else
                    {
                        throw new ArgumentException($"Input ({input}) less than range ({MinVal} - {MaxVal}");
                    }
                }
            }

            if (Periodic)
            {
                if (input >= MaxVal)
                {
                    throw new ArgumentException($"Input ({input}) greater than periodic range ({MinVal} - {MaxVal}");
                }
            }
            else
            {
                if (input > MaxVal)
                {
                    if (ClipInput)
                    {

                        Debug.WriteLine($"Clipped input {Name} = {input} to maxval MaxVal");
                        input = MaxVal;
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
                centerbin = (int)((input - MinVal) * NInternal / Range + Padding);
            }
            else
            {
                centerbin = ((int)(((input - MinVal) + Resolution / 2) / Resolution)) + Padding;
            }

            return centerbin - HalfWidth;
        }


        /// <summary>
        /// Gets the bucket index of the given value.
        /// </summary>
        /// <param name="inputData">The data to be encoded. Must be of type double.</param>
        /// <param name="bucketIndex">The bucket index.</param>
        /// <returns></returns>
        public int? GetBucketIndex(object inputData)
        {
            double input = Convert.ToDouble(inputData, CultureInfo.InvariantCulture);
            if (input == double.NaN)
            {
                return null;
            }

            int? bucketVal = GetFirstOnBit(input);

            return bucketVal; 
        }


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
                int minbin = bucketIdx;
                int maxbin = minbin + 2 * HalfWidth;
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
                        int topbins = -minbin;
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
        public int[] Compute(object inputData, bool learn)
        {
            return Encode(inputData);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="List{T}"/></returns>
        ///  Vinay
        int bucketIdx;

        public int GetBucketIndices(object inputData)
        {
            double input = 0.0; // assign a default value
            if (inputData is double && double.IsNaN((double)inputData))
            {
                input = SENTINEL_VALUE_FOR_MISSING_DATA;
            }
            else
            {
                // handle the case where inputData is not a double or not NaN
                throw new ArgumentException("Input data type not supported");
            }
            if (input == SENTINEL_VALUE_FOR_MISSING_DATA)
            {
                return 0;
            }
            // rest of the code

            var minbin = GetFirstOnBit(input).ToString()[0];
            // For periodic encoders, the bucket index is the index of the center bit
            if (Periodic)
            {
                bucketIdx = minbin + HalfWidth;
                if (bucketIdx < 0)
                {
                    bucketIdx += N;
                }
                else
                {
                    /// for non-periodic encoders, the bucket index is the index of the left bit
                    bucketIdx = minbin;
                }
                return bucketIdx;
            }
            return 0;
        }



        public int encodeIntoArray(int input, double output, int n)
        {

            if (input != 0)
            {
                throw new ArgumentException("Expected a scalar input but got input of type " + input.GetType().Name);
            }

            if (!Double.IsNaN(input))
            {
                input = 0;
                bucketIdx = (int)GetFirstOnBit(input);
            }

            if (bucketIdx == 0)
            {
                output = 0;

            }
            else
            {
                /// # The bucket index is the index of the first bit to set in the output             
                output[0, n] = 0;
                minbin = bucketIdx;
                maxbin = minbin + 2 * halfwidth;
            }

            if (Periodic)
            {
                if (axbin >= n)
                {
                    bottombins = maxbin - n + 1;
                    output[0, bottombins] = 1;
                    maxbin = this.n - 1;


                }




                if (minbin < 0)
                {
                    topbins = -minbin;
                    output[n - topbins, n] = 1;
                    minbin = 0;

                }
            }
            return 0;
        }

        public int decode(object encoded, string parentFieldName = "")
        {
            tmpoutput = NumSharp.array(encoded[0, this.n] > 0).astype(encoded.dtype);
            if (!tmpOutput.any())
            {
                return (new Dictionary<object, object>(), new List<object>());
            }
            maxzerosinrow = halfwidth;

            if (this.Periodic)
            {
                foreach (int j in xrange(this.n))
                {
                    var outputIndices = NumSharp.arange(j, j + subLen);
                    outputIndices %= this.n;
                    if (NumSharp.array_equal(searchStr, tmpOutput[outputIndices]))
                    {
                        tmpOutput[outputIndices] = 1;
                    }

                }
            }
            else
            {
                foreach (var j in xrange(this.n - subLen + 1))
                {
                    if (NumSharp.array_equal(searchStr, tmpOutput[j: (j + subLen)]))
                    {
                        tmpoutput[j: (j + subLen)] = 1;
                    }
                }
            }

            //if (this.verbosity >= 2)
            //{
            //    Console.WriteLine("raw output:", encoded[self.n]);
            //    Console.WriteLine("filtered output:", tmpOutput);
            //}

            var nz = tmpOutput.nonzero()[0];
            var runs = new List<object>();    /// will be tuples of (startIdx, runLength)
            var run = new List<object> { nz[0], 1 };

            var i = 1;

            while (i < nz.Count)
            {
                if (nz[i].Equals((int)run[0] + (int)run[1]))
                {
                    run[1] += 1;
                }
                else
                {
                    runs.append(run);
                    run = new List<object> {
                            nz[i],
                            1
                        };
                }
                i += 1;

            }
            runs.append(run);
            // If we have a periodic encoder, merge the first and last run if they
            //  both go all the way to the edges
            if (this.Periodic && runs.Count > 1)
            {
                if (runs[0][0] == 0 && runs[-1][0] + runs[-1][1] == this.n)
                {
                    runs[-1][1] += runs[0][1];
                    runs = runs[1];
                }
            }


            // ------------------------------------------------------------------------
            // Now, for each group of 1's, determine the "left" and "right" edges, where
            //  the "left" edge is inset by halfwidth and the "right" edge is inset by
            //  halfwidth.
            // For a group of width w or less, the "left" and "right" edge are both at
            //   the center position of the group.
            var ranges = new List<object>();
            foreach (var run in runs)
            {
                (start, runLen) = run;
                if (runLen <= this.w)
                {
                    left = start + runLen / 2;
                }
                else
                {
                    left = start + halfwidth;
                    var right = start + runLen - 1 - halfwidth;
                }

                if (!this.Periodic)
                {
                    // Convert to input space.
                    inMin = (left - this.padding) * this.Resolution + this.MinVal;
                    inMax = (right - this.padding) * this.Resolution + this.MinVal;
                }
                else
                {
                    // Convert to input space.
                    inMin = (left - this.padding) * this.range / this.nInternal + this.MinVal;
                    inMax = (right - this.padding) * this.range / this.nInternal + this.MinVal;
                }

                // Handle Wape-around if periodic 
                if (this.Periodic)
                {
                    if (inMin >= this.maxval)
                    {
                        inMin -= this.range;
                        inMax -= this.range;
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
                if (this.Periodic && inMax >= this.maxval)
                {
                    ranges.append(new List<object> {
                            inMin,
                            this.maxval
                        });
                    ranges.append(new List<object> {
                            this.MinVal,
                            inMax - this.range
                        });
                }
                else
                {
                    if (inMax > this.maxval)
                    {
                        inMax = this.maxval;
                    }
                    if (inMin > this.maxval)
                    {
                        inMin = this.maxval;
                    }
                    ranges.append(new List<object> {
                            inMin,
                            inMax
                        });
                }

            }

            var desc = this._generateRangeDescription(ranges);
            // Return result
            if (parentFieldName != "")
            {
                String fieldName = String.Format("%s.%s", parentFieldName, this.name);
            }
            else
            {
                String fieldName = this.name;
            }
            return (new Dictionary<object, object> {
                    {
                        fieldName,
                        (ranges, desc)}}, new List<object> {
                    fieldName
                });
        }



        //  Return the interal _topDownMappingM matrix used for handling the
        //     bucketInfo() and topDownCompute() methods. This is a matrix, one row per
        //     category (bucket) where each row contains the encoded output for that
        //     category.
        //     
        public int getTopDownMapping()
        {
            // Do we need to build up our reverse mapping table?
            if (this._topDownMappingM == null)
            {
                // The input scalar value corresponding to each possible output encoding
                if (this.Periodic)
                {
                    this._topDownValues = numpy.arange(this.MinVal + this.Resolution / 2.0, this.maxval, this.Resolution);
                }
                else
                {
                    //Number of values is (max-min)/resolutions
                    this._topDownValues = numpy.arange(this.minval, this.maxval + this.Resolution / 2.0, this.Resolution);
                }
                // Each row represents an encoded output pattern
                var numCategories = this._topDownValues.Count;
                this._topDownMappingM = SM32(numCategories, this.n);
                var outputSpace = numpy.zeros(this.n, dtype: GetNTAReal());
                foreach (var i in xrange(numCategories))
                {
                    var value = this._topDownValues[i];
                    value = max(value, this.minval);
                    value = min(value, this.maxval);
                    this.encodeIntoArray(value, outputSpace, learn: false);
                    this._topDownMappingM.setRowFromDense(i, outputSpace);
                }
            }
            return this._topDownMappingM;
        }




        public int getBucketInfo(object buckets)
        {
            object inputVal;
            // Get/generate the topDown mapping table
            //NOTE: although variable topDownMappingM is unused, some (bad-style) actions
            //are executed during _getTopDownMapping() so this line must stay here
            var topDownMappingM = this._getTopDownMapping();
            // The "category" is simply the bucket index
            var category = buckets[0];
            var encoding = this._topDownMappingM.getRow(category);
            // Which input value does this correspond to?
            if (this.Periodic)
            {
                inputVal = this.minval + this.Resolution / 2.0 + category * this.Resolution;
            }
            else
            {
                inputVal = this.minval + category * this.Resolution;
            }
            return new List<object>
            {
                EncoderResult(value: inputVal, scalar: inputVal, encoding: encoding)
            };
        }

        public int topDownCompute(object encoded)
        {
            // Get/generate the topDown mapping table
            var topDownMappingM = this._getTopDownMapping();
            // See which "category" we match the closest.
            var category = topDownMappingM.rightVecProd(encoded).argmax();
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
                expValue = expValue % this.maxval;
                actValue = actValue % this.maxval;
            }
            var err = abs(expValue - actValue);
            if (this.Periodic)
            {
                err = min(err, this.maxval - err);
            }
            if (fractional)
            {
                var pctErr = (float)err / (this.maxval - this.minval);
                pctErr = min(1.0, pctErr);
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

        //  See the function description in base.py
        //     
        public virtual object closenessScoresNew(object expValues, object actValues, object fractional = null)
        {
            object closeness;
            var expValue = expValues[0];
            var actValue = actValues[0];
            if (this.Periodic)
            {
                expValue = expValue % this.maxval;
                actValue = actValue % this.maxval;
            }
            var err = abs(expValue - actValue);
            if (this.Periodic)
            {
                err = min(err, this.maxval - err);
            }
            if (fractional)
            {
                var pctErr = (float)err / (this.maxval - this.minval);
                pctErr = min(1.0, pctErr);
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
            @string += $" min: {this.minval}";
            @string += $" min: {this.maxval}";
            @string += $" min: {this.w}";
            @string += $" min: {this.n}";
            @string += $" min: {this.Resolution}";
            @string += $" min: {this.radius}";
            @string += $" min: {this.Periodic}";
            @string += $" min: {this.nInternal}";
            @string += $" min: {this.rangeInternal}";
            @string += $" min: {this.padding}";
            return @string;
        }
        public static object GetSchema(Type cls)
        {
            return ScalarEncoderProto;
        }

        
        public static object read(object cls, object proto)
        {
            object resolution;
            object radius;
            if (proto.n != null)
            {
                radius = DEFAULT_RADIUS;
                resolution = DEFAULT_RESOLUTION;
            }
            else
            {
                radius = proto.radius;
                resolution = proto.resolution;
            }
            return cls(w: proto.w, minval: proto.minval, maxval: proto.maxval, periodic: proto.periodic, n: proto.n, name: proto.name, verbosity: proto.verbosity, clipInput: proto.clipInput, forced: true);
        }

        public virtual object write(object proto)
        {
            proto.w = this.w;
            proto.minval = this.minval;
            proto.maxval = this.maxval;
            proto.periodic = this.Periodic;
            // Radius and resolution can be recalculated based on n
            proto.n = this.n;
            proto.name = this.name;
            proto.verbosity = this.verbosity;
            proto.clipInput = this.clipInput;
        }





        //public static object Deserialize<T>(StreamReader sr, string name)
        //{
        //    var excludeMembers = new List<string> { nameof(ScalarEncoder.Properties) };
        //    return HtmSerializer2.DeserializeObject<T>(sr, name, excludeMembers);
        //}

    }

        
    
}
