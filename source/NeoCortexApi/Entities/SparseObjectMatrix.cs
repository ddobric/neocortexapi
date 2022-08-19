// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Allows storage of array data in sparse form, meaning that the indexes of the data stored are maintained while empty indexes are not. This allows
    /// savings in memory and computational efficiency because iterative algorithms need only query indexes containing valid data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// @author David Ray, Damir Dobric
    /// </remarks>
    public class SparseObjectMatrix<T> : AbstractSparseMatrix<T>, IEquatable<T>, ISerializable where T : class
    {

        //private IDictionary<int, T> sparseMap = new Dictionary<int, T>();
        private IDistributedDictionary<int, T> m_SparseMap;

        /// <summary>
        /// Returns true if sparse memory is remotely distributed. It means objects has to be synced with remote partitions.
        /// </summary>
        public bool IsRemotelyDistributed
        {
            get
            {
                return this.m_SparseMap is IHtmDistCalculus;
            }
        }

        /// <summary>
        /// Gets partitions (nodes) with assotiated indexes.
        /// </summary>
        /// <returns></returns>
        //public List<(int partId, int minKey, int maxKey)> GetPartitions()
        //{
        //    if (IsRemotelyDistributed)
        //    {
        //        IHtmDistCalculus map = this.sparseMap as IHtmDistCalculus;
        //        return map.GetPartitions();
        //    }
        //    else
        //        throw new InvalidOperationException("GetPartitions can only be ued for remotely distributed collections.");
        //}

        /**
         * Constructs a new {@code SparseObjectMatrix}
         * @param dimensions	the dimensions of this array
         */
        //public SparseObjectMatrix(int[] dimensions) : base(dimensions, false)
        //{

        //}

        /// <summary>
        /// Constructs a new <see cref="SparseObjectMatrix{T}"/>
        /// </summary>
        /// <param name="dimensions">the dimensions of this array</param>
        /// <param name="useColumnMajorOrdering">where inner index increments most frequently</param>
        /// <param name="dict"></param>
        public SparseObjectMatrix(int[] dimensions, bool useColumnMajorOrdering = false, IDistributedDictionary<int, T> dict = null) : base(dimensions, useColumnMajorOrdering)
        {
            if (dict == null)
                this.m_SparseMap = new InMemoryDistributedDictionary<int, T>(1);
            else
                this.m_SparseMap = dict;
        }
        //Default constructor for Deserialisation
        public SparseObjectMatrix()
        {
        }


        /// <summary>
        /// Sets the object to occupy the specified index.
        /// </summary>
        /// <param name="index">The index the object will occupy</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns></returns>
        public override AbstractFlatMatrix<T> set(int index, T obj)
        {
            //
            // If not distributed in cluster, we add element by element.
            if (!(this.m_SparseMap is IHtmDistCalculus))
            {
                if (!m_SparseMap.ContainsKey(index))
                    m_SparseMap.Add(index, obj);
                else
                    m_SparseMap[index] = obj;
            }
            else
            {
                m_SparseMap[index] = obj;
            }

            return this;
        }

        public override AbstractFlatMatrix<T> set(List<KeyPair> updatingValues)
        {
            m_SparseMap.AddOrUpdate(updatingValues);
            return this;
        }

        /// <summary>
        /// Sets the specified object to be indexed at the index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">the row major coordinates [outer --> ,...,..., inner]</param>
        /// <param name="obj">the object to be indexed.</param>
        /// <returns></returns>
        public override AbstractFlatMatrix<T> set(int[] coordinates, T obj)
        {
            set(ComputeIndex(coordinates), obj);
            return this;
        }

        /**
         * Returns the T at the specified index.
         * 
         * @param index     the index of the T to return
         * @return  the T at the specified index.
         */
        // @Override
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override T GetObject(int index)
        {
            return GetColumn(index);
        }



        /**
         * Returns the T at the index computed from the specified coordinates
         * @param coordinates   the coordinates from which to retrieve the indexed object
         * @return  the indexed object
         */
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="coordinates"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override T get(int[] coordinates)
        {
            return GetColumn(ComputeIndex(coordinates));
        }

        /// <summary>
        /// Returns the T at the specified index.
        /// </summary>
        /// <param name="index">the index of the T to return</param>
        /// <returns>the T at the specified index.</returns>
        public override T GetColumn(int index)
        {
            T val = null;

            this.m_SparseMap.TryGetValue(index, out val);

            return val;
            //return this.sparseMap[index];
        }

        /// <summary>
        /// Returns a sorted array of occupied indexes.
        /// </summary>
        /// <returns>a sorted array of occupied indexes.</returns>
        public override int[] GetSparseIndices()
        {
            return Reverse(m_SparseMap.Keys.ToArray());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override String ToString()
        {
            return GetDimensions().ToString();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((m_SparseMap == null) ? 0 : m_SparseMap.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            var matrix = obj as SparseObjectMatrix<T>;
            if (matrix == null)
                return false;
            return this.Equals(matrix);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override bool Equals(AbstractFlatMatrix<T> obj)
        {
            if (this == obj)
                return true;
            if (!base.Equals(obj))
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            SparseObjectMatrix<T> other = obj as SparseObjectMatrix<T>;
            return this.Equals(other);
        }

        public bool Equals(SparseObjectMatrix<T> other)
        {
            if (other == null)
                return false;

            if (m_SparseMap == null)
            {
                if (other.m_SparseMap != null)
                    return false;
            }
            else if (!m_SparseMap.Equals(other.m_SparseMap))
                return false;
            if (ModuleTopology == null)
            {
                if (other.ModuleTopology != null)
                    return false;
            }
            else if (!ModuleTopology.Equals(other.ModuleTopology))
                return false;
            if (IsRemotelyDistributed != other.IsRemotelyDistributed)
                return false;

            return true;
        }

        public bool Equals(T other)
        {
            return this.Equals((object)other);
        }

        public override ICollection<KeyPair> GetObjects(int[] indexes)
        {
            throw new NotImplementedException();
        }


        public override void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(SparseObjectMatrix<T>), writer);

            ser.SerializeValue(this.IsRemotelyDistributed, writer);

            if (this.ModuleTopology != null)
            { this.ModuleTopology.Serialize(writer); }

            if (this.m_SparseMap != null)
            { this.m_SparseMap.Serialize(writer); }


            ser.SerializeEnd(nameof(SparseObjectMatrix<T>), writer);
        }

        public static SparseObjectMatrix<T> Deserialize(StreamReader sr)
        {
            SparseObjectMatrix<T> sparse = new SparseObjectMatrix<T>();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(SparseObjectMatrix<T>)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(HtmModuleTopology)))
                {
                    sparse.ModuleTopology = HtmModuleTopology.Deserialize(sr);
                }
                //else if (data == ser.ReadBegin(nameof(InMemoryDistributedDictionary<TKey, TValue>) <{ nameof(TKey}>))
                //{
                //    sparse.m_SparseMap = InMemoryDistributedDictionary<TKey, TValue>.Deserialize(sr);
                //}
                else if (data == ser.ReadEnd(nameof(SparseObjectMatrix<T>)))
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
                                    //sparse.IsRemotelyDistributed = ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return sparse;
        }


        public void Serialize(object obj, string name, StreamWriter sw)
        {
            HtmSerializer2.SerializeObject(obj, name, sw);

            //var matrixColumns = obj as SparseObjectMatrix<Column>;
            //if (matrixColumns != null)
            //{
            //    var ddSynapses = matrixColumns.m_SparseMap.Values.SelectMany(c => c.Cells).SelectMany(c => c.DistalDendrites).SelectMany(d => d.Synapses);
            //    var cellSynapses = matrixColumns.m_SparseMap.Values.SelectMany(c => c.Cells).SelectMany(c => c.ReceptorSynapses);

            //    var synapses = new List<Synapse>();
            //    foreach (var synapse in ddSynapses)
            //    {
            //        if (synapses.Contains(synapse) == false)
            //        {
            //            synapses.Add(synapse);
            //        }
            //    }

            //    foreach (var synapse in cellSynapses)
            //    {
            //        if (synapses.Contains(synapse) == false)
            //        {
            //            synapses.Add(synapse);
            //        }
            //    }

            //    HtmSerializer2.Serialize(synapses, "synapsesList", sw);
            //}
        }

        public static object Deserialize<TItem>(StreamReader sr, string name)
        {
            var ignoreMembers = new List<string>
            {
                //"synapsesList"
            };
            var matrix = HtmSerializer2.DeserializeObject<SparseObjectMatrix<T>>(sr, name, ignoreMembers, (m, propName) =>
            {
                //var matrixColumns = m as SparseObjectMatrix<Column>;
                //if (matrixColumns == null)
                //    return;

                //var synapses = HtmSerializer2.Deserialize<List<Synapse>>(sr, "synapsesList");

                //foreach (var column in matrixColumns.m_SparseMap.Values)
                //{
                //    foreach (var cell in column.Cells)
                //    {
                //        var cellSynapses = synapses.Where(s => s.InputIndex == cell.Index).ToList();
                //        foreach (var synapse in cellSynapses)
                //        {
                //            synapse.SourceCell = cell;
                //        }
                //    }
                //}


                //foreach (var column in matrixColumns.m_SparseMap.Values)
                //{
                //    foreach (var cell in column.Cells)
                //    {
                //        cell.ReceptorSynapses = synapses.Where(s => s.InputIndex == cell.Index).ToList();

                //        foreach (var distalDentrite in cell.DistalDendrites)
                //        {
                //            distalDentrite.Synapses = synapses.Where(s => s.SegmentIndex == distalDentrite.SegmentIndex).ToList();
                //        }
                //    }
                //}
            });

            return matrix;
        }
    }
}
