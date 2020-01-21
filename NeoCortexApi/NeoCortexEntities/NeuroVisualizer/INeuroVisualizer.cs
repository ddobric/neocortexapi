using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModelAsync(NeuroModel model, ClientWebSocket websocket);

        Task UpdateColumnOverlapsAsync(List<MiniColumn> columns, ClientWebSocket websocket);

        Task UpdateSynapsesAsync(List<SynapseData> synapses, ClientWebSocket websocket);

        Task Connect(string url, ClientWebSocket websocket);
    }
}
