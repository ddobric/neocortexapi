// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;

namespace UnitTestsProject.EncoderTests
{
    [TestClass]
    public class CategoryEncoderExperimentalTests
    {
        /// <summary>
        /// Tests if the object is being created by default constructor
        /// </summary>
        [TestMethod]
        public void TestCategoryTestEncoderDefaultConstructor()
        {
            CategoryEncoder categoryEncoder = new CategoryEncoder(); // creating object using default constructor
            Assert.IsNotNull(categoryEncoder); // assert if the object created is not null
        }


        /// <summary>
        /// Tests if the object is being created by parameterized contructor or not.
        /// The arguments to call the parameterized constructor are categoryArray and settings
        /// categoryEncoder here defines the array of strings
        /// settings here defines the dictionary in which key is string and values to be stored ae object
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderConstructor()
        {

            String[] categoryArray = new String[10]; // intialised array of size 10
            Dictionary<string, object> settings = new Dictionary<string, object>(); // intialising a dictionary with name as settings
            CategoryEncoder categoryEncoder = new CategoryEncoder(categoryArray, settings);
            Assert.IsNotNull(categoryEncoder); // assert if the object created is not null
        }


        /// <summary>
        /// Tests the category encoder if the defualt settings are intialized
        /// Over here encoderSettings value is set as <h1> hello </h1> with key as <h1> Name </h1>
        /// Assertion verifies the result for the same
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderWithSelfMadeEncoderSettings()
        {
            Dictionary<string, object> encoderSettings = getDefaultSettings();
            encoderSettings["Name"] = "hello"; // setting the key-value pair for the encoderSettings
            var arrayOfStrings = new string[] { "Milk", "Sugar", "Bread", "Egg" };
            var encoder = new CategoryEncoder(arrayOfStrings, encoderSettings);
            encoder.Initialize(encoderSettings);
            Assert.AreEqual("hello", encoder["Name"]); // assertion the value for the encoderSettings
        }

        /// <summary>
        /// Tests if the default settings are getting intialized during object creation of category encoder
        /// Over here we are passing the default settings as a parameter while creating an object for category encoder
        /// and validating the result if the encoder settings are having the same settings as default settings or not
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderIfDefaultSettingsAreInitialised()
        {
            var arrayOfStrings = new string[] { "Milk", "Sugar", "Bread", "Egg" };
            Dictionary<string, object> encoderSettings = getDefaultSettings(); // default setting
            var encoder = new CategoryEncoder(arrayOfStrings, encoderSettings); // passing default setting as an argument

            // validating the result
            Assert.AreEqual(encoderSettings["W"], encoder["W"]);
            Assert.AreEqual(encoderSettings["Radius"], encoder["Radius"]);
        }

        /// <summary>
        ///  Tests if the default settings are getting intialized by category encoder using method Initialize
        ///  Over here we are using the initialise method to set the settings for the encoder
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderIfDefaultSettingsAreInitialisedByEncoderInitializer()
        {
            Dictionary<string, object> encoderSettings = getDefaultSettings();
            encoderSettings["Name"] = "hello"; // adding key value parir for default setting
            var encoder = new CategoryEncoder();
            encoder.Initialize(encoderSettings); // intialising the encoder settings by using the method Initialize

            // validating the result
            Assert.AreEqual("hello", encoder["Name"]);
        }

        /// <summary>
        /// Tests if the encoding of input array(size 1) is getting encoded or not
        /// Over here we are using only one input that is Pakistan whose enocdedBits are 1,1,1
        /// when we pass this input to the category encoder we get the same result which validates that encoder is working fine
        /// Encoded bits are calculated using the formula :
        ///    placeForEncodeBit = element_Index * (W - (int)Radius + 1) % elementArray.length
        ///    where w  and r are the width and the radius passed in the default settings
        ///    elementArray is the input array(of string)
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderWithInputArrayOfSizeOneDefaultSettings()
        {
            // by default size of width will be taken
            int[] encodedBits = { 1, 1, 1 }; // encoded value for the string Pakistan
            Dictionary<string, object> encoderSettings = getDefaultSettings();
            var arrayOfStrings = new string[] { "Pakistan" };
            CategoryEncoder categoryEncoder = new CategoryEncoder(arrayOfStrings, encoderSettings);
            var result = categoryEncoder.Encode(arrayOfStrings[0]); // passing inout here

            // validating the result
            Assert.AreEqual(encodedBits.Length, result.Length);
            CollectionAssert.AreEqual(encodedBits, result);
        }

        /// <summary>
        /// Tests if the encoding of input array(size 4) is getting encoded or not
        /// Over here we are using only one input that is Milk whose enocdedBits are 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
        /// when we pass this input to the category encoder we get the same result which validates that encoder is working fine
        /// Encoded bits are calculated using the formula :
        ///    placeForEncodeBit = element_Index * (W - (int)Radius + 1) % elementArray.length
        ///    where w  and r are the width and the radius passed in the default settings
        ///    elementArray is the input array(of string) 
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderWithInputArrayOfSizeFourDefaultSettings()
        {
            // as the size of string array is 4 and width by default is 3 therefore the encoded bit array should be of
            // 12 bits in size
            int[] encodedBits = { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Dictionary<string, object> encoderSettings = getDefaultSettings(); // creaing default constructor
            var arrayOfStrings = new string[] { "Milk", "Sugar", "Bread", "Egg" }; // array of input strings
            CategoryEncoder categoryEncoder = new CategoryEncoder(arrayOfStrings, encoderSettings); // passing the input array here
            var result = categoryEncoder.Encode(arrayOfStrings[0]); // encoding string "Milk" 

            // validates the result
            Assert.AreEqual(encodedBits.Length, result.Length);
            CollectionAssert.AreEqual(encodedBits, result);
        }

        /// <summary>
        /// Tests if the encoding of input array(size 8) is getting encoded or not
        /// Over here we are using only one input that is Milk whose enocdedBits are  1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        /// when we pass this input to the category encoder we get the same result which validates that encoder is working fine
        /// Encoded bits are calculated using the formula :
        ///    placeForEncodeBit = element_Index * (W - (int)Radius + 1) % elementArray.length
        ///    where w  and r are the width and the radius passed in the default settings
        ///    elementArray is the input array(of string) 
        /// </summary>
        [TestMethod]
        public void TestCategoryEncoderWithArrayOfSizeEightDefaultSettings()
        {
            // As the size of string array is 8 and width by default is 3 therefore the encoded bit array should be of
            // 24 bits in size
            int[] encodedBitsForPorsche = { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // enocded bits for porsche
            int[] encodedBitsForMercedes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // encoded bits for mercedes

            Dictionary<string, object> encoderSettings = getDefaultSettings();
            var arrayOfStrings = new string[] { "Porsche", "Audi", "Opel", "BMW", "Mercedes", "Ford", "Jeep", "MINI" };
            CategoryEncoder categoryEncoder = new CategoryEncoder(arrayOfStrings, encoderSettings); // creating object for category encoder
            var resultForPorsche = categoryEncoder.Encode(arrayOfStrings[0]); // encoding porsche 
            var resultForMercedes = categoryEncoder.Encode(arrayOfStrings[4]); // encoding mercedes

            // validating result
            Assert.AreEqual(encodedBitsForPorsche.Length, resultForPorsche.Length);
            CollectionAssert.AreEqual(encodedBitsForPorsche, resultForPorsche);
            Assert.AreEqual(encodedBitsForMercedes.Length, resultForMercedes.Length);
            CollectionAssert.AreEqual(encodedBitsForMercedes, resultForMercedes);
        }

        /// <summary>
        /// This method is used to set the default settings for the encoder.
        /// by default we are keeping width as 2 and radius as 1  
        /// </summary>
        /// <returns>Dictionary<string, object> where key is string and value is object</returns>
        private static Dictionary<string, object> getDefaultSettings()
        {
            Dictionary<String, Object> encoderSettings = new Dictionary<string, object>();
            encoderSettings.Add("W", 3);
            encoderSettings.Add("Radius", (double)1);
            return encoderSettings;
        }

    }
}