// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.DataMappers
{
    /// <summary>
    /// Class for asigning set of properties for each feature (data column)
    /// </summary>
    internal class DataMapper
    {
        private DataDescriptor m_Descriptor;

        private CortexNetworkContext m_Context;

        /// <summary>
        /// Width of input across of all encoders.
        /// </summary>
        public int InputWidth
        {
            get
            {
                int width = 0;
                foreach (var feature in m_Descriptor.Features)
                {
                    width += feature.Encoder.Width;
                }
                return width;
            }
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        public DataMapper(DataDescriptor descriptor, CortexNetworkContext context)
        {
            this.m_Context = context;
            this.m_Descriptor = descriptor;

            foreach (var feature in descriptor.Features)
            {
                if (feature.EncoderSettings != null)
                {
                    feature.Encoder = this.m_Context.CreateEncoder(feature.EncoderSettings);
                }
                else
                {
                    throw new ArgumentException("Encoder settings not specified.");
                }
            }
        }


        /// <summary>
        /// Transform the featureVector from natural format in to double format. 
        /// ** AFTER MAPPING:  'LABEL COLUMN IS THE LAST ELEMENT IN ARRAY'**
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public int[] Run(object[] vector)
        {
            //sort features by id
            Array.Sort(this.m_Descriptor.Features, (x, y) => x.Id.CompareTo(y.Id));

            //enumerate all dataset data
            List<int> output = new List<int>();

            //
            // Transform rawData in to raw of Features with proper type, normalization value, and corect binary and catogery type 
            // during enumeration Features are sorted by Id property
            //for (int featureIndx = 0; featureIndx < data[0].Length; featureIndx++)
            foreach (var featureIndx in this.m_Descriptor.Features.OrderBy(x => x.Id).Select(x => x.Index))
            {
                var col = this.m_Descriptor.Features[featureIndx];
                if (col.Encoder == null)
                    col.Encoder = m_Context.CreateEncoder(col.EncoderSettings);

                int[] encodedValue = col.Encoder.Encode(vector[featureIndx]);

                output.AddRange(encodedValue);
            }


            //after real data is transformed in to numeric format, then we can calculate number of feature
            // ctx.DataDescriptor.NumOfFeatures = rows.FirstOrDefault().Count;

            // Returns rows of double value feture vectors
            return output.ToArray();
        }
    }

    /// <summary>
    /// Implementation of the data column used in Data Mapper 
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Feature Id. Features are sorted by this property
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Feature (Column) name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Feature position in trainData/testData
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Assembly qualified name of the column encoder. If specified, encoder is used for encoding of the value.
        /// </summary>
        public Dictionary<String, Object> EncoderSettings { get; set; }

        /// <summary>
        /// Instance of encoder used for this column.
        /// </summary>
        public EncoderBase Encoder { get; set; }


        /// <summary>
        /// In case of binary and Category type, values represent class values enumerated in ascedenting order
        /// binary:
        /// {false,true} - mean: 0->false, 1->true
        /// {no, yes}; - mean: 0->no, 1->yes  
        /// {0, 1}; - mean: 0->0, 1->1
        /// 
        /// multiclass: 1->n representation 
        /// {Red, Green, Blue}; - mean: (Red=0, Green=1, Blue=2) normalized values: Red-> (1,0,0), Green ->(0,1,0), Blue ->(0,0,1) 
        /// </summary>
        public string[] Values { get; set; }

        /// <summary>
        /// Replaces the missing value in the cell
        /// </summary>
        public double DefaultMissingValue { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResultMapping
    {
        public Dictionary<string, double> Mappings { get; set; }
    }
}
