using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
        }
    }
}