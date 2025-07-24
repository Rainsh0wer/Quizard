using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class EditQuizView : UserControl
    {
        public EditQuizView(int quizId)
        {
            InitializeComponent();
            DataContext = new QuizManagementViewModel();
        }
    }
}