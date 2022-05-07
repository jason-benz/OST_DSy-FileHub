using System.Net;
using System.Net.WebSockets;
using System.Text;
using FileHub.Service.Datahandling;

namespace FileHub.Service.Network
{
    //example used: https://github.com/paulbatum/WebSocket-Samples/blob/master/HttpListenerWebSocketEcho/Server/Server.cs
    class WebsocketServer
    {
        private int connectionsAmount;
        private WebSocket webSocket;

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
                    listenerContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    HandleNonWebsocketConnection(listenerContext);
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            try
            {
                webSocket = await UpgradeConnectionToWebSocket(listenerContext);
                await RouteHttpRequest(listenerContext.Request, webSocket);
                Interlocked.Decrement(ref connectionsAmount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                Console.WriteLine("Connection closed. Amount: {0}", connectionsAmount);
                Thread.Sleep(100);
                if(webSocket.State == WebSocketState.Open)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                webSocket?.Dispose();
            }
        }

        private async Task<WebSocket> UpgradeConnectionToWebSocket(HttpListenerContext listenerContext)
        {
            WebSocketContext webSocketContext;
            
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

        private async Task RouteHttpRequest(HttpListenerRequest httpRequest, WebSocket webSocket)
        {
            string[] path = httpRequest.Url.AbsolutePath.Split('/');
            if (path.Length < 5)
            {
                throw new Exception("Invalid request path"); //todo replace with more semantic exception
            }

            string operation = path[2];
            string groupId = path[3];
            string fileName = path[4];
            WebsocketHandler handler = new WebsocketHandler(webSocket);
            IBinaryDataHandler fileHandler = new BinaryArchitect(fileName, groupId); 
            
            switch (operation)
            {
                case "send": //todo replace constant
                    await (handler.Read(fileHandler));
                    break;
                case "receive": //todo replace constant
                    await (handler.Write(fileHandler));
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Operation: {operation}");
            }
            fileHandler.Close(); 

        }

        private void HandleNonWebsocketConnection(HttpListenerContext listenerContext)
        {
            string[] path = listenerContext.Request.Url.AbsolutePath.Split('/');
            if (path.Length == 2) //health-check-response
            {
                RespondStatus(listenerContext, 200);
            }
            else if (path.Length == 4 && path[2] == "info")
            {
                listenerContext.Response.ContentType = "text/plain";
                foreach (var fileInfo in FolderInfo.GetFileInfos(path[3]))
                {
                    var buffer = Encoding.UTF8.GetBytes($"{fileInfo.FileName}:{fileInfo.FileSize},");
                    listenerContext.Response.OutputStream.Write(buffer);
                }
                RespondStatus(listenerContext, 200);
            }
            else
            {
                RespondStatus(listenerContext, 400);
            }
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