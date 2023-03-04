// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace NeoCortexApi.Encoders
{
    /// <summary>
    /// Base class for all encoders.
    /// </summary> 
    /// 
    public abstract class EncoderBase : IHtmModule, ISerializable
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
        private int offset;
        private object encoder;

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
                verbosity = 0;
                forced = false;



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


        public int[] outputIndices
        {
            get { return (int[])this["outputIndices"]; }
            set { this["outputIndices"] = value; }
        }

        public int[] run
        {
            get { return (int[])this["run"]; }
            set { this["run"] = value; }
        }

        public int[] nz
        {
            get { return (int[])this["nz"]; }
            set { this["nz"] = value; }
        }
        /// <summary>
        /// In real cortex mode, W must be >= 21. Empirical value.
        /// </summary>
        public bool IsRealCortexModel { get => (bool)this["IsRealCortexModel"]; set => this["IsRealCortexModel"] = (bool)value; }

        public bool forced { get => (bool)this["forced"]; set => this["forced"] = (bool)value; }


        /// <summary>
        /// The width of output vector of encoder. 
        /// It specifies the length of array, which will be occupied by output vector.
        /// </summary>

        public int N { get => (int)this["N"]; set => this["N"] = (int)value; }

        public int verbosity { get => (int)this["verbosity"]; set => this["verbosity"] = (int)value; }

        public int NInternal { get => (int)this["NInternal"]; set => this["NInternal"] = (int)value; }

        public int subLen { get => (int)this["subLen"]; set => this["subLen"] = (int)value; }


        public int maxZerosInARow { get => (int)this["maxZerosInARow"]; set => this["maxZerosInARow"] = (int)value; }
        /// <summary>
        /// Number of bits set on one, which represents single encoded value.
        /// </summary>
        public int W { get => (int)this["W"]; set => this["W"] = (int)value; }


        public int Start { get => (int)this["Start"]; set => this["Start"] = (int)value; }

        public int runLen { get => (int)this["runLen"]; set => this["runLen"] = (int)value; }
        public int left { get => (int)this["left"]; set => this["left"] = (int)value; }

        public double inMin { get => (double)this["inMin"]; set => this["inMin"] = (double)value; }

        public string fieldName { get => (string)this["fieldName"]; set => this["fieldName"] = (string)value; }

        public double inMax { get => (double)this["inMax"]; set => this["inMax"] = (double)value; }

        public int right { get => (int)this["right"]; set => this["right"] = (int)value; }

        //This matrix is used for the topDownCompute. We build it the first time
        //topDownCompute is called
        public int _topDownMappingM { get => (int)this["_topDownMappingM"]; set => this["_topDownMappingM"] = (int)value; }

        public int _topDownValues { get => (int)this["_topDownValues"]; set => this["_topDownValues"] = (int)value; }

        public double MinVal { get => (double)this["MinVal"]; set => this["MinVal"] = (double)value; }

        public double MaxVal { get => (double)this["MaxVal"]; set => this["MaxVal"] = (double)value; }

        /// <summary>
        /// How many input values are represented with W encoding bits. r=W*Res.
        /// </summary>
        public double Radius { get => (double)this["Radius"]; set => this["Radius"] = (double)value; }

        public double bucketVal { get => (double)this["bucketVal"]; set => this["bucketVal"] = (double)value; }

       
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


        public double RangeInternal { get => RangeInternal; set => this.RangeInternal = value; }

        //public int NumOfBits { get => m_NumOfBits; set => this.m_NumOfBits = value; }     

        public int HalfWidth { get => HalfWidth; set => this.HalfWidth = value; }

        public int minbin { get => (int)this["minbin"]; set => this["minbin"] = value; }

        public int maxbin { get => (int)this["maxbin"]; set => this["maxbin"] = value; }
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

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="inputData">The inputData<see cref="object"/></param>
        /// <returns>The <see cref="int[]"/></returns>
        //public abstract int[] Encode(object inputData);

       

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
        public class EncoderResult
        {
            // TODO: Define the `EncoderResult` class
        }

        public List<EncoderResult> GetBucketInfo(List<int> buckets)
        {
            // Fall back topdown compute
            if (Encoders == null)
            {
                throw new InvalidOperationException("Must be implemented in sub-class");
            }

            // Concatenate the results from bucketInfo on each child encoder
            List<EncoderResult> retVals = new List<EncoderResult>();
            int bucketOffset = 0;
            for (int i = 0; i < Encoders.Count; i++)
            {
                string s = Convert.ToString(i);
                EncoderInfo encoderInfo = Encoders[s];
                IEncoder encoder = (IEncoder)encoderInfo.encoder;
                int offset = encoderInfo.offset;
                string name = encoderInfo.Name;
                //(string name, IEncoder encoder, int offset) = Encoders[i];

                int nextBucketOffset;
                if (encoder.Encoders != null)
                {
                    nextBucketOffset = bucketOffset + ((Array)encoder.Encoders).Length;

                }
                else
                {
                    nextBucketOffset = bucketOffset + 1;
                }

                List<int> bucketIndices = buckets.GetRange(bucketOffset, nextBucketOffset - bucketOffset);
                List<EncoderResult> values = encoder.GetBucketInfo(bucketIndices);

                retVals.AddRange(values);

                bucketOffset = nextBucketOffset;
            }

            return retVals;
        }
        public abstract void EncodeIntoArray(object inputData, double[] output);
       

        public int GetWidth()
        {
            // TODO: Return the appropriate width value
            throw new NotImplementedException();
        }

        private Type defaultDtype = typeof(double);



        public void EncodeIntoArray(object inputData, Array output)
        {
            // Encodes inputData and puts the encoded value into the output array, which is a 1-D array of length returned by GetWidth().
            // .. note:: The output array is reused, so clear it before updating it.
            throw new NotImplementedException();
        }

        public virtual List<(string name, int offset)> GetDescription()
        {
            throw new NotImplementedException("GetDescription must be implemented by all subclasses");
        }

        public void PPrint(double[] output, string prefix = "")
        {
            Console.Write(prefix);
            var description = this.GetDescription().Concat(new List<(string, int)> { ("end", this.GetWidth()) }).ToList();

            for (int i = 0; i < description.Count - 1; i++)
            {
                int offset = description[i].Item2;
                int nextoffset = description[i + 1].Item2;
                Console.Write($" {bitsToString(output, offset, nextoffset)} |");
            }

            Console.WriteLine();
        }


        public (Dictionary<string, (List<(double, double)> ranges, string desc)> fieldsDict, List<string> fieldsOrder) Decode(BitArray encoded, int i, string parentFieldName = "")
        {
            var fieldsDict = new Dictionary<string, (List<(double, double)> ranges, string desc)>();
            var fieldsOrder = new List<string>();

            // What is the effective parent name?
            var parentName = parentFieldName == "" ? Name : $"{parentFieldName}.{Name}";

            if (Encoders != null)
            {
                // Merge decodings of all child encoders together
                for (int i = 0; i < Encoders.Count; i++)
                {
                    string k = Convert.ToString(i);
                    // Get the encoder and the encoded output
                    var (Encode, offset) = Encoders[k];
                    var nextOffset = i < Encoders.Count - 1 ? Encoders[k + 1].offset : Width;
                    var fieldOutput = new BitArray(encoded.Cast<bool>().Skip(offset).Take(nextOffset - offset).ToArray());
                    var (subFieldsDict, subFieldsOrder) = encoder.Decode(fieldOutput, parentName);

                    foreach (var (key, value) in subFieldsDict)
                    {
                        var fieldName = $"{parentName}.{key}";
                        if (!fieldsDict.TryGetValue(fieldName, out var existingValue))
                        {
                            existingValue = (new List<(double, double)>(), "");
                            fieldsDict[fieldName] = existingValue;
                            fieldsOrder.Add(key);
                        }

                        var (ranges, desc) = value;
                        existingValue.ranges.AddRange(ranges);
                        if (existingValue.desc != "" && desc != "")
                        {
                            existingValue.desc += ", ";
                        }

                        existingValue.desc += desc;
                    }

                    fieldsOrder.AddRange(subFieldsOrder);
                }
            }

            return (fieldsDict, fieldsOrder);
        }




        public string DecodedToStr(Tuple<Dictionary<string, Tuple<List<int>, string>>, List<string>> decodeResults)
        {
            var fieldsDict = decodeResults.Item1;
            var fieldsOrder = decodeResults.Item2;
            var desc = "";

            foreach (var fieldName in fieldsOrder)
            {
                var ranges = fieldsDict[fieldName].Item1;
                var rangesStr = fieldsDict[fieldName].Item2;
                if (desc.Length > 0)
                {
                    desc += $", {fieldName}:";
                }
                else
                {
                    desc += $"{fieldName}:";
                }

                desc += $"[{rangesStr}]";
            }

            return desc;
        }

        private string bitsToString(double[] output, int start, int end)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < end; i++)
            {
                sb.Append(output[i] > 0.5 ? "1" : "0");
            }

            return sb.ToString();
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

        public override bool Equals(object obj)
        {
            var encoder = obj as EncoderBase;
            if (encoder == null)
                return false;
            return this.Equals(encoder);
        }

        public bool Equals(EncoderBase other)
        {
            if (other == null)
                return false;
            if (this.Properties == null)
                return other.Properties == null;

            foreach (var key in this.Properties.Keys)
            {
                if (other.Properties.TryGetValue(key, out var value) == false)
                    return false;
                if (!this[key].Equals(value))
                    return false;
            }
            return true;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var excludeMembers = new List<string> 
            { 
                nameof(EncoderBase.Properties),
                nameof(EncoderBase.halfWidth),
                nameof(EncoderBase.rangeInternal),
                nameof(EncoderBase.nInternal),
                nameof(EncoderBase.encLearningEnabled),
                nameof(EncoderBase.flattenedFieldTypeList),
                nameof(EncoderBase.decoderFieldTypes),
                nameof(EncoderBase.topDownValues),
                nameof(EncoderBase.bucketValues),
                nameof(EncoderBase.topDownMapping),

            };
            HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers: excludeMembers);
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            var excludeMembers = new List<string> { nameof(EncoderBase.Properties) };
            return HtmSerializer.DeserializeObject<T>(sr, name, excludeMembers);
        }

        public bool Equals(IHtmModule other)
        {
            return this.Equals((object)other);
        }
        // Define the Encoders property as a dictionary of tuples
        public Dictionary<string, (object encoder, int offset)> Encoders { get; set; }

        public class EncoderInfo
        {
            internal IEncoder encoder;
            internal int offset;

            public string Name { get; internal set; }

            public static implicit operator EncoderInfo((object encoder, int offset) v)
            {
                throw new NotImplementedException();
            }
        }
    }

    
}