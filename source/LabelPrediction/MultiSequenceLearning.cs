using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;

namespace LabelPrediction
{
    public class MultiSequenceLearning
    {
        static readonly string PowerConsumptionCSV = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory + @"\Dataset\rec-center-hourly-og.csv");
        static readonly string PowerConsumptionCSV_Exp = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory + @"\Dataset\rec-center-hourly-exp.csv");


        public MultiSequenceLearning()
        {
            //needs no implementation
        }

        public void StartLearning()
        {
            Console.WriteLine("Reading CSV File..");
            var csvData = HelperMethods.ReadPowerConsumptionDataFromCSV(PowerConsumptionCSV_Exp);
            Console.WriteLine("Completed reading CSV File..");

            Console.WriteLine("Encoding data read from CSV...");
            var encodedData = HelperMethods.EncodePowerConsumptionData(csvData);


        }

    }
}
