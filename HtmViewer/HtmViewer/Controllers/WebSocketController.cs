using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Newtonsoft.Json;

namespace HtmViewer.Controllers
{
    [Route("/ws")]
    public class WebSocketController : Controller
    {

        private ILogger<WebSocketController> m_Logger;
        private WebSocketManager m_Manager;

        public WebSocketController(ILogger<WebSocketController> logger, WebSocketManager manager)
        {
            m_Logger = logger;
            m_Manager = manager;
        }
        public static void LogInfo(ILogger<WebSocketController> logger, string logMessage)
        {
            logger.LogInformation(logMessage);
        }
        [HttpGet("{client}")]
        public async Task Get(string client)
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                m_Logger.LogInformation("Websocket conntect {client}", client);
                m_Logger.LogInformation("Websocket conntect with following ID {}", context.Connection.Id);
               
                m_Manager.AddConnection(client, webSocket);
                await handleWebSocketRequestsAsync(client, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }


        }

        private async Task handleWebSocketRequestsAsync(string client, WebSocket webSocket)
        {

            //{ "msgType": "init", "data": { "clientType": "NeuroVisualizer"} ""}
            // ws://localhost:5000/ws
            //ws://localhost:5000/ws/client
            //ws://localhost:5000/ws/NeuroVisualizer
            // send the data to the relevant cleint by using client id

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                //TODO:await handleReceiveMessageAsync(client, buffer);
                await handleReceiveMessageAsync(client, buffer, result.Count, result.MessageType, result.EndOfMessage);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                m_Logger.LogInformation("Receive {message}", Encoding.Default.GetString(buffer, 0, result.Count));

            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            m_Manager.RemoveConnection(client);


        }

        private async Task handleReceiveMessageAsync(string client, byte[] buffer, int lengthOfMessage, WebSocketMessageType messageType, bool endOfMessage)
        {
            if (client == "NeuroVisualizer")
            {
                // No Callback to other clients.
            }
            else
            {
                WebSocket neuroViusalizerWebSocket = m_Manager.GetWebSocket("NeuroVisualizer");
                if (neuroViusalizerWebSocket != null)
                {
                    await neuroViusalizerWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, lengthOfMessage), messageType, endOfMessage, CancellationToken.None);

                }
                else
                {
                    m_Logger.LogInformation("Client NeuroVisualizer is not connected");
                }




            }
            //WebSocket neuroViusalizerWebSocket = m_Manager.GetWebSocket("NeuroVisualizer");
            //await neuroViusalizerWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, lengthOfMessage), messageType, endOfMessage, CancellationToken.None);
        }
    }
}