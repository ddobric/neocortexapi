// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;

namespace NeoCortexApi.Classifiers
{
    public interface IClassifier<TIN, TOUT>
    {
        void Learn(TIN input, Cell[] output);

        TIN GetPredictedInputValue(Cell[] predictiveCells);
    }
}
