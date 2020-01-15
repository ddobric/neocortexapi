using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModelAsync(NeuroModel model);

        Task UpdateColumnOverlapsAsync(List<MiniColumn> columns);

        Task UpdateSynapsesAsync(List<SynapseData> synapses);

        Task Connect(string url, ClientWebSocket websocket);
    }
}
