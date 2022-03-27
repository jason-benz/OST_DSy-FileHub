using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileHub.Service.Datahandling
{
    public class BinaryArchitect : IBinaryDataHandler
    {
        private string FilePath { get; set; }
        private FileStream FileStream { get; set; }

        private bool InUse { get; set; }
        public BinaryArchitect(string fileName, string groupId)
        {
            
            FilePath = $"./{fileName}";  //$"./{DataFolderName}/{groupId}/{fileName}";
        }
        public void WritePart(DataPart part)
        {
            if (!InUse)
            {
                FileStream = File.Open(FilePath, FileMode.Create);
                InUse = true;
            }
            
            FileStream.Write(part.Data ?? Array.Empty<byte>(), 0, part.DataLength);

            if (part.LastPart)
            {
                InUse = false;
            }
        }

        public IEnumerable<DataPart> ReadParts(int partSizeInBytes)
        {
            if (!InUse)
            {
                FileStream = File.Open(FilePath, FileMode.Open);
            }
            var dataPart = ReadDataPart(partSizeInBytes);
            yield return dataPart;
            var lastPart = dataPart.LastPart;
            while (!lastPart)
            {
                dataPart = ReadDataPart(partSizeInBytes);
                yield return dataPart;
                lastPart = dataPart.LastPart;
            }

            InUse = false;
        }

        private DataPart ReadDataPart(int partSizeInBytes)
        {
            byte[] data = new byte[partSizeInBytes];
            int length = FileStream.Read(data, 0, partSizeInBytes);
            return new DataPart {Data = data, DataLength = length, LastPart = length < partSizeInBytes}; //todo check if LastPart is asserted correctly
        }

        public void Close()
        {
            FileStream.Close();
        }
        ~BinaryArchitect()
        {
            this?.Close();
        }
    }
}