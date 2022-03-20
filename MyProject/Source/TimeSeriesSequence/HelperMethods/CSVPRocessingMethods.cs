using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence.HelperMethods
{
    public static class CSVPRocessingMethods
    {
        /// <summary>
        /// Slots of segment with 1 hours intervel
        /// </summary>
        /// <returns></returns>
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
                        var pickupDate = strRow[1].ToString();
                        string dateTime = GetPickUpDateTime(pickupDate);
                        taxiData.lpep_pickup_datetime = dateTime;
                        taxiData.passenger_count = Convert.ToInt32(strRow[7]);
                        taxiDatas.Add(taxiData);
                    }
                }
            }

            var processedTaxiData = CreateProcessedCSVFile(taxiDatas, path);

            return processedTaxiData;
        }
        /// <summary>
        /// Formated the date time
        /// </summary>
        /// <param name="pickupDate"></param>
        /// <returns></returns>
        private static string GetPickUpDateTime(string pickupDate)
        {
            string[] splitDateTime = pickupDate.Split(" ");

            string[] date = splitDateTime[0].Split("/");
            int dd = int.Parse(date[1]);
            int MM = int.Parse(date[0]);
            int yy = int.Parse(date[2]);
            string[] time = splitDateTime[1].Split(":");
            int hh = int.Parse(time[0]);
            int mm = int.Parse(time[1]);
            string dateTime = dd.ToString("00") + "/" + MM.ToString("00") + "/" + yy.ToString("00") + " " + hh.ToString("00") + ":" + mm.ToString("00");

            return dateTime;
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
                DateTime dateTime = DateTime.Parse(item.lpep_pickup_datetime);
                var pickupTime = dateTime.ToString("HH:mm");
                Slot result = GetSlot(pickupTime);

                ProcessedData processedData = new ProcessedData();
                processedData.Date = dateTime.Date;
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
        public static Slot GetSlot(string pickupTime)
        {
            var timeSlots = GetSlots();
            var time = TimeSpan.Parse(pickupTime);
            Slot slots = timeSlots.FirstOrDefault(x => x.EndTime >= time && x.StartTime <= time);

            return slots;
        }
    }
}
