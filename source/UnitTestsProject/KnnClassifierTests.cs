using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    [TestClass]
    public class KnnClassifierTests
    {
        private int numColumns = 1024;
        private int cellsPerColumn = 25;
        private KNeighborsClassifier<string, ComputeCycle> knnClassifier;
        private Dictionary<string, List<double>> sequences;
        private List<Cell> lastActiveCells = new List<Cell>();

        [TestInitialize]
        public void Setup()
        {
            knnClassifier = new KNeighborsClassifier<string, ComputeCycle>();
            sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));

            LearnknnClassifier();
        }

        /// <summary>
        /// Here our target is to whether we are getting any predicted value for input we have given one sequence s1
        /// and check from this sequence each input, will we get prediction or not.
        /// </summary>
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [TestCategory("Prod")]
        [TestMethod]
        public void CheckNextValueIsNotEmpty(int input)
        {
            var predictiveCells = getMockCells(CellActivity.PredictiveCell);
            var res = knnClassifier.GetPredictedInputValues(predictiveCells.ToArray(), 3);

            var tokens = res.First().PredictedInput.Split('_');
            var tokens2 = res.First().PredictedInput.Split('-');
            Debug.WriteLine($"->{tokens2[tokens.Length - 1]}");
            var predictValue = Convert.ToInt32(tokens2[tokens.Length - 1]);
            //Assert.IsTrue(predictValue > 0, $"{predictValue} is not > 0");
        }

        /// <summary>
        /// Here we are checking if cell count is zero will we get any kind of exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void NoExceptionIfCellsCountIsZero()
        {
            var cells = new Cell[] { };
            var res = knnClassifier.GetPredictedInputValues(cells, 3);
            Assert.AreEqual(res.Count, 0, $"{res.Count} != 0");
        }


        /// <summary>
        /// Check how many prediction results will be retrieved.
        /// </summary>
        [DataTestMethod]
        [TestCategory("Prod")]
        [DataRow(3)]
        [DataRow(4)]
        [TestMethod]
        public void CheckHowManyOfGetPredictedInputValues(int howMany)
        {
            var predictiveCells = getMockCells(CellActivity.PredictiveCell);

            var res = knnClassifier.GetPredictedInputValues(predictiveCells.ToArray(), Convert.ToInt16(howMany));

            Assert.IsTrue(res.Count == howMany, $"{res.Count} != {howMany}");
        }

        private void LearnknnClassifier()
        {
            int maxCycles = 60;

            foreach (var sequenceKeyPair in sequences)
            {
                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    foreach (var input in sequenceKeyPair.Value)
                    {
                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > maxPrevInputs + 1)
                            previousInputs.RemoveAt(0);

                        /* In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        knnClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        memorized, it will match as the first one. */
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.Key);
                        List<Cell> actCells = getMockCells(CellActivity.ActiveCell);
                        knnClassifier.Learn(key, actCells.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Mock the cells data that we get from the Temporal Memory
        /// </summary>
        private List<Cell> getMockCells(CellActivity cellActivity)
        {
            var cells = new List<Cell>();
            for (int k = 0; k < Random.Shared.Next(5, 20); k++)
            {
                int parentColumnIndx = Random.Shared.Next(0, numColumns);
                int numCellsPerColumn = Random.Shared.Next(0, cellsPerColumn);
                int colSeq = Random.Shared.Next(0, cellsPerColumn);

                cells.Add(new Cell(parentColumnIndx, colSeq, numCellsPerColumn, cellActivity));
            }

            if (cellActivity == CellActivity.ActiveCell)
                lastActiveCells = cells;

            else if (cellActivity == CellActivity.PredictiveCell)
                /* Append one of the cell from lastActiveCells to the randomly generated predictive cells to have some
                similarity */
                cells.AddRange(lastActiveCells.GetRange(Random.Shared.Next(lastActiveCells.Count), 1));

            return cells;
        }

        private string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = string.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += prevInputs[i];
            }

            return $"{sequence}_{key}";
        }
    }
}