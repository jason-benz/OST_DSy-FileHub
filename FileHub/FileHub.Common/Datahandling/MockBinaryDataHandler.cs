using System;
using System.Collections.Generic;
using System.Text;

namespace FileHub.Service.Datahandling
{
    public class MockBinaryDataHandler : IBinaryDataHandler
    {
        public void WritePart(DataPart part)
        {
            Console.WriteLine(Encoding.UTF8.GetString(part.Data));
        }

        public IEnumerable<DataPart> ReadParts(int partSizeInBytes)
        {
            yield return new DataPart {Data = Encoding.UTF8.GetBytes("Hello"), DataLength = 5, LastPart = false};
            yield return new DataPart {Data = Encoding.UTF8.GetBytes("World"), DataLength = 5, LastPart = true};
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}