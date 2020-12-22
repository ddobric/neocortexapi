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
   /// <summary>
   /// Serialization class used for serialization and deserialization of primitive types.
   /// </summary>
    public class HtmSerializer2
    {
        //SP
        const string valueDelimiter = " ";

        const string typeDelimiter = " ";

        /// <summary>
        /// Serializes the begin marker of the type.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="sw"></param>
        public void SerializeBegin(String typeName, StreamWriter sw)
        {
            //
            // -- BEGIN ---
            // typeName

            sw.WriteLine();
            sw.Write($"{typeDelimiter} BEGIN '{typeName}' {typeDelimiter}");
            sw.WriteLine();
            
        }

        /// <summary>
        /// Serialize the end marker of the type.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="sw"></param>
        public void SerializeEnd(String typeName, StreamWriter sw)
        {
            sw.WriteLine();
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public int ReadIntValue(StreamReader reader)
        {
            throw new NotImplementedException();
        }
    }

}