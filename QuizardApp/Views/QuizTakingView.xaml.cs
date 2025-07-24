using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class QuizTakingView : UserControl
    {
        public QuizTakingView(int studentQuizId)
        {
            InitializeComponent();
            DataContext = new QuizTakingViewModel(studentQuizId);
        }
    }
}