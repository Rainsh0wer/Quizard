using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class SubjectManagementView : UserControl
    {
        public SubjectManagementView()
        {
            InitializeComponent();
            DataContext = new SubjectManagementViewModel();
        }
    }
}