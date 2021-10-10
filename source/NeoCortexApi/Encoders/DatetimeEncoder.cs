// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;


namespace NeoCortexApi.Encoders
{
    public class DateTimeEncoder : EncoderBase
    {
        /// <summary>
        /// Defines the precision of DateTime encoder.
        /// </summary>
        public enum Precision
        {
            Days,

            Hours,

            Minutes,

            Seconds,

            Miliseconds,

            Nanoseconds,

            Microseconds,

            Ticks
        }

        #region Private Fields


        public override int Width => throw new NotImplementedException();

        public override bool IsDelta => throw new NotImplementedException();

        private Dictionary<string, Dictionary<string, object>> m_Settings;

        /// <summary>
        /// Minimum time encoded by encoder.
        /// </summary>
        private DateTimeOffset m_MinDateTime;

        private Precision m_Precision;

        private ScalarEncoder m_DateTimeEncoder;

        private ScalarEncoder m_SeasonEncoder;

        private ScalarEncoder m_DayOfWeekEncoder;

        private ScalarEncoder m_WeekendEncoder;

#pragma warning disable CS0169 // The field 'DateTimeEncoder.m_CustomDaysEncoder' is never used
        private ScalarEncoder m_CustomDaysEncoder;
#pragma warning restore CS0169 // The field 'DateTimeEncoder.m_CustomDaysEncoder' is never used

#pragma warning disable CS0169 // The field 'DateTimeEncoder.m_HolidayEncoder' is never used
        private ScalarEncoder m_HolidayEncoder;
#pragma warning restore CS0169 // The field 'DateTimeEncoder.m_HolidayEncoder' is never used

        #endregion

        #region Init

        public DateTimeEncoder(Dictionary<string, Dictionary<string, object>> settings, Precision precision)
        {
            this.m_Settings = settings;
            this.m_Precision = precision;

            Initialize();
        }


        public void Initialize()
        {
            if (this.m_Settings.ContainsKey("SeasonEncoder"))
            {
                this.m_SeasonEncoder = new ScalarEncoder(this.m_Settings["SeasonEncoder"]);
            }

            if (this.m_Settings.ContainsKey("DayOfWeekEncoder"))
            {
                this.m_DayOfWeekEncoder = new ScalarEncoder(this.m_Settings["DayOfWeekEncoder"]);
            }

            if (this.m_Settings.ContainsKey("WeekendEncoder"))
            {
                this.m_WeekendEncoder = new ScalarEncoder(this.m_Settings["WeekendEncoder"]);
            }

            if (this.m_Settings.ContainsKey("DateTimeEncoder"))
            {
                // We keep this value, because it is needued in encoding process.
                this.m_MinDateTime = (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MinVal"];

                this.m_Settings["DateTimeEncoder"]["MaxVal"] = GetValue(this.m_Precision, (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MaxVal"] - (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MinVal"]);
                this.m_Settings["DateTimeEncoder"]["MinVal"] = 0.0;

                this.m_DateTimeEncoder = new ScalarEncoder(this.m_Settings["DateTimeEncoder"]);
                //this.m_DateTimeEncoder.MinVal = 0.0;
                //this.m_DateTimeEncoder.MaxVal = GetValue(this.m_Precision,
                //    (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MaxVal"] - (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MinVal"]);
            }
        }
        #endregion

        private static double GetValue(Precision precision, TimeSpan span)
        {
            double val = 0.0;

            switch (precision)
            {
                case Precision.Days:
                    val = span.TotalDays;
                    break;

                case Precision.Hours:
                    val = span.TotalHours;
                    break;

                case Precision.Minutes:
                    val = span.TotalMinutes;
                    break;

                case Precision.Seconds:
                    val = span.TotalSeconds;
                    break;

                case Precision.Miliseconds:
                    val = span.TotalMilliseconds;
                    break;

                case Precision.Microseconds:
                    val = span.Ticks / 100 / 1000;
                    break;

                case Precision.Nanoseconds:
                    val = span.Ticks / 100;
                    break;

                case Precision.Ticks:
                    val = span.Ticks;
                    break;
            }

            return val;
        }

        public override int[] Encode(object inputData)
        {
            DateTimeOffset input = (DateTimeOffset)inputData;

            List<int> result = new List<int>();

            if (this.m_DateTimeEncoder != null)
            {
                var val = GetValue(this.m_Precision, input - this.m_MinDateTime);

                var sdr = this.m_DateTimeEncoder.Encode(val);

                result.AddRange(sdr);

                if (this.m_Settings["DateTimeEncoder"].ContainsKey("Offset"))
                    result.AddRange(new int[(int)this.m_Settings["DateTimeEncoder"]["Offset"]]);
            }

            if (this.m_DayOfWeekEncoder != null)
            {
                var sdr = this.m_DayOfWeekEncoder.Encode(input.DayOfWeek);

                result.AddRange(sdr);

                if (this.m_Settings["DayOfWeekEncoder"].ContainsKey("Offset"))
                    result.AddRange(new int[(int)this.m_Settings["DayOfWeekEncoder"]["Offset"]]);
            }

            if (this.m_SeasonEncoder != null)
            {
                var sdr = this.m_SeasonEncoder.Encode(input.DayOfYear);

                result.AddRange(sdr);

                if (this.m_Settings["SeasonEncoder"].ContainsKey("Offset"))
                    result.AddRange(new int[(int)this.m_Settings["SeasonEncoder"]["Offset"]]);
            }

            if (this.m_WeekendEncoder != null)
            {
                var sdr = this.m_WeekendEncoder.Encode(input.DayOfWeek == DayOfWeek.Sunday || input.DayOfWeek == DayOfWeek.Saturday ? true : false);

                result.AddRange(sdr);

                if (this.m_Settings["WeekendEncoder"].ContainsKey("Offset"))
                    result.AddRange(new int[(int)this.m_Settings["WeekendEncoder"]["Offset"]]);
            }

            return result.ToArray();
        }

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
