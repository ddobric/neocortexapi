using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class Publisher
    {
        private static readonly int HEADER_SIZE = 3;

      

        private Network parentNetwork;
       

        public class Builder<T>
        {
            private ReplaySubject<String> subject;

            private Consumer<Publisher> notifier;

            // The 3 lines of the header
            String[] lines = new String[3];

            int cursor = 0;


            /**
             * Creates a new {@code Builder}
             */
            public Builder()
            {
                this(null);
            }

            /**
             * Instantiates a new {@code Builder} with the specified
             * {@link Consumer} used to propagate "build" events using a
             * plugged in function.
             * 
             * @param c     Consumer used to notify the {@link Network} of new
             *              builds of a {@link Publisher}
             */
            public Builder(Consumer<Publisher> c)
            {
                this.notifier = c;
            }

            /**
             * Adds a header line which in the case of a multi column input 
             * is a comma separated string.
             * 
             * @param s     string representing one line of a header
             * @return  this Builder
             */
         
            public Builder<PublishSubject<String>> addHeader(String s)
            {
                lines[cursor] = s;
                ++cursor;
                return (Builder<PublishSubject<String>>)this;
            }

            /**
             * Builds and validates the structure of the expected header then
             * returns an {@link Observable} that can be used to submit info to the
             * {@link Network}
             * @return  a new Publisher
             */
            public Publisher build()
            {
                subject = ReplaySubject.createWithSize(3);
                for (int i = 0; i < HEADER_SIZE; i++)
                {
                    if (lines[i] == null)
                    {
                        throw new FormatException("Header not properly formed (must contain 3 lines) see Header.java");
                    }
                    subject.onNext(lines[i]);
                }

                Publisher p = new Publisher();
                p.subject = subject;

                if (notifier != null)
                {
                    notifier.accept(p);
                }

                return p;
            }
        }

        /**
         * Returns a builder that is capable of returning a configured {@link PublishSubject} 
         * (publish-able) {@link Observable}
         * 
         * @return
         */
        public static Builder<PublishSubject<String>> builder()
        {
            return new Builder<>();
        }

        /**
         * Builder that notifies a Network on every build of a new {@link Publisher}
         * @param c     Consumer which consumes a Publisher and executes an Network notification.
         * @return      a new Builder
         */
        public static Builder<PublishSubject<String>> builder(Consumer<Publisher> c)
        {
            return new Builder<>(c);
        }

        /**
         * Sets the parent {@link Network} on this {@code Publisher} for use as a convenience. 
         * @param n     the Network to which the {@code Publisher} is connected.
         */
        public void setNetwork(Network n)
        {
            this.parentNetwork = n;
        }

        /**
         * Returns the parent {@link Network} connected to this {@code Publisher} for use as a convenience. 
         * @return  this {@code Publisher}'s parent {@link Network}
         */
        public Network getNetwork()
        {
            return parentNetwork;
        }

        /**
         * Provides the Observer with a new item to observe.
         * <p>
         * The {@link Observable} may call this method 0 or more times.
         * <p>
         * The {@code Observable} will not call this method again after it calls either {@link #onComplete} or
         * {@link #onError}.
         * 
         * @param input the item emitted by the Observable
         */
        public void onNext(String input)
        {
            subject.onNext(input);
        }

        /**
         * Notifies the Observer that the {@link Observable} has finished sending push-based notifications.
         * <p>
         * The {@link Observable} will not call this method if it calls {@link #onError}.
         */
        public void onComplete()
        {
            subject.onCompleted();
        }

        /**
         * Notifies the Observer that the {@link Observable} has experienced an error condition.
         * <p>
         * If the {@link Observable} calls this method, it will not thereafter call {@link #onNext} or
         * {@link #onComplete}.
         * 
         * @param e     the exception encountered by the Observable
         */
        public void onError(Throwable e)
        {
            subject.onError(e);
        }

        /**
         * Subscribes to an Observable and provides an Observer that implements functions to handle the items the
         * Observable emits and any error or completion notification it issues.
         * <dl>
         *  <dt><b>Scheduler:</b></dt>
         *  <dd>{@code subscribe} does not operate by default on a particular {@link Scheduler}.</dd>
         * </dl>
         *
         * @param observer
         *             the Observer that will handle emissions and notifications from the Observable
         * @return a {@link Subscription} reference with which the {@link Observer} can stop receiving items before
         *         the Observable has completed
         * @see <a href="http://reactivex.io/documentation/operators/subscribe.html">ReactiveX operators documentation: Subscribe</a>
         */
        public Subscription subscribe(IObserver<String> observer)
        {
            return subject.subscribe(observer);
        }

        /**
         * Called within package to access this {@link Publisher}'s wrapped {@link Observable}
         * @return
         */
        public IObservable<String> observable()
        {
            return subject;
        }
    }
}
