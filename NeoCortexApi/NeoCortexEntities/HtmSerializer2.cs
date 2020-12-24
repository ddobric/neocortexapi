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

        const string parameterDelimiter = "|";

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

        /// <summary>
        /// Serialize the property of type Double.
        /// </summary>
        public void SerializeValue(double val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", val));
            sw.Write(valueDelimiter);
            sw.Write(parameterDelimiter);
        }
        
        /// <summary>
        /// Serialize the property of type String.
        /// </summary>
        public void SerializeValue(String val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            sw.Write(val);
            sw.Write(valueDelimiter);
            sw.Write(parameterDelimiter);
        }
        
        /// <summary>
        /// Serialize the property of type Int.
        /// </summary>
        public void SerializeValue(int val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            sw.Write(val.ToString());
            sw.Write(valueDelimiter);
            sw.Write(parameterDelimiter);
        }
        /// <summary>
        /// Serialize the property of type long.
        /// </summary>
        public void SerializeValue(long val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            sw.Write(val.ToString());
            sw.Write(valueDelimiter);
            sw.Write(parameterDelimiter);
        }
        /// <summary>
        /// Serialize the array of type long.
        /// </summary>
        public void SerializeValue(Double[] val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            foreach (Double i in val)
            {
                sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", i));
                sw.Write(valueDelimiter);
            }
            sw.Write(parameterDelimiter);

        }
        /// <summary>
        /// Serialize the array of type int.
        /// </summary>
        public void SerializeValue(int[] val, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            foreach (int i in val)
            {
                sw.Write(i.ToString());
                sw.Write(valueDelimiter);
            }
            sw.Write(parameterDelimiter);

        }
        /// <summary>
        /// Serialize the dictionary with key:string and value:int.
        /// </summary>
        public void SerializeValue(Dictionary<String, int> keyValues, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            foreach (KeyValuePair<string, int> i in keyValues)
            {
                sw.Write(i.Key + ": " + i.Value.ToString());
                sw.Write(valueDelimiter);
            }
                sw.Write(parameterDelimiter);
        }
        /// <summary>
        /// Serialize the dictionary with key:string and value:int[].
        /// </summary>
        public void SerializeValue(Dictionary<String, int[]> keyValues, StreamWriter sw)
        {
            sw.Write(valueDelimiter);
            foreach (KeyValuePair<string, int[]> i in keyValues)
            {
                sw.Write(i.Key + ": ");
                foreach (int val in i.Value)
                {
                    sw.Write(val.ToString());
                    sw.Write(valueDelimiter);
                }

                sw.Write(valueDelimiter);
            }
            sw.Write(parameterDelimiter);
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
        /// <summary>
        /// Deserialize the property of type Double.
        /// </summary>
        public Double ReadDoubleValue(StreamReader sr)
        {
            Double val = 0.0;
            string value = sr.ReadToEnd();
            val = Convert.ToDouble(value);
            return val;
        }
        /// <summary>
        /// Deserialize the property of type String.
        /// </summary>
        public String ReadStringValue(StreamReader sr)
        {
            string val = sr.ReadToEnd();
            return val;

        }
    }

}