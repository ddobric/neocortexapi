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

        readonly string url = "ws://localhost:5000/ws/client1";

        ClientWebSocket websocket = new ClientWebSocket();
        string messageType = "";

        public async Task InitModelAsync(NeuroModel model)
        {
            messageType = "init";

            await SendData(websocket, (messageType + model.ToString()), true);
        }

        public async Task UpdateColumnOverlapsAsync(List<MiniColumn> columns)
        {
            messageType = "updateOverlap";
            // MiniColumn minCol = new MiniColumn(columns[i].AreaId, columns[i].Overlap, columns[i].ColDims.GetLength(0), columns[i].ColDims.GetLength(1));
            string updateOverlap = "";
            for (int i = 0; i < columns.Count; i++)
            {
                updateOverlap = columns[i].ToString();

            }
            await SendData(websocket, (messageType + updateOverlap), true);
        }
        public async Task UpdateSynapsesAsync(List<SynapseData> synapses)
        {
            messageType = "updateSynapse";
            SynapseData synData = null;
      

            for (int syn = 0; syn < synapses.Count; syn++)
            {
                synData = new SynapseData
                {
                    PreCell = synapses[syn].Synapse.SourceCell,
                

                };

                if (synapses[syn].Synapse.Segment is DistalDendrite)
                {
                    DistalDendrite seg = (DistalDendrite)synapses[syn].Synapse.Segment;
                    synData.PostCell = seg.ParentCell;
                }
                else if (synapses[syn].Synapse.Segment is ProximalDendrite)
                {
                    ProximalDendrite seg = (ProximalDendrite)synapses[syn].Synapse.Segment;
                   // synData.PostCell = seg.
                       
                    // DimX = seg.ParentColumnIndex
                    // DImZ = 4;
                }
                else
                    throw new ApplicationException("");

               


            }

            await SendData(websocket, (messageType + synData.ToString()), true);

        }
        public async Task Connect(string url, CancellationToken cancellationToken)
        {
            try
            {
                // once per Minute H:M:S
                websocket.Options.KeepAliveInterval = new TimeSpan(0, 1, 0);
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
