using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class QuizDetailsView : UserControl
    {
        public QuizDetailsView(int quizId)
        {
            InitializeComponent();
            DataContext = new QuizManagementViewModel();
        }
    }
}