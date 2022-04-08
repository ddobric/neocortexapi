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
    /// Used as container for properties, which define topology of HTM module.
    /// </summary>
    public class HtmModuleTopology
    {
        public HtmModuleTopology()
        {

        }

        public HtmModuleTopology(int[] dimensions, bool isMajorOrdering)
        {
            this.Dimensions = dimensions;
            this.IsMajorOrdering = isMajorOrdering;
            this.DimensionMultiplies = AbstractFlatMatrix.InitDimensionMultiples(dimensions);
        }

        public int[] Dimensions { get; set; }

        public bool IsMajorOrdering { get; set; }

        public int[] DimensionMultiplies { get; set; }

        public int NumDimensions { get { return Dimensions.Length; } }

        public bool Equals(HtmModuleTopology obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if ((obj.GetType() != this.GetType()))
                return false;

            //HtmModuleTopology other = (HtmModuleTopology)obj;
            if (this.IsMajorOrdering != obj.IsMajorOrdering)
                return false;
            if (obj.Dimensions != null && this.Dimensions != null)
            {
                
                if (!obj.Dimensions.SequenceEqual(this.Dimensions))
                    return false;
            }
            if (obj.DimensionMultiplies != null && this.DimensionMultiplies != null)
            {

                if (!obj.DimensionMultiplies.SequenceEqual(this.DimensionMultiplies))
                    return false;
            }
            return true;

        }

        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(HtmModuleTopology), writer);

            ser.SerializeValue(this.Dimensions, writer);
            ser.SerializeValue(this.IsMajorOrdering, writer);
            ser.SerializeValue(this.DimensionMultiplies, writer);
            //this.NumDimensions --- It is not serialised since it returns only length of dimensions.

            ser.SerializeEnd(nameof(HtmModuleTopology), writer);
        }

        public static HtmModuleTopology Deserialize(StreamReader sr)
        {
            HtmModuleTopology htm = new HtmModuleTopology();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == ser.LineDelimiter || data == ser.ReadBegin(nameof(HtmModuleTopology)) || data == ser.ReadEnd(nameof(SegmentActivity)))
                {
                    continue;
                }
                else if (data == ser.ReadEnd(nameof(HtmModuleTopology)))
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

                                    htm.Dimensions = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    htm.IsMajorOrdering = ser.ReadBoolValue(str[i]);
                                    break;
                                }
                            case 2:
                                {
                                    htm.DimensionMultiplies = ser.ReadArrayInt(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return htm;
        }
        #endregion

    }
}
