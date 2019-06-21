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

        public int[] Compute(int[] input, bool learn)
        {
            for (int i = 0; i < this.HtmModules.Count; i++)
            {

            }

            foreach (var module in this.HtmModules)
            {
              //  var out = module.Compute(...);
            }

            return null;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
