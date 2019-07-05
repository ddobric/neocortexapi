using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkkaSb.Net
{
    public interface IPersistenceProvider
    {
        Task InitializeAsync(string name, Dictionary<string, object> setrtings, bool purgeOnStart = false, ILogger logger = null);

        Task PersistActor(ActorBase actorInstance);

        Task<ActorBase> LoadActor(ActorId actorId);

        Task Purge();
    }
}
