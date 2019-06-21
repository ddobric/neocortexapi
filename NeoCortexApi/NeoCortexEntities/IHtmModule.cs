using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{

    /// <summary>
    /// Defines the interface, which needs to be implemented by all classes, which can be inserted as a layer in region.
    /// </summary>
    public interface IHtmModule
    {
        //int[] Compute(int[] input, bool learn);
    }

    public interface IComputeOutput
    {

    }
}
