using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace QuizardApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
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

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        private void Login(object obj)
        {
            // TODO: Xử lý đăng nhập
        }

        private void Register(object obj)
        {
            // TODO: Chuyển sang màn hình đăng ký
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}