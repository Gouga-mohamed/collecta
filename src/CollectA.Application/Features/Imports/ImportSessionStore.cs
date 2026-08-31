using System.Collections.Concurrent;

namespace CollectA.Application.Features.Imports;

public class ImportSession
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<string[]> Rows { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class ImportSessionStore
{
    private static readonly ConcurrentDictionary<Guid, ImportSession> _sessions = new();

    public static void Set(ImportSession session)
    {
        _sessions[session.Id] = session;
    }

    public static ImportSession? Get(Guid id)
    {
        _sessions.TryGetValue(id, out var session);
        return session;
    }

    public static void Remove(Guid id)
    {
        _sessions.TryRemove(id, out _);
    }
}
