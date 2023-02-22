using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApi.Classifiers
{
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
        private Dictionary<string, int[][]> _model;

        KNeighborsClassifier(int nNeighbors = 5)
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
        ///     Returns a dataframe containing the cell information classified matrix and unclassified matrix
        ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance
        ///              2                  3                 3                  2             1.65
        /// </returns>
        Dictionary<string, double> GetDistance(int[] model, int[] item)
        {
            double distance =
                Math.Sqrt(Math.Pow(model[0] - item[0], 2) + Math.Pow(model[1] - item[1], 2));
            return new Dictionary<string, double>
            {
                // The idea of using dictionary to list items is to keep a track of the elements with convenience.
                { "Classified Row", model[0] }, { "Classified Col", model[1] }, { "Unclassified Row", item[0] },
                { "Unclassified Col", item[1] }, { "Distance", distance }
            };
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
        /// <returns>
        ///     Returns a dataframe containing the cell information classified matrix and unclassified matrix.
        ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance
        ///              2                  3                 3                  4             1.65
        ///              4                  5                 3                  4             1.23
        ///             ...                ...               ...                ...             ...
        /// </returns>
        Dictionary<string, List<double>> FirstNValues(List<int[]> classifiedMatrixIndexes, int[] unclassifiedIdx)
        {
            var rawDistanceTable = new Dictionary<string, List<double>>()
            {
                { "Classified Row", new List<double>() }, { "Classified Col", new List<double>() },
                { "Unclassified Row", new List<double>() },
                { "Unclassified Col", new List<double>() }, { "Distance", new List<double>() }
            };

            foreach (var classifiedIdx in classifiedMatrixIndexes)
            {
                foreach (var KeyValue in GetDistance(classifiedIdx, unclassifiedIdx))
                    rawDistanceTable[KeyValue.Key].Add(KeyValue.Value);
            }

            var sortedDistanceTable = new Dictionary<string, List<double>>()
            {
                { "Classified Row", new List<double>() }, { "Classified Col", new List<double>() },
                { "Unclassified Row", new List<double>() },
                { "Unclassified Col", new List<double>() }, { "Distance", new List<double>() }
            };

            List<double> distances = rawDistanceTable["Distance"].Distinct().ToList();
            distances.Sort();

            for (int i = 0; i < _nNeighbors;) // This loop gathers the first nNeighbors.
            {
                // This stores the indexes of the distance/s from the unsorted Dictionary.
                var indices = rawDistanceTable["Distance"]
                    .Select((value, index) => new { value, index })
                    .Where(x => x.value.Equals(distances[i]))
                    .Select(x => x.index).ToList();

                // The counter increments multiple times if the distances are the same in coordinates.
                if (indices.Count > 1)
                    i = indices.Count - 1;

                foreach (var idx in indices)
                {
                    // loops through all the key value pairs of the unsorted matrix and add it to the sorted values given the indices is known.
                    foreach (var item in rawDistanceTable)
                        sortedDistanceTable[item.Key].Add(item.Value[idx]);
                }
            }

            return sortedDistanceTable;
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
        ///     Returns a dataframe containing the cell information classified matrix and unclassified matrix of
        ///     *all the models*.
        ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance
        ///              2                  3                 3                  2             1.65
        ///              6                  5                 3                  2             3.65
        ///              4                  5                 3                  4             1.23
        ///              3                  7                 3                  4             2.23
        ///             ...                ...               ...                ...             ...
        /// </returns>
        Dictionary<string, List<double>> GetComparisonMatrix(int[][] classifiedMatrix,
            int[][] unclassifiedMatrix)
        {
            List<int[]> classifiedMatrixIndexes = GetIndexes(classifiedMatrix);
            List<int[]> unclassifiedMatrixIndexes = GetIndexes(unclassifiedMatrix);

            var distanceTable = new Dictionary<string, List<double>>()
            {
                { "Classified Row", new List<double>() }, { "Classified Col", new List<double>() },
                { "Unclassified Row", new List<double>() },
                { "Unclassified Col", new List<double>() }, { "Distance", new List<double>() }
            };

            foreach (var index in unclassifiedMatrixIndexes)
            {
                foreach (var item in FirstNValues(classifiedMatrixIndexes, index))
                    distanceTable[item.Key].AddRange(item.Value);
            }

            return distanceTable;
        }


        /// <summary>
        ///     This function should take the Table and find the shortest distances to the different unclassified
        ///     coordinates i.e (3, 2) and cast a vote, i.e (A, B, C) which classification occurs the highest. This
        ///     should be done for all the unclassified coordinates.
        /// </summary>
        /// <param name="Table">
        ///     Takes in the tabular data of all the comparison points and gives the verdict on the maximum occurance.
        ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance  Classification
        ///              2                  3                 3                  2             1.65          A
        ///              6                  5                 3                  2             3.65          B
        ///              4                  5                 3                  4             1.23          A
        ///              3                  7                 3                  4             2.23          C
        ///             ...                ...               ...                ...             ...         ...
        /// </param>
        /// <returns>
        ///     |0 0 A B|
        ///     |C 0 A 0| = Unclassified Matrix
        ///     |A B 0 0|
        /// </returns>
        char[][] Voting(Dictionary<string, dynamic> Table)
        {
            //TODO: Implementation!!!
            return null;
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
            var distanceTables = new Dictionary<string, dynamic>()
            {
                { "Classified Row", new List<double>() }, { "Classified Col", new List<double>() },
                { "Unclassified Row", new List<double>() }, { "Unclassified Col", new List<double>() },
                { "Distance", new List<double>() }, { "Classification", new List<string>() }
            };

            foreach (var dict in _model)
            {
                foreach (var item in GetComparisonMatrix(dict.Value, unclassifiedMatrix))
                    distanceTables[item.Key].AddRange(item.Value);

                distanceTables["Classification"]
                    .AddRange(Enumerable.Repeat(dict.Key, distanceTables["Distance"].Count));
            }

            var info = Voting(distanceTables);
        }

        void Learn(object x, string[] tags)
        {
        }
    }
}