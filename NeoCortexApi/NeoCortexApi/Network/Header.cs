using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi
{
    public class Header
    {
        private List<List<object>> rawTupleList;

        /** Name of each field */
        private List<String> fieldNames;

        /** Field data types */
        private List<FieldMetaType> fieldMeta;

        /** Processing flags and hints */
        private List<string> sensorFlags;

        private bool isChanged;

        private bool isLearn = true;

        private int[] resetIndexes;

        private int[] seqIndexes;

        private int[] tsIndexes;

        private int[] learnIndexes;

        private int[] categoryIndexes;

        private List<String> sequenceCache;

        public int Size { get { return rawTupleList.Count; } }

        public List<String> FieldNames { get { return fieldNames; } }

        public List<FieldMetaType> FieldTypes { get { return fieldMeta; } }

        /**
    * Returns the header line ({@link List}) containing the
    * control flags (in the 3rd line) which designate control
    * operations such as turning learning on/off and resetting
    * the state of an algorithm.
    * 
    * @return
    */
        public List<string> Flags { get { return sensorFlags; } }

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


        //public Header(List<List<object>> input)
        public Header(List<string> fieldNames, List<FieldMetaType> fieldMeta, List<string> sensorFlags)
        {
            this.fieldNames = fieldNames;
            this.fieldMeta = fieldMeta;
            this.sensorFlags = sensorFlags;
            //if (input.Count != 3)
            //{
            //    throw new ArgumentException("Input did not have 3 rows");
            //}
            //this.rawTupleList = input;
            //this.fieldNames = input[0].Cast<string>().ToList();
            //this.fieldMeta = input[1].Cast<FieldMetaType>().ToList();
            //this.sensorFlags = input[2].Cast<string>().ToList();

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

            foreach (string sf in sensorFlags)
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
