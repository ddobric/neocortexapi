using System;
using System.Collections.Generic;
using NeoCortexApi.Entities;
using System.Linq;

/*
The KNN (K-Nearest-Neighbor) Classifier is designed and integrated with the Neocortex API. It takes in a
sequence of values and preassigned labels to train the model. Once the model (a Dictionary mapping of labels to
their sequences) is trained the user can give unclassified sequence that needs to be labeled.

There are three labels A, B and C which has 6 sequnces in total each label has two sequnces we will use these sequnces to train the Classifier and then the classifier will
predict the label value for unclassified sequence.

Take a look at below example:
_models = {
    "A" : [[1, 3, 4, 7, 12, 13, 14], [2, 3, 5, 6, 7, 8, 12]],
    "B" : [[0, 4, 5, 6, 9, 10, 13], [2, 3, 4, 5, 6, 7, 8]],
    "C" : [[1, 4, 5, 6, 8, 10, 15], [1, 2, 7, 8, 13, 15, 16]]
}

unknown = [1, 3, 4, 7, 12, 14, 15]

The Verdict: List = [A, B, ...] "A" being the closest match, "B" the next closest match and so on ...

The Output in this case is a list of ClassifierResult objects which is sort in the order of highest match.
*/

namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// Extends the foreach method to give out an item and index of type IEnumerable.
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// For Example: List[int].foreach((item, idx) => (some operation!!!))
        /// </summary>
        /// <param name="self">Take in an Enumerable object</param>
        /// <typeparam name="T">A single Generic item from the IEnumerable</typeparam>
        /// <returns>null</returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));
    }

    /// <summary>
    /// Returns the default value of the declared type.
    /// i.e var sample = DefaultDictionary[string, int]()
    /// >>> sample['A']
    /// >>> 0
    /// </summary>
    /// <typeparam name="TKey">A key of Generic type.</typeparam>
    /// <typeparam name="TValue">A newly created value of Generic type.</typeparam>
    public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
    {
        /// <summary>
        /// Implementing the DefaultDict similar to python.
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
        /// <summary>
        /// Comparison classification with respect to model data.
        /// </summary>
        public string Classification { get; }

        /// <summary>
        /// Distance with respect to classification of a model data.
        /// </summary>
        public int Distance { get; }

        /// <summary>
        /// Storing the SDR number under the classification. 
        /// </summary>
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
        /// <param name="other">Past object of the implementation for comparison.</param>
        /// <returns>Comparison between past and present object.</returns>
        public int CompareTo(ClassificationAndDistance other) => Distance.CompareTo(other.Distance);
    }

    /// <summary>
    /// Implementation of the KNN algorithm. 
    /// </summary>
    public class KNeighborsClassifier<TIN, TOUT> : IClassifierKnn<TIN, TOUT>
    {
        private int _nNeighbors = 1; // From Numenta's example 1 is default
        private DefaultDictionary<string, List<int[]>> _sdrMap = new DefaultDictionary<string, List<int[]>>();
        private int _sdrs = 10;

        /// <summary>
        /// This method compares a single value with a sequence of values from given sequence.
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
        private int LeastValue(ref int[] classifiedSequence, int unclassifiedIdx)
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
        /// <param name="classifiedSequence">One of the sequences received from the SDR.</param>
        /// <param name="unclassifiedSequence">The unknown sequence to be classified.</param>
        /// <returns>
        /// Returns a dictionary mapping of the Unclassified sequence index to the shortest distance. Done for all
        /// indices of the unclassified sequence.
        /// </returns>
        private Dictionary<int, int> GetDistanceTable(int[] classifiedSequence, ref int[] unclassifiedSequence)
        {
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
        /// <param name="howMany">The amount of desired outputs.</param>
        /// <returns>
        /// Returns a list of ClassifierResult objects ranked based on the closest resemblances.
        /// </returns>
        private List<ClassifierResult<string>> Voting(Dictionary<int, List<ClassificationAndDistance>> mapping,
            short howMany)
        {
            var votes = new DefaultDictionary<string, int>();
            var overLaps = new Dictionary<string, int>();
            var similarity = new Dictionary<string, double>();

            // Initializing the overlaps with 0
            foreach (var key in _sdrMap.Keys)
                overLaps[key] = 0;

            foreach (var coordinates in mapping)
            {
                for (int i = 0; i < _nNeighbors; i++)
                    votes[coordinates.Value[i].Classification] += 1;

                for (int i = 0; i < coordinates.Value.Count; i++)
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
        /// <param name="unclassifiedCells">A sequence of Cell objects.</param>
        /// <param name="howMany">Number of desired outputs.</param>
        /// <returns>Returns a list of ClassifierResult objects ranked based on the closest resemblances</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(Cell[] unclassifiedCells, short howMany = 1)
        {
            if (unclassifiedCells.Length == 0)
                return new List<ClassifierResult<TIN>>();

            var unclassifiedSequence = unclassifiedCells.Select(idx => idx.Index).ToArray();
            var mappedElements = new DefaultDictionary<int, List<ClassificationAndDistance>>();
            _nNeighbors = _sdrMap.Values.Count;

            foreach (var sdrList in _sdrMap)
            {
                foreach (var (sequence, idx) in sdrList.Value.WithIndex())
                {
                    foreach (var dict in GetDistanceTable(sequence, ref unclassifiedSequence))
                        mappedElements[dict.Key].Add(new ClassificationAndDistance(sdrList.Key, dict.Value, idx));
                }
            }

            foreach (var mappings in mappedElements)
                mappings.Value.Sort(); //Sorting values according to distance

            return Voting(mappedElements, howMany) as List<ClassifierResult<TIN>>;
        }

        /// <summary>
        /// This Function adds and removes SDRs to the model.
        /// </summary>
        /// <param name="input">The classification type.</param>
        /// <param name="cells">object of type Cell.</param>
        public void Learn(TIN input, Cell[] cells)
        {
            var label = input as string;
            int[] cellIndicies = cells.Select(idx => idx.Index).ToArray();

            if (!_sdrMap[label].Exists(seq => cellIndicies.SequenceEqual(seq)))
            {
                if (_sdrMap[label].Count > _sdrs)
                    _sdrMap[label].RemoveAt(0);
                _sdrMap[label].Add(cellIndicies);
            }
        }

        /// <summary>
        /// Clears the model from all the stored sequences.
        /// </summary>
        public void ClearState() => _sdrMap.Clear();
    }
}