using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Encoders;

namespace LabelPrediction
{
    public class HelperMethods
    {
        public HelperMethods()
        { }

        /// <summary>
        /// Reads PowerConsumption CSV file and returns it into List of Dictionary
        /// </summary>
        /// <param name="csvFilePath">CSV file</param>
        /// <returns></returns>
        public static List<Dictionary<string,string>> ReadPowerConsumptionDataFromCSV(string csvFilePath)
        {
            List<Dictionary<string, string>> sequencesCollection = new List<Dictionary<string, string>>();

            int keyForUniqueIndexes = 0;
            
            if(File.Exists(csvFilePath))
            {
                using(StreamReader reader = new StreamReader(csvFilePath))
                {
                    Dictionary<string, string> sequence = new Dictionary<string, string>();
                    while(reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        string[] values = line.Split(",");

                        keyForUniqueIndexes++;

                        var columnDateTime = values[0];
                        var columnPower    = values[1];

                        if (sequence.ContainsKey(columnPower))
                        {
                            var newKey = columnPower + "," + keyForUniqueIndexes;
                            sequence.Add(newKey, columnDateTime);
                        }
                        else
                            sequence.Add(columnPower, columnDateTime);
                    }
                    sequencesCollection.Add(sequence);
                }

                return sequencesCollection; ;
            }

            return null;
        }

        /// <summary>
        /// Encodes the DateTime from the List of Dictionary (which was CSV File) using ScalerEncoder 
        /// and return SDR of DateTime in List of Dictionary
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Dictionary<string, int[]>> EncodePowerConsumptionData(List<Dictionary<string,string>> data)
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

                    DateTime dateTime = DateTime.Parse(value);
                    int day = dateTime.Day;
                    int month = dateTime.Month;
                    int year = dateTime.Year;
                    int hour = dateTime.Hour;

                    int[] sdr = new int[0];

                    sdr = sdr.Concat(dayEncoder.Encode(day)).ToArray();
                    sdr = sdr.Concat(monthEncoder.Encode(month)).ToArray();
                    sdr = sdr.Concat(yearEncoder.Encode(year)).ToArray();
                    sdr = sdr.Concat(hourEncoder.Encode(hour)).ToArray();

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
                { "W", 5},
                { "N", 36},
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
                { "W", 3},
                { "N", 15},
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
                { "W", 5},
                { "N", 30},
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
                { "W", 3},
                { "N", 7},
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
            ScalarEncoder hourEncoder = FetchHourEncoder();
            ScalarEncoder dayEncoder = FetchDayEncoder();
            ScalarEncoder monthEncoder = FetchMonthEncoder();
            ScalarEncoder yearEncoder = FetchYearEncoder();

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
