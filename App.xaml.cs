using System.Windows;

namespace BG3LocalizationMerger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e
        )
        {
            string errorMessage = string.Format(
                "An unhandled exception occurred:\n\n{0}\n\nStackTrace:\n{1}",
                e.Exception.Message,
                e.Exception.StackTrace
            );
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
