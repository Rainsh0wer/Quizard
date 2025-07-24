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
        private NavigationService? _navigationService;
        private CurrentUserService? _currentUserService;
        private MainCursorViewModel? _mainCursorViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            _currentUserService = new CurrentUserService();
            
            // Create main window
            var mainWindow = new MainWindow();
            
            // Initialize navigation service with window's content control
            _navigationService = new NavigationService(view => 
            {
                mainWindow.MainContent.Content = view;
            });

            // Initialize main cursor view model
            _mainCursorViewModel = new MainCursorViewModel(_navigationService, _currentUserService);

            // Set window data context
            mainWindow.DataContext = _mainCursorViewModel;

            // Show main window
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
