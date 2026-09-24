using System.Collections.Concurrent;

namespace CollectA.Application.Features.Imports;

public class ImportSession
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<string[]> Rows { get; set; } = new();
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class ImportSessionStore
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(2);

    private static readonly ConcurrentDictionary<Guid, ImportSession> _sessions = new();

    public static void Set(ImportSession session)
    {
        CleanupExpired();
        _sessions[session.Id] = session;
    }

    public static ImportSession? Get(Guid id)
    {
        if (!_sessions.TryGetValue(id, out var session)) return null;

        if (DateTime.UtcNow - session.CreatedAt > SessionTtl)
        {
            _sessions.TryRemove(id, out _);
            return null;
        }

        return session;
    }

    public static void Remove(Guid id)
    {
        _sessions.TryRemove(id, out _);
    }

    private static void CleanupExpired()
    {
        var cutoff = DateTime.UtcNow - SessionTtl;
        foreach (var kvp in _sessions)
        {
            if (kvp.Value.CreatedAt < cutoff)
            {
                _sessions.TryRemove(kvp.Key, out _);
            }
        }
    }
}
