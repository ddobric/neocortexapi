using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
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

        internal static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
        {
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1,

                //NumInputs = 88
            };

            return cfg;
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

                int[] sdr = new int[0];

                sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
                sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
                sdr = sdr.Concat(segmentEncoder.Encode(segement)).ToArray();
                sdr = sdr.Concat(dayOfWeekEncoder.Encode(dayOfWeek)).ToArray();

                //    UNCOMMENT THESE LINES TO DRAW SDR BITMAP
                //int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(sdr, 100, 100);
                //var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
                //NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{sequence.Date.Day}.png", null);

                tempDictionary.Add(observationLabel.ToString(), sdr);
                ListOfEncodedTrainingSDR.Add(tempDictionary);
            }

            return ListOfEncodedTrainingSDR;
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
        /// Processed the raw passenger data set
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<ProcessedData> ProcessExistingDatafromCSVfile( string path)
        {
            List<TaxiData> taxiDatas = new List<TaxiData>();

            using (StreamReader sr = new StreamReader(path + "2021_Green.csv"))
            {
                string line = string.Empty;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    string[] strRow = line.Split(','); ;
                    TaxiData taxiData = new TaxiData();
                    if (strRow[7] != "")
                    {
                        taxiData.lpep_pickup_datetime = Convert.ToDateTime(strRow[1]);
                        taxiData.passenger_count = Convert.ToInt32(strRow[7]);
                        taxiDatas.Add(taxiData);
                    }
                }
            }

            var processedTaxiData = CreateProcessedCSVFile(taxiDatas, path);

            return processedTaxiData;
        }

        /// <summary>
        /// Create the processed CSV file with required column
        /// </summary>
        /// <param name="taxiDatas"></param>
        /// <param name="path"></param>
        private static List<ProcessedData> CreateProcessedCSVFile(List<TaxiData> taxiDatas, string path)
        {
            List<ProcessedData> processedTaxiDatas = new List<ProcessedData>();

            foreach (var item in taxiDatas)
            {
                var pickupTime = item.lpep_pickup_datetime.ToString("HH:mm");
                Slot result = GetSlot(pickupTime, HelperMethods.GetSlots());

                ProcessedData processedData = new ProcessedData();
                processedData.Date = item.lpep_pickup_datetime.Date;
                processedData.TimeSpan = result.StartTime.ToString() + " - " + result.EndTime.ToString();
                processedData.Segment = result.Segment;
                processedData.Passanger_count = item.passenger_count;
                processedTaxiDatas.Add(processedData);
            }

            var accumulatedPassangerData = processedTaxiDatas.GroupBy(c => new
            {
                c.Date,
                c.Segment
            }).Select(
                        g => new
                        {
                            Date = g.First().Date,
                            TimeSpan = g.First().TimeSpan,
                            Segment = g.First().Segment,
                            Passsanger_Count = g.Sum(s => s.Passanger_count),
                        }).AsEnumerable()
                          .Cast<dynamic>();

            var totalPassangerData = accumulatedPassangerData.ToList();
            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("Pickup_Date,TimeSpan,Segment,Passenger_count");
            processedTaxiDatas.Clear();
            foreach (var taxiData in totalPassangerData)
            {
                processedTaxiDatas.Add(new ProcessedData { Date = taxiData.Date, Passanger_count = taxiData.Passsanger_Count, Segment = taxiData.Segment, TimeSpan = taxiData.TimeSpan });
                var newLine = string.Format("{0},{1},{2},{3}", taxiData.Date, taxiData.TimeSpan, taxiData.Segment, taxiData.Passsanger_Count);
                csvcontent.AppendLine(newLine);
            }

            // Delete the existing file to avoid duplicate records.
            if (File.Exists(path + "2021_Green_Processed.csv"))
            {
                File.Delete(path + "2021_Green_Processed.csv");
            }

            // Save processed CSV data
            File.AppendAllText(path + "2021_Green_Processed.csv", csvcontent.ToString());

            return processedTaxiDatas;
        }

        /// <summary>
        /// Get the slot based on pick up time
        /// </summary>
        /// <param name="pickupTime"></param>
        /// <param name="timeSlots"></param>
        /// <returns></returns>
        private static Slot GetSlot(string pickupTime, List<Slot> timeSlots)
        {
            var time = TimeSpan.Parse(pickupTime);
            Slot slots = timeSlots.FirstOrDefault(x => x.EndTime >= time && x.StartTime <= time);

            return slots;
        }
    }
}
