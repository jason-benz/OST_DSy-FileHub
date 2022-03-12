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
        private const UInt32  kilo = 1024;
        private WebSocket Socket { get; set; }
        private UInt64 PartSize { get; set; }
        public WebsocketHandler(WebSocket webSocket, UInt64 partSize = 8 * kilo)
        {
            this.Socket = webSocket;
        }

        public Task Read(IBinaryDataHandler binaryHandler)
        {
            return Task.Run(() =>
            {
                int partNr = 0;
                foreach (byte[] part in ReadBinaryData())
                {
                    Console.WriteLine($"{partNr++}: {Encoding.Default.GetString(part)}");
                }
            });
        }

        public IEnumerable<byte[]> ReadBinaryData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                yield return ReceiveBytes().GetAwaiter().GetResult();
            }
        }

        public Task<byte[]> ReceiveBytes()
        {
            return Task.Run(() =>
            {
                byte[] receiveBuffer = new byte[PartSize];
                WebSocketReceiveResult receiveResult = Socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None).GetAwaiter().GetResult();
                if (receiveResult.MessageType != WebSocketMessageType.Binary && receiveResult.MessageType != WebSocketMessageType.Close)
                {
                    Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                    throw new InvalidMessageTypeException();
                }
                return receiveResult.MessageType == WebSocketMessageType.Binary ?  receiveBuffer : Array.Empty<byte>();
            });
            
        }
    }
}