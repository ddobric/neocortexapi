// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;

namespace NeoCortexApi
{
    public class ThreadSafeRandom : Random
    {
        public ThreadSafeRandom() : base()
        {
            
        }
        
        public ThreadSafeRandom(int seed) : base(seed)
        {

        }

        public override int Next()
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next();
            }
        }

        public override int Next(int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next(maxValue);
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next(minValue, maxValue);
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            throw new NotSupportedException();
        }

        public override double NextDouble()
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.NextDouble();
            }
        }
    }
}
