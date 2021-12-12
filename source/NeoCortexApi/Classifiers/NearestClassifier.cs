// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.Classifiers
{
    public class NearestClassifier<TIN, TOUT> : IHtmModule<ComputeCycle, object>
    {
        private Dictionary<TIN, int[]> activeMap = new Dictionary<TIN, int[]>();

        //private int radius = 3;

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Learn(TIN input, Cell[] output, bool learn)
        {
            if (!activeMap.ContainsKey(input))
            {
                activeMap.Add(input, GetCellIndicies(output));
            }
            else
            {
                var indicies = GetCellIndicies(output);

            }
        }

        public TOUT Inference(Cell[] predictiveCells)
        {
            throw new NotImplementedException();
        }



        ///// <summary>
        ///// Gets predicted value for next cycle
        ///// </summary>
        ///// <param name="output"></param>
        ///// <returns></returns>
        //public String GetPredictedInputValue(Cell[] output)
        //{
        //    int maxSameBits = 0;
        //    string charOutput = null;
        //    int[] arr = new int[output.Length];
        //    for (int i = 0; i < output.Length; i++)
        //    {
        //        arr[i] = output[i].Index;
        //    }
        //    if (output.Length != 0)
        //    {
        //        foreach (TIN inputVal in activeArray.Keys)
        //        {
        //            int numOfSameBits = predictNextValue(arr, activeArray[inputVal]);
        //            if (numOfSameBits > maxSameBits)
        //            {
        //                maxSameBits = numOfSameBits;
        //                charOutput = inputVal as String;
        //            }
        //        }
        //        return charOutput;
        //        //return activeMap[ComputeHash(FlatArray(output))];
        //    }
        //    return null;
        //}




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


        public object Compute(ComputeCycle cycle, bool learn)
        {
            //if (!activeMap.ContainsKey(input))
            //{
            //    this.activeMap.Add(input, GetCellIndicies(output));
            //}
            //else
            //{
            //    var indicies = GetCellIndicies(output);
            //}

            return null;
        }

    }
}
