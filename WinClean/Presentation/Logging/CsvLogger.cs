using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Logging;

/// <summary>Provides CSV logging.</summary>
public sealed class CsvLogger : Logger, IDisposable
{
    private const string DateTimeFilenameFormat = "yyyy-MM-dd--HH-mm-ss";
    private const string LogFileExtension = ".csv";

    private readonly CsvWriter _csvWriter;

    private readonly string _currentLogFile;

    public CsvLogger()
    {
        _currentLogFile = Path.Join(AppDirectory.Logs, Process.GetCurrentProcess().StartTime.ToString(DateTimeFilenameFormat, DateTimeFormatInfo.InvariantInfo) + LogFileExtension);
        _csvWriter = new(new StreamWriter(_currentLogFile, false, Encoding.Unicode), new CsvConfiguration(CultureInfo.InvariantCulture));
        _csvWriter.WriteHeader<LogEntry>();
    }

    public override void ClearLogs()
    {
        foreach (string logFile in Directory.EnumerateFiles(AppDirectory.Logs, $"*{LogFileExtension}").Where(CanLogFileBeDeleted))
        {
            try
            {
                File.Delete(logFile);
            }
            catch (Exception e) when (e.IsFileSystem())
            {
                Logs.FailedToDeleteLogFile.FormatWith(logFile, e).Log(LogLevel.Error);
                // Swallow the exception. Failing to delete a log file is not serious enough to justify terminating the
                // application with an unhandled exception.
            }
        }
        Logs.ClearedLogsFolder.Log();
    }

    public void Dispose() => _csvWriter.Dispose();

    protected override void Log(LogEntry entry)
    {
        _csvWriter.NextRecord();
        _csvWriter.WriteRecord(entry);
        _csvWriter.Flush(); // This is to force the writer to be done.
    }

    private bool CanLogFileBeDeleted(string logFile)
        => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(logFile), DateTimeFilenameFormat,
                                  DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _) && logFile != _currentLogFile;
}