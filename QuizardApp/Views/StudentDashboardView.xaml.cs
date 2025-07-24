using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class StudentDashboardView : UserControl
    {
        public StudentDashboardView()
        {
            InitializeComponent();
            // DataContext = new StudentDashboardViewViewModel(); // Xóa dòng này để nhận DataContext từ cha
        }
    }
}