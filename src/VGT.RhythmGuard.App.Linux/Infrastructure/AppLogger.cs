using System.Globalization;

namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Schlanker, ausfallsicherer Dateilogger. Logging darf die App nie unterbrechen.</summary>
internal sealed class AppLogger
{
    private readonly object _gate = new();
    private readonly string _path;

    public AppLogger(string path)
    {
        _path = path;
    }

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var detail = exception is null
            ? message
            : string.Create(CultureInfo.InvariantCulture, $"{message} | {exception.GetType().Name}: {exception.Message}");
        Write("ERROR", detail);
    }

    private void Write(string level, string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var line = string.Create(
                CultureInfo.InvariantCulture,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");

            lock (_gate)
            {
                File.AppendAllText(_path, line);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // Bewusst verschluckt: Diagnose darf den Rhythmus nicht stoppen.
        }
    }
}
