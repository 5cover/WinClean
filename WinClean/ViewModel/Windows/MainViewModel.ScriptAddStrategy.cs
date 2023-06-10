using System.Diagnostics;

using Optional;

using Scover.Dialogs;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.ViewModel.Windows;

public sealed partial class MainViewModel
{
    private sealed class ScriptAddStrategy
    {
        private readonly Func<ICollection<ScriptViewModel>, string, Option<ScriptViewModel>, bool> _add;

        private ScriptAddStrategy(Func<ICollection<ScriptViewModel>, string, Option<ScriptViewModel>, bool> add) => _add = add;

        public static ScriptAddStrategy Add { get; } = new((scripts, path, script) => script.Match(s =>
        {
            scripts.Add(s);
            Logs.ScriptAdded.FormatWith(path, s).Log();
            return true;
        },
        () => false));

        public static ScriptAddStrategy DontAddBecauseAlreadyExists { get; } = new((_, path, script) =>
        {
            script.MatchSome(s => Logs.ScriptAlreadyExistsCannotAdd.FormatWith(path, s.InvariantName).Log(LogLevel.Info));
            return false;
        });

        public static ScriptAddStrategy DontAddBecauseDeserializationFailed { get; } = new((_, path, _) =>
        {
            Logs.ScriptDeserializationFailedCannotAdd.FormatWith(path).Log(LogLevel.Info);
            return false;
        });

        public static ScriptAddStrategy Overwrite { get; } = new((scripts, path, script) => script.Match(s =>
        {
            Debug.Assert(scripts.Remove(s));
            scripts.Add(s);
            Logs.ScriptOverwritten.FormatWith(path, s.InvariantName).Log(LogLevel.Info);
            return true;
        },
        () => false));

        /// <returns>Whether the script was added.</returns>
        public static bool AddScript(ICollection<ScriptViewModel> scripts, string path, Option<ScriptViewModel> newScript)
        => newScript.Match(s =>
            scripts.Contains(s)
                ? AskUserToOverwriteScript(s)
                    ? Overwrite
                    : DontAddBecauseAlreadyExists
                : Add,
        () => DontAddBecauseDeserializationFailed)._add(scripts, path, newScript);

        private static bool AskUserToOverwriteScript(ScriptViewModel existingScript)
        {
            using Page overwrite = new()
            {
                WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
                Icon = DialogIcon.Warning,
                Content = Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(existingScript.Name),
                Buttons = { Button.Yes, Button.No },
            };
            return Button.Yes.Equals(new Dialog(overwrite).Show(ParentWindow.Active));
        }
    }
}