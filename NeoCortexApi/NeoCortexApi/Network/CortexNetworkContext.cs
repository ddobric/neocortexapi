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

        public EncoderBase<T> CreateEncoder<T>(string encoderType)
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

            var encoderTp = this.allEncoders.FirstOrDefault(t => t.Name == encoderType);
            if (encoderTp != null)
            {
                var tp = typeof(EncoderBase<>);
                Type constructedClass = tp.MakeGenericType(typeof(T));
                EncoderBase<T> instance = Activator.CreateInstance(constructedClass) as EncoderBase<T>;
                return instance;
            }
            else
                throw new ArgumentException();
           
        }
    }
}
