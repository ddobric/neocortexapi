using System;
using System.Collections.Generic;
using NeoCortexApi.Entities;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApi.Classifiers
{
    public class ClassificationAndDistance : IComparable<ClassificationAndDistance>
    {
        public string Classification { get; }
        public int Distance { get; }

        public ClassificationAndDistance(string classification, int distance)
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
            return Distance.CompareTo(other.Distance);
        }
    }

    /// <summary>
    ///     This KNN classifier takes an input as a 2D array of on: 1, off: 0 which is provided for classification.
    /// 
    ///     { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, } = Unclassified
    ///
    ///     Which then needs to run through a model of similar matrices to be classified which matrix it resembles
    ///     closest to. The K-nearest-neighbor 
    ///
    ///     models = { A = [[ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0], [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0]],
    ///                B = []
    ///
    ///     The verdict must be in terms of %. Where the distance of each point of the unknown/unclassified matrix is
    ///     calculated and compared to the known matrices. Whence a comparison is made by using the shortest nNeighbors
    ///     to each unclassified coordinate and assigns the classification keys A, B, C by considering the most amount
    ///     classifications closest to that point.
    ///     {
    ///         3: A,
    ///         0: B,
    ///         ...
    ///     }
    ///     In this case A is highest occurence hence A for the coordinate (3, 2).
    /// </summary>
    public class KNeighborsClassifier
    {
        private int _nNeighbors = 1;
        private Dictionary<string, List<int[]>> _models = new Dictionary<string, List<int[]>>();
        private int _sdrs = 10;

        /// <summary>
        ///     This function return the first N values which is determined by the _nNeighbours.
        ///     i.e comparison unclassified coordinates 2 to unclassified coordinates [1, 3 .... 6, 7] 
        /// </summary>
        /// <param name="classifiedSequence">
        ///     The active indices from the classified Matrix.
        ///     [1, 3 .... 6, 7]
        /// </param>
        /// <param name="unclassifiedIdx">
        ///     The active index from the unclassified Matrix.
        ///     2
        /// </param>
        /// <returns>
        ///     Returns a sorted distance List[float].
        ///         [1.23, 1.53, ...]
        /// </returns>
        int LeastValue(int[] classifiedSequence, int unclassifiedIdx)
        {
            var rawDistanceList = new List<int>();

            foreach (var classifiedIdx in classifiedSequence)
                rawDistanceList.Add(Math.Abs(classifiedIdx - unclassifiedIdx));

            rawDistanceList.Sort();
            return rawDistanceList[0];
        }

        /// <summary>
        ///     This function computes the distance of the unclassified point to the distance of the classified points.
        /// </summary>
        /// <param name="classifiedSequence">
        ///     A = [1.01, ...]
        /// </param>
        /// <param name="unclassifiedSequence">
        ///     [1.23, 1.65, 2.23, ...] = Unclassified Matrix
        /// </param>
        /// <returns>
        ///     Returns a dictionary mapping of a int[] to List[floats].
        ///     {
        ///         (1, 3): [1.23, 1.65, 2.23, ...],
        ///         (2, 0): [1.01, ...],
        ///         ...
        ///     }
        /// </returns>
        Dictionary<int, List<int>> GetDistanceTable(int[] classifiedSequence, int[] unclassifiedSequence)
        {
            // i.e: {1: [1.23, 1.65, 2.23, ...], ...}
            var distanceTable = new Dictionary<int, List<int>>();

            foreach (var index in unclassifiedSequence)
            {
                if (distanceTable.ContainsKey(index))
                    distanceTable[index].Add(LeastValue(classifiedSequence, index));
                else
                    distanceTable[index] = new List<int>{LeastValue(classifiedSequence, index)};
            }

            return distanceTable;
        }

        /// <summary>
        ///     This function should take the Table and find the shortest distances to the different unclassified
        ///     coordinates i.e 3 and cast a vote, i.e (A, B, C) which classification occurs the highest. This
        ///     should be done for all the unclassified coordinates. After which the max number of occurence is
        ///     considered 
        /// </summary>
        /// <param name="table">
        ///     Takes in the tabular data of all the comparison points and gives the verdict on the maximum occurrence.
        ///     {
        ///         2: [(ClassificationAndDistance 1), (ClassificationAndDistance 2), ...],
        ///         3: [(ClassificationAndDistance 1), (ClassificationAndDistance 2), ...],
        ///         ...
        ///     }
        /// </param>
        /// <returns>
        ///     Returns a list of classifier results.
        ///     {
        ///         [ClassifierResult 1, ClassifierResult 2, ...]
        ///     }
        /// </returns>
        List<ClassifierResult<string>> Voting(Dictionary<int, List<ClassificationAndDistance>> table)
        {
            var votes = new Dictionary<string, int>();
            var orderedDict = new Dictionary<string, int>();

            foreach (var coordinates in table)
            {
                for (int i = 0; i < _nNeighbors && i < coordinates.Value.Count; i++)
                {
                    if (votes.ContainsKey(coordinates.Value[i].Classification))
                        votes[coordinates.Value[i].Classification] += 1;
                    else
                        votes[coordinates.Value[i].Classification] = 1;
                }

                orderedDict = votes.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            var result = new List<ClassifierResult<string>>();
            foreach (var paired in orderedDict)
            {
                var cls = new ClassifierResult<string>();
                cls.PredictedInput = paired.Key;
                result.Add(cls);
            }

            return result;
        }

        /// <summary>
        ///     This function needs to classify the sequence of numbers that is provided to it.
        /// </summary>
        /// <param name="unclassifiedCells">
        ///     Takes in a sequence of numbers which needs to be classified and fed into the SDR.
        ///     i.e { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, }
        /// </param>
        /// <param name="sdr">Number of SDRs stored under the same classification</param>
        public List<ClassifierResult<string>> GetPredictedInputValues(Cell[] unclassifiedCells, int sdr)
        {
            var unclassifiedSequence = unclassifiedCells.Select(idx => idx.Index).ToArray();
            _sdrs = sdr;
            var mappedElements = new Dictionary<int, List<ClassificationAndDistance>>();

            foreach (var model in _models)
            {
                foreach (var sequence in model.Value)
                {
                    foreach (var index in GetDistanceTable(sequence, unclassifiedSequence))
                    {
                        var values = index.Value
                            .Select(dist => new ClassificationAndDistance(model.Key, dist))
                            .ToList();

                        if (mappedElements.ContainsKey(index.Key))
                            mappedElements[index.Key].AddRange(values);
                        else
                            mappedElements[index.Key] = values;
                    }
                }
            }

            foreach (var coordinates in mappedElements)
                coordinates.Value.Sort();

            return Voting(mappedElements);
        }

        /// <summary>
        /// Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sdr"></param>
        /// <returns></returns>
        private bool ContainsSdr(string input, int[] sdr)
        {
            foreach (var item in _models[input])
                return item.SequenceEqual(sdr);

            return false;
        }

        /// <summary>
        ///     This Function adds and removes SDR's to the model.
        /// </summary>
        /// <param name="input">The classification type.</param>
        /// <param name="cell">object of type Cell</param>
        public void Learn(string input, Cell[] cell)
        {
            int[] cellIndicies = cell.Select(idx => idx.Index).ToArray();

            if (_models.ContainsKey(input) == false)
                _models.Add(input, new List<int[]>());

            if (!ContainsSdr(input, cellIndicies))
                _models[input].Add(cellIndicies);

            if (_models[input].Count > _sdrs)
                _models[input].RemoveRange(_sdrs, _models[input].Count - _sdrs);

            var previousOne = _models[input][Math.Max(0, _models[input].Count - 2)];

            if (!previousOne.SequenceEqual(cellIndicies))
            {
                var numOfSameBitsPct = previousOne.Intersect(cellIndicies).Count();
                Debug.WriteLine($"Prev/Now/Same={previousOne.Length}/{cellIndicies.Length}/{numOfSameBitsPct}");
            }
        }

        public void ClearState()
        {
            _models.Clear();
        }
    }
}