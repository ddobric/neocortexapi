// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class Subscription<T> : ISubscription<T>
    {
        private List<IObserver<T>> observers;

        private IObserver<T> observer;

        internal Subscription(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }

        public void Unsubscribe()
        {
            if (!(observer == null)) observers.Remove(observer);
        }

        public void Dispose()
        {
            this.Unsubscribe();
        }

        public bool IsSubscribed()
        {
            return observers.Contains(this.observer);
        }
    }
}
