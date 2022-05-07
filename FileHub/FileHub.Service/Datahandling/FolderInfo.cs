namespace FileHub.Service.Datahandling;

public class FolderInfo
{
    public static string DataFolderName => "data";

    public static ICollection<FileInfo> GetFileInfos(string groupId)
    {
        string folderPath = $"./{DataFolderName}/{groupId}";
        string[]? fileNames;
        
        try
        {
            fileNames = Directory.GetFiles(folderPath);

        }
        catch (DirectoryNotFoundException)
        {
            return new List<FileInfo>();
        }

        List<FileInfo> infos = new();
        foreach (var t in fileNames)
        {
            var fileName = Path.GetFileName(t);
            infos.Add( new FileInfo()
            {
                FileName = fileName, 
                FileSize = new System.IO.FileInfo($"{folderPath}/{fileName}").Length
            });
        }

        return infos;
    }
}