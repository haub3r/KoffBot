using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KoffBot.Models;
using System.Text;
using System.Text.Json;

namespace KoffBot.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<List<T>> GetAllAsync<T>(string containerName) where T : DefaultLog
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var results = new List<T>();

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var response = await blobClient.DownloadContentAsync();
            var entity = JsonSerializer.Deserialize<T>(response.Value.Content.ToString());
            if (entity is not null)
            {
                results.Add(entity);
            }
        }

        return results;
    }

    public async Task<T> GetLatestAsync<T>(string containerName) where T : DefaultLog
    {
        var all = await GetAllAsync<T>(containerName);
        return all.OrderByDescending(x => x.Created).FirstOrDefault();
    }

    public async Task<int> GetCountAsync(string containerName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var count = 0;

        await foreach (var _ in containerClient.GetBlobsAsync())
        {
            count++;
        }

        return count;
    }

    public async Task AddAsync<T>(string containerName, T entity) where T : DefaultLog
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobPath = GetBlobPath(entity);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var json = JsonSerializer.Serialize(entity);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/json" });
    }

    private static string GetBlobPath<T>(T entity) where T : DefaultLog
    {
        var created = entity.Created;
        return $"{created.Year}/{created.Month:D2}/{created.Day:D2}/{entity.Id}.json";
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}
