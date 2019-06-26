using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.Network;

namespace NeoCortexApi
{
    public class Header
    {
        private HeaderMetaData metadata;

        private List<List<object>> rawTupleList;

    

        private bool isChanged;

        private bool isLearn = true;

        private int[] resetIndexes;

        private int[] seqIndexes;

        private int[] tsIndexes;

        private int[] learnIndexes;

        private int[] categoryIndexes;

        private List<String> sequenceCache;

        public int Size { get { return rawTupleList.Count; } }


     
        /**
         * Returns a flag indicating whether any watched column
         * has changed data.
         * 
         * @return
         */
        public bool IsReset { get { return isChanged; } }


        /**
         * Returns a flag indicating whether the current input state
         * is set to learn or not.
         * 
         * @return
         */
        public bool IsLearn { get { return IsLearn; } }

        public HeaderMetaData Metadata { get => metadata; set => metadata = value; }

        public Header(HeaderMetaData metadata)
        {
            this.Metadata = metadata;

            initIndexes();
        }



        /**
   * Initializes the indexes of {@link SensorFlags} types to aid
   * in line processing.
   */
        private void initIndexes()
        {
            int idx = 0;
            List<int> tList = new List<int>();
            List<int> rList = new List<int>();
            List<int> cList = new List<int>();
            List<int> sList = new List<int>();
            List<int> lList = new List<int>();

            foreach (string sf in this.metadata.SensorFlags)
            {
                switch (sf)
                {
                    case SensorFlags.T: tList.Add(idx); break;
                    case SensorFlags.R: rList.Add(idx); break;
                    case SensorFlags.C: cList.Add(idx); break;
                    case SensorFlags.S: sList.Add(idx); break;
                    case SensorFlags.L: lList.Add(idx); break;
                    case SensorFlags.B: lList.Add(idx); break;
                }
                idx++;
            }

            // Add + 1 to each to offset for Sensor insertion of sequence number in all row headers.
            resetIndexes = rList.Select(i => i + 1).ToArray();
            seqIndexes = sList.Select(i => i + 1).ToArray();
            categoryIndexes = cList.Select(i => i + 1).ToArray(); ;
            tsIndexes = tList.Select(i => i + 1).ToArray();
            learnIndexes = lList.Select(i => i + 1).ToArray();

            if (seqIndexes.Length > 0)
            {
                sequenceCache = new List<string>();
            }
        }

        /**
    * Processes the current line of input and sets flags based on 
    * sensor flags formed by the 3rd line of a given header.
    * 
    * @param input
    */
        void process(String[] input)
        {
            isChanged = false;

            if (resetIndexes.Length > 0)
            {
                foreach (int i in resetIndexes)
                {
                    if (int.Parse(input[i].Trim()) == 1)
                    {
                        isChanged = true; break;
                    }
                    else
                    {
                        isChanged = false;
                    }
                }
            }

            if (learnIndexes.Length > 0)
            {
                foreach (int i in learnIndexes)
                {
                    if (int.Parse(input[i].Trim()) == 1)
                    {
                        isLearn = true; break;
                    }
                    else
                    {
                        isLearn = false;
                    }
                }
            }

            // Store lines in cache to detect when current input is a change.
            if (seqIndexes.Length > 0)
            {
                bool sequenceChanged = false;
                if (sequenceCache == null || sequenceCache.Count == 0)
                {
                    foreach (int i in seqIndexes)
                    {
                        sequenceCache.Add(input[i]);
                    }
                }
                else
                {
                    int idx = 0;
                    foreach (int i in seqIndexes)
                    {
                        if (!sequenceCache[idx].SequenceEqual(input[i]))
                        {
                            sequenceCache[idx] = input[i];
                            sequenceChanged = true;
                        }
                    }
                }
                isChanged |= sequenceChanged;
            }
        }
    }
}
