using CsvHelper;
using CsvHelper.Configuration;

using Scover.WinClean.DataAccess;

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

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
        _csvWriter = new(new StreamWriter(_currentLogFile.FullName, true, System.Text.Encoding.Unicode), new CsvConfiguration(CultureInfo.InvariantCulture));
        _csvWriter.WriteHeader<LogEntry>();
    }

    public static Logger Instance { get; } = new();

    /// <value>A string that briefly describes what's happening in the program right now.</value>
    public string? Happening { get; set; }

    /// <summary>Empties the log folder, except for the current log file.</summary>
    public async void ClearLogsFolderAsync()
        => await Task.Run(() =>
        {
            IEnumerable<FileInfo> deletableLogFiles = AppDirectory.LogDir.Info.EnumerateFiles("*.csv").Where(csvFile => CanLogFileBeDeleted(csvFile));

            foreach (FileInfo logFile in deletableLogFiles)
            {
                logFile.Delete();
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
        lvl ??= LogLevel.Verbose;
        if (App.Settings.LogLevel <= lvl.Value)
        {
            _csvWriter.NextRecord();
            _csvWriter.WriteRecord(new LogEntry()
            {
                Date = DateTime.Now,
                Level = lvl.Name.ToString(),
                Happening = Happening ?? string.Empty,
                Message = message,
                Caller = caller,
                CallFile = Path.GetFileName(callFile), // Only keep the filename of the source file to avoid showing personal information about the path of the project.
                CallLine = callLine
            });
            _csvWriter.Flush(); // This is to force the writer to be done when leaving the method.
        }
    }

    private bool CanLogFileBeDeleted(FileInfo logFile)
        => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(logFile.Name), DateTimeFilenameFormat,
                                  DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _) && logFile.Name != _currentLogFile.Name;

    ///<remarks>The fields are in the order we want the CSV header to be in. Topmost = leftmost</remarks>
    private record LogEntry
    {
        public string Level { get; init; } = LogLevel.Verbose.Name;
        public DateTime Date { get; init; }
        public string Happening { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Caller { get; init; } = string.Empty;
        public int CallLine { get; init; }
        public string CallFile { get; init; } = string.Empty;
    }
}