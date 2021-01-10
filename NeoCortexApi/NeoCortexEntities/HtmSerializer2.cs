using NeoCortexApi.Entities;
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

namespace NeoCortexApi.Entities
{
   /// <summary>
   /// Serialization class used for serialization and deserialization of primitive types.
   /// </summary>
    public class HtmSerializer2
    {
        //SP
        public const char ValueDelimiter = ' ';

        public const char TypeDelimiter = ' ';

        public const char ParameterDelimiter = '|';

        public string LineDelimiter = "";

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
            sw.Write($"{TypeDelimiter} BEGIN '{typeName}' {TypeDelimiter}");
            sw.WriteLine();
            
        }
        public String ReadBegin(String typeName, StreamReader sr)
        {
            string val = ($"{TypeDelimiter} BEGIN '{typeName}' {TypeDelimiter}");
            return val;

        }
        /// <summary>
        /// Serialize the end marker of the type.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="sw"></param>
        public void SerializeEnd(String typeName, StreamWriter sw)
        {
            sw.WriteLine();
            sw.Write($"{TypeDelimiter} END '{typeName}' {TypeDelimiter}");
            sw.WriteLine();
        }

        public String ReadEnd(String typeName, StreamReader sr)
        {
            string val = ($"{TypeDelimiter} END '{typeName}' {TypeDelimiter}");
            return val;

        }
        /// <summary>
        /// Serialize the property of type Double.
        /// </summary>
        public void SerializeValue(double val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", val));
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }

        
        /// <summary>
        /// Deserialize the property of type Double.
        /// </summary>
        public Double ReadDoubleValue(StreamReader sr)
        {
            string value = sr.ReadLine();
            Double val = Convert.ToDouble(value);
            return val;
        }
        
        /// <summary>
        /// Serialize the property of type String.
        /// </summary>
        public void SerializeValue(String val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val);
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }


        /// <summary>
        /// Serialize the property of type Int.
        /// </summary>
        public void SerializeValue(int val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val.ToString());
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public int ReadIntValue(String reader)
        {
            reader = reader.Trim();
            int val = Convert.ToInt32(reader);
            return val;
            
        }
        /// <summary>
        /// Serialize the property of type long.
        /// </summary>
        public void SerializeValue(long val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val.ToString());
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the array of type Double.
        /// </summary>
        public void SerializeValue(Double[] val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (Double i in val)
            {
                sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", i));
                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);

        }
        /// <summary>
        /// Serialize the array of type int.
        /// </summary>
        public void SerializeValue(int[] val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (int i in val)
            {
                sw.Write(i.ToString());
                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);

        }
        /// <summary>
        /// Serialize the dictionary with key:string and value:int.
        /// </summary>
        public void SerializeValue(Dictionary<String, int> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<string, int> i in keyValues)
            {
                sw.Write(i.Key + ": " + i.Value.ToString());
                sw.Write(ValueDelimiter);
            }
                sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the dictionary with key:string and value:int.
        /// </summary>
        public void SerializeValue(Dictionary<int, int> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<int, int> i in keyValues)
            {
                sw.Write(i.Key.ToString() + ": " + i.Value.ToString());
                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the dictionary with key:string and value:int[].
        /// </summary>
        public void SerializeValue(Dictionary<String, int[]> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<string, int[]> i in keyValues)
            {
                sw.Write(i.Key + ": ");
                foreach (int val in i.Value)
                {
                    sw.Write(val.ToString());
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the Bool.
        /// </summary>
        public void SerializeValue(bool val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            String value = val ? "True" : "False";
            sw.Write(value);
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }

        /// <summary>
        /// Serialize the array of cells.
        /// </summary>
        public void SerializeValue(Cell[] val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            if (val != null)
            {
                foreach (Cell cell in val)
                {
                    cell.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }
            }
            sw.Write(ParameterDelimiter);
        }

        /// <summary>
        /// Serialize the List of DistalDendrite.
        /// </summary>
        public void SerializeValue(List<DistalDendrite> value, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            if (value != null)
            {
                foreach (DistalDendrite val in value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the List of Synapse.
        /// </summary>
        public void SerializeValue(List<Synapse> value, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            if (value != null)
            {
                foreach (Synapse val in value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the List of Integers.
        /// </summary>
        public void SerializeValue(List<int> value, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            if (value != null)
            {
                foreach (int val in value)
                {
                    sw.Write(val.ToString());
                    sw.Write(ValueDelimiter);
                }
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Serialize the Dictionary<Segment, List<Synapse>>.
        /// </summary>
        public void SerializeValue(Dictionary<Segment, List<Synapse>> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<Segment, List<Synapse>> i in keyValues)
            {
                i.Key.Serialize(sw);
                sw.Write(": ");
                foreach (Synapse val in i.Value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        //private Dictionary<Cell, LinkedHashSet<Synapse>> m_ReceptorSynapses;

        /// <summary>
        /// Serialize the Dictionary<Segment, List<Synapse>>.
        /// </summary>
        public void SerializeValue(Dictionary<Cell, List<DistalDendrite>> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<Cell, List<DistalDendrite>> i in keyValues)
            {
                i.Key.Serialize(sw);
                sw.Write(": ");
                foreach (DistalDendrite val in i.Value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ValueDelimiter);
            }
            sw.Write(ParameterDelimiter);
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