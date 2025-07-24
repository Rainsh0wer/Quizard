using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class StudentResultsView : UserControl
    {
        public StudentResultsView()
        {
            InitializeComponent();
            DataContext = new StudentResultsViewModel();
        }
    }
}