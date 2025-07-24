using System.Windows.Controls;
using QuizardApp.ViewModels;
using System.Windows.Input; // Added for ICommand

namespace QuizardApp.Views
{
    public partial class StudentDashboardView : UserControl
    {
        public StudentDashboardView(ICommand? showAvailableQuizzesCommand = null, ICommand? showSubjectsCommand = null, ICommand? showMyResultsCommand = null)
        {
            InitializeComponent();
            DataContext = new StudentDashboardViewViewModel(showAvailableQuizzesCommand, showSubjectsCommand, showMyResultsCommand);
        }
    }
}