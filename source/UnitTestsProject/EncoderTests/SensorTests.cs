// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using NeoCortexApi.Sensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnitTestsProject.EncoderTests;

namespace UnitTestsProject
{
    /// <summary>
    /// NOT USED!
    /// </summary>
    [TestClass]
    public class SensorTests 
    {

        [TestMethod]
        public void TestSequence()
        {
            CortexNetworkContext context = new CortexNetworkContext();

            var descriptor = loadMetaDataWithCategoricLabel();

            var data = loadSampleDate();

            DataSequenceSensor sensor = new DataSequenceSensor(data, descriptor, context);

            while (sensor.MoveNext())
            {
                Debug.WriteLine(Helpers.StringifyVector(sensor.Current));
            }
        }


        private static Dictionary<string, object> getBinaryEncoderSettings()
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("N", 64);
            encoderSettings.Add("MinVal", (double)int.MinValue);
            encoderSettings.Add("MaxVal", (double)int.MaxValue);
            encoderSettings.Add(EncoderProperties.EncoderQualifiedName, typeof(BinaryEncoder).FullName);

            return encoderSettings;
        }

      

        private NeoCortexApi.DataDescriptor loadMetaDataWithCategoricLabel()
        {
            var des = new NeoCortexApi.DataDescriptor();

            var encoder1Sett1 = getBinaryEncoderSettings();
            var encoder2Sett2 = getBinaryEncoderSettings();

            des.Features = new NeoCortexApi.DataMappers.Column[2];
            des.Features[0] = new NeoCortexApi.DataMappers.Column { Id = 1, Name = "col1", Index = 0, Values = null, EncoderSettings = encoder1Sett1 };
            des.Features[1] = new NeoCortexApi.DataMappers.Column { Id = 2, Name = "col2", Index = 0, Values = null, EncoderSettings = encoder1Sett1 };
            //des.Features[1] = new Column { Id = 2, Name = "col2", Index = 1, Type = ColumnType.BOOLEAN, Values = new string[] { "0", "1" }, EncoderSettings = encoder2Sett2 };
            //des.Features[2] = new Column { Id = 3, Name = "col3", Index = 2, Type = ColumnType.STRING, Values = null, DefaultMissingValue = 1.4 };
            //des.Features[3] = new Column { Id = 4, Name = "col4", Index = 3, Type = ColumnType.CLASS, Values = new string[] { "red", "green", "blue" }, DefaultMissingValue = 1 };

            des.LabelIndex = 3;
            return des;
        }

        private object[][] loadSampleDate()
        {
            var data = new object[20][] {
                            new object[] {"+1.283", "yes", "This id description of the column and can be ignored.", "red" },
                            new object[] {"-0.843", "yes", "This id description of the column and can be ignored.", "green" },
                            new object[] {"+2.364", "no", "This id description of the column and can be ignored.", "red" },
                            new object[] {"+4.279", "yes", "This id description of the column and can be ignored.", "green" },
                            new object[] {"+3.383", "no", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"-1.624", "yes", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"-2.628", "yes", "This id description of the column and can be ignored.", "red" },
                            new object[] {"+2.847", "yes", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"+1.362", "no", "This id description of the column and can be ignored.", "green" },
                            new object[] {"+2.640", "no", "This id description of the column and can be ignored.", "green" },
                            new object[] {"-4.188", "yes", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"-1.161", "no", "This id description of the column and can be ignored.", "green" },
                            new object[] {"+0.825", "yes", "This id description of the column and can be ignored.", "red" },
                            new object[] {"-0.253", "no", "This id description of the column and can be ignored.", "green" },
                            new object[] {"-2.286", "no", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"-3.162", "yes", "This id description of the column and can be ignored.", "blue" },
                            new object[] {"-4.714", "yes", "This id description of the column and can be ignored.", "green" },
                            new object[] {"-0.242", "no", "This id description of the column and can be ignored.", "green" },
                            new object[] {"-0.400", "no", "This id description of the column and can be ignored.", "red" },
                            new object[] {"-3.315", "yes", "This id description of the column and can be ignored.", "red" }
                            };

           data = new object[20][] {
                            new object[] {"+1.283", "yes","22", "red" },
                            new object[] {"-0.843", "yes","21", "green" },
                            new object[] {"+2.364", "no", "11", "red" },
                            new object[] {"+4.279", "yes","15", "green" },
                            new object[] {"+3.383", "no", "67", "blue" },
                            new object[] {"-1.624", "yes","90", "blue" },
                            new object[] {"-2.628", "yes","21", "red" },
                            new object[] {"+2.847", "yes","56", "blue" },
                            new object[] {"+1.362", "no", "77", "green" },
                            new object[] {"+2.640", "no", "98", "green" },
                            new object[] {"-4.188", "yes","101", "blue" },
                            new object[] {"-1.161", "no", "12", "green" },
                            new object[] {"+0.825", "yes","1", "red" },
                            new object[] {"-0.253", "no", "0", "green" },
                            new object[] {"-2.286", "no", "2", "blue" },
                            new object[] {"-3.162", "yes","7", "blue" },
                            new object[] {"-4.714", "yes","6", "green" },
                            new object[] {"-0.242", "no", "5", "green" },
                            new object[] {"-0.400", "no", "8", "red" },
                            new object[] {"-3.315", "yes","1", "red" }
                            };

            //
            return data;
        }
    }
}
