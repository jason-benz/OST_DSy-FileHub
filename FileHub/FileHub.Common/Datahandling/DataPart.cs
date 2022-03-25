using System;

namespace FileHub.Service.Datahandling
{
    public class DataPart
    {
        public bool LastPart { get; set; }
        public byte[]? Data { get; set; }
        public int DataLength { get; set; }
    }
}