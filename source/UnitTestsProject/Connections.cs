using NeoCortexApi.Entities;
using System;
using System.IO;

namespace UnitTestsProject
{
    internal class Connections
    {
        internal object ActiveSegments;
        private HtmConfig matrix;

        public Connections(HtmConfig matrix)
        {
            this.matrix = matrix;
        }

        public Connections()
        {
        }

        internal void Serialize(StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        internal static Connections Deserialize(StreamReader sr)
        {
            throw new NotImplementedException();
        }
    }
}