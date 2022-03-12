// See https://aka.ms/new-console-template for more information

using System;
using FileHub.Service.Network;

class Program
{
    private static void Main(string[] args)
    {
        var websocketServer = new WebsocketServer();
        websocketServer.Start("http://127.0.0.1:8080/wsDemo/");
        Console.WriteLine("any key to stop");
        Console.ReadKey();
    }
    
}



