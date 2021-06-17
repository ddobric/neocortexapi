// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Used as container for properties, which define topology of HTM module.
    /// </summary>
    //public class HtmModuleTopology
    //{
    //    public HtmEnpointTopology InputTopology { get; set; }

    //    public HtmEnpointTopology ColumnTopology { get; set; } 
    //}

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

        public int NumDimensions { get { return Dimensions.Length;  } }
    }
}
