// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// For the implemention of encoder for the datetime encoder, date encoder only and time encoder only by using DateTimeEncoder class by using abstract class of EncoderBase.
    /// </summary>
    public class DateTimeEncoderExperimental : EncoderBase
    {
        /// <summary>
        /// For the setting of radius and the width.
        /// </summary>
        /// <param name="settings"></param>
        public DateTimeEncoderExperimental(Dictionary<string, object> settings)
        {
            if (settings.TryGetValue("Radius", out object radius) && (double)radius > 0)
            {
                this.Radius = (double)radius;
            }
            else
            {
                this.Radius = 1;
            }

            if (settings.TryGetValue("W", out object width) && (int)width > 0)
            {
                this.W = (int)width;
            }
            else
            {
                this.W = 1;
            }
        }

        /// <summary>
        /// Implementation of only Date encoder.  
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public int[] EncodeDateOnly(object inputData)
        {
            DateTime inputDate = DateTime.Parse((String)inputData, CultureInfo.InvariantCulture);
            int[] monthArray = CreateEncodingArray(inputDate.Month - 1, (int)(W * 12 / Radius));
            int[] dayArray = CreateEncodingArray(inputDate.Day - 1, (int)(W * 31 / Radius));
            int[] yearArray = CreateEncodingArray(inputDate.Year, (int)(W * 10 / Radius));

            int[] dateArray = new int[monthArray.Length + dayArray.Length + yearArray.Length];
            foreach (var item in dateArray)
            {
                dateArray[item] = 0;
            }
            monthArray.CopyTo(dateArray, 0);
            dayArray.CopyTo(dateArray, monthArray.Length);
            yearArray.CopyTo(dateArray, monthArray.Length + dayArray.Length);
            return dateArray;
        }

        /// <summary>
        ///  Implementation of only Time encoder.  
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public int[] EncodeTimeOnly(object inputData)
        {
            DateTime inputTime = DateTime.Parse((String)inputData);
            int[] hourArray = CreateEncodingArray(inputTime.Hour, (int)(W * 24 / Radius));
            int[] minuteArray = CreateEncodingArray(inputTime.Minute, (int)(W * 60 / Radius));
            int[] secondArray = CreateEncodingArray(inputTime.Second, (int)(W * 60 / Radius));

            int[] timeArray = new int[hourArray.Length + minuteArray.Length + secondArray.Length];
            foreach (var item in timeArray)
            {
                timeArray[item] = 0;
            }
            hourArray.CopyTo(timeArray, 0);
            minuteArray.CopyTo(timeArray, hourArray.Length);
            secondArray.CopyTo(timeArray, hourArray.Length + minuteArray.Length);
            return timeArray;
        }

        /// <summary>
        /// Implementation of n = width * (radius / range) forumla. 
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public override int[] Encode(object inputData)
        {
            DateTime inputDateTime = DateTime.Parse((String)inputData, CultureInfo.InvariantCulture);
            int[] monthArray = CreateEncodingArray(inputDateTime.Month - 1, (int)(W * 12 / Radius));
            int[] dayArray = CreateEncodingArray(inputDateTime.Day - 1, (int)(W * 31 / Radius));
            int[] yearArray = CreateEncodingArray(inputDateTime.Year, (int)(W * 10 / Radius));
            int[] hourArray = CreateEncodingArray(inputDateTime.Hour, (int)(W * 24 / Radius));
            int[] minuteArray = CreateEncodingArray(inputDateTime.Minute, (int)(W * 60 / Radius));
            int[] secondArray = CreateEncodingArray(inputDateTime.Second, (int)(W * 60 / Radius));

            int[] outArray = new int[monthArray.Length + dayArray.Length + yearArray.Length + hourArray.Length + minuteArray.Length + secondArray.Length];
            foreach (var item in outArray)
            {
                outArray[item] = 0;
            }
            monthArray.CopyTo(outArray, 0);
            dayArray.CopyTo(outArray, monthArray.Length);
            yearArray.CopyTo(outArray, monthArray.Length + dayArray.Length);
            hourArray.CopyTo(outArray, monthArray.Length + dayArray.Length + yearArray.Length);
            minuteArray.CopyTo(outArray, monthArray.Length + dayArray.Length + yearArray.Length + hourArray.Length);
            secondArray.CopyTo(outArray, monthArray.Length + dayArray.Length + yearArray.Length + hourArray.Length + minuteArray.Length);
            return outArray;
        }

        /// <summary>
        /// Implemented logical condition for encoding bits of date and time. 
        /// </summary>
        /// <param name="element">The scalar value to be encoded.</param>
        /// <param name="numberOfBits">Number of required bits.</param>
        /// <returns></returns>
        private int[] CreateEncodingArray(int element, int numberOfBits)
        {
            int[] encoderArray = new int[numberOfBits];

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

        /// <summary>
        /// From InitTest method for the setting of radius and width.
        /// </summary>
        public DateTimeEncoderExperimental()
        {

        }

        /// <summary>
        /// Overriding the width of encoder.
        /// </summary>
        public override int Width => W;

        /// <summary>
        /// Overriding the IsDelta for the encoder. 
        /// </summary>
        public override bool IsDelta => throw new NotImplementedException();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override List<T> GetBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
