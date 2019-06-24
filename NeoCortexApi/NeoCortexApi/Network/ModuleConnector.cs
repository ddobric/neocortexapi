using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class ModuleConnector
    {
        public IModuleData Input { get; set; }

        public IModuleData Output { get; set; }
    }
}
