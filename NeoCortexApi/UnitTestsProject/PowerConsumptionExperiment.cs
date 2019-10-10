using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ImageBinarizer;
using System.Drawing;
using NeoCortex;
using NeoCortexApi.Network;
using System.Linq;
using NeoCortexApi.Encoders;
using System.Globalization;

namespace UnitTestsProject
{
    [TestClass]
    public class PowerConsumptionExperiment
    {
        private const int OutImgSize = 1024;

        [TestMethod]
        [TestCategory("LongRunning")]
        public void PowerPredictionExperiment()
        {
              string outFolder = nameof(PowerPredictionExperiment);

            Directory.CreateDirectory(outFolder);

            CortexNetworkContext ctx = new CortexNetworkContext();

            Dictionary<string, object> scalarEncoderSettings = getScalarEncoderDefaultSettings();
            var dateTimeEncoderSettings = getFullDateTimeEncoderSettings();

            ScalarEncoder scalarEncoder = new ScalarEncoder(scalarEncoderSettings);
            DateTimeEncoder dtEncoder = new DateTimeEncoder(dateTimeEncoderSettings, DateTimeEncoder.Precision.Hours);
                      
            using (StreamReader sr = new StreamReader("TestFiles\\rec-center-hourly.csv"))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    List<int> output = new List<int>();

                    string[] tokens = line.Split(",");

                    // Encode scalar value
                    var result = scalarEncoder.Encode(tokens[1]);

                    output.AddRange(result);

                    output.AddRange(new int[scalarEncoder.Offset]);

                    DateTime dt = DateTime.Parse(tokens[0], CultureInfo.InvariantCulture);

                    // Encode date/time/hour.
                    result = dtEncoder.Encode(new DateTimeOffset(dt, TimeSpan.FromMilliseconds(0)));

                    output.AddRange(result);

                    output.AddRange(new int[scalarEncoder.Offset]);

                    var outArr = output.ToArray();

                    int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(outArr, (int)Math.Sqrt(outArr.Length), (int)Math.Sqrt(output.Count));
                    var twoDimArray = ArrayUtils.Transpose(twoDimenArray);

                    //NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{outFolder}\\{tokens[0].Replace("/", "-").Replace(":", "-")}.png", Color.Yellow, Color.Black);
                }
            }
        }


        /// <summary>
        /// The getDefaultSettings
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, object}"/></returns>
        private static Dictionary<string, object> getScalarEncoderDefaultSettings()
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("W", 21);                       //the number of bits that are set to encode a single value -the "width" of the output signal 
                                                                //restriction: w must be odd to avoid centering problems.
            encoderSettings.Add("N", 1024);                     //The number of bits in the output. Must be greater than or equal to w
            encoderSettings.Add("MinVal", (double)0.0);         //The minimum value of the input signal.
            encoderSettings.Add("MaxVal", (double)150.0);       //The upper bound of the input signal
                                                                //encoderSettings.Add("Radius", (double)0);         //Two inputs separated by more than the radius have non-overlapping representations.
                                                                //Two inputs separated by less than the radius will in general overlap in at least some
                                                                //of their bits. You can think of this as the radius of the input.
                                                                //encoderSettings.Add("Resolution", (double)0.15);  // Two inputs separated by greater than, or equal to the resolution are guaranteed
                                                                //to have different representations.
            encoderSettings.Add("Periodic", (bool)true);        //If true, then the input value "wraps around" such that minval = maxval
                                                                //For a periodic value, the input must be strictly less than maxval,
                                                                //otherwise maxval is a true upper bound.
            encoderSettings.Add("ClipInput", (bool)false);       //if true, non-periodic inputs smaller than minval or greater than maxval 
                                                                 //will be clipped to minval/maxval

            encoderSettings.Add("Offset", 100);

            return encoderSettings;
        }



        /// <summary>
        /// Values for the radius and width will be passed here. For this unit test the width = 1 and radius = 1.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, object>> getFullDateTimeEncoderSettings()
        {
            Dictionary<string, Dictionary<string, object>> encoderSettings = new Dictionary<string, Dictionary<string, object>>();

            //encoderSettings.Add("SeasonEncoder",
            //  new Dictionary<string, object>()
            //  {
            //        { "W", 3},
            //        { "N", 12},
            //        //{ "Radius", 365/4},
            //        { "MinVal", 1.0},
            //        { "MaxVal", 367.0},
            //        { "Periodic", true},
            //        { "Name", "SeasonEncoder"},
            //        { "ClipInput", true},
            //        { "Offset", 100},
            //  }
            //  );

            encoderSettings.Add("DayOfWeekEncoder",
                new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 66},
                    { "MinVal", 1.0},
                    { "MaxVal", 8.0},
                    { "Periodic", false},
                    { "Name", "DayOfWeekEncoder"},
                    { "ClipInput", true},
                    { "Offset", 90},
                });

            encoderSettings.Add("WeekendEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 42},
                    { "MinVal", 0.0},
                    { "MaxVal", 1.0},
                    { "Periodic", false},
                    { "Name", "WeekendEncoder"},
                    { "ClipInput", true},
                    { "Offset", 100},
                });


            encoderSettings.Add("DateTimeEncoder", new Dictionary<string, object>()
                {
                    { "W", 21},
                    { "N", 1024},
                    { "MinVal", new DateTimeOffset(new DateTime(2010, 1, 1), TimeSpan.FromHours(0))},
                    { "MaxVal", new DateTimeOffset(new DateTime(2010, 12, 31), TimeSpan.FromHours(0))},
                    { "Periodic", false},
                    { "Name", "DateTimeEncoder"},
                    { "ClipInput", false},
                    { "Offset", 100},
                });

            return encoderSettings;
        }
    }

}
