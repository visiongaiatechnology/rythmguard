using System.Text.Json;
using System.Text.Json.Serialization;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Lädt und speichert die validierte Konfiguration atomar.</summary>
internal sealed class JsonConfigStore
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

    public JsonConfigStore(string path, AppLogger logger)
    {
        _path = path;
        _logger = logger;
    }

    public RhythmSettings Load()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return RhythmSettings.Default.Validate();
                }

                var json = File.ReadAllText(_path);
                var settings = JsonSerializer.Deserialize<RhythmSettings>(json, SerializerOptions)
                    ?? RhythmSettings.Default;
                return settings.Validate();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or RhythmValidationException)
            {
                _logger.Warn("Konfiguration unlesbar, Standardwerte werden verwendet: " + ex.Message);
                TryBackupCorruptFile();
                return RhythmSettings.Default.Validate();
            }
        }
    }

    /// <exception cref="RhythmStorageException">Schreiben war nicht möglich.</exception>
    public void Save(RhythmSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var valid = settings.Validate();

        lock (_gate)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                var tempPath = _path + ".tmp";
                var json = JsonSerializer.Serialize(valid, SerializerOptions);
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new RhythmStorageException("Konfiguration konnte nicht gespeichert werden.", ex);
            }
        }
    }

    private void TryBackupCorruptFile()
    {
        try
        {
            var backup = _path + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            File.Move(_path, backup, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Kein weiterer Handlungsbedarf: Standardwerte werden ohnehin verwendet.
        }
    }
}
