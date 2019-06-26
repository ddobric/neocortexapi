using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    /**
  * <p>
  * Executes check point behavior through the {@link #checkPoint(Observer)} method. The
  * checkPoint() method adds the specified {@link rx.Observer} to the list of those
  * observers notified following a check point operation. This "subscribe" action invokes
  * the underlying check point operation and returns a notification. The notification consists of
  * a byte[] containing the serialized {@link Network}.
  * </p><p>
  * <b>Typical usage is as follows:</b>
  * <pre>
  *  {@link Persistence} p = Persistence.get();
  *  
  *  p.checkPointOp().checkPoint(new Observer<byte[]>() { 
  *      public void onCompleted() {}
  *      public void onError(Throwable e) { e.printStackTrace(); }
  *      public void onNext(byte[] bytes) {
  *          // Do work here, use serialized Network byte[] here if desired...
  *      }
  *  });
  * 
  * Again, by subscribing to this CheckPointOp, the Network knows to check point after completion of 
  * the current compute cycle (it checks the List of Observers to see if it's non-empty).
  * Then after it notifies all current observers, it clears the list prior to the next 
  * following compute cycle. see {@link PAPI} for a more detailed discussion...
  * 
  * @author cogmission
  *
  * @param <T>  the notification return type
  */

    public interface ICheckPointOp<T>
    {
        /**
         * Registers the Observer for a single notification following the checkPoint
         * operation. The user will be notified with the byte[] of the {@link Network}
         * being serialized.
         * 
         * @param t     a {@link rx.Observer}
         * @return  a Subscription object which is meaningless.
         */
        public Subscription CheckPoint(IObserver<T> t);
    }

}
