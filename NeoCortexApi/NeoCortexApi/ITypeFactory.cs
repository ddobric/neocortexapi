using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public interface ITypeFactory<T>
    {
        T make(int[] args);
    }
}
