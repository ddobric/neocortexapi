// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace NeoCortexApi.Network
{
    public class CortexLayer<TIN, TOUT> : IHtmModule<TIN, TOUT> , ISerializable
    {
        #region Properties

        

        public string Name { get; set; }

        public Dictionary<string, IHtmModule> HtmModules { get; set; }
        #endregion

        #region Constructors and Initialization
        public CortexLayer()
        {

        }

        public CortexLayer(string name) : this(name, new Dictionary<string, IHtmModule>())
        {

        }

        public CortexLayer(string name, Dictionary<string, IHtmModule> modules)
        {
            this.Name = name;
            this.HtmModules = modules;
        }

        #endregion

        #region Public Methods
        public CortexLayer<TIN, TOUT> AddModule(string moduleName, IHtmModule module)
        {
            this.HtmModules.Add(moduleName, module);
            // connect
            return this;
        }

        private IHtmModule GetModuleByName(string moduleName)
        {
            if (this.HtmModules.ContainsKey(moduleName) == false)
                throw new ArgumentException($"Cannot find module with name {moduleName}");

            return this.HtmModules[moduleName];
        }

        /// <summary>
        /// Outputs of evey module in the pipeline.
        /// </summary>
        private Dictionary<string, object> m_Results = new Dictionary<string, object>();

        /// <summary>
        /// Gets the result of the specific module inside of the layer's pipeline.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public object GetResult(string moduleName)
        {
            return this.m_Results[moduleName];
        }

        private void SetResult(string moduleName, object result)
        {
            if (this.m_Results.ContainsKey(moduleName))
                this.m_Results[moduleName] = result;
            else
                this.m_Results.Add(moduleName, result);
        }

        /// <summary>
        /// Computes over the pipeline of installed modules.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="learn">FALSE: Infering mode, TRUE: Learning Mode.</param>
        /// <returns></returns>
        public TOUT Compute(TIN input, bool learn)
        {
            //results.Clear();

            object moduleOutput = null;

            int i = 0;

            var keys = this.HtmModules.Keys;

            foreach (var key in new List<string>(keys))
            {
                dynamic module = this.HtmModules[key];

                dynamic moduleInput = (i == 0) ? input : moduleOutput;

                moduleOutput = module.Compute(moduleInput, learn);

                SetResult(key, moduleOutput);

                i++;
            }

            return (TOUT)moduleOutput;
        }
        //protected virtual TOUT ComputeFollowingModules(string moduleName, int[] input, bool learn)
        //{
        //    var followingModules = this.ConnectionGraph.GetFollowingModules(moduleName);
        //    if (followingModules.Count > 0)
        //    {
        //        foreach (var module in followingModules)
        //        {
        //            var output = ((dynamic)module).Compute(input, learn);
        //            return ComputeFollowingModules(module.Name, output, learn);
        //        }
        //    }
        //}


        #endregion

        public override bool Equals(object obj)
        {
            var layer = obj as CortexLayer<TIN, TOUT>;
            if (layer == null)
                return false;
            return this.Equals(layer);
        }

        public bool Equals(CortexLayer<TIN, TOUT> other)
        {
            if (Name != other.Name)
                return false;
            if (this.HtmModules == null)
                return other.HtmModules == null;

            foreach (var item in this.HtmModules.Keys)
            {
                if (other.HtmModules.TryGetValue(item, out var value))
                {
                    if (!this.HtmModules[item].Equals(value))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool Equals(IHtmModule other)
        {
            return this.Equals((object)other);
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {

            if (obj is CortexLayer<object,object> layer)
            {
                
                var model_tm = "Model_tm.txt";
                StreamWriter sw_tm = new StreamWriter(model_tm);
                var tm = (TemporalMemory)layer.HtmModules["tm"];
                tm.Serialize(tm, null, sw_tm);
                sw_tm.Close();

                
                var modelTrace = "ModelTrace.txt";
                StreamWriter sw_sp = new StreamWriter(modelTrace);
                var sp = (SpatialPoolerMT)layer.HtmModules["sp"];
                sp.Serialize(sp, null, sw_sp) ;
                                     
            }
            
        }

        public static object Deserialize<T>(StreamReader sr)
        {
            // We will use 100 bits to represent an input vector( pattern).
            int inputBits = 100;
            double max = 20;

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            var model_tm = "Model_tm.txt";
            var modelTrace = "ModelTrace.txt";
        
            StreamReader sr_tm = new StreamReader(model_tm);
            var tm = TemporalMemory.Deserialize<TemporalMemoryMT>(sr_tm, null);
            sr_tm.Close();
        
            StreamReader sr_sp = new StreamReader(modelTrace);
            var sp = SpatialPooler.Deserialize(sr_sp);
            sr_sp.Close();

            EncoderBase encoder = new ScalarEncoder(settings);

            CortexLayer<object, object> layer = new CortexLayer<object, object>("L");
            layer.HtmModules.Add("encoder", encoder);
            layer.HtmModules.Add("sp", (sp));
            layer.HtmModules.Add("tm", (TemporalMemory)tm);

            return layer;
        }
        #region Private Methods

        #endregion
    }
}
