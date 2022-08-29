using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation;

/// <summary>Provides CSV logging.</summary>
public class Logger
{
    private const string DateTimeFilenameFormat = "yyyy-MM-dd--HH-mm-ss";

    private readonly CsvWriter _csvWriter;

    private readonly FileInfo _currentLogFile;

    private Logger()
    {
        _currentLogFile = new(AppDirectory.LogDir.Join($"{Process.GetCurrentProcess().StartTime.ToString(DateTimeFilenameFormat, DateTimeFormatInfo.InvariantInfo)}.csv"));
        _csvWriter = new(new StreamWriter(_currentLogFile.FullName, true, Encoding.Unicode), new CsvConfiguration(CultureInfo.InvariantCulture));
        _csvWriter.WriteHeader<LogEntry>();
    }

    public static Logger Instance { get; } = new();

    /// <summary>Empties the log folder, except for the current log file.</summary>
    public async void ClearLogsFolderAsync()
        => await Task.Run(() =>
        {
            IEnumerable<FileInfo> deletableLogFiles = AppDirectory.LogDir.Info.EnumerateFiles("*.csv").Where(CanLogFileBeDeleted);

            foreach (FileInfo logFile in deletableLogFiles)
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

    /// <summary>Logs a string.</summary>
    /// <param name="message">The string to log.</param>
    /// <param name="lvl">The level of the log entry.</param>
    /// <param name="caller"><see cref="CallerMemberNameAttribute"/> - Don't specify</param>
    /// <param name="callLine"><see cref="CallerLineNumberAttribute"/> - Don't specify</param>
    /// <param name="callFile"><see cref="CallerFilePathAttribute"/> - Don't specify</param>
    public void Log(string message, LogLevel? lvl = null,
                           [CallerMemberName] string caller = "",
                           [CallerLineNumber] int callLine = 0,
                           [CallerFilePath] string callFile = "")
    {
        _csvWriter.NextRecord();
        _csvWriter.WriteRecord(new LogEntry
        (
            (lvl ?? LogLevel.Verbose).ToString(),
            DateTime.Now,
            message,
            caller,
            callLine,
            // Only keep the filename of the source file to avoid showing personal information about the path of the project.
            Path.GetFileName(callFile)
        ));
        _csvWriter.Flush(); // This is to force the writer to be done when leaving the method.
    }

    private bool CanLogFileBeDeleted(FileInfo logFile)
        => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(logFile.Name), DateTimeFilenameFormat,
                                  DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _) && logFile.Name != _currentLogFile.Name;

    ///<remarks>The properties are in the order we want the CSV header to be in. Topmost = leftmost.</remarks>
    private record LogEntry(string Level,
                            DateTime Date,
                            string Message,
                            string Caller,
                            int CallLine,
                            string CallFile);
}