using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class EditSubjectView : UserControl
    {
        public EditSubjectView(int subjectId)
        {
            InitializeComponent();
            DataContext = new SubjectManagementViewModel();
        }
    }
}