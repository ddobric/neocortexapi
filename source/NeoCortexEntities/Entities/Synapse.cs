// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;


namespace NeoCortexApi.Entities
{


    /**
     * THI SSGOULD BE VALIDATED. IT SEEMS TO BE WRONG
* Represents a connection with varying strength which when above 
* a configured threshold represents a valid connection. 
* 
* IMPORTANT: 	For DistalDendrites, there is only one synapse per pool, so the
* 				synapse's index doesn't really matter (in terms of tracking its
* 				order within the pool). In that case, the index is a global counter
* 				of all distal dendrite synapses.
* 
* 				For ProximalDendrites, there are many synapses within a pool, and in
* 				that case, the index specifies the synapse's sequence order within
* 				the pool object, and may be referenced by that index.
*    

*/

    /// <summary>
    /// Implements the synaptic connection.
    /// ProximalDendrites hold many synapses, which connect columns to the sensory input.
    /// DistalDendrites build synaptic connections to cells inside of columns.
    /// </summary>
    public class Synapse : IEquatable<Synapse>, IComparable<Synapse>/*, ISerializable*/
    {
        /// <summary>
        /// Cell which activates this synapse. On proximal dendrite is this set on NULL. That means proximal dentrites have no presynaptic cell.
        /// </summary>
        public Cell SourceCell { get; set; }

        //[JsonIgnore]
        //public Segment Segment { get; set; }

        /// <summary>
        /// The index of the associating (conected) segment.
        /// </summary>
        public int SegmentIndex { get; set; }

        /// <summary>
        /// The index of the segment's parent cell.
        /// </summary>
        public int SegmentParentCellIndex { get; set; }

        /// <summary>
        /// The unique name of the area of the segment's parent cell.
        /// </summary>
        public string? SegmentAreaName { get; set; }

        /// <summary>
        /// The index of the synapse.
        /// </summary>
        public int SynapseIndex { get; set; }

        /// <summary>
        /// Index of pre-synaptic cell.
        /// </summary>
        public int InputIndex { get; set; }

        /// <summary>
        /// The synaptic weight.
        /// </summary>
        public double Permanence { get; set; }

        /// <summary>
        /// the flag indicating whether this segment has been destroyed.
        /// </summary>
        public bool IsDestroyed { get; set; }

        /// <summary>
        /// Used by serialization.
        /// </summary>
        public Synapse() { }


        /// <summary>
        /// Creates the synapse on the DistalDendrite, which connect cells during temporal learning process.
        /// </summary>
        /// <param name="presynapticCell">The cell which connects to the segment.</param>
        /// <param name="distalSegmentIndex">The index of the segment.</param>
        /// <param name="synapseIndex">The index of the synapse.</param>
        /// <param name="permanence">The permanmence value.</param>
        public Synapse(Cell presynapticCell, int distalSegmentIndex, int synapseIndex, double permanence)
        {
            this.SourceCell = presynapticCell;
            this.SegmentIndex = distalSegmentIndex;
            this.SynapseIndex = synapseIndex;
            this.InputIndex = presynapticCell.Index;
            this.Permanence = permanence;
        }


        /// <summary>
        /// Creates the synapse on the DistalDendrite, which connect cells during temporal learning process.
        /// </summary>
        /// <param name="presynapticCell">The cell which connects to the segment.</param>
        /// <param name="distalSegmentIndex">The index of the segment.</param>
        /// <param name="synapseIndex">The index of the synapse.</param>
        /// <param name="permanence">The permanmence value.</param>
        /// <param name="segmentCellIndex"></param>
        /// <param name="segmentAreaName"></param>
        /// <param name="permanence"></param>
        /// <remarks>Used by NAA.</remarks>
        public Synapse(Cell presynapticCell, int synapseIndex, int segmentIndex, int segmentCellIndex, string segmentAreaName, double permanence)
        {
            this.SourceCell = presynapticCell;
            this.SegmentIndex = segmentIndex;
            this.SynapseIndex = synapseIndex;
            this.InputIndex = presynapticCell.Index;

            this.SegmentParentCellIndex = segmentCellIndex;
            this.SegmentAreaName = segmentAreaName;

            this.Permanence = permanence;
        }


        /// <summary>
        /// Creates the synapse on the PriximalDendrite segment, which connects columns to sensory input.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment.</param>
        /// <param name="inputIndex">The index of the synapse.</param>
        public Synapse(int segmentIndex, int synapseIndex, int inputIndex)
        {
            this.SourceCell = null;
            this.SegmentIndex = segmentIndex;
            this.SynapseIndex = synapseIndex;
            this.InputIndex = inputIndex;
        }


        /// <summary>
        /// Called by <see cref="Connections.DestroySynapse(Synapse, Segment)"/> to assign a reused Synapse to another presynaptic Cell
        /// </summary>
        /// <param name="cell">the new presynaptic cell</param>
        //public void SetPresynapticCell(Cell cell)
        //{
        //    this.SourceCell = cell;
        //}

        /// <summary>
        /// Returns the containing <see cref="Cell"/>
        /// </summary>
        /// <returns></returns>
        public Cell GetPresynapticCell()
        {
            return SourceCell;
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string srcCell = string.Empty;
            if (SourceCell != null)
            {
                srcCell = $"[SrcCell: {SourceCell.ToString()}]";
            }

            return $"Syn: synIndx:{SynapseIndex}, inpIndx:{InputIndex}, perm:{this.Permanence}[ segIndx: {this.SegmentAreaName}/{this.SegmentParentCellIndex}/{SegmentIndex}], {srcCell}";
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + InputIndex;
            if (SegmentAreaName != null)
                result = prime * result + SegmentAreaName.GetHashCode();
            result = prime * result + SegmentParentCellIndex;
            result = prime * result + this.SegmentIndex;
            result = prime * result + ((SourceCell == null) ? 0 : SourceCell.GetHashCode());
            result = prime * result + SynapseIndex;
            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (typeof(Synapse) != obj.GetType())
                return false;
            Synapse other = (Synapse)obj;
            if (InputIndex != other.InputIndex)
                return false;
            if (SegmentIndex != ((Synapse)obj).SegmentIndex)
            {
                return false;
            }
            if (SourceCell == null)
            {
                if (other.SourceCell != null)
                    return false;
            }
            else if (!SourceCell.Equals(other.SourceCell))
                return false;
            if (SynapseIndex != other.SynapseIndex)
                return false;
            if (Permanence != other.Permanence)
                return false;

            if (SegmentAreaName != other.SegmentAreaName)
                return false;

            if (SegmentIndex != other.SegmentIndex)
                return false;

            if (SegmentParentCellIndex != other.SegmentParentCellIndex)
                return false;

            return true;
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool Equals(Synapse obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (InputIndex != obj.InputIndex)
                return false;

            //
            // Synapses are equal if they belong to the same segment.
            if (SegmentIndex != ((Synapse)obj).SegmentIndex)
                return false;

            if (SourceCell == null)
            {
                if (obj.SourceCell != null)
                    return false;
            }
            // We check here the cell id only! The cell as parent must be correctlly created to avoid having different cells with the same id.
            // If we would use here SourceCell.Equals method, that method would cause a cicular invoke of this.Equals etc.
            //else if (SourceCell.CellId != obj.SourceCell.CellId)
            //    return false;
            if (SynapseIndex != obj.SynapseIndex)
                return false;
            if (Permanence != obj.Permanence)
                return false;
            if (IsDestroyed != obj.IsDestroyed)
                return false;
            //if (boxedIndex == null)
            //{
            //    if (obj.boxedIndex != null)
            //        return false;
            //}
            //else if (boxedIndex != obj.boxedIndex)
            //    return false;

            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public int CompareTo(Synapse other)
        {
            if (this.SegmentIndex < other.SegmentIndex)
                return -1;
            if (this.SegmentIndex > other.SegmentIndex)
                return 1;
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
            HtmSerializer ser = new HtmSerializer();

            ser.SerializeBegin(nameof(Synapse), writer);

            ser.SerializeValue(this.SegmentIndex, writer);
            ser.SerializeValue(this.SynapseIndex, writer);
            ser.SerializeValue(this.InputIndex, writer);
            ser.SerializeValue(this.Permanence, writer);
            ser.SerializeValue(this.IsDestroyed, writer);

            //if (this.boxedIndex != null)
            //{
            //    this.boxedIndex.Serialize(writer);
            //}

            // If we use this, we will get a cirular serialization.
            //if (this.SourceCell != null)
            //{
            //    this.SourceCell.SerializeT(writer);
            //}

            ser.SerializeEnd(nameof(Synapse), writer);
        }

        /// <summary>
        /// Serialize method for Synapse
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer ser = new HtmSerializer();

            ser.SerializeBegin(nameof(Synapse), writer);

            ser.SerializeValue(this.SegmentIndex, writer);
            ser.SerializeValue(this.SynapseIndex, writer);
            ser.SerializeValue(this.InputIndex, writer);
            ser.SerializeValue(this.Permanence, writer);
            ser.SerializeValue(this.IsDestroyed, writer);

            //if (this.boxedIndex != null)
            //{
            //    this.boxedIndex.Serialize(writer);
            //}

            if (this.SourceCell != null)
            {
                this.SourceCell.SerializeT(writer);
            }

            ser.SerializeEnd(nameof(Synapse), writer);
        }

        public static Synapse Deserialize(StreamReader sr)
        {
            Synapse synapse = new Synapse();

            HtmSerializer ser = new HtmSerializer();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Synapse)))
                {
                    continue;
                }
                //else if (data == ser.ReadBegin(nameof(Integer)))
                //{
                //    synapse.boxedIndex = Integer.Deserialize(sr);
                //}
                else if (data == ser.ReadBegin(nameof(Cell)))
                {
                    synapse.SourceCell = Cell.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(Synapse)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    synapse.SegmentIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    synapse.SynapseIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    synapse.InputIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    synapse.Permanence = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 4:
                                {
                                    synapse.IsDestroyed = ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return synapse;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            if (obj is Synapse synapse)
            {
                var ignoreMembers = new List<string>
                {
                    //nameof(Synapse.SourceCell),
                };
                HtmSerializer.SerializeObject(synapse, name, sw, ignoreMembers);
            }
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            return HtmSerializer.DeserializeObject<T>(sr, name);
        }
        #endregion
    }
}
