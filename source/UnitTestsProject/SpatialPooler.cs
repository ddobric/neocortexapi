using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTM_SP_tm
{
    internal class SpatialPooler

    {   // Creates an empty pooler.
        SpatialPooler sp = new SpatialPooler();

        // Creates the cell-space
        Connections mem = new Connections();

        // Initialize cell-space with set of required parameters.
        parameters.apply(mem);

        // Initializes the SpatialPooler.
        sp.init(mem);
    }

} 
