// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
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

        Task ConnectToWSServerAsync(string url, ClientWebSocket websocket);
    }
}
