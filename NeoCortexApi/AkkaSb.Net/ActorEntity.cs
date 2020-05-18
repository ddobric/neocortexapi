// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSb.Net
{
    using Microsoft.Azure.Cosmos.Table;
    public class ActorEntity : TableEntity
    {
        public ActorEntity()
        {
        }

        public ActorEntity(string actorSystemId, string actorId)
        {
            PartitionKey = actorSystemId;
            RowKey = actorId;
        }

        public string SerializedActor { get; set; }

    }
}
