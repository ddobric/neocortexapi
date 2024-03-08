// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Collections.Generic;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Helpref class, which hold active columns, active segments and mathcing segments. 
    /// </summary>
    public class TemporalTuple
    {
        public Column Column { get; set; }

        /// <summary>
        /// Holds active columns
        /// </summary>
        public List<Column> ActiveColumns { get; set; }

        public List<DistalDendrite> ActiveSegments { get; set; }

        public List<DistalDendrite> MathichngSegments { get; set; }
    }
}
