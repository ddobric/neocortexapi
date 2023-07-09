// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class NaaComputeCycle
    {
        public List<int> ActiveCellsIndicies { get; set; } = new List<int>();

        public List<int> WinnerCellsIndicies { get; set; } = new List<int>();
    }
}
