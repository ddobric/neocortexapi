// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.IO;

namespace NeoCortexApi
{
    public class ThreadSafeRandom : Random, ISerializable
    {
        private int seed;
        private int counter;

        public ThreadSafeRandom() : base()
        {

        }

        public ThreadSafeRandom(int seed) : base(seed)
        {
            this.seed = seed;
            this.counter = 0;
        }

        public ThreadSafeRandom(int seed, int count) : this(seed)
        {
            for (int i = 0; i < count; i++)
            {
                base.Next();
                counter++;
            }
        }

        public override int Next()
        {
            lock (typeof(ThreadSafeRandom))
            {
                counter++;
                return base.Next();
            }
        }

        public override int Next(int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                counter++;
                return base.Next(maxValue);
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                counter++;
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
                counter++;
                return base.NextDouble();
            }
        }

        public override bool Equals(object obj)
        {
            var random = obj as ThreadSafeRandom;
            if (random == null)
                return false;
            return this.Equals(random);
        }

        public bool Equals(ThreadSafeRandom other)
        {
            return seed == other.seed && counter == other.counter;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            var random = obj as ThreadSafeRandom;
            HtmSerializer2.Serialize(random.seed, nameof(ThreadSafeRandom.seed), sw);
            HtmSerializer2.Serialize(random.counter, nameof(ThreadSafeRandom.counter), sw);
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            if (typeof(T) != typeof(ThreadSafeRandom))
                return null;
            int seed = default;
            int counter = default;

            while (sr.Peek() > 0)
            {
                var content = sr.ReadLine();
                if (content.StartsWith("Begin") && content.Contains(name))
                {
                    continue;
                }
                if (content.StartsWith("End") && content.Contains(name))
                {
                    break;
                }
                if (content.Contains(nameof(ThreadSafeRandom.seed)))
                {
                    seed = HtmSerializer2.Deserialize<int>(sr, nameof(ThreadSafeRandom.seed));

                }
                if (content.Contains(nameof(ThreadSafeRandom.counter)))
                {
                    counter = HtmSerializer2.Deserialize<int>(sr, nameof(ThreadSafeRandom.counter));

                }
            }

            return new ThreadSafeRandom(seed, counter);
        }
    }
}
