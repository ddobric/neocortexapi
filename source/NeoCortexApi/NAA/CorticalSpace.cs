// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
