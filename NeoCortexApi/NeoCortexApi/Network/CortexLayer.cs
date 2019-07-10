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
        public CortexLayer<TIN, TOUT> AddModule(IHtmModule module)
        {
            this.HtmModules.Add(module);
            // connect
            return this;
        }

        //public class CONN
        //{
        //    public List<IHtmModule> GetFollowingModules(string moduleName)
        //    {
        //        return this.ConnectorMap[moduleName];
        //    }

        //    public Dictionary<string, List<IHtmModule>> ConnectorMap = new Dictionary<string, List<IHtmModule>>();
        //}

        //public void Connect(string inModule, string outModule)
        //{
        //    if (this.ConnectionGraph.ConnectorMap.ContainsKey(inModule) == false)
        //        this.ConnectionGraph.ConnectorMap.Add(inModule, new List<IHtmModule>());

        //    this.ConnectionGraph.ConnectorMap[inModule].Add(GetModuleByName(outModule));
        //}

        private IHtmModule GetModuleByName(string moduleName)
        {
            var module = this.HtmModules.FirstOrDefault(m => m.Name == moduleName);
            if (module == null)
                throw new ArgumentException($"Cannot find module with name {moduleName}");

            return module;
        }
        // public CONN ConnectionGraph = new CONN();

        public TOUT Compute(TIN input, bool learn)
        {
            object moduleOutput = null;

            for (int i = 0; i < this.HtmModules.Count; i++)
            {
                dynamic module = this.HtmModules[i];
                dynamic moduleInput = (i == 0) ? input : moduleOutput;
                moduleOutput = module.Compute(moduleInput, learn);
            }

            return (TOUT)moduleOutput;

          
            //if (output is IIntegerArrayData)
            //{
            //    ComputeFollowingModules(module.Name, ((IIntegerArrayData)output).Data, learn);
            //}
            //else
            //    throw new NotImplementedException();

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
