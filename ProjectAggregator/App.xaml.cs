using System.Windows;
using ProjectAggregator.ViewModel;

namespace ProjectAggregator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logger = new AggregatorLogger();
            var mainViewModel = new MainViewModel(logger);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
    }
}
