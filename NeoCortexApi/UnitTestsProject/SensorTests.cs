using LearningFoundation;
using LearningFoundation.DataMappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Network;
using NeoCortexApi.Sensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnitTestsProject
{
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

        private DataDescriptor loadMetaDataWithCategoricLabel()
        {
            var des = new DataDescriptor();

            des.Features = new Column[4];
            des.Features[0] = new Column { Id = 1, Name = "col1", Index = 0, Type = ColumnType.NUMERIC, Values = null,  };
            des.Features[1] = new Column { Id = 2, Name = "col2", Index = 1, Type = ColumnType.BOOLEAN, Values = new string[] { "0", "1" }, DefaultMissingValue = 0 };
            des.Features[2] = new Column { Id = 3, Name = "col3", Index = 2, Type = ColumnType.STRING, Values = null, DefaultMissingValue = 1.4 };
            des.Features[3] = new Column { Id = 4, Name = "col4", Index = 3, Type = ColumnType.CLASS, Values = new string[] { "red", "green", "blue" }, DefaultMissingValue = 1 };

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

            //
            return data;
        }
    }
}
