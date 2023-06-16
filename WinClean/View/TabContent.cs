// Source: https://www.codeproject.com/Articles/460989/WPF-TabControl-Turning-Off-Tab-Virtualization
// License: Apache-2.0 Changes: Changed namespace Formatting & code style

// TabContent.cs, version 1.2 The code in this file is Copyright (c) Ivan Krivyakov See
// http://www.ikriv.com/legal.php for more information

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Scover.WinClean.View;

/// <summary>Attached properties for persistent tab control</summary>
/// <remarks>
/// By default WPF TabControl bound to an ItemsSource destroys visual state of invisible tabs. Set
/// ikriv:TabContent.IsCached="True" to preserve visual state of each tab.
/// </remarks>
public static class TabContent
{
    // Using a DependencyProperty as the backing store for InternalCachedContent. This enables animation,
    // styling, binding, etc...
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly DependencyProperty InternalCachedContentProperty =
        DependencyProperty.RegisterAttached("InternalCachedContent", typeof(ContentControl), typeof(TabContent), new UIPropertyMetadata(null));

    // Using a DependencyProperty as the backing store for InternalContentManager. This enables animation,
    // styling, binding, etc...
    public static readonly DependencyProperty InternalContentManagerProperty =
        DependencyProperty.RegisterAttached("InternalContentManager", typeof(object), typeof(TabContent), new UIPropertyMetadata(null));

    // Using a DependencyProperty as the backing store for InternalTabControl. This enables animation,
    // styling, binding, etc...
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly DependencyProperty InternalTabControlProperty =
        DependencyProperty.RegisterAttached("InternalTabControl", typeof(TabControl), typeof(TabContent), new UIPropertyMetadata(null, OnInternalTabControlChanged));

    /// <summary>Controls whether tab content is cached or not</summary>
    /// <remarks>
    /// When TabContent.IsCached is true, visual state of each tab is preserved (cached), even when the tab
    /// is hidden
    /// </remarks>
    public static readonly DependencyProperty IsCachedProperty =
        DependencyProperty.RegisterAttached("IsCached", typeof(bool), typeof(TabContent), new UIPropertyMetadata(false, OnIsCachedChanged));

    /// <summary>Used instead of TabControl.ContentTemplate for cached tabs</summary>
    public static readonly DependencyProperty TemplateProperty =
        DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent), new UIPropertyMetadata(null));

    /// <summary>Used instead of TabControl.ContentTemplateSelector for cached tabs</summary>
    public static readonly DependencyProperty TemplateSelectorProperty =
        DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent), new UIPropertyMetadata(null));

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ContentControl GetInternalCachedContent(DependencyObject obj) => (ContentControl)obj.GetValue(InternalCachedContentProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static object GetInternalContentManager(DependencyObject obj) => obj.GetValue(InternalContentManagerProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TabControl GetInternalTabControl(DependencyObject obj) => (TabControl)obj.GetValue(InternalTabControlProperty);

    public static bool GetIsCached(DependencyObject obj) => (bool)obj.GetValue(IsCachedProperty);

    public static DataTemplate GetTemplate(DependencyObject obj) => (DataTemplate)obj.GetValue(TemplateProperty);

    public static DataTemplateSelector GetTemplateSelector(DependencyObject obj) => (DataTemplateSelector)obj.GetValue(TemplateSelectorProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetInternalCachedContent(DependencyObject obj, ContentControl value) => obj.SetValue(InternalCachedContentProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetInternalContentManager(DependencyObject obj, object value) => obj.SetValue(InternalContentManagerProperty, value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetInternalTabControl(DependencyObject obj, TabControl value) => obj.SetValue(InternalTabControlProperty, value);

    public static void SetIsCached(DependencyObject obj, bool value) => obj.SetValue(IsCachedProperty, value);

    public static void SetTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(TemplateProperty, value);

    public static void SetTemplateSelector(DependencyObject obj, DataTemplateSelector value) => obj.SetValue(TemplateSelectorProperty, value);

    private static DataTemplate CreateContentTemplate()
    {
        const string xaml =
            "<DataTemplate><Border b:TabContent.InternalTabControl=\"{Binding RelativeSource={RelativeSource AncestorType=TabControl}}\" /></DataTemplate>";

        ParserContext context = new()
        {
            XamlTypeMapper = new XamlTypeMapper(Array.Empty<string>())
        };
        context.XamlTypeMapper.AddMappingProcessingInstruction("b", typeof(TabContent).Namespace, typeof(TabContent).Assembly.FullName);

        context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        context.XmlnsDictionary.Add("b", "b");

        DataTemplate template = (DataTemplate)XamlReader.Parse(xaml, context);
        return template;
    }

    private static void EnsureContentTemplateIsNotModified(TabControl tabControl)
    {
        DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty, typeof(TabControl));
        descriptor.AddValueChanged(tabControl, (sender, args)
            => throw new InvalidOperationException("Cannot assign to TabControl.ContentTemplate when TabContent.IsCached is True. Use TabContent.Template instead"));
    }

    private static void EnsureContentTemplateIsNull(TabControl tabControl)
    {
        if (tabControl.ContentTemplate is not null)
        {
            throw new InvalidOperationException("TabControl.ContentTemplate value is not null. If TabContent.IsCached is True, use TabContent.Template instead of ContentTemplate");
        }
    }

    private static ContentManager GetContentManager(TabControl tabControl, Decorator container)
    {
        ContentManager? contentManager = (ContentManager)GetInternalContentManager(tabControl);
        if (contentManager is not null)
        {
            /*
             * Content manager already exists for the tab control. This means that tab content template is applied
             * again, and new instance of the Border control (container) has been created. The old container
             * referenced by the content manager is no longer visible and needs to be replaced
             */
            contentManager.ReplaceContainer(container);
        }
        else
        {
            // create content manager for the first time
            contentManager = new ContentManager(tabControl, container);
            SetInternalContentManager(tabControl, contentManager);
        }

        return contentManager;
    }

    private static void OnInternalTabControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is null)
        {
            return;
        }

        if (obj is not Decorator container)
        {
            string message = "Cannot set TabContent.InternalTabControl on object of type " + obj.GetType().Name +
                ". Only controls that derive from Decorator, such as Border can have a TabContent.InternalTabControl.";
            throw new InvalidOperationException(message);
        }

        if (args.NewValue is null)
        {
            return;
        }

        if (args.NewValue is not TabControl)
        {
            throw new InvalidOperationException("Value of TabContent.InternalTabControl cannot be of type " + args.NewValue.GetType().Name + ", it must be of type TabControl");
        }

        TabControl tabControl = (TabControl)args.NewValue;
        ContentManager contentManager = GetContentManager(tabControl, container);
        contentManager.UpdateSelectedTab();
    }

    private static void OnIsCachedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is null)
        {
            return;
        }

        TabControl tabControl = obj as TabControl ?? throw new InvalidOperationException("Cannot set TabContent.IsCached on object of type " + args.NewValue.GetType().Name +
                   ". Only objects of type TabControl can have TabContent.IsCached property.");

        bool newValue = (bool)args.NewValue;

        if (!newValue)
        {
            if (args.OldValue is not null && (bool)args.OldValue)
            {
                throw new NotImplementedException("Cannot change TabContent.IsCached from True to False. Turning tab caching off is not implemented");
            }

            return;
        }

        EnsureContentTemplateIsNull(tabControl);
        tabControl.ContentTemplate = CreateContentTemplate();
        EnsureContentTemplateIsNotModified(tabControl);
    }

    public class ContentManager
    {
        private readonly TabControl _tabControl;
        private Decorator _border;

        public ContentManager(TabControl tabControl, Decorator border)
        {
            _tabControl = tabControl;
            _border = border;
            _tabControl.SelectionChanged += (sender, args) => UpdateSelectedTab();
        }

        public void ReplaceContainer(Decorator newBorder)
        {
            if (ReferenceEquals(_border, newBorder))
            {
                return;
            }

            _border.Child = null; // detach any tab content that old border may hold
            _border = newBorder;
        }

        public void UpdateSelectedTab() => _border.Child = GetCurrentContent();

        private ContentControl? GetCurrentContent()
        {
            object item = _tabControl.SelectedItem;
            if (item is null)
            {
                return null;
            }

            DependencyObject tabItem = _tabControl.ItemContainerGenerator.ContainerFromItem(item);
            if (tabItem is null)
            {
                return null;
            }

            ContentControl? cachedContent = GetInternalCachedContent(tabItem);
            if (cachedContent is null)
            {
                cachedContent = new ContentControl
                {
                    DataContext = item,
                    ContentTemplate = GetTemplate(_tabControl),
                    ContentTemplateSelector = GetTemplateSelector(_tabControl)
                };

                _ = cachedContent.SetBinding(ContentControl.ContentProperty, new Binding());
                SetInternalCachedContent(tabItem, cachedContent);
            }

            return cachedContent;
        }
    }
}