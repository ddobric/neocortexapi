// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Stores the calculus of a temporal cycle.
    /// </summary>
    public class SegmentActivity
    {
        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than threshold 
        /// <see cref="connectedPermanence"/>, which makes synapse connected.
        /// Dictionary[segment index, number of active synapses].
        /// </summary>
        public Dictionary<int, int> ActiveSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Dictionary, which holds the number of potential synapses of every segment.
        /// Potential synspses are all established synapses between receptor cell and the segment's cell. 
        /// Receprot cell was active cell in the previous cycle.
        /// Dictionary [segment index, number of potential synapses].
        /// </summary>
        public Dictionary<int, int> PotentialSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SegmentActivity()
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool Equals(SegmentActivity obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            if (ActiveSynapses.SequenceEqual(obj.ActiveSynapses) && PotentialSynapses.SequenceEqual(obj.PotentialSynapses))
                return true;
            else
                return false;
        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(SegmentActivity), writer);

            ser.SerializeValue(this.ActiveSynapses, writer);
            ser.SerializeValue(this.PotentialSynapses, writer);

            ser.SerializeEnd(nameof(SegmentActivity), writer);
        }

        public static SegmentActivity Deserialize(StreamReader sr)
        {
            SegmentActivity segment = new SegmentActivity();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == ser.LineDelimiter || data == ser.ReadBegin(nameof(SegmentActivity)) || data == ser.ReadEnd(nameof(SegmentActivity)))
                { }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {

                                    segment.ActiveSynapses = ser.ReadDictionaryIIValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    segment.PotentialSynapses = ser.ReadDictionaryIIValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return segment;
        }
        #endregion

    }
}
