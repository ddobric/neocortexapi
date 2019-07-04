using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkkaSb.Net
{
    public interface IPersistenceProvider
    {
        Task Initialize(string name, Dictionary<string, object> setrtings, ILogger logger);

        Task SerializeActor(ActorBase actorInstance);

        Task<ActorBase> DeserializeActor(ActorId actorId);
    }
}
