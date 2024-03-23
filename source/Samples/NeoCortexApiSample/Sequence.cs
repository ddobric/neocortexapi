using System;

namespace NeoCortexApiSample
{
    public class Sequence
    {
        public String name { get; set; }
        public object Name { get; internal set; }
        public int[] data { get; set; }
        public int[] Data { get; internal set; }
        public object Value { get; internal set; }
        public object Key { get; internal set; }

        internal void Add(int value)
        {
            throw new NotImplementedException();
        }
    }
}