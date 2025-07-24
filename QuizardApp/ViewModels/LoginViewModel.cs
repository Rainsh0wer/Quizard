using System;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using System.Windows;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string username;
        private string password;
        private string message;

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
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
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Please enter both username and password.";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var user = context.Users
                                      .FirstOrDefault(u => u.Username == Username && u.PasswordHash == Password && u.IsActive == true);

                    if (user != null)
                    {
                        CurrentUserService.Instance.SetCurrentUser(user);
                        
                        if (user.Role == "teacher")
                        {
                            NavigationService.Instance.Navigate(new TeacherDashboardPage());
                        }
                        else if (user.Role == "student")
                        {
                            NavigationService.Instance.Navigate(new StudentDashboardPage());
                        }
                    }
                    else
                    {
                        Message = "Invalid username or password, or account is inactive.";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Login error: {ex.Message}";
            }
        }

        private void ExecuteSignUp(object obj)
        {
            NavigationService.Instance.Navigate(new RegisterPage());
        }
    }
}
