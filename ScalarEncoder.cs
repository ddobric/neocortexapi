// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NeoCortexApi.Encoders;

namespace NeoCortexApi.Encoders
{


    /// <summary>
    /// Defines the <see cref="ScalarEncoder" />
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
        /// Initializes a new instance of the <see cref="ScalarEncoder"/> class.
        /// </summary>
        /// <param name="encoderSettings">The encoderSettings<see cref="Dictionary{string, object}"/></param>
        public ScalarEncoder(Dictionary<string, object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }

        /// <summary>
        /// Parses the arguments array into dictionary of appropriate properties.
        /// After parsing and creating a dictionary,
        /// it calls <see cref="NeoCortexApi.Encoders.EncoderBase.Initialize(Dictionary{string, object})"/>.
        /// This initializes to create an ScalarEncoder object
        /// </summary>
        /// <param name="args"></param>
        public ScalarEncoder(string[] args)
        {
            if (args.Length <= 1)
            {
                throw new ArgumentException("No or incomplete arguments provided for ScalarEncoder.");
            }
            else
            {
                Dictionary<string, object> encoderSettingsRaw = new Dictionary<string, object> { };

                for (int i = 0; i < args.Length; i += 2)
                {
                    encoderSettingsRaw.Add(args[i].Split("--")[1], args[i + 1]);

                }

                Dictionary<string, object> encoderSettings = new Dictionary<string, object> { };

                // Loops through each items of dictionary which was created using the command line args.
                // if match is found, converts the corresponding item and its value to appropriate form
                // so that Encode method can take this encoderSettings to encode the data.
                foreach (var item in encoderSettingsRaw)
                {
                    try
                    {
                        if (item.Key.ToLower() == "n")
                        {
                            encoderSettings.Add("N", Convert.ToInt32(item.Value));
                        }
                        if (item.Key.ToLower() == "w")
                        {
                            encoderSettings.Add("W", Convert.ToInt32(item.Value));
                        }
                        if (item.Key.ToLower() == "minval" || item.Key == "minvalue")
                        {
                            encoderSettings.Add("MinVal", Convert.ToDouble(item.Value));
                        }
                        if (item.Key.ToLower() == "maxval" || item.Key == "maxvalue")
                        {
                            encoderSettings.Add("MaxVal", Convert.ToDouble(item.Value));
                        }
                        if (item.Key.ToLower() == "radius")
                        {
                            encoderSettings.Add("Radius", Convert.ToDouble(item.Value));
                        }
                        if (item.Key.ToLower() == "resolution")
                        {
                            encoderSettings.Add("Resolution", Convert.ToDouble(item.Value));
                        }
                        if (item.Key.ToLower() == "periodic")
                        {
                            encoderSettings.Add("Periodic", Convert.ToBoolean(item.Value));
                        }
                        if (item.Key.ToLower() == "clipinput")
                        {
                            encoderSettings.Add("ClipInput", Convert.ToBoolean(item.Value));
                        }
                    }
                    catch (FormatException ex)
                    {
                        throw new ArgumentException($"Unable to convert the argument to proper type for ScalarEncoder settings. \n {ex}");

                    };
                }

                this.Initialize(encoderSettings);
            }
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

            //nInternal represents the output area excluding the possible padding on each side
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
        }


        protected void InitEncoder(int w, double minVal, double maxVal, int n, double radius, double resolution)
        {

            // N, radius and resolution are mutually exclusive parameters that determine
            // overall size of the ouptut. Only one of them should be set when setting encoderSettings.
            // Remaining two MUST be zero.
            if (n != 0)
            {


                if (n <= w)
                {
                    throw new ArgumentException(
                        "Total Number of output bits (N) must be greater than number of active bits (W) for a ScalarEncoder."
                    );
                }


                if (radius != 0 || resolution != 0)
                {
                    if (radius != 0)
                    {
                        if (resolution != 0)
                        {
                            throw new ArgumentException(
                                "Only one of the parameter: output bits(N), Radius or Resolution should be specified for a Scalar Encoder."
                            );
                        }
                        else
                        {
                            throw new ArgumentException(
                                "Only one of the parameter: output bits(N) or Radius should be specified for a Scalar Encoder."
                            );
                        }

                    }
                    else
                    {
                        throw new ArgumentException(
                            "Only one of the parameter: output bits(N) or Resolution should be specified for a Scalar Encoder."
                        );
                    }
                }

                else if (double.NaN != minVal && double.NaN != maxVal)
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

                // checking if value of N is minimum required
                int requiredN;
                if (!Periodic)
                {
                    requiredN = (int)Math.Ceiling(w + maxVal - minVal);
                }
                else
                {
                    requiredN = (int)Math.Ceiling(maxVal - minVal); ;
                }

                if (N < requiredN)
                {
                    throw new ArgumentException(
                        "The value of N is too low. This will result in overlapping of input bits. Two values separated by 1 might have similar encodings!");
                }

            }
            else
            {
                if (radius != 0)
                {
                    if (resolution != 0)
                    {
                        throw new ArgumentException(
                            "Only one of the Radius or Resolution should be specified for a ScalarEncoder!"
                            );
                    }

                    // Checking for value of Resolution from the given Radius that could result in similar encodings
                    Resolution = Radius / w;

                    if (Resolution > 1)
                    {
                        throw new ArgumentException(
                            $"The value of Radius is too high. Values that are less than {Resolution} apart will have similar encodings! Two values separated by 1 might have similar encodings!  "
                            );
                    }
                }
                else if (resolution != 0)
                {
                    // Checking for value of Resolution that could result in similar encodings
                    if (resolution > 1)
                    {
                        throw new ArgumentException(
                            $"The value of Resolution is too high. Values that are less than {resolution} apart will have similar encodings! Two values separated by 1 might have similar encodings!  "
                            );
                    }

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

                // Math.ceiling makes sure there are required number of Total bits
                N = (int)Math.Ceiling(nFloat);



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
                Debug.WriteLine($"minbin = {minbin} , maxbin = {maxbin}");
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
        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }

    }
}
