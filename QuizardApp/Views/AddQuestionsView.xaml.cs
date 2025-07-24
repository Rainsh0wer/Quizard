using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class AddQuestionsView : UserControl
    {
        public AddQuestionsView(int quizId)
        {
            InitializeComponent();
            DataContext = new QuizManagementViewModel();
        }
    }
}