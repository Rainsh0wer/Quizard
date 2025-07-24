using System.Windows;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}