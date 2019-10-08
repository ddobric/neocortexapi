using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace NeoCortexApi.Encoders
{
    public class DateTimeEncoder2 : EncoderBase
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

        private Precision m_Precision;

        private ScalarEncoderExperimental m_DateTimeEncoder;

        private ScalarEncoderExperimental m_SeasonEncoder;

        private ScalarEncoderExperimental m_DayOfWeekEncoder;

        private ScalarEncoderExperimental m_WeekendEncoder;

        private ScalarEncoderExperimental m_CustomDaysEncoder;

        private ScalarEncoderExperimental m_HolidayEncoder;

        #endregion

        #region Init

        public DateTimeEncoder2(Dictionary<string, Dictionary<string, object>> settings, Precision precision)
        {
            this.m_Settings = settings;
            this.m_Precision = precision;
            Initialize();
        }


        public void Initialize()
        {
            if (this.m_Settings.ContainsKey("SeasonEncoder"))
            {
                this.m_SeasonEncoder = new ScalarEncoderExperimental(this.m_Settings["SeasonEncoder"]);
            }

            if (this.m_Settings.ContainsKey("DayOfWeekEncoder"))
            {
                this.m_DayOfWeekEncoder = new ScalarEncoderExperimental(this.m_Settings["DayOfWeekEncoder"]);
            }

            if (this.m_Settings.ContainsKey("WeekendEncoder"))
            {
                this.m_WeekendEncoder = new ScalarEncoderExperimental(this.m_Settings["WeekendEncoder"]);
            }

            if (this.m_Settings.ContainsKey("DateTimeEncoder"))
            {
                this.m_DateTimeEncoder = new ScalarEncoderExperimental(this.m_Settings["DateTimeEncoder"]);
                this.m_DateTimeEncoder.MinVal = 0.0;
                this.m_DateTimeEncoder.MaxVal = GetValue(this.m_Precision,
                    (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MaxVal"] - (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MinVal"]);
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
                    val = span.Ticks * 100 * 1000;
                    break;

                case Precision.Nanoseconds:
                    val = span.Ticks * 100;
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
                var max = (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MaxVal"];
                var min = (DateTimeOffset)this.m_Settings["DateTimeEncoder"]["MinVal"];

                var v = GetValue(this.m_Precision, input - min);

                var sdr = this.m_DateTimeEncoder.Encode(v);

                result.AddRange(sdr);

                if (this.m_Settings["DateTimeEncoder"].ContainsKey("Padding"))
                    result.AddRange(new int[(int)this.m_Settings["DateTimeEncoder"]["Padding"]]);
            }

            if (this.m_DayOfWeekEncoder != null)
            {
                var sdr = this.m_DayOfWeekEncoder.Encode(input.Day);

                result.AddRange(sdr);

                if (this.m_Settings["DayOfWeekEncoder"].ContainsKey("Padding"))
                    result.AddRange(new int[(int)this.m_Settings["DayOfWeekEncoder"]["Padding"]]);
            }

            if (this.m_SeasonEncoder != null)
            {
                var sdr = this.m_SeasonEncoder.Encode(input.DayOfYear);

                result.AddRange(sdr);

                if (this.m_Settings["SeasonEncoder"].ContainsKey("Padding"))
                    result.AddRange(new int[(int)this.m_Settings["SeasonEncoder"]["Padding"]]);
            }

            if (this.m_WeekendEncoder != null)
            {
                var sdr = this.m_WeekendEncoder.Encode(input.DayOfWeek == DayOfWeek.Sunday || input.DayOfWeek == DayOfWeek.Saturday ? true : false);

                result.AddRange(sdr);

                if (this.m_Settings["WeekendEncoder"].ContainsKey("Padding"))
                    result.AddRange(new int[(int)this.m_Settings["WeekendEncoder"]["Padding"]]);
            }

            return result.ToArray();
        }

        public override List<T> getBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
