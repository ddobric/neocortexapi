using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class HeaderMetaData
    {
        public List<FieldMetaType> FieldMetaData { get; set; }

        public List<string> FieldNames { get; set; }

        public List<string>  SensorFlags{ get; set; }
    }
}
