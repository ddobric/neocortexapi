using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeoCortexApi
{

    /**
   * Contains methods for Serialization and Deserialization and is applicable to Spatial Pooler and Temoral Memory Class
   */
    public class HtmSerializer
    {
        /**
       *  Method for Serialization of an object. Can serialize properties and fields o the particular Object to 
       *  Can serialize properties and fields ofthe particular Object to a variable like String or a storage place like file.
       */
        public string Serialize(object instance, string fileName)
        {
            var result = Serialize(instance).ToString();

            File.WriteAllText(fileName, result);

            return result;
        }

        /**
       * Stores and returns the serialized string value through this Stringbuilder.
       * Uses Reflection API and lower level method SerializeMember() to fetch properties and fields of the object and stores the result in the Stringbuilder
       */

        public StringBuilder Serialize(object instance)
        {
            Debug.WriteLine("");
            Debug.WriteLine($"Inst: {instance.GetType().Name}");

            StringBuilder sb = new StringBuilder();

            sb.Append('{');

            bool isFirst = true;

            foreach (PropertyInfo property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                Debug.WriteLine($"Prop: {property.Name}");
                if (property.Name == "Segment")
                    continue;
                SerializeMember(isFirst, sb, property.PropertyType, property.Name, property.GetValue(instance));
                isFirst = false;
            }

            foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Debug.WriteLine($"Field: {field.Name}");
                SerializeMember(isFirst, sb, field.FieldType, field.Name, field.GetValue(instance));
                isFirst = false;
            }

            sb.AppendLine();
            sb.Append("}");

            return sb;
        }
        /**
        * Obtains property and fields of the object by applying some of the lower level generic methods which are implemented in the subsequent section of this code.. 
        * Different methods are applied based on specific attributes like types, type names etc.
        * These conditions are applied to solve some of the complexities involved in Serializing complex classes within HTM
    */
        private void SerializeMember(bool isFirst, StringBuilder sb, Type type, string name, object value)
        {
            if (name.Contains("k__BackingField") || name.Contains("i__Field"))
                return;

            if (isFirst == false)
                sb.Append(",");

            if (type.Name == "String")
                SerializeStringValue(sb, name, value as string);
            else if (type.IsArray || (type.IsGenericType && type.Name == "List`1"))
                SerializeArray(sb, name, value);
            else if (type.Name == "Dictionary`2")
            {
                SerializeDictionary(sb, name, value);
            }
            else if (type.IsGenericType && ((type.Name == "IDistributedDictionary`2") || (type.Name == "ISparseMatrix`1")))
                SerializeCustomType(sb, name, value);
            else if (type.IsClass)
                SerializeComplexValue(sb, name, value);

            else
                SerializeNumericValue(sb, name, JsonConvert.SerializeObject(value));
        }
        /**
         * Serialization Method for some of the specific classes. 
         * Similar Serialization method is impplemented in thos concerned classes and the Serialize method can be invoked through this method in this class.
       */

        private void SerializeCustomType(StringBuilder sb, string name, object value)
        {
            MethodInfo serializeMethod = value.GetType().GetMethods().Where(x => x.Name == "Serialize").FirstOrDefault(x => x.IsGenericMethod);


            if (serializeMethod != null)
            {
                StringBuilder res = serializeMethod.Invoke(value, null) as StringBuilder;

                SerializeStringValue(sb, name, res.ToString());
            }
            else
            {
                AppendPropertyName(sb, name);

                sb.Append("null");

            }



        }
        /**
         * Applied if the type of the concerned property/field is an Array 
         
       */
        private void SerializeArray(StringBuilder sb, string name, object value)
        {
            var arrStr = JsonConvert.SerializeObject(value);

            sb.AppendLine();
            sb.Append('\"');
            sb.Append(name);
            sb.Append('\"');
            sb.Append(": ");
            sb.Append(arrStr);
        }

        /**
         * Applied if the type of the concerned  property/field is a Dictionary
         */
        private void SerializeDictionary(StringBuilder sb, string name, object value)
        {
            var arrStr = JsonConvert.SerializeObject(value);

            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");

            sb.Append(arrStr);
        }
        /**
             * Returns the name of the particular Property and String value. Used for simple properties.
        */

        private void SerializeStringValue(StringBuilder sb, string name, string value)
        {
            AppendPropertyName(sb, name);

            sb.Append("\"");
            sb.Append(value);
            sb.Append("\"");
        }
        /**
         * Returns the name of the particular Property and Numeric value. 
         */
        private void SerializeNumericValue(StringBuilder sb, string name, string value)
        {
            AppendPropertyName(sb, name);

            sb.Append(value);
        }
        /**
         * Returns the name of the particular Property
         */
        private static void AppendPropertyName(StringBuilder sb, string name)
        {
            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");
        }
        /**
         * Applied if the concerned  property/field involves Complex Types like class etc
         */
        private void SerializeComplexValue(StringBuilder sb, string name, object value)
        {
            sb.AppendLine();
            sb.Append("\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append(": ");

            if (value != null)
            {
                var sbComplex = Serialize(value);
                sb.Append(sbComplex.ToString());
            }
            else
            {
                sb.Append("null");
            }
        }
        /**
         * Custom class that uses JsonTextReader to read Json files.
         * Based on the position of the index Json Text Reader allocates Token Type and Values for the particular position in the Json file.
         * This Custom reader reads the token type and states of the parameter and indicates current position from the standpoint of the Object. 
         */

        public class HtmJsonTextReder : JsonTextReader
        {
            public HtmJsonTextReder(TextReader reader) : base(reader)
            {



            }


            public bool IsStart
            {
                get
                {
                    return base.CurrentState == State.ObjectStart;
                }
            }

            public bool IsObjectEnd
            {
                get
                {
                    return base.TokenType == JsonToken.EndObject;
                }
            }

            public bool IsProperty
            {
                get
                {
                    return base.CurrentState == State.Property;
                }
            }

            public bool IsValue
            {
                get
                {
                    return base.CurrentState == State.PostValue;
                }
            }

            public bool IsArrayStart
            {
                get
                {
                    return base.CurrentState == State.ArrayStart;
                }
            }

            public bool IsArrayEnd
            {
                get
                {
                    return base.CurrentState == State.PostValue && base.TokenType == JsonToken.EndArray;
                }
            }

            public bool IsClass
            {
                get
                {
                    return base.CurrentState == State.PostValue;
                }
            }

            public object ReadValue(Type tp)
            {
                object val = null;

                if (tp == typeof(double))
                {
                    val = ReadAsDouble();
                }
                else if (tp == typeof(Int32))
                {
                    val = ReadAsInt32();
                }
                else if (tp == typeof(string))
                {
                    val = ReadAsString();
                }
                else if (tp == typeof(bool))
                {
                    val = ReadAsBoolean();
                }
                else
                {

                }

                return val;
            }

        }
        /**
         * This method Deserializes the Json string based on the output of the Json Text Reader.
         * The type of the property is fetched through Reflection and Creates an instance of the specified type.
         * Lower level methods are called to set the values for the particular property
         */
        public static T Deserialize<T>(string fileName) where T : class
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var json = File.ReadAllText(fileName);

            T res = JsonConvert.DeserializeObject<T>(json, settings);
            List<object> instanceList = new List<object>();
            int instanceCnt = 0;
            object currentInst = null;

            HtmJsonTextReder reader = new HtmJsonTextReder(new StringReader(json));

            Type currentMemberType = null;

            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.IsProperty)
                    {
                        currentMemberType = GetMemberType(GetPropertyName(reader), currentInst.GetType(), out MemberInfo memberInfo);

                        if (currentMemberType.IsPrimitive || currentMemberType == typeof(string))
                        {
                            var val = reader.ReadValue(currentMemberType);

                            SetMemberValue(currentInst, GetPropertyName(reader), val);
                        }
                    }
                    else if (reader.IsValue)
                    {

                    }
                    Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                }
                else
                {
                    if (reader.IsStart)
                    {
                        if (instanceCnt == 0)
                        {
                            instanceList.Add(currentInst = Activator.CreateInstance<T>());
                            instanceCnt++;
                        }
                        else
                        {
                            string propName = GetPropertyName(reader);



                            MemberInfo membInf;
                            Type memberType = GetMemberType(propName, currentInst.GetType(), out membInf);

                            if (memberType == null)
                                Debug.WriteLine($"The prop/field '{reader.Path}' cannot be found on the object '{currentInst.GetType().Name}'");



                            else
                            {
                                var propInst = Activator.CreateInstance(memberType);
                                SetMemberValue(currentInst, propName, propInst);
                                instanceList.Add(currentInst = propInst);
                                instanceCnt++;
                            }
                        }
                    }
                    else if (reader.IsArrayStart)
                    {
                        ReadArray(currentInst, reader, currentMemberType);
                    }
                    else if (reader.IsObjectEnd)
                    {
                        instanceCnt--;
                        instanceList.RemoveAt(instanceCnt);
                        currentInst = instanceList[instanceList.Count - 1];
                    }

                    else
                    {

                    }

                    Console.WriteLine("Token: {0}", reader.TokenType);
                }

            }

            return default(T);
        }
        /**
        * Reads the value form Json Reader for Array Types and cretaes instances
        */
        private static void ReadArray(object currentInst, HtmJsonTextReder reader, Type currentMemberType)
        {
            var prop = GetPropertyName(reader);
            Type lstType = Type.GetType(currentMemberType.AssemblyQualifiedName.Replace("[]", ""));

            List<object> elements = new List<object>();
            do
            {
                var val = reader.ReadValue(lstType);
                if (val == null)
                    break;
                else
                    elements.Add(val);


            } while (true);

            //Type genericList1 = typeof(List<>);
            //Type genericList2 = genericList1.MakeGenericType(lstType);
            dynamic arr;
            if (currentMemberType.Name.Contains("List"))
            {
                arr = Activator.CreateInstance(currentMemberType);
                for (int i = 0; i < elements.Count; i++)
                {
                    arr.Add(elements[i]);
                }
            }
            else
            {
                arr = Array.CreateInstance(lstType, elements.Count);
                for (int i = 0; i < elements.Count; i++)
                {
                    arr.SetValue(elements[i], i);
                }
            }


            SetMemberValue(currentInst, prop, arr);

            //Type t = typeof(List<>).MakeGenericType(lstType);
            //arr.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, currentInst, (object[]) arr);
            //var listOfElements = Activator.CreateInstance(t);
        }

        private void ReadObject(HtmJsonTextReder reader, Type currentMemberType, string propName, object currentInstance)
        {

        }

        private void ReadArray(HtmJsonTextReder reader, Type currentMemberType, string propName, object currentInstance)
        {

        }
        /**
   * Identifies the name of the property from the Json string and returns the same
   */
        private static string GetPropertyName(HtmJsonTextReder reader)
        {
            var tokens = reader.Path.Split('.');
            return tokens[tokens.Length - 1];
        }
        /**
      * Fetches the type of the member and sets the property value of the specified object
      */
        private static void SetMemberValue(object objInst, string propName, object val)
        {
            MemberInfo memberInfo;
            var currentMemberType = GetMemberType(propName, objInst.GetType(), out memberInfo);

            if (memberInfo is PropertyInfo)
            {
                if (((PropertyInfo)memberInfo).SetMethod != null)
                    ((PropertyInfo)memberInfo).SetValue(objInst, val);
            }
            else
                ((FieldInfo)memberInfo).SetValue(objInst, val);
        }
        /**
       * Uses Reflection to filter Fields and Properties and returns type of that particular attribute
       */
        private static Type GetMemberType(string path, Type instanceType, out MemberInfo memberInfo)
        {
            Type memberType;

            var memb = instanceType.GetProperty(path, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (memb == null)
            {
                var fld = instanceType.GetField(path, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                memberType = fld.FieldType;
                memberInfo = fld;
            }
            else
            {
                memberType = ((PropertyInfo)memb).PropertyType;
                memberInfo = memb;
            }

            return memberType;
        }

    }
}