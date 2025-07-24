using System.Windows.Controls;
using QuizardApp.ViewModels;
using System.Windows.Input;

namespace QuizardApp.Views
{
    public partial class TeacherDashboardView : UserControl
    {
        public TeacherDashboardView(ICommand? showQuizzesCommand = null, ICommand? showSubjectsCommand = null, ICommand? showResultsCommand = null, ICommand? showClassroomsCommand = null)
        {
            InitializeComponent();
            DataContext = new TeacherDashboardViewViewModel(showQuizzesCommand, showSubjectsCommand, showResultsCommand, showClassroomsCommand);
        }
    }
}