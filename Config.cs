using System.IO;

namespace OctaneBridge;

public sealed record BridgeConfig(int Port, int WsPort, string MetaFile);

public static class Config
{
    private const string FileName = "app.ini";
    private const string DefaultMetaFile = "meta.json";

    public static BridgeConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing config file: {path}");

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';') || line.StartsWith('['))
                continue;
            var eq = line.IndexOf('=');
            if (eq <= 0) continue;
            values[line[..eq].Trim()] = line[(eq + 1)..].Trim();
        }

        if (!values.TryGetValue("PORT", out var portStr) || !int.TryParse(portStr, out var port))
            throw new InvalidOperationException($"Missing or invalid PORT in {path}");

        var wsPort = values.TryGetValue("WS_PORT", out var ws) && int.TryParse(ws, out var p) ? p : 8080;
        var metaFile = values.TryGetValue("META_FILE", out var mf) && mf.Length > 0 ? mf : DefaultMetaFile;
        return new BridgeConfig(port, wsPort, metaFile);
    }
}