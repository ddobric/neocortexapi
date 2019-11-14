using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModel(NeuroModel model);
        
        Task UpdateColumnOverlaps(List<ColumnData> columns );

        Task UpdateSynapses(List<SynapseData> synapses);
    }
}
