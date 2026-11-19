namespace CloudBox.Core.Models;

public record PlanRequest(
    string FileHash,
    string FileName,
    long Size,
    string DeviceId,
    string? ContentType,
    DateTimeOffset FileTime,
    DateTimeOffset LastModified);

public record PlanResponse(Guid UploadId, bool AlreadyComplete, Guid? FileId);

public record CompleteResponse(Guid FileId, bool Deduplicated);

public record HashesResponse(int Count, List<HashesEntry> Items);

public record HashesEntry(
    string FileName,
    long Size,
    DateTimeOffset LastModified,
    string FileHash);

public record UploadOutcome(Guid? FileId, string FileHash, bool Skipped);

public class LocalSyncRecord
{
    public string LocalPath { get; set; } = "";
    public string FileHash { get; set; } = "";
    public long Size { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public Guid? ServerFileId { get; set; }
    public DateTimeOffset? UploadedAt { get; set; }
}

public class ServerFileIndex
{
    public HashSet<string> Hashes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<(string FileName, long Size, long LastModifiedUnix)> Keys { get; } = new();

    public void Add(string fileName, long size, DateTimeOffset lastModified, string fileHash)
    {
        if (!string.IsNullOrEmpty(fileHash))
            Hashes.Add(fileHash);
        Keys.Add((fileName, size, lastModified.ToUnixTimeSeconds()));
    }

    public bool ContainsByMetadata(string fileName, long size, DateTimeOffset lastModified)
        => Keys.Contains((fileName, size, lastModified.ToUnixTimeSeconds()));

    public bool ContainsByHash(string fileHash) => Hashes.Contains(fileHash);
}