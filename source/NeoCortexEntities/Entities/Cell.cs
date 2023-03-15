// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Defines a single cell (neuron).
    /// </summary>
    public class Cell : IEquatable<Cell>, IComparable<Cell>, ISerializable
    {
        /// <summary>
        /// If the cell has a meaning like a Grand-Mather cell, the meaning value is set.
        /// </summary>
        //public string Label { get; set; }

        /// <summary>
        /// Index of the cell.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The mini-column index or the area identifier, which owns this cell.
        /// A cell can be owned by area explicitely if mini-columns are not used.
        /// </summary>
        public int ParentColumnIndex { get; set; }

        /// <summary>
        /// Stores the calculated cell's hashcode.
        /// </summary>
        private readonly int m_Hashcode;

        /// <summary>
        /// List of dendrites of the cell.
        /// </summary>
        public List<DistalDendrite> DistalDendrites { get; set; } = new List<DistalDendrite>();

        /// <summary>
        /// List of apical dendrites of the cell. 
        /// </summary>
        public List<ApicalDendrite> ApicalDendrites { get; set; } = new List<ApicalDendrite>();

        /// <summary>
        /// List of receptor synapses (outgoing synapses) that connect this cells as a source (presynaptic) cell to the distal dendrite segment owned by some other cell.
        /// This synapse assotiates the cell (presynaptic = source cell) with the next cell (postsynaptic = destination cell).
        /// The destination cell is the parent cell of the segment to which the presynaptic cell is connected.
        /// </summary>
        public List<Synapse> ReceptorSynapses { get; set; } = new List<Synapse>();

        /// <summary>
        /// Used for testing.
        /// </summary>
        public Cell()
        {
            this.ParentColumnIndex = -1;
        }

        /// <summary>
        /// Constructs a new <see cref="Cell"/> object
        /// </summary>
        /// <param name="parentColumnIndx"></param>
        /// <param name="colSeq">The index of this <see cref="Cell"/> within its column.</param>
        /// <param name="numCellsPerColumn"></param>
        /// <param name="cellId"></param>
        /// <param name="cellActivity"></param>
        public Cell(int parentColumnIndx, int colSeq, int numCellsPerColumn, CellActivity cellActivity)
        {
            this.ParentColumnIndex = parentColumnIndx;

            this.Index = parentColumnIndx * numCellsPerColumn + colSeq;
        }


        /// <summary>
        /// Creates the cell
        /// </summary>
        /// <param name="parentIndex">The index of the area that owns the cell. This can be a mini-column or a <see cref="CorticalArea"/></param>
        /// <param name="index">The index of the cell inside the given area.</param>
        public Cell(int parentIndex, int index)
        {
            this.ParentColumnIndex = parentIndex;

            this.Index = index;
        }

        /// <summary>
        /// Gets the hashcode of the cell.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ParentColumnIndex;
            result = prime * result + Index;
            result = result * ParentColumnIndex;

            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool Equals(Cell obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            if (Index != obj.Index)
                return false;
            else
            {
                if (obj.ParentColumnIndex != this.ParentColumnIndex)
                    return false;

                //if (obj.CellId != this.CellId)
                //    return false;

                if (obj.DistalDendrites != null && this.DistalDendrites != null)
                {
                    //if (!Enumerable.SequenceEqual(obj.DistalDendrites, this.DistalDendrites))
                    //    return false;
                    if (!obj.DistalDendrites.ElementsEqual(this.DistalDendrites))
                        return false;
                }

                if (obj.ReceptorSynapses != null && this.ReceptorSynapses != null)
                {
                    if (!obj.ReceptorSynapses.ElementsEqual(this.ReceptorSynapses))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Parent={this.ParentColumnIndex} - Index={this.Index}";
        }



        /// <summary>
        /// Trace dendrites and synapses.
        /// </summary>
        /// <returns></returns>
        public string TraceCell()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Cell {this.Index} ");

            sb.AppendLine($"\tApical Segments {this.ApicalDendrites.Count}");

            foreach (var seg in this.ApicalDendrites)
            {
                sb.AppendLine(seg.ToString());
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Compares two cells.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Cell other)
        {
            if (this.Index < other.Index)
                return -1;
            else if (this.Index > other.Index)
                return 1;
            else
                return 0;
        }

        #region Serialization

        /// <summary>
        /// Serializes the cell to the stream.
        /// </summary>
        /// <param name="writer"></param>
        public void SerializeT(StreamWriter writer)
        {
            HtmSerializer ser = new HtmSerializer();

            ser.SerializeBegin(nameof(Cell), writer);

            ser.SerializeValue(this.Index, writer);
            //ser.SerializeValue(this.CellId, writer);
            ser.SerializeValue(this.ParentColumnIndex, writer);

            if (this.DistalDendrites != null && this.DistalDendrites.Count > 0)
                ser.SerializeValue(this.DistalDendrites, writer);

            if (this.ReceptorSynapses != null && this.ReceptorSynapses.Count > 0)
                ser.SerializeValue(this.ReceptorSynapses, writer);

            ser.SerializeEnd(nameof(Cell), writer);
        }

        public static Cell Deserialize(StreamReader sr)
        {
            Cell cell = new Cell();

            HtmSerializer ser = new HtmSerializer();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Cell)) || data.ToCharArray()[0] == HtmSerializer.ElementsDelimiter || (data.ToCharArray()[0] == HtmSerializer.ElementsDelimiter && data.ToCharArray()[1] == HtmSerializer.ParameterDelimiter))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(Segment)))
                {
                    //cell.DistalDendrites.Add(DistalDendrite.Deserialize(sr));
                }
                else if (data == ser.ReadBegin(nameof(Synapse)))
                {
                    return cell;
                }
                else if (data == ser.ReadEnd(nameof(Cell)))
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
                                    cell.Index = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    // cell.CellId = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    cell.ParentColumnIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return cell;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string>
            {
                nameof(Cell.ReceptorSynapses),
                nameof(m_Hashcode)
            };
            HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers);
            //var cell = obj as Cell;
            //if (cell != null)
            //{
            //    var synapses = cell.ReceptorSynapses.Select(s => new Synapse() { SynapseIndex = s.SynapseIndex });
            //    HtmSerializer2.Serialize(synapses, nameof(Cell.ReceptorSynapses), sw);
            //}
        }
        public static object Deserialize<T>(StreamReader sr, string name)
        {
            if (typeof(T) != typeof(Cell))
                return null;
            var cell = HtmSerializer.DeserializeObject<Cell>(sr, name);

            //foreach (var distalDentrite in cell.DistalDendrites)
            //{
            //    distalDentrite.ParentCell = cell;
            //}
            return cell;
        }

      
        #endregion
    }
}