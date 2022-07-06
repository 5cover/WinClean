using Ookii.Dialogs.Wpf;

using Scover.WinClean.Logic;
using Scover.WinClean.Operational;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Resources;

using System.Diagnostics;

namespace Scover.WinClean.Presentation;

/// <summary>Represents the directory where scripts are stored.</summary>
public class ScriptsDir : AppSubdirectory
{
    public static ScriptsDir Instance { get; } = new();
    public override string Name => "Scripts";

    /// <summary>Loads all the scripts present in the scripts directory.</summary>
    /// <remarks>Will not load scripts located in subdirectories.</remarks>
    public IEnumerable<Script> LoadAllScripts()
    {
        List<Script> scripts = new();

        Happenings.ScriptLoading.SetAsHappening();

        IScriptSerializer serializer = new ScriptXmlSerializer(Info);

        foreach (FileInfo script in Info.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly))
        {
            Script? deserializedScript = null;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    deserializedScript = serializer.Deserialize(script);
                }
                catch (Exception e) when (e is System.Xml.XmlException or ArgumentException)
                {
                    Logs.BadScriptData.FormatWith(script.Name).Log(LogLevel.Error);

                    using CustomDialog badScriptData = new(Resources.ScriptsDir.EditTheScriptAndRetry, Resources.ScriptsDir.DeleteTheScript)
                    {
                        MainIcon = TaskDialogIcon.Error,
                        Content = Resources.ScriptsDir.BadScriptDataDialogContent.FormatWith(script.Name),
                        ExpandedInformation = e.ToString()
                    };
                    string result = badScriptData.ShowDialog();

                    if (result == Resources.ScriptsDir.EditTheScriptAndRetry)
                    {
                        using Process notepad = Process.Start("notepad", script.FullName);
                        Logs.NotepadOpened.Log();

                        notepad.WaitForExit();

                        retry = true;
                    }
                    else if (result == Resources.ScriptsDir.DeleteTheScript && YesNoDialog.ScriptDeletion.ShowDialog() == DialogResult.Yes)
                    {
                        script.Delete();
                        Logs.ScriptDeleted.FormatWith(script.Name).Log(LogLevel.Info);
                    }
                }
                catch (Exception e) when (e.FileSystem())
                {
                    Logs.FileSystemErrorAcessingScript.FormatWith(script.Name).Log(LogLevel.Error);
                    retry = FSErrorFactory.MakeFSError<RetryIgnoreExitDialog>(e, FSVerb.Acess, script).ShowDialog() == DialogResult.Retry;
                }
            } while (retry);
            if (deserializedScript is not null)
            {
                scripts.Add(deserializedScript);
            }
        }

        Logs.ScriptsLoaded.FormatWith(scripts.Count).Log(LogLevel.Info);

        return scripts;
    }
}