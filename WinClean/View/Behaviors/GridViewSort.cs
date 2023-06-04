// https://thomaslevesque.com/2009/08/04/wpf-automatically-sort-a-gridview-continued/

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Scover.WinClean.View.Behaviors;

public static class GridViewSort
{
    public static readonly DependencyProperty AutoSortProperty =
           DependencyProperty.RegisterAttached(
               "AutoSort",
               typeof(bool),
               typeof(GridViewSort),
               new UIPropertyMetadata(
                   false,
                   (o, e) =>
                    {
                        ListView? listView = o as ListView;
                        if (listView is not null)
                        {
                            if (GetCommand(listView) is null) // Don't change click handler if a command is set
                            {
                                bool oldValue = (bool)e.OldValue;
                                bool newValue = (bool)e.NewValue;
                                if (oldValue && !newValue)
                                {
                                    listView.RemoveHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (!oldValue && newValue)
                                {
                                    listView.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }));

    public static readonly DependencyProperty CommandProperty =
           DependencyProperty.RegisterAttached(
               "Command",
               typeof(ICommand),
               typeof(GridViewSort),
               new UIPropertyMetadata(
                   null,
                   (o, e) =>
                    {
                        ItemsControl? listView = o as ItemsControl;
                        if (listView is not null)
                        {
                            if (!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
                            {
                                if (e.OldValue is not null && e.NewValue is null)
                                {
                                    listView.RemoveHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (e.OldValue is null && e.NewValue is not null)
                                {
                                    listView.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }));

    public static readonly DependencyProperty PropertyNameProperty =
           DependencyProperty.RegisterAttached(
               "PropertyName",
               typeof(string),
               typeof(GridViewSort),
               new UIPropertyMetadata(null));

    public static readonly DependencyProperty ShowSortGlyphProperty =
           DependencyProperty.RegisterAttached("ShowSortGlyph", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(true));

    public static readonly DependencyProperty SortGlyphAscendingProperty =
           DependencyProperty.RegisterAttached("SortGlyphAscending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

    public static readonly DependencyProperty SortGlyphDescendingProperty =
           DependencyProperty.RegisterAttached("SortGlyphDescending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

    private static readonly DependencyProperty SortedColumnHeaderProperty =
            DependencyProperty.RegisterAttached("SortedColumnHeader", typeof(GridViewColumnHeader), typeof(GridViewSort), new UIPropertyMetadata(null));

    public static bool GetAutoSort(DependencyObject obj) => (bool)obj.GetValue(AutoSortProperty);

    public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);

    public static string GetPropertyName(DependencyObject obj) => (string)obj.GetValue(PropertyNameProperty);

    public static bool GetShowSortGlyph(DependencyObject obj) => (bool)obj.GetValue(ShowSortGlyphProperty);

    public static ImageSource GetSortGlyphAscending(DependencyObject obj) => (ImageSource)obj.GetValue(SortGlyphAscendingProperty);

    public static ImageSource GetSortGlyphDescending(DependencyObject obj) => (ImageSource)obj.GetValue(SortGlyphDescendingProperty);

    public static void SetAutoSort(DependencyObject obj, bool value) => obj.SetValue(AutoSortProperty, value);

    public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

    public static void SetPropertyName(DependencyObject obj, string value) => obj.SetValue(PropertyNameProperty, value);

    public static void SetShowSortGlyph(DependencyObject obj, bool value) => obj.SetValue(ShowSortGlyphProperty, value);

    public static void SetSortGlyphAscending(DependencyObject obj, ImageSource value) => obj.SetValue(SortGlyphAscendingProperty, value);

    public static void SetSortGlyphDescending(DependencyObject obj, ImageSource value) => obj.SetValue(SortGlyphDescendingProperty, value);

    public static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader)
    {
        ListSortDirection direction = ListSortDirection.Ascending;
        if (view.SortDescriptions.Count > 0)
        {
            SortDescription currentSort = view.SortDescriptions[0];
            if (currentSort.PropertyName == propertyName)
            {
                direction = currentSort.Direction is ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            view.SortDescriptions.Clear();

            GridViewColumnHeader currentSortedColumnHeader = GetSortedColumnHeader(listView);
            if (currentSortedColumnHeader is not null)
            {
                RemoveSortGlyph(currentSortedColumnHeader);
            }
        }
        if (!string.IsNullOrEmpty(propertyName))
        {
            view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            if (GetShowSortGlyph(listView))
            {
                AddSortGlyph(
                    sortedColumnHeader,
                    direction,
                    direction is ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));
            }

            SetSortedColumnHeader(listView, sortedColumnHeader);
        }
    }

    public static T? GetAncestor<T>(DependencyObject reference) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(reference);
        while (parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent is not null ? (T)parent : null;
    }

    private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj) => (GridViewColumnHeader)obj.GetValue(SortedColumnHeaderProperty);

    private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value) => obj.SetValue(SortedColumnHeaderProperty, value);

    private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        GridViewColumnHeader? headerClicked = e.OriginalSource as GridViewColumnHeader;
        if (headerClicked is not null && headerClicked.Column is not null)
        {
            string propertyName = GetPropertyName(headerClicked.Column);
            if (!string.IsNullOrEmpty(propertyName))
            {
                ListView? listView = GetAncestor<ListView>(headerClicked);
                if (listView is not null)
                {
                    ICommand command = GetCommand(listView);
                    if (command is not null)
                    {
                        if (command.CanExecute(propertyName))
                        {
                            command.Execute(propertyName);
                        }
                    }
                    else if (GetAutoSort(listView))
                    {
                        ApplySort(listView.Items, propertyName, listView, headerClicked);
                    }
                }
            }
        }
    }

    private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
        adornerLayer.Add(
            new SortGlyphAdorner(
                columnHeader,
                direction,
                sortGlyph));
    }

    private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
        Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
        if (adorners is not null)
        {
            foreach (Adorner adorner in adorners)
            {
                if (adorner is SortGlyphAdorner)
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }
    }

    private sealed class SortGlyphAdorner : Adorner
    {
        private readonly GridViewColumnHeader _columnHeader;
        private readonly ListSortDirection _direction;
        private readonly ImageSource _sortGlyph;

        public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
            : base(columnHeader)
        {
            _columnHeader = columnHeader;
            _direction = direction;
            _sortGlyph = sortGlyph;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_sortGlyph is not null)
            {
                double x = _columnHeader.ActualWidth - 13;
                double y = _columnHeader.ActualHeight / 2 - 5;
                Rect rect = new(x, y, 10, 10);
                drawingContext.DrawImage(_sortGlyph, rect);
            }
            else
            {
                drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
            }
        }

        private Geometry GetDefaultGlyph()
        {
            double x1 = _columnHeader.ActualWidth - 13;
            double x2 = x1 + 10;
            double x3 = x1 + 5;
            double y1 = _columnHeader.ActualHeight / 2 - 3;
            double y2 = y1 + 5;

            if (_direction == ListSortDirection.Ascending)
            {
                (y2, y1) = (y1, y2);
            }

            PathSegmentCollection pathSegmentCollection = new()
            {
                new LineSegment(new Point(x2, y1), true),
                new LineSegment(new Point(x3, y2), true)
            };

            PathFigure pathFigure = new(
                       new Point(x1, y1),
                       pathSegmentCollection,
                       true);

            PathFigureCollection pathFigureCollection = new()
            {
                pathFigure
            };

            PathGeometry pathGeometry = new(pathFigureCollection);
            return pathGeometry;
        }
    }
}