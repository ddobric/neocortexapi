using NeoCortexApi.Entities;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModelAsync(NeuroModel model);

        Task UpdateColumnAsync(List<MiniColumn> columns);

        Task UpdateSynapsesAsync(List<Synapse> synapses);

        Task ConnectToWSServerAsync();

    }
}
