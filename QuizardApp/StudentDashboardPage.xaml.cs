using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp
{
    public partial class StudentDashboardPage : Page
    {
        public StudentDashboardPage()
        {
            InitializeComponent();
            DataContext = new StudentDashboardViewModel();
        }
    }
}