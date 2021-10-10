// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System.Collections.Generic;

namespace NeoCortexApi
{
    public class DentriteComparer : IComparer<DistalDendrite>
    {
        private int m_NextSegmentOrdinal;

        public DentriteComparer(int nextSegmentOrdinal)
        {
            m_NextSegmentOrdinal = nextSegmentOrdinal;
        }

        public int Compare(DistalDendrite s1, DistalDendrite s2)
        {
            double c1 = s1.ParentCell.Index + ((double)(s1.Ordinal / (double)m_NextSegmentOrdinal));
            double c2 = s2.ParentCell.Index + ((double)(s2.Ordinal / (double)m_NextSegmentOrdinal));
            return c1 == c2 ? 0 : c1 > c2 ? 1 : -1;
        }
    }
}
