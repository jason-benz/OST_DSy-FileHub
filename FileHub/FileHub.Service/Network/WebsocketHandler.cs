using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileHub.Service.Datahandling;

namespace FileHub.Service.Network
{
    public class WebsocketHandler
    {
        private const int  Kilo = 1024;
        private WebSocket Socket { get; set; }
        private int PartSize { get; set; }
        private WebSocketMessageType MessageType { get; set; }
        public WebsocketHandler(WebSocket webSocket, WebSocketMessageType messageType = WebSocketMessageType.Binary, int partSize = Kilo)
        {
            this.Socket = webSocket;
            this.PartSize = partSize;
            this.MessageType = messageType;
        }

        public Task Read(IBinaryDataHandler binaryHandler)
        {
            return Task.Run(() =>
            {
                int partNr = 0;
                foreach (DataPart part in ReadData())
                {
                    Console.WriteLine($"{partNr++}: {Encoding.Default.GetString(part.Data)}");
                }
            });
        }

        public Task Write(IBinaryDataHandler binaryHandler)
        {
            return Task.Run(() =>
            {
                foreach(DataPart data in binaryHandler.ReadParts(PartSize))
                {
                    SendBytes(data.Data, data.LastPart);
                }
            });
        }

        private IEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                byte[] data = ReceiveBytes().GetAwaiter().GetResult();
                yield return new DataPart{Data = data, LastPart = Socket.State != WebSocketState.Open}; // != open possible, since receivebytes resets it
            }
        }

        private Task<byte[]> ReceiveBytes()
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
                return receiveResult.MessageType == MessageType ?  receiveBuffer : Array.Empty<byte>(); 
            });
            
        }

        private void SendBytes(byte[] data, bool endOfMessage)
        {
            Socket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), MessageType, endOfMessage,
                CancellationToken.None).Wait();
        } 
    }
}