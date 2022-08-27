using System.Collections.ObjectModel;
using System.ComponentModel;

using Ookii.Dialogs.Wpf;

namespace Scover.WinClean.Presentation.Dialogs;

public class CommandLinkDialog : Dialog
{
    private readonly Dictionary<CommandLink, TaskDialogButton> _commandLinks = new();
    private CommandLink? _defaultCommandLink;

    public CommandLinkDialog()
    {
        ShowArrowGlyph = true;
        Dlg.Buttons.Add(new(ButtonType.Cancel));
        CommandLinks.CollectionChanged += CommandLinks_CollectionChanged;
    }

    public ObservableCollection<CommandLink> CommandLinks { get; } = new();

    public CommandLink? DefaultCommandLink
    {
        get => _defaultCommandLink;
        set
        {
            if (_defaultCommandLink is not null)
            {
                // Remove old default command link.
                _commandLinks[_defaultCommandLink].Default = false;
            }
            if (value is not null)
            {
                // Set the new default command link.
                _commandLinks[value].Default = true;
            }

            _defaultCommandLink = value;
        }
    }

    /// <summary>Gets or sets whether to show the arrow glyph near command links.</summary>
    /// <remarks>Default value is <see langword="true"/>.</remarks>
    public bool ShowArrowGlyph
    {
        get => Dlg.ButtonStyle == TaskDialogButtonStyle.CommandLinks;
        set => Dlg.ButtonStyle = value ? TaskDialogButtonStyle.CommandLinks : TaskDialogButtonStyle.CommandLinksNoIcon;
    }

    protected override DialogResult GetResult(TaskDialogButton? clickedButton)
    {
        return base.GetResult(clickedButton) with
        {
            ClickedCommandLink = IsClosed ? null : _commandLinks.Keys.SingleOrDefault(cl => ReferenceEquals(_commandLinks[cl], clickedButton))
        };
    }

    private void CommandLink_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var commandLink = (CommandLink)sender!; // ! : Should not be null. Will fail if null.
        var dlgCommandLink = _commandLinks[commandLink];
        // Update the properties on the TaskDialogButton
        dlgCommandLink.Text = commandLink.Text;
        dlgCommandLink.CommandLinkNote = commandLink.Note;
        dlgCommandLink.Enabled = commandLink.IsEnabled;
    }

    private void CommandLinks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (CommandLink newCommandLink in e.NewItems ?? Array.Empty<CommandLink>())
        {
            newCommandLink.PropertyChanged += CommandLink_PropertyChanged;
            TaskDialogButton ookiiCommandLink = new(newCommandLink.Text)
            {
                Enabled = newCommandLink.IsEnabled,
                CommandLinkNote = newCommandLink.Note
            };
            _commandLinks[newCommandLink] = ookiiCommandLink;
            Dlg.Buttons.Add(ookiiCommandLink);
        }

        foreach (CommandLink oldCommandLink in e.OldItems ?? Array.Empty<CommandLink>())
        {
            oldCommandLink.PropertyChanged -= CommandLink_PropertyChanged;
            Dlg.Buttons.Remove(_commandLinks[oldCommandLink]);
            _commandLinks.Remove(oldCommandLink);
        }
    }
}