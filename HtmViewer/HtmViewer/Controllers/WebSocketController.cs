using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;

namespace HtmViewer.Controllers
{
    [Route("/ws")]
    public class WebSocketController : Controller
    {
        ConcurrentDictionary<string, string> sessions = new ConcurrentDictionary<string, string>();

        private ILogger<WebSocketController> m_Logger;

        public WebSocketController(ILogger<WebSocketController> logger)
        {
            m_Logger = logger;
        }

        [HttpGet]
        public async Task Get()
        {
            var context = ControllerContext.HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                m_Logger.LogInformation("Websocket conntect from {ip}", context.Connection.RemoteIpAddress);
                m_Logger.LogInformation("Websocket conntect with following ID {}", context.Connection.Id);
                m_Logger.LogInformation("Client Message");
                await SendMsgToWebApp(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        public static async Task SendMsgToWebApp(HttpContext context, WebSocket webSocket)
        {
            //{ "msgType": "init", "data": { "clientType": "NeuroVisualizer"} ""}
           // ws://localhost:5000/ws
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);


                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

        }

    }
}