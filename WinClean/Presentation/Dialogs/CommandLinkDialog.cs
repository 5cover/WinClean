using System.Collections.ObjectModel;
using System.ComponentModel;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>A cancelable dialog with command links with a Cancel button.</summary>
public sealed class CommandLinkDialog : Dialog
{
    private readonly Dictionary<CommandLink, TaskDialogButton> _commandLinks = new();
    private CommandLink? _defaultCommandLink;

    public CommandLinkDialog()
    {
        Dlg.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
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

    protected override DialogResult GetResult(TaskDialogButton? clickedButton) => base.GetResult(clickedButton) with
    {
        ClickedCommandLink = IsClosed ? null : _commandLinks.Keys.SingleOrDefault(cl => _commandLinks[cl] == clickedButton)
    };

    private void CommandLink_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var commandLink = (CommandLink)sender.AssertNotNull();
        var dlgCommandLink = _commandLinks[commandLink];
        switch (e.PropertyName)
        {
        case nameof(CommandLink.IsEnabled):
            dlgCommandLink.Enabled = commandLink.IsEnabled;
            break;

        case nameof(CommandLink.Text):
            dlgCommandLink.Text = commandLink.Text;
            break;

        case nameof(CommandLink.Note):
            dlgCommandLink.CommandLinkNote = commandLink.Note;
            break;
        }
    }

    private void CommandLinks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (CommandLink newCommandLink in e.NewItems)
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
        }
        if (e.OldItems is not null)
        {
            foreach (CommandLink oldCommandLink in e.OldItems)
            {
                oldCommandLink.PropertyChanged -= CommandLink_PropertyChanged;
                _ = Dlg.Buttons.Remove(_commandLinks[oldCommandLink]);
                _ = _commandLinks.Remove(oldCommandLink);
            }
        }
    }
}