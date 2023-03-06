// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Encoders
{
    public class CategoryEncoder : EncoderBase
    {
        /// <summary>
        /// Encodes a list of discrete categories (Strings), that are not related to each other
        /// so the value of the radius is set to 1
        /// </summary>
        /// <param name="category"></param>
        /// <param name="settings"></param>

        public CategoryEncoder(String[] category, Dictionary<string, object> settings) : base(settings)
        {
            this.Radius = 1;                                 // Radius is fixed in category encoder
            this.scalarNames = new List<string>(category);
            this.Range = category.Length;                   // this gives the length of category list    


            // Assig the value of width


            if (settings.TryGetValue("W", out object width) && (int)width > 0)
            {
                this.W = (int)width;
                this.N = this.W * category.Length;         // Number of bits in array
            }
            else if (settings.TryGetValue("N", out object numberofbits) && (int)numberofbits > 0)
            {
                this.N = (int)numberofbits;
                this.W = this.N / category.Length;
            }
            else
            {
                this.W = 1;
            }

        }

        public CategoryEncoder()
        {
        }

        public override int Width => throw new NotImplementedException();

        public override bool IsDelta => throw new NotImplementedException();
        /// <summary>
        /// This method will gives the result of output array according to the given input
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>

        public override int[] Encode(object inputData)
        {
            int index = scalarNames.IndexOf((String)inputData);
            int[] outArray = Encoding(index);
            return outArray;
        }

        
        public int[] Compute(string inputData, bool learn)
        {
            return Encode(inputData);
        }

        /// <summary>
        /// First for loop is used to assig array to zeros to avoid garbage value
        /// Second for loop is used for encode the array according to the Width, Radius and the elements 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private int[] Encoding(int element)
        {
            int[] encoderArray = new int[N];

            foreach (var item in encoderArray)
            {
                encoderArray[item] = 0;
            }

            for (int i = element * (W - (int)Radius + 1); i < element * (W - (int)Radius + 1) + W; i++)
            {
                int j = i % encoderArray.Length;
                encoderArray[j] = 1;
            }

            return encoderArray;
        }

        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
