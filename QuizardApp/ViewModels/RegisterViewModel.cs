using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace QuizardApp.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            BackCommand = new RelayCommand(Back);
        }

        private void Register(object obj)
        {
            // TODO: Xử lý đăng ký
        }

        private void Back(object obj)
        {
            // TODO: Quay lại màn hình đăng nhập
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}