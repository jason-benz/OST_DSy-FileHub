namespace FileHub.Service.Datahandling
{
    public interface IBinaryDataHandler
    {
        public void WritePart(DataPart part);
        public IAsyncEnumerable<DataPart> ReadPartsAsync(int partSizeInBytes);
        public void Close();
    }
}