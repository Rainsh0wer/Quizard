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
            NavigationService.Instance.Initialize(this);
        }
    }
}