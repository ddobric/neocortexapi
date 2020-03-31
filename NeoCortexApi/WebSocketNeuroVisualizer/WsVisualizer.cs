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

        
        public async Task InitModelAsync(NeuroModel model, ClientWebSocket websocket)
        {
            WebsocketData dataModel = new WebsocketData()
            {
               MsgType = "init",
               Model = model

            };


            await SendDataAsync(websocket, dataModel);
        }

        public async Task UpdateColumnOverlapsAsync(List<MiniColumn> columns, ClientWebSocket websocket)
        {
            List<object> updateO = new List<object>();
            
            for (int i = 0; i < columns.Count; i++)
            {
                WebsocketData updateOverlap = new WebsocketData()
                {
                    MsgType = "updateOverlap",
                    Columns = columns[i]

                };
                updateO.Add(updateOverlap);

            }
           
            // MiniColumn minCol = new MiniColumn(columns[i].AreaId, columns[i].Overlap, columns[i].ColDims.GetLength(0), columns[i].ColDims.GetLength(1));
            
            //for (int i = 0; i < columns.Count; i++)
            //{
            //    updateOverlap = columns[i].ToString();

            //}
            await SendDataAsync(websocket, updateO);
        }
        public async Task UpdateSynapsesAsync(List<Synapse> synapses, ClientWebSocket websocket)
        {
            List<object> updateS = new List<object>();

            for (int i = 0; i < synapses.Count; i++)
            {
                WebsocketData updateSynapse = new WebsocketData()
                {
                    MsgType = "updateOrAddSynapse",
                    Synapses = synapses[i]

                };
                updateS.Add(updateSynapse);

            }
            await SendDataAsync(websocket, updateS);
            //WebsocketData updateSynapses = new WebsocketData()
            //{
            //    MsgType = "updateSynapse",
            //    Synapses = synapses

            //};

            //SynapseData synData = null;


            //for (int syn = 0; syn < synapses.Count; syn++)
            //{
            //    synData = new SynapseData
            //    {
            //        PreCell = synapses[syn].Synapse.SourceCell,


            //    };

            //    if (synapses[syn].Synapse.Segment is DistalDendrite)
            //    {
            //        DistalDendrite seg = (DistalDendrite)synapses[syn].Synapse.Segment;
            //        synData.PostCell = seg.ParentCell;
            //    }
            //    else if (synapses[syn].Synapse.Segment is ProximalDendrite)
            //    {
            //        ProximalDendrite seg = (ProximalDendrite)synapses[syn].Synapse.Segment;
            //        // synData.PostCell = seg.

            //        // DimX = seg.ParentColumnIndex
            //        // DImZ = 4;
            //    }
            //    else
            //        throw new ApplicationException("");

            //}

            //await SendDataAsync(websocket, updateSynapses);

        }
        public async Task ConnectToWSServerAsync(string url,  ClientWebSocket websocket)
        {
            try
            {
                if (url == null)
                {
                    throw new ArgumentNullException("url");
                }
                //// once per Minute H:M:S
                ////Days, housr, minutes, seconds, milliseconds
                //websocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 10, 0, 0);

                Uri uri = new Uri(url);
                await websocket.ConnectAsync(uri, CancellationToken.None);
                await websocket.CloseAsync(websocket.CloseStatus.Value, websocket.CloseStatusDescription, CancellationToken.None);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }


        }

        public async Task SendDataAsync(ClientWebSocket websocket, object message)
        {

            try
            {
                if (websocket.State != WebSocketState.Open)
                {
                    throw new Exception("Connection is not open.");
                }

                while (websocket.State == WebSocketState.Open)
                {
                    var data = JsonConvert.SerializeObject(message);
                    var encoded = Encoding.UTF8.GetBytes(data);
                    var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

                    await websocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

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
    public struct WebsocketData
    {
        public string MsgType { get; set; }

        public NeuroModel Model { get; set; }

        public List<SynapseData> SynapsesData { get; set; }

        public List<MiniColumn> ColumnsList { get; set; }

        public MiniColumn Columns { get; set; }

        public Synapse Synapses { get; set; }

    }

}
