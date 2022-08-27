using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Serialization class used for serialization and deserialization of primitive types.
    /// </summary>
    public class HtmSerializer2
    {
        private static int Id = 0;
        private static Dictionary<object, int> SerializedHashCodes = new Dictionary<object, int>();
        private static Dictionary<int, object> MapObjectHashCode = new Dictionary<int, object>();

        //SP
        public string tab = "\t";

        public string newLine = "\n";

        public string ValueDelimiter = " ";

        public const char TypeDelimiter = ' ';

        public const char ParameterDelimiter = '|';

        public string LineDelimiter = "";

        public static string KeyValueDelimiter = ": ";

        public const char ElementsDelimiter = ',';
        private const string cReplaceId = "ReplaceId";
        private const string cIdString = "Id";
        private static List<int> isCellsSerialized = new List<int>();

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

        public static void Reset()
        {
            SerializedHashCodes.Clear();
            MapObjectHashCode.Clear();
            Id = 0;
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

        #region NewImplementation
        #region Serialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="sw"></param>
        /// <param name="propertyType"></param>
        /// <param name="ignoreMembers"></param>
        public static void Serialize(object obj, string name, StreamWriter sw, Type propertyType = null, List<string> ignoreMembers = null)
        {
            if (obj == null)
            {
                return;
            }
            if (name == nameof(DistalDendrite.ParentCell))
            {

            }
            var type = obj.GetType();

            bool isSerializeWithType = propertyType != null && (propertyType.IsInterface || propertyType.IsAbstract || propertyType != type);

            SerializeBegin(name, sw, isSerializeWithType ? type : null);

            var objHashCode = obj.GetHashCode();


            if (type.GetInterfaces().FirstOrDefault(i => i.FullName.Equals(typeof(ISerializable).FullName)) != null)
            {
                if (SerializedHashCodes.TryGetValue(obj, out int serializedId))
                {
                    Serialize(serializedId, cReplaceId, sw);
                }

                //if (HtmSerializer2.SerializedHashCodes.ContainsKey(objHashCode))
                //{
                //    if (type == typeof(Synapse))
                //    {
                //        var synapse = SerializedHashCodes[objHashCode] as Synapse;
                //        if (synapse != null && synapse.SegmentIndex == 31)
                //        {

                //        }
                //    }
                //}
                else
                {
                    Serialize(Id, cIdString, sw);
                    HtmSerializer2.SerializedHashCodes.Add(obj, Id++);
                    (obj as ISerializable).Serialize(obj, name, sw);
                }
            }
            else if (type.IsPrimitive || type == typeof(string))
            {
                SerializeValue(name, obj, sw);
            }
            else
            {
                if (IsDictionary(type))
                {
                    SerializeDictionary(name, obj, sw, ignoreMembers);
                }
                else if (IsList(type))
                {
                    SerializeIEnumerable(name, obj, sw, ignoreMembers);
                }
                else if (type.IsClass || type.IsValueType)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        SerializeKeyValuePair(name, obj, sw);
                    }
                    else
                    {
                        if (SerializedHashCodes.TryGetValue(obj, out int serializedId))
                        {
                            Serialize(serializedId, cReplaceId, sw);
                        }
                        else
                        {
                            Serialize(Id, cIdString, sw);
                            HtmSerializer2.SerializedHashCodes.Add(obj, Id++);
                            SerializeObject(obj, name, sw, ignoreMembers);
                        }
                    }
                }
            }
            SerializeEnd(name, sw, isSerializeWithType ? type : null);
        }

        private static void SerializeKeyValuePair(string name, object obj, StreamWriter sw)
        {
            var type = obj.GetType();

            var keyField = GetFields(type).FirstOrDefault(f => f.Name == "key");
            if (keyField != null)
            {
                var key = keyField.GetValue(obj);
                Serialize(key, "key", sw);
            }
            var valueField = GetFields(type).FirstOrDefault(f => f.Name == "value");
            if (valueField != null)
            {
                var value = valueField.GetValue(obj);
                Serialize(value, "value", sw);
            }
        }

        public static void Serialize1(object obj, string name,
            StreamWriter sw, Dictionary<Type, Action<StreamWriter, string, object>> customSerializers = null)
        {
            if (obj == null)
            {
                return;
            }
            var type = obj.GetType();

            SerializeBegin(name, sw, null);

            if (type.IsPrimitive || type == typeof(string))
            {
                SerializeValue(name, obj, sw);
            }
            else if (IsDictionary(type))
            {
                SerializeDictionary(name, obj, sw);
            }
            else if (IsList(type))
            {
                SerializeIEnumerable(name, obj, sw);
            }
            else if (type.IsClass || type.IsValueType)
            {
                if (customSerializers != null && customSerializers.ContainsKey(type))
                {
                    customSerializers[type](sw, name, obj);
                }
                else
                {
                    //if (type is baseType)
                    //{
                    //    sert
                    //}
                    //if (type == typeof(DistalDendrite))
                    //{
                    //    // serialize distal dendrite
                    //    SerializeDistalDendrite(obj, name, sw);
                    //}
                    //else if (type == typeof(HtmConfig))
                    //{
                    //    SerializeHtmConfig(obj, name, sw);
                    //}
                    //else
                    //{
                    //    SerializeObject(obj, name, sw);
                    //}
                }
            }

            SerializeEnd(name, sw, null);

        }

        private static void SerializeDictionary(string name, object obj, StreamWriter sw, List<string> ignoreMembers = null)
        {
            var type = obj.GetType();
            if (type.IsGenericType)
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];

                var enumerable = ((IEnumerable)obj);

                foreach (var item in enumerable)
                {
                    var properties = item.GetType().GetProperties();
                    SerializeBegin("DictionaryItem", sw, null);
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(item, null);
                        Serialize(value, property.Name, sw, property.PropertyType, ignoreMembers: ignoreMembers);
                    }
                    SerializeEnd("DictionaryItem", sw, null);
                }
            }
        }

        private static void SerializeBegin(string propName, StreamWriter sw, Type type)
        {
            var listString = new List<string> { "Begin" };

            if (string.IsNullOrEmpty(propName) == false)
            {
                listString.Add(propName);
            }
            if (type != null)
            {
                listString.Add(type.FullName.Replace(" ", ""));
            }

            sw.WriteLine(String.Join(' ', listString));
        }

        private static void SerializeEnd(string propName, StreamWriter sw, Type type)
        {
            sw.WriteLine();
            var listString = new List<string> { "End" };

            if (string.IsNullOrEmpty(propName) == false)
            {
                listString.Add(propName);
            }
            if (type != null)
            {
                listString.Add(type.FullName.Replace(" ", ""));
            }

            sw.WriteLine(String.Join(' ', listString));
        }

        private static string ReadGenericBegin(string propName, Type type = null)
        {
            var listString = new List<string> { "Begin" };
            if (string.IsNullOrEmpty(propName) == false)
            {
                listString.Add(propName);
            }
            if (type != null)
            {
                listString.Add(type.FullName.Replace(" ", ""));
            }

            return String.Join(' ', listString);
        }

        private static string ReadGenericEnd(string propName, Type type = null)
        {
            var listString = new List<string> { "End" };
            if (string.IsNullOrEmpty(propName) == false)
            {
                listString.Add(propName);
            }
            if (type != null)
            {
                listString.Add(type.FullName.Replace(" ", ""));
            }

            return String.Join(' ', listString);
        }


        public static void SerializeValue(string propertyName, object val, StreamWriter sw)
        {

            var content = val.ToString();
            if (val.GetType() == typeof(string))
            {
                content = $"\"{val}\"";
            }
            sw.Write(content);
        }

        //private static void SerializeBegin<T>(string propName, StreamWriter sw)
        //{
        //    sw.WriteLine(begin)
        //}

        private static void SerializeIEnumerable(string propertyName, object obj, StreamWriter sw, List<string> ignoreMembers = null)
        {
            if (IsMultiDimensionalArray(obj))
            {
                SerializeMultidimensionalArray(obj, propertyName, sw);
                return;
            }

            var enumerable = ((IEnumerable)obj);

            foreach (var item in enumerable)
            {
                Serialize(item, "CollectionItem", sw, obj.GetType().GetElementType(), ignoreMembers: ignoreMembers);
            }
        }

        private static bool IsMultiDimensionalArray(object obj)
        {
            var array = obj as Array;
            if (array == null)
                return false;
            if (array.Rank > 1)
                return true;
            return false;
        }

        private static void SerializeMultidimensionalArray(object obj, string name, StreamWriter sw)
        {
            var array = obj as Array;

            if (array.Rank > 2)
                throw new NotSupportedException("Serialize does not support array with rank greater than 2!");

            SerializeBegin(nameof(Array.Rank), sw, null);
            SerializeValue(nameof(Array.Rank), array.Rank, sw);
            SerializeEnd(nameof(Array.Rank), sw, null);

            var dimensions = new List<int>();

            for (int i = 0; i < array.Rank; i++)
            {
                SerializeBegin($"Dim{i}", sw, null);
                var dimensionLength = array.GetLength(i);
                dimensions.Add(dimensionLength);
                SerializeValue("", dimensionLength, sw);
                SerializeEnd($"Dim{i}", sw, null);
            }

            var elementType = array.GetType().GetElementType();
            var defaultValue = GetDefault(elementType);
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    var value = array.GetValue(i, j);
                    if (value.Equals(defaultValue) == false)
                    {
                        var index = new int[] { i, j };
                        Serialize(value, "ActiveElement", sw, elementType);
                        Serialize(index, "ActiveIndex", sw, elementType);
                    }
                }
            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static void SerializeObject(object obj, string name, StreamWriter sw, List<string> ignoreMembers = null)
        {
            if (obj == null)
                return;
            var type = obj.GetType();

            List<PropertyInfo> properties = GetProperties(type);
            foreach (var property in properties)
            {
                if (ignoreMembers != null && ignoreMembers.Contains(property.Name) || property.CanWrite == false)
                {
                    continue;
                }
                if (property.CanRead && property.Name != "Item" && property.PropertyType != typeof(object))
                {
                    object value = null;

                    try
                    {
                        value = property.GetValue(obj, null);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"**Warning: Type {type.Name} does not implement property {property.Name}");
                        continue;
                    }

                    Serialize(value, property.Name, sw, property.PropertyType, ignoreMembers: ignoreMembers);
                }
            }

            List<FieldInfo> fields = GetFields(type);
            foreach (var field in fields)
            {
                if (ignoreMembers != null && ignoreMembers.Contains(field.Name))
                {
                    continue;
                }

                object value = null;

                try
                {
                    value = field.GetValue(obj);
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine($"**Warning: Type {type.Name} does not implement property {field.Name}");
                    continue;
                }
                catch (Exception)
                {
                    throw;
                }

                Serialize(value, field.Name, sw, field.FieldType, ignoreMembers: ignoreMembers);
                //Serialize(value, property.Name, sw, property.PropertyType, ignoreMembers: ignoreMembers);
                //var value = field.GetValue(obj);
            }
        }

        private static List<FieldInfo> GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null).ToList();
            if (type.BaseType != null)
            {
                fields.AddRange(type.BaseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null));
            }

            return fields;
        }

        private static List<PropertyInfo> GetProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            //if (type.BaseType != null)
            //{
            //    properties.AddRange(type.BaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            //}

            return properties;
        }

        private static void SerializeDistalDendrite(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string> { nameof(DistalDendrite.ParentCell) };
            SerializeObject(obj, name, sw, ignoreMembers);

            var cell = (obj as DistalDendrite).ParentCell;

            if (isCellsSerialized.Contains(cell.Index) == false)
            {
                isCellsSerialized.Add(cell.Index);
                Serialize((obj as DistalDendrite).ParentCell, nameof(DistalDendrite.ParentCell), sw, ignoreMembers: ignoreMembers);
            }
        }

        private static void SerializeHtmConfig(object obj, string name, StreamWriter sw)
        {
            var excludeEntries = new List<string> { nameof(HtmConfig.Random) };
            SerializeObject(obj, name, sw, excludeEntries);

            var htmConfig = obj as HtmConfig;
            Serialize(htmConfig.RandomGenSeed, nameof(HtmConfig.Random), sw);

        }

        private static void SerializeHomeostaticPlasticityController(object obj, string name, StreamWriter sw)
        {
            var excludeEntries = new List<string> { "m_OnStabilityStatusChanged" };
            SerializeObject(obj, name, sw, excludeEntries);
        }
        #endregion
        #region Deserialization

        //public static T Deserialize<T>(StreamReader sr, string propName = null)
        //{
        //    var type = typeof(T);

        //    T obj = ReadContent<T>(sr, propName);

        //    return obj;
        //}

        public static T Deserialize<T>(StreamReader sr, string propName = null)
        {
            T obj = default;
            var type = typeof(T);

            if (type.GetInterfaces().FirstOrDefault(i => i.FullName.Equals(typeof(ISerializable).FullName)) != null)
            {
                var methods = type.GetMethods().ToList();
                if (type.BaseType != null)
                {
                    methods.AddRange(type.BaseType.GetMethods());
                }

                var deserializeMethod = methods.FirstOrDefault(m => m.Name == nameof(ISerializable.Deserialize) && m.IsStatic && m.GetParameters().Length == 2);
                if (deserializeMethod == null)
                    throw new NotImplementedException($"Deserialize method is not implemented in the target type {type.Name}");

                obj = (T)deserializeMethod.MakeGenericMethod(type).Invoke(null, new object[] { sr, propName });
                return obj;
            }

            if (IsValueType(type))
            {
                // deserialize value
                obj = DeserializeValue<T>(sr, propName);
            }
            else if (IsDictionary(type))
            {
                // deserialize dictionary
                obj = DeserializeDictionary<T>(sr, propName);
            }
            else if (IsList(type))
            {
                // deserialize list
                obj = DeserializeIEnumerable<T>(sr, propName);
            }
            else
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    obj = DeserializeKeyValuePair<T>(sr, propName);
                }
                else
                    // deserialize object
                    obj = DeserializeObject<T>(sr, propName);
            }

            return obj;
        }

        private static T DeserializeKeyValuePair<T>(StreamReader sr, string propName)
        {
            var type = typeof(T);

            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];


            var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize));

            var content = sr.ReadLine();
            var key = deserializeMethod.MakeGenericMethod(keyType).Invoke(null, new object[] { sr, "key" });
            content = sr.ReadLine();
            var value = deserializeMethod.MakeGenericMethod(valueType).Invoke(null, new object[] { sr, "value" });
            var keyValuePair = (T)Activator.CreateInstance(type, new[] { key, value });

            return keyValuePair;

        }

        private static object DeserializeCell(StreamReader sr, string propName)
        {
            var cell = DeserializeObject<Cell>(sr, propName);

            foreach (var distalDentrite in cell.DistalDendrites)
            {
                distalDentrite.ParentCell = cell;
            }
            return cell;
        }

        private static T DeserializeIEnumerable<T>(StreamReader sr, string propName)
        {
            var type = typeof(T);

            if (type.IsArray && type.GetArrayRank() > 1)
            {
                var array = DeserializeMultidimensionalArray(sr, propName, type);
                return (T)(object)array;
            }

            List<object> enumerable = new List<object>();


            Type elementType;
            string convertMethodName;

            if (type.IsGenericType)
            {
                elementType = type.GetGenericArguments()[0];
                if (IsSet(type))
                {
                    convertMethodName = nameof(System.Linq.Enumerable.ToHashSet);
                }
                else
                {
                    convertMethodName = nameof(System.Linq.Enumerable.ToList);
                }
            }
            else
            {
                elementType = type.GetElementType();
                convertMethodName = nameof(System.Linq.Enumerable.ToArray);
            }

            while (sr.Peek() > 0)
            {
                Type specifiedType = elementType;
                var content = sr.ReadLine().Trim();
                if (content == ReadGenericEnd(propName) || content == ReadGenericEnd(propName, type))
                {
                    break;
                }
                if (string.IsNullOrEmpty(content) || content.StartsWith(ReadGenericBegin(propName)) && content.Contains("CollectionItem") == false)
                {
                    if (content.StartsWith(ReadGenericBegin(propName)) && content.Contains("CollectionItem") == false)
                    {
                        specifiedType = GetSpecifiedTypeOrDefault(content, elementType);
                    }
                    continue;
                }

                var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(specifiedType);

                var item = deserializeMethod.Invoke(null, new object[] { sr, "CollectionItem" });

                enumerable.Add(item);
                //Debug.WriteLine($"Add {item.ToString()} to {propName ?? type.Name}");
            }
            object enumerableCast = CastListToType(enumerable, elementType);

            var convertMethod = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == convertMethodName)?.MakeGenericMethod(elementType);
            T obj = (T)convertMethod?.Invoke(null, new object[] { enumerableCast });

            return obj;
        }

        private static Array DeserializeMultidimensionalArray(StreamReader sr, string propName, Type arrayType)
        {
            Array array = default;
            var elementType = arrayType.GetElementType();

            var dimList = new List<int>();
            if (sr.Peek() > 0)
            {
                sr.ReadLine();
                var rank = Deserialize<int>(sr, nameof(Array.Rank));
                for (int i = 0; i < rank; i++)
                {
                    sr.ReadLine();
                    var dim = Deserialize<int>(sr, $"Dim{i}");
                    dimList.Add(dim);
                }

                array = Array.CreateInstance(elementType, dimList.ToArray());
            }

            while (sr.Peek() > 0)
            {
                var content = sr.ReadLine();
                var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(elementType);

                if (content == ReadGenericEnd(propName) || content == ReadGenericEnd(propName, arrayType))
                {
                    break;
                }
                if (string.IsNullOrEmpty(content) || content == ReadGenericBegin(propName) || content == ReadGenericBegin(propName, arrayType))
                {
                    continue;
                }
                var activeElement = deserializeMethod.Invoke(null, new object[] { sr, "ActiveElement" });
                var activeIndex = Deserialize<int[]>(sr, "ActiveIndex");

                array.SetValue(activeElement, activeIndex);
            }



            return array;
        }

        private static object CastListToType(List<object> enumerable, Type elementType)
        {
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(elementType);
            var enumerableCast = castMethod.Invoke(null, new object[] { enumerable });
            return enumerableCast;
        }

        private static T DeserializeDictionary<T>(StreamReader sr, string propName)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            var type = typeof(T);
            object key = null;
            object value = null;
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];

            var beginKey = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.ReadGenericBegin), BindingFlags.NonPublic | BindingFlags.Static);
            var beginValue = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.ReadGenericBegin), BindingFlags.NonPublic | BindingFlags.Static);
            var beginKeyValuePair = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.ReadGenericBegin), BindingFlags.NonPublic | BindingFlags.Static);
            var endKeyValuePair = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.ReadGenericEnd), BindingFlags.NonPublic | BindingFlags.Static);

            var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize));
            //var deserializeValueMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(typeValue);
            while (sr.Peek() > 0)
            {
                var content = sr.ReadLine().Trim();

                if (content == ReadGenericEnd(propName))
                {
                    break;
                }
                if (string.IsNullOrEmpty(content) || content == (string)beginKeyValuePair.Invoke(null, new object[] { "DictionaryItem", default(Type) }))
                {
                    continue;
                }
                if (content == (string)endKeyValuePair.Invoke(null, new object[] { "DictionaryItem", default(Type) }))
                {
                    key = null;
                    value = null;
                    continue;
                }
                if (content.StartsWith((string)beginKey.Invoke(null, new object[] { "Key", default(Type) })))
                {
                    var specifiedType = GetSpecifiedTypeOrDefault(content, keyType);
                    key = deserializeMethod.MakeGenericMethod(specifiedType).Invoke(null, new object[] { sr, "Key" });
                }
                else if (content.StartsWith((string)beginValue.Invoke(null, new object[] { "Value", default(Type) })))
                {
                    var specifiedType = GetSpecifiedTypeOrDefault(content, valueType);
                    value = deserializeMethod.MakeGenericMethod(specifiedType).Invoke(null, new object[] { sr, "Value" });
                }

                if (key != null && value != null)
                {
                    if (type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                    {
                        var tryAddMethod = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetMethod("TryAdd");
                        tryAddMethod.Invoke(obj, new object[] { key, value });
                    }
                    else
                    {
                        var addMethod = typeof(IDictionary<,>).MakeGenericType(keyType, valueType).GetMethod("Add");
                        addMethod.Invoke(obj, new object[] { key, value });
                    }

                    //Debug.WriteLine($"Try add {key}, {value} to {propName ?? type.Name}");
                }
            }

            return obj;
        }

        /// <summary>
        /// Get specified type from Header "Begin name Type" or default Type
        /// </summary>
        /// <param name="content"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        private static Type GetSpecifiedTypeOrDefault(string content, Type defaultType = null)
        {
            var components = content.Split(' ');
            if (components.Length <= 2 || components.Length > 3)
                return defaultType;
            var type = GetType(components[2]);
            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sr"></param>
        /// <param name="propertyName"></param>
        /// <param name="excludeEntries">Fields or Properties that are skipped (action<> or func<,> type) or deserialized using <paramref name="action"/></param>
        /// <param name="action">Method that deserialize special fields/properties</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(StreamReader sr, string propertyName, List<string> excludeEntries = null, Action<T, string> action = null)
        {
            var type = typeof(T);
            T obj = default;

            if (type.IsInterface || type.IsAbstract)
            {
                return obj;
            }

            var properties = GetProperties(type);
            var fields = GetFields(type);
            while (sr.Peek() > 0)
            {
                if (obj == null)
                {
                    obj = (T)Activator.CreateInstance(type);
                }
                var content = sr.ReadLine().Trim();
                //Debug.WriteLine(content);
                if (content == ReadGenericBegin(propertyName))
                {

                    continue;
                }
                if (content == ReadGenericEnd(propertyName) || content == ReadGenericEnd(propertyName, type))
                {
                    break;
                }

                var components = content.Split(' ');

                if (content.StartsWith("Begin"))
                {
                    if (content == ReadGenericBegin(cReplaceId))
                    {
                        var replaceHashCode = Deserialize<int>(sr, cReplaceId);
                        if (MapObjectHashCode.TryGetValue(replaceHashCode, out object replaceedObj))
                        {
                            obj = (T)replaceedObj;
                        }
                        else
                        {
                            throw new KeyNotFoundException($"Object with hash code {replaceHashCode} cannot be found!");
                        }
                    }
                    if (content == ReadGenericBegin(cIdString))
                    {
                        var hashcode = Deserialize<int>(sr, cIdString);
                        MapObjectHashCode.TryAdd(hashcode, obj);
                        continue;
                    }
                    if (components.Length == 2)
                    {
                        if (excludeEntries == null || excludeEntries.Contains(components[1]) == false)
                        {
                            var property = properties.FirstOrDefault(p => p.Name == components[1]);
                            if (property != null && property.CanWrite)
                            {
                                var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(property.PropertyType);

                                var propertyValue = deserializeMethod.Invoke(null, new object[] { sr, property.Name });
                                property.SetValue(obj, propertyValue);
                            }
                            else
                            {
                                var field = fields.FirstOrDefault(f => f.Name == components[1]);
                                if (field != null && field.IsInitOnly == false)
                                {
                                    var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(field.FieldType);

                                    var fieldValue = deserializeMethod.Invoke(null, new object[] { sr, field.Name });
                                    field.SetValue(obj, fieldValue);
                                }
                            }
                        }
                        else
                        {
                            // Deserialize the excluded fields or properties
                            action?.Invoke(obj, components[1]);
                        }
                    }
                    else if (components.Length == 3)
                    {
                        if (components[1] == "m_PredictiveCells")
                        {

                        }
                        if (excludeEntries == null || excludeEntries.Contains(components[1]) == false)
                        {
                            var property = properties.FirstOrDefault(p => p.Name == components[1]);
                            if (property != null && property.CanWrite)
                            {
                                var implementedType = GetType(components[2]);

                                if (implementedType != null)
                                {
                                    var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(implementedType);

                                    var propertyValue = deserializeMethod.Invoke(null, new object[] { sr, property.Name });
                                    property.SetValue(obj, propertyValue);
                                    //Debug.WriteLine($"set {propertyName ?? type.Name}.{property.Name} to {propertyValue.ToString()}");
                                }
                            }
                            else
                            {
                                var field = fields.FirstOrDefault(f => f.Name == components[1]);
                                if (field != null && field.IsInitOnly == false)
                                {
                                    var implementedType = GetType(components[2]);

                                    if (implementedType != null)
                                    {
                                        var deserializeMethod = typeof(HtmSerializer2).GetMethod(nameof(HtmSerializer2.Deserialize)).MakeGenericMethod(implementedType);

                                        var fieldValue = deserializeMethod.Invoke(null, new object[] { sr, field.Name });
                                        field.SetValue(obj, fieldValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Deserialize the excluded fields or properties
                            action?.Invoke(obj, components[1]);
                        }
                    }
                }
            }

            return obj;
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static object DeserializeHtmConfig(StreamReader sr, string propertyName)
        {
            var excludeEntries = new List<string> { nameof(HtmConfig.Random) };
            var htmConfig = DeserializeObject<HtmConfig>(sr, propertyName, excludeEntries, (config, propertyName) =>
            {
                if (propertyName == nameof(HtmConfig.Random))
                {
                    var seed = Deserialize<int>(sr, propertyName);
                    config.Random = new ThreadSafeRandom(seed);
                }
            });
            return htmConfig;


        }

        private static T DeserializeValue<T>(StreamReader sr, string propertyName)
        {
            T obj = default;
            while (sr.Peek() > 0)
            {
                var content = sr.ReadLine().Trim();

                if (content == ReadGenericEnd(propertyName))
                {
                    break;
                }

                var type = typeof(T);

                if (type == typeof(string))
                {
                    content = content.Trim('"');
                }

                obj = (T)Convert.ChangeType(content, type);
            }
            return obj;
        }
        #endregion

        private static bool IsValueType(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }

        private static bool IsDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)) || type.GetGenericTypeDefinition().IsAssignableFrom(typeof(ConcurrentDictionary<,>)) || type.GetInterfaces().FirstOrDefault(i => i.Name.Contains("IDictionary")) != null);
        }

        public static StreamReader ToStreamReader(string content)
        {
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var reader = new StreamReader(ms);
            return reader;
        }

        private static bool IsList(Type type)
        {
            return type.GetInterfaces().Any(i => i.Name == nameof(IEnumerable));
        }

        private static bool IsSet(Type type)
        {
            return type.IsGenericType && (typeof(ISet<>) == type.GetGenericTypeDefinition() || type.GetInterface("ISet`1") != null);
        }

        #endregion

        private bool TryGetNullableType(Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);

            return underlyingType != null;
        }


        public void SerializeValue(object val, Type type, StreamWriter sw)
        {
            if (type.IsValueType)
            {
                sw.Write(ValueDelimiter);
                sw.Write(val.ToString());
                sw.Write(ValueDelimiter);
                sw.Write(ParameterDelimiter);
            }
            else
            {
                var method = type.GetMethod("Serialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(val, new object[] { sw });
                }
                else
                    throw new NotSupportedException($"No serialization implemented on the type {type}!");
            }
        }

        public T DeserializeValue<T>(StreamReader sr)
        {
            Type type = typeof(T);

            if (type.IsValueType)
            {
                var reader = sr.ReadLine().Trim().Replace(ParameterDelimiter.ToString(), "");
                return (T)Convert.ChangeType(reader, type);
            }
            else
            {
                var method = type.GetMethod("Deserialize");
                if (method != null)
                {
                    return (T)method.Invoke(null, new object[] { sr });
                }
                else
                    throw new NotSupportedException($"No de-serialization implemented on the type {type}!");
            }

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

        ///// <summary>
        ///// Serialize the Dictionary<Segment, List<Synapse>>.
        ///// </summary>
        //public void SerializeValue(Dictionary<Segment, List<Synapse>> keyValues, StreamWriter sw)
        //{
        //    sw.Write(ValueDelimiter);
        //    foreach (KeyValuePair<Segment, List<Synapse>> i in keyValues)
        //    {
        //        i.Key.Serialize(sw);
        //        sw.Write(KeyValueDelimiter);
        //        foreach (Synapse val in i.Value)
        //        {
        //            //val.Serialize(sw);
        //            sw.Write(ValueDelimiter);
        //        }

        //        sw.Write(ElementsDelimiter);
        //    }
        //    sw.Write(ParameterDelimiter);
        //}
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