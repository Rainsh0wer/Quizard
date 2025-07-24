using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class RemoveStudentView : UserControl
    {
        public RemoveStudentView(int classId)
        {
            InitializeComponent();
            DataContext = new ClassroomManagementViewModel();
        }
    }
}