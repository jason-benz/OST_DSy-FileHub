using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileHub.Service.Datahandling;

namespace FileHub.Frontend.Network
{
    public class WebsocketClient
    {
        private int i = 0;
        private const int  Kilo = 1024;
        private ClientWebSocket Socket { get; set; }
        private int PartSize { get; set; }
        private WebSocketMessageType MessageType { get; set; }
        
        public WebsocketClient(ClientWebSocket webSocket, WebSocketMessageType messageType = WebSocketMessageType.Binary, int partSize = Kilo)
        {
            this.Socket = webSocket;
            this.PartSize = partSize;
            this.MessageType = messageType;
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
                Console.WriteLine();
                SendBytes(part.Data ?? Array.Empty<byte>());
            }
        }

        public async Task<byte[]> Receive(IBinaryDataHandler binaryHandler)
        {
            //var bytes = new List<byte>();
            
            // await foreach (DataPart part in ReadData())
            // {
            //     bytes.AddRange(part.Data);
            //     binaryHandler.WritePart(part);
            // }
            
            var bytes = await ReadData();
            return bytes.ToArray();
        }

        private async Task<List<byte>> ReadData()
        {
            int j = 0;
            var bytes = new List<byte>();

            while (Socket.State == WebSocketState.Open)
            {
                j++;
                Console.WriteLine("ReadData-While: " + j);
                // Thread.Sleep(10); // TODO Remove
                byte[] data = await ReceiveBytes();
                bytes.AddRange(data);
                
                //yield return new DataPart{Data = data, LastPart = Socket.State != WebSocketState.Open, DataLength = data.Length}; //can be closed, since receivebytes overrides it
            }
            Console.WriteLine("ReadData-Final: " + j);
            return bytes;
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
            i++;
            Console.WriteLine("ReceiveBytes: " + i);
            
            byte[] buffer = new byte[PartSize];
            WebSocketReceiveResult receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Close " + i); // TODO: Remove
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else if (receiveResult.MessageType != MessageType)
            {
                Console.WriteLine("Invalid: " + i);
                await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                throw new Exception("Invalid message type"); //todo semantic exception
            }


            return buffer;
        }
    }
}