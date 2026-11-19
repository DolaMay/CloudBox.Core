namespace CloudBox.Core.Services;

public interface IContentUploader
{
    Task UploadContentAsync(
        string baseUrl,
        string apiKey,
        Guid uploadId,
        string localFilePath,
        long totalBytes,
        IProgress<string>? progress,
        CancellationToken ct);
}