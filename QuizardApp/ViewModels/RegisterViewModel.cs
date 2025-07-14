using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuizardApp.Models;

namespace QuizardApp.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        // Fields
        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _message;
        public event Action RequestNavigateToLogin;
        public ICommand GoToLoginCommand { get; }


        // Properties
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        // Command
        public ICommand RegisterCommand { get; }

        // Constructor
        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            GoToLoginCommand = new RelayCommand(_ => RequestNavigateToLogin?.Invoke());
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

                    Message = "Registration successful!";
                }
            }
            catch (Exception ex)
            {
                Message = "Registration failed: " + ex.Message;
            }
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
