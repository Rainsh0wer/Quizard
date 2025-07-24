using System.Windows;
using QuizardApp.Services;
using QuizardApp.ViewModels;
using QuizardApp.Views;

namespace QuizardApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CurrentUserService? _currentUserService;
        private MainCursorViewModel? _mainCursorViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            _currentUserService = new CurrentUserService();
            
            // Create main window
            var mainWindow = new MainWindow();

            // Initialize main cursor view model
            _mainCursorViewModel = new MainCursorViewModel(_currentUserService);

            // Set window data context
            mainWindow.DataContext = _mainCursorViewModel;

            // Show main window
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
