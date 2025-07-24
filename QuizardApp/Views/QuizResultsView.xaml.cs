using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class QuizResultsView : UserControl
    {
        public QuizResultsView(int quizId)
        {
            InitializeComponent();
            DataContext = new ResultsViewModel();
        }
    }
}