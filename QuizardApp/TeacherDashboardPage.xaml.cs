using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp
{
    public partial class TeacherDashboardPage : Page
    {
        public TeacherDashboardPage()
        {
            InitializeComponent();
            DataContext = new TeacherDashboardViewModel();
        }
    }
}