using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class DateTimeEncoder2 : EncoderBase
    {
        public override int Width => throw new NotImplementedException();

        public override bool IsDelta => throw new NotImplementedException();

        private ScalarEncoder m_SeasonEncoder;

        private ScalarEncoder m_DayOfWeekEncoder;

        private ScalarEncoder m_WeekendEncoder;

        private ScalarEncoder m_CustomDaysEncoder;

        private ScalarEncoder m_HolidayEncoder;

        private ScalarEncoder m_TimeOfDayEncoder;

        public override void AfterInitialize()
        {
            this.m_SeasonEncoder = new ScalarEncoder(
                new Dictionary<string, object>()
                {
                    { "W", 3},
                    { "N", 12},
                    { "Radius", 365/4},
                    { "MinVal", 1.0},
                    { "MaxVal", 366.0},
                    { "Periodic", true},
                    { "Name", "season"},
                }
                );

            this.m_DayOfWeekEncoder = new ScalarEncoder(new Dictionary<string, object>()
                {
                    { "W", this.Properties["W"]},
                    { "Radius", 365/4},
                    { "MinVal", 1},
                    { "MaxVal", 366},
                    { "Periodic", true},
                    { "Name", "season"},
                });

        }

        public override int[] Encode(object inputData)
        {
            throw new NotImplementedException();
        }

        public override List<T> getBucketValues<T>()
        {
            throw new NotImplementedException();
        }
    }
}
