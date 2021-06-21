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
        public string ValueDelimiter = " ";

        public const char TypeDelimiter = ' ';

        public const char ParameterDelimiter = '|';

        public string LineDelimiter = "";

        public string KeyValueDelimiter = ": ";

        public const char  ElementsDelimiter = ',';

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
        public String ReadBegin(String typeName)
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
        public String ReadEnd(String typeName)
        {
            string val = ($"{TypeDelimiter} END '{typeName}' {TypeDelimiter}");
            return val;
        }

        /// <summary>
        /// Serialize the property of type Int.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param> 
        public void SerializeValue(int val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val.ToString());
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the property of type Int.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Int</returns>
        public int ReadIntValue(String reader)
        {
            reader = reader.Trim();
            if (string.IsNullOrEmpty(reader))
            {
                Debug.WriteLine(reader);
                return 0;
            }
            else
                return Convert.ToInt32(reader);

        }

        /// <summary>
        /// Serialize the property of type Double.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(double val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.000}", val));
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the property of type Double.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Double</returns>
        public Double ReadDoubleValue(String reader)
        {
            reader = reader.Trim();
            Double val = Convert.ToDouble(reader);
            return val;
        }

        /// <summary>
        /// Serialize the property of type String.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(String val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val);
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the property of type String.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>String</returns>
        public String ReadStringValue(String reader)
        {
            reader = reader.Trim();
            return reader;
        }

        /// <summary>
        /// Serialize the property of type Long.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(long val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            sw.Write(val.ToString());
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the property of type Long.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Long</returns>
        public long ReadLongValue(String reader)
        {
            reader = reader.Trim();
            long val = Convert.ToInt64(reader);
            return val;

        }

        /// <summary>
        /// Serialize the Bool.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(bool val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            String value = val ? "True" : "False";
            sw.Write(value);
            sw.Write(ValueDelimiter);
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the property of type Long.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Bool</returns>
        public bool ReadBoolValue(String reader)
        {
            reader = reader.Trim();
            bool val = bool.Parse(reader);
            return val;

        }

        /// <summary>
        /// Serialize the array of type Double.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(Double[] val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (Double i in val)
            {
                sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.000}", i));
                sw.Write(ElementsDelimiter);
            }
            sw.Write(ParameterDelimiter);

        }
        /// <summary>
        /// Read the array of type Double
        /// <summary>
        /// <param name="reader"></param>
        /// <returns>Double[]</returns>
        public Double[] ReadArrayDouble(string reader)
        {
            string[] str = reader.Split(ElementsDelimiter);
            Double[] vs = new double[str.Length-1];
            for (int i = 0; i < str.Length-1; i++)
            {
                
                vs[i] = Convert.ToDouble(str[i].Trim());
                
            }
            return vs;
        }

        /// <summary>
        /// Serialize the array of type Int.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
        public void SerializeValue(int[] val, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (int i in val)
            {
                sw.Write(i.ToString());
                sw.Write(ElementsDelimiter);
            }
            sw.Write(ParameterDelimiter);

        }
        /// <summary>
        /// Read the array of type Int.
        /// <summary>
        /// <param name="reader"></param>
        /// <returns>Int[]</returns>
        public int[] ReadArrayInt(string reader)
        {
            string[] str = reader.Split(ElementsDelimiter);
            int[] vs = new int[str.Length - 1];
            for (int i = 0; i < str.Length - 1; i++)
            {

                vs[i] = Convert.ToInt32(str[i].Trim());

            }
            return vs;
        }

        /// <summary>
        /// Serialize the array of cells.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sw"></param>
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
        /// Serialize the dictionary with key:string and value:int.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="sw"></param>
        public void SerializeValue(Dictionary<String, int> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<string, int> i in keyValues)
            {
                sw.Write(i.Key + KeyValueDelimiter + i.Value.ToString());
                sw.Write(ElementsDelimiter);
            }
                sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the dictionary with key:string and value:int.
        /// <summary>
        /// <param name="reader"></param>
        /// <returns>Dictionary<String, int></returns>
        public Dictionary<String, int> ReadDictSIValue(string reader)
        {
            string[] str = reader.Split(ElementsDelimiter);
            Dictionary<String, int> keyValues = new Dictionary<String, int>();
            for (int i = 0; i < str.Length - 1; i++)
            {
                string[] tokens = str[i].Split(KeyValueDelimiter);
                keyValues.Add(tokens[0].Trim(), Convert.ToInt32(tokens[1])) ;
            }

            return keyValues;
        }
        /// <summary>
        /// Serialize the dictionary with key:int and value:int.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="sw"></param>
        public void SerializeValue(Dictionary<int, int> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<int, int> i in keyValues)
            {
                sw.Write(i.Key.ToString() + KeyValueDelimiter + i.Value.ToString());
                sw.Write(ElementsDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        /// <summary>
        /// Read the dictionary with key:int and value:int.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Dictionary<int, int></returns>
        public Dictionary<int,int> ReadDictionaryIIValue(string reader)
        {
            string[] str = reader.Split(ElementsDelimiter);
            Dictionary<int, int> keyValues = new Dictionary<int, int>();
            for (int i = 0; i < str.Length-1; i++)
            {
                string[] tokens = str[i].Split(KeyValueDelimiter);
                keyValues.Add(Convert.ToInt32(tokens[0].Trim()), Convert.ToInt32(tokens[1]));
            }
            return keyValues;
        }

        /// <summary>
        /// Serialize the dictionary with key:string and value:int[].
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="sw"></param>
        public void SerializeValue(Dictionary<String, int[]> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<string, int[]> i in keyValues)
            {
                sw.Write(i.Key + KeyValueDelimiter);
                foreach (int val in i.Value)
                {
                    sw.Write(val.ToString());
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ElementsDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        ///<summary>
        ///Read the dictionary with key:String and value:int[].
        ///</summary>
        ///<param name="reader"></param>
        /// <returns>Dictionary<string, int[]></returns>
        public Dictionary<String, int[]> ReadDictSIarray(String reader)
        {
            string[] str = reader.Split(ElementsDelimiter);
            Dictionary<String, int[]> keyValues = new Dictionary<String, int[]>();
            for (int i = 0; i < str.Length-1; i++)
            {
                string[] tokens = str[i].Split(KeyValueDelimiter);
                string[] values = tokens[1].Split(ValueDelimiter);
                int[] arrayValues = new int[values.Length - 1];
                for (int j = 0; j < values.Length - 1; j++)
                {

                    arrayValues[j] = Convert.ToInt32(values[j].Trim());

                }
                keyValues.Add(tokens[0].Trim(), arrayValues);
            }
            return keyValues;
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
                    sw.Write(ElementsDelimiter);
                }
            }
            sw.Write(ParameterDelimiter);
        }

        ///// <summary>
        ///// Serialize the List of Synapse.
        ///// </summary>
        //public void SerializeValue(List<Synapse> value, StreamWriter sw)
        //{
        //    sw.Write(ValueDelimiter);
        //    if (value != null)
        //    {
        //        foreach (Synapse val in value)
        //        {
        //            if (!File.Exists($"Users / mouni.kolisetty / neocortexapi / NeoCortexApi / UnitTestsProject / bin / Debug / net5.0 / ser_SerializeSynapseTest_{val.SynapseIndex}"))
        //            {
        //                using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSynapseTest)}_"))
        //                    val.Serialize(sw);
        //            }
        //            sw.Write(ElementsDelimiter);
        //        }
        //    }
        //    sw.Write(ParameterDelimiter);
        //}

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
                sw.Write(KeyValueDelimiter);
                foreach (Synapse val in i.Value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ElementsDelimiter);
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
                sw.Write(KeyValueDelimiter);
                foreach (DistalDendrite val in i.Value)
                {
                    val.Serialize(sw);
                    sw.Write(ValueDelimiter);
                }

                sw.Write(ElementsDelimiter);
            }
            sw.Write(ParameterDelimiter);
        }
        
        
        
    }

}