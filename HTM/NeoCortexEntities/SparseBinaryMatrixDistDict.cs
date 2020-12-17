#if USE_AKKA

using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Reflection;

namespace NeoCortexApi.Entities
{
    /**
 * Implementation of a sparse matrix which contains binary integer
 * values only.
 * 
 * @author cogmission
 *
 */
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class SparseBinaryMatrix : AbstractSparseBinaryMatrix
    {
        /// <summary>
        /// Holds the matrix with connections between columns and inputs.
        /// </summary>
        public IDistributedArray backingArray;


        public SparseBinaryMatrix()
        {

        }

        /**
         * Constructs a new {@code SparseBinaryMatrix} with the specified
         * dimensions (defaults to row major ordering)
         * 
         * @param dimensions    each indexed value is a dimension size
         */
        public SparseBinaryMatrix(int[] dimensions) : this(dimensions, false)
        {

        }

        /**
         * Constructs a new {@code SparseBinaryMatrix} with the specified dimensions,
         * allowing the specification of column major ordering if desired. 
         * (defaults to row major ordering)
         * 
         * @param dimensions                each indexed value is a dimension size
         * @param useColumnMajorOrdering    if true, indicates column first iteration, otherwise
         *                                  row first iteration is the default (if false).
         */
        public SparseBinaryMatrix(int[] dimensions, bool useColumnMajorOrdering, IDistributedArray distArray = null) : base(dimensions, useColumnMajorOrdering)
        {
            // We  create here a simple array on a single node.
            if (distArray == null)
                this.backingArray = new InMemoryArray(1, typeof(int), dimensions);
            else
                this.backingArray = distArray;
        }

        /**
         * Sets the value on specified call in array and automattically calculates number of '1' bits as TrueCount.
         * Called during mutation operations to simultaneously set the value
         * on the backing array dynamically.
         * @param val
         * @param coordinates
         */
        //private void back(int val, int[] coordinates)
        //{
        //    //update true counts
        //    this.backingArray.SetValue(val, coordinates);

        //    var aggVal = this.backingArray.AggregateArray(coordinates[0]);

        //    setTrueCount(coordinates[0], aggVal);
        //    // setTrueCount(coordinates[0], DistributedArrayHelpers.aggregateArray(((Object[])this.backingArray)[coordinates[0]]));
        //}

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
        // @Override

        public override Object getSlice(params int[] coordinates)
        {
            //Object slice = DistributedArrayHelpers.getValue(this.backingArray, coordinates);
            Object slice;
            if (coordinates.Length == 1)
                slice = getRow<int>(this.backingArray, coordinates[0]);

            // DistributedArrayHelpers.GetRow<int>((int[,])this.backingArray, coordinates[0]);
            //else if (coordinates.Length == 1)
            //    slice = ((int[])this.backingArray)[coordinates[0]];
            else
                throw new ArgumentException();

            //Ensure return value is of type Array
            if (!slice.GetType().IsArray)
            {
                sliceError(coordinates);
            }

            return slice;
        }

        /// <summary>
        /// Gets the access to a row inside of multidimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        //public static T[] GetRow<T>(this T[,] array, int row)
        private static T[] getRow<T>(IDistributedArray array, int row)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            for (int i = 0; i < cols; i++)
            {
                result[i] = (T)array[row, i];
            }

            return result;
        }
        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector		the right side vector
         * @param results			the results array
         */
        public override void rightVecSumAtNZ(int[] inputVector, int[] results)
        {
            for (int i = 0; i < this.ModuleTopology.Dimensions[0]; i++)
            {
                int[] slice = (int[])(this.ModuleTopology.Dimensions.Length > 1 ? getSlice(i) : backingArray);
                for (int j = 0; j < slice.Length; j++)
                {
                    results[i] += (inputVector[j] * slice[j]);
                }
            }
        }

        /**
         * Fills the specified results array with the result of the 
         * matrix vector multiplication.
         * 
         * @param inputVector       the right side vector
         * @param results           the re\sults array
         */
        public override void rightVecSumAtNZ(int[] inputVector, int[] results, double stimulusThreshold)
        {
            for (int colIndx = 0; colIndx < this.ModuleTopology.Dimensions[0]; colIndx++)
            {
                // Gets the synapse mapping between column-i with input vector.
                int[] slice = (int[])(this.ModuleTopology.Dimensions.Length > 1 ? getSlice(colIndx) : backingArray);

                // Go through all connections (synapses) between column and input vector.
                for (int inpBit = 0; inpBit < slice.Length; inpBit++)
                {
                    //Debug.WriteLine($"Slice {i} - {String.Join("","", slice )}");

                    // Result (overlapp) is 1 if 
                    results[colIndx] += (inputVector[inpBit] * slice[inpBit]);
                    if (inpBit == slice.Length - 1)
                    {
                        // If the overlap (num of connected synapses to TRUE input) is less than stimulusThreshold then we set result on 0.
                        // If the overlap (num of connected synapses to TRUE input) is greather than stimulusThreshold then result remains as calculated.
                        // This ensures that only overlaps are calculated, which are over the stimulusThreshold. All less than stimulusThreshold are set on 0.
                        results[colIndx] -= results[colIndx] < stimulusThreshold ? results[colIndx] : 0;
                    }
                }
            }
        }


        //public AbstractSparseBinaryMatrix set(int index, Object value)
        //{
        //    set(index, ((Integer)value).Value);
        //    return this;
        //}

        public override AbstractFlatMatrix<int> set(int index, int value)
        {
            set(index, value);
            return (AbstractFlatMatrix<int>)this;
        }


        /**
         * Sets the value to be indexed at the index
         * computed from the specified coordinates.
         * @param coordinates   the row major coordinates [outer --> ,...,..., inner]
         * @param object        the object to be indexed.
         */
        //@Override
        public override AbstractSparseBinaryMatrix set(int value, int[] coordinates)
        {
            //back(value, coordinates);

            //update true counts
            this.backingArray.SetValue(value, coordinates);

            var aggVal = this.backingArray.AggregateArray(coordinates[0]);

            setTrueCount(coordinates[0], aggVal);

            return this;
        }

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

        /**
         * Clears the true counts prior to a cycle where they're
         * being set
         */
        public override void clearStatistics(int row)
        {
            if (backingArray.Rank != 2)
                throw new InvalidOperationException("Currently supported 2D arrays only");

            backingArray.SetRowValuesTo(row, 0);

            this.setTrueCount(row, 0);
        }




        //  @Override
        public override AbstractSparseBinaryMatrix setForTest(int index, int value)
        {
            this.backingArray.SetValue(value, ComputeCoordinates(getNumDimensions(), getDimensionMultiples(), ModuleTopology.IsMajorOrdering, index));

            //DistributedArrayHelpers.setValue(this.backingArray, value,
            //    ComputeCoordinates(getNumDimensions(), getDimensionMultiples(), ModuleTopology.IsMajorOrdering, index));
            return this;
        }


        //public override Integer Get(int index)
        //{
        //    return (Integer)get(index);
        //}


        // @Override
        public override int GetColumn(int index)
        {
            int[] coordinates = ComputeCoordinates(getNumDimensions(), getDimensionMultiples(), this.ModuleTopology.IsMajorOrdering, index);
            if (coordinates.Length == 1)
            {
                return (Int32)backingArray.GetValue(index);
            }
            else
                return (Int32)backingArray.GetValue(coordinates);
        }

        public override AbstractFlatMatrix<int> set(List<KeyPair> updatingValues)
        {
            throw new NotImplementedException();
        }

        public override ICollection<KeyPair> GetObjects(int[] indexes)
        {
            throw new NotImplementedException();
        }

        /*     public StringBuilder Serialize()
             {
                 return new StringBuilder(); // TODO.
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
#endif