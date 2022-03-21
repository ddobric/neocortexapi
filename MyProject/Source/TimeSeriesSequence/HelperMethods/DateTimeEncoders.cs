using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence.HelperMethods
{
    public static class DateTimeEncoders
    {
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
                { "MaxVal", (double)32}, // Max value = (32).
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
                { "MaxVal", (double)13}, // Max value = (12).
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
                { "MinVal", (double)0}, // Min value = (0).
                { "MaxVal", (double)24}, // Max value = (23).
                { "Periodic", true}, // Since Segment would repeat again.
                { "Name", "Segment"},
                { "ClipInput", true},
            });
            return segmentEncoder;
        }

        /// <summary>
        /// Multi encoder for day month and segment 
        /// </summary>
        /// <returns></returns>
        public static EncoderBase FetchDateTimeEncoder()
        {
            EncoderBase dayEncoder = FetchDayEncoder();
            EncoderBase monthEncoder = FetchMonthEncoder();
            EncoderBase segmentEncoder = FetchSegmentEncoder();
            EncoderBase dayOfWeek = FetchWeekDayEncoder();

            List<EncoderBase> encoder = new List<EncoderBase>();
            encoder.Add(dayEncoder);
            encoder.Add(monthEncoder);
            encoder.Add(segmentEncoder);
            encoder.Add(dayOfWeek);

            MultiEncoder encoderSetting = new MultiEncoder(encoder);

            return encoderSetting;
        }
        
        /// <summary>
        /// Encode the passenger data based on day month segment and day of week
        /// </summary>
        /// <param name="taxiData"></param>
        /// <returns></returns>
        public static List<Dictionary<string, int[]>> EncodePassengerData(List<ProcessedData> taxiData)
        {
            var unsortedTaxiData = taxiData.GroupBy(c => new
            {
                c.Date

            }).AsEnumerable().Cast<dynamic>();

            var multSequenceTaxiData = unsortedTaxiData.ToList();
            List<Dictionary<string, int[]>> ListOfEncodedTrainingSDR = new List<Dictionary<string, int[]>>();

            ScalarEncoder dayEncoder = FetchDayEncoder();
            ScalarEncoder monthEncoder = FetchMonthEncoder();
            ScalarEncoder segmentEncoder = FetchSegmentEncoder();
            ScalarEncoder dayOfWeekEncoder = FetchWeekDayEncoder();

            foreach (var sequenceData in multSequenceTaxiData)
            {
                var tempDictionary = new Dictionary<string, int[]>();

                foreach (var sequence in sequenceData)
                {
                    var observationLabel = sequence.Passanger_count;
                    int day = sequence.Date.Day;
                    int month = sequence.Date.Month;
                    int segement = Convert.ToInt32(sequence.Segment);
                    int dayOfWeek = (int)sequence.Date.DayOfWeek;

                    int[] sdr = new int[0];

                    sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
                    sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
                    sdr = sdr.Concat(segmentEncoder.Encode(segement)).ToArray();
                    sdr = sdr.Concat(dayOfWeekEncoder.Encode(dayOfWeek)).ToArray();

                    //    UNCOMMENT THESE LINES TO DRAW SDR BITMAP
                    //int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(sdr, 100, 100);
                    //var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                    //NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{sequence.Date.Day}.png", null);

                    if (tempDictionary.Count > 0 && tempDictionary.ContainsKey(observationLabel.ToString()))
                    {
                        var newKey = observationLabel + "," + segement;
                        tempDictionary.Add(newKey, sdr);
                    }
                    else
                        tempDictionary.Add(observationLabel.ToString(), sdr);
                }

                ListOfEncodedTrainingSDR.Add(tempDictionary);
            }

            return ListOfEncodedTrainingSDR;
        }
        
        /// <summary>
        /// Get SDR of a date time
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int[] GetSDRofDateTime(DateTime dateTime)
        {
            int[] sdr = new int[0];
            ScalarEncoder dayEncoder = FetchDayEncoder();
            ScalarEncoder monthEncoder = FetchMonthEncoder();
            ScalarEncoder segmentEncoder = FetchSegmentEncoder();
            ScalarEncoder dayOfWeekEncoder = FetchWeekDayEncoder();

            int day = dateTime.Day;
            int month = dateTime.Month;
            Slot result = CSVPRocessingMethods.GetSlot(dateTime.ToString("H:mm"));
            int segement = Convert.ToInt32(result.Segment);
            int dayOfWeek = (int)dateTime.DayOfWeek;

            sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
            sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
            sdr = sdr.Concat(segmentEncoder.Encode(segement)).ToArray();
            sdr = sdr.Concat(dayOfWeekEncoder.Encode(dayOfWeek)).ToArray();
            return sdr;
        }
    }
}
