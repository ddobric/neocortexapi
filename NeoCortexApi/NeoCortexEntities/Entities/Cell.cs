// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Defines a single cell (neuron).
    /// </summary>
    public class Cell : IEquatable<Cell>, IComparable<Cell>
    {

        /// <summary>
        /// Index of the cell.
        /// </summary>
        public int Index { get; set; }
        
        /// <summary>
        /// The identifier of the cell.
        /// </summary>
        public int CellId { get; private set; }
               

        /// <summary>
        /// The column, which owns this cell.
        /// </summary>
        public int ParentColumnIndex { get; set; }

        /// <summary>
        /// Stores the calculated cell's hashcode.
        /// </summary>
        private readonly int m_Hashcode;

        /// <summary>
        /// List of dendrites of the cell. Every dendrite segment is owned bt the cell.
        /// </summary>
        public List<DistalDendrite> DistalDendrites { get; set; } = new List<DistalDendrite>();

        /// <summary>
        /// List of receptor synapses that connect this cells as a source cell to the distal dendrit segment owned by some other cell.
        /// </summary>
        public List<Synapse> ReceptorSynapses { get; set; } = new List<Synapse>();

        /// <summary>
        /// Used for testing.
        /// </summary>
        public Cell()
        {
     
        }
       
        /// <summary>
        /// Constructs a new <see cref="Cell"/> object
        /// </summary>
        /// <param name="parentColumnIndx"></param>
        /// <param name="colSeq">the index of this <see cref="Cell"/> within its column</param>
        /// <param name="numCellsPerColumn"></param>
        /// <param name="cellId"></param>
        /// <param name="cellActivity"></param>
        public Cell(int parentColumnIndx, int colSeq, int numCellsPerColumn, int cellId, CellActivity cellActivity)
        {
            this.ParentColumnIndex = parentColumnIndx;
            
            this.Index = parentColumnIndx * numCellsPerColumn + colSeq;
            
            this.CellId = cellId;
        }

        /// <summary>
        /// Gets the hashcode of the cell.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (m_Hashcode == 0)
            {
                int prime = 31;
                int result = 1;
                result = prime * result + Index;
                return result;
            }
            return m_Hashcode;
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

                if (obj.CellId != this.CellId)
                    return false;

                if (obj.DistalDendrites != null && this.DistalDendrites != null)
                {
                    //if (!Enumerable.SequenceEqual(obj.DistalDendrites, this.DistalDendrites))
                    //    return false;
                    if (!obj.DistalDendrites.SequenceEqual(this.DistalDendrites))
                        return false;
                }

                if (obj.ReceptorSynapses != null && this.ReceptorSynapses != null)
                {
                    if (!obj.ReceptorSynapses.SequenceEqual(this.ReceptorSynapses))
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
            return $"Cell: Indx={this.Index}, [{this.ParentColumnIndex}]";
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
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Cell), writer);

            ser.SerializeValue(this.Index, writer);
            ser.SerializeValue(this.CellId, writer);
            ser.SerializeValue(this.ParentColumnIndex, writer);
            if (this.DistalDendrites != null && this.DistalDendrites.Count > 0)
                ser.SerializeValue(this.DistalDendrites.Select(s => s.Ordinal).ToArray(), writer);

            if (this.ReceptorSynapses != null && this.ReceptorSynapses.Count > 0)
                ser.SerializeValue(this.ReceptorSynapses.Select(s => s.SynapseIndex).ToArray(), writer);

            ser.SerializeEnd(nameof(Cell), writer);
        }

        /// <summary>
        /// Deserializes the cell from the stream.
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static Cell Deserialize(StreamReader sr)
        {
            Cell cell = new Cell();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();

                if (data == String.Empty || data == ser.ReadBegin(nameof(Cell)))
                {
                    continue;
                }
                else if (data == ser.ReadEnd(nameof(Cell)))
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
                                    cell.Index = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    cell.CellId = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    cell.ParentColumnIndex = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    int[] vs = ser.ReadArrayInt(str[i]);
                                    cell.DistalDendrites = new List<DistalDendrite>();
                                    for (int j = 0; j < vs.Count(); j++)
                                    {
                                        cell.DistalDendrites.Add(new DistalDendrite());
                                        cell.DistalDendrites[j].Ordinal = vs[j];
                                        using (StreamReader swLD = new StreamReader($"ser_SerializeDistalDendrite_{cell.DistalDendrites[j].Ordinal}.txt"))
                                        {
                                            cell.DistalDendrites[j] = DistalDendrite.Deserialize(swLD);
                                        }
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    int[] vs = ser.ReadArrayInt(str[i]);
                                    cell.ReceptorSynapses = new List<Synapse>();
                                    for (int j = 0; j < vs.Count(); j++)
                                    {
                                        cell.ReceptorSynapses.Add(new Synapse());
                                        cell.ReceptorSynapses[j].SynapseIndex = vs[j];

                                        using (StreamReader swLS = new StreamReader($"ser_SerializeSynapseTest_{cell.ReceptorSynapses[j].SynapseIndex}.txt"))
                                        {
                                            cell.ReceptorSynapses[j] = Synapse.Deserialize(swLS);
                                        }
                                    }
                                    
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
        #endregion
    }
}