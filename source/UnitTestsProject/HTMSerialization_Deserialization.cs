using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;


namespace UnitTestsProject
{
    [TestClass]
    public class HTMSerialization_Deserialization
    {
        [TestMethod]
        [TestCategory("Deserialization")]
        public void DeserializeValueTest()
        {
            HtmDeserializer2 htm = new HtmDserializer2();

            using (StreamWriter sw = new StreamWriter("ser.txt"))
            {
                htm.SerializeBegin("UnitTest", sw);

                htm.SerializeValue(12, sw);
                htm.SerializeValue(15.34, sw);
                htm.SerializeValue(8765421, sw);
                htm.SerializeValue("olleH", sw);
                htm.SerializeValue(true, sw);
                htm.SerializeEnd("UnitTest", sw);
            }

            using (StreamReader sr = new StreamReader("ser.txt"))
            {
                int intfulldata;
                double doublefulldata;
                long longfulldata;
                string stringfulldata;
                bool boolfulldata;
                while (sr.Peek() > 0)
                {
                    string data = sr.ReadLine();
                    if (data == string.Empty || data == htm.ReadBegin("UnitTest"))
                    {
                        continue;
                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    {
                                        intfulldata = htm.ReadIntValue(str[i]);
                                        break;
                                    }
                                case 1:
                                    {
                                        doublefulldata = htm.ReadDoubleValue(str[i]);
                                        break;
                                    }
                                case 2:
                                    {
                                        longfulldata = htm.ReadLongValue(str[i]);
                                        break;
                                    }
                                case 3:
                                    {
                                        stringfulldata = htm.ReadStringValue(str[i]);
                                        break;
                                    }
                                case 4:
                                    {
                                        boolfulldata = htm.ReadBoolValue(str[i]);
                                        break;
                                    }
                                default:
                                    { break; }

                            }
                        }
                    }
                }
            }
        }


         public void DeserializeArrayDouble()
        {
            HtmDeserializer2 htm = new HtmDeserializer2();
            Double[] visual = new Double[10];
            Double[] visual1 = new Double[10];
            using (StreamWriter sw = new StreamWriter($"ser_{nameof(DeserializeArrayDouble)}.txt"))
            {
                htm.DeserializeBegin("UnitTest", sw);

                for (int i = 0; i < 10; i++)
                {
                    visual[i] = i;
                }

                htm.DeserializeValue(visual, sw);

                htm.DeserializeEnd("UnitTest", sw);
            }
            using (StreamReader sr = new StreamReader($"ser_{nameof(DeserializeArrayDouble)}.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string data = sr.ReadLine();

                    if (data == String.Empty || data == htm.ReadBegin("UnitTest"))
                    {
                        continue;
                    }
                    else if (data == htm.ReadEnd("UnitTest"))
                    {
                        break;
                    }
                    else
                    {
                        string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                        for (int i = 0; i < str.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    visual1 = htm.ReadArrayDouble(str[i]);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }
            Assert.IsTrue(visual1.SequenceEqual(visual));

            public static SpatialPooler Deserialize(StreamReader sr)
        {
            SpatialPooler sp = new SpatialPooler();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(SpatialPooler)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(HomeostaticPlasticityController)))
                {
                    sp.m_HomeoPlastAct = HomeostaticPlasticityController.Deserialize(sr);
                }
                else if (data == ser.ReadBegin(nameof(Connections)))
                {
                    sp.connections = Connections.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(SpatialPooler)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    //sp.MaxInibitionDensity = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    sp.Name = ser.ReadStringValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return sp;
        
        }

}
