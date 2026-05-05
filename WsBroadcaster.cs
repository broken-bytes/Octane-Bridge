using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace OctaneBridge;

public sealed class WsBroadcaster
{
    private readonly ConcurrentDictionary<Guid, ClientEntry> _clients = new();
    private readonly HashSet<string> _channels;

    public WsBroadcaster(IEnumerable<string> channels)
    {
        _channels = new HashSet<string>(channels, StringComparer.Ordinal);
    }

    private sealed class ClientEntry
    {
        public required WebSocket Socket { get; init; }
        public required string Channel { get; init; }
        public SemaphoreSlim SendLock { get; } = new(1, 1);
    }

    public async Task RunAsync(int port, CancellationToken ct)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();
        Console.WriteLine($"[ws] listening on ws://127.0.0.1:{port}{{{string.Join(",", _channels)}}}");

        ct.Register(() => { try { listener.Stop(); } catch { } });

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try { ctx = await listener.GetContextAsync(); }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }

            var path = ctx.Request.Url?.AbsolutePath ?? "/";

            if (!_channels.Contains(path))
            {
                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
                continue;
            }

            if (!ctx.Request.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                continue;
            }

            _ = HandleClientAsync(ctx, path, ct);
        }
    }

    private async Task HandleClientAsync(HttpListenerContext ctx, string channel, CancellationToken ct)
    {
        WebSocketContext wsCtx;
        try { wsCtx = await ctx.AcceptWebSocketAsync(null); }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ws] accept failed: {ex.Message}");
            return;
        }

        var entry = new ClientEntry { Socket = wsCtx.WebSocket, Channel = channel };
        var id = Guid.NewGuid();
        _clients[id] = entry;
        Console.WriteLine($"[ws] client connected to {channel} ({_clients.Count} total)");

        var buf = new byte[1024];
        try
        {
            while (entry.Socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await entry.Socket.ReceiveAsync(buf, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await entry.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    break;
                }
            }
        }
        catch { }
        finally
        {
            _clients.TryRemove(id, out _);
            try { entry.Socket.Dispose(); } catch { }
            Console.WriteLine($"[ws] client disconnected from {channel} ({_clients.Count} total)");
        }
    }

    public async Task BroadcastAsync(string channel, string json, CancellationToken ct)
    {
        if (_clients.IsEmpty) return;
        var bytes = Encoding.UTF8.GetBytes(json);
        var dead = new List<Guid>();

        var tasks = _clients
            .Where(kv => kv.Value.Channel == channel)
            .Select(kv => SendOneAsync(kv.Key, kv.Value, bytes, dead, ct))
            .ToArray();

        if (tasks.Length == 0) return;
        await Task.WhenAll(tasks);

        foreach (var id in dead) _clients.TryRemove(id, out _);
    }

    private static async Task SendOneAsync(Guid id, ClientEntry entry, byte[] bytes, List<Guid> dead, CancellationToken ct)
    {
        if (entry.Socket.State != WebSocketState.Open)
        {
            lock (dead) dead.Add(id);
            return;
        }
        await entry.SendLock.WaitAsync(ct);
        try
        {
            await entry.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
        }
        catch
        {
            lock (dead) dead.Add(id);
        }
        finally { entry.SendLock.Release(); }
    }
}
