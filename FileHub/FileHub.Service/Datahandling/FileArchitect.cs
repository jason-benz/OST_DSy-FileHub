using System.IO;

namespace FileHub.Service.Datahandling
{
    public class BinaryArchitect : IBinaryDataHandler
    {
        //todo: replace with real file datastructure
        private string File { get; set; }

        public BinaryArchitect(string fileName)
        {
            //todo initialize real datastructure
            File = fileName;
        }
        public void WritePart(byte[] part)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ReadPart(int partSizeInBytes)
        {
            throw new System.NotImplementedException();
        }
    }
}