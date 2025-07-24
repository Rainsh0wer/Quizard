using System.Windows.Controls;

namespace QuizardApp.Views
{
    public partial class StudentPerformanceView : UserControl
    {
        public StudentPerformanceView(string studentUsername)
        {
            InitializeComponent();
            DataContext = new ResultsViewModel();
        }
    }
}