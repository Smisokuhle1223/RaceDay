using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayAPI.Services
{
    public class BlobStorageService
    {
        private readonly string? _connectionString;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration config)
        {
            _connectionString = config["AzureStorage:ConnectionString"];
            _containerName = config["AzureStorage:ContainerName"] ?? "raceday-images";
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            var container = new BlobContainerClient(_connectionString, _containerName);
            await container.CreateIfNotExistsAsync();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid()}{extension}");

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blob.Uri.ToString();
        }
    }
}