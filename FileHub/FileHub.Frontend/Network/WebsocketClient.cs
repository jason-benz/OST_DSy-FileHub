﻿using System;
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
                //Console.WriteLine($"Sending {Encoding.UTF8.GetString(part.Data)}");
                SendBytes(part.Data ?? Array.Empty<byte>());
            }
        }

        public async Task Receive(IBinaryDataHandler binaryHandler)
        {
            await foreach (DataPart part in ReadData())
            {
                binaryHandler.WritePart(part);
            }
        }

        private async IAsyncEnumerable<DataPart> ReadData()
        {
            while (Socket.State == WebSocketState.Open)
            {
                byte[] data = await ReceiveBytes();
                yield return new DataPart{Data = data, LastPart = Socket.State != WebSocketState.Open, DataLength = data.Length}; //can be closed, since receivebytes overrides it
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
            WebSocketReceiveResult receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else if (receiveResult.MessageType != MessageType)
            {
                Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid Message Type", CancellationToken.None);
                throw new Exception("Invalid message type"); //todo semantic exception
            }


            return buffer;
        }
    }
}