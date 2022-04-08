// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Daenet.ImageBinarizerLib;
using Daenet.ImageBinarizerLib.Entities;
using IronXL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTestProject
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SchemaImageClassificationExperiment
    {
        /// <summary>
        /// Input: image resource folder path "..\\..\\..\\images"
        /// Output: grouped into different folders , destination root folder "output"
        /// hamming distance "distance.xlsx"
        /// </summary>
        [TestMethod]
        public void SchemaImageClassificationTest()
        {
            var imagesFolder = "..\\..\\..\\TestFiles\\SchemaImageClassification\\image";
            var csvPath = "..\\..\\..\\TestFiles\\SchemaImageClassification\\csv";
            string[] files = Directory.GetFiles(imagesFolder, "*", SearchOption.AllDirectories);

            string path, name;
            (path, name) = GetPathAndName(files[0]);

            int activeColumn = 60;
            int inputDimension = 32;

            string[] imageNames = new string[files.Length];

            SpatialPooler sp = new SpatialPooler();
            Connections mem = new Connections();
            Parameters config = GetParam(inputDimension, activeColumn);
            config.apply(mem);
            sp.Init(mem);

            List<int[]> activeArray = new List<int[]>();// to store n active sequences for n images

            // For each image in this directory
            for (int i = 0; i < files.Length; i++)
            {

                // TODO: activeArray: is a buffer to store active column sequence from spatial pooler
                activeArray.Add(new int[activeColumn * activeColumn]);
                (path, name) = GetPathAndName(files[i]);
                imageNames[i] = name;

                string binaryImagePath = BinarizeImage(path, inputDimension, inputDimension, csvPath, name);

                int[] inputVector = ReadCsvFileTest(binaryImagePath).ToArray(); // 1D binary of a image

                List<int[]> tempArr = new List<int[]>();
                tempArr.Add(new int[activeColumn * activeColumn]);
                tempArr.Add(new int[activeColumn * activeColumn]);
                int iter = -1;
                int id = 0;

                while (true) // Train spatial pooler with a single image until stable active column sequence is achieve
                {
                    id = (++iter & 1) == 0 ? 0 : 1;
                    for (int i_x = 0; i_x < tempArr[id].Length; i_x++) tempArr[id][i_x] = 0;


                    sp.compute(inputVector, tempArr[id], true);
                    if (iter == 1) continue;

                    var d = GetHammingDistance(tempArr[id], tempArr[id ^ 1], false);
                    if (d != double.NegativeInfinity) Console.WriteLine(d);
                    /*Note: GetHamming distance will return  a distance between 0 and 100.
                     if two sequences are the same the return value will be 100. Therefore, the similarity is (100==distance).
                     To give a four digit precision, < is used instead of ==.
                     */
                    if ((100.0 - d) < 0.00001)
                    {
                        for (int i_x = 0; i_x < tempArr[id].Length; i_x++)
                            activeArray[i][i_x] = tempArr[id][i_x];

                        break;
                    }
                }

                Console.WriteLine("finished image : " + i);
            }

            int[] parent = new int[files.Length];
            bool[] hasFolder = new bool[files.Length];
            List<double[]> distance = new List<double[]>();
            for (int k = 0; k < files.Length; k++)
            {
                distance.Add(new double[files.Length]);
                for (int kk = 0; kk < files.Length; kk++)
                {
                    distance[k].Append(0);
                }
            }

            for (int k = 0; k < files.Length; k++) { parent[k] = k; hasFolder[k] = false; }

            for (int i = 0; i < activeArray.Count; i++)
            {
                for (int j = i; j < activeArray.Count; j++)
                {
                    double d = Math.Min(GetHammingDistance(activeArray[i], activeArray[j], true), GetHammingDistance(activeArray[j], activeArray[i], true));
                    //double d = GetHammingDistance(activeArray[i], activeArray[j], false);

                    distance[i][j] = d;
                    distance[j][i] = d;
                }
            }
            SaveOutput(activeArray, path, imageNames);
            Groupping(distance, imageNames, path);
            Report(distance, imageNames);
        }


        /// <summary>
        /// Read csv file from the given file path.    
        /// </summary> 
        private List<int> ReadCsvFileTest(String path)
        {
            string fileContent = File.ReadAllText(path);
            string[] integerStrings = fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> intList = new List<int>();
            for (int n = 0; n < integerStrings.Length; n++)
            {
                String s = integerStrings[n];
                char[] sub = s.ToCharArray();
                for (int j = 0; j < sub.Length; j++)
                {
                    intList.Add(int.Parse(sub[j].ToString()));
                }
            }
            return intList;
        }

        private double GetHammingDistance(int[] originArray, int[] comparingArray, bool countNoneZerosOnly = false)
        {

            double[] arr1 = ArrayUtils.ToDoubleArray(originArray);
            double[] arr2 = ArrayUtils.ToDoubleArray(comparingArray);
            return GetHammingDistance(new double[][] { arr1 }, new double[][] { arr2 }, countNoneZerosOnly)[0];
        }

        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        private double[] GetHammingDistance(double[][] originArray, double[][] comparingArray, bool countNoneZerosOnly = false)
        {
            double[][] hDistance = new double[originArray.Length][];
            double[] h = new double[originArray.Length];
            double[] hammingDistance = new double[originArray.Length];

            for (int i = 0; i < originArray.Length; i++)
            {
                int len = Math.Max(originArray[i].Length, comparingArray[i].Length);
                int numOfDifferentBits = 0;
                for (int j = 0; j < len; j++)
                {
                    if (originArray[i].Length > j && comparingArray[i].Length > j)
                    {
                        if (originArray[i][j] == comparingArray[i][j])
                        {
                            numOfDifferentBits = numOfDifferentBits + 0;
                        }
                        else
                        {
                            if (countNoneZerosOnly == false)
                                numOfDifferentBits++;
                            else
                            {
                                if (originArray[i][j] == 1)
                                    numOfDifferentBits++;
                            }
                        }
                    }
                    else
                        numOfDifferentBits++;
                }

                h[i] = numOfDifferentBits;
                if (originArray[i].Length > 0 && originArray[i].Count(b => b == 1) > 0)
                {
                    //hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100 / originArray[i].Length);
                    if (countNoneZerosOnly == true)
                    {
                        hammingDistance[i] = ((originArray[i].Count(b => b == 1) - numOfDifferentBits) * 100.0 / originArray[i].Count(b => b == 1));
                    }
                    else
                    {
                        hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100.0 / originArray[i].Length);
                    }

                }
                else
                    hammingDistance[i] = double.NegativeInfinity;
            }

            return hammingDistance;
        }

        /// <summary>
        /// save active column sequence from spatial pooler to a given file path
        /// </summary>
        /// <param name="activeArray"></param>
        /// <param name="path"></param>
        /// <param name="imageNames"></param>
        private void SaveOutput(List<int[]> activeArray, String path, string[] imageNames)
        {
            var doc = "outputval.txt";
            var docpath = Path.Combine(path, "..", doc);
            StreamWriter ff;

            if (!File.Exists(docpath)) ff = File.CreateText(docpath);
            else ff = new StreamWriter(docpath);

            for (int jj = 0; jj < activeArray.Count; jj++)
            {
                String val = String.Format("{0} {1}", imageNames[jj], jj);
                ff.WriteLine(val);
                ff.Flush();
            }
            for (int jj = 0; jj < activeArray.Count; jj++)
            {
                for (int kk = 0; kk < activeArray[jj].Length; kk++) { ff.Write($"{activeArray[jj][kk]} "); ff.Flush(); }

                ff.WriteLine();
                ff.Flush();
            }
        }

        private string BinarizeImage(string sourcepath, int imageWidth, int imageHeight, string destinationPath, string name)
        {
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            string imgDestinationPath = Path.Combine(destinationPath, $"{name}.jpg");
            
            string _sourcePath = Path.Combine(sourcepath, $"{name}.jpg");
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            ImageBinarizer imageBinarizer = new ImageBinarizer(new BinarizerParams { RedThreshold = 200, GreenThreshold = 200, BlueThreshold = 200, ImageWidth = imageWidth, ImageHeight = imageHeight, InputImagePath = sourcepath, OutputImagePath = imgDestinationPath });

            imageBinarizer.Run();

            return destinationPath;
        }

        /* copy origial image to the grouped output folder*/
        private void WriteFile(string sourcePath, string destinationPath, string fileName, string foldername)
        {
            string outFilePath = Path.Combine(destinationPath, foldername);//, $"{fileName}.jpg");
            string inFilePath = Path.Combine(sourcePath, $"{fileName}.jpg");

            if (!Directory.Exists(outFilePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(outFilePath);

            }

            outFilePath = Path.Combine(outFilePath, $"{fileName}.jpg");

            // Try to create the directory.
            if (!File.Exists(outFilePath))
            {
                FileStream fs = File.Create(outFilePath);
                byte[] readBuffer = File.ReadAllBytes(inFilePath);
                fs.Write(readBuffer);
            }
        }

        private Parameters GetDefaultParams()
        {
            ThreadSafeRandom rnd = new ThreadSafeRandom(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 50.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            //parameters.Set(KEY.WRAP_AROUND, false);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 100000);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);
            return parameters;
        }

        /* Spatial pooler parameter config.*/
        private Parameters GetParam(int inputDimension, int activeColumn)
        {
            Parameters config = GetDefaultParams();
            config.setInputDimensions(new int[] { inputDimension, inputDimension });
            config.setColumnDimensions(new int[] { activeColumn, activeColumn });
            config.Set(KEY.GLOBAL_INHIBITION, false);
            config.setNumActiveColumnsPerInhArea(0.02 * activeColumn * activeColumn);

            return config;
        }

        /*Split path and file name*/
        private (string, string) GetPathAndName(string fullPath)
        {
            int index1 = fullPath.LastIndexOf("\\") + 1;
            int index2 = fullPath.LastIndexOf(".");
            string name = fullPath.Substring(index1, index2 - index1);
            string path = fullPath.Substring(0, index1);
            return (path, name);
        }

        // Delete unused image directory
        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        /*Grouping according to hamming distance*/
        private void Groupping(List<double[]> activeArray, string[] imageNames, string path)
        {
            const double thresold = 100.0;
            int[] groups = new int[activeArray.Count];
            for (int i = 0; i < activeArray.Count; i++) groups[i] = -1;
            int group = 0;
            for (int i = 0; i < activeArray.Count; i++)
            {
                for (int j = i + 1; j < activeArray[i].Length; j++)
                {
                    if (activeArray[i][j] >= thresold)
                    {
                        if (groups[i] == -1 && groups[j] == -1) groups[i] = groups[j] = group++;
                        else
                        {
                            int g = Math.Max(groups[i], groups[j]);
                            groups[i] = groups[j] = g;
                        }
                    }
                }
            }
            /*
            for (int i = 0; i < activeArray.Count; i++)
            {
                if (groups[i] == -1)
                {
                    double max = -1.0;
                    int index = -1;

                    for (int j = 0; j < activeArray[i].Length; j++)
                    {

                        if (i != j)
                        {
                            if (activeArray[i][ j]-max > 0.00000001)
                            {
                                max = activeArray[i][ j];
                                index = j;
                            }
                        }

                    }
                    if (max - 98.8 > 0.0000001 && index != -1)
                    {
                        if (groups[index] == -1)
                        {
                            groups[i] = groups[index] = group++;
                        }
                        else
                        {
                            groups[i] = groups[index];
                        }
                    }
                    else
                    {
                        groups[i] = group++;
                    }

                }
            }*/
            for (int i = 0; i < activeArray.Count; i++) groups[i] = groups[i] == -1 ? group++ : groups[i];
            String dPath = Path.Combine(path, "..", "output"); // output folder
            if (Directory.Exists(dPath)) DeleteDirectory(dPath);
            for (int i = 0; i < groups.Length; i++)
            {
                // send path + group[i] // create if does not exits
                WriteFile(path, dPath, imageNames[i], groups[i].ToString());
            }
        }
        private int Difference(int[] a, int[] b)
        {
            int count = 0;
            for (int i = 0; i < a.Length; i++)
            {
                count += a[i] != b[i] ? 1 : 0;
            }
            return count;
        }
        private void Report(List<double[]> distance, string[] names)
        {
            string filePath = "..\\..\\..\\distance.xlsx";
            if (File.Exists(filePath)) File.Delete(filePath);

            WorkBook workbook = WorkBook.Create();
            WorkSheet sheet = workbook.CreateWorkSheet("test.xl");
            var range = sheet["A1:OO" + (distance.Count + 5)];
            foreach (var cell in range)
            {
                int r = cell.RowIndex;
                int c = cell.ColumnIndex;
                if (r > distance.Count || c > distance.Count) continue;
                if (r == 0) // first row for names
                {
                    if (c == 0) continue;
                    cell.Value = names[c - 1];
                }
                else if (c == 0) // first column for names
                {
                    if (r == 0) continue;
                    cell.Value = names[r - 1];
                }
                else
                {
                    cell.Value = decimal.Round((decimal)distance[r - 1][c - 1], 3, MidpointRounding.AwayFromZero);
                }
            }
            workbook.SaveAs(filePath);
        }
    }
}

