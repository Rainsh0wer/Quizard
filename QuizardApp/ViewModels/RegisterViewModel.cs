using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;

namespace QuizardApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _fullName = string.Empty;
        private string _selectedRole = "Student";
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public RegisterViewModel(MainCursorViewModel mainCursor)
        {
            _mainCursor = mainCursor;
            
            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanRegister);
            BackToLoginCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.Login));
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsernameValidation));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EmailValidation));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswordValidation));
                OnPropertyChanged(nameof(ConfirmPasswordValidation));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConfirmPasswordValidation));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        // Validation properties
        public string UsernameValidation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Username))
                    return "Username is required";
                if (Username.Length < 3)
                    return "Username must be at least 3 characters";
                if (Username.Length > 50)
                    return "Username cannot exceed 50 characters";
                if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
                    return "Username can only contain letters, numbers, and underscores";
                return string.Empty;
            }
        }

        public string EmailValidation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Email))
                    return "Email is required";
                if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return "Please enter a valid email address";
                return string.Empty;
            }
        }

        public string PasswordValidation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Password))
                    return "Password is required";
                if (Password.Length < 6)
                    return "Password must be at least 6 characters";
                if (Password.Length > 100)
                    return "Password cannot exceed 100 characters";
                return string.Empty;
            }
        }

        public string ConfirmPasswordValidation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                    return "Please confirm your password";
                if (Password != ConfirmPassword)
                    return "Passwords do not match";
                return string.Empty;
            }
        }

        public bool IsUsernameValid => string.IsNullOrEmpty(UsernameValidation);
        public bool IsEmailValid => string.IsNullOrEmpty(EmailValidation);
        public bool IsPasswordValid => string.IsNullOrEmpty(PasswordValidation);
        public bool IsConfirmPasswordValid => string.IsNullOrEmpty(ConfirmPasswordValidation);

        public string[] AvailableRoles => new[] { "Student", "Teacher" };

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private bool CanRegister()
        {
            return !IsLoading &&
                   IsUsernameValid &&
                   IsEmailValid &&
                   IsPasswordValid &&
                   IsConfirmPasswordValid &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(SelectedRole);
        }

        private async Task RegisterAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Check if username already exists
                    if (context.Users.Any(u => u.Username == Username))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Username already exists. Please choose a different username.";
                        });
                        return;
                    }

                    // Check if email already exists
                    if (context.Users.Any(u => u.Email == Email))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Email already exists. Please use a different email address.";
                        });
                        return;
                    }

                    // Hash password
                    var hashedPassword = HashPassword(Password);

                    // Create new user
                    var newUser = new User
                    {
                        Username = Username,
                        Email = Email,
                        PasswordHash = hashedPassword,
                        FullName = FullName,
                        Role = SelectedRole,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _mainCursor.StatusMessage = "Registration successful! You can now log in.";
                        _mainCursor.NavigateTo(AppState.Login);
                    });
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Helper methods for demo/testing
        public void FillDemoStudent()
        {
            Username = "demo_student";
            Email = "student@demo.com";
            Password = "demo123";
            ConfirmPassword = "demo123";
            FullName = "Demo Student";
            SelectedRole = "Student";
        }

        public void FillDemoTeacher()
        {
            Username = "demo_teacher";
            Email = "teacher@demo.com";
            Password = "demo123";
            ConfirmPassword = "demo123";
            FullName = "Demo Teacher";
            SelectedRole = "Teacher";
        }

        public void ClearForm()
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            SelectedRole = "Student";
            ErrorMessage = string.Empty;
        }
    }
}