using System;
using System.Collections.Generic;
using NeoCortexApi.Entities;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApi.Classifiers
{
    public static class EnumExtension
    {
        /// <summary>
        /// Extends the foreach method to take a the item and index.
        /// For Example: List[int].foreach((item, idx) => (some operation!!!))
        /// </summary>
        /// <param name="self">Take in an Enumerable object</param>
        /// <typeparam name="T">Generic type string</typeparam>
        /// <returns>null</returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));
    }

    public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
    {
        /// <summary>
        /// Implementing the DefaultDict similar to python.
        /// i.e var sample = DefaultDictionary[string, int]()
        /// >>> sample['A']
        /// >>> 0
        /// prints the default value of in which is 0.
        /// </summary>
        /// <param name="key">A dictionary key.</param>
        public new TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out TValue val))
                {
                    val = new TValue();
                    Add(key, val);
                }

                return val;
            }
            set => base[key] = value;
        }
    }

    /// <summary>
    /// A generic container class which is a replacement for something similar in Python pandas implemented by
    /// nupic.org
    /// </summary>
    public class ClassificationAndDistance : IComparable<ClassificationAndDistance>
    {
        public string Classification { get; }
        public int Distance { get; }
        public int ClassificationNo { get; }

        /// <summary>
        /// An Constructor which initializes an object to store and handles comparison data.
        /// </summary>
        /// <param name="classification">Comparison classification with respect to model data.</param>
        /// <param name="distance">Distance with respect to classification of a model data.</param>
        /// <param name="classificationNo">Storing the SDR number under the classification.</param>
        public ClassificationAndDistance(string classification, int distance, int classificationNo)
        {
            Classification = classification;
            Distance = distance;
            ClassificationNo = classificationNo;
        }

        /// <summary>
        /// Implementation of the Method for sorting the given generic object.
        /// </summary>
        /// <param name="other">Past object of the implementation for comparison</param>
        /// <returns>Comparison between past and present object</returns>
        public int CompareTo(ClassificationAndDistance other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    /// <summary>
    /// This KNN classifier takes an input as a sequence of on: 1, off: 0 which is provided for classification.
    /// 
    /// For example: [ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0] = Unclassified
    ///
    /// Which then needs to run through a model of similar sequence to be classified, comparing which sequence it
    /// resembles closest to. The K-nearest-neighbor algorithm is implemented with this regard for getting the closest
    /// resemblance.
    ///
    /// For example:
    /// models = { A = [[ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0], ...],
    ///                B = [[10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0], ...] }
    /// unknown = [ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0]
    ///
    /// The verdict in this case is a list of ClassifierResult objects which is sort in the order of highest match.
    /// 
    /// </summary>
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int _nNeighbors = 1; // From Numenta's example 1 is default
        private Dictionary<string, List<int[]>> _models = new Dictionary<string, List<int[]>>();
        private int _sdrs = 10;

        /// <summary>
        /// This method compares a single value with a sequence of values from given sequence
        /// </summary>
        /// <param name="classifiedSequence">
        /// The active indices from the classified Sequence.
        /// </param>
        /// <param name="unclassifiedIdx">
        /// The active index from the unclassified Sequence.
        /// </param>
        /// <returns>
        /// Returns the smallest value of type int from the list.
        /// </returns>
        int LeastValue(ref int[] classifiedSequence, int unclassifiedIdx)
        {
            int shortestDistance = unclassifiedIdx;
            foreach (var classifiedIdx in classifiedSequence)
            {
                var distance = Math.Abs(classifiedIdx - unclassifiedIdx);
                if (shortestDistance > distance)
                    shortestDistance = distance;
            }

            return shortestDistance;
        }

        /// <summary>
        /// This function computes the distances of the unclassified points to the distance of the classified points.
        /// </summary>
        /// <param name="classifiedSequence">One of the sequences received from the SDR</param>
        /// <param name="unclassifiedSequence">The unknown sequence to be classified</param>
        /// <returns>
        /// Returns a dictionary mapping of the Unclassified sequence index to the shortest distance. Done for all
        /// indices of the unclassified sequence.
        /// </returns>
        Dictionary<int, int> GetDistanceTable(int[] classifiedSequence, ref int[] unclassifiedSequence)
        {
            // i.e: {1: [1.23, 1.65, 2.23, ...], ...}
            var distanceTable = new Dictionary<int, int>();

            foreach (var index in unclassifiedSequence)
                distanceTable[index] = LeastValue(ref classifiedSequence, index);

            return distanceTable;
        }

        /// <summary>
        /// This method takes a dictionary mapping of indices of the unclassified sequence to a list of
        /// ClassificationAndDistance objects which contains the distance, classification and classification No.
        /// </summary>
        /// <param name="mapping">
        /// A Mapping of unclassified sequence indices. to a list of ClassificationAndDistance objects.
        ///     Dictionary:  {
        ///                     2: [(ClassificationAndDistance obj 1), (ClassificationAndDistance obj 2), ...],
        ///                     5: [(ClassificationAndDistance obj 1), (ClassificationAndDistance obj 2), ...],
        ///                     ...
        ///                  }
        /// </param>
        /// <para name="howMany">The amount of desired outputs</para>
        /// <returns>
        /// Returns a list of ClassifierResult objects ranked based on the closest resemblances.
        /// </returns>
        List<ClassifierResult<string>> Voting(Dictionary<int, List<ClassificationAndDistance>> mapping, short howMany)
        {
            var votes = new DefaultDictionary<string, int>();
            var overLaps = new Dictionary<string, int>();
            var similarity = new Dictionary<string, double>();

            // Initializing the overlaps with 0
            foreach (var key in _models.Keys)
                overLaps[key] = 0;

            foreach (var coordinates in mapping)
            {
                for (int i = 0; i < _nNeighbors; i++)
                    votes[coordinates.Value[i].Classification] += 1;

                for (int i = 0;  i < coordinates.Value.Count; i++)
                {
                    if (coordinates.Value[i].Distance.Equals(0))
                        overLaps[coordinates.Value[i].Classification] += 1;
                }
            }

            var orderedVotes = votes.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var orderedOverLaps = overLaps.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (var paired in orderedOverLaps)
            {
                if (paired.Value != 0)
                    similarity[paired.Key] = (double)paired.Value / mapping.Count;
                else
                    similarity[paired.Key] = 0;
            }

            var result = new List<ClassifierResult<string>>();
            // Checks If the sequence have 50% of overlaps if not the data is ordered in voting manner.
            var orderedResults = orderedOverLaps.Values.First() > mapping.Count / 2
                ? orderedOverLaps.Keys
                : orderedVotes.Keys;

            foreach (var key in orderedResults)
            {
                var cls = new ClassifierResult<string>();
                cls.PredictedInput = key;
                cls.Similarity = similarity[key];
                cls.NumOfSameBits = overLaps[key];
                result.Add(cls);
            }

            return result.GetRange(0, howMany).ToList();
        }

        /// <summary>
        /// This method is called in the pipeline when an unknown sequence needs to be classified.
        /// </summary>
        /// <param name="unclassifiedCells">A sequence of Cell objects</param>
        /// <param name="howMany">NUmber f desired outputs</param>
        /// <returns>Returns a list of ClassifierResult objects ranked based on the closest resemblances</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(Cell[] unclassifiedCells, short howMany = 1)
        {
            if (unclassifiedCells.Length == 0)
                return new List<ClassifierResult<TIN>>();
            
            var unclassifiedSequence = unclassifiedCells.Select(idx => idx.Index).ToArray();
            _nNeighbors = _models.Values.Count;
            var mappedElements = new DefaultDictionary<int, List<ClassificationAndDistance>>();

            foreach (var model in _models)
            {
                foreach (var (sequence, idx) in model.Value.WithIndex())
                {
                    foreach (var index in GetDistanceTable(sequence, ref unclassifiedSequence))
                    {
                        var value = new ClassificationAndDistance(model.Key, index.Value, idx);
                        mappedElements[index.Key].Add(value);
                    }
                }
            }

            foreach (var coordinates in mappedElements)
                coordinates.Value.Sort();

            return Voting(mappedElements, howMany) as List<ClassifierResult<TIN>>;
        }

        /// <summary>
        /// Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="classification">The classification type</param>
        /// <param name="sdr">A sequence of cell positions</param>
        /// <returns>true if the sequence exist false if it doesn't</returns>
        private bool ContainsSdr(string classification, int[] sdr)
        {
            foreach (var item in _models[classification])
                return item.SequenceEqual(sdr);

            return false;
        }

        /// <summary>
        /// This Function adds and removes SDRs to the model
        /// </summary>
        /// <param name="input">The classification type</param>
        /// <param name="cells">object of type Cell</param>
        public void Learn(TIN input, Cell[] cells)
        {
            var classification = input as string;
            int[] cellIndicies = cells.Select(idx => idx.Index).ToArray();

            if (_models.ContainsKey(classification) == false)
                _models.Add(classification, new List<int[]>());

            if (!ContainsSdr(classification, cellIndicies))
                _models[classification].Add(cellIndicies);

            if (_models[classification].Count > _sdrs)
                _models[classification].RemoveRange(_sdrs, _models[classification].Count - _sdrs);

            var previousOne = _models[classification][Math.Max(0, _models[classification].Count - 2)];

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