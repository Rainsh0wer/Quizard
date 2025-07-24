using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class StudentSubjectsView : UserControl
    {
        public StudentSubjectsView()
        {
            InitializeComponent();
            DataContext = new StudentSubjectsViewModel();
        }
    }
}