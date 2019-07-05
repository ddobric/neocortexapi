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
