using System.Security.Cryptography;
using InTagEntitiesLayer.Interfaces;

namespace InTagLogicLayer.Services
{
    /// <summary>
    /// Local disk file storage for development. Swap with AzureBlobStorageService in production.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task<StoredFileResult> UploadAsync(Stream stream, string fileName, string folder)
        {
            var safeName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
            var folderPath = Path.Combine(_basePath, folder);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, safeName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            // Compute hash
            fileStream.Position = 0;
            stream.Position = 0;
            var hash = await ComputeHashAsync(stream);

            return new StoredFileResult
            {
                StoragePath = Path.Combine(folder, safeName).Replace('\\', '/'),
                Hash = hash,
                FileSize = stream.Length
            };
        }

        public Task<Stream> DownloadAsync(string storagePath)
        {
            var fullPath = Path.Combine(_basePath, storagePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {storagePath}");

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string storagePath)
        {
            var fullPath = Path.Combine(_basePath, storagePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string storagePath)
        {
            var fullPath = Path.Combine(_basePath, storagePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        private static async Task<string> ComputeHashAsync(Stream stream)
        {
            using var sha = SHA256.Create();
            var hashBytes = await sha.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
