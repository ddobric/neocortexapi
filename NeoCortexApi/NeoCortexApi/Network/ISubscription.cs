using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public interface ISubscription<T> : IDisposable
    {
        void Unsubscribe();

        bool IsSubscribed();
    }
}
