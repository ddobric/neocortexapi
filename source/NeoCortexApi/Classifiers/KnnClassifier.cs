using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace NeoCortexApi.Classifiers
{
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int _nNeighbors;
        private Dictionary<string, int[][]> _model;

        KNeighborsClassifier(int nNeighbors = 5)
        {
            Debug.Assert(nNeighbors % 2 != 0);
            _nNeighbors = nNeighbors;
        }


        Dictionary<string, double> GetDistance(int[] comparable, int[] sample)
        {
            double distance =
                Math.Sqrt(Math.Pow(comparable[0] - sample[0], 2) + Math.Pow(comparable[0] - sample[0], 2));
            return new Dictionary<string, double>
            {
                { "Model Row", comparable[0] }, { "Model Col", comparable[1] }, { "Item Row", sample[0] },
                { "Item Col", sample[1] }, { "Distance", distance }
            };
        }

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

        async Task<Dictionary<string, List<double>>> GetComparisonMatrix(KeyValuePair<string, int[][]> dict,
            int[][] sample)
        {
            List<int[]> categoryIndexes = GetIndexes(dict.Value);
            List<int[]> sampleIndexes = GetIndexes(sample);
            var distanceTable = new Dictionary<string, List<double>>()
            {
                { "Model Row", new List<double>() }, { "Model Col", new List<double>() },
                { "Item Row", new List<double>() },
                { "Item Col", new List<double>() }, { "Distance", new List<double>() }
            };
            for (int i = 0; i < categoryIndexes.Count; i++)
            {
                foreach (var KeyValue in GetDistance(categoryIndexes[i], sampleIndexes[i]))
                    distanceTable[KeyValue.Key].Add(KeyValue.Value);
            }

            return distanceTable;
        }

        void GetPredictedInputValue(int[][] sample)
        {
            var matrices = new List<Task>();
            foreach (var dict in _model)
                matrices.Add(GetComparisonMatrix(dict, sample));

        }

        void Learn(object x, int[] tags)
        {
        }
    }
}