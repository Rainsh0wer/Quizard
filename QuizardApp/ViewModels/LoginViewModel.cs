using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuizardApp.Models;
using System.Windows;
using System.Windows.Navigation;

namespace QuizardApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string username;
        private string password;
        private string message;

        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => message;
            set { message = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            SignUpCommand = new RelayCommand(ExecuteSignUp);
        }

        private void ExecuteLogin(object obj)
        {
            using (var context = new QuizardContext())
            {
                var user = context.Users
                                  .FirstOrDefault(u => u.Username == Username && u.PasswordHash == Password);

                if (user != null)
                {
                    if (user.Role == "teacher")
                        Message = $"Welcome Teacher, {user.FullName}!";
                    else if (user.Role == "student")
                        Message = $"Welcome Student, {user.FullName}!";
                    else
                        Message = $"Welcome {user.FullName}!";
                }
                else
                {
                    Message = "Invalid username or password.";
                }
            }
        }

        private void ExecuteSignUp(object obj)
        {
            // chuyển hướng sang RegisterPage nếu xài NavigationService
            var registerPage = new RegisterPage();

            // nếu LoginPage đang nằm trong một Frame, phải lấy NavigationService từ Frame
            var navService = NavigationService.GetNavigationService(Application.Current.MainWindow.Content as DependencyObject);
            navService?.Navigate(registerPage);

            // nếu không được thì dùng Frame trong MainWindow
            // ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(registerPage);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
