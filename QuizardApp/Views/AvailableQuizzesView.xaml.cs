using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class AvailableQuizzesView : UserControl
    {
        public AvailableQuizzesView()
        {
            InitializeComponent();
            DataContext = new AvailableQuizzesViewModel();
        }
    }
}