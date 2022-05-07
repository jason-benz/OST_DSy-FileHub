﻿using System.Net.WebSockets;
using FileHub.Service.Datahandling;

namespace FileHub.Frontend.Network
{
    public class WebsocketClient
    {
        private const int  Size = 128 * 1024;
        private ClientWebSocket Socket { get; set; }
        private int PartSize { get; set; }
        private WebSocketMessageType MessageType { get; set; }
        
        public WebsocketClient(ClientWebSocket webSocket, WebSocketMessageType messageType = WebSocketMessageType.Binary, int partSize = Size)
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

        public async Task<byte[]> Receive()
        {
            var bytes = new List<byte>();

            await foreach (DataPart part in ReadData())
            {
                var arr = part.Data;
                Array.Resize(ref arr, part.DataLength);
                bytes.AddRange(arr);
            }

            return bytes.ToArray();
        }

        private async IAsyncEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                var part = await ReceiveBytes();
                part.LastPart = Socket.State != WebSocketState.Open;
                yield return part;
            }
        }

        private void SendBytes(byte[] buffer)
        {
            if (Socket.State != WebSocketState.Open)
            {
                throw new Exception("socketstateclosed");
            }
            Socket.SendAsync(new ArraySegment<byte>(buffer), MessageType, true, CancellationToken.None);
        }
        
        private async Task<DataPart> ReceiveBytes()
        {
            byte[] buffer = new byte[PartSize];
            WebSocketReceiveResult receiveResult = null;
            
            try
            {
                var timeOut = new CancellationTokenSource(2000).Token;
                receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), timeOut);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Timeout reached");
                return new DataPart() {Data = buffer, DataLength = 0};
            }
            
            if (receiveResult.MessageType != WebSocketMessageType.Close && receiveResult.MessageType != MessageType)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                throw new Exception("Invalid message type");
            }
            
            return new DataPart() {Data = buffer, DataLength = receiveResult.Count};
        }
    }
}