using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class QuizResultDetailView : UserControl
    {
        public QuizResultDetailView(int studentQuizId)
        {
            InitializeComponent();
            DataContext = new ResultsViewModel();
        }
    }
}