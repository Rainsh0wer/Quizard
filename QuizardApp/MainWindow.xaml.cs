using System.Windows.Navigation;
using QuizardApp.Services;

namespace QuizardApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            AppNavigationService.Instance.Initialize(this);
            
            // Navigate to LoginPage as initial page
            this.Loaded += (s, e) => 
            {
                AppNavigationService.Instance.Navigate(new LoginPage());
            };
        }
    }
}