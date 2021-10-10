// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoCortexEntities.NeuroVisualizer
{
    public interface INeuroVisualizer
    {
        Task InitModelAsync(NeuroModel model);

        Task UpdateColumnAsync(List<MiniColumn> columns);

        Task UpdateSynapsesAsync(List<Synapse> synapses);

        Task UpdateCellsAsync(List<Cell> cells);

        Task ConnectToWSServerAsync();

    }
}
