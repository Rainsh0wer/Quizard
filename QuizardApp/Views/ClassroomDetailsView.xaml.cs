using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class ClassroomDetailsView : UserControl
    {
        public ClassroomDetailsView(int classId)
        {
            InitializeComponent();
            DataContext = new ClassroomManagementViewModel();
        }
    }
}