using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Serialization class used for serialization and deserialization of primitive types.
    /// </summary>
    public class HtmSerializer2
    {
        //SP
        public string tab = "\t";

        public string newLine = "\n";

        public string ValueDelimiter = " ";

        public const char TypeDelimiter = ' ';

        public const char ParameterDelimiter = '|';

        public string LineDelimiter = "";

        public static string KeyValueDelimiter = ": ";

        public const char ElementsDelimiter = ',';

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
        public String ReadBegin(string typeName)
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


        public void SerializeValue(object val, Type type, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);

            if (type.IsValueType)
            {
                //if(type == typeof(int) || type == typeof(double)
                sw.Write(val.ToString());
            }
            else
            {
                var method = type.GetMethod("", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(val, new object[] { sw });
                }
                else
                    throw new NotSupportedException($"No serialization implemented on the type {type}!");

                sw.Write(ValueDelimiter);
                sw.Write(ParameterDelimiter);
            }
        }

            public void DeserializeValue(object value, Type type)
            {
                //todo..
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
            /// Deserializes from text file to DistalDendrite
            /// </summary>
            /// <param name="sr"></param>
            /// <returns>DistalDendrite</returns>
            public Synapse DeserializeSynapse(StreamReader sr)
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == ReadBegin(nameof(Synapse)))
                    {
                        Synapse synapseT1 = Synapse.Deserialize(sr);

                        Cell cell1 = synapseT1.SourceCell;

                        DistalDendrite distSegment1 = synapseT1.SourceCell.DistalDendrites[0];

                        DistalDendrite distSegment2 = synapseT1.SourceCell.DistalDendrites[1];

                        distSegment1.ParentCell = cell1;
                        distSegment2.ParentCell = cell1;
                        synapseT1.SourceCell = cell1;

                        return synapseT1;
                    }
                }
                return null;
            }

            /// <summary>
            /// Deserializes from text file to DistalDendrite
            /// </summary>
            /// <param name="sr"></param>
            /// <returns>DistalDendrite</returns>
            public DistalDendrite DeserializeDistalDendrite(StreamReader sr)
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == ReadBegin(nameof(DistalDendrite)))
                    {

                        DistalDendrite distSegment1 = DistalDendrite.Deserialize(sr);

                        Cell cell1 = distSegment1.ParentCell;

                        distSegment1 = distSegment1.ParentCell.DistalDendrites[0];
                        distSegment1.ParentCell = cell1;
                        DistalDendrite distSegment2 = distSegment1.ParentCell.DistalDendrites[1];
                        distSegment2.ParentCell = cell1;

                        return distSegment1;
                    }
                }
                return null;
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
                string value = reader.Trim();
                if (value == LineDelimiter)
                    return null;
                else
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
            /// Read the property of type Long.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns>Bool</returns>
            public Random ReadRandomValue(String reader)
            {
                int val = Convert.ToInt16(reader);
                Random rnd = new ThreadSafeRandom(val);
                return rnd;

            }
            public void SerializeValue(Array array, StreamWriter sw)
            {
                sw.Write(ValueDelimiter);
                sw.WriteLine();

                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        sw.Write(array.GetValue(i, j));
                    }
                }

                sw.Write(ValueDelimiter);
                sw.Write(ParameterDelimiter);
            }
            /// <summary>
            /// Serialize the array of type Double.
            /// </summary>
            /// <param name="val"></param>
            /// <param name="sw"></param>
            public void SerializeValue(Double[] val, StreamWriter sw)
            {
                sw.Write(ValueDelimiter);
                if (val != null)
                {
                    foreach (Double i in val)
                    {
                        sw.Write(string.Format(CultureInfo.InvariantCulture, "{0:0.000}", i));
                        sw.Write(ElementsDelimiter);
                    }
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
                string value = reader.Trim();
                if (value == LineDelimiter)
                    return null;
                else
                {
                    string[] str = reader.Split(ElementsDelimiter);
                    Double[] vs = new double[str.Length - 1];
                    for (int i = 0; i < str.Length - 1; i++)
                    {

                        vs[i] = Convert.ToDouble(str[i].Trim());

                    }
                    return vs;
                }

            }

            /// <summary>
            /// Serialize the array of type Int.
            /// </summary>
            /// <param name="val"></param>
            /// <param name="sw"></param>
            public void SerializeValue(int[] val, StreamWriter sw)
            {
                sw.Write(ValueDelimiter);
                if (val != null)
                {
                    foreach (int i in val)
                    {
                        sw.Write(i.ToString());
                        sw.Write(ElementsDelimiter);
                    }
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
                        cell.SerializeT(sw);
                        sw.Write(ValueDelimiter);
                    }
                }
                sw.Write(ParameterDelimiter);

            }

            /// <summary>
            /// Deserialize the array of cells.
            /// </summary>
            /// <param name="reader"></param>
            public Cell[] DeserializeCellArray(string data, StreamReader reader)
            {
                List<Cell> cells = new List<Cell>();
                if (data == ReadBegin(nameof(Cell)))
                {
                    Cell cell1 = Cell.Deserialize(reader);

                    if (cell1.DistalDendrites.Count != 0)
                    {

                        DistalDendrite distSegment1 = cell1.DistalDendrites[0];

                        DistalDendrite distSegment2 = cell1.DistalDendrites[1];


                        distSegment1.ParentCell = cell1;
                        distSegment2.ParentCell = cell1;
                    }
                    cells.Add(cell1);
                }
                while (reader.Peek() >= 0)
                {
                    string val = reader.ReadLine();
                    if (val == ReadBegin(nameof(Cell)))
                    {
                        Cell cell1 = Cell.Deserialize(reader);
                        if (cell1.DistalDendrites.Count != 0)
                        {
                            DistalDendrite distSegment1 = cell1.DistalDendrites[0];

                            DistalDendrite distSegment2 = cell1.DistalDendrites[1];

                            distSegment1.ParentCell = cell1;
                            distSegment2.ParentCell = cell1;
                        }
                        cells.Add(cell1);

                    }
                }

                Cell[] cells1 = cells.ToArray();
                return cells1;
            }

            /// <summary>
            /// Deserializes from text file to Cell
            /// </summary>
            /// <param name="sr"></param>
            /// <returns>Cell</returns>
            public Cell DeserializeCell(StreamReader sr)
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == ReadBegin(nameof(Cell)))
                    {
                        Cell cell1 = Cell.Deserialize(sr);

                        DistalDendrite distSegment1 = cell1.DistalDendrites[0];

                        DistalDendrite distSegment2 = cell1.DistalDendrites[1];

                        distSegment1.ParentCell = cell1;
                        distSegment2.ParentCell = cell1;

                        return cell1;
                    }
                }
                return null;
            }


        /// <summary>
        /// Serialize the dictionary with key:string and value:int.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="sw"></param>
        public void SerializeGenericValue<TKey, TValue>(Dictionary<TKey, TValue> keyValues, StreamWriter sw)
        {
            sw.Write(ValueDelimiter);
            foreach (KeyValuePair<TKey, TValue> i in keyValues)
            {
                //TODO..
               //sw.Write(i.Key + KeyValueDelimiter + i.Value.ToString());
               // sw.Write(ElementsDelimiter);
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
                    keyValues.Add(tokens[0].Trim(), Convert.ToInt32(tokens[1]));
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
            public Dictionary<int, int> ReadDictionaryIIValue(string reader)
            {
                string[] str = reader.Split(ElementsDelimiter);
                Dictionary<int, int> keyValues = new Dictionary<int, int>();
                for (int i = 0; i < str.Length - 1; i++)
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
                for (int i = 0; i < str.Length - 1; i++)
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
            public void SerializeValue(List<DistalDendrite> distSegments, StreamWriter sw)
            {
                sw.Write(ValueDelimiter);
                if (distSegments != null)
                {
                    foreach (DistalDendrite val in distSegments)
                    {
                        val.SerializeT(sw);
                        sw.Write(ElementsDelimiter);
                    }
                }
                sw.Write(ParameterDelimiter);
            }
            /// <summary>
            /// Read the List of DistalDendrite.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns>List<DistalDendrite></returns>
            //public List<DistalDendrite> ReadListDendrite(StreamReader reader)
            //{
            //    List<DistalDendrite> keyValues = new List<DistalDendrite>();
            //    string data = reader.ReadLine();
            //    if (data == ReadBegin(nameof(DistalDendrite)))
            //    {
            //        keyValues.Add(DistalDendrite.Deserialize(reader));
            //    }

            //    return keyValues;
            //}
            ///// <summary>
            ///// Serialize the List of Synapse.
            ///// </summary>
            public void SerializeValue(List<Synapse> value, StreamWriter sw)
            {
                sw.Write(ValueDelimiter);
                if (value != null)
                {
                    foreach (Synapse val in value)
                    {
                        val.SerializeT(sw);
                        sw.Write(ElementsDelimiter);
                    }
                }
                sw.Write(ParameterDelimiter);
            }
            /// <summary>
            /// Read the List of Synapse.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns>List<Synapse></returns>
            //public List<Synapse> ReadListSynapse(StreamReader reader)
            //{
            //    List<Synapse> keyValues = new List<Synapse>();
            //    string data = reader.ReadLine();
            //    if (data == ReadBegin(nameof(Synapse)))
            //    {
            //        keyValues.Add(Synapse.Deserialize(reader));
            //    }

            //    return keyValues;
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
                        sw.Write(ElementsDelimiter);
                    }
                }
                sw.Write(ParameterDelimiter);
            }
            /// <summary>
            /// Read the List of Integers.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns>List<int></returns>
            public List<int> ReadListInt(String reader)
            {
                string[] str = reader.Split(ElementsDelimiter);
                List<int> keyValues = new List<int>();
                for (int i = 0; i < str.Length - 1; i++)
                {
                    keyValues.Add(Convert.ToInt32(str[i].Trim()));
                }
                return keyValues;
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
                        //val.Serialize(sw);
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
                    //i.Key.Serialize(sw);
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

            /// <summary>
            /// Serialize the dictionary with key:int and value:Synapse.
            /// </summary>
            /// <param name="keyValues"></param>
            /// <param name="sw"></param>
            public void SerializeValue(Dictionary<int, Synapse> keyValues, StreamWriter sw)
            {
                sw.WriteLine();
                sw.Write(ValueDelimiter);
                foreach (KeyValuePair<int, Synapse> i in keyValues)
                {
                    sw.Write(i.Key.ToString() + KeyValueDelimiter);
                    i.Value.Serialize(sw);
                    sw.Write(ElementsDelimiter);
                }
                sw.Write(ParameterDelimiter);
            }

            /// <summary>
            /// Read the dictionary with key:int and value:Synapse.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns>Dictionary<int, Synapse></returns>
            public int ReadKeyISValue(string reader)
            {
                string val = reader.Replace(KeyValueDelimiter, "");
                if (val.Contains(ElementsDelimiter))
                {
                    val = val.Replace(ElementsDelimiter.ToString(), "");
                }
                return Convert.ToInt32(val);
            }

            /// <summary>
            /// Serialize the Concurrentdictionary with key:int and value:DistalDendrite.
            /// </summary>
            /// <param name="keyValues"></param>
            /// <param name="sw"></param>
            public void SerializeValue(ConcurrentDictionary<int, DistalDendrite> keyValues, StreamWriter sw)
            {
                sw.WriteLine();
                sw.Write(ValueDelimiter);
                foreach (var i in keyValues)
                {
                    sw.Write(i.Key.ToString() + KeyValueDelimiter);
                    i.Value.Serialize(sw);
                    sw.Write(ElementsDelimiter);
                }
                sw.Write(ParameterDelimiter);
            }


            public static bool IsEqual(object obj1, object obj2)
            {
                if (obj1 == null && obj2 == null)
                {
                    return true;
                }
                else if ((obj1 == null && obj2 != null) || (obj1 != null && obj2 == null))
                {
                    return false;
                }

                var type = obj1.GetType();


                if (type.IsPrimitive || type == typeof(Decimal) || type == typeof(String))
                {
                    var obj1Value = obj1.ToString();
                    var obj2Value = obj2.ToString();

                    if (obj1Value != obj2Value)
                    {
                        return false;
                    }
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var dict1 = obj1 as IDictionary;
                    var dict2 = obj2 as IDictionary;

                    var keys = dict1.Keys;

                    foreach (var key in keys)
                    {
                        var dict1Item = dict1[key];
                        var dict2Item = dict2[key];
                        if (!IsEqual(dict1Item, dict2Item))
                        {
                            return false;
                        }
                    }
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
                {
                    // TODO
                }
                else if (type.FullName.StartsWith("System.Action"))
                {
                    // SKIP
                }
                else if (type.IsArray)
                {
                    var array1 = (IEnumerable)obj1;
                    var array2 = (IEnumerable)obj2;

                    var sequence1 = array1.GetEnumerator();
                    var sequence2 = array2.GetEnumerator();

                    while (sequence1.MoveNext())
                    {
                        sequence2.MoveNext();
                        if (!IsEqual(sequence1.Current, sequence2.Current))
                        {
                            return false;
                        }
                    }
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // TODO
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>))
                {
                    // TODO
                }
                else if (type.IsAbstract)
                {
                    // SKIP
                }
                else
                {
                    const System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                    var props1 = obj1.GetType().GetFields(bindingAttr); //GetProperties(bindingAttr);

                    foreach (var prop1 in props1)
                    {
                        var name = prop1.Name;
                        var prop2 = obj2.GetType().GetField(name, bindingAttr);

                        if (!IsEqual(prop1.GetValue(obj1), prop2.GetValue(obj2)))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

        }

    }