namespace CloudBox.Core.Services;

public record MediaItem
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public long Size { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public DateTimeOffset? TakenAt { get; set; }
    public string ContentType { get; set; } = "";

    /// <summary>平台扫描器在扫描时就把「如何打开流」封进委托，Core 侧调用 OpenStream() 即可。</summary>
    public Func<Stream> OpenStream { get; set; } = () => throw new NotSupportedException();
}

public interface IMediaScanner
{
    Task<List<MediaItem>> ScanPhotosAndVideosAsync(CancellationToken ct = default);
}