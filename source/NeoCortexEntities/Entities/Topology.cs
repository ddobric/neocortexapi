// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeoCortexApi.Entities
{

    /// <summary>
    /// Transforms coordinates from multidimensional space into the single dimensional space of flat indexes.
    /// </summary>
    /// <remarks>
    /// Authors:
    /// cogmission, Damir Dobric.
    /// </remarks>
    public class Topology
    {

        protected int[] dimensions;
        protected int[] dimensionMultiples;

        /// <summary>
        /// 
        /// </summary>
        protected bool isColumnMajor;


        protected int numDimensions;


        public HtmModuleTopology HtmTopology
        {
            get
            {
                return new HtmModuleTopology(this.dimensions, this.isColumnMajor);
            }
        }

        /// <summary>
        /// Constructs a new <see cref="Coordinator"/> object to be configured with specified dimensions and major ordering.
        /// </summary>
        /// <param name="shape">the dimensions of this matrix</param>
        public Topology(int[] shape) : this(shape, false)
        {

        }
        /**
         * 
         *
         * 
         * @param shape                     
         * @param useColumnMajorOrdering    
         *                                  
         *                                  
         *                                 
         */
        /// <summary>
        /// Constructs a new <see cref="Coordinator"/> object to be configured with specified dimensions and major ordering.
        /// </summary>
        /// <param name="shape">the dimensions of this sparse array</param>
        /// <param name="useColumnMajorOrdering">flag indicating whether to use column ordering or row major ordering. if false 
        ///                                      (the default), then row major ordering will be used. If true, then column major
        ///                                      ordering will be used.</param>
        public Topology(int[] shape, bool useColumnMajorOrdering)
        {
            this.dimensions = shape;
            this.numDimensions = shape.Length;
            this.dimensionMultiples = InitDimensionMultiples(
                useColumnMajorOrdering ? HtmCompute.Reverse(shape) : shape);
            isColumnMajor = useColumnMajorOrdering;
        }

        public Topology()
        {
        }

        /// <summary>
        /// Initializes internal helper array which is used for multidimensional index computation.
        /// </summary>
        /// <param name="dimensions">matrix dimensions</param>
        /// <returns>array for use in coordinates to flat index computation.</returns>
        protected int[] InitDimensionMultiples(int[] dimensions)
        {
            int holder = 1;
            int len = dimensions.Length;
            int[] dimensionMultiples = new int[numDimensions];
            for (int i = 0; i < len; i++)
            {
                holder *= (i == 0 ? 1 : dimensions[len - i]);
                dimensionMultiples[len - 1 - i] = holder;
            }
            return dimensionMultiples;
        }
        public bool Equals(Topology obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (!dimensions.SequenceEqual(obj.dimensions))
                return false;
            if (!dimensionMultiples.SequenceEqual(obj.dimensionMultiples))
                return false;
            if (isColumnMajor != obj.isColumnMajor)
                return false;
            if (numDimensions != obj.numDimensions)
                return false;

            return true;
        }
        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Topology), writer);

            ser.SerializeValue(this.dimensions, writer);
            ser.SerializeValue(this.dimensionMultiples, writer);
            ser.SerializeValue(this.isColumnMajor, writer);
            ser.SerializeValue(this.numDimensions, writer);

            ser.SerializeEnd(nameof(Topology), writer);

        }
        public static Topology Deserialize(StreamReader sr)
        {
            Topology topology = new Topology();
            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Topology)))
                {
                    continue;
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
                                    topology.dimensions = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    topology.dimensionMultiples = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    topology.isColumnMajor= ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            case 3:
                                {
                                    topology.numDimensions = ser.ReadIntValue(str[i]);
                                    break;
                                }

                        }
                    }
                }
            }
            return topology;
        }
        #endregion
    }
}
