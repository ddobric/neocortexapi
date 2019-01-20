using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class PublisherSubject<T> : IObservable<T>
    {
        private List<IObserver<T>> observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);

            return new Subscription<T>(observers, observer);
        }

        public void OnNext(T val)
        {
            foreach (var observer in this.observers)
            {
                observer.OnNext(val);
            }
        }

        internal void OnCompleted()
        {
            foreach (var observer in this.observers)
            {
                observer.OnCompleted();
            }
        }
    }

}
