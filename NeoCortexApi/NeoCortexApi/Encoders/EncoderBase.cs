using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public class EncoderBase
    {

    }

    public abstract class EncoderBase<T> : EncoderBase
    {

        /**
         * <pre>
         * An encoder takes a value and encodes it with a partial sparse representation
         * of bits.  The Encoder base implements:
         * - encode() - returns an array encoding the input; syntactic sugar
         *   on top of encodeIntoArray. If pprint, prints the encoding to the terminal
         * - pprintHeader() -- prints a header describing the encoding to the terminal
         * - pprint() -- prints an encoding to the terminal
         *
         * Methods/properties that must be implemented by subclasses:
         * - getDecoderOutputFieldTypes()   --  must be implemented by leaf encoders; returns
         *                                      [`nupic.data.fieldmeta.FieldMetaType.XXXXX`]
         *                                      (e.g., [nupic.data.fieldmetaFieldMetaType.float])
         * - getWidth()                     --  returns the output width, in bits
         * - encodeIntoArray()              --  encodes input and puts the encoded value into the output array,
         *                                      which is a 1-D array of length returned by getWidth()
         * - getDescription()               --  returns a list of (name, offset) pairs describing the
         *                                      encoded output
         * </pre>
         *
         * <P>
         * Typical usage is as follows:
         * <PRE>
         * CategoryEncoder.Builder builder =  ((CategoryEncoder.Builder)CategoryEncoder.builder())
         *      .w(3)
         *      .radius(0.0)
         *      .minVal(0.0)
         *      .maxVal(8.0)
         *      .periodic(false)
         *      .forced(true);
         *
         * CategoryEncoder encoder = builder.build();
         *
         * <b>Above values are <i>not</i> an example of "sane" values.</b>
         *
         * </PRE>
         * @author Numenta
         * @author David Ray
         */


        /** Value used to represent no data */
        //public static readonly double SENTINEL_VALUE_FOR_MISSING_DATA = Double.NaN;
        protected List<Tuple<string, int>> description = new List<Tuple<string, int>>();

        protected int n = 0;

        /** The number of bits that are set to encode a single value - the
         * "width" of the output signal
         */
        protected int w = 0;

        /** number of bits in the representation (must be >= w) */
        protected int m_NumOfBits = 0;

        /** the half width value */
        protected int halfWidth;
        /**
         * inputs separated by more than, or equal to this distance will have non-overlapping
         * representations
         */
        protected double radius = 0;

        /** inputs separated by more than, or equal to this distance will have different representations */
        protected double resolution = 0;
        /**
         * If true, then the input value "wraps around" such that minval = maxval
         * For a periodic value, the input must be strictly less than maxval,
         * otherwise maxval is a true upper bound.
         */
        protected bool periodic = true;
        /** The minimum value of the input signal.  */
        protected double minVal = 0;
        /** The maximum value of the input signal. */
        protected double maxVal = 0;
        /** if true, non-periodic inputs smaller than minval or greater
                than maxval will be clipped to minval/maxval */
        protected bool clipInput;
        /** if true, skip some safety checks (for compatibility reasons), default false */
        protected bool isForced;
        /** Encoder name - an optional string which will become part of the description */
        protected String m_Name = "";
        protected int padding;
        protected int nInternal;
        protected double rangeInternal;
        protected double range;
        protected bool encLearningEnabled;
        protected List<FieldMetaType> flattenedFieldTypeList;
        protected Dictionary<Dictionary<string, int>, List<FieldMetaType>> decoderFieldTypes;
        /**
         * This matrix is used for the topDownCompute. We build it the first time
         * topDownCompute is called
         */
        protected SparseObjectMatrix<int[]> topDownMapping;
        protected double[] topDownValues;
        protected List<object> bucketValues;
        protected Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>> encoders;
        protected List<String> scalarNames;


        protected EncoderBase() { }

        #region Properties

        public int N { get => this.n; set => this.n = value; }

        /**
         * Returns w
         * @return
         */
        ///////////////////////////////////////////////////////////
        /**
         * Sets the "w" or width of the output signal
         * <em>Restriction:</em> w must be odd to avoid centering problems.
         * @param w
         */
        public int W { get => w; set => this.w = value; }


        /**
* For non-periodic inputs, padding is the number of bits "outside" the range,
* on each side. I.e. the representation of minval is centered on some bit, and
* there are "padding" bits to the left of that centered bit; similarly with
* bits to the right of the center bit of maxval
*
* @param padding
*/
        public int Padding { get => padding; set => this.padding = value; }

        /**
       * Returns the radius
       * @return
       */
        /**
 * inputs separated by more than, or equal to this distance will have non-overlapping
 * representations
 *
 * @param radius
 */
        public double Radius { get => radius; set => this.radius = value; }


        /**
         * Returns the range internal value
         * @return
         */
        /**
 * Sets rangeInternal
 * @param r
 */
        public double RangeInternal { get => rangeInternal; set => this.rangeInternal = value; }

        /**
         * Returns the range
         * @return
         */
        /**
 * Sets the range
 * @param range
 */
        public double Range { get => range; set => this.range = value; }

        /**
         * nInternal represents the output area excluding the possible padding on each
         * side
         * @return
         */
        /**
 * nInternal represents the output area excluding the possible padding on each side
 *
 * @param n
 */
        public int NInternal { get => nInternal; set => this.nInternal = value; }


        /**
         * Returns the top down range of values
         * @return
         */
        /**
 * Range of values.
 * @param values
 */
        public double[] TopDownValues { get => topDownValues; set => this.topDownValues = value; }


        /**
         * Returns n
         * @return
         */
        /**
 * The number of bits in the output. Must be greater than or equal to w
 * @param n
 */
        public int NumOfBits { get => m_NumOfBits; set => this.m_NumOfBits = value; }
        /**
       * Returns minval
       * @return
       */
        /**
 * The minimum value of the input signal.
 * @param minVal
 */
        public double MinVal { get => minVal; set => this.minVal = value; }


        /**
         * Returns maxval
         * @return
         */
        /**
 * The maximum value of the input signal.
 * @param maxVal
 */
        public double MaxVal { get => maxVal; set => this.maxVal = value; }


        /**
         * Return the half width value.
         * @return
         */

        /**
         * Half the width
         * @param hw
         */
        public int HalfWidth { get => halfWidth; set => this.halfWidth = value; }


        /**
         * inputs separated by more than, or equal to this distance will have different
         * representations
         *
         * @param resolution
         */
        public double Resolution { get => resolution; set => this.resolution = value; }



        /**
         * Returns the clip input flag
         * @return
         */
        /**
     * If true, non-periodic inputs smaller than minval or greater
     * than maxval will be clipped to minval/maxval
     * @param b
     */
        public bool ClipInput { get => this.clipInput; set => this.clipInput = value; }

        /**
         * Returns the periodic flag
         * @return
         */
        /**
     * If true, then the input value "wraps around" such that minval = maxval
     * For a periodic value, the input must be strictly less than maxval,
     * otherwise maxval is a true upper bound.
     *
     * @param b
     */
        public bool Periodic { get => periodic; set => this.periodic = value; }

        /**
         * Returns the forced flag
         * @return
         */
        /**
     * If true, skip some safety checks (for compatibility reasons), default false
     * @param b
     */
        public bool IsForced { get => isForced; set => this.isForced = value; }



        /// <summary>
        /// The number of bits in the output. Must be greater than or equal to w
        /// </summary>
        public string Name { get => m_Name; set => this.m_Name = value; }

        /**
 * Returns the names of the fields
 *
 * @return	the list of names
 */
        /**
 * Sets the names of the fields
 *
 * @param names	the list of names
 */
        public List<String> ScalarNames { get => scalarNames; set => this.scalarNames = value; }

        /**
      * Returns the list of all field types of a parent {@link Encoder} and all
      * leaf encoders flattened in a linear list which does not retain any parent
      * child relationship information.
      *
      * @return	List<FieldMetaType>
      */
        /**
 * Sets the list of flattened {@link FieldMetaType}s
 *
 * @param l		list of {@link FieldMetaType}s
 */
        public List<FieldMetaType> FlattenedFieldTypeList { get => flattenedFieldTypeList; set => this.flattenedFieldTypeList = value; }
        #endregion

        /**
         * This matrix is used for the topDownCompute. We build it the first time
         * topDownCompute is called
         *
         * @param sm
         */
        public void setTopDownMapping(SparseObjectMatrix<int[]> sm)
        {
            this.topDownMapping = sm;
        }

        /**
         * Returns the resolution
         * @return
         */




        /**
         * Adds a the specified {@link Encoder} to the list of the specified
         * parent's {@code Encoder}s.
         *
         * @param parent	the parent Encoder
         * @param name		Name of the {@link Encoder}
         * @param e			the {@code Encoder}
         * @param offset	the offset of the encoded output the specified encoder
         * 					was used to encode.
         */
        public void addEncoder(EncoderBase<T> parent, String name, EncoderBase<T> child, int offset)
        {
            if (encoders == null)
            {
                encoders = new Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>>();
            }

            EncoderTuple<T> key = getEncoderTuple(parent);
            // Insert a new Tuple for the parent if not yet added.
            if (key == null)
            {
                encoders.Add(key = new EncoderTuple<T> { Name = String.Empty, Encoder = this, Offset = 0 },
                new List<EncoderTuple<T>>());
            }

            List<EncoderTuple<T>> childEncoders = null;
            if ((childEncoders = encoders[key]) == null)
            {
                encoders.Add(key, childEncoders = new List<EncoderTuple<T>>());
            }
            childEncoders.Add(new EncoderTuple<T>() { Name = name, Encoder = child, Offset = offset });
        }

        /**
         * Returns the {@link Tuple} containing the specified {@link Encoder}
         * @param e		the Encoder the return value should contain
         * @return		the {@link Tuple} containing the specified {@link Encoder}
         */
        public EncoderTuple<T> getEncoderTuple(EncoderBase<T> encoder)
        {
            if (encoders == null)
            {
                //encoders = new LinkedHashMap<EncoderTuple, List<EncoderTuple>>();
                encoders = new Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>>();
            }

            foreach (var tpl in encoders)
            {
                if (tpl.Value.Equals(encoder))
                {
                    return tpl.Key;
                }
            }
            return null;
        }

        /**
         * Returns the list of child {@link Encoder} {@link Tuple}s
         * corresponding to the specified {@code Encoder}
         *
         * @param e		the parent {@link Encoder} whose child Encoder Tuples are being returned
         * @return		the list of child {@link Encoder} {@link Tuple}s
         */
        public List<EncoderTuple<T>> getEncoders(EncoderBase<T> e)
        {
            return getEncoders()[e.getEncoderTuple(e)];
        }

        /**
         * Returns the list of {@link Encoder}s
         * @return
         */
        public Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>> getEncoders()
        {
            if (encoders == null)
            {
                encoders = new Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>>();
            }
            return encoders;
        }

        /**
         * Sets the encoder flag indicating whether learning is enabled.
         *
         * @param	encLearningEnabled	true if learning is enabled, false if not
         */
        public void setLearningEnabled(bool encLearningEnabled)
        {
            this.encLearningEnabled = encLearningEnabled;
        }

        /**
         * Returns a flag indicating whether encoder learning is enabled.
         */
        public bool isEncoderLearningEnabled()
        {
            return encLearningEnabled;
        }

        /**
         * Returns the list of all field types of the specified {@link Encoder}.
         *
         * @return	List<FieldMetaType>
         */
        //public List<FieldMetaType> getFlattenedFieldTypeList(EncoderBase<T> e)
        //{
        //    if (decoderFieldTypes == null)
        //    {
        //        //Dictionary<Dictionary<string, int>, List<FieldMetaType>>
        //        decoderFieldTypes = new Dictionary<Dictionary<string, int>, List<FieldMetaType>>();
        //    }

        //    EncoderTuple key = getEncoderTuple(e);
        //    List<FieldMetaType> fieldTypes = null;
        //    if ((fieldTypes = decoderFieldTypes.get(key)) == null)
        //    {
        //        decoderFieldTypes.Add(key, fieldTypes = new List<FieldMetaType>());
        //    }
        //    return fieldTypes;
        //}


        ///////////////////////////////////////////////////////////


        /**
         * Should return the output width, in bits.
         */
        public abstract int getWidth();

        /**
         * Returns true if the underlying encoder works on deltas
         */
        public abstract bool isDelta();

        /**
         * Encodes inputData and puts the encoded value into the output array,
         * which is a 1-D array of length returned by {@link #getW()}.
         *
         * Note: The output array is reused, so clear it before updating it.
         * @param inputData Data to encode. This should be validated by the encoder.
         * @param output 1-D array of same length returned by {@link #getW()}
         *
         * @return
         */
        public abstract int[] encodeIntoArray(T inputData);

        /**
         * Set whether learning is enabled.
         * @param 	learningEnabled		flag indicating whether learning is enabled
         */
        public void setLearning(bool learningEnabled)
        {
            setLearningEnabled(learningEnabled);
        }

        /**
         * This method is called by the model to set the statistics like min and
         * max for the underlying encoders if this information is available.
         * @param	fieldName			fieldName name of the field this encoder is encoding, provided by
         *     							{@link MultiEncoder}
         * @param	fieldStatistics		fieldStatistics dictionary of dictionaries with the first level being
         *     							the fieldName and the second index the statistic ie:
         *     							fieldStatistics['pounds']['min']
         */
        // public void setFieldStats(String fieldName, Map<String, Double> fieldStatistics) { }

        /**
         * Convenience wrapper for {@link #encodeIntoArray(double, int[])}
         * @param inputData		the input scalar
         *
         * @return	an array with the encoded representation of inputData
         */
        public int[] Encode(T inputData)
        {
            //int[] output = new int[NumOfBits];
            int[] output = encodeIntoArray(inputData);
            return output;
        }

        /**
         * Return the field names for each of the scalar values returned by
         * .
         * @param parentFieldName	parentFieldName The name of the encoder which is our parent. This name
         *     						is prefixed to each of the field names within this encoder to form the
         *      					keys of the dict() in the retval.
         *
         * @return
         */


        public List<String> getScalarNames(String parentFieldName)
        {
            List<String> names = new List<String>();
            if (getEncoders() != null)
            {
                List<EncoderTuple<T>> encoders = getEncoders(this);
                foreach (var tuple in encoders)
                {
                    List<String> subNames = ((EncoderBase<T>)tuple.Encoder).getScalarNames(tuple.Name);

                    List<String> hierarchicalNames = new List<String>();
                    if (parentFieldName != null)
                    {
                        foreach (String name in subNames)
                        {
                            hierarchicalNames.Add($"{parentFieldName}.{name}");
                        }
                    }
                    names.AddRange(hierarchicalNames);
                }
            }
            else
            {
                if (parentFieldName != null)
                {
                    names.Add(parentFieldName);
                }
                else
                {
                    names.Add((String)getEncoderTuple(this).Name);
                }
            }

            return names;
        }

        /**
         * Returns a sequence of field types corresponding to the elements in the
         * decoded output field array.  The types are defined by {@link FieldMetaType}
         *
         * @return
         */
        // @SuppressWarnings("unchecked")

        public List<FieldMetaType> getDecoderOutputFieldTypes()
        {
            if (FlattenedFieldTypeList != null)
            {
                return new List<FieldMetaType>(FlattenedFieldTypeList);
            }

            List<FieldMetaType> retVal = new List<FieldMetaType>();
            foreach (var t in getEncoders(this))
            {
                List<FieldMetaType> subTypes = ((EncoderBase<T>)t.Encoder).getDecoderOutputFieldTypes();
                retVal.AddRange(subTypes);
            }
            FlattenedFieldTypeList = retVal;
            return retVal;
        }

        /**
         * Gets the value of a given field from the input record
         * @param inputObject	input object
         * @param fieldName		the name of the field containing the input object.
         * @return
         */
        //       public Object getInputValue(Object inputObject, String fieldName)
        //       {
        //           if (Map.class.isAssignableFrom(inputObject.getClass())) {

        //           Map map = (Map)inputObject;
        //		if(!map.containsKey(fieldName)) {
        //			throw new IllegalArgumentException("Unknown field name " + fieldName +
        //				" known fields are: " + map.keySet() + ". ");
        //   }
        //		return map.get(fieldName);
        //	}
        //	return null;
        //}

        /**
         * Returns an {@link TDoubleList} containing the sub-field scalar value(s) for
         * each sub-field of the inputData. To get the associated field names for each of
         * the scalar values, call getScalarNames().
         *
         * For a simple scalar encoder, the scalar value is simply the input unmodified.
         * For category encoders, it is the scalar representing the category string
         * that is passed in.
         *
         * TODO This is not correct for DateEncoder:
         *
         * For the datetime encoder, the scalar value is the
         * the number of seconds since epoch.
         *
         * The intent of the scalar representation of a sub-field is to provide a
         * baseline for measuring error differences. You can compare the scalar value
         * of the inputData with the scalar value returned from topDownCompute() on a
         * top-down representation to evaluate prediction accuracy, for example.
         *
         * @param <S>  the specifically typed input object
         *
         * @return
         */
        public List<double> getScalars(double inputData)
        {
            List<double> retVals = new List<double>();

            List<EncoderTuple<T>> encoders = getEncoders(this);
            if (encoders != null)
            {
                foreach (EncoderTuple<T> t in encoders)
                {
                    List<double> values = t.Encoder.getScalars(inputData);
                    retVals.AddRange(values);
                }
            }

            return retVals;
        }

        /**
         * Returns the input in the same format as is returned by topDownCompute().
         * For most encoder types, this is the same as the input data.
         * For instance, for scalar and category types, this corresponds to the numeric
         * and string values, respectively, from the inputs. For datetime encoders, this
         * returns the list of scalars for each of the sub-fields (timeOfDay, dayOfWeek, etc.)
         *
         * This method is essentially the same as getScalars() except that it returns
         * strings
         * @param <S> 	The input data in the format it is received from the data source
         *
         * @return A list of values, in the same format and in the same order as they
         * are returned by topDownCompute.
         *
         * @return	list of encoded values in String form
         */
        public List<String> getEncodedValues<TINP>(TINP inputData)
        {
            List<String> retVals = new List<String>();
            Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>> encoders = getEncoders();
            if (encoders != null && encoders.Count > 0)
            {
                foreach (EncoderTuple<T> t in encoders.Keys)
                {
                    retVals.AddRange(t.Encoder.getEncodedValues(inputData));
                }
            }
            else
            {
                retVals.Add(inputData.ToString());
            }

            return retVals;
        }

        /**
         * Returns an array containing the sub-field bucket indices for
         * each sub-field of the inputData. To get the associated field names for each of
         * the buckets, call getScalarNames().
         * @param  	input 	The data from the source. This is typically a object with members.
         *
         * @return 	array of bucket indices
         */
        public int[] getBucketIndices(String input)
        {
            List<int> l = new List<int>();
            Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>> encoders = getEncoders();
            if (encoders != null && encoders.Count > 0)
            {
                foreach (EncoderTuple<T> t in encoders.Keys)
                {
                    l.AddRange(t.Encoder.getBucketIndices(input));
                }
            }
            else
            {
                throw new InvalidOperationException("Should be implemented in base classes that are not " +
                    "containers for other encoders");
            }
            return l.ToArray();
        }

        /**
         * Returns an array containing the sub-field bucket indices for
         * each sub-field of the inputData. To get the associated field names for each of
         * the buckets, call getScalarNames().
         * @param  	input 	The data from the source. This is typically a object with members.
         *
         * @return 	array of bucket indices
         */
        public int[] getBucketIndices(double input)
        {
            List<int> l = new List<int>();
            Dictionary<EncoderTuple<T>, List<EncoderTuple<T>>> encoders = getEncoders();
            if (encoders != null && encoders.Count > 0)
            {
                foreach (EncoderTuple<T> t in encoders.Keys)
                {
                    l.AddRange(t.Encoder.getBucketIndices(input));
                }
            }
            else
            {
                throw new InvalidOperationException("Should be implemented in base classes that are not " +
                    "containers for other encoders");
            }
            return l.ToArray();
        }

        /**
         * Return a pretty print string representing the return values from
         * getScalars and getScalarNames().
         * @param scalarValues 	input values to encode to string
         * @param scalarNames 	optional input of scalar names to convert. If None, gets
         *                  	scalar names from getScalarNames()
         *
         * @return string representation of scalar values
         */
        public String scalarsToStr<S>(List<S> scalarValues, List<String> scalarNames)
        {
            if (scalarNames == null || scalarNames == null || scalarNames.Count == 0)
            {
                scalarNames = getScalarNames("");
            }

            StringBuilder desc = new StringBuilder();
            foreach (var t in ArrayUtils.Zip(scalarNames, scalarValues))
            {
                if (desc.Length > 0)
                {
                    desc.Append($"{t.Item1}:{t.Item2}");
                }
                else
                {
                    desc.Append($"{t.Item1}:{t.Item2}");
                }
            }
            return desc.ToString();
        }

        /**
         * This returns a list of tuples, each containing (name, offset).
         * The 'name' is a string description of each sub-field, and offset is the bit
         * offset of the sub-field for that encoder.
         *
         * For now, only the 'multi' and 'date' encoders have multiple (name, offset)
         * pairs. All other encoders have a single pair, where the offset is 0.
         *
         * @return		list of tuples, each containing (name, offset)
         */
        public List<Tuple<string, int>> getDescription()
        {
            return description;
        }


        /**
         * Return a description of the given bit in the encoded output.
         * This will include the field name and the offset within the field.
         * @param bitOffset  	Offset of the bit to get the description of
         * @param formatted     If True, the bitOffset is w.r.t. formatted output,
         *                     	which includes separators
         *
         * @return tuple(fieldName, offsetWithinField)
         */
        public Tuple<string, int> encodedBitDescription(int bitOffset, bool formatted)
        {
            //Find which field it's in
            List<Tuple<string, int>> description = getDescription();

            String prevFieldName = null;
            int prevFieldOffset = -1;
            int offset = -1;
            for (int i = 0; i < description.Count; i++)
            {
                var keyPair = description[i];//(name, offset)
                if (formatted)
                {
                    offset = keyPair.Item2 + 1;
                    if (bitOffset == offset - 1)
                    {
                        prevFieldName = "separator";
                        prevFieldOffset = bitOffset;
                    }
                }
                if (bitOffset < offset) break;
            }
            // Return the field name and offset within the field
            // return (fieldName, bitOffset - fieldOffset)
            int width = formatted ? getDisplayWidth() : getWidth();

            if (prevFieldOffset == -1 || bitOffset > getWidth())
            {
                throw new InvalidOperationException($"Bit is outside of allowable range: [0 - {width}");
            }
            return new Tuple<string, int>(prevFieldName, bitOffset - prevFieldOffset);
        }

        /**
         * Pretty-print a header that labels the sub-fields of the encoded
         * output. This can be used in conjunction with {@link #pprint(int[], String)}.
         * @param prefix
         */
        public void pprintHeader(String prefix)
        {
            //LOGGER.info(prefix == null ? "" : prefix);

            List<Tuple<string, int>> description = getDescription();
            description.Add(new Tuple<string, int>("end", getWidth()));

            int len = description.Count - 1;
            for (int i = 0; i < len; i++)
            {
                String name = (String)description[i].Item1;
                int width = (int)description[i + 1].Item2;

                String formatStr = $"{width}";
                StringBuilder pname = new StringBuilder(name);
                if (name.Length > width)
                    pname.Length = width;

                // LOGGER.info(String.format(formatStr, pname));
            }

            len = getWidth() + (description.Count - 1) * 3 - 1;
            StringBuilder hyphens = new StringBuilder();
            for (int i = 0; i < len; i++)
                hyphens.Append("-");

            Debug.WriteLine(hyphens);
        }

        /**
         * Pretty-print the encoded output using ascii art.
         * @param output
         * @param prefix
         */
        //public void pprint(int[] output, String prefix)
        //{
        //    LOGGER.info(prefix == null ? "" : prefix);

        //    List<Tuple> description = getDescription();
        //    description.add(new Tuple("end", getWidth()));

        //    int len = description.size() - 1;
        //    for (int i = 0; i < len; i++)
        //    {
        //        int offset = (int)description.get(i).get(1);
        //        int nextOffset = (int)description.get(i + 1).get(1);

        //        LOGGER.info(
        //                String.format("%s |",
        //                        ArrayUtils.bitsToString(
        //                                ArrayUtils.sub(output, ArrayUtils.range(offset, nextOffset))
        //                        )
        //                )
        //        );
        //    }
        //}

        /**
         * Takes an encoded output and does its best to work backwards and generate
         * the input that would have generated it.
         *
         * In cases where the encoded output contains more ON bits than an input
         * would have generated, this routine will return one or more ranges of inputs
         * which, if their encoded outputs were ORed together, would produce the
         * target output. This behavior makes this method suitable for doing things
         * like generating a description of a learned coincidence in the SP, which
         * in many cases might be a union of one or more inputs.
         *
         * If instead, you want to figure the *most likely* single input scalar value
         * that would have generated a specific encoded output, use the topDownCompute()
         * method.
         *
         * If you want to pretty print the return value from this method, use the
         * decodedToStr() method.
         *
         *************
         * OUTPUT EXPLAINED:
         *
         * fieldsMap is a {@link Map} where the keys represent field names
         * (only 1 if this is a simple encoder, > 1 if this is a multi
         * or date encoder) and the values are the result of decoding each
         * field. If there are  no bits in encoded that would have been
         * generated by a field, it won't be present in the Map. The
         * key of each entry in the dict is formed by joining the passed in
         * parentFieldName with the child encoder name using a '.'.
         *
         * Each 'value' in fieldsMap consists of a {@link Tuple} of (ranges, desc),
         * where ranges is a list of one or more {@link MinMax} ranges of
         * input that would generate bits in the encoded output and 'desc'
         * is a comma-separated pretty print description of the ranges.
         * For encoders like the category encoder, the 'desc' will contain
         * the category names that correspond to the scalar values included
         * in the ranges.
         *
         * The fieldOrder is a list of the keys from fieldsMap, in the
         * same order as the fields appear in the encoded output.
         *
         * Example retvals for a scalar encoder:
         *
         *   {'amount':  ( [[1,3], [7,10]], '1-3, 7-10' )}
         *   {'amount':  ( [[2.5,2.5]],     '2.5'       )}
         *
         * Example retval for a category encoder:
         *
         *   {'country': ( [[1,1], [5,6]], 'US, GB, ES' )}
         *
         * Example retval for a multi encoder:
         *
         *   {'amount':  ( [[2.5,2.5]],     '2.5'       ),
         *   'country': ( [[1,1], [5,6]],  'US, GB, ES' )}
         * @param encoded      		The encoded output that you want decode
         * @param parentFieldName 	The name of the encoder which is our parent. This name
         *      					is prefixed to each of the field names within this encoder to form the
         *    						keys of the {@link Map} returned.
         *
         * @returns Tuple(fieldsMap, fieldOrder)
         */


        public Tuple<Dictionary<String, object>, List<String>> decode(int[] encoded, String parentFieldName)
        {
            Dictionary<string, object> fieldsMap = new Dictionary<string, object>();
            List<String> fieldsOrder = new List<String>();

            String parentName = parentFieldName == null || parentFieldName.Length == 0 || parentFieldName == null ?
                this.m_Name : $"{parentFieldName}.{this.m_Name}";

            List<EncoderTuple<T>> encoders = getEncoders(this);

            int len = encoders.Count;

            for (int i = 0; i < len; i++)
            {
                var threeFieldsTuple = encoders[i];
                int nextOffset = 0;
                if (i < len - 1)
                {
                    nextOffset = (Integer)encoders[i + 1].Offset;
                }
                else
                {
                    nextOffset = W;
                }

                int[] fieldOutput = ArrayUtils.sub(encoded, ArrayUtils.range((Integer)threeFieldsTuple.Offset, nextOffset));

                var result = ((EncoderBase<T>)threeFieldsTuple.Encoder).decode(fieldOutput, parentName);

                fieldsMap.AddRange<String, object>((Dictionary<String, object>)result.Item1);
                fieldsOrder.AddRange((List<String>)result.Item2);
            }

            return new Tuple<Dictionary<String, object>, List<String>>(fieldsMap, fieldsOrder);
        }

        /**
         * Return a pretty print string representing the return value from decode().
         *
         * @param decodeResults
         * @return
         */


        public String decodedToStr(Tuple<Dictionary<String, object>, List<String>> decodeResults)
        {
            StringBuilder desc = new StringBuilder();
            Dictionary<String, object> fieldsDict = decodeResults.Item1;
            List<String> fieldsOrder = (List<String>)decodeResults.Item2;

            foreach (String fieldName in fieldsOrder)
            {
                var ranges = fieldsDict[fieldName];
                if (desc.Length > 0)
                {
                    desc.Append(", ").Append(fieldName).Append(":");
                }
                else
                {
                    desc.Append(fieldName).Append(":");
                }
                desc.Append("[").Append(ranges).Append("]");
            }
            return desc.ToString();
        }


        /**
         * Returns a list of items, one for each bucket defined by this encoder.
         * Each item is the value assigned to that bucket, this is the same as the
         * EncoderResult.value that would be returned by getBucketInfo() for that
         * bucket and is in the same format as the input that would be passed to
         * encode().
         *
         * This call is faster than calling getBucketInfo() on each bucket individually
         * if all you need are the bucket values.
         *
         * @param	returnType 		class type parameter so that this method can return encoder
         * 							specific value types
         *
         * @return  list of items, each item representing the bucket value for that
         *          bucket.
         */
        public abstract List<B> getBucketValues<B>(B returnType);

        /**
         * Returns a list of {@link Encoding}s describing the inputs for
         * each sub-field that correspond to the bucket indices passed in 'buckets'.
         * To get the associated field names for each of the values, call getScalarNames().
         * @param buckets 	The list of bucket indices, one for each sub-field encoder.
         *              	These bucket indices for example may have been retrieved
         *              	from the getBucketIndices() call.
         *
         * @return A list of {@link Encoding}s. Each EncoderResult has
         */
        public List<Encoding> getBucketInfo(int[] buckets)
        {
            //Concatenate the results from bucketInfo on each child encoder
            List<Encoding> retVals = new List<Encoding>();
            int bucketOffset = 0;
            foreach (EncoderTuple<T> encoderTuple in getEncoders(this))
            {
                int nextBucketOffset = -1;
                List<EncoderTuple<T>> childEncoders = null;
                if ((childEncoders = getEncoders((EncoderBase<T>)encoderTuple.Encoder)) != null)
                {
                    nextBucketOffset = bucketOffset + childEncoders.Count;
                }
                else
                {
                    nextBucketOffset = bucketOffset + 1;
                }
                int[] bucketIndices = ArrayUtils.sub(buckets, ArrayUtils.range(bucketOffset, nextBucketOffset));
                List<Encoding> values = encoderTuple.Encoder.getBucketInfo(bucketIndices);

                retVals.AddRange(values);

                bucketOffset = nextBucketOffset;
            }

            return retVals;
        }

        /**
         * Returns a list of EncoderResult named tuples describing the top-down
         * best guess inputs for each sub-field given the encoded output. These are the
         * values which are most likely to generate the given encoded output.
         * To get the associated field names for each of the values, call
         * getScalarNames().
         * @param encoded The encoded output. Typically received from the topDown outputs
         *              from the spatial pooler just above us.
         *
         * @returns A list of EncoderResult named tuples. Each EncoderResult has
         *        three attributes:
         *
         *        -# value:         This is the best-guess value for the sub-field
         *                          in a format that is consistent with the type
         *                          specified by getDecoderOutputFieldTypes().
         *                          Note that this value is not necessarily
         *                          numeric.
         *
         *        -# scalar:        The scalar representation of this best-guess
         *                          value. This number is consistent with what
         *                          is returned by getScalars(). This value is
         *                          always an int or float, and can be used for
         *                          numeric comparisons.
         *
         *        -# encoding       This is the encoded bit-array
         *                          that represents the best-guess value.
         *                          That is, if 'value' was passed to
         *                          encode(), an identical bit-array should be
         *                          returned.
         */


        public List<Encoding> topDownCompute(int[] encoded)
        {
            List<Encoding> retVals = new List<Encoding>();

            List<EncoderTuple<T>> encoders = getEncoders(this);
            int len = encoders.Count;
            for (int i = 0; i < len; i++)
            {
                int offset = (int)encoders[i].Offset;
                EncoderBase<T> encoder = encoders[i].Encoder;

                int nextOffset;
                if (i < len - 1)
                {
                    //Encoders = List<Encoder> : Encoder = EncoderTuple(name, encoder, offset)
                    nextOffset = (int)encoders[i + 1].Offset;
                }
                else
                {
                    nextOffset = W;
                }

                int[] fieldOutput = ArrayUtils.sub(encoded, ArrayUtils.range(offset, nextOffset));
                List<Encoding> values = encoder.topDownCompute(fieldOutput);

                retVals.AddRange(values);
            }

            return retVals;
        }

        public List<double> closenessScores(List<double> expValues, List<double> actValues, bool fractional)
        {

            List<double> retVal = new List<double>();

            //Fallback closenss is a percentage match
            List<EncoderTuple<T>> encoders = getEncoders(this);
            if (encoders == null || encoders.Count < 1)
            {
                double err = Math.Abs(expValues[0] - actValues[0]);
                double closeness = -1;
                if (fractional)
                {
                    double denom = Math.Max(expValues[0], actValues[0]);
                    if (denom == 0)
                    {
                        denom = 1.0;
                    }

                    closeness = 1.0 - err / denom;
                    if (closeness < 0)
                    {
                        closeness = 0;
                    }
                }
                else
                {
                    closeness = err;
                }

                retVal.Add(closeness);
                return retVal;
            }

            int scalarIdx = 0;
            foreach (EncoderTuple<T> res in getEncoders(this))
            {
                List<double> values = res.Encoder.closenessScores(
                    expValues.Sublist(scalarIdx, expValues.Count), actValues.Sublist(scalarIdx, actValues.Count), fractional);

                scalarIdx += values.Count;
                retVal.AddRange(values);
            }

            return retVal;
        }

        /**
         * Returns an array containing the sum of the right
         * applied multiplications of each slice to the array
         * passed in.
         *
         * @param encoded
         * @return
         */
        public int[] rightVecProd(SparseObjectMatrix<int[]> matrix, int[] encoded)
        {
            int[] retVal = new int[matrix.getMaxIndex() + 1];
            for (int i = 0; i < retVal.Length; i++)
            {
                int[] slice = matrix.getObject(i);
                for (int j = 0; j < slice.Length; j++)
                {
                    retVal[i] += (slice[j] * encoded[j]);
                }
            }
            return retVal;
        }

        /**
         * Calculate width of display for bits plus blanks between fields.
         *
         * @return	width
         */
        public int getDisplayWidth()
        {
            return getWidth() + getDescription().Count - 1;
        }

        /**
         * Base class for {@link Encoder} builders
         * @param <T>
         */



    }
}