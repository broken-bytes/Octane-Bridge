using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using OctaneBridge;
using OctaneBridge.Models;

var cfg = Config.Load();
Console.WriteLine($"[bridge] tcp -> 127.0.0.1:{cfg.Port}, ws on 127.0.0.1:{cfg.WsPort}");

var readOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
var writeOpts = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var ws = new WsBroadcaster(new[] { Mapper.EventsChannel, Mapper.StateChannel, Mapper.MetaChannel });
var metaPath = Path.Combine(AppContext.BaseDirectory, cfg.MetaFile);
var metaStore = new MetaStore(metaPath, writeOpts);
Console.WriteLine($"[meta] persisting to {metaPath}");

var queue = Channel.CreateUnbounded<MappedMessage>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
});

metaStore.Changed += meta =>
{
    queue.Writer.TryWrite(new MappedMessage(Mapper.MetaChannel, meta));
};

ws.SetInitialMessage(Mapper.MetaChannel, () =>
{
    var json = JsonSerializer.Serialize(metaStore.Current, writeOpts);
    return Encoding.UTF8.GetBytes(json);
});

ws.SetHttpHandler(Mapper.MetaChannel, async (httpCtx, token) =>
{
    httpCtx.Response.Headers["Access-Control-Allow-Origin"] = "*";
    httpCtx.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
    httpCtx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
    httpCtx.Response.Headers["Cache-Control"] = "no-store";

    var method = httpCtx.Request.HttpMethod;
    if (method == "OPTIONS")
    {
        httpCtx.Response.StatusCode = 204;
        httpCtx.Response.Close();
        return;
    }

    if (method == "GET")
    {
        var json = JsonSerializer.Serialize(metaStore.Current, writeOpts);
        var bytes = Encoding.UTF8.GetBytes(json);
        httpCtx.Response.ContentType = "application/json; charset=utf-8";
        httpCtx.Response.ContentLength64 = bytes.Length;
        await httpCtx.Response.OutputStream.WriteAsync(bytes, token);
        httpCtx.Response.Close();
        return;
    }

    if (method == "POST")
    {
        try
        {
            using var reader = new StreamReader(httpCtx.Request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync(token);
            var next = JsonSerializer.Deserialize<OctaneMeta>(body, writeOpts);
            if (next is null)
            {
                httpCtx.Response.StatusCode = 400;
                httpCtx.Response.Close();
                return;
            }
            metaStore.Update(next);
            httpCtx.Response.StatusCode = 204;
            httpCtx.Response.Close();
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[meta] bad POST body: {ex.Message}");
            httpCtx.Response.StatusCode = 400;
            httpCtx.Response.Close();
        }
        return;
    }

    httpCtx.Response.StatusCode = 405;
    httpCtx.Response.Close();
});

var tcp = new TcpReader(readOpts);
tcp.OnMessage += raw =>
{
    try
    {
        var mapped = Mapper.Map(raw);
        if (mapped is not null)
        {
            queue.Writer.TryWrite(mapped);
        }
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
