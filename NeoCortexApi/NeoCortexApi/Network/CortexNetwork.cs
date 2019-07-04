using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class CortexNetwork
    {
        public CortexNetwork(string name) : this(name, new List<CortexRegion>())
        {
         
        }

        public CortexNetwork(string name, List<CortexRegion> regions)
        {
            this.Name = name;
        }
        #region Properties

        public string Name { get; set; }

        public List<CortexRegion> Regions { get; set; }

        public void Compute()
        {
            foreach (var module in this.Regions)
            {

            }
        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
