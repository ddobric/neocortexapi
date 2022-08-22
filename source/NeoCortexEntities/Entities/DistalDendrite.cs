// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Implements a distal dendritic segment that is used for learning sequences.
    /// Segments are owned by <see cref="Cell"/>s and in turn own <see cref="Synapse"/>s which are obversely connected to by a "source cell", 
    /// which is the <see cref="Cell"/> which will activate a given <see cref="Synapse"/> owned by this <see cref="Segment"/>.
    /// </summary>
    public class DistalDendrite : Segment, IComparable<DistalDendrite>, IEquatable<DistalDendrite>, ISerializable
    {
        /// <summary>
        /// The cell that owns (parent) the segment.
        /// </summary>        
        public Cell ParentCell;

        private long m_LastUsedIteration;

        private int m_Ordinal = -1;

        /// <summary>
        /// the last iteration in which this segment was active.
        /// </summary>
        public long LastUsedIteration { get => m_LastUsedIteration; set => m_LastUsedIteration = value; }

        /// <summary>
        /// The seqence number of the segment. Specifies the order of the segment of the <see cref="Connections"/> instance.
        /// </summary>
        public int Ordinal { get => m_Ordinal; set => m_Ordinal = value; }

        /// <summary>
        /// Default constructor used by deserializer.
        /// </summary>
        public DistalDendrite()
        {

        }


        /// <summary>
        /// Creates the Distal Segment.
        /// </summary>
        /// <param name="parentCell">The cell, which owns the segment.</param>
        /// <param name="flatIdx">The flat index of the segment. If some segments are destroyed (synapses lost permanence)
        /// then the new segment will reuse the flat index. In contrast, 
        /// the ordinal number will increas when new segments are created.</param>
        /// <param name="lastUsedIteration"></param>
        /// <param name="ordinal">The ordindal number of the segment. This number is incremented on each new segment.
        /// If some segments are destroyed, this number is still incrementd.</param>
        /// <param name="synapsePermConnected"></param>
        /// <param name="numInputs"></param>
        public DistalDendrite(Cell parentCell, int flatIdx, long lastUsedIteration, int ordinal, double synapsePermConnected, int numInputs) : base(flatIdx, synapsePermConnected, numInputs)
        {
            this.ParentCell = parentCell;
            this.m_Ordinal = ordinal;
            this.m_LastUsedIteration = lastUsedIteration;
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"DistalDendrite: Indx:{this.SegmentIndex}";
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((ParentCell == null) ? 0 : ParentCell.GetHashCode());
            result *= this.SegmentIndex;
            return result;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DistalDendrite distalDendrite))
                return false;
            return this.Equals(distalDendrite);
        }

        /// <summary>
        /// Compares this segment with the given one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public bool Equals(DistalDendrite obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            DistalDendrite other = (DistalDendrite)obj;
            if (ParentCell == null)
            {
                if (other.ParentCell != null)
                    return false;
            }
            // We check here the cell id only! The cell as parent must be correctlly created to avoid having different cells with the same id.
            // If we would use here ParenCell.Equals method, that method would cause a cicular invoke of this.Equals etc.
            else if (ParentCell.Index != other.ParentCell.Index)
                return false;
            if (m_LastUsedIteration != other.m_LastUsedIteration)
                return false;
            if (m_Ordinal != other.m_Ordinal)
                return false;
            if (LastUsedIteration != other.LastUsedIteration)
                return false;
            if (Ordinal != other.Ordinal)
                return false;
            if (SegmentIndex != obj.SegmentIndex)
                return false;
            if (Synapses == null)
            {
                if (obj.Synapses != null)
                    return false;
            }
            else if (!Synapses.ElementsEqual(obj.Synapses))
                return false;
            //if (boxedIndex == null)
            //{
            //    if (obj.boxedIndex != null)
            //        return false;
            //}
            //else if (!boxedIndex.Equals(obj.boxedIndex))
            //    return false;
            if (SynapsePermConnected != obj.SynapsePermConnected)
                return false;
            if (NumInputs != obj.NumInputs)
                return false;

            return true;
        }


        /// <summary>
        /// Compares by index.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DistalDendrite other)
        {
            if (this.SegmentIndex > other.SegmentIndex)
                return 1;
            else if (this.SegmentIndex < other.SegmentIndex)
                return -1;
            else
                return 0;
        }

        #region Serialization

        /// <summary>
        /// Only cell Serialize method should invoke this method!
        /// </summary>
        /// <param name="writer"></param>
        internal void SerializeT(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(DistalDendrite), writer);

            ser.SerializeValue(this.m_LastUsedIteration, writer);
            ser.SerializeValue(this.m_Ordinal, writer);
            ser.SerializeValue(this.LastUsedIteration, writer);
            ser.SerializeValue(this.Ordinal, writer);
            ser.SerializeValue(this.SegmentIndex, writer);
            ser.SerializeValue(this.SynapsePermConnected, writer);
            ser.SerializeValue(this.NumInputs, writer);

            //if (this.boxedIndex != null)
            //{
            //    this.boxedIndex.Serialize(writer);
            //}

            // If we use this, we will get a cirular serialization.
            //if (this.ParentCell != null)
            //{
            //    this.ParentCell.SerializeT(writer);
            //}

            if (this.Synapses != null && this.Synapses.Count > 0)
                ser.SerializeValue(this.Synapses, writer);

            ser.SerializeEnd(nameof(DistalDendrite), writer);
        }

        /// <summary>
        /// Serialize method for DistalDendrite
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(DistalDendrite), writer);

            ser.SerializeValue(this.m_LastUsedIteration, writer);
            ser.SerializeValue(this.m_Ordinal, writer);
            ser.SerializeValue(this.LastUsedIteration, writer);
            ser.SerializeValue(this.Ordinal, writer);
            ser.SerializeValue(this.SegmentIndex, writer);
            ser.SerializeValue(this.SynapsePermConnected, writer);
            ser.SerializeValue(this.NumInputs, writer);


            //if (this.boxedIndex != null)
            //{
            //    this.boxedIndex.Serialize(writer);
            //}

            if (this.ParentCell != null)
            {
                this.ParentCell.SerializeT(writer);
            }

            if (this.Synapses != null && this.Synapses.Count > 0)
                ser.SerializeValue(this.Synapses, writer);

            ser.SerializeEnd(nameof(DistalDendrite), writer);
        }

        public static DistalDendrite Deserialize(StreamReader sr)
        {
            DistalDendrite distal = new DistalDendrite();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(DistalDendrite)) || data.ToCharArray()[0] == HtmSerializer2.ElementsDelimiter || (data.ToCharArray()[0] == HtmSerializer2.ElementsDelimiter && data.ToCharArray()[1] == HtmSerializer2.ParameterDelimiter))
                {
                    continue;
                }
                //else if (data == ser.ReadBegin(nameof(Integer)))
                //{
                //    distal.boxedIndex = Integer.Deserialize(sr);
                //}
                else if (data == ser.ReadBegin(nameof(Synapse)))
                {
                    distal.Synapses.Add(Synapse.Deserialize(sr));
                }
                else if (data == ser.ReadBegin(nameof(Cell)))
                {
                    distal.ParentCell = Cell.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(DistalDendrite)))
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
                                    distal.m_LastUsedIteration = ser.ReadLongValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    distal.m_Ordinal = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    distal.LastUsedIteration = ser.ReadLongValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    distal.Ordinal = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 4:
                                {
                                    distal.SegmentIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 5:
                                {
                                    distal.SynapsePermConnected = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 6:
                                {
                                    distal.NumInputs = ser.ReadIntValue(str[i]);
                                    break;
                                }

                            default:
                                { break; }

                        }
                    }
                }
            }

            return distal;

        }

        #endregion

        public static void Serialize1(StreamWriter sw, object obj, string propName)
        {
            HtmSerializer2.SerializeObject(obj, propName, sw, new List<string>{nameof(DistalDendrite.ParentCell)});
        }

        public static DistalDendrite Deserialize1(StreamReader sr, string propName)
        {

            var result = HtmSerializer2.DeserializeObject<DistalDendrite>(sr, propName);
            return result;
        }

        private static List<int> isCellsSerialized = new List<int>();

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string> 
            { 
                //nameof(DistalDendrite.ParentCell),
                nameof(Segment.Synapses)
            };
            HtmSerializer2.SerializeObject(obj, name, sw, ignoreMembers);
            //var synapses = this.Synapses.Select(s => new Synapse() { SynapseIndex = s.SynapseIndex });
            //HtmSerializer2.Serialize(synapses, nameof(Segment.Synapses), sw);

            //var cell = (obj as DistalDendrite).ParentCell;

            //if (cell != null && isCellsSerialized.Contains(cell.HashCode()) == false)
            //{
            //    isCellsSerialized.Add(cell.HashCode());
            //    HtmSerializer2.Serialize((obj as DistalDendrite).ParentCell, nameof(DistalDendrite.ParentCell), sw, ignoreMembers: ignoreMembers);
            //}
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            var distal = HtmSerializer2.DeserializeObject<T>(sr, name);
            return distal;
        }
    }
}

