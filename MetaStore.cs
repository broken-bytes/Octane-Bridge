using System.Text.Json;
using OctaneBridge.Models;

namespace OctaneBridge;

public sealed class MetaStore
{
    private const int WriteDebounceMs = 200;

    private readonly string _path;
    private readonly JsonSerializerOptions _jsonOpts;
    private readonly object _lock = new();
    private OctaneMeta _current;
    private CancellationTokenSource? _writeCts;

    public event Action<OctaneMeta>? Changed;

    public MetaStore(string path, JsonSerializerOptions jsonOpts)
    {
        _path = path;
        _jsonOpts = jsonOpts;
        _current = LoadInitial();
    }

    public OctaneMeta Current
    {
        get { lock (_lock) return _current; }
    }

    public void Update(OctaneMeta next)
    {
        lock (_lock)
        {
            _current = next;
            _writeCts?.Cancel();
            var cts = new CancellationTokenSource();
            _writeCts = cts;
            _ = ScheduleWriteAsync(cts.Token);
        }
        Changed?.Invoke(next);
    }

    private OctaneMeta LoadInitial()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var loaded = JsonSerializer.Deserialize<OctaneMeta>(json, _jsonOpts);
                if (loaded is not null) return loaded;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[meta] load failed: {ex.Message}");
        }
        return new OctaneMeta();
    }

    private async Task ScheduleWriteAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(WriteDebounceMs, ct);
            OctaneMeta snapshot;
            lock (_lock) snapshot = _current;
            var json = JsonSerializer.Serialize(snapshot, _jsonOpts);
            var tmp = _path + ".tmp";
            await File.WriteAllTextAsync(tmp, json, ct);
            File.Move(tmp, _path, overwrite: true);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[meta] write failed: {ex.Message}");
        }
    }
}
