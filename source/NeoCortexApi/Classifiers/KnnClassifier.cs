using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// _model: is a list of dictionaries. Which consist of classified data.
    /// 
    ///           Value
    ///         |0 0 1 1|
    ///   Key = |1 0 0 0|
    ///         |0 1 1 0|
    ///      _nNeighbors = 5 by default; which selects the nearest 5 elements.
    ///
    /// </summary>
    /// <typeparam name="TIN"></typeparam>
    /// <typeparam name="TOUT"></typeparam>
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
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
        ///     Returns a dataframe containing the cell information model and unidentified
        ///          Model Row   Model Col   Item Row   Item Col   Distance
        ///              2          3           3           2         1.65
        /// </returns>
        Dictionary<string, double> GetDistance(int[] model, int[] item)
        {
            double distance =
                Math.Sqrt(Math.Pow(model[0] - item[0], 2) + Math.Pow(model[0] - item[0], 2));
            return new Dictionary<string, double>
            {
                { "Trained Row", model[0] }, { "Trained Col", model[1] }, { "Test Row", item[0] },
                { "Test Col", item[1] }, { "Distance", distance }
            };
        }

        /// <summary>
        ///     Looks for the active cells in the matrix.
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

        void GetPredictedInputValue(int[][] test)
        {
            foreach (var dict in _model)
            {
                List<int[]> trainedIndexes = GetIndexes(dict.Value);
                List<int[]> testIndexes = GetIndexes(test);

                Dictionary<string, List<double>> distanceMatrix;
                for (int i = 0; i < categoryIndexes.Count; i++)
                {
                    //TODO: use multiple threads for compute return avg CompareIndexes()
                    Dictionary<string, double> distanceDict = GetDistance(trainedIndexes[i], testIndexes[i]);
                }
            }
        }

        void Learn(object x, int[] tags)
        {
        }
    }
}