using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public interface IClassifier<TIN,TOUT>
    {
        void Learn(TIN input, Cell[] activeCells, bool learn);
        TOUT Inference(Cell[] activeCells);
    }
}
