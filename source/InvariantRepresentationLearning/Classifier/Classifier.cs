using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Utility;
using System.Diagnostics;

namespace Classifier
{
    public class MyClassifier<TLabel> : NeoCortexApi.Classifiers.HtmClassifier<string, List<int[]>>
    {

    }

    public class Classifier<TLabel>
    {
        private Dictionary<TLabel, List<int[]>> recordedEntries = new Dictionary<TLabel, List<int[]>>();

        private int maxRecordedElements = 10;


        ///// <summary>
        ///// Defines the predicting input.
        ///// </summary>
        //public class ClassifierResult<TLabel>
        //{
        //    /// <summary>
        //    /// The predicted input value.
        //    /// </summary>
        //    public TLabel PredictedInput { get; set; }

        //    /// <summary>
        //    /// Number of identical non-zero bits in the SDR.
        //    /// </summary>
        //    public int NumOfSameBits { get; set; }

        //    /// <summary>
        //    /// The similarity between the SDR of  predicted cell set with the SDR of the input.
        //    /// </summary>
        //    public double Similarity { get; set; }
        //}

        /// <summary>
        /// Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sdr"></param>
        /// <returns></returns>
        private bool ContainsSdr(TLabel input, int[] sdr)
        {
            foreach (var item in recordedEntries[input])
            {
                if (item.SequenceEqual(sdr))
                    return true;
                else
                    return false;
            }

            return false;
        }


        private int GetBestMatch(TLabel input, int[] cellIndicies, out double similarity, out int[] bestSdr)
        {
            int maxSameBits = 0;
            bestSdr = new int[1];

            foreach (var sdr in recordedEntries[input])
            {
                var numOfSameBitsPct = sdr.Intersect(cellIndicies).Count();
                if (numOfSameBitsPct >= maxSameBits)
                {
                    maxSameBits = numOfSameBitsPct;
                    bestSdr = sdr;
                }
            }

            similarity = Math.Round(MathHelpers.CalcArraySimilarity(bestSdr, cellIndicies), 2);

            return maxSameBits;
        }


        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP.</param>
        /// <param name="predictedOutput"></param>
        public void Learn(TLabel input, int[] output)
        {
            var cellIndicies = output;

            if (recordedEntries.ContainsKey(input) == false)
                recordedEntries.Add(input, new List<int[]>());

            // Store the SDR only if it was not stored under the same key already.
            if (!ContainsSdr(input, cellIndicies))
                recordedEntries[input].Add(cellIndicies);
            else
            {
                // for debugging
            }

            //
            // Make sure that only few last SDRs are recorded.
            if (recordedEntries[input].Count > maxRecordedElements)
            {
                Debug.WriteLine($"The input {input} has more ");
                recordedEntries[input].RemoveAt(0);
            }

            var previousOne = recordedEntries[input][Math.Max(0, recordedEntries[input].Count - 2)];

            if (!previousOne.SequenceEqual(cellIndicies))
            {
                // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / Math.Max((double)cellIndicies.Length, this.activeMap2[input].Length)));
                // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / (double)this.activeMap2[input].Length));
                var numOfSameBitsPct = previousOne.Intersect(cellIndicies).Count();
                Debug.WriteLine($"Prev/Now/Same={previousOne.Length}/{cellIndicies.Length}/{numOfSameBitsPct}");
            }
        }



        /// <summary>
        /// Gets multiple predicted values.
        /// </summary>
        /// <param name="inputCols">The current set of predictive cells.</param>
        /// <param name="howMany">The number of predections to return.</param>
        /// <returns>List of predicted values with their similarities.</returns>
        public List<ClassifierResult<TLabel>> GetPredictedInputValues(int[] inputCols, short howMany = 1)
        {
            List<ClassifierResult<TLabel>> res = new List<ClassifierResult<TLabel>>();
            double maxSameBits = 0;
            TLabel predictedValue = default;
            Dictionary<TLabel, ClassifierResult<TLabel>> dict = new Dictionary<TLabel, ClassifierResult<TLabel>>();

            var predictedList = new List<KeyValuePair<double, string>>();
            if (inputCols.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Debug.WriteLine($"Item length: {inputCols.Length}\t Items: {this.recordedEntries.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                var cellIndicies = inputCols;

                Debug.WriteLine($"Predictive cells: {cellIndicies.Length} \t {Helpers.StringifyVector(cellIndicies)}");

                foreach (var pair in this.recordedEntries)
                {
                    if (ContainsSdr(pair.Key, cellIndicies))
                    {
                        Debug.WriteLine($">indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{cellIndicies.Length}, Same Bits = {cellIndicies.Length.ToString("D3")}\t, Similarity 100.00 %\t {Helpers.StringifyVector(cellIndicies)}");

                        res.Add(new ClassifierResult<TLabel> { PredictedInput = pair.Key, Similarity = (float)100.0, NumOfSameBits = cellIndicies.Length });
                    }
                    else
                    {
                        // Tried following:
                        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(arr).Count()) / Math.Max(arr.Length, pair.Value.Count())));
                        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(celIndicies).Count()) / (double)pair.Value.Length));// ;
                        double similarity;
                        int[] bestMatch;
                        var numOfSameBitsPct = GetBestMatch(pair.Key, cellIndicies, out similarity, out bestMatch);// pair.Value.Intersect(cellIndicies).Count();
                        //double simPercentage = Math.Round(MathHelpers.CalcArraySimilarity(pair.Value, cellIndicies), 2);
                        dict.Add(pair.Key, new ClassifierResult<TLabel> { PredictedInput = pair.Key, NumOfSameBits = numOfSameBitsPct, Similarity = similarity });
                        predictedList.Add(new KeyValuePair<double, string>(similarity, pair.Key.ToString()));

                        if (numOfSameBitsPct > maxSameBits)
                        {
                            Debug.WriteLine($">indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{bestMatch.Length}, Same Bits = {numOfSameBitsPct.ToString("D3")}\t, Similarity {similarity.ToString("000.00")} % \t {Helpers.StringifyVector(bestMatch)}");
                            maxSameBits = numOfSameBitsPct;
                            predictedValue = pair.Key;
                            indxOfMatchingInp = n;
                        }
                        else
                            Debug.WriteLine($"<indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{bestMatch.Length}, Same Bits = {numOfSameBitsPct.ToString("D3")}\t, Similarity {similarity.ToString("000.00")} %\t {Helpers.StringifyVector(bestMatch)}");
                    }
                    n++;
                }
            }

            int cnt = 0;
            foreach (var keyPair in dict.Values.OrderByDescending(key => key.Similarity))
            {
                res.Add(keyPair);
                if (++cnt >= howMany)
                    break;
            }

            return res;
        }

        /// <summary>
        /// Clear all recorded Entries
        /// </summary>
        public void ClearState()
        {
            recordedEntries.Clear();
        }
    }
}