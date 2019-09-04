using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace HtmViewer
{
    public class WebSocketManager
    {
        ConcurrentDictionary<string, WebSocket> m_Sockets;
        private ILogger<WebSocketManager> m_Logger;

        public WebSocketManager(ILogger<WebSocketManager> logger)
        {
            this.m_Logger = logger;
            m_Sockets = new ConcurrentDictionary<string, WebSocket>();
        }

        internal void AddConnection(string client, WebSocket webSocket)
        {
            // TODO Add to dictionary andf overwrite and close overwritten ones.
        }

        internal WebSocket GetWebSocket(string v)
        {
            throw new NotImplementedException();
        }

        internal void RemoveConnection(string client)
        {
            throw new NotImplementedException();
        }
    }
}
