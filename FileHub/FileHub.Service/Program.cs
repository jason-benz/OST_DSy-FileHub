// See https://aka.ms/new-console-template for more information

using FileHub.Service.Network;

class Program
{
    private static void Main(string[] args)
    {
        var websocketServer = new WebsocketServer();
        websocketServer.Start("http://*:8080/dataservice/");
        while (true)
        {
            Thread.Yield();
        }
    }
    
}



