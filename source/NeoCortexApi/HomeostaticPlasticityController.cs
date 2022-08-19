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

namespace NeoCortexApi
{
    /// <summary>
    /// Implements the new-born effect in the SP. This effect tracks the learning process of the SP 
    /// and switches-off the boosting mechanism (new-born effect) after the SP has entered a stable state 
    /// for all seen input patterns.
    /// </summary>
    /// <remarks>
    /// The research related to this component can be found here:
    /// https://www.researchgate.net/publication/358996456_On_the_Importance_of_the_Newborn_Stage_When_Learning_Patterns_with_the_Spatial_Pooler
    /// Published 2022 in Springer Nature Computer Sciences.
    /// </remarks>
    public class HomeostaticPlasticityController : ISerializable
    {
        private double m_RequiredSimilarityThreshold;

        private int m_MaxPreviousElements = 5;

        private Connections m_HtmMemory;

        private int m_Cycle = 0;

        private int m_MinCycles;

        /// <summary>
        /// Number of minimal required stable cycles for every input.
        /// When this value is reached, the new-born effect will take a place.
        /// It means, the boosting will be disabled.
        /// </summary>
        private int m_RequiredNumOfStableCycles;

        /// <summary>
        /// Number stable cycles for every input.
        /// </summary>
        private Dictionary<string, int> m_NumOfStableCyclesForInput = new Dictionary<string, int>();

        /// <summary>
        /// Number of active columns in SDRs of last N steps.
        /// </summary>
        private Dictionary<string, int[]> m_NumOfActiveColsForInput = new Dictionary<string, int[]>();

        /// <summary>
        /// Keeps the list of hash values of all seen input patterns.
        /// List of hashes. [key, val] = [hash(input), hash(output)]
        /// </summary>
        private Dictionary<string, int[]> m_InOutMap = new Dictionary<string, int[]>();

        /// <summary>
        /// Action to be invoked when the SP become stable or instable.
        /// <is stable, num of seen patterns, derivation of active column function, cycle>
        /// </summary>
        private Action<bool, int, double, int> m_OnStabilityStatusChanged;

        public Action<bool, int, double, int> OnStabilityStatusChanged { get => m_OnStabilityStatusChanged; set => m_OnStabilityStatusChanged = value; }
        /// <summary>
        /// Set on true when SP deactivates boosting and enter the stable state.
        /// Once SP enters the stable state and it becomes instable again, this value is set on false.
        /// </summary>
        private bool m_IsStable = false;

        /// <summary>
        /// Creates the instance of the HPC to stabilize the Spatial Pooler learning process.
        /// </summary>
        /// <param name="htmMemory">The initialized HTM memory.</param>
        /// <param name="minCycles">The minimume required cycles to keep boosting. This parameter defiens the new-born stage of the SP.
        /// During this period, the SP will boost columns and bee instable. After this period the HPC will switch off boosting.</param>
        /// <param name="onStabilityStatusChanged">Invoked when the SP changes the state from instable to stable and vise versa.</param>
        /// <param name="numOfCyclesToWaitOnChange">How many cycles SDRs of all input patterns must be unchanged to declare the SP as stable.</param>
        /// <summary>
        /// Used during the deserialization proicess.
        /// </summary>
        public HomeostaticPlasticityController()
        {

        }

        /// <summary>
        /// Creates the instance of HomeostaticPlasticityController.
        /// </summary>
        /// <param name="htmMemory">The HTM memory.</param>
        /// <param name="minCycles">The minimum calls to the Learn method until HPC algorithm is activated. When this number is reached the HPC will disable boosting in SP. </param>
        /// <param name="onStabilityStatusChanged">Action invoked when the SP status is changed from stable t unstable and vise versa.</param>
        /// <param name="numOfCyclesToWaitOnChange">How many cycles all seen patterns must not change to declare SP as stable. Using smaller numbers might cause frequent status change.
        /// Higher numbers ensure more stable SP, but it takes longer time to enter the stable stabe.</param>
        /// <param name="requiredSimilarityThreshold">The similarity between last and current SDR of the single pattern that must be reached to declare the SRR
        /// these two SDRs same.</param>
        public HomeostaticPlasticityController(Connections htmMemory, int minCycles, Action<bool, int, double, int> onStabilityStatusChanged, int numOfCyclesToWaitOnChange = 50, double requiredSimilarityThreshold = 0.97)
        {
            this.m_OnStabilityStatusChanged = onStabilityStatusChanged;
            this.m_HtmMemory = htmMemory;
            this.m_MinCycles = minCycles;
            this.m_RequiredNumOfStableCycles = numOfCyclesToWaitOnChange;
            this.m_RequiredSimilarityThreshold = requiredSimilarityThreshold;
        }

        /// <summary>
        /// Invoked as the last step in learning of the SP.
        /// </summary>
        /// <param name="input">The input of the SP in the current cycle.</param>
        /// <param name="output">The output SDR of the Spatial Pooler compute cycle.</param>
        /// <returns>True if the PS has enetered the stable state.</returns>
        public bool Compute(int[] input, int[] output)
        {
            bool res = false;

            double avgDerivation = -1;

            // We take the hash value of the input.
            var inpHash = GetHash(input);

            //
            // Here we track the number of active columns for every cycle for every input.
            // We want that this number for every input is approximately the same.
            if (m_NumOfActiveColsForInput.ContainsKey(inpHash))
                ArrayUtils.PushToInterval(m_NumOfActiveColsForInput[inpHash], m_MaxPreviousElements, output.Count(c => c == 1));

            //
            // If the pattern appears for the first time, add it to dictionary of seen patterns.
            if (!m_InOutMap.ContainsKey(inpHash))
            {
                m_InOutMap.Add(inpHash, output);
                m_NumOfActiveColsForInput.Add(inpHash, new int[m_MaxPreviousElements]);
                m_NumOfStableCyclesForInput.Add(inpHash, 0);
            }
            else
            {
                if (m_Cycle >= this.m_MinCycles)
                {
                    this.m_HtmMemory.HtmConfig.MaxBoost = 0.0;

                    this.m_HtmMemory.HtmConfig.MinPctOverlapDutyCycles = 0.0;

                    this.m_HtmMemory.HtmConfig.MinPctActiveDutyCycles = 0.0;
                }

                // If the input has been already seen, we calculate the similarity between already seen input
                // and the new input. The similarity is calculated as a correlation function.
                var similarity = CalcArraySimilarity(ArrayUtils.IndexWhere(m_InOutMap[inpHash], k => k == 1), ArrayUtils.IndexWhere(output, k => k == 1));

                // We replace the existing value with the new one.
                m_InOutMap[inpHash] = output;

                //
                // We cannot expect the 100% for the entire learning cycle. Sometimes some
                // SDR appear with few more or less bits than in the previous cycle.
                // If this happen we take the new SDR (output) as the winner and put it in the map.
                if (similarity >= m_RequiredSimilarityThreshold)
                {
                    // We calculate here the average change of the SDR for the given input.
                    avgDerivation = ArrayUtils.AvgDelta(m_NumOfActiveColsForInput[inpHash]);

                    //
                    // If there is no change (SDR is stable) we count nuber of stable cycles.
                    // If the average value is not 0, then we reset the number of stable cycles.
                    if (avgDerivation == 0)
                        m_NumOfStableCyclesForInput[inpHash] = m_NumOfStableCyclesForInput[inpHash] + 1;
                    else
                        m_NumOfStableCyclesForInput[inpHash] = 0;

                    if (m_Cycle >= this.m_MinCycles)
                    {
                        if (m_NumOfStableCyclesForInput[inpHash] > m_RequiredNumOfStableCycles && IsInStableState(m_NumOfStableCyclesForInput, m_RequiredNumOfStableCycles))
                        {
                            // We fire event when changed from instable to stable.
                            if (!m_IsStable)
                                this.m_OnStabilityStatusChanged(true, m_InOutMap.Keys.Count, avgDerivation, m_Cycle);

                            m_IsStable = true;
                            res = true;
                        }
                    }
                }
                else
                {
                    m_NumOfStableCyclesForInput[inpHash] = 0;

                    // If the new SDR output for the already seen input

                    if (m_IsStable)
                    {
                        // THIS SHOULD NEVER HAPPEN! MEANS FROM STABLE TO INSTABLE!
                        m_IsStable = false;
                        this.m_OnStabilityStatusChanged(false, m_InOutMap.Keys.Count, avgDerivation, m_Cycle);
                    }
                }
            }

            this.m_Cycle++;

            return res;
        }

        private static bool IsInStableState(Dictionary<string, int> stableCyclesForAllInputs, int requiredNumOfStableCycles)
        {
            bool res = true;

            foreach (var stableCycles in stableCyclesForAllInputs.Values)
            {
                if (stableCycles < requiredNumOfStableCycles)
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Calculates the correlation of bits in ywo arrays.
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>Similarity of two arrays.</returns>
        public static double CalcArraySimilarityOld2(int[] data1, int[] data2)
        {
            double min = Math.Min(data1.Length, data2.Length);
            double max = Math.Max(data1.Length, data2.Length);

            double sum = 0;

            for (int i = 0; i < min; i++)
            {
                if (data1[i] == data2[i])
                    sum++;
            }

            return sum / max;
        }

        public void SetConnections(Connections connections)
        {
            this.m_HtmMemory = connections;
        }

        /// <summary>
        /// Calculates how many elements of the array are same in percents. This method is useful to compare 
        /// two arays that contains indicies of active columns.
        /// </summary>
        /// <param name="originArray">Indexes of non-zero bits in the SDR.</param>
        /// <param name="comparingArray">Indexes of non-zero bits in the SDR.</param>
        /// <returns>Similarity between arrays 0.0-1.0</returns>
        public static double CalcArraySimilarity(int[] originArray, int[] comparingArray)
        {
            if (originArray.Length > 0 && comparingArray.Length > 0)
            {
                int cnt = 0;

                foreach (var item in comparingArray)
                {
                    if (originArray.Contains(item))
                        cnt++;
                }

                return ((double)cnt / (double)Math.Max(originArray.Length, comparingArray.Length));
            }
            else
            {
                return -1.0;
            }
        }

        /// <summary>
        /// Compute the hash from the array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string GetHash(int[] input)
        {
            List<byte> buff = new List<byte>();

            foreach (var item in input)
            {
                buff.AddRange(BitConverter.GetBytes(item));
            }

            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(buff.ToArray());

                return Encoding.UTF8.GetString(data);
            }
        }

        //public static HomeostaticPlasticityController Deserialize(StreamReader sr, Connections htmMemory = null)
        //{
        //    HomeostaticPlasticityController ctrl = new HomeostaticPlasticityController();
        //    ctrl.m_HtmMemory = htmMemory;

        //    HtmSerializer2 ser = new HtmSerializer2();

        //    while (sr.Peek() >= 0)
        //    {
        //        string data = sr.ReadLine();
        //        if (data == String.Empty || data == ser.ReadBegin(nameof(HomeostaticPlasticityController)))
        //        {
        //            continue;
        //        }
        //        else if (data == ser.ReadBegin(nameof(Connections)))
        //        {
        //            ctrl.m_HtmMemory = Connections.Deserialize(sr);
        //        }
        //        else if (data == ser.ReadEnd(nameof(HomeostaticPlasticityController)))
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
        //            for (int i = 0; i < str.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case 0:
        //                        {
        //                            ctrl.m_RequiredSimilarityThreshold = ser.ReadDoubleValue(str[i]);
        //                            break;
        //                        }
        //                    case 1:
        //                        {
        //                            ctrl.m_MaxPreviousElements = ser.ReadIntValue(str[i]);
        //                            break;
        //                        }
        //                    case 2:
        //                        {
        //                            ctrl.m_Cycle = ser.ReadIntValue(str[i]);
        //                            break;
        //                        }
        //                    case 3:
        //                        {
        //                            ctrl.m_MinCycles = ser.ReadIntValue(str[i]);
        //                            break;
        //                        }
        //                    case 4:
        //                        {
        //                            ctrl.m_RequiredNumOfStableCycles = ser.ReadIntValue(str[i]);
        //                            break;
        //                        }
        //                    case 5:
        //                        {
        //                            ctrl.m_NumOfStableCyclesForInput = ser.ReadDictSIValue(str[i]);
        //                            break;
        //                        }
        //                    case 6:
        //                        {
        //                            ctrl.m_NumOfActiveColsForInput = ser.ReadDictSIarray(str[i]);
        //                            break;
        //                        }
        //                    case 7:
        //                        {
        //                            ctrl.m_InOutMap = ser.ReadDictSIarray(str[i]);
        //                            break;
        //                        }

        //                    case 8:
        //                        {
        //                            ctrl.m_IsStable = ser.ReadBoolValue(str[i]);
        //                            break;
        //                        }
        //                    default:
        //                        { break; }

        //                }
        //            }
        //        }
        //    }

        //    return ctrl;

        //}

        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState(string fileName = null)
        {
            if (fileName == null)
                fileName = $"{nameof(HomeostaticPlasticityController)}.state.csv";

            Debug.WriteLine("........... Column State .............");

            int cnt = 0;

            using (var cellStateSw = new StreamWriter(fileName))
            {
                foreach (var item in m_InOutMap)
                {
                    //string keyStr = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Key));
                    //string res = $"{cnt++}- stable cycles: {this.m_NumOfStableCyclesForInput.Count}";

                    var sdr = Helpers.StringifyVector(ArrayUtils.IndexWhere(m_InOutMap[item.Key], k => k == 1));

                    string str = $"[{cnt++} - stable cycles: {this.m_NumOfStableCyclesForInput[item.Key]},len = {m_InOutMap[item.Key].Count(l => l == 1)}] \t {sdr}";

                    //Debug.WriteLine(keyStr);
                    //Debug.WriteLine($"{res}");
                    Debug.WriteLine(str);

                    //cellStateSw.WriteLine($"{res} \t {keyStr}");
                    cellStateSw.WriteLine(str);
                }

                var min = int.MaxValue;
                var minKey = String.Empty;

                foreach (var item in this.m_NumOfStableCyclesForInput)
                {
                    if (item.Value < min)
                    {
                        min = item.Value;
                        minKey = item.Key;
                    }
                }

                Debug.WriteLine($"MinKey={minKey}, min stable states={min}");
                cellStateSw.WriteLine($"MinKey={minKey}, min stable states={min}");
            }
        }

        public bool Equals(HomeostaticPlasticityController obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (m_HtmMemory == null)
            {
                if (obj.m_HtmMemory != null)
                    return false;
            }
            else if (!m_HtmMemory.Equals(obj.m_HtmMemory))
                return false;
            if (m_RequiredSimilarityThreshold != obj.m_RequiredSimilarityThreshold)
                return false;
            else if (m_MaxPreviousElements != obj.m_MaxPreviousElements)
                return false;
            else if (m_Cycle != obj.m_Cycle)
                return false;
            else if (m_MinCycles != obj.m_MinCycles)
                return false;
            else if (m_RequiredNumOfStableCycles != obj.m_RequiredNumOfStableCycles)
                return false;
            else if (!m_NumOfStableCyclesForInput.SequenceEqual(obj.m_NumOfStableCyclesForInput) && !m_NumOfActiveColsForInput.SequenceEqual(m_NumOfActiveColsForInput) && !m_InOutMap.SequenceEqual(m_InOutMap))
                return false;
            else if (m_IsStable != obj.m_IsStable)
                return false;

            return true;

        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(HomeostaticPlasticityController), writer);

            ser.SerializeValue(this.m_RequiredSimilarityThreshold, writer);
            ser.SerializeValue(this.m_MaxPreviousElements, writer);
            ser.SerializeValue(this.m_Cycle, writer);
            ser.SerializeValue(this.m_MinCycles, writer);
            ser.SerializeValue(this.m_RequiredNumOfStableCycles, writer);
            ser.SerializeValue(this.m_NumOfStableCyclesForInput, writer);
            ser.SerializeValue(this.m_NumOfActiveColsForInput, writer);
            ser.SerializeValue(this.m_InOutMap, writer);
            ser.SerializeValue(this.m_IsStable, writer);

            if (this.m_HtmMemory != null)
            {
                this.m_HtmMemory.Serialize(writer);
            }

            ser.SerializeEnd(nameof(HomeostaticPlasticityController), writer);

        }

        public static HomeostaticPlasticityController Deserialize(StreamReader sr)
        {
            HomeostaticPlasticityController ctrl = new HomeostaticPlasticityController();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(HomeostaticPlasticityController)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(Connections)))
                {
                    ctrl.m_HtmMemory = Connections.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(HomeostaticPlasticityController)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    ctrl.m_RequiredSimilarityThreshold = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    ctrl.m_MaxPreviousElements = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    ctrl.m_Cycle = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    ctrl.m_MinCycles = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 4:
                                {
                                    ctrl.m_RequiredNumOfStableCycles = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 5:
                                {
                                    ctrl.m_NumOfStableCyclesForInput = ser.ReadDictSIValue(str[i]);
                                    break;
                                }
                            case 6:
                                {
                                    ctrl.m_NumOfActiveColsForInput = ser.ReadDictSIarray(str[i]);
                                    break;
                                }
                            case 7:
                                {
                                    ctrl.m_InOutMap = ser.ReadDictSIarray(str[i]);
                                    break;
                                }

                            case 8:
                                {
                                    ctrl.m_IsStable = ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return ctrl;

        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var excludeEntries = new List<string>
            {
                nameof(m_OnStabilityStatusChanged),
                nameof(OnStabilityStatusChanged),
                nameof(m_HtmMemory),
                nameof(m_InOutMap)
            };

            if (obj is HomeostaticPlasticityController controller)
            {
                HtmSerializer2.SerializeObject(obj, name, sw, excludeEntries);
                var convertInOutMap = controller.m_InOutMap.ToDictionary(kv => kv.Key, kv => new KeyValuePair<int, int[]>(kv.Value.Length, ArrayUtils.IndexesWithNonZeros(kv.Value)));
                HtmSerializer2.Serialize(convertInOutMap, "convertInOutMap", sw);
            }


        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            var excludeEntries = new List<string> { "convertInOutMap" };

            var controller = HtmSerializer2.DeserializeObject<T>(sr, name, excludeEntries, (obj, propName) =>
            {
                if (obj is HomeostaticPlasticityController hpc)
                {
                    if (propName == "convertInOutMap")
                    {
                        var convertInOutMap = HtmSerializer2.Deserialize<Dictionary<string, KeyValuePair<int, int[]>>>(sr, propName);

                        Dictionary<string, int[]> inOutMap = new Dictionary<string, int[]>();
                        foreach (var map in convertInOutMap)
                        {
                            var array = new int[map.Value.Key];
                            ArrayUtils.FillArray(array, 0);
                            ArrayUtils.SetIndexesTo(array, map.Value.Value, 1);
                            inOutMap[map.Key] = array;
                        }
                        hpc.m_InOutMap = inOutMap;
                    }
                }
            });

            return controller;
        }
        #endregion
    }
}
