using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using OctaneBridge;

var cfg = Config.Load();
Console.WriteLine($"[bridge] tcp -> 127.0.0.1:{cfg.Port}, ws on 127.0.0.1:{cfg.WsPort}");

var readOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
var writeOpts = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var ws = new WsBroadcaster(new[] { Mapper.EventsChannel, Mapper.StateChannel });
var queue = Channel.CreateUnbounded<MappedMessage>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = true
});

var tcp = new TcpReader(readOpts);
tcp.OnMessage += raw =>
{
    try
    {
        var mapped = Mapper.Map(raw);
        if (mapped is not null) queue.Writer.TryWrite(mapped);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[map] {ex.Message}");
    }
};

var pumpTask = Task.Run(async () =>
{
    await foreach (var mapped in queue.Reader.ReadAllAsync(cts.Token))
    {
        try
        {
            var json = JsonSerializer.Serialize(mapped.Payload, mapped.Payload.GetType(), writeOpts);
            await ws.BroadcastAsync(mapped.Channel, json, cts.Token);
        }
        catch (OperationCanceledException) { break; }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[pump] {ex.Message}");
        }
    }
});

var wsTask = ws.RunAsync(cfg.WsPort, cts.Token);
var tcpTask = tcp.RunAsync("127.0.0.1", cfg.Port, cts.Token);

await Task.WhenAny(wsTask, tcpTask, pumpTask);
cts.Cancel();
queue.Writer.TryComplete();
try { await Task.WhenAll(wsTask, tcpTask, pumpTask); } catch { }
