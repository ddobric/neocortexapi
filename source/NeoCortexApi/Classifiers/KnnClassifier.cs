﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeoCortexApi.Encoders;


namespace NeoCortexApi.Classifiers
{
    public class ClassificationAndDistance : IComparable<ClassificationAndDistance>
    {
        public string Classification { get; }
        public double Distance { get; }

        public ClassificationAndDistance(string classification, double distance)
        {
            Classification = classification;
            Distance = distance;
        }

        /// <summary>
        ///     Implementation of the Method for sorting itself.
        /// </summary>
        /// <param name="other">Past object of the implementation for comparison</param>
        /// <returns></returns>
        public int CompareTo(ClassificationAndDistance other)
        {
            if (Distance < other.Distance)  
                return -1;
            else if (Distance > other.Distance)
                return +1;
            else
                return 0;
        }
    }

    /// <summary>
    ///     This KNN classifier takes an input as a 2D array of on: 1, off: 0 which is provided for classification.
    /// 
    ///     |0 0 1 1|
    ///     |1 0 0 0| = Unclassified
    ///     |0 1 1 1|
    ///
    ///     Which then needs to run through a model of similar matrices to be classified which matrix it resembles
    ///     closest to. The K-nearest-neighbor 
    ///
    ///               {     |0 0 1 1|       |1 1 0 0|       |1 1 0 0| }
    ///  Dict model = { A = |1 0 0 1| , B = |0 1 1 1| , C = |1 1 1 1| }
    ///               {     |0 1 1 0|       |1 0 0 0|       |0 0 0 0| }
    ///
    ///     The verdict must be in terms of %. Where the distance of each point of the unknown/unclassified matrix is
    ///     calculated and compared to the known matrices. Whence a comparison is made by using the shortest nNeighbors
    ///     to each unclassified coordinate and assigns the classification keys A, B, C by considering the most amount
    ///     classifications closest to that point.
    ///
    ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance  Classification
    ///              2                  3                 3                  2             1.65          A
    ///              6                  5                 3                  2             3.65          B
    ///              4                  5                 3                  2             1.23          A
    ///              3                  7                 3                  2             2.23          C
    ///
    ///     In this case A is highest occurence hence A for the coordinate (3, 2).
    /// </summary>
    public class KNeighborsClassifier
    {
        private int _nNeighbors;
        private Dictionary<string, int[][]> _model = new Dictionary<string, int[][]>();

        KNeighborsClassifier(int nNeighbors = 7)
        {
            Debug.Assert(nNeighbors % 2 != 0);
            _nNeighbors = nNeighbors;
        }

        /// <summary>
        ///     Gets the distance using the euclidean principle using coordinate values int[] = (x, y).
        ///     i.e (x1, x2) & (x2, y2) => sqrt(pow(x2 - x1, 2) + pow(y2 - y1, 2))
        /// </summary>
        /// <param name="model">The model with active cells</param>
        /// <param name="item">The unidentified data with active cells</param>
        /// <returns>
        ///     Distance between 2 points.
        /// </returns>
        double GetDistance(int[] model, int[] item)
        {
            return Math.Sqrt(Math.Pow(model[0] - item[0], 2) + Math.Pow(model[1] - item[1], 2));
        }

        /// <summary>
        ///     Looks for the active cells in the matrix of the unclassified matrix and the classified matrix.
        /// </summary>
        /// <param name="dataItem">Is the matrix:
        ///     |0 0 1 1|
        ///     |1 0 0 1|
        ///     |0 1 1 0|
        /// </param>
        /// <returns>
        ///     Returns a 2d array containing the cell information of the active regions.
        ///     List[int[2]] A = [(0,2),(0,3),(1,0)....]
        /// </returns>
        List<int[]> GetIndexes(int[][] dataItem)
        {
            List<int[]> coordinates = new List<int[]>();
            for (int i = 0; i < dataItem.Length; i++)
            {
                for (int j = 0; j < dataItem[0].Length; j++)
                {
                    if (dataItem[i][j] == 1)
                        coordinates.Add(new int[] { i, j });
                }
            }

            return coordinates;
        }

        /// <summary>
        ///     This function return the first N values which is determined by the _nNeighbours.
        ///     i.e comparison unclassified coordinates (2, 3) to unclassified coordinates (1, 5), (3, 2) .... & (6, 7) 
        /// </summary>
        /// <param name="classifiedMatrixIndexes">
        ///     The active indices from the classified Matrix.
        ///     (1, 5), (3, 2) .... & (6, 7)
        /// </param>
        /// <param name="unclassifiedIdx">
        ///     The active index from the unclassified Matrix.
        ///     (2, 3)
        /// </param>
        /// <returns>
        ///     Returns a sorted distance List[float].
        ///         [1.23, 1.53, ...]
        /// </returns>
        List<double> FirstNValues(List<int[]> classifiedMatrixIndexes, int[] unclassifiedIdx)
        {
            var rawDistanceList = new List<double>();

            foreach (var classifiedIdx in classifiedMatrixIndexes)
                rawDistanceList.Add(GetDistance(classifiedIdx, unclassifiedIdx));

            rawDistanceList.Sort();
            return rawDistanceList.GetRange(0, _nNeighbors - 1);
        }

        /// <summary>
        ///     This function computes the distance of the unclassified point to the distance of the classified points.
        /// </summary>
        /// <param name="classifiedMatrix">
        ///       |0 0 1 1|
        ///   A = |1 0 0 1|
        ///       |0 1 1 0|
        /// </param>
        /// <param name="unclassifiedMatrix">
        ///     |0 0 1 1|
        ///     |1 0 1 0| = Unclassified Matrix
        ///     |1 1 0 0|
        /// </param>
        /// <returns>
        ///     Returns a dictionary mapping of a int[] to List[floats].
        ///     {
        ///         (1, 3): [1.23, 1.65, 2.23, ...],
        ///         (2, 0): [1.01, ...],
        ///         ...
        ///     }
        /// </returns>
        Dictionary<int[], List<double>> GetDistanceTable(int[][] classifiedMatrix, int[][] unclassifiedMatrix)
        {
            List<int[]> classifiedMatrixIndexes = GetIndexes(classifiedMatrix);
            List<int[]> unclassifiedMatrixIndexes = GetIndexes(unclassifiedMatrix);

            // i.e: {(1, 3): [1.23, 1.65, 2.23, ...], ...}
            var distanceTable = new Dictionary<int[], List<double>>();

            foreach (var index in unclassifiedMatrixIndexes)
                distanceTable[index].AddRange(FirstNValues(classifiedMatrixIndexes, index));

            return distanceTable;
        }

        /// <summary>
        ///     This function should take the Table and find the shortest distances to the different unclassified
        ///     coordinates i.e (3, 2) and cast a vote, i.e (A, B, C) which classification occurs the highest. This
        ///     should be done for all the unclassified coordinates.
        /// </summary>
        /// <param name="table">
        ///     Takes in the tabular data of all the comparison points and gives the verdict on the maximum occurrence.
        ///     {
        ///         (2, 3): [(ClassificationAndDistance 1), (ClassificationAndDistance 2), ...],
        ///         (2, 0): [(ClassificationAndDistance 1), (ClassificationAndDistance 2), ...],
        ///         ...
        ///     }
        /// </param>
        /// <returns>
        ///     Returns a mapping of coordinates to classification.
        ///     {
        ///         (2, 3): A,
        ///         (2, 0): B,
        ///         ...
        ///     }
        /// </returns>
        Dictionary<int[], string> Voting(Dictionary<int[], List<ClassificationAndDistance>> table)
        {
            var votes = new Dictionary<string, int>();
            var classification = new Dictionary<int[], string>();
            foreach (var coordinates in table)
            {
                int i;
                for (i = 0; i < _nNeighbors; i++)
                {
                    // Returns the Classification of [(ClassificationAndDistance 1), ...]
                    if (votes.ContainsKey(coordinates.Value[i].Classification))
                        votes[coordinates.Value[i].Classification] += 1;
                    else
                        votes[coordinates.Value[i].Classification] = 0;
                }

                // Handles if a variable contains the same value multiple times.
                for (; coordinates.Value[i].Distance <= coordinates.Value[i + 1].Distance; i++)
                    votes[coordinates.Value[i].Classification] += 1;

                var classificationType = votes.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                classification[coordinates.Key] = classificationType;
            }

            return classification;
        }

        /// <summary>
        ///     This function takes in the unclassified matrix and assigns a classification verdict. i.e:
        ///     A = 30%, B = 30%, C = 40%
        /// </summary>
        /// <param name="unclassifiedMatrix">
        ///     |0 0 1 1|
        ///     |1 0 0 0| = Unclassified Matrix
        ///     |0 1 1 0|
        /// </param>
        void GetPredictedInputValue(int[][] unclassifiedMatrix)
        {
            var mappedElements = new Dictionary<int[], List<ClassificationAndDistance>>();
            foreach (var dict in _model)
            {
                foreach (var indices in GetDistanceTable(dict.Value, unclassifiedMatrix))
                {
                    var values = indices.Value.Select(dist => new ClassificationAndDistance(dict.Key, dist)).ToList();
                    mappedElements[indices.Key].AddRange(values);
                }
            }

            foreach (var coordinates in mappedElements)
                coordinates.Value.Sort();

            var classificationType = Voting(mappedElements).Values.Max();

            Console.WriteLine($"Verdict is: {classificationType}");
        }

        void Learn(object x, string[] tags)
        {
        }
    }
}