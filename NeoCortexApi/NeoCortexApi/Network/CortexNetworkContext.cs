using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace NeoCortexApi.Network
{
    /// <summary>
    /// Holds general context information.
    /// </summary>
    public class CortexNetworkContext
    {
        private List<Type> allEncoders = new List<Type>();

        public IHtmModule MyProperty { get; set; }

        public EncoderBase CreateEncoder(string encoderType, Dictionary<String, Object> encoderSettings)
        {
            if (allEncoders.Count == 0)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var tp in asm.GetTypes())
                    {
                        if (typeof(EncoderBase).IsAssignableFrom(tp))
                            this.allEncoders.Add(tp);
                    }
                }
            }

            var encoderTp = this.allEncoders.FirstOrDefault(t => t.FullName == encoderType);
            if (encoderTp != null)
            {
                if (encoderTp.IsGenericType)
                {
                    throw new ArgumentException("Encoders cannot be generic types.");
                    //Type constructedClass = encoderTp.MakeGenericType(typeof(T));
                    //EncoderBase<T> instance = Activator.CreateInstance(constructedClass) as EncoderBase<T>;
                    //instance.Initialize(encoderSettings);
                    //return instance;
                }
                else
                {
                    EncoderBase instance = Activator.CreateInstance(encoderTp) as EncoderBase;
                    instance.Initialize(encoderSettings);
                    return instance;
                }              
            }
            else
                throw new ArgumentException($"Specified encoder cannot be resolved. Encoder: {encoderTp}");
           
        }
    }
}
