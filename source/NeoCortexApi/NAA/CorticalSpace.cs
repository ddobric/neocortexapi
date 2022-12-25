using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.NAA
{
    /// <summary>
    /// Holds the set of cortical areas.
    /// It does not implement any cortical function.
    /// </summary>
    public class CorticalSpace
    {
        private List<CorticalArea> _areas = new List<CorticalArea>();

        public CorticalSpace() { }
    }
}
