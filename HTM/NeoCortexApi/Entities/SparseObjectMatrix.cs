using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NeoCortexApi.DistributedComputeLib;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Reflection;

namespace NeoCortexApi.Entities
{
    /**
   * Allows storage of array data in sparse form, meaning that the indexes
   * of the data stored are maintained while empty indexes are not. This allows
   * savings in memory and computational efficiency because iterative algorithms
   * need only query indexes containing valid data.
   * 
   * @author David Ray, Damir Dobric
   *
   * @param <T>
   */
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class SparseObjectMatrix<T> : AbstractSparseMatrix<T>, IEquatable<T> where T : class
    {

        //private IDictionary<int, T> sparseMap = new Dictionary<int, T>();
        private IDistributedDictionary<int, T> sparseMap;
        
        /// <summary>
        /// Returns true if sparse memory is remotely distributed. It means objects has to be synced with remote partitions.
        /// </summary>
        public bool IsRemotelyDistributed
        {
            get
            {
                return this.sparseMap is IHtmDistCalculus;
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

        /**
         * Constructs a new {@code SparseObjectMatrix}
         * @param dimensions					the dimensions of this array
         * @param useColumnMajorOrdering		where inner index increments most frequently
         */
        public SparseObjectMatrix(int[] dimensions, bool useColumnMajorOrdering = false, IDistributedDictionary<int, T> dict = null) : base(dimensions, useColumnMajorOrdering)
        {
            if (dict == null)
                this.sparseMap = new InMemoryDistributedDictionary<int, T>(1);
            else
                this.sparseMap = dict;
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
            if (!(this.sparseMap is IHtmDistCalculus))
            {
                if (!sparseMap.ContainsKey(index))
                    sparseMap.Add(index, (T)obj);
                else
                    sparseMap[index] = obj;
            }
            else
            {
                sparseMap[index] = obj;
            }

            return this;
        }

        public override AbstractFlatMatrix<T> set(List<KeyPair> updatingValues)
        {
            sparseMap.AddOrUpdate(updatingValues);
            return this;
        }

        /**
         * Sets the specified object to be indexed at the index
         * computed from the specified coordinates.
         * @param object        the object to be indexed.
         * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
         */
     
        public override AbstractFlatMatrix<T> set(int[] coordinates, T obj)
        {
            set(computeIndex(coordinates), obj);
            return this;
        }

        /**
         * Returns the T at the specified index.
         * 
         * @param index     the index of the T to return
         * @return  the T at the specified index.
         */
        // @Override
        public override T getObject(int index)
        {
            return GetColumn(index);
        }



        /**
         * Returns the T at the index computed from the specified coordinates
         * @param coordinates   the coordinates from which to retrieve the indexed object
         * @return  the indexed object
         */
        // @Override
        public T get(int[] coordinates)
        {
            return GetColumn(computeIndex(coordinates));
        }


        /**
         * Returns the T at the specified index.
         * 
         * @param index     the index of the T to return
         * @return  the T at the specified index.
         */
        // @Override
        public override T GetColumn(int index)
        {
            T val = null;

            this.sparseMap.TryGetValue(index, out val);

            return val;
            //return this.sparseMap[index];
        }

        /**
         * Returns a sorted array of occupied indexes.
         * @return  a sorted array of occupied indexes.
         */
        // @Override
        public override int[] getSparseIndices()
        {
            return Reverse(sparseMap.Keys.ToArray());
        }

        /**
         * {@inheritDoc}
         */
        // @Override
        public override String ToString()
        {
            return getDimensions().ToString();
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */
        //    @Override
        public override int GetHashCode()
        {
            int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + ((sparseMap == null) ? 0 : sparseMap.GetHashCode());
            return result;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#equals(java.lang.Object)
         */
        //SuppressWarnings("rawtypes")
        // @Override
        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (!base.Equals(obj))
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            SparseObjectMatrix<T> other = obj as SparseObjectMatrix<T>;
            if (other == null)
                return false;

            if (sparseMap == null)
            {
                if (other.sparseMap != null)
                    return false;
            }
            else if (!sparseMap.Equals(other.sparseMap))
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
        public new StringBuilder Serialize(object instance)
        {
            Debug.WriteLine("");
            Debug.WriteLine($"Inst: {instance.GetType().Name}");

            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            bool isFirst = true;

            foreach (PropertyInfo property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                Debug.WriteLine($"Prop: {property.Name}");
                if (property.Name == "Segment")
                    continue;
                SerializeMember(isFirst, sb, property.PropertyType, property.Name, property.GetValue(instance));
                isFirst = false;
            }

            foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Debug.WriteLine($"Field: {field.Name}");
                SerializeMember(isFirst, sb, field.FieldType, field.Name, field.GetValue(instance));
                isFirst = false;
            }

            sb.AppendLine();
            sb.Append("}");

            return sb;
        }

        private void SerializeMember(bool isFirst, StringBuilder sb, Type type, string name, object value)
        {
            //      if (name.Contains("k__BackingField") || name.Contains("i__Field"))
            //         return;

            if (isFirst == false)
                sb.Append(",");

            if (type.Name == "String")
                SerializeStringValue(sb, name, value as string);
            else if (type.IsArray || (type.IsGenericType && type.Name == "List`1"))
                SerializeArray(sb, name, value);
            else if (type.Name == "Dictionary`2")
                SerializeDictionary(sb, name, value);
            else if (type.IsInterface)
                SerializeComplexValue(sb, name, value);

            else if (type.IsClass)
                SerializeComplexValue(sb, name, value);
            else
                SerializeNumericValue(sb, name, JsonConvert.SerializeObject(value));
        }




        private void SerializeArray(StringBuilder sb, string name, object value)
        {
            var arrStr = JsonConvert.SerializeObject(value);

            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");

            sb.Append(arrStr);
        }

        private void SerializeDictionary(StringBuilder sb, string name, object value)
        {
            var arrStr = JsonConvert.SerializeObject(value);

            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");

            sb.Append(arrStr);
        }

        private void SerializeStringValue(StringBuilder sb, string name, string value)
        {
            AppendPropertyName(sb, name);

            sb.Append("\"");
            sb.Append(value);
            sb.Append("\"");
        }


        private void SerializeNumericValue(StringBuilder sb, string name, string value)
        {
            AppendPropertyName(sb, name);

            sb.Append(value);
        }

        private static void AppendPropertyName(StringBuilder sb, string name)
        {
            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");
        }

        private void SerializeComplexValue(StringBuilder sb, string name, object value)
        {
            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");

            if (value != null)
            {
                var sbComplex = Serialize(value);
                sb.Append(sbComplex.ToString());
            }
            else
            {
                sb.Append("null");
            }
        }

    }
}
