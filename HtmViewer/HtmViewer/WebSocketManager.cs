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
            m_Sockets.AddOrUpdate(client, webSocket, (existingClient, existingWebSocket) => 
            {
                if (client == existingClient)
                {
                    existingWebSocket = webSocket;
                }
                return webSocket;

            });

        }

        internal WebSocket GetWebSocket(string neuroVisualizer)
        {
            WebSocket webSocket = null;
            if (m_Sockets.ContainsKey(neuroVisualizer))
            {
                m_Sockets.TryGetValue(neuroVisualizer, out webSocket);
            }
            else
            {
                m_Logger.LogInformation("Not Connected {neuroVisualizer}", neuroVisualizer);

            }
            return webSocket;
        }

        internal void RemoveConnection(string client)
        {
            WebSocket webSocket;
            m_Sockets.Remove(client, out webSocket); 
        }
    }
}
