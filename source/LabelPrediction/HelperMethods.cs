using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;

namespace LabelPrediction
{
    public class HelperMethods
    {
        public HelperMethods()
        { }

        /// <summary>
        /// Reads PowerConsumption CSV file and pre-processes the data and returns it into List of Dictionary
        /// </summary>
        /// <param name="csvFilePath">CSV file</param>
        /// <returns></returns>
        public static List<Dictionary<string,string>> ReadPowerConsumptionDataFromCSV(string csvFilePath, string sequenceFormat)
        {
            List<Dictionary<string, string>> sequencesCollection = new List<Dictionary<string, string>>();

            int keyForUniqueIndexes = 0;
            string[] sequenceFormatType = { "byMonth", "byWeek", "byDay" };

            int[] daysOfMonth = { -1, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            int count = 0, maxCount = 0;

            bool firstTime = true;

            if (File.Exists(csvFilePath))
            {
                using(StreamReader reader = new StreamReader(csvFilePath))
                {
                    // [Power, "MM/10 SEG"]
                    Dictionary<string, string> sequence = new Dictionary<string, string>();
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        string[] values = line.Split(",");

                        keyForUniqueIndexes++;
                        count++;

                        var columnDateTime = values[0];
                        var columnPower    = values[1];

                        /* 
                         * This is a bit mess. The DateTime in date set is M/d/yy h:mm or MM/dd/yy hh:mm parsing with ParseExact seemed difficult, 
                         * reformatting to dd/MM/yy hh:mm 
                         */
                        string[] splitDateTime = columnDateTime.Split(" ");

                        string[] date = splitDateTime[0].Split("/");
                        int dd = int.Parse(date[1]);
                        int MM = int.Parse(date[0]);
                        int yy = int.Parse(date[2]);

                        /* 
                         * Parse only one month of data
                         */
                        if(firstTime)
                        {
                            if (sequenceFormatType[0].Equals(sequenceFormat))        /* byMonth */
                                maxCount = daysOfMonth[MM] * 24;
                            else if (sequenceFormatType[1].Equals(sequenceFormat))   /* byWeek  */
                                maxCount = 7 * 24;
                            else if (sequenceFormatType[2].Equals(sequenceFormat))   /* byDay   */
                                maxCount = 24;

                            firstTime = false;
                        }

                        string[] time = splitDateTime[1].Split(":");
                        int hh = int.Parse(time[0]);
                        int mm = int.Parse(time[1]);

                        string dateTime = dd.ToString("00") + "/" + MM.ToString("00") + "/" + yy.ToString("00") + " " + hh.ToString("00") + ":" + mm.ToString("00");

                        if (sequence.ContainsKey(columnPower))
                        {
                            var newKey = columnPower + "," + keyForUniqueIndexes;
                            sequence.Add(newKey, dateTime);
                        }
                        else
                            sequence.Add(columnPower, dateTime);

                        /*
                         * Creating multiple sequences for each month
                         */
                        if(count >= maxCount)
                        {
                            count = 0;
                            maxCount = 0;
                            firstTime = true;

                            sequencesCollection.Add(sequence);

                            sequence = new Dictionary<string, string>();
                        }
                    }
                }

                return sequencesCollection; 
            }

            return null;
        }

        public static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
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

        public static int[] EncodeSingleInput(string userInput)
        {
            DateTime date = DateTime.Parse(userInput);
            var day = date.Day;
            var month = date.Month;
            var year = date.Year;
            var hour = date.Hour;

            EncoderBase dayEncoder   = FetchDayEncoder();
            EncoderBase monthEncoder = FetchMonthEncoder();
            EncoderBase yearEncoder  = FetchYearEncoder();
            EncoderBase hourEncoder  = FetchHourEncoder();

            int[] sdr = new int[0];

            sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
            sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
            sdr = sdr.Concat(yearEncoder.Encode(year)).ToArray();
            sdr = sdr.Concat(hourEncoder.Encode(hour)).ToArray();

            return sdr;
        }

        /// <summary>
        /// Encodes the DateTime from the List of Dictionary (which was CSV File) using ScalerEncoder 
        /// and return SDR of DateTime in List of Dictionary
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Dictionary<string, int[]>> EncodePowerConsumptionData(List<Dictionary<string,string>> data, bool trace = false)
        {
            List<Dictionary<string,int[]>> listOfSDR = new List<Dictionary<string,int[]>>();

            ScalarEncoder hourEncoder = FetchHourEncoder();
            ScalarEncoder dayEncoder = FetchDayEncoder();
            ScalarEncoder monthEncoder = FetchMonthEncoder();
            ScalarEncoder yearEncoder = FetchYearEncoder();

            foreach (var sequence in data)
            {
                var tempDic = new Dictionary<string, int[]>();

                foreach (var keyValuePair in sequence)
                {
                    var label = keyValuePair.Key;
                    var value = keyValuePair.Value;

                    string[] formats = { "MM/dd/yy hh:mm" };
                    //DateTime dateTime = DateTime.ParseExact(value, formats, CultureInfo.InvariantCulture);
                    DateTime dateTime = DateTime.Parse(value);
                    int day = dateTime.Day;
                    int month = dateTime.Month;
                    int year = dateTime.Year;
                    int hour = dateTime.Hour;

                    int[] sdr = new int[0];

                    sdr = sdr.Concat(yearEncoder.Encode(year)).ToArray();
                    sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
                    sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
                    sdr = sdr.Concat(hourEncoder.Encode(hour)).ToArray();

                    //logger.WriteInformation(Helpers.StringifyVector(sdr));

                    tempDic.Add(label, sdr);
                }
                
                listOfSDR.Add(tempDic);
            }


            return listOfSDR;
        }

        public static ScalarEncoder FetchDayEncoder()
        {
            ScalarEncoder dayEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 9},
                { "N", 40},
                { "MinVal", (double)1}, // Min value = (1).
                { "MaxVal", (double)32}, // Max value = (31).
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
                { "W", 5},
                { "N", 17},
                { "MinVal", (double)1}, // Min value = (1).
                { "MaxVal", (double)13}, // Max value = (12).
                { "Periodic", true}, 
                { "Name", "Month"},
                { "ClipInput", true},
            });
            return monthEncoder;
        }

        public static ScalarEncoder FetchHourEncoder()
        {
            ScalarEncoder hourEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 9},
                { "N", 34},
                { "MinVal", (double)0},
                { "MaxVal", (double)23 + 1},
                { "Periodic", true},
                { "Name", "Hour of the day."},
                { "ClipInput", true},
            });
            return hourEncoder;
        }

        public static ScalarEncoder FetchYearEncoder()
        {
            ScalarEncoder yearEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 5},
                { "N", 9},
                { "MinVal", (double)2009}, // Min value = (2009).
                { "MaxVal", (double)2012}, // Max value = (2012).
                { "Periodic", false},
                { "Name", "Year"},
                { "ClipInput", true},
            });
            return yearEncoder;
        }

        public static MultiEncoder FetchDateTimeEncoder()
        {
            EncoderBase hourEncoder = FetchHourEncoder();
            EncoderBase dayEncoder = FetchDayEncoder();
            EncoderBase monthEncoder = FetchMonthEncoder();
            EncoderBase yearEncoder = FetchYearEncoder();

            List<EncoderBase> datetime = new List<EncoderBase>();
            datetime.Add(hourEncoder);
            datetime.Add(dayEncoder);
            datetime.Add(monthEncoder);
            datetime.Add(yearEncoder);

            MultiEncoder datetimeEncoder = new MultiEncoder(datetime);

            return datetimeEncoder;
        }
    }
}
