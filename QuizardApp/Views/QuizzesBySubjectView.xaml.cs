using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class QuizzesBySubjectView : UserControl
    {
        public QuizzesBySubjectView(int subjectId)
        {
            InitializeComponent();
            DataContext = new AvailableQuizzesViewModel();
        }
    }
}