using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace QuizardApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel()
        {
            // Mặc định vào LoginView
            CurrentView = new QuizardApp.Views.LoginView();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}