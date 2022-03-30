using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Encoders;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        #region Test methods for CSV processing
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
        [DataRow("10:58")]
        [DataRow("01:20")]
        [DataRow("05:18")]
        [DataRow("12:10")]
        [DataRow("11:48")]
        public void TimeSlotTest(string time)
        {
            //Arrange
            var solts = CSVPRocessingMethods.GetSlots();
            var testTime = TimeSpan.Parse(time);
            Slot slots = solts.FirstOrDefault(x => x.EndTime >= testTime && x.StartTime <= testTime);

            //Act
            var timeSolt = CSVPRocessingMethods.GetSlot(time);

            //Assert
            Assert.AreEqual(slots.Segment, timeSolt.Segment);
            Assert.AreEqual(slots.StartTime, timeSolt.StartTime);
            Assert.AreEqual(slots.EndTime, timeSolt.EndTime);
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

            var processedData = csvProcessedDatas.Find(x => x.GetSegment() == "00")
            Assert.AreEqual(23+15+6, processedData.GetPassanger_count())
        }

        [TestMethod]
        [DataRow("05/10/2022 10:58")]
        [DataRow("15/06/2022 01:20")]
        [DataRow("10/02/2022 05:18")]

        public void GetSDRofDateTimeTest(string dateTime)
        {
            DateTime userInput = DateTime.Parse(dateTime);

            // Encode the user input and return the SDR
            var sdr1 = DateTimeEncoders.GetSDRofDateTime(userInput);
            var sdr2 = DateTimeEncoders.GetSDRofDateTime(userInput);

            Assert.AreEqual(sdr1.Length, sdr2.Length);
        }

        #endregion

        #region Test Methods for Encoder Methods
        /// <summary>
        /// Generates encoded images for week days.
        /// </summary>
        /// <param name="input"></param>
        [TestMethod]
        [DataRow("05/10/2022 10:58:07")]
        [DataRow("05/01/2022 01:20:11")]
        [DataRow("05/02/2022 05:18:22")]

        public void DayOfWeekEncoderTest(string input)
        {
            ScalarEncoder dayEncoder = DateTimeEncoders.FetchDayEncoder();
            DateTime dateTime = DateTime.Parse(input);
            int dayOfWeek = (int)dateTime.DayOfWeek;

            var result = dayEncoder.Encode(dayOfWeek);
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 100, 100);
            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{dateTime.Day}.png", null);
        }

        /// <summary>
        /// Generates encoded images for day.
        /// </summary>
        /// <param name="input"></param>
        [TestMethod]
        [DataRow("05/10/2022 10:58:07")]
        [DataRow("05/01/2022 01:20:11")]
        [DataRow("05/02/2022 05:18:22")]

        /// <summary>
        /// Generates encoded images for month.
        /// </summary>
        /// <param name="input"></param>
        public void DayEncoderTest(string input)
        {
            ScalarEncoder dayEncoder = DateTimeEncoders.FetchDayEncoder();
            DateTime dateTime = DateTime.Parse(input);
            int day = dateTime.Day;

            var result = dayEncoder.Encode(day);
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 100, 100);
            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{dateTime.Day}.png", null);
        }

        [TestMethod]
        [DataRow("05/10/2022 10:58:07")]
        [DataRow("15/06/2022 01:20:11")]
        [DataRow("10/02/2022 05:18:22")]

        public void MonthEncoderTest(string input)
        {
            ScalarEncoder monthEncoder = DateTimeEncoders.FetchMonthEncoder();
            DateTime dateTime = DateTime.Parse(input);
            int month = dateTime.Month;

            var result = monthEncoder.Encode(month);
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 100, 100);
            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{dateTime.Day}.png", null);
        }

        [TestMethod]
        [DataRow("05/10/2022 10:58:07")]
        [DataRow("15/06/2022 01:20:11")]
        [DataRow("10/02/2022 05:18:22")]

        public void SegmentEncoderTest(string input)
        {
            ScalarEncoder segmentEncoder = DateTimeEncoders.FetchSegmentEncoder();
            DateTime dateTime = DateTime.Parse(input);
            var time = dateTime.ToString("HH:mm");
            Slot slot = CSVPRocessingMethods.GetSlot(time);

            var result = segmentEncoder.Encode(slot.Segment);
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(result, 100, 100);
            var twoDimArray = ArrayUtils.Transpose(twoDimenArray);
            NeoCortexUtils.DrawBitmap(twoDimArray, 1024, 1024, $"{dateTime.Day}.png", null);
        }

        #endregion
    }
}
