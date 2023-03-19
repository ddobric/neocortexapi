// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Classifiers
{
    public interface IClassifier<TIN, TOUT>
    {
        void Learn(TIN input, Cell[] activeCells, bool learn);
        TIN GetPredictedInputValue(Cell[] predictiveCells);
    }
}
