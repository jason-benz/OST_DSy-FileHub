using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileHub.Service.Datahandling;

namespace FileHub.Service.Network
{
    public class WebsocketHandler
    {
        private const int  Size = 128 * 1024;
        private WebSocket Socket { get; set; }
        private int PartSize { get; set; }
        private WebSocketMessageType MessageType { get; set; }
        public WebsocketHandler(WebSocket webSocket, WebSocketMessageType messageType = WebSocketMessageType.Binary, int partSize = Size)
        {
            Socket = webSocket;
            PartSize = partSize;
            MessageType = messageType;
        }

        public Task Read(IBinaryDataHandler binaryHandler)
        {
            return Task.Run(() =>
            {
                foreach (DataPart part in ReadData())
                {
                    binaryHandler.WritePart(part);
                }
            });
            
        }

        public async Task Write(IBinaryDataHandler binaryHandler)
        {
            await foreach (DataPart data in binaryHandler.ReadPartsAsync(PartSize))
            {
                SendBytes(data.Data, data.DataLength);
            }
        }

        private IEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                var part = ReceiveBytes().Result;
                part.LastPart = Socket.State != WebSocketState.Open;
                //Console.WriteLine($"Server Receive: {Encoding.UTF8.GetString(part.Data)}, of length: {part.DataLength}");
                yield return part;
            }
        }

        private Task<DataPart> ReceiveBytes()
        {
            return Task.Run(() =>
            {
                byte[] receiveBuffer = new byte[PartSize];
                WebSocketReceiveResult receiveResult = Socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None).GetAwaiter().GetResult();
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else if (receiveResult.MessageType != MessageType)
                {
                    Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                    throw new InvalidMessageTypeException();
                }
                Array.Resize(ref receiveBuffer, receiveResult.Count);
                return new DataPart() {Data = receiveBuffer, DataLength = receiveResult.Count};
            });
            
        }

        private void SendBytes(byte[] data, int length)
        {
            if (Socket.State == WebSocketState.Closed)
            {
                throw new Exception("socket was closed while trying to send");
            }

            Socket.SendAsync(new ArraySegment<byte>(data, 0, length), MessageType, true, CancellationToken.None).Wait();
        } 
    }
}