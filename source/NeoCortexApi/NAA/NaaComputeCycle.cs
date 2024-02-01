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
