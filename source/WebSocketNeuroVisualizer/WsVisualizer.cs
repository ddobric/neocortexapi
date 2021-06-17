// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license
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
        public ClientWebSocket WS { get; set; }

        public async Task InitModelAsync(NeuroModel model)
        {
            WebsocketData dataModel = new WebsocketData()
            {
                MsgType = "init",
                Model = model

            };


            await SendDataAsync(WS, dataModel);
        }

        public async Task UpdateColumnAsync(List<MiniColumn> columns)
        {
            WebsocketData update = new WebsocketData()
            {
                MsgType = "updateOverlap",
                Columns = columns

            };


            await SendDataAsync(WS, update);
        }
        public async Task UpdateSynapsesAsync(List<Synapse> synapses)
        {
            WebsocketData update = new WebsocketData()
            {
                MsgType = "updateOrAddSynapse",
                Synapses = synapses

            };

            await SendDataAsync(WS, update);
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
        public async Task UpdateCellsAsync(List<Cell> cells)
        {
            WebsocketData update = new WebsocketData()
            {
                MsgType = "updateCells",
                Cells = cells

            };

            await SendDataAsync(WS, update);
        }
        public async Task ConnectToWSServerAsync()
        {
            try
            {
                WS = new ClientWebSocket();

                //// once per Minute H:M:S
                ////Days, housr, minutes, seconds, milliseconds
                //websocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 10, 0, 0);

                Uri uri = new Uri("ws://localhost:5555/ws/RunExperimentClient");
                await WS.ConnectAsync(uri, CancellationToken.None);
                await WS.CloseAsync(WS.CloseStatus.Value, WS.CloseStatusDescription, CancellationToken.None);

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

        public List<MiniColumn> Columns { get; set; }

        public List<Synapse> Synapses { get; set; }

        public List<Cell> Cells { get; set; }
    }

}
