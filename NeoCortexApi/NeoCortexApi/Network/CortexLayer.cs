using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace NeoCortexApi.Network
{
    public class CortexLayer : IHtmModule
    {
        #region Properties

        public string Name { get; set; }

        public List<IHtmModule> HtmModules { get; set; }
        #endregion


        #region Constructors and Initialization
        public CortexLayer(string name) : this(name, new List<IHtmModule>())
        {

        }

        public CortexLayer(string name, List<IHtmModule> modules)
        {
            this.Name = name;
            this.HtmModules = modules;
        }

        #endregion

        #region Public Methods
        public CortexLayer AddModule(IHtmModule module)
        {
            this.HtmModules.Add(module);
            // connect
            return this;
        }

        public class CONN
        {
            public List<IHtmModule> GetFollowingModules(string moduleName)
            {
                return this.ConnectorMap[moduleName];
            }

            public Dictionary<string, List<IHtmModule>> ConnectorMap = new Dictionary<string, List<IHtmModule>>();
        }

        public void Connect(string inModule, string outModule)
        {
            if (this.ConnectionGraph.ConnectorMap.ContainsKey(inModule) == false)
                this.ConnectionGraph.ConnectorMap.Add(inModule, new List<IHtmModule>());

            this.ConnectionGraph.ConnectorMap[inModule].Add(GetModuleByName(outModule));
        }

        private IHtmModule GetModuleByName(string moduleName)
        {
            var module = this.HtmModules.FirstOrDefault(m => m.Name == moduleName);
            if (module == null)
                throw new ArgumentException($"Cannot find module with name {moduleName}");

            return module;
        }
        public CONN ConnectionGraph = new CONN();

        public void Compute(int[] input, bool learn)
        {
            IHtmModule module = this.HtmModules[0];

            var output = module.Compute(input, learn);
            if (output is IIntegerArrayData)
            {
                ComputeFollowingModules(module.Name, ((IIntegerArrayData)output).Data, learn);
            }
            else
                throw new NotImplementedException();

            //var followingModules = this.Connections.GetFollowingModules(module.Name);

            //ParallelOptions opts = new ParallelOptions();
            //opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

            //Parallel.ForEach(followingModules, opts, (followingModule) =>
            //{
            //    if (output is IIntegerArrayData)
            //    {
            //        var output1 = followingModule.Compute(((IIntegerArrayData)output).Data, learn);
            //    }
            //});
        }
        protected virtual void ComputeFollowingModules(string moduleName, int[] input, bool learn)
        {
            var followingModules = this.ConnectionGraph.GetFollowingModules(moduleName);
            if (followingModules.Count > 0)
            {
                ParallelOptions opts = new ParallelOptions();
                opts.MaxDegreeOfParallelism = Environment.ProcessorCount;

                Parallel.ForEach(followingModules, opts, (followingModule) =>
                {
                    var output = followingModule.Compute(input, learn);

                    if (output is IIntegerArrayData)
                    {
                        var output1 = followingModule.Compute(((IIntegerArrayData)output).Data, learn);
                    }
                    else
                        throw new NotImplementedException();
                });
            }
        }


        IModuleData IHtmModule.Compute(int[] input, bool learn)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
