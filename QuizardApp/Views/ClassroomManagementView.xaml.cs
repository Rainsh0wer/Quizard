using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class ClassroomManagementView : UserControl
    {
        public ClassroomManagementView()
        {
            InitializeComponent();
            DataContext = new ClassroomManagementViewModel();
        }
    }
}