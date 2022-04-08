// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoCortexApi.Network
{
    /// <summary>
    /// Holds general context information.
    /// </summary>
    public class CortexNetworkContext
    {
        private List<Type> m_AllEncoders = new List<Type>();

        public IHtmModule MyProperty { get; set; }

        /// <summary>
        /// Gets all available encoders.
        /// </summary>
        public List<Type> Encoders { get => m_AllEncoders; }

        /// <summary>
        /// Loads all implemented encoders in all load assemblies.
        /// </summary>
        public CortexNetworkContext()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.Contains("Microsoft."))
                    continue;

                foreach (var tp in asm.GetTypes())
                {
                    if (typeof(EncoderBase).IsAssignableFrom(tp))
                        this.m_AllEncoders.Add(tp);
                }
            }
        }


        /// <summary>
        /// Creates the encoder instance from specified set of properties.
        /// </summary>
        /// <param name="encoderSettings"></param>
        /// <returns></returns>
        public EncoderBase CreateEncoder(Dictionary<string, object> encoderSettings)
        {
            var encoderType = encoderSettings[EncoderProperties.EncoderQualifiedName] as string;
            if (string.IsNullOrEmpty(encoderType))
                throw new ArgumentException("Property 'encoderType' must be specified.");

            return CreateEncoder(encoderType, encoderSettings);
        }

        /// <summary>
        /// Creates the encoder instance from specified set of properties.
        /// </summary>
        /// <param name="encoderType">Assembly qualified name of the encoder.</param>
        /// <param name="encoderSettings">List of all required parameters for encoder. 
        /// If encoder has already been created, this argument SHOULD be null.</param>
        /// <returns></returns>
        public EncoderBase CreateEncoder(string encoderType, Dictionary<string, object> encoderSettings)
        {
            var encoderTp = this.m_AllEncoders.FirstOrDefault(t => t.FullName == encoderType);
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
