using FileHub.Service.Datahandling;
using Microsoft.AspNetCore.Components.Forms;

namespace FileHub.Frontend.Datahandling
{
    public class BinaryDataHandler : IBinaryDataHandler
    {
        private readonly IBrowserFile _file;

        public BinaryDataHandler(IBrowserFile file)
        {
            _file = file;
        }

        public async IAsyncEnumerable<DataPart> ReadPartsAsync(int partSizeInBytes)
        {
            bool lastPart;
            var stream = _file.OpenReadStream(_file.Size);

            do
            {
                byte[] buffer = new byte[partSizeInBytes];
                int length = await stream.ReadAsync(buffer, 0, partSizeInBytes);
                lastPart = length < partSizeInBytes;
                yield return new DataPart { Data = buffer, DataLength = length, LastPart = lastPart };
            } while (!lastPart);
        }

        public void WritePart(DataPart part)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
