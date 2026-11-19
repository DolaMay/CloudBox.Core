using CloudBox.Core.Models;
using SQLite;

namespace CloudBox.Core.Services;

public class SyncStateStore
{
    private const int FlushThreshold = 20;

    private readonly SQLiteConnection _db;
    private readonly Dictionary<string, LocalSyncRecord> _cache;
    private readonly List<LocalSyncRecord> _pending = new();

    public SyncStateStore()
    {
        var dbPath = Path.Combine(AppPaths.AppDataDirectory, "sync-state.db3");
        _db = new SQLiteConnection(dbPath);

        _db.ExecuteScalar<string>("PRAGMA journal_mode=WAL");
        _db.ExecuteScalar<string>("PRAGMA synchronous=NORMAL");

        _db.CreateTable<SyncRecordEntity>();
        _cache = LoadCache();
    }

    public LocalSyncRecord? Get(string localPath) =>
        _cache.TryGetValue(localPath, out var r) ? r : null;

    public void Upsert(LocalSyncRecord record)
    {
        _cache[record.LocalPath] = record;
        _pending.Add(record);

        if (_pending.Count >= FlushThreshold)
            Flush();
    }

    public void Flush()
    {
        if (_pending.Count == 0) return;

        try
        {
            _db.RunInTransaction(() =>
            {
                foreach (var record in _pending)
                {
                    _db.InsertOrReplace(new SyncRecordEntity
                    {
                        LocalPath = record.LocalPath,
                        FileHash = record.FileHash,
                        Size = record.Size,
                        LastModifiedIso = record.LastModified.ToString("o"),
                        ServerFileId = record.ServerFileId?.ToString(),
                        UploadedAtIso = record.UploadedAt?.ToString("o")
                    });
                }
            });
        }
        catch { }
        finally { _pending.Clear(); }
    }

    private Dictionary<string, LocalSyncRecord> LoadCache()
    {
        var result = new Dictionary<string, LocalSyncRecord>();
        foreach (var e in _db.Table<SyncRecordEntity>())
        {
            result[e.LocalPath] = new LocalSyncRecord
            {
                LocalPath = e.LocalPath,
                FileHash = e.FileHash,
                Size = e.Size,
                LastModified = DateTimeOffset.Parse(e.LastModifiedIso),
                ServerFileId = string.IsNullOrEmpty(e.ServerFileId)
                    ? null : Guid.Parse(e.ServerFileId),
                UploadedAt = string.IsNullOrEmpty(e.UploadedAtIso)
                    ? null : DateTimeOffset.Parse(e.UploadedAtIso)
            };
        }
        return result;
    }
}

[Table("sync_state")]
public class SyncRecordEntity
{
    [PrimaryKey]
    public string LocalPath { get; set; } = "";
    public string FileHash { get; set; } = "";
    public long Size { get; set; }
    public string LastModifiedIso { get; set; } = "";
    public string? ServerFileId { get; set; }
    public string? UploadedAtIso { get; set; }
}