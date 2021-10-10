using System;
using System.Collections.Generic;

namespace NeoCortexApi.Utility
{
    public class FlexComRowMatrix<T>
    {
        public List<List<object>> Matrix = new List<List<object>>();

        public void AddAndUpdate(int row, int collumn, double value)
        {
            Matrix[row][collumn] = Convert.ToDouble(Matrix[row][collumn]) + value;
        }
    }
}
