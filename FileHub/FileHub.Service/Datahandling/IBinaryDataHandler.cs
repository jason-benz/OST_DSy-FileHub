using System;
using System.Collections.Generic;

namespace FileHub.Service.Datahandling
{
    public interface IBinaryDataHandler
    {
        public void WritePart(DataPart part);
        public IEnumerable<DataPart> ReadParts(int partSizeInBytes);
    }
}