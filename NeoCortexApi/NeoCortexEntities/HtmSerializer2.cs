using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeoCortexApi
{
    /**
   * Contains methods for Serialization and Deserialization and is applicable to Spatial Pooler and Temoral Memory Class
   */
    public class HtmSerializer2
    {
        //SP
        const string valueDelimiter = " ";

        const string typeDelimiter = " ";

        public void SerializeBegin(String typeName, StreamWriter sw)
        {
            //
            // -- BEGIN ---
            // typeName

            sw.Write($"{typeDelimiter} BEGIN '{typeName}' {typeDelimiter}");
            sw.WriteLine();
            
        }

        public void SerializeEnd(String typeName, StreamWriter sw)
        {
            sw.Write($"{typeDelimiter} END '{typeName}' {typeDelimiter}");
            sw.WriteLine();
        }

        public void SerializeValue(double val, StreamWriter sw)
        {
            sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", val));
            sw.Write(valueDelimiter);
        }
        //public Double ReadValue(StreamReader sr)
        //{
        //    Double val = 0.0;
        //    string value = sr.ReadToEnd();
        //    val = Convert.ToDouble(value);
        //    return val;

        //}
        public void SerializeValue(String val, StreamWriter sw)
        {
            sw.Write(val);
            sw.Write(valueDelimiter);
        }
        public String ReadValue(StreamReader sr)
        {
            string val = sr.ReadToEnd();
            return val;

        }

        public int ReadIntValue(StreamReader reader)
        {
            throw new NotImplementedException();
        }
    }

}