using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class TeacherDashboardView : UserControl
    {
        public TeacherDashboardView()
        {
            InitializeComponent();
            // DataContext = new TeacherDashboardViewViewModel(); // Xóa dòng này để nhận DataContext từ cha
        }
    }
}