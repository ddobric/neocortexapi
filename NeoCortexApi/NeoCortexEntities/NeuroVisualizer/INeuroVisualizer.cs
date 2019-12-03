using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModelAsync(NeuroModel model);

        Task UpdateColumnOverlapsAsync(List<MiniColumn> columns);

        Task UpdateSynapsesAsync(List<SynapseData> synapses);
    }
}
