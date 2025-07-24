using System;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        // Fields
        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _message;
        
        public ICommand GoToLoginCommand { get; }

        // Properties
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        // Command
        public ICommand RegisterCommand { get; }

        // Constructor
        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            GoToLoginCommand = new RelayCommand(_ => GoToLogin());
        }

        private void GoToLogin()
        {
            AppNavigationService.Instance.Navigate(new LoginPage());
        }

        // Register Logic
        private void Register(object parameter)
        {
            if (Password != ConfirmPassword)
            {
                Message = "Passwords do not match.";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    // Check if username or email already exists
                    var existingUser = context.Users
                        .FirstOrDefault(u => u.Username == Username || u.Email == Email);
                    if (existingUser != null)
                    {
                        Message = "Username or email already exists.";
                        return;
                    }

                    // Hash password (simple hash, replace with stronger one if needed)
                    string passwordHash = Password;

                    var newUser = new User
                    {
                        Username = Username,
                        Email = Email,
                        PasswordHash = passwordHash,
                        Role = "Student", // or "Teacher"
                        FullName = "",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    Message = "Registration successful! Redirecting to login...";
                    
                    // Navigate to login page after successful registration
                    System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ => 
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => 
                        {
                            GoToLogin();
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Message = "Registration failed: " + ex.Message;
            }
        }
    }
}
