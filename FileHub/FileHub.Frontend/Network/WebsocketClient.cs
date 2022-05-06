using System.Net.WebSockets;
using System.Text;
using FileHub.Service.Datahandling;

namespace FileHub.Frontend.Network
{
    public class WebsocketClient
    {
        private const int  Kilo = 1024;
        private ClientWebSocket Socket { get; set; }
        private int PartSize { get; set; }
        private WebSocketMessageType MessageType { get; set; }
        
        public WebsocketClient(ClientWebSocket webSocket, WebSocketMessageType messageType = WebSocketMessageType.Binary, int partSize = Kilo)
        {
            Socket = webSocket;
            PartSize = partSize;
            MessageType = messageType;
        }
        
        public async Task<bool> Connect(string uri)
        {
            Socket = new ClientWebSocket();

            try
            {
                await Socket.ConnectAsync(new Uri(uri), CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Close()
        {
            Socket?.Dispose();
        }

        public async Task<bool> SendAsync(IBinaryDataHandler binaryHandler)
        {
            try
            {
                await foreach (DataPart part in binaryHandler.ReadPartsAsync(PartSize))
                {
                    var buffer = part.Data;
                    Array.Resize(ref buffer, part.DataLength);
                    SendBytes(buffer);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<byte[]> Receive(IBinaryDataHandler binaryHandler)
        {
            var bytes = new List<byte>();
            
            await foreach (DataPart part in ReadData())
            {
                var arr = part.Data;
                Array.Resize(ref arr, part.DataLength);
                bytes.AddRange(arr);
                //binaryHandler.WritePart(part); // TODO: May move this code section to BinaryDataHandler
            }
            
            return bytes.ToArray();
        }

        private async IAsyncEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                var part = await ReceiveBytes();
                part.LastPart = Socket.State != WebSocketState.Open;
                //Console.WriteLine($"Received result: {Encoding.UTF8.GetString(part.Data)}, of Length: {part.DataLength}"); //todo remove
                yield return part;
            }
        }

        private void SendBytes(byte[] buffer)
        {
            if (Socket.State != WebSocketState.Open)
            {
                throw new Exception("socketstateclosed"); //Todo make semantic exception
            }
            Socket.SendAsync(new ArraySegment<byte>(buffer), MessageType, true, CancellationToken.None);
        }
        
        private async Task<DataPart> ReceiveBytes()
        {
            byte[] buffer = new byte[PartSize];
            WebSocketReceiveResult receiveResult = null;
            
            receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            if (receiveResult.MessageType != WebSocketMessageType.Close && receiveResult.MessageType != MessageType)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                throw new Exception("Invalid message type"); //todo semantic exception
            }

            return new DataPart() {Data = buffer, DataLength = receiveResult.Count};
        }
    }
}