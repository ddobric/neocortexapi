using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// Base class for all encoders.
    /// </summary> 
    public abstract class EncoderBase : IHtmModule
    {
        /// <summary>
        /// List of all encoder properties.
        /// </summary>
        protected Dictionary<string, object> Properties = new Dictionary<string, object>();
        

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

        /** number of bits in the representation (must be >= w) */
        protected int m_NumOfBits = 0;

        /** the half width value */
        protected int halfWidth;
     
      
        protected int nInternal;
        protected double rangeInternal;
        //protected double range;
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

        // Moved to MultiEncoder.
        //protected Dictionary<EncoderTuple, List<EncoderTuple>> encoders;
        protected List<String> scalarNames;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EncoderBase()
        {
            
        }

        /// <summary>
        /// Called by framework to initialize encoder with all required settings.
        /// </summary>
        /// <param name="encoderSettings"></param>
        public EncoderBase(Dictionary<String, Object> encoderSettings)
        {
            this.Initialize(encoderSettings);
        }


        /// <summary>
        /// Sets the copy all properties.
        /// </summary>
        /// <param name="encoderSettings"></param>

        public void Initialize(Dictionary<String, Object> encoderSettings)
        {
            this.Properties.Clear();

            foreach (var item in encoderSettings)
            {
                this.Properties.Add(item.Key, item.Value);
            }

            this.AfterInitialize();
        }


        /// <summary>
        /// Called by framework to initialize encoder with all required settings. This method is useful
        /// for implementation of validation logic for properties.
        /// Otherwise, if any additional initialization is required, override this method.
        /// When this method is called, all encoder properties are already set in member <see cref="Properties"/>.
        /// </summary>
        public virtual void AfterInitialize()
        {

        }
        
        #region Properties

        /// <summary>
        /// Key acces to property set.
        /// </summary>
        /// <param name="key">Name of property.</param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return Properties[key];
            }

            set
            {
                Properties[key] = value;
            }
        }

        #region Keyval Properties
        /// <summary>
        /// The width of output vector of encoder. 
        /// It specifies the length of array, which will be occupied by output vector.
        /// </summary>
       
        public int N { get => (int)this["N"]; set => this["N"] = (int)value; }
       
        /// <summary>
        /// Number of bits set on one, which represents single encoded value.
        /// </summary>
        public int W { get => (int)this["W"]; set => this["W"] = (int)value; }

        public double MinVal { get => (double)this["MinVal"]; set => this["MinVal"] = (double)value; }

        public double MaxVal { get => (double)this["MaxVal"]; set => this["MaxVal"] = (double)value; }

        public double Radius { get => (double)this["Radius"]; set => this["Radius"] = (double)value; }

        public double Resolution { get => (double)this["Resolution"]; set => this["Resolution"] = (double)value; }

        public bool Periodic { get => (bool)this["Periodic"]; set => this["Periodic"] = (bool)value; }

        public bool ClipInput { get => (bool)this["ClipInput"]; set => this["ClipInput"] = (bool)value; } 


        public int Padding { get => (int)this["Padding"]; set => this["Padding"] = value; }

        public double Range { get => (double)this["Range"]; set => this["Range"] = value; }

        public bool IsForced { get => (bool)this["IsForced"]; set => this["IsForced"] = value; }

        public string Name { get => (string)this["Name"]; set => this["Name"] = value; }

        #endregion


        public double RangeInternal { get => rangeInternal; set => this.rangeInternal = value; }

        public int NInternal { get => nInternal; set => this.nInternal = value; }

        public double[] TopDownValues { get => topDownValues; set => this.topDownValues = value; }

        //public int NumOfBits { get => m_NumOfBits; set => this.m_NumOfBits = value; }     
 
        public int HalfWidth { get => halfWidth; set => this.halfWidth = value; }   
 
        //public List<String> ScalarNames { get => scalarNames; set => this.scalarNames = value; }

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
        //public List<FieldMetaType> FlattenedFieldTypeList { get => flattenedFieldTypeList; set => this.flattenedFieldTypeList = value; }
        #endregion

        /**
         * This matrix is used for the topDownCompute. We build it the first time
         * topDownCompute is called
         *
         * @param sm
         */
        //public void setTopDownMapping(SparseObjectMatrix<int[]> sm)
        //{
        //    this.topDownMapping = sm;
        //}

        /**
         * Returns the resolution
         * @return
         */




        ///**
        // * Adds a the specified {@link Encoder} to the list of the specified
        // * parent's {@code Encoder}s.
        // *
        // * @param parent	the parent Encoder
        // * @param name		Name of the {@link Encoder}
        // * @param e			the {@code Encoder}
        // * @param offset	the offset of the encoded output the specified encoder
        // * 					was used to encode.
        // */
        //public void addEncoder(EncoderBase parent, String name, EncoderBase child, int offset)
        //{
        //    if (encoders == null)
        //    {
        //        encoders = new Dictionary<EncoderTuple, List<EncoderTuple>>();
        //    }

        //    EncoderTuple key = getEncoderTuple(parent);
        //    // Insert a new Tuple for the parent if not yet added.
        //    if (key == null)
        //    {
        //        encoders.Add(key = new EncoderTuple{ Name = String.Empty, Encoder = this, Offset = 0 },
        //        new List<EncoderTuple>());
        //    }

        //    List<EncoderTuple> childEncoders = null;
        //    if ((childEncoders = encoders[key]) == null)
        //    {
        //        encoders.Add(key, childEncoders = new List<EncoderTuple>());
        //    }
        //    childEncoders.Add(new EncoderTuple() { Name = name, Encoder = child, Offset = offset });
        //}

        ///**
        // * Returns the {@link Tuple} containing the specified {@link Encoder}
        // * @param e		the Encoder the return value should contain
        // * @return		the {@link Tuple} containing the specified {@link Encoder}
        // */
        //public EncoderTuple getEncoderTuple(EncoderBase encoder)
        //{
        //    if (encoders == null)
        //    {
        //        //encoders = new LinkedHashMap<EncoderTuple, List<EncoderTuple>>();
        //        encoders = new Dictionary<EncoderTuple, List<EncoderTuple>>();
        //    }

        //    foreach (var tpl in encoders)
        //    {
        //        if (tpl.Value.Equals(encoder))
        //        {
        //            return tpl.Key;
        //        }
        //    }
        //    return null;
        //}

        ///**
        // * Returns the list of child {@link Encoder} {@link Tuple}s
        // * corresponding to the specified {@code Encoder}
        // *
        // * @param e		the parent {@link Encoder} whose child Encoder Tuples are being returned
        // * @return		the list of child {@link Encoder} {@link Tuple}s
        // */
        //public List<EncoderTuple> getEncoders(EncoderBase e)
        //{
        //    return encoders[e.getEncoderTuple(e)];
        //}

        ///**
        // * Returns the list of {@link Encoder}s
        // * @return
        // */
        //public Dictionary<EncoderTuple, List<EncoderTuple>> getEncoders()
        //{
        //    if (encoders == null)
        //    {
        //        encoders = new Dictionary<EncoderTuple, List<EncoderTuple>>();
        //    }
        //    return encoders;
        //}

      

        ///<summary>
        /// Gets the output width, in bits.
        ///</summary>
        public abstract int Width { get; }


        /// <summary>
        /// Returns true if the underlying encoder works on deltas
        /// </summary>
        public abstract bool IsDelta { get; }

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
        public abstract int[] Encode(object inputData);

        public IModuleData Compute(int[] input, bool learn)
        {
            var result = Encode(input);
            //return new NeoCortexApiInArrawyOutput(result);
            return null;
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
            int width = formatted ? getDisplayWidth() : Width;

            if (prevFieldOffset == -1 || bitOffset > Width)
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
        public void printHeader(String prefix)
        {
            //LOGGER.info(prefix == null ? "" : prefix);

            List<Tuple<string, int>> description = getDescription();
            description.Add(new Tuple<string, int>("end", Width));

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

            len = Width + (description.Count - 1) * 3 - 1;
            StringBuilder hyphens = new StringBuilder();
            for (int i = 0; i < len; i++)
                hyphens.Append("-");

            Debug.WriteLine(hyphens);
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
        public abstract List<T> getBucketValues<T>();

    

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
            return Width + getDescription().Count - 1;
        }
    }
}