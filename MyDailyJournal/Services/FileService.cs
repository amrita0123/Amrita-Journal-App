using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

public class FileService : IFileService
{
    public async Task<string> SaveFileAsync(string fileName, byte[] content)
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, content);
        return filePath;
    }
}