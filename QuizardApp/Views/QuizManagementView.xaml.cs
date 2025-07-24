using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class QuizManagementView : UserControl
    {
        public QuizManagementView()
        {
            InitializeComponent();
            DataContext = new QuizManagementViewModel();
        }
    }
}