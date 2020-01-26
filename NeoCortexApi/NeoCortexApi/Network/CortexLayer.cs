using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace NeoCortexApi.Network
{
    public class CortexLayer<TIN, TOUT> : IHtmModule<TIN, TOUT>
    {
        #region Properties

        public string Name { get; set; }

        public Dictionary<string, IHtmModule> HtmModules { get; set; }
        #endregion

        #region Constructors and Initialization
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
        private Dictionary<string, object> results = new Dictionary<string, object>();

        /// <summary>
        /// Gets the result of the specific module inside of the layer's pipeline.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public object GetResult(string moduleName)
        {
            return this.results[moduleName];
        }

        private void SetResult(string moduleName, object result)
        {
            if (this.results.ContainsKey(moduleName))
                this.results[moduleName] = result;
            else
                this.results.Add(moduleName, result);
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

            foreach (var moduleKeyPair in this.HtmModules)
            {
                dynamic module = moduleKeyPair.Value;

                dynamic moduleInput = (i == 0) ? input : moduleOutput;

                moduleOutput = module.Compute(moduleInput, learn);

                SetResult(moduleKeyPair.Key, moduleOutput);

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

        #region Private Methods

        #endregion
    }
}
