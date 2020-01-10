using NeoCortexApi.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
            double c1 = s1.GetParentCell().Index + ((double)(s1.getOrdinal() / (double)m_NextSegmentOrdinal));
            double c2 = s2.GetParentCell().Index + ((double)(s2.getOrdinal() / (double)m_NextSegmentOrdinal));
            return c1 == c2 ? 0 : c1 > c2 ? 1 : -1;
        }
    }
}
