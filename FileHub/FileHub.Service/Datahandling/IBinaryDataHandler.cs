namespace FileHub.Service.Datahandling
{
    public interface IBinaryDataHandler
    {
        public void WritePart(byte[] part);
        public byte[] ReadPart(int partSizeInBytes);
    }
}