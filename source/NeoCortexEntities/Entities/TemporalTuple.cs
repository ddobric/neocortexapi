// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
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

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return Equals(obj as TemporalTuple);
        }

        public bool Equals(TemporalTuple tt)
        {
            if (ReferenceEquals(this, tt))
                return true;

            return Column.Equals(tt.Column) && ActiveColumns.ElementsEqual(tt.ActiveColumns)
                && ActiveSegments.ElementsEqual(tt.ActiveSegments) && MathichngSegments.ElementsEqual(tt.MathichngSegments);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Column, ActiveColumns, ActiveSegments, MathichngSegments);
        }
    }
}
