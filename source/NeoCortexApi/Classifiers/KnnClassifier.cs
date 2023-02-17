using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// This KNN classified takes an input as a 2D array of on: 1, off: 0 which is provided for classification.
    /// 
    ///     |0 0 1 1|
    ///     |1 0 0 0|
    ///     |0 1 1 0|
    ///
    /// Which then needs to run through a model of similar martices to be classified which matrix it resembles
    /// closest to.
    ///
    ///               {     |0 0 1 1|       |1 1 0 0|       |0 0 0 0| }
    ///  Dict model = { A = |1 0 0 0| , B = |0 1 1 1| , C = |1 1 1 1| }
    ///               {     |0 1 1 0|       |1 0 0 0|       |0 0 0 0| }
    ///
    /// The verdict must be in terms of %. Where each point of the unknown matrix is calculated and  
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
        ///     Gets the distance using the equilidean principle using coordinate values (x, y).
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
                Math.Sqrt(Math.Pow(model[0] - item[0], 2) + Math.Pow(model[0] - item[0], 2));
            return new Dictionary<string, double>
            {
                { "Classified Row", model[0] }, { "Classified Col", model[1] }, { "Unclassified Row", item[0] },
                { "Unclassified Col", item[1] }, { "Distance", distance }
            };
        }

        /// <summary>
        ///     Looks for the active cells in the matrix of the unclassified matrix and the classified matrix.
        /// </summary>
        /// <param name="dataItem">Is the matrix:
        ///     |0 0 1 1|
        ///     |1 0 0 0|
        ///     |0 1 1 0|
        /// </param>
        /// <returns>
        ///     Returns a 2d array containing the cell information of the active regions.
        ///     A[(0,2),(0,3),(1,0)....]
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

                if (indices.Count > 1)
                    i = indices.Count - 1; // The counter increments multiple times if the distances are the same in coordinates.
                
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
        /// This function computes the distance of the unclassified point to the distance of the classified points.
        /// </summary>
        /// <param name="classifiedMatrix">
        ///       |0 0 1 1|
        ///   A = |1 0 0 0|
        ///       |0 1 1 0|
        /// </param>
        /// <param name="unclassifiedMatrix">
        ///     |0 0 1 1|
        ///     |1 0 0 0| = ?
        ///     |0 1 1 0|
        /// </param>
        /// <returns>
        ///     Returns a dataframe containing the cell information classified matrix and unclassified matrix of all the models
        ///          Classified Row   Classified Col   Unclassified Row   Unclassified Col   Distance
        ///              2                  3                 3                  2             1.65
        ///              4                  5                 3                  4             1.23
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
                FirstNValues(classifiedMatrixIndexes, index);


            return distanceTable;
        }

        /// <summary>
        ///     This function should return the voting
        /// </summary>
        /// <param name="Matrix"></param>
        /// <returns></returns>
        string Voting(Dictionary<string, List<double>> Matrix)
        {
        }

        /// <summary>
        ///     This function takes in the unclassified matrix and assigns a classification verdict.
        /// </summary>
        /// <param name="unclassifiedMatrix">
        ///     |0 0 1 1|
        ///     |1 0 0 0| = ?
        ///     |0 1 1 0|
        /// </param>
        void GetPredictedInputValue(int[][] unclassifiedMatrix)
        {
            var matrices = new List<Task>();
            foreach (var dict in _model)
                GetComparisonMatrix(dict.Value, unclassifiedMatrix);
        }

        void Learn(object x, string[] tags)
        {
        }
    }
}