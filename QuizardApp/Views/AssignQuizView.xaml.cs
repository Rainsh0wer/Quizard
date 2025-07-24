using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class AssignQuizView : UserControl
    {
        public AssignQuizView(int classId)
        {
            InitializeComponent();
            DataContext = new QuizManagementViewModel();
        }
    }
}