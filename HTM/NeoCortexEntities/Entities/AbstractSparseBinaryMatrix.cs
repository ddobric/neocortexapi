
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using System.Diagnostics;

namespace NeoCortexApi.Entities
{
    //TODO see type object
   // [Serializable]
    public abstract class AbstractSparseBinaryMatrix : AbstractSparseMatrix<int>, IEquatable<object>
    {
        /** keep it simple */
        private static readonly long serialVersionUID = 1L;

        public int[] trueCounts;

        public AbstractSparseBinaryMatrix()
        {

        }
        /**
         * Constructs a new {@code AbstractSparseBinaryMatrix} with the specified
         * dimensions (defaults to row major ordering)
         * 
         * @param dimensions    each indexed value is a dimension size
         */
        public AbstractSparseBinaryMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /**
         * Constructs a new {@code AbstractSparseBinaryMatrix} with the specified dimensions,
         * allowing the specification of column major ordering if desired. 
         * (defaults to row major ordering)
         * 
         * @param dimensions                each indexed value is a dimension size
         * @param useColumnMajorOrdering    if true, indicates column first iteration, otherwise
         *                                  row first iteration is the default (if false).
         */
        public AbstractSparseBinaryMatrix(int[] dimensions, bool useColumnMajorOrdering) : base(dimensions, useColumnMajorOrdering)
        {

            this.trueCounts = new int[dimensions[0]];
        }

        /**
         * Returns the slice specified by the passed in coordinates.
         * The array is returned as an object, therefore it is the caller's
         * responsibility to cast the array to the appropriate dimensions.
         * 
         * @param coordinates	the coordinates which specify the returned array
         * @return	the array specified
         * @throws	IllegalArgumentException if the specified coordinates address
         * 			an actual value instead of the array holding it.
         */
        public abstract Object getSlice(params int[] coordinates);

        /**
         * Launch getSlice error, to share it with subclass {@link #getSlice(int...)}
         * implementations.
         * @param coordinates
         */
        protected void sliceError(int[] coordinates)
        {
            throw new ArgumentException(
                "This method only returns the array holding the specified maximum index: " +
                        "dimensions.ToString()");
        }

        /**
         * Calculate the flat indexes of a slice
         * @return the flat indexes array
         */
        protected int[] getSliceIndexes(int[] coordinates)
        {
            int[] dimensions = getDimensions();
            // check for valid coordinates
            if (coordinates.Length >= dimensions.Length)
            {
                sliceError(coordinates);
            }

            int sliceDimensionsLength = dimensions.Length - coordinates.Length;
            int[] sliceDimensions = (int[])Array.CreateInstance(typeof(int), sliceDimensionsLength);

            for (int i = coordinates.Length; i < dimensions.Length; i++)
            {
                sliceDimensions[i - coordinates.Length] = dimensions[i];
            }

            int[] elementCoordinates = new int[coordinates.Length + 1];
            Array.Copy(coordinates, elementCoordinates, coordinates.Length + 1);

            int sliceSize = 1;
            foreach (var item in sliceDimensions)
            {
                sliceSize *= item;
            }

            //int sliceSize = Arrays.stream(sliceDimensions).reduce((n, i)->n * i).getAsInt();

            int[] slice = new int[sliceSize];

            if (coordinates.Length + 1 == dimensions.Length)
            {
                // last slice 
                for (int i = 0; i < dimensions[coordinates.Length]; i++)
                {
                    elementCoordinates[coordinates.Length] = i;
                    slice[i] = computeIndex(elementCoordinates);
                    // Array.set(slice, i, computeIndex(elementCoordinates));
                }
            }
            else
            {
                for (int i = 0; i < dimensions[sliceDimensionsLength]; i++)
                {
                    elementCoordinates[coordinates.Length] = i;
                    int[] indexes = getSliceIndexes(elementCoordinates);
                    Array.Copy(indexes, 0, slice, i * indexes.Length, indexes.Length);
                    //System.arraycopy(indexes, 0, slice, i* indexes.length, indexes.length);
                }
            }

            return slice;
        }

        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector		the right side vector
         * @param results			the results array
         */
        public abstract void rightVecSumAtNZ(int[] inputVector, int[] results);

        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector       the right side vector
         * @param results           the results array
         */
        public abstract void rightVecSumAtNZ(int[] inputVector, int[] results, double stimulusThreshold);

        /**
         * Sets the value at the specified index.
         * 
         * @param index     the index the object will occupy
         * @param object    the object to be indexed.
         */


        public override AbstractFlatMatrix<int> set(int index, int value)
        {
            int[] coordinates = ComputeCoordinates(getNumDimensions(), getDimensionMultiples(), this.ModuleTopology.IsMajorOrdering, index);
            return set(value, coordinates);
        }

        /**
         * Sets the value to be indexed at the index
         * computed from the specified coordinates.
         * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
         * @param object        the object to be indexed.
         */

        public abstract AbstractSparseBinaryMatrix set(int value, params int[] coordinates);

        /**
         * Sets the specified values at the specified indexes.
         * 
         * @param indexes   indexes of the values to be set
         * @param values    the values to be indexed.
         * 
         * @return this {@code SparseMatrix} implementation
         */
        public AbstractSparseBinaryMatrix set(int[] indexes, int[] values)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                set(indexes[i], values[i]);
            }
            return this;
        }


        public Integer get(int[] coordinates)
        {
            return GetColumn(computeIndex(coordinates));
        }


#pragma warning disable IDE1006 // Naming Styles
       // public abstract Integer get(int index);
#pragma warning restore IDE1006 // Naming Styles



        /**
         * Sets the value at the specified index skipping the automatic
         * truth statistic tallying of the real method.
         * 
         * @param index     the index the object will occupy
         * @param object    the object to be indexed.
         */
        public abstract AbstractSparseBinaryMatrix setForTest(int index, int value);

        /**
         * Call This for TEST METHODS ONLY
         * Sets the specified values at the specified indexes.
         * 
         * @param indexes   indexes of the values to be set
         * @param values    the values to be indexed.
         * 
         * @return this {@code SparseMatrix} implementation
         */
        public AbstractSparseBinaryMatrix set(int[] indexes, int[] values, bool isTest)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (isTest) setForTest(indexes[i], values[i]);
                else set(indexes[i], values[i]);
            }
            return this;
        }

        /**
         * Returns the count of 1's set on the specified row.
         * @param index
         * @return
         */
        public int getTrueCount(int index)
        {
            return trueCounts[index];
        }

        /**
         * Sets the count of 1's on the specified row.
         * @param index
         * @param count
         */
        public void setTrueCount(int index, int count)
        {
            this.trueCounts[index] = count;
        }

        /**
         * Get the true counts for all outer indexes.
         * @return
         */
        public int[] getTrueCounts()
        {
            return trueCounts;
        }

        /**
         * Clears the true counts prior to a cycle where they're
         * being set
         */
        public virtual void clearStatistics(int row)
        {
            trueCounts[row] = 0;

            foreach (int index in getSliceIndexes(new int[] { row }))
            {
                set(index, 0);
            }
        }

        /**
         * Returns the int value at the index computed from the specified coordinates.
         * For example value {7, 21} is TRU if the column 7 is connected to input bit 21.
         * @param coordinates   the coordinates from which to retrieve the indexed object
         * @return  the indexed object
         */
        public new int getIntValue(params int[] coordinates)
        {
            return GetColumn(computeIndex(coordinates));
        }

        /**
         * Returns the T at the specified index.
         * 
         * @param index     the index of the T to return
         * @return  the T at the specified index.
         */
        //@Override
        public new int getIntValue(int index)
        {
            return GetColumn(index);
        }

        /**
         * Returns a sorted array of occupied indexes.
         * @return  a sorted array of occupied indexes.
         */
        //@Override
        public override int[] getSparseIndices()
        {
            List<int> indexes = new List<int>();
            //TIntList indexes = new TIntArrayList();
            for (int i = 0; i <= getMaxIndex(); i++)
            {
                if (GetColumn(i) > 0)
                {
                    indexes.Add(i);
                }
            }

            return indexes.ToArray();
        }

        /**
         * This {@code SparseBinaryMatrix} will contain the operation of or-ing
         * the inputMatrix with the contents of this matrix; returning this matrix
         * as the result.
         * 
         * @param inputMatrix   the matrix containing the "on" bits to or
         * @return  this matrix
         */
        public AbstractSparseBinaryMatrix or(AbstractSparseBinaryMatrix inputMatrix)
        {
            int[] mask = inputMatrix.getSparseIndices();
            int[] ones = new int[mask.Length];
            ArrayUtils.Fill(ones, 1);
            return set(mask, ones);
        }

        /**
         * This {@code SparseBinaryMatrix} will contain the operation of or-ing
         * the sparse list with the contents of this matrix; returning this matrix
         * as the result.
         * 
         * @param onBitIndexes  the matrix containing the "on" bits to or
         * @return  this matrix
         */
        public AbstractSparseBinaryMatrix or(List<int> onBitIndexes)
        {
            int[] ones = new int[onBitIndexes.Count];
            Utility.ArrayUtils.Fill(ones, 1);
            return set(onBitIndexes.ToArray(), ones);
        }

        /**
         * This {@code SparseBinaryMatrix} will contain the operation of or-ing
         * the sparse array with the contents of this matrix; returning this matrix
         * as the result.
         * 
         * @param onBitIndexes  the int array containing the "on" bits to or
         * @return  this matrix
         */
        public AbstractSparseBinaryMatrix or(int[] onBitIndexes)
        {
            int[] ones = new int[onBitIndexes.Length];
            Utility.ArrayUtils.Fill(ones, 1);
            return set(onBitIndexes, ones);
        }

        protected HashSet<int> getSparseSet()
        {
            return new HashSet<int>(getSparseIndices());
        }

        /**
         * Returns true if the on bits of the specified matrix are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool All(AbstractSparseBinaryMatrix matrix)
        {
            var sparseSet = getSparseSet();
            bool hasAll = matrix.getSparseIndices().All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().Contains(
            //    containsAll(matrix.getSparseIndices());
        }

        /**
         * Returns true if the on bits of the specified list are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool All(List<int> onBits)
        {
            var sparseSet = getSparseSet();
            bool hasAll = onBits.All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().containsAll(onBits);
        }

        /**
         * Returns true if the on bits of the specified array are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool All(int[] onBits)
        {
            var sparseSet = getSparseSet();
            bool hasAll = onBits.All(itm2 => sparseSet.Contains(itm2));
            return hasAll;
            //return getSparseSet().containsAll(onBits);
        }

        /**
         * Returns true if any of the on bits of the specified matrix are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool any(AbstractSparseBinaryMatrix matrix)
        {
            var keySet = getSparseSet();

            foreach (int i in matrix.getSparseIndices())
            {
                if (keySet.Contains(i)) return true;
            }

            return false;
        }

        /**
         * Returns true if any of the on bit indexes of the specified collection are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool any(HashSet<int> onBits)
        {
            var keySet = getSparseSet();

            foreach (var i in onBits)
            {
                if (keySet.Contains(i)) return true;
            }
            //for (TIntIterator i = onBits.iterator(); i.hasNext();)
            //{
            //    if (keySet.contains(i.next())) return true;
            //}
            return false;
        }

        /**
         * Returns true if any of the on bit indexes of the specified matrix are
         * matched by the on bits of this matrix. It is allowed that 
         * this matrix have more on bits than the specified matrix.
         * 
         * @param matrix
         * @return
         */
        public bool any(int[] onBits)
        {
            var keySet = getSparseSet();

            foreach (int i in onBits)
            {
                if (keySet.Contains(i)) return true;
            }
            return false;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#hashCode()
         */

   
        public override int GetHashCode()
        {
            const int prime = 31;
            int result = base.GetHashCode();
            result = prime * result + trueCounts.GetHashCode();
            return result;
        }



        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (!base.Equals(obj))
                return false;
            if ((obj.GetType() != this.GetType()))
                return false;
            AbstractSparseBinaryMatrix other = (AbstractSparseBinaryMatrix)obj;
            if (!Array.Equals(trueCounts, other.trueCounts))
                return false;
            return true;
        }

        /*    public  string  Serialize()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");

                foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
                {
                    sb.AppendLine();
                    sb.Append("<?");

                    sb.Append(property.Name);
                    sb.Append(": ");
                    if (property.GetIndexParameters().Length > 0)
                    {
                        sb.Append("Indexed Property cannot be used");
                    }
                    else
                    {
                        sb.Append(property.GetValue(this, null));
                    }


                    sb.Append(Environment.NewLine);
                    sb.Append("<?");
                }

                foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (field.FieldType.IsArray)
                    {
                        sb.AppendLine();
                        sb.Append("<?");
                        sb.Append(field.Name);
                        string array = JsonConvert.SerializeObject(field.GetValue(this));

                        sb.Append(array);

                        sb.AppendLine();
                        sb.Append("<?");

                    }

                    else
                    {
                        sb.AppendLine();
                        sb.Append("<?");

                        sb.Append(field.Name);
                        sb.Append(": ");

                        sb.Append(field.GetValue(this));

                        sb.Append(Environment.NewLine);
                        sb.Append("<?");
                    }
                }

                sb.AppendLine("}");

                return sb.ToString();


            }
            */

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
