using System.Windows;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptSelectionView
{
    public static readonly DependencyProperty UsageProperty = DependencyProperty.Register(nameof(Usage), typeof(Usage), typeof(ScriptSelectionView));

    public ScriptSelectionView() => InitializeComponent();

    public Usage Usage
    {
        get => (Usage)GetValue(UsageProperty);
        set => SetValue(UsageProperty, value);
    }
}

public sealed class SelectionDataTemplateDictionary : Dictionary<Usage, DataTemplate> { }