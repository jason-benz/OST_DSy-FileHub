using Microsoft.VisualBasic;

namespace FileHub.Service.Datahandling;

public class FolderInfo
{
    public static string DataFolderName { get; set; } = "data"; //todo clean
    
    public static ICollection<FileInfo> GetFileInfos(string groupId)
    {
        string folderPath = $"./{DataFolderName}/{groupId}";
        string[]? fileNames = null;
        try
        {
            fileNames = Directory.GetFiles(folderPath);

        }
        catch (DirectoryNotFoundException e)
        {
            return new List<FileInfo>();
        }

        List<FileInfo> infos = new();
        for (int i = 0; i < fileNames.Length; i++)
        {
            var fileName = Path.GetFileName(fileNames[i]);
            infos.Add( new FileInfo()
            {
                FileName = fileName, 
                FileSize = new System.IO.FileInfo($"{folderPath}/{fileName}").Length
            });
        }

        return infos;
    }
}