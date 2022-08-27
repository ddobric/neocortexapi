// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Network
{
    public class CortexLayer<TIN, TOUT> : IHtmModule<TIN, TOUT>
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
        #region Private Methods

        #endregion
    }
}
