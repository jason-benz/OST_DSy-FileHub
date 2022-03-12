using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FileHub.Service.Datahandling;

namespace FileHub.Service.Network
{
    //example used: https://github.com/paulbatum/WebSocket-Samples/blob/master/HttpListenerWebSocketEcho/Server/Server.cs
    class WebsocketServer
    {
        private int connectionsAmount = 0;

        public async void Start(string listenerPrefix)
        {
            HttpListener httpListener = new();
            httpListener.Prefixes.Add(listenerPrefix);
            httpListener.Start();
            Console.WriteLine("Server listening for http connections");

            while (true)
            {
                HttpListenerContext listenerContext = await httpListener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(listenerContext);
                }
                else
                {
                    HandleNonWebsocketConnection(listenerContext);
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            WebSocket webSocket = null;
            try
            {
                webSocket = await UpgradeConnectionToWebSocket(listenerContext);
                WebsocketHandler handler = new WebsocketHandler(webSocket);
                //todo handle real files
                await (handler.Read(new BinaryArchitect("xy")));
                Interlocked.Decrement(ref connectionsAmount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                Console.WriteLine("Connection closed. Amount: {0}", connectionsAmount);
                webSocket?.Dispose();
            }
        }

        private async Task<WebSocket> UpgradeConnectionToWebSocket(HttpListenerContext listenerContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(null);
                Interlocked.Increment(ref connectionsAmount);
                Console.WriteLine("Accepted connection. Amount: {0}", connectionsAmount);
            }
            catch (WebSocketException e)
            {
                HandleUpgradeException(listenerContext, e);
                throw;
            }
            return webSocketContext.WebSocket;
        }

        private void HandleNonWebsocketConnection(HttpListenerContext listenerContext)
        {
            RespondStatus(listenerContext, 400);
        }

        private void HandleUpgradeException(HttpListenerContext listenerContext, Exception e)
        {
            RespondStatus(listenerContext, 500);
            Console.WriteLine($"Exception {e}");
        }

        private void RespondStatus(HttpListenerContext listenerContext, int statusCode)
        {
            listenerContext.Response.StatusCode = statusCode;
            listenerContext.Response.Close();
        }
    }
}