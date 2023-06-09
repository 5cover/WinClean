using System.Windows;

using Scover.WinClean.Model;
using Scover.WinClean.Services;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptCodeView
{
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ScriptCodeView));

    public ScriptCodeView() => InitializeComponent();

    public static TypedEnumerableDictionary Metadatas => ServiceProvider.Get<IMetadatasProvider>().Metadatas;

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}