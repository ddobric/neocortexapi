using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace WebSocketNeuroVisualizer
{
 
    public class WSNeuroVisualizer : INeuroVisualizer
    {
        string url = "ws://localhost:5011/ws/client1";

        public Task InitModel(NeuroModel model)
        {
            throw new NotImplementedException();
        }

        public Task UpdateColumnOverlaps(List<ColumnData> columns)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSynapses(List<SynapseData> synapses)
        {
            throw new NotImplementedException();
            foreach (var syn in synapses)
            {
                //syn.Synapse.SourceCell
                if (syn.Synapse.Segment is DistalDendrite)
                {
                    DistalDendrite seg = (DistalDendrite)syn.Synapse.Segment;
                    // POstSynCEll = seg.ParentCell
                }
                else if (syn.Synapse.Segment is ProximalDendrite)
                {
                    ProximalDendrite seg = (ProximalDendrite)syn.Synapse.Segment;
                    // DimX = seg.ParentColumnIndex
                    // DImZ = 4;
                }
            }
            
        }
        public async Task Connect(string url, CancellationToken cancellationToken)
        {
            try
            {
                ClientWebSocket websocket = new ClientWebSocket();
                await websocket.ConnectAsync((new Uri(url)), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }


        }

        public async Task SendData(ClientWebSocket websocket, string message, bool endOfMessage)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                var msg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                await websocket.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Text, endOfMessage, CancellationToken.None);

            }

        }
    }

}
