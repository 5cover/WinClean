using System.Diagnostics;
using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Logging;

/// <summary>Provides CSV logging.</summary>
public sealed class CsvLogger : Logger
{
    private const string DateTimeFilenameFormat = "yyyy-MM-dd--HH-mm-ss";

    private readonly CsvWriter _csvWriter;

    private readonly FileInfo _currentLogFile;

    public CsvLogger()
    {
        _currentLogFile = new(AppDirectory.LogDir.Join($"{Process.GetCurrentProcess().StartTime.ToString(DateTimeFilenameFormat, DateTimeFormatInfo.InvariantInfo)}.csv"));
        _csvWriter = new(new StreamWriter(_currentLogFile.FullName, true, Encoding.Unicode), new CsvConfiguration(CultureInfo.InvariantCulture));
        _csvWriter.WriteHeader<LogEntry>();
    }

    /// <summary>Empties the log folder, except for the current log file.</summary>
    public async void ClearLogsFolderAsync()
    {
        await Task.Run(() =>
            {
                foreach (FileInfo logFile in AppDirectory.LogDir.Info.EnumerateFiles("*.csv").Where(CanLogFileBeDeleted))
                {
                    try
                    {
                        logFile.Delete();
                    }
                    catch (Exception e) when (e.IsFileSystem())
                    {
                        // Swallow the filesystem exception and silenty fail to delete the log file. This will avoid unhandled exceptions.
                    }
                }
            }).ConfigureAwait(false);
        Logs.ClearedLogsFolder.Log();
    }

    protected override void Log(LogEntry entry)
    {
        _csvWriter.NextRecord();
        _csvWriter.WriteRecord(entry);
        _csvWriter.Flush(); // This is to force the writer to be done when leaving the method.
    }

    private bool CanLogFileBeDeleted(FileInfo logFile)
        => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(logFile.Name), DateTimeFilenameFormat,
                                  DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _) && logFile.Name != _currentLogFile.Name;
}