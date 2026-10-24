using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Anhängende JSONL-Ereignishistorie mit Aufbewahrungsgrenze.</summary>
internal sealed class EventLog
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly object _gate = new();
    private readonly string _path;

    public EventLog(string path)
    {
        _path = path;
    }

    /// <exception cref="RhythmStorageException">Ereignis konnte nicht persistiert werden.</exception>
    public void Append(RhythmEvent rhythmEvent)
    {
        ArgumentNullException.ThrowIfNull(rhythmEvent);

        lock (_gate)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                var line = JsonSerializer.Serialize(rhythmEvent, SerializerOptions) + Environment.NewLine;
                File.AppendAllText(_path, line, Encoding.UTF8);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new RhythmStorageException("Ereignis konnte nicht gespeichert werden.", ex);
            }
        }
    }

    public IReadOnlyList<RhythmEvent> ReadAll()
    {
        lock (_gate)
        {
            if (!File.Exists(_path))
            {
                return Array.Empty<RhythmEvent>();
            }

            var result = new List<RhythmEvent>();
            try
            {
                foreach (var line in File.ReadLines(_path, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        var parsed = JsonSerializer.Deserialize<RhythmEvent>(line, SerializerOptions);
                        if (parsed is not null)
                        {
                            result.Add(parsed);
                        }
                    }
                    catch (JsonException)
                    {
                        // Einzelne beschädigte Zeile darf die Historie nicht unlesbar machen.
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return result;
            }

            return result;
        }
    }

    /// <summary>Entfernt Ereignisse älter als die konfigurierte Aufbewahrungsdauer.</summary>
    public void Prune(DateTime localNow, int retentionDays)
    {
        if (retentionDays <= 0)
        {
            return;
        }

        lock (_gate)
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return;
                }

                var cutoff = localNow.Date.AddDays(-retentionDays);
                var kept = ReadAll().Where(e => e.AtLocal >= cutoff).ToList();
                var tempPath = _path + ".tmp";
                var builder = new StringBuilder();
                foreach (var rhythmEvent in kept)
                {
                    builder.Append(JsonSerializer.Serialize(rhythmEvent, SerializerOptions)).Append(Environment.NewLine);
                }

                File.WriteAllText(tempPath, builder.ToString(), Encoding.UTF8);
                File.Move(tempPath, _path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Aufräumen ist best effort.
            }
        }
    }
}
