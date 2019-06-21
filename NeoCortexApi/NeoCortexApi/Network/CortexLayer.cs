using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class CortexLayer : IHtmModule
    {
        #region Properties

        public string Name { get; set; }

        public List<IHtmModule> HtmModules { get; set; }
        #endregion


        #region Constructors and Initialization
        public CortexLayer(string name) : this(name, new List<IHtmModule>())
        {

        }

        public CortexLayer(string name, List<IHtmModule> modules)
        {
            this.Name = name;
            this.HtmModules = modules;
        }

        #endregion

        #region Public Methods
        public CortexLayer AddModule(IHtmModule module)
        {
            this.HtmModules.Add(module);
            // connect
            return this;
        }

        public void Compute(int[] input, bool learn)
        {
            foreach (var module in this.HtmModules)
            {
                module.Compute(input, learn);
            }
        }

        IComputeOutput IHtmModule.Compute(int[] input, bool learn)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
