using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSequenceLearning
{
    public class Sequence
    {
        public String name { get; set; }
        public int[] data { get; set; }
    }

    public class SequenceString
    {
        public String name { get; set; }
        public String data { get; set; }
    }
}
