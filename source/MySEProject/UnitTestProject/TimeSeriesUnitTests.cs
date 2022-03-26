using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TimeSeriesSequence.HelperMethods;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace UnitTestProject
{
    [TestClass]
    public class TimeSeriesUnitTests
    {
        public CSVPRocessingMethods _csvPRocessingMethod;

        public TimeSeriesUnitTests()
        {
            _csvPRocessingMethod = new CSVPRocessingMethods();
        }

        [TestMethod]
        public void CheckSegmentsOfTime()
        {
            //Arrange
            string time = "01:18";

            //Act
            var timeSolt = CSVPRocessingMethods.GetSlot(time);

            //Assert
            Assert.IsNotNull(timeSolt.Segment);
            Assert.AreEqual("01", timeSolt.Segment);
            Assert.AreEqual(new TimeSpan(1, 0, 0), timeSolt.StartTime);
            Assert.AreEqual(new TimeSpan(1, 59, 59), timeSolt.EndTime);
        }

        [TestMethod]
        public void CheckTimeSlots()
        {
            //Act
            var timeSolt = CSVPRocessingMethods.GetSlots();

            //Assert
            Assert.IsNotNull(timeSolt);
            Assert.AreEqual(24, timeSolt.Count);
        }

        [TestMethod]
        public void CSVProcessingDataTest()
        {
            //Arrange
            List<TaxiData> taxiDataSet = new List<TaxiData>
            {
               new TaxiData { lpep_pickup_datetime="1/1/2021 0:15", passenger_count = 23 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 0:50", passenger_count = 15 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 5:20", passenger_count = 12 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 0:02", passenger_count = 6 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 5:45", passenger_count = 25 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 2:05", passenger_count = 9 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 3:45", passenger_count = 11 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 3:15", passenger_count = 16 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 4:35", passenger_count = 22 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 11:05", passenger_count = 10 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 11:55", passenger_count = 20 },
               new TaxiData { lpep_pickup_datetime="1/1/2021 2:34", passenger_count = 28 },
            };

            //Act
            var csvProcessedDatas = CSVPRocessingMethods.CreateProcessedCSVFile(taxiDataSet, null);

            //Assert
            Assert.IsNotNull(csvProcessedDatas);
            Assert.AreEqual(6, csvProcessedDatas.Count);
        }
    }
}