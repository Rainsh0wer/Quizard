using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;

namespace QuizardApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public LoginViewModel(MainCursorViewModel mainCursor)
        {
            _mainCursor = mainCursor;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            RegisterCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.Register));
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private bool CanLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    var hashedPassword = HashPassword(Password);
                    
                    var user = context.Users.FirstOrDefault(u => 
                        u.Username == Username && u.PasswordHash == hashedPassword && u.IsActive == true);

                    if (user != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _mainCursor.OnUserLoggedIn(user);
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Invalid username or password.";
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
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

        // Demo users for testing
        public void LoginAsStudent()
        {
            Username = "student1";
            Password = "password123";
        }

        public void LoginAsTeacher()
        {
            Username = "teacher1";
            Password = "password123";
        }
    }
}