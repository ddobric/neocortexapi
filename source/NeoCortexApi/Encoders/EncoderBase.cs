// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// number of bits in the representation (must be >= w) 
        /// </summary>
        protected int m_NumOfBits = 0;

        /// <summary>
        /// the half width value
        /// </summary>
        protected int halfWidth;


        protected int nInternal;
        
        protected double rangeInternal;
        
        protected bool encLearningEnabled;
        
        protected List<FieldMetaType> flattenedFieldTypeList;

        protected Dictionary<Dictionary<string, int>, List<FieldMetaType>> decoderFieldTypes;

        /// <summary>
        /// This matrix is used for the topDownCompute. We build it the first time topDownCompute is called
        /// </summary>
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
        /// It invokes <see cref="AfterInitialize"./>
        /// </summary>
        /// <param name="encoderSettings">If not NULL, then all properties are copied to the new internal instance of properties.</param>

        public void Initialize(Dictionary<String, Object> encoderSettings)
        {
            if (encoderSettings != null)
            {
                this.Properties.Clear();

                Name = this.GetType().Name;
                IsRealCortexModel = false;
                N = 0;
                Resolution = -1.0;
                Radius = -1.0;
                Periodic = false;
                ClipInput = false;

                foreach (var item in encoderSettings)
                {
                    if (!this.Properties.ContainsKey(item.Key))
                        this.Properties.Add(item.Key, item.Value);
                    else
                        this.Properties[item.Key] = item.Value;
                }
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

        #region Properties

        /// <summary>
        /// In real cortex mode, W must be >= 21. Empirical value.
        /// </summary>
        public bool IsRealCortexModel { get => (bool)this["IsRealCortexModel"]; set => this["IsRealCortexModel"] = (bool)value; }

        /// <summary>
        /// The width of output vector of encoder. 
        /// It specifies the length of array, which will be occupied by output vector.
        /// </summary>

        public int N { get => (int)this["N"]; set => this["N"] = (int)value; }

        public int NInternal { get => (int)this["NInternal"]; set => this["NInternal"] = (int)value; }

        /// <summary>
        /// Number of bits set on one, which represents single encoded value.
        /// </summary>
        public int W { get => (int)this["W"]; set => this["W"] = (int)value; }

        public double MinVal { get => (double)this["MinVal"]; set => this["MinVal"] = (double)value; }

        public double MaxVal { get => (double)this["MaxVal"]; set => this["MaxVal"] = (double)value; }

        /// <summary>
        /// How many input values are represented with W encoding bits. r=W*Res.
        /// </summary>
        public double Radius { get => (double)this["Radius"]; set => this["Radius"] = (double)value; }

        /// <summary>
        /// How many input values are embedded in the single encoding bit. Res = (max-min)/N.
        /// </summary>
        public double Resolution { get => (double)this["Resolution"]; set => this["Resolution"] = (double)value; }

        public bool Periodic { get => (bool)this["Periodic"]; set => this["Periodic"] = (bool)value; }

        /// <summary>
        /// It cats bits at the beginning with negative position and at th eend after last bit.
        /// This happens only if Periodic is set on false.
        /// </summary>
        public bool ClipInput { get => (bool)this["ClipInput"]; set => this["ClipInput"] = (bool)value; }

        public int Padding { get => (int)this["Padding"]; set => this["Padding"] = value; }

        public double Range { get => (double)this["Range"]; set => this["Range"] = value; }

        public bool IsForced { get => (bool)this["IsForced"]; set => this["IsForced"] = value; }

        public string Name { get => (string)this["Name"]; set => this["Name"] = value; }

        public int Offset { get => (int)this["Offset"]; set => this["Offset"] = value; }

        #endregion


        public double RangeInternal { get => rangeInternal; set => this.rangeInternal = value; }

        //public int NumOfBits { get => m_NumOfBits; set => this.m_NumOfBits = value; }     

        public int HalfWidth { get => halfWidth; set => this.halfWidth = value; }


        ///<summary>
        /// Gets the output width, in bits.
        ///</summary>
        public abstract int Width { get; }


        /// <summary>
        /// Returns true if the underlying encoder works on deltas
        /// </summary>
        public abstract bool IsDelta { get; }
        #endregion

        /// <summary>
        /// Encodes inputData and puts the encoded value into the output array, which is a 1-D array of length returned by <see cref="W"/>.
        /// </summary>
        /// <param name="inputData">Data to encode. This should be validated by the encoder.</param>
        /// <returns>1-D array of same length returned by <see cref="W"/></returns>
        /// <remarks>
        /// Note: The output array is reused, so clear it before updating it.
        /// </remarks>
        public abstract int[] Encode(object inputData);

        public IModuleData Compute(int[] input, bool learn)
        {
            var result = Encode(input);

            return null;
        }

        /// <summary>
        /// Returns a list of items, one for each bucket defined by this encoder. Each item is the value assigned to that bucket, this is the same as the
        /// EncoderResult.value that would be returned by getBucketInfo() for that bucket and is in the same format as the input that would be passed to encode().<br></br>
        /// 
        /// This call is faster than calling getBucketInfo() on each bucket individually if all you need are the bucket values.
        /// </summary>
        /// <typeparam name="T">class type parameter so that this method can return encoder specific value types</typeparam>
        /// <returns>list of items, each item representing the bucket value for that bucket.</returns>
        public abstract List<T> GetBucketValues<T>();

        /// <summary>
        /// Returns an array containing the sum of the right applied multiplications of each slice to the array passed in.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public int[] RightVecProd(SparseObjectMatrix<int[]> matrix, int[] encoded)
        {
            int[] retVal = new int[matrix.GetMaxIndex() + 1];
            for (int i = 0; i < retVal.Length; i++)
            {
                int[] slice = matrix.GetObject(i);
                for (int j = 0; j < slice.Length; j++)
                {
                    retVal[i] += (slice[j] * encoded[j]);
                }
            }
            return retVal;
        }


        /// <summary>
        /// Returns the rendered similarity matrix for the whole rage of values between min and max.
        /// </summary>
        /// <param name="traceValues">True if every value should be included in the output.</param>
        /// <returns>Formatted matrix of similariteis, betwen encoded values.</returns>
        public string TraceSimilarities(bool traceValues = true)
        {
            Dictionary<string, int[]> sdrMap = new Dictionary<string, int[]>();
            List<string> inpVals = new List<string>();
            StringBuilder sb = new StringBuilder();
            
            for (double i = this.MinVal; i < this.MaxVal; i += 1.0)
            {
                var sdr = this.Encode(i);
                sdrMap.Add($"{i}", ArrayUtils.IndexWhere(sdr, (el) => el == 1));
                inpVals.Add($"{i}");

                if (traceValues)
                {
                    sb.AppendLine($"{i.ToString("000")} - {Helpers.StringifyVector(sdr, separator: null)}");
                }                
            }

            sb.AppendLine();

            var similarities = MathHelpers.CalculateSimilarityMatrix(sdrMap);

            var results = Helpers.RenderSimilarityMatrix(inpVals, similarities);

            return sb.ToString() + results;
        }

    }
}