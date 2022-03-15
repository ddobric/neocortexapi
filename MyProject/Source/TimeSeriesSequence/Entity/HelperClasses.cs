using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesSequence.Entity
{
    public class HelperClasses
    {
        public class Slot
        {
            public string Segment { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }

        public class Data
        {
            public DateTime PickupTime { get; set; }
            public int passanger { get; set; }
        }

        public class ProcessedData
        {
            public DateTime Date { get; set; }
            public string TimeSpan { get; set; }
            public string Segment { get; set; }
            public int Passanger_count { get; set; }
        }
        public class TaxiData
        {
            public string lpep_pickup_datetime { get; set; }
            public int passenger_count { get; set; }
        }
    }
}
