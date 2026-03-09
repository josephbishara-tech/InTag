namespace InTagEntitiesLayer.Interfaces
{
    public class StoredFileResult
    {
        public string StoragePath { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public long FileSize { get; set; }
    }

    public interface IFileStorageService
    {
        Task<StoredFileResult> UploadAsync(Stream stream, string fileName, string folder);
        Task<Stream> DownloadAsync(string storagePath);
        Task DeleteAsync(string storagePath);
        Task<bool> ExistsAsync(string storagePath);
    }
}
