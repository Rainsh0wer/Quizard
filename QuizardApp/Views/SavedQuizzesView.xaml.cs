using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class SavedQuizzesView : UserControl
    {
        public SavedQuizzesView()
        {
            InitializeComponent();
            DataContext = new SavedQuizzesViewModel();
        }
    }
}