using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileHub.Service.Datahandling
{
    public class BinaryArchitect : IBinaryDataHandler
    {
        private const string DataPath = @"D:\";
        private FileStream FileStream { get; }
        private string GroupId { get; }
        private const string DataFolderName = "data"; //todo: make configurable

        public BinaryArchitect(string fileName, string groupId)
        {
            string fullFilePath = $"./{fileName}";  //$"{DataPath}/{DataFolderName}/{GroupId}/{fileName}";
            FileStream = File.Open(fullFilePath, FileMode.Create);
            GroupId = groupId;
        }
        public void WritePart(DataPart part)
        {
            FileStream.Write(part.Data ?? Array.Empty<byte>(), 0, part.DataLength);
        }

        public IEnumerable<DataPart> ReadParts(int partSizeInBytes)
        {
            var dataPart = ReadDataPart(partSizeInBytes);
            yield return dataPart;
            var lastPart = dataPart.LastPart;
            while (!lastPart)
            {
                dataPart = ReadDataPart(partSizeInBytes);
                yield return dataPart;
                lastPart = dataPart.LastPart;
            }
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
            Close();
        }
    }
}