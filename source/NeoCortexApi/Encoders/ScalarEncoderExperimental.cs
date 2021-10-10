// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;


namespace NeoCortexApi.Encoders
{


    /// <summary>
    /// Defines the <see cref="ScalarEncoderExperimental" />
    /// </summary>
    public class ScalarEncoderExperimental : EncoderBase
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
        /// Defines the NoOfBits. Works same as N. It is used to change Type of a variable
        /// </summary>
        private double NoOfBits;

        /// <summary>
        /// Defines the Starting point in an array to map active bits
        /// </summary>
        private double StartPoint;

        /// <summary>
        /// Defines the EndingPoint in an array where active bits ends
        /// </summary>
        private double EndingPoint;

        /// <summary>
        /// Defines the EndingPointForPeriodic
        /// </summary>
        private double EndingPointForPeriodic;// Ending point in an array where active bits ends only works for periodic data

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarEncoderExperimental"/> class.
        /// </summary>
        public ScalarEncoderExperimental()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarEncoderExperimental"/> class.
        /// </summary>
        /// <param name="encoderSettings">The encoderSettings<see cref="Dictionary{string, object}"/></param>
        public ScalarEncoderExperimental(Dictionary<string, object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }

        /// <summary>
        /// The AfterInitialize
        /// </summary>
        public override void AfterInitialize()
        {
            this.IsForced = false;  //if true, skip some safety checks (for compatibility reasons), default false
            this.RangeInternal = 0; //Difference of MaxVal and MinVal to set the Range of the Input Value.
        }

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="inputData">The inputData<see cref="object"/></param>
        /// <returns>The <see cref="int[]"/></returns>
        public override int[] Encode(object inputData)
        {
            //this.N = 0;
            double input = Convert.ToDouble(inputData);

            // An occurrence of the following data should throw an Exception;
            // *Value less than MinVal when clipInput == false || periodic == true
            // *Value greater than MaxVal when periodic == true
            // *Value greater than MaxVal when periodic == false && clipInput == true 
            // *Value greater than MaxVal when periodic == false && clipInput == false

            if (ClipInput && !Periodic)
            {
                if (input < MinVal)
                {
                    inputData = MinVal;
                }

                else if (input > MaxVal)
                {
                    inputData = MaxVal;
                }
            }
            else if (!ClipInput && Periodic)
            {
                if (input < MinVal)
                {
                    throw new ArgumentException("Input Value should be equal to MinVal or greater than MinVal");
                }
            }
            else if (Periodic)
            {
                if (input > MaxVal)
                {
                    throw new ArgumentException("Input Value should be equal to MaxVal or lesser than MaxVal");
                }
            }

            // Width(W) should be an odd number to avoid centering difficulty

            if (W % 2 == 0)
            {
                throw new ArgumentException("W must be an odd number (to eliminate centering difficulty)");
            }


            // Setting the Range in which the encoder works

            HalfWidth = (W - 1) / 2;

            if (MinVal >= MaxVal)
            {
                throw new ArgumentException("maxVal must be > minVal");
            }
            else
            {
                RangeInternal = (MaxVal - MinVal);

            }

            // Implementing the one of the following statement;
            // *Resolution and Radius in case *No. of Bits* are provided 
            // *Resolution and No. of Bits in case *Radius* is provided 
            // *Radius and No. of Bits in case *Resolution* is provided

            if (N != 0)
            {
                if (!double.IsNaN(MinVal) && !double.IsNaN(MaxVal))
                {
                    if (!Periodic)
                    {
                        Resolution = (RangeInternal / (N - W));
                    }
                    else
                    {
                        Resolution = (RangeInternal / N);
                    }

                    Radius = W * Resolution;
                }
            }
            else
            {
                if (Radius != 0)
                {
                    Resolution = Radius / W;
                }
                else if (Resolution != 0)
                {
                    Radius = Resolution * W;
                }
                else
                {
                    throw new ArgumentException("One of n, radius, resolution must be specified for a ScalarEncoder");
                }
                NoOfBits = W * (RangeInternal / Radius);
                N = (int)NoOfBits;
            }

            //Setting the output 1-D array starting and ending point of a loop, where the bits will become high i.e. 1.

            if (Periodic)
            {
                StartPoint = (input / Resolution) - (N / RangeInternal) + (N - HalfWidth);
            }
            else
            {
                StartPoint = (input / Resolution) - (N / RangeInternal);
            }

            EndingPoint = StartPoint + W;
            //if (EndingPoint > N)
            //    N = (int)EndingPoint;

            if (!Periodic /* && N == 0*/)
            {
                NoOfBits = (W) * (RangeInternal / Radius) + (2 * HalfWidth);
                N = (int)NoOfBits;
            }

            // Return the no of bits and the position of the first bit to be set in the encoder.
            // For periodic encoders, this can exceed the total no of bits, therefore, it is clipped accordingly,
            // wraps around in the encoded output 

            int[] EncodeIntoArray = new int[N];

            // Encodes inputData and puts the encoded value into the output array, which is a 1 - D array of length 
            // returned by EncodeIntoArray

            // This below Statement only works for periodic data as its starting and ending point on the array is different
            // from the non-periodic data in a way that the bits can be wrapped around according to input. 
            if (Periodic)
            {
                if (EndingPoint > N)
                {
                    EndingPointForPeriodic = (int)EndingPoint;
                    EndingPoint = N;

                    if (input > RangeInternal)
                    { StartPoint -= N; }
                }
            }

            for (int i = (int)StartPoint; i < (int)EndingPoint; i++)
            {
                EncodeIntoArray[i] = 1;
            }

            // This loop will only works when the data is periodic and depending upon the data, bits has to be wrapped around.
            if (Periodic)
            {
                if (EndingPointForPeriodic > N)
                {
                    double LeftOver = Math.Min(EndingPointForPeriodic - N, N);

                    if (input > RangeInternal)
                    {
                        LeftOver -= N;
                    }

                    StartPoint = LeftOver - W;

                    if (StartPoint < 0)
                    { StartPoint = 0; }

                    for (double j = StartPoint; j < LeftOver; j++)
                    {
                        EncodeIntoArray[(int)j] = 1;
                    }
                }
            }

            // Output 1-D array of same length resulted in parameter N    
            return EncodeIntoArray;
        }

        /// <summary>
        /// The getBucketValues
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="List{T}"/></returns>
        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}