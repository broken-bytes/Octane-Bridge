using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using OctaneBridge.RawModels.Events;

namespace OctaneBridge;

public sealed class TcpReader
{
    private readonly JsonSerializerOptions _opts;

    public TcpReader(JsonSerializerOptions opts) => _opts = opts;

    public event Action<RawMessage>? OnMessage;

    public async Task RunAsync(string host, int port, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(host, port, ct);
                Console.WriteLine($"[tcp] connected to {host}:{port}");

                using var stream = client.GetStream();
                await ReadLoopAsync(stream, ct);

                Console.WriteLine("[tcp] disconnected");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[tcp] {ex.Message}");
            }

            try { await Task.Delay(TimeSpan.FromSeconds(2), ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task ReadLoopAsync(NetworkStream stream, CancellationToken ct)
    {
        var buf = new byte[16 * 1024];
        using var frame = new MemoryStream();

        int depth = 0;
        bool inString = false;
        bool escape = false;
        bool started = false;

        while (!ct.IsCancellationRequested)
        {
            int n = await stream.ReadAsync(buf, ct);
            if (n == 0) return;

            for (int i = 0; i < n; i++)
            {
                byte b = buf[i];

                if (!started)
                {
                    if (b == (byte)'{')
                    {
                        started = true;
                        depth = 1;
                        inString = false;
                        escape = false;
                        frame.SetLength(0);
                        frame.WriteByte(b);
                    }
                    continue;
                }

                frame.WriteByte(b);

                if (inString)
                {
                    if (escape) escape = false;
                    else if (b == (byte)'\\') escape = true;
                    else if (b == (byte)'"') inString = false;
                    continue;
                }

                if (b == (byte)'"') inString = true;
                else if (b == (byte)'{') depth++;
                else if (b == (byte)'}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        started = false;
                        var text = Encoding.UTF8.GetString(frame.GetBuffer(), 0, (int)frame.Length);
                        Dispatch(text);
                    }
                }
            }
        }
    }

    private void Dispatch(string text)
    {
        RawMessage? msg;
        try
        {
            var normalized = UnwrapStringData(text);
            msg = JsonSerializer.Deserialize<RawMessage>(normalized, _opts);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[tcp] parse error: {ex.Message}");
            Console.WriteLine(text);
            return;
        }
        if (msg is not null) OnMessage?.Invoke(msg);
    }

    private static string UnwrapStringData(string text)
    {
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object) return text;
        if (!root.TryGetProperty("Data", out var data) || data.ValueKind != JsonValueKind.String)
            return text;

        var inner = data.GetString();
        if (string.IsNullOrEmpty(inner)) return text;

        using var innerDoc = JsonDocument.Parse(inner);
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.NameEquals("Data"))
                {
                    writer.WritePropertyName("Data");
                    innerDoc.RootElement.WriteTo(writer);
                }
                else
                {
                    prop.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
