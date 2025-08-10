using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Web.Services;

public record ReportItem(string Name, string? Year, long Size, DateTimeOffset? LastModified, string? Summary, string? DownloadUrl);

public interface IBlobService
{
    Task<IEnumerable<ReportItem>> ListAsync(DateTime? from, DateTime? to, int sasMinutes);
    Task UploadAsync(Stream file, string fileName, string? year, string? summary);
}

public class BlobService(IConfiguration cfg) : IBlobService
{
    private readonly string _conn = cfg.GetSection("AzureStorage").GetValue<string>("ConnectionString")!;
    private readonly string _containerName = cfg.GetSection("AzureStorage").GetValue<string>("ContainerName")!;

    private BlobContainerClient GetContainer() => new BlobContainerClient(_conn, _containerName);

    public async Task<IEnumerable<ReportItem>> ListAsync(DateTime? from, DateTime? to, int sasMinutes)
    {
        var container = GetContainer();
        await container.CreateIfNotExistsAsync(PublicAccessType.None);
        var items = new List<ReportItem>();
        await foreach (var blob in container.GetBlobsAsync())
        {
            var lm = blob.Properties.LastModified?.UtcDateTime;
            if (from.HasValue && (lm == null || lm < from.Value.ToUniversalTime())) continue;
            if (to.HasValue && (lm == null || lm > to.Value.ToUniversalTime())) continue;

            var bc = container.GetBlobClient(blob.Name);
            var sas = BuildSas(bc, TimeSpan.FromMinutes(sasMinutes));
            items.Add(new ReportItem(
                Name: blob.Name,
                Year: ExtractYear(blob.Name),
                Size: blob.Properties.ContentLength ?? 0,
                LastModified: blob.Properties.LastModified,
                Summary: blob.Metadata.TryGetValue("summary", out var s) ? s : null,
                DownloadUrl: sas
            ));
        }
        return items.OrderByDescending(i => i.Year).ThenBy(i => i.Name);
    }

    public async Task UploadAsync(Stream file, string fileName, string? year, string? summary)
    {
        var container = GetContainer();
        await container.CreateIfNotExistsAsync(PublicAccessType.None);
        var blob = container.GetBlobClient(fileName);
        await blob.UploadAsync(file, overwrite: true);
        var md = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(summary)) md["summary"] = summary;
        if (!string.IsNullOrWhiteSpace(year)) md["year"] = year!;
        await blob.SetMetadataAsync(md);
    }

    private static string? ExtractYear(string name)
    {
        var m = System.Text.RegularExpressions.Regex.Match(name, "(20\d{2})");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string BuildSas(BlobClient blob, TimeSpan ttl)
    {
        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException("Blob client cannot generate SAS. Use key or MI.");
        var b = new BlobSasBuilder
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(ttl)
        };
        b.SetPermissions(BlobSasPermissions.Read);
        return blob.GenerateSasUri(b).ToString();
    }
}