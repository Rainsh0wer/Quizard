using System.Windows;
using System.Windows.Controls;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public partial class ViewTemplates : ResourceDictionary
    {
        public ViewTemplates()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && passwordBox.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.Password = passwordBox.Password;
            }
        }

        private void DemoStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginAsStudent();
            }
        }

        private void DemoTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginAsTeacher();
            }
        }
    }
}