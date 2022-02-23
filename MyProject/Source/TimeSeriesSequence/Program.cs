using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static TimeSeriesSequence.Entity.HelperClasses;
using static TimeSeriesSequence.MultiSequenceLearning;

namespace TimeSeriesSequence
{
    class program
    {
        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            RunPassangerTimeSeriesSequenceExperiment();
        } 

        /// <summary>
        /// Prediction of taxi passangers based on data set
        /// </summary>
        private static void RunPassangerTimeSeriesSequenceExperiment()
        {
            //Read the taxi data set and write into new processed csv with reuired column
            var taxiData =  ProcessExistingDatafromCSVfile();

            var trainTaxiData = HelperMethods.EncodePassengerData(taxiData);

            //Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();
            //MultiSequenceLearning experiment = new MultiSequenceLearning();
            //var predictor = experiment.Run(sequences);

        }
        /// <summary>
        /// Read the datas from taxi data set and process it
        /// </summary>
        private static List<object> ProcessExistingDatafromCSVfile()
        {
            List<TaxiData> taxiDatas = new List<TaxiData>();
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"DataSet\");

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
        private static List<object> CreateProcessedCSVFile(List<TaxiData> taxiDatas, string path)
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


            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("Pickup_Date,TimeSpan,Segment,Passenger_count");
            foreach (var taxiData in accumulatedPassangerData)
            {
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

            return accumulatedPassangerData.ToList();
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
        private static void PredictNextElement(HtmPredictionEngine predictor, double[] list)
        {
            Debug.WriteLine("------------------------------");

            foreach (var item in list)
            {
                var res = predictor.Predict(item);

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2[tokens.Length - 1]}");
                }
                else
                    Debug.WriteLine("Nothing predicted :(");
            }

            Debug.WriteLine("------------------------------");
        }

    }
}
