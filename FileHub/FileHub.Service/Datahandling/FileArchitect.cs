using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileHub.Service.Datahandling
{
    public class BinaryArchitect : IBinaryDataHandler
    {
        public static string DataFolderName { get; set; } = "data"; //todo clean
        
        private readonly string _filePath;
        private FileStream _fileStream; 
        
        private bool InUse { get; set; }
        public BinaryArchitect(string fileName, string groupId)
        {
            
            _filePath =  $"./{DataFolderName}/{groupId}/{fileName}";
        }
        public void WritePart(DataPart part)
        {
            if (!InUse)
            {
                _fileStream = File.Open(_filePath, FileMode.Create);
                InUse = true;
            }
            
            _fileStream.Write(part.Data ?? Array.Empty<byte>(), 0, part.DataLength);

            if (part.LastPart)
            {
                InUse = false;
            }
        }

        public async IAsyncEnumerable<DataPart> ReadPartsAsync(int partSizeInBytes)
        {
            if (!InUse)
            {
                _fileStream = File.Open(_filePath, FileMode.Open);
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
            int length = _fileStream.Read(data, 0, partSizeInBytes);
            return new DataPart {Data = data, DataLength = length, LastPart = length < partSizeInBytes}; //todo check if LastPart is asserted correctly
        }

        public void Close()
        {
            _fileStream.Close();
        }
        ~BinaryArchitect()
        {
            this?.Close();
        }
    }
}