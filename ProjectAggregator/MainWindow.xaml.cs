using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectAggregator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ListBox _listBox;
        private ScrollViewer _scrollViewer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            _listBox = (ListBox)sender;
            var border = (Border)VisualTreeHelper.GetChild(_listBox, 0);
            _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            var collection = (INotifyCollectionChanged)_listBox.ItemsSource;
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _scrollViewer.ScrollToBottom();
            }
        }
    }
}
