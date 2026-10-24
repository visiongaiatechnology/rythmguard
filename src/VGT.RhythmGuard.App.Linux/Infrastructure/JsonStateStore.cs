using System.Text.Json;
using System.Text.Json.Serialization;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Persistenter Laufzeitzustand (Bett-Sitzung, Ausführungsguard).</summary>
internal sealed record RuntimeState
{
    public int SchemaVersion { get; init; } = 1;

    public BedSession? BedSession { get; init; }

    public bool BedPowerActionExecuted { get; init; }
}

/// <summary>Atomare Persistenz des Laufzeitzustands.</summary>
internal sealed class JsonStateStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly object _gate = new();
    private readonly AppLogger _logger;
    private readonly string _path;

    public JsonStateStore(string path, AppLogger logger)
    {
        _path = path;
        _logger = logger;
    }

    public RuntimeState Load()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return new RuntimeState();
                }

                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<RuntimeState>(json, SerializerOptions) ?? new RuntimeState();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                _logger.Warn("Laufzeitzustand unlesbar, frischer Zustand wird verwendet: " + ex.Message);
                return new RuntimeState();
            }
        }
    }

    public void Save(RuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        lock (_gate)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                var tempPath = _path + ".tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(state, SerializerOptions));
                File.Move(tempPath, _path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new RhythmStorageException("Laufzeitzustand konnte nicht gespeichert werden.", ex);
            }
        }
    }
}
