using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NeoCortexApi.TemporalMemory;

namespace WebSocketNeuroVisualizer
{
    public class WSNeuroVisualizer : INeuroVisualizer
    {
        public Task InitModel(NeuroModel model)
        {
            throw new NotImplementedException();
        }

        public Task UpdateColumnOverlaps(List<ColumnData> columns)
        {
            throw new NotImplementedException();
        }

        public Task UpdateColumnOverlaps(List<MiniColumn> columns)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSynapses(List<SynapseData> synapses)
        {
            throw new NotImplementedException();
        }



        //public Task UpdateSynapses(List<SynapseData> synapses)
        //{
        //    throw new NotImplementedException();
        //    foreach (var syn in synapses)
        //    {
        //        //syn.Synapse.SourceCell
        //        if (syn.Synapse.Segment is DistalDendrite)
        //        {
        //            DistalDendrite seg = (DistalDendrite)syn.Synapse.Segment;
        //            // POstSynCEll = seg.ParentCell
        //        }
        //        else if (syn.Synapse.Segment is ProximalDendrite)
        //        {
        //            ProximalDendrite seg = (ProximalDendrite)syn.Synapse.Segment;
        //            // DimX = seg.ParentColumnIndex
        //            // DImZ = 4;
        //        }
        //    }

        //}
    }
}
