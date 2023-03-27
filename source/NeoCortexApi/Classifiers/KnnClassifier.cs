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
        ///     Extends the foreach method to take a the item and index.
        ///     i.e List[int].foreach((item, idx) => (some operation!!!))
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
        ///     Implementing the DefaultDict similar to python.
        ///     i.e var sample = DefaultDictionary[string, int]()
        ///         >>> sample['A']
        ///         >>> 0
        ///     prints the default value of in which is 0.
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

    public class ClassificationAndDistance : IComparable<ClassificationAndDistance>
    {
        public string Classification { get; }
        public int Distance { get; }
        public int ClassificationNo { get; }

        /// <summary>
        ///     Constructor which initializes an object to store and handles comparison data.
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
        ///   Implementation of the Method for sorting itself.
        /// </summary>
        /// <param name="other">Past object of the implementation for comparison</param>
        /// <returns>CompareTo </returns>
        public int CompareTo(ClassificationAndDistance other)
        {
<<<<<<< HEAD
            if (Distance < other.Distance)  
                return -1;
            else if (Distance > other.Distance)
                return +1;
            else
                return 0;
=======
            return Distance.CompareTo(other.Distance);
>>>>>>> 380161f82b0fb1dbf497dd5ae1e5ef5325010eea
        }
    }

    /// <summary>
    ///     This KNN classifier takes an input as a 2D array of on: 1, off: 0 which is provided for classification.
    /// 
    ///     [ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, ] = Unclassified
    ///
    ///     Which then needs to run through a model of similar sequence to be classified which sequence it resembles
    ///     closest to. The K-nearest-neighbor 
    ///
    ///     models = { A = [[ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0], [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0]],
    ///                B = [] }
    /// </summary>
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
<<<<<<< HEAD
        private int _nNeighbors;
        private Dictionary<string, int[][]> _model = new Dictionary<string, int[][]>();

        KNeighborsClassifier(int nNeighbors = 3)
        {
            Debug.Assert(nNeighbors % 2 != 0);
            _nNeighbors = nNeighbors;
        }
=======
        private int _nNeighbors = 1; // From Numenta's example 1 is default
        private Dictionary<string, List<int[]>> _models = new Dictionary<string, List<int[]>>();
        private int _sdrs = 10;
>>>>>>> 380161f82b0fb1dbf497dd5ae1e5ef5325010eea

        /// <summary>
        ///     This function compares the a single value with a sequence of values
        ///     i.e comparison unclassified coordinates 2 to unclassified coordinates [1, 3 .... 6, 7] 
        /// </summary>
        /// <param name="classifiedSequence">
        ///     The active indices from the classified Sequence.
        ///     [1, 3 .... 6, 7]
        /// </param>
        /// <param name="unclassifiedIdx">
        ///     The active index from the unclassified Sequence.
        ///     2
        /// </param>
        /// <returns>
        ///     Returns the smallest value of type int from the list.
        ///     [1, 5, ...] => 1
        /// </returns>
        int LeastValue(ref int[] classifiedSequence, int unclassifiedIdx)
        {
            int smallestDistance = unclassifiedIdx;
            foreach (var classifiedIdx in classifiedSequence)
            {
                var distance = Math.Abs(classifiedIdx - unclassifiedIdx);
                if (smallestDistance > distance)
                    smallestDistance = distance;
            }

            return smallestDistance;
        }

        /// <summary>
        ///     This function computes the distance of the unclassified point to the distance of the classified points.
        /// </summary>
        /// <param name="classifiedSequence">
        ///     A = [1.01, ...]
        /// </param>
        /// <param name="unclassifiedSequence">
        ///     [1.23, 1.65, 2.23, ...] = Unclassified sequence
        /// </param>
        /// <returns>
        ///     i.e Returns a dictionary mapping of a int to int.
        ///     {
        ///         0: 1.23,
        ///         2: 1.01,
        ///         ...
        ///     }
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
        /// Handles if a variable contains the same value multiple times. 
        /// {
        ///     for (; coordinates.Value[i].Distance <= coordinates.Value[i + 1].Distance; i++)
        ///           votes[coordinates.Value[i].Classification] += 1;
        /// }
        /// </returns>
        List<ClassifierResult<string>> Voting(Dictionary<int, List<ClassificationAndDistance>> table)
        {
            var votes = new DefaultDictionary<string, int>();
            var overLaps = new Dictionary<string, int>();
            var similarity = new Dictionary<string, double>();

            // Initializing the overlaps with 0
            foreach (var key in _models.Keys)
                overLaps[key] = 0;

            foreach (var coordinates in table)
            {
                for (int i = 0; i < _nNeighbors && i < coordinates.Value.Count; i++)
                {
                    votes[coordinates.Value[i].Classification] += 1;

                    if (coordinates.Value[i].Distance.Equals(0))
                        overLaps[coordinates.Value[i].Classification] += 1;
                }
            }

            var orderedVotes = votes.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var orderedOverLaps = overLaps.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (var paired in orderedOverLaps)
            {
                if (paired.Value != 0)
                    similarity[paired.Key] = (double)paired.Value / table.Count;
                else
                    similarity[paired.Key] = 0;
            }

            var result = new List<ClassifierResult<string>>();
            var orderedResults = orderedOverLaps.Values.First() > table.Count / 2
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

            return result;
        }

        void DynamicKnnAdjustment()
        {
            // Total length of the SDRs stored
            var modelLength = _models.Values.Select(value => value.Count).Aggregate(0, (acc, x) => acc + x) / 10;
            _nNeighbors = modelLength % 2 != 0 ? modelLength : modelLength + 1;
        }

        public List<ClassifierResult<TIN>> GetPredictedInputValues(int[] cellIndicies, short howMany = 1)
        {
            _sdrs = howMany;
            var mappedElements = new DefaultDictionary<int, List<ClassificationAndDistance>>();

            foreach (var model in _models)
            {
                foreach (var (sequence, idx) in model.Value.WithIndex())
                {
                    foreach (var index in GetDistanceTable(sequence, ref cellIndicies))
                    {
                        var value = new ClassificationAndDistance(model.Key, index.Value, idx);
                        mappedElements[index.Key].Add(value);
                    }
                }
            }

            foreach (var coordinates in mappedElements)
                coordinates.Value.Sort();

            return Voting(mappedElements) as List<ClassifierResult<TIN>>;
        }

        /// <summary>
<<<<<<< HEAD
        ///     This function takes in the unclassified matrix and assigns a classification verdict. i.e:
        ///  A = 30%, B = 30%, C = 40%
=======
        ///     This function needs to classify the sequence of numbers that is provided to it.
>>>>>>> 380161f82b0fb1dbf497dd5ae1e5ef5325010eea
        /// </summary>
        /// <param name="unclassifiedCells">
        ///     Takes in a sequence of numbers which needs to be classified and fed into the SDR.
        ///     i.e [ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, ...]
        /// </param>
        /// <param name="sdr">Number of SDRs stored under the same classification</param>
        public List<ClassifierResult<string>> GetPredictedInputValues(Cell[] unclassifiedCells, int sdr)
        {
            var unclassifiedSequence = unclassifiedCells.Select(idx => idx.Index).ToArray();

            return GetPredictedInputValues(unclassifiedSequence, (short)sdr) as List<ClassifierResult<string>>;
        }

        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            throw new NotImplementedException(
                "This method will be removed in the future. Use GetPredictedInputValues instead.");
        }

<<<<<<< HEAD
        void Learn (object x, string[] tags)
=======
        /// <summary>
        ///     Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="classification">The classification type</param>
        /// <param name="sdr">A sequence of cell positions</param>
        /// <returns>true if the sequence exist false if it doesn't</returns>
        private bool ContainsSdr(string classification, int[] sdr)
>>>>>>> 380161f82b0fb1dbf497dd5ae1e5ef5325010eea
        {
            foreach (var item in _models[classification])
                return item.SequenceEqual(sdr);

            return false;
        }

        /// <summary>
        ///     This Function adds and removes SDR's to the model.
        /// </summary>
        /// <param name="classification">The classification type</param>
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