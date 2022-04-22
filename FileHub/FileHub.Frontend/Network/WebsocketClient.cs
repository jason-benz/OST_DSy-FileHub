using System.Net.WebSockets;
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
        
        public async Task Connect(string uri)
        {
            Socket = new ClientWebSocket();
            await Socket.ConnectAsync(new Uri(uri), CancellationToken.None);
        }

        public void Close()
        {
            Socket?.Dispose();
        }

        public async Task SendAsync(IBinaryDataHandler binaryHandler)
        {
            await foreach (DataPart part in binaryHandler.ReadPartsAsync(PartSize))
            {
                SendBytes(part.Data ?? Array.Empty<byte>());
            }
        }

        public async Task<byte[]> Receive(IBinaryDataHandler binaryHandler)
        {
            var bytes = new List<byte>();
            
            await foreach (DataPart part in ReadData())
            {
                bytes.AddRange(part.Data);
                //binaryHandler.WritePart(part); // TODO: May move this code section to BinaryDataHandler
            }
            
            return bytes.ToArray();
        }

        private async IAsyncEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                byte[] data = await ReceiveBytes();
                yield return new DataPart  { Data = data, LastPart = Socket.State != WebSocketState.Open, DataLength = data.Length }; //can be closed, since receivebytes overrides it
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
        
        private async Task<byte[]> ReceiveBytes()
        {
            byte[] buffer = new byte[PartSize];
            WebSocketReceiveResult receiveResult = null;
            
            receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            if (receiveResult.MessageType != WebSocketMessageType.Close && receiveResult.MessageType != MessageType)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                throw new Exception("Invalid message type"); //todo semantic exception
            }

            return buffer;
        }
    }
}