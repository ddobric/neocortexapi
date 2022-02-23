using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence
{
    public static class HelperMethods
    {
        public static List<Slot> GetSlots()
        {
            List<Slot> timeSlots = new List<Slot>
            {
               new Slot { Segment="00", StartTime=new TimeSpan(0,0,0), EndTime= new TimeSpan(0,59,59) },
               new Slot { Segment="01", StartTime=new TimeSpan(1,0,0), EndTime= new TimeSpan(1,59,59) },
               new Slot { Segment="02", StartTime=new TimeSpan(2,0,0), EndTime= new TimeSpan(2,59,59) },
               new Slot { Segment="03", StartTime=new TimeSpan(3,0,0), EndTime= new TimeSpan(3,59,59) },
               new Slot { Segment="04", StartTime=new TimeSpan(4,0,0), EndTime= new TimeSpan(4,59,59) },
               new Slot { Segment="05", StartTime=new TimeSpan(5,0,0), EndTime= new TimeSpan(5,59,59) },
               new Slot { Segment="06", StartTime=new TimeSpan(6,0,0), EndTime= new TimeSpan(6,59,59) },
               new Slot { Segment="07", StartTime=new TimeSpan(7,0,0), EndTime= new TimeSpan(7,59,59) },
               new Slot { Segment="08", StartTime=new TimeSpan(8,0,0), EndTime= new TimeSpan(8,59,59) },
               new Slot { Segment="09", StartTime=new TimeSpan(9,0,0), EndTime= new TimeSpan(9,59,59) },
               new Slot { Segment="10", StartTime=new TimeSpan(10,0,0), EndTime= new TimeSpan(10,59,59) },
               new Slot { Segment="11", StartTime=new TimeSpan(11,0,0), EndTime= new TimeSpan(11,59,59) },
               new Slot { Segment="12", StartTime=new TimeSpan(12,0,0), EndTime= new TimeSpan(12,59,59) },
               new Slot { Segment="13", StartTime=new TimeSpan(13,0,0), EndTime= new TimeSpan(13,59,59) },
               new Slot { Segment="14", StartTime=new TimeSpan(14,0,0), EndTime= new TimeSpan(14,59,59) },
               new Slot { Segment="15", StartTime=new TimeSpan(15,0,0), EndTime= new TimeSpan(15,59,59) },
               new Slot { Segment="16", StartTime=new TimeSpan(16,0,0), EndTime= new TimeSpan(16,59,59) },
               new Slot { Segment="17", StartTime=new TimeSpan(17,0,0), EndTime= new TimeSpan(17,59,59) },
               new Slot { Segment="18", StartTime=new TimeSpan(18,0,0), EndTime= new TimeSpan(18,59,59) },
               new Slot { Segment="19", StartTime=new TimeSpan(19,0,0), EndTime= new TimeSpan(19,59,59) },
               new Slot { Segment="20", StartTime=new TimeSpan(20,0,0), EndTime= new TimeSpan(20,59,59) },
               new Slot { Segment="21", StartTime=new TimeSpan(21,0,0), EndTime= new TimeSpan(21,59,59) },
               new Slot { Segment="22", StartTime=new TimeSpan(22,0,0), EndTime= new TimeSpan(22,59,59) },
               new Slot { Segment="23", StartTime=new TimeSpan(23,0,0), EndTime= new TimeSpan(23,59,59) },
            };

            return timeSlots;
        }

        internal static object EncodePassengerData(List<object> taxiData)
        {
            List<Dictionary<string, int[]>> ListOfEncodedTrainingSDR = new List<Dictionary<string, int[]>>();

            ScalarEncoder dayEncoder = FetchDayEncoder();
            ScalarEncoder monthEncoder = FetchMonthEncoder();
            ScalarEncoder segmentEncoder = FetchSegmentEncoder();
            ScalarEncoder dayOfWeekEncoder = FetchWeekDayEncoder();

            foreach (ProcessedData sequence in taxiData)
            {
                var tempDictionary = new Dictionary<string, int[]>();

                var observationLabel = sequence.Passanger_count;
                int day = sequence.Date.Day;
                int month = sequence.Date.Month;
                int segement = Convert.ToInt32(sequence.Segment);
                int dayOfWeek = (int)sequence.Date.DayOfWeek;

                //    int[] sdr = new int[0];

                //    sdr = sdr.Concat(DayEncoder.Encode(day)).ToArray();
                //    sdr = sdr.Concat(MonthEncoder.Encode(month)).ToArray();
                //    sdr = sdr.Concat(YearEncoder.Encode(year)).ToArray();
                //    sdr = sdr.Concat(DayOfWeekEncoder.Encode(dayOfWeek)).ToArray();

                //    //    UNCOMMENT THESE LINES TO DRAW SDR BITMAP
                //    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(sdr, 100, 100);
                //    var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                //    NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{observationDateTime.Day.ToString()}.png", null);
                //    tempDictionary.Add(observationLabel, sdr);
                //}

                ListOfEncodedTrainingSDR.Add(tempDictionary);
            }
            return null;
        }
        public static ScalarEncoder FetchWeekDayEncoder()
        {
            ScalarEncoder weekOfDayEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 11},
                { "MinVal", (double)0}, // Min value = (0).
                { "MaxVal", (double)7}, // Max value = (7).
                { "Periodic", true}, // Since Monday would repeat again.
                { "Name", "WeekDay"},
                { "ClipInput", true},
            });
            return weekOfDayEncoder;
        }
        public static ScalarEncoder FetchDayEncoder()
        {
            ScalarEncoder dayEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 35},
                { "MinVal", (double)1}, // Min value = (0).
                { "MaxVal", (double)32}, // Max value = (7).
                { "Periodic", true},
                { "Name", "Date"},
                { "ClipInput", true},
           });

            return dayEncoder;
        }
        public static ScalarEncoder FetchMonthEncoder()
        {
            ScalarEncoder monthEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 15},
                { "MinVal", (double)1}, // Min value = (0).
                { "MaxVal", (double)12}, // Max value = (7).
                { "Periodic", true}, // Since Monday would repeat again.
                { "Name", "Month"},
                { "ClipInput", true},
            });
            return monthEncoder;
        }
        public static ScalarEncoder FetchSegmentEncoder()
        {
            ScalarEncoder segmentEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 27},
                { "MinVal", (double)0}, // Min value = (2018).
                { "MaxVal", (double)23}, // Max value = (2021).
                { "Periodic", true}, // Since Monday would repeat again.
                { "Name", "Segment"},
                { "ClipInput", true},
            });
            return segmentEncoder;
        }
    }
}
