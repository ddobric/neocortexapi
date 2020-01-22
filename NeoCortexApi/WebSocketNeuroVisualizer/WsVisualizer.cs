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
        // To Fix if list of update or a single update send over the websocket
        string messageType = "";

        public async Task InitModelAsync(NeuroModel model, ClientWebSocket websocket)
        {
            messageType = "init";

            await SendData(websocket, (messageType + model.ToString()), true);
        }

        public async Task UpdateColumnOverlapsAsync(List<MiniColumn> columns, ClientWebSocket websocket)
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
        public async Task UpdateSynapsesAsync(List<SynapseData> synapses, ClientWebSocket websocket)
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

            await SendData(websocket, (messageType + synData.ToString()),true);

        }
        public async Task Connect(string url,  ClientWebSocket websocket)
        {
            try
            {
                if (url == null)
                {
                    throw new ArgumentNullException("url");
                }
                // once per Minute H:M:S
                //Days, housr, minutes, seconds, milliseconds
                websocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 5, 0, 0);
                await websocket.ConnectAsync((new Uri(url)), CancellationToken.None);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }


        }

        public async Task SendData(ClientWebSocket websocket, string message, bool endOfMessage)
        {

            try
            {

                while (websocket.State == WebSocketState.Open)
            {
                    var buffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));

                    await websocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);

                    //var receiveData = new ArraySegment<byte>();
                    //WebSocketReceiveResult result = await websocket.ReceiveAsync(receiveData, CancellationToken.None);
                    //await websocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }

        }

    }

}
