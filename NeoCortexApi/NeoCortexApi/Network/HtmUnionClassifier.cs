using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi.Network
{
    public class HtmUnionClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private Dictionary<TIN, int[]> activeMap = new Dictionary<TIN, int[]>();

        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            int result = 0;
            dynamic charOutput = null;
            int[] arr = new int[predictiveCells.Length];
            for (int i = 0; i < predictiveCells.Length; i++)
            {
                arr[i] = predictiveCells[i].Index;
            }
            foreach (var key in activeMap.Keys)
            {
                if (result < predictNextValue(arr, activeMap[key]))
                {
                    result = predictNextValue(arr, activeMap[key]);
                    charOutput = key as String;
                }
            }
            return (TIN)charOutput;
        }

        /*
private Dictionary<TIN, int[]> activeMap = new Dictionary<TIN, int[]>();

public void Learn(TIN input, Cell[] activeCells, bool learn)
{
   if (learn == true)
   {
       int[] unionArray;
       int[] cellAsInt = new int[activeCells.Length];
       for (int i = 0; i < activeCells.Length; i++)
       {
           cellAsInt[i] = activeCells[i].Index;
       }
       if (!activeMap.TryGetValue(input, out unionArray))
       {
           this.activeMap.Add(input, cellAsInt);
           return; // or whatever you want to do
       }
       else
       {
           this.activeMap[input] = GetUnionArr(cellAsInt, activeMap[input]);
       }
   }
}

public String Inference(Cell[] activeCells)
{
   int result = 0;
   String charOutput = null;
   int[] arr = new int[activeCells.Length];
   for (int i = 0; i < activeCells.Length; i++)
   {
       arr[i] = activeCells[i].Index;
   }
   foreach (var key in activeMap.Keys)
   {
       if (result < predictNextValue(arr, activeMap[key]))
       {
           result = predictNextValue(arr, activeMap[key]);
           charOutput = key as String;
       }
   }
   return charOutput;
}
*/
        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            if (learn == true)
            {
                int[] unionArray;
                int[] cellAsInt = new int[activeCells.Length];
                for (int i = 0; i < activeCells.Length; i++)
                {
                    cellAsInt[i] = activeCells[i].Index;
                }
                if (!activeMap.TryGetValue(input, out unionArray))
                {
                    this.activeMap.Add(input, cellAsInt);
                    return; // or whatever you want to do
                }
                else
                {
                    this.activeMap[input] = GetUnionArr(cellAsInt, activeMap[input]);
                }
            }
        }

        private int[] GetUnionArr(int[] prevCells, int[] currCells)
        {
            return prevCells.Union(currCells).ToArray<int>();
        }

        private int predictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }

     
    }
}
