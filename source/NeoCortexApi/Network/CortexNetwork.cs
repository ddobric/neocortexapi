// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Network
{
    public class CortexNetwork
    {
        #region Properties
        public string Name { get; set; }
        public List<CortexRegion> HtmRegions { get; set; }
        #endregion

        #region Constructors and Initialization
        public CortexNetwork(string name) : this(name, new List<CortexRegion>())
        {

        }

        public CortexNetwork(string name, List<CortexRegion> regions)
        {
            this.Name = name;
            this.HtmRegions = regions;
        }
        #endregion

        #region Public Methods
        public CortexNetwork AddRegion(CortexRegion region)
        {
            this.HtmRegions.Add(region);
            return this;
        }

        public void Compute(int[] input, Boolean learn)
        {
            foreach (var region in this.HtmRegions)
            {
                region.Compute(input, learn);
            }
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
