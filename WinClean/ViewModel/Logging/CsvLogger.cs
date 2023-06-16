using System.Diagnostics;
using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Scover.WinClean.Model;
using Scover.WinClean.Resources;

namespace Scover.WinClean.ViewModel.Logging;

/// <summary>Provides CSV logging.</summary>
public sealed class CsvLogger : Logger, IDisposable
{
    private const string DateTimeFilenameFormat = "yyyy-MM-dd--HH-mm-ss";
    private const string LogFileExtension = ".csv";

    private readonly Lazy<CsvWriter> _csvWriter;

    private readonly string _currentLogFile;

    public CsvLogger()
    {
        _currentLogFile = Path.Join(AppDirectory.Logs, Process.GetCurrentProcess().StartTime.ToString(DateTimeFilenameFormat, DateTimeFormatInfo.InvariantInfo) + LogFileExtension);
        // Defer writer creation. This prevents creating of empty log file at the start of the program.
        _csvWriter = new(() =>
        {
            CsvWriter writer = new(new StreamWriter(_currentLogFile, false, Encoding.Unicode), new CsvConfiguration(CultureInfo.InvariantCulture));
            return writer;
        });
    }

    public override Task ClearLogsAsync() => Task.Run(() =>
    {
        foreach (string logFile in Directory.EnumerateFiles(AppDirectory.Logs, $"*{LogFileExtension}").Where(CanLogFileBeDeleted))
        {
            try
            {
                File.Delete(logFile);
            }
            catch (Exception e) when (e.IsFileSystemExogenous())
            {
                Logs.FailedToDeleteLogFile.FormatWith(logFile, e).Log(LogLevel.Error);
                // Swallow the exception. Failing to delete a log file is not serious enough to justify
                // terminating the application with an unhandled exception.
            }
        }
        Logs.ClearedLogsFolder.Log();
    });

    public void Dispose() => _csvWriter.Value.Dispose();

    protected override void Log(LogEntry entry)
    {
        lock (_csvWriter)
        {
            _csvWriter.Value.NextRecord();
            _csvWriter.Value.WriteRecord(entry);
            _csvWriter.Value.Flush();
        }
    }

    private bool CanLogFileBeDeleted(string logFile)
        => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(logFile), DateTimeFilenameFormat,
                                  DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _) && logFile != _currentLogFile;
}