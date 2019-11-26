using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace WebSocketNeuroVisualizer
{

    public class WSNeuroVisualizer : INeuroVisualizer
    {

        static string ipAddress = "ws://localhost:";
        static string port = "5011/";
        static string route = "ws/";
        static string clientName = "client1";
        readonly string url = string.Concat(ipAddress, port, route, clientName);
        //readonly string url = "ws://localhost:5011/ws/client1";

        ClientWebSocket websocket = new ClientWebSocket();
        string msgType = "";

        public async Task InitModelAsync(NeuroModel model)
        {
            model = new NeuroModel
            {
                msgType = "init"

            };
            ;
           await SendData(websocket,  model.ToString(), true);
        }

        public async Task UpdateColumnOverlapsAsync(List<ColumnData> columns)
        {
            ColumnData obj= null;
            for (int i = 0; i < columns.Count; i++)
            {
                 obj = new ColumnData
                {
                    Overlap = columns[i].Overlap,
                    ColDims = columns[i].ColDims,
                    msgType = "updateOverlap"

                };

            }
            await SendData(websocket, obj.ToString(), true);
        }
        public async Task UpdateSynapsesAsync(List<SynapseData> synapses)
        {

      
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
            await SendData(websocket, synapses.ToString(), true);

        }
        public async Task Connect(string url, CancellationToken cancellationToken)
        {
            try
            {
                // once per Minute H:M:S
                websocket.Options.KeepAliveInterval = new TimeSpan(0, 1, 0);
                // ClientWebSocket websocket = new ClientWebSocket();
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
