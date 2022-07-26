// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// Defines the predicting input.
    /// </summary>
    public class ClassifierResult<TIN>
    {
        /// <summary>
        /// The predicted input value.
        /// </summary>
        public TIN PredictedInput { get; set; }

        /// <summary>
        /// Number of identical non-zero bits in the SDR.
        /// </summary>
        public int NumOfSameBits { get; set; }

        /// <summary>
        /// The similarity between the SDR of  predicted cell set with the SDR of the input.
        /// </summary>
        public double Similarity { get; set; }
    }


    /// <summary>
    /// Classifier implementation which memorize all seen values.
    /// </summary>
    /// <typeparam name="TIN"></typeparam>
    /// <typeparam name="TOUT"></typeparam>
    public class HtmClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int maxRecordedElements = 10;

        private List<TIN> inputSequence = new List<TIN>();

        private Dictionary<int[], int> inputSequenceMap = new Dictionary<int[], int>();

        /// <summary>
        /// Recording of all SDRs. See maxRecordedElements.
        /// </summary>
        private Dictionary<TIN, List<int[]>> m_AllInputs = new Dictionary<TIN, List<int[]>>();

        /// <summary>
        /// Mapping between the input key and the SDR assootiated to the input.
        /// </summary>
        //private Dictionary<TIN, int[]> m_ActiveMap2 = new Dictionary<TIN, int[]>();

        /// <summary>
        /// Clears th elearned state.
        /// </summary>
        public void ClearState()
        {
            m_AllInputs.Clear();
        }

        /// <summary>
        /// Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sdr"></param>
        /// <returns></returns>
        private bool ContainsSdr(TIN input, int[] sdr)
        {
            foreach (var item in m_AllInputs[input])
            {
                if (item.SequenceEqual(sdr))
                    return true;
                else
                    return false;
            }

            return false;
        }


        private int GetBestMatch(TIN input, int[] cellIndicies, out double similarity, out int[] bestSdr)
        {
            int maxSameBits = 0;
            bestSdr = new int[1];

            foreach (var sdr in m_AllInputs[input])
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
        public void Learn(TIN input, Cell[] output)
        {
            var cellIndicies = GetCellIndicies(output);

            Learn(input, cellIndicies);
        }

        /// <summary>
        /// Assotiate specified input to the given set of predictive cells. This can also be used to classify Spatial Pooler Columns output as int array
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP as int array</param>
        public void Learn(TIN input, int[] cellIndicies)
        {
            if (m_AllInputs.ContainsKey(input) == false)
                m_AllInputs.Add(input, new List<int[]>());

            // Store the SDR only if it was not stored under the same key already.
            if (!ContainsSdr(input, cellIndicies))
                m_AllInputs[input].Add(cellIndicies);
            else
            {
                // for debugging
            }

            //
            // Make sure that only few last SDRs are recorded.
            if (m_AllInputs[input].Count > maxRecordedElements)
            {
                Debug.WriteLine($"The input {input} has more ");
                m_AllInputs[input].RemoveAt(0);
            }

            var previousOne = m_AllInputs[input][Math.Max(0, m_AllInputs[input].Count - 2)];

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
        /// <param name="predictiveCells">The current set of predictive cells.</param>
        /// <param name="howMany">The number of predections to return.</param>
        /// <returns>List of predicted values with their similarities.</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(Cell[] predictiveCells, short howMany = 1)
        {
            var cellIndicies = GetCellIndicies(predictiveCells);

            return GetPredictedInputValues(cellIndicies, howMany);
        }

        /// <summary>
        /// Gets multiple predicted values. This can also be used to classify Spatial Pooler Columns output as int array
        /// </summary>
        /// <param name="predictiveCells">The current set of predictive cells in int array.</param>
        /// <param name="howMany">The number of predections to return.</param>
        /// <returns>List of predicted values with their similarities.</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(int[] cellIndicies, short howMany = 1)
        {
            List<ClassifierResult<TIN>> res = new List<ClassifierResult<TIN>>();
            double maxSameBits = 0;
            TIN predictedValue = default;
            Dictionary<TIN, ClassifierResult<TIN>> dict = new Dictionary<TIN, ClassifierResult<TIN>>();

            var predictedList = new List<KeyValuePair<double, string>>();
            if (cellIndicies.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Debug.WriteLine($"Item length: {cellIndicies.Length}\t Items: {this.m_AllInputs.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                Debug.WriteLine($"Predictive cells: {cellIndicies.Length} \t {Helpers.StringifyVector(cellIndicies)}");

                foreach (var pair in this.m_AllInputs)
                {
                    if (ContainsSdr(pair.Key, cellIndicies))
                    {
                        Debug.WriteLine($">indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{cellIndicies.Length}, Same Bits = {cellIndicies.Length.ToString("D3")}\t, Similarity 100.00 %\t {Helpers.StringifyVector(cellIndicies)}");

                        res.Add(new ClassifierResult<TIN> { PredictedInput = pair.Key, Similarity = (float)100.0, NumOfSameBits = cellIndicies.Length });
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
                        dict.Add(pair.Key, new ClassifierResult<TIN> { PredictedInput = pair.Key, NumOfSameBits = numOfSameBitsPct, Similarity = similarity });
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
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="predictiveCells">The list of predictive cells.</param>
        /// <returns></returns>
        [Obsolete("This method will be removed in the future. Use GetPredictedInputValues instead.")]
        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            throw new NotImplementedException("This method will be removed in the future. Use GetPredictedInputValues instead.");
            // bool x = false;
            //double maxSameBits = 0;
            //TIN predictedValue = default;

            //if (predictiveCells.Length != 0)
            //{
            //    int indxOfMatchingInp = 0;
            //    Debug.WriteLine($"Item length: {predictiveCells.Length}\t Items: {m_ActiveMap2.Keys.Count}");
            //    int n = 0;

            //    List<int> sortedMatches = new List<int>();

            //    var celIndicies = GetCellIndicies(predictiveCells);

            //    Debug.WriteLine($"Predictive cells: {celIndicies.Length} \t {Helpers.StringifyVector(celIndicies)}");

            //    foreach (var pair in m_ActiveMap2)
            //    {
            //        if (pair.Value.SequenceEqual(celIndicies))
            //        {
            //            Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length}\tsimilarity 100pct\t {Helpers.StringifyVector(pair.Value)}");
            //            return pair.Key;
            //        }

            //        // Tried following:
            //        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(arr).Count()) / Math.Max(arr.Length, pair.Value.Count())));
            //        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(celIndicies).Count()) / (double)pair.Value.Length));// ;
            //        var numOfSameBitsPct = pair.Value.Intersect(celIndicies).Count();
            //        if (numOfSameBitsPct > maxSameBits)
            //        {
            //            Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");
            //            maxSameBits = numOfSameBitsPct;
            //            predictedValue = pair.Key;
            //            indxOfMatchingInp = n;
            //        }
            //        else
            //            Debug.WriteLine($"<indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");

            //        n++;
            //    }
            //}

            //return predictedValue;
        }
        /*
        //
        // This loop peeks the best input
        foreach (var pair in this.activeMap)
        {
            //
            // We compare only outputs which are similar in the length.
            // This is important, because some outputs, which are not related to the comparing output
            // might have much mode cells (length) than the current output. With this, outputs with much more cells
            // would be declared as matching outputs even if they are not.
            if ((Math.Min(arr.Length, pair.Key.Length) / Math.Max(arr.Length, pair.Key.Length)) > 0.9)
            {
                double numOfSameBitsPct = (double)((double)(pair.Key.Intersect(arr).Count() / (double)arr.Length));
                if (numOfSameBitsPct > maxSameBits)
                {
                    Debug.WriteLine($"indx:{n}\tbits/arrbits: {pair.Key.Length}/{arr.Length}\t{pair.Value} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                    maxSameBits = numOfSameBitsPct;
                    predictedValue = pair.Value;
                    indxOfMatchingInp = n;
                }

                //if (maxSameBits > 0.9)
                //{
                //    sortedMatches.Add(n);
                //    // We might have muliple matchin candidates.
                //    // For example: Let the matchin input be i1
                //    // I1 - c1, c2, c3, c4
                //    // I2 - c1, c2, c3, c4, c5, c6

                //    Debug.WriteLine($"cnt:{n}\t{pair.Value} = bits {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                //}
            }
            n++;
        }

        foreach (var item in sortedMatches)
        {

        }

        Debug.Write("[ ");
        for (int i = Math.Max(0, indxOfMatchingInp - 3); i < Math.Min(indxOfMatchingInp + 3, this.activeMap.Keys.Count); i++)
        {
            if (i == indxOfMatchingInp) Debug.Write("* ");
            Debug.Write($"{this.inputSequence[i]}");
            if (i == indxOfMatchingInp) Debug.Write(" *");

            Debug.Write(", ");
        }
        Debug.WriteLine(" ]");

        return predictedValue;
        //return activeMap[ComputeHash(FlatArray(output))];
    }
    return default(TIN);
    }*/


        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public string TraceState(string fileName = null)
        {
            StringWriter strSw = new StringWriter();

            StreamWriter sw = null;

            if (fileName != null)
                sw = new StreamWriter(fileName);

            List<TIN> processedValues = new List<TIN>();

            //
            // Trace out the last stored state.
            foreach (var item in this.m_AllInputs)
            {
                strSw.WriteLine("");
                strSw.WriteLine($"{item.Key}");
                strSw.WriteLine($"{Helpers.StringifyVector(item.Value.Last())}");
            }

            strSw.WriteLine("........... Cell State .............");

            foreach (var item in m_AllInputs)
            {
                strSw.WriteLine("");

                strSw.WriteLine($"{item.Key}");

                strSw.Write(Helpers.StringifySdr(new List<int[]>(item.Value)));

                //foreach (var cellState in item.Value)
                //{
                //    var str = Helpers.StringifySdr(cellState);
                //    strSw.WriteLine(str);
                //}
            }

            if (sw != null)
            {
                sw.Write(strSw.ToString());
                sw.Flush();
                sw.Close();
            }

            Debug.WriteLine(strSw.ToString());
            return strSw.ToString();
        }



        /*
    /// <summary>
    /// Traces out all cell indicies grouped by input value.
    /// </summary>
    public void TraceState2(string fileName = null)
    {

        List<TIN> processedValues = new List<TIN>();

        foreach (var item in activeMap.Values)
        {
            if (processedValues.Contains(item) == false)
            {
                StreamWriter sw = null;

                if (fileName != null)
                    sw = new StreamWriter(fileName.Replace(".csv", $"_Digit_{item}.csv"));

                Debug.WriteLine("");
                Debug.WriteLine($"{item}");

                foreach (var inp in this.activeMap.Where(i => EqualityComparer<TIN>.Default.Equals((TIN)i.Value, item)))
                {
                    Debug.WriteLine($"{Helpers.StringifyVector(inp.Key)}");

                    if (sw != null)
                        sw.WriteLine($"{Helpers.StringifyVector(inp.Key)}");
                }

                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }

                processedValues.Add(item);
            }
        }
    }
     */


        private string ComputeHash(byte[] rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(rawData);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private static byte[] FlatArray(Cell[] output)
        {
            byte[] arr = new byte[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = (byte)output[i].Index;
            }
            return arr;
        }

        private static int[] GetCellIndicies(Cell[] output)
        {
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }
            return arr;
        }

        private int PredictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }

        // TODO: Traces Print, reduce methods

        /// <summary>
        /// Calculate correlations from every saved SDRs of 2 selected Labels in to a 2D double matrix 
        /// </summary>
        /// <param name="label1">selected label 1</param>
        /// <param name="label2">selected label 2</param>
        /// <returns></returns>
        public double[,] TraceCrossSimilarity(TIN label1, TIN label2, bool visualize = false)
        {
            var entry1 = m_AllInputs[label1];
            var entry2 = m_AllInputs[label2];
            int dim1 = entry1.Count;
            int dim2 = entry2.Count;

            double[,] similarityMat = new double[dim1, dim2];

            for (int i = 0; i < dim1; i += 1)
            {
                for (int j = 0; j < dim2; j += 1)
                {
                    similarityMat[i, j] = MathHelpers.CalcArraySimilarity(entry1[i], entry2[j]);
                }
            }

            if (visualize)
            {
                Debug.WriteLine($"Saved SDRs of {label1}(rows)");
                Debug.WriteLine(NeoCortexApi.Helpers.StringifySdr(m_AllInputs[label1]));
                Debug.WriteLine("\n");

                Debug.WriteLine($"Saved SDRs of {label2}(columns)");
                Debug.WriteLine(NeoCortexApi.Helpers.StringifySdr(m_AllInputs[label2]));
                Debug.WriteLine("\n");

                Debug.WriteLine($"Correlation table of {label1}(row) and {label2}(column):");
                for (int i = 0; i < dim1; i += 1)
                {
                    for (int j = 0; j < dim2; j += 1)
                    {
                        Debug.Write(String.Format("{0, -10}", Math.Round(similarityMat[i, j], 4).ToString()));
                    }
                    Debug.WriteLine("\n");
                }
            }

            return similarityMat;
        }

        /// <summary>
        /// Calculate correlations from every saved SDRs of the selected Label with itself in to a 2D double matrix 
        /// </summary>
        /// <param name="label">selected label</param>
        /// <returns></returns>
        public double[,] TraceAutoSimilarity(TIN label, bool visualize = false)
        {
            return TraceCrossSimilarity(label,label, visualize);
        }

        /// <summary>
        /// Get the Max, Min and average value from the computed correlation matrix
        /// </summary>
        /// <param name="correlationMat2D"></param>
        /// <returns></returns>
        public Dictionary<string, double> GetCorrelationStat(double[,] correlationMat2D)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();

            double maxVal = 0;
            double minVal = 1000;
            double sum = 0;
            double count = 0;
            for (int i = 0; i < correlationMat2D.GetLength(0);i+=1)
            {
                for(int j = 0; j < correlationMat2D.GetLength(1); j++)
                {
                    if(correlationMat2D[i,j] > maxVal)
                    {
                        maxVal = correlationMat2D[i, j];
                    }
                    if (correlationMat2D[i, j] < minVal)
                    {
                        minVal = correlationMat2D[i, j];
                    }
                    sum+=correlationMat2D[i,j];
                    count++;
                }
            }

            res.Add("Max", Math.Round(maxVal,4));
            res.Add("Min", Math.Round(minVal,4));
            res.Add("Average", Math.Round(sum / count,4));

            return res;
        }

        /// <summary>
        /// extension of the TraceCorrelationTwoLabel to get Correlation data of 2 specified labels Lists
        /// </summary>
        /// <returns></returns>
        public Dictionary<(TIN, TIN), Dictionary<string, double>> TraceCorrelation(List<TIN> labels1, List<TIN> labels2)
        {
            Dictionary<(TIN, TIN), Dictionary<string, double>> correlationInfoAll = new Dictionary<(TIN, TIN), Dictionary<string, double>>();

            foreach (var label1 in labels1)
            {
                foreach (var label2 in labels2)
                {
                    var res = GetCorrelationStat(TraceCrossSimilarity(label1, label2));
                    correlationInfoAll.Add((label1, label2), res);
                }
            }
            return correlationInfoAll;
        }

        /// <summary>
        /// extension of the TraceCorrelationTwoLabel to get all Correlation data
        /// </summary>
        /// <returns></returns>
        public Dictionary<(TIN, TIN), Dictionary<string, double>> TraceCorrelation()
        {
            return TraceCorrelation(m_AllInputs.Keys.ToList<TIN>(), m_AllInputs.Keys.ToList<TIN>());
        }

        /// <summary>
        /// extension of the TraceCorrelationTwoLabel to get auto Correlation data of one list of labels
        /// </summary>
        /// <returns></returns>
        public Dictionary<(TIN, TIN), Dictionary<string, double>> TraceCorrelation(List<TIN> labels)
        {
            return TraceCorrelation(labels,labels);
        }

        /// <summary>
        /// output the correlation data matrix from TraceCorrelation of a List of labels with another label list to csv format 
        /// </summary>
        /// <returns></returns>
        public List<string> RenderCorrelationMatrixToCSVFormat(List<TIN> labels1, List<TIN> labels2)
        {
            var correlationInfoAll = TraceCorrelation(labels1, labels2);
            List<string> output = new List<string>();
            string header = " ";
            foreach (var key in labels2)
                header += $";{key.ToString()}";
            output.Add(header);

            var rows = new List<string>();

            List<string> entities = new List<string> { "Min", "Average", "Max" };

            foreach (var label1 in labels1)
            {
                List<string> rowForm = new List<string>();

                for (int i = 0; i < 3; i += 1)
                {
                    rowForm.Add("");
                    if (i == 1)
                    {
                        rowForm[i] += label1;
                    }
                    else
                    {
                        rowForm[i] += " ";
                    }
                    foreach (var label2 in labels2)
                    {
                        rowForm[i] += $";{entities[i]}: {correlationInfoAll[(label1, label2)][entities[i]]}";
                    }
                }
                rows.AddRange(rowForm);
            }
            output.AddRange(rows);
            return output;
        }

        /// <summary>
        /// output the correlation data matrix from TraceCorrelation of all label in m_AllInputs with each other to csv format 
        /// </summary>
        /// <returns></returns>
        public List<string> RenderCorrelationMatrixToCSVFormat()
        {
            return RenderCorrelationMatrixToCSVFormat(m_AllInputs.Keys.ToList<TIN>(), m_AllInputs.Keys.ToList<TIN>());
        }

        /// <summary>
        /// output the correlation data matrix from TraceCorrelation of a List of labels with itself to csv format 
        /// </summary>
        /// <returns></returns>
        public List<string> RenderCorrelationMatrixToCSVFormat(List<TIN> labels)
        {
            return RenderCorrelationMatrixToCSVFormat(labels,labels);
        }

        /// <summary>
        /// Print correlation table from 2 label lists
        /// </summary>
        /// <param name="labels1"></param>
        /// <param name="labels2"></param>
        public void TraceSimilarities(List<TIN> labels1, List<TIN> labels2)
        {
            var tableData = RenderCorrelationMatrixToCSVFormat(labels1, labels2);
            List<string[]> allEntries = new List<string[]>();
            int countToLine = 3; // for writing a line every 3 line of max min average
            int offsetIndex = 0; // this is for printting a line after the first header line
            int cellLength = 19;
            string dashLine = "";
            int lineLength = (cellLength+1) * tableData[0].Split(";").Length + 1;
            for (int i = 0; i < lineLength;i+=1)
            {
                dashLine += "-";
            }
            Debug.WriteLine(dashLine);
            foreach (var t in tableData)
            {
                var oneLine = t.Split(";");
                allEntries.Add(oneLine);
                Debug.Write("| ");
                foreach (var cell in oneLine)
                { 
                    Debug.Write(string.Format("{0," + -cellLength + "}|", cell));
                }
                Debug.Write("\n");
                if (offsetIndex % countToLine == 0)
                {
                    Debug.WriteLine(dashLine);
                }
                offsetIndex++;
            }
        }

        /// <summary>
        /// Print correlation table from 1 label list with itself
        /// </summary>
        public void TraceSimilarities(List<TIN> labels)
        {
            TraceSimilarities(labels, labels);
        }

        /// <summary>
        /// Print correlation table from all labels in m_AllInputs
        /// </summary>
        public void TraceSimilarities()
        {
            TraceSimilarities(m_AllInputs.Keys.ToList<TIN>(), m_AllInputs.Keys.ToList<TIN>());
        }
    }
}
