public interface IFileService
{
    Task<string> SaveFileAsync(string fileName, byte[] content);
}
