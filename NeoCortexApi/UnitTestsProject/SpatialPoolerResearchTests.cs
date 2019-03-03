using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UnitTestsProject
{
    [TestClass]
    public class SpatialPoolerResearchTests
    {
        [TestMethod]
        public void StableOutputOnSameInputTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[64 * 64];

            int[] inputVector = Helpers.GetRandomVector(32 * 32, parameters.Get<Random>(KEY.RANDOM));

            for (int i = 0; i < 100; i++)
            {
                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine(str);
            }

        }


        /// <summary>
        /// Corresponds to git\nupic\examples\sp\sp_tutorial.py
        /// </summary>
        [TestMethod]
        public void SPTutorialTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 1000 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(false);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[2048];

            int[] inputVector = Helpers.GetRandomVector(1000, parameters.Get<Random>(KEY.RANDOM));

            sp.compute(mem, inputVector, activeArray, true);

            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

            var str = Helpers.StringifyVector(activeCols);

            Console.WriteLine(str);

        }

        /// <summary>
        /// This test generates neghborhood cells for different radius and center of cell topoogy 64 single dimensional cell array.
        /// Reults are stored in CSV file, which is loaded by Python code 'neighborhood-test.py'to create a plotly diagram.
        /// </summary>
        [TestMethod]
        public void NeighborhoodTest()
        {
            var parameters = GetDefaultParams();

            int cellsDim1 = 64;
            int cellsDim2 = 64;

            parameters.setInputDimensions(new int[] { 32 });
            parameters.setColumnDimensions(new int[] { cellsDim1 });

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            for (int rad = 1; rad < 10; rad++)
            {
                using (StreamWriter sw = new StreamWriter($"neighborhood-test-rad{rad}-center-from-{cellsDim1}-to-{0}.csv"))
                {
                    sw.WriteLine($"{cellsDim1}|{cellsDim2}|{rad}|First column defines center of neiborhood. All other columns define indicies of neiborhood columns");

                    for (int center = 0; center < 64; center++)
                    {
                        var nbs = mem.getColumnTopology().GetNeighborhood(center, rad);

                        StringBuilder sb = new StringBuilder();

                        sb.Append(center);
                        sb.Append('|');

                        foreach (var neighobordCellIndex in nbs)
                        {
                            sb.Append(neighobordCellIndex);
                            sb.Append('|');
                        }

                        string str = sb.ToString();

                        sw.WriteLine(str.TrimEnd('|'));
                    }
                }
            }
        }


        /// <summary>
        /// Generates result of inhibition
        /// </summary>
        [TestMethod]
        public void SPInhibitionTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 1000 });
            parameters.setColumnDimensions(new int[] { 2048 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 2048);
            parameters.setGlobalInhibition(true);

            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] inputVector = Helpers.GetRandomVector(1000, parameters.Get<Random>(KEY.RANDOM));

            int[] activeArray = new int[2048];

            for (int i = 0; i < 10; i++)
            {
                var overlaps = sp.calculateOverlap(mem, inputVector);
                var strOverlaps = Helpers.StringifyVector(overlaps);

                var inhibitions = sp.inhibitColumns(mem, ArrayUtils.toDoubleArray(overlaps));
                var strInhibitions = Helpers.StringifyVector(inhibitions);

                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                var strActiveCols = Helpers.StringifyVector(activeCols);

                Console.WriteLine(strOverlaps);
                Console.WriteLine(strInhibitions);
                Console.WriteLine(strActiveCols);
            }

        }

        [TestMethod]
        public void CalculateSpeedOfLearningTest()
        {
            var parameters = GetDefaultParams();

            parameters.setInputDimensions(new int[] { 32, 32 });
            parameters.setColumnDimensions(new int[] { 64, 64 });
            parameters.setNumActiveColumnsPerInhArea(0.02 * 64 * 64);
            var sp = new SpatialPooler();

            var mem = new Connections();
            parameters.apply(mem);
            sp.init(mem);

            int[] activeArray = new int[64 * 64];

            int[] inputVector = Helpers.GetRandomVector(32 * 32, parameters.Get<Random>(KEY.RANDOM));

            int[] oldArray = new int[0];

            for (int i = 0; i < 100; i++)
            {
                sp.compute(mem, inputVector, activeArray, true);

                var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
                var distance = MathHelpers.GetHammingDistance(oldArray, activeCols);
                var str = Helpers.StringifyVector(activeCols);

                Debug.WriteLine($"{distance} - {str}");

                oldArray = activeCols;
            }

        }


        public static int[] addNoise(int[] input, double percent)
        {
            int noiseCount = (int)(percent * input.Length);

            var rand = new Random();

            List<int> flippedIndex = new List<int>();
            for (int i = 0; i < noiseCount; i++)
            {
                var indx = 0;
                do
                {
                    indx = rand.Next(input.Length);
                    if (!flippedIndex.Contains(indx))
                    {
                        flippedIndex.Add(indx);
                        break;
                    }
                } while (true);

                var val = input[indx];
                input[indx] = val == 0 ? 1 : 0;
            }
            return input;
        }

        [TestMethod]
        public void noiseAdditionTest()
        {
            Console.WriteLine();
            int[] overlaps = Helpers.GetRandomVector(10);
            Console.WriteLine(Helpers.StringifyVector(overlaps));
            var x = addNoise(overlaps, 0.20);
            Console.WriteLine(Helpers.StringifyVector(overlaps));
            Console.WriteLine(Helpers.StringifyVector(x));
        }

        [TestMethod]
        public void qualityOfLearning()
        {
            // double density = 0.5;
            // ARRAY SIZE SHOULD BE EQUAL TO NUM_OF_INPUTS
            // int[] overlaps = new int[] { 34, 17, 65, 22, 93, 15, 13, 71, 31, 34, 60, 63, 30, 70, 72, 69, 38, 97, 56, 99, 8, 64, 9, 23, 51, 73, 84, 88, 49, 8, 8, 18, 62, 68 };
            // int[] overlaps = new int[] { 37, 19, 66, 27, 95, 16, 17, 75, 34, 39, 60, 67, 30, 75, 77, 73, 40, 97, 59, 102, 11, 64, 11, 28, 54, 76, 89, 91, 49, 11, 9, 19, 64, 73 };
            // int[] overloapsWithMoreNoise = new int[] { 45, 31, 78, 26, 111, 40, 17, 80, 39, 41, 77, 70, 55, 76, 76, 90, 58, 112, 60, 111, 12, 79, 19, 40, 64, 85, 87, 105, 66, 18, 28, 26, 77, 92 };
            //  L  W  W  L  L  W  W   L   W    W (wrapAround=true)
            //  L  W  W  L  L  W  W   L   L    W (wrapAround=false)

            var parameters = GetDefaultParams();
            int[] overlaps = Helpers.GetRandomVector(10, parameters.Get<Random>(KEY.RANDOM));

            var NUM_OF_INPUTS = overlaps.Length;

            int[] activeArray = new int[NUM_OF_INPUTS];
            int[] temp = new int[NUM_OF_INPUTS];
            int iterationCount = 0;
            Console.WriteLine();


            parameters.setInputDimensions(new int[] { NUM_OF_INPUTS });
            parameters.setColumnDimensions(new int[] { 10 });
            Connections c = new Connections();
            parameters.apply(c);


            // int misMatchedDims = 6; // not 8
            SpatialPooler sp = new SpatialPooler();
            sp.init(c);
            //Internally calculated during init, to overwrite we put after init
            c.InhibitionRadius = 2;
            c.setWrapAround(true);
            do
            {
                iterationCount++;

                activeArray.CopyTo(temp, 0);
                sp.compute(c, overlaps, activeArray, true);
                Console.WriteLine(iterationCount + ": " + Helpers.StringifyVector(temp));
                Console.WriteLine(iterationCount + ": " + Helpers.StringifyVector(activeArray));
            } while (MathHelpers.GetHammingDistance(activeArray, temp) != 100);

            Console.WriteLine("Iteration Count: " + iterationCount);

            var quality = 1.0 / iterationCount;

            Console.WriteLine("Quality of Learning: " + quality);
        }

        [TestMethod]
        public void robustnessTest()
        {
            int[] overlaps = Helpers.GetRandomVector(100);
            int[] withNoise = new int[overlaps.Length];
            overlaps.CopyTo(withNoise, 0);
            addNoise(withNoise, 0.20);
            Console.WriteLine("org ip: " + Helpers.StringifyVector(overlaps));
            Console.WriteLine("flipped ip: " + Helpers.StringifyVector(withNoise));

            var parameters = GetDefaultParams();
            parameters.setInputDimensions(new int[] { overlaps.Length });
            parameters.setColumnDimensions(new int[] { 10 });
            Connections c = new Connections();
            parameters.apply(c);


            // int misMatchedDims = 6; // not 8
            SpatialPooler sp = new SpatialPooler();
            sp.init(c);
            //Internally calculated during init, to overwrite we put after init
            c.InhibitionRadius = 2;
            c.setWrapAround(true);

            double density = 0.5;
            var org = sp.inhibitColumnsLocal(c, ArrayUtils.toDoubleArray(overlaps), density);
            var noise = sp.inhibitColumnsLocal(c, ArrayUtils.toDoubleArray(withNoise), density);
            Console.WriteLine("org op: " + Helpers.StringifyVector(org));
            Console.WriteLine("flipped op: " + Helpers.StringifyVector(noise));

            double pni = MathHelpers.GetHammingDistance(overlaps, withNoise);
            double pno = MathHelpers.GetHammingDistance(org, noise);


            // int[] input = new int[] { 1, 2, 3, 4, 5, };
            // int[] inputChanged = new int[] { 1, 2, 4, 5, 5, };

            // // int ip = getChangesCount(input, inputChanged);
            // // double pni = (ip / input.Length) * 100;
            // double pni = MathHelpers.GetHammingDistance(input, inputChanged);

            // int[] output = new int[] { 1, 1, 1, 1, 0 };
            // int[] outputChanged = new int[] { 0, 1, 1, 1, 0 };

            // // int count = getChangesCount(ArrayUtils.toDoubleArray(output), 
            // // ArrayUtils.toDoubleArray(outputChanged));
            // // double pno = (op / output.Length) * 100;
            // double pno = MathHelpers.GetHammingDistance(output, outputChanged);

            double robustness = Math.Abs(pni / pno);
            Console.WriteLine("pni: " + pni);
            Console.WriteLine("pno: " + pno);
            Console.WriteLine("Robustness: " + robustness);
        }

        public static void calculateRobustness()
        {

        }


        public static int getChangesCount(double[] original, double[] withNoise)
        {
            int count = 0;
            for (var i = 0; i < original.Length; i++)
            {
                if (original[i] != withNoise[i]) count++;
            }

            return count;
        }

        // Returns true if arr1[0..n-1] and 
        // arr2[0..m-1] contain same elements. 
        public static bool areEqual(int[] arr1,
                                    int[] arr2)
        {
            int n = arr1.Length;
            int m = arr2.Length;

            // If lengths of array are not  
            // equal means array are not equal 
            if (n != m)
                return false;

            // Sort both arrays 
            Array.Sort(arr1);
            Array.Sort(arr2);

            // Linearly compare elements 
            for (int i = 0; i < n; i++)
                if (arr1[i] != arr2[i])
                    return false;

            // If all elements were same. 
            return true;
        }




        [TestMethod]
        public void justTesting()
        {
            int[] overlaps = new int[] { 34, 17, 65, 22, 93, 15, 13, 71, 31, 34, 60, 63, 30, 70, 72, 69, 38, 97, 56, 99, 8, 64, 9, 23, 51, 73, 84, 88, 49, 8, 8, 18, 62, 68 };
            int[] overlaps1 = new int[] { 35, 17, 65, 22, 93, 15, 13, 71, 31, 34, 60, 63, 30, 70, 72, 69, 38, 97, 56, 99, 8, 64, 9, 23, 51, 73, 84, 88, 49, 8, 8, 18, 62, 68 };
            var x = MathHelpers.GetHammingDistance(overlaps, overlaps1);
            Console.WriteLine(x);


            int[] input = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            int[] inputChanged = new int[] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 1 };
            Console.WriteLine("Hamming Distance Test: " + MathHelpers.GetHammingDistance(input, inputChanged));

            int ip1 = getChangesCount(ArrayUtils.toDoubleArray(input),
               ArrayUtils.toDoubleArray(inputChanged));
            double ip = MathHelpers.GetHammingDistance(input, inputChanged);
            double pni = ((double)ip1 / input.Length) * 100;


            Console.WriteLine("ip: " + ip);
            Console.WriteLine("pni: " + pni);

        }







        #region Private Helpers



        private static Parameters GetDefaultParams()
        {
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 5);
            parameters.Set(KEY.POTENTIAL_PCT, 0.5);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            //parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 3.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0.0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.1);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 10.0);
            parameters.Set(KEY.RANDOM, rnd);
            //int r = parameters.Get<int>(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA);

            return parameters;
        }

        #endregion


    }
}
