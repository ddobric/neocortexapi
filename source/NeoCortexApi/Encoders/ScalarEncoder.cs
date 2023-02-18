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
        /// The Encode
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

            // Added guard against immense string concatenation
            //if (LOGGER.isTraceEnabled())
            //{
            //    LOGGER.trace("");
            //    LOGGER.trace("input: " + input);
            //    LOGGER.trace("range: " + getMinVal() + " - " + getMaxVal());
            //    LOGGER.trace("n:" + getN() + "w:" + getW() + "resolution:" + getResolution() +
            //                    "radius:" + getRadius() + "periodic:" + isPeriodic());
            //    LOGGER.trace("output: " + Arrays.toString(output));
            //    LOGGER.trace("input desc: " + decode(output, ""));
            //}

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

        public  int GetBucketIndices(object inputData)
        {

            if ((typeof(input) == Double) && Double.IsNaN(input))
            {
                input = SENTINEL_VALUE_FOR_MISSING_DATA;
            }
            if (input == SENTINEL_VALUE_FOR_MISSING_DATA)
            {
                return 0;
            }


            var minbin = GetFirstOnBit(input)[0];
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
        }



        public  int encodeIntoArray(int input, double output,int n)
        {

            if (input != 0)
            {
                throw new ArgumentException("$Expected a scalar input but got input of type",  input );
            }

            if (input != Double.IsNaN(input))
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
                output[0,n] = 0;
                minbin = bucketIdx;
                maxbin = minbin + 2 * self.halfwidth;
            }

            if (periodic)
            {
                if (maxbin >= n)
                {
                    bottombins = maxbin - n + 1;
                    output[0,bottombins] = 1;
                    maxbin = this.n - 1;


                }




                    if (minbin < 0)
                    {
                        topbins = -minbin;
                        output[n - topbins,n] = 1;
                        minbin = 0;

                    }
            }
        }

        public int decode(object encoded, string parentFieldName = "")
        {
            tmpoutput = NumSharp.array(encoded[0, this.n] > 0).astype(encoded.dtype);
            if (!tmpOutput.any())
            {
                return (new Dictionary<object, object>(), new List<object>());
            }
            maxzerosinrow = this.halfwidth;

            if (this.periodic)
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
            var run = new List<object> {nz[0],1};

            var i = 1;

            while (i < nz.Count)
            {
                if (nz[i] == run[0] + run[1])
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
            if (this.periodic && runs.Count > 1)
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
                    left = start + this.halfwidth;
                    var right = start + runLen - 1 - this.halfwidth;
                }

                // Convert to input space.
                if (!this.periodic)
                {
                    inMin = (left - this.padding) * this.resolution + this.minval;
                    inMax = (right - this.padding) * this.resolution + this.minval;
                }
                else
                {
                    inMin = (left - this.padding) * this.range / this.nInternal + this.minval;
                    inMax = (right - this.padding) * this.range / this.nInternal + this.minval;
                }

                // Handle Wape-around if periodic 
                if (this.periodic)
                {
                    if (inMin >= this.maxval)
                    {
                        inMin -= this.range;
                        inMax -= this.range;
                    }
                }


                // Clip low end
                if (inMin < this.minval)
                {
                    inMin = this.minval;
                }
                if (inMax < this.minval)
                {
                    inMax = this.minval;
                }

                /// If we have a periodic encoder, and the max is past the edge, break into
                ///  2 separate ranges
                if (this.periodic and inMax >= this.maxval)
                    {
                    ranges.append([inMin, this.maxval])
                    ranges.append([this.minval, inMax - this.range])
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
                    ranges.append([inMin, inMax])

                }

                desc = this._generateRangeDescription(ranges)
                    //# Return result
                    if (parentFieldName != '')
                    {
                    fieldName = "%s.%s" % (parentFieldName, this.name);
                    }
                else:

                {
                    FieldName = this.name;

                    return ({ fieldName: (ranges, desc)}, [FieldName] })
                }        
            }

        public int generateRange Desccriptions(int Ranges)
        {
            """generate description from a text description of the ranges"""
            float desc = " ";
            int i = 0;
            int numRanges = Convert.ToString(Ranges);
            String NumR = numRanges.length();
            for i in range(){
                if ranges[i][0] != Ranges[i][1]{
                    desc = desc + double.(Ranges[i][0], Ranges[i][1])
                }
                else
                {
                    desc = desc + double.(Ranges[1][0])
                }
                
            }
            return desc;
        }









        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();

           
        }

        //public static object Deserialize<T>(StreamReader sr, string name)
        //{
        //    var excludeMembers = new List<string> { nameof(ScalarEncoder.Properties) };
        //    return HtmSerializer2.DeserializeObject<T>(sr, name, excludeMembers);
        //}


    }
}