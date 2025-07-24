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
                Message = "Connecting to database...";
                using (var context = new QuizardContext())
                {
                    Message = "Searching for user...";
                    var user = context.Users
                                      .FirstOrDefault(u => u.Username == Username && u.PasswordHash == Password && u.IsActive == true);

                    if (user != null)
                    {
                        CurrentUserService.Instance.SetCurrentUser(user);
                        Message = $"Login successful! Welcome {user.FullName}";
                        
                        if (user.Role == "teacher")
                        {
                            try
                            {
                                var teacherPage = new TeacherDashboardPage();
                                AppNavigationService.Instance.Navigate(teacherPage);
                            }
                            catch (Exception navEx)
                            {
                                Message = $"Navigation error: {navEx.Message}";
                                System.Diagnostics.Debug.WriteLine($"Teacher dashboard navigation failed: {navEx}");
                            }
                        }
                        else if (user.Role == "student")
                        {
                            try
                            {
                                var studentPage = new StudentDashboardPage();
                                AppNavigationService.Instance.Navigate(studentPage);
                            }
                            catch (Exception navEx)
                            {
                                Message = $"Navigation error: {navEx.Message}";
                                System.Diagnostics.Debug.WriteLine($"Student dashboard navigation failed: {navEx}");
                            }
                        }
                        else
                        {
                            Message = $"Unknown role: '{user.Role}' (Length: {user.Role?.Length})";
                        }
                    }
                    else
                    {
                        Message = "Invalid username or password, or account is inactive.";
                        
                        // Debug: Check if user exists with different credentials
                        var userExists = context.Users.FirstOrDefault(u => u.Username == Username);
                        if (userExists != null)
                        {
                            Message += $" User exists but password/status mismatch. IsActive: {userExists.IsActive}";
                        }
                        else
                        {
                            Message += " User not found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Login error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Full exception: {ex}");
            }
        }

        private void ExecuteSignUp(object obj)
        {
            AppNavigationService.Instance.Navigate(new RegisterPage());
        }
    }
}
