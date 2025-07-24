using System.Windows.Input;
using QuizardApp.Services;
using QuizardApp.Views;
using System.Windows;

namespace QuizardApp.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
        private string _currentPageTitle = string.Empty;
        private string _currentPageSubtitle = string.Empty;
        private object _currentView;

        public string CurrentUserName => CurrentUserService.Instance.CurrentUser?.FullName ?? "Student";

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set => SetProperty(ref _currentPageTitle, value);
        }

        public string CurrentPageSubtitle
        {
            get => _currentPageSubtitle;
            set => SetProperty(ref _currentPageSubtitle, value);
        }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowAvailableQuizzesCommand { get; }
        public ICommand ShowMyResultsCommand { get; }
        public ICommand ShowSavedQuizzesCommand { get; }
        public ICommand ShowSubjectsCommand { get; }
        public ICommand LogoutCommand { get; }

        public StudentDashboardViewModel()
        {
            ShowDashboardCommand = new RelayCommand(_ => ShowDashboard());
            ShowAvailableQuizzesCommand = new RelayCommand(_ => ShowAvailableQuizzes());
            ShowMyResultsCommand = new RelayCommand(_ => ShowMyResults());
            ShowSavedQuizzesCommand = new RelayCommand(_ => ShowSavedQuizzes());
            ShowSubjectsCommand = new RelayCommand(_ => ShowSubjects());
            LogoutCommand = new RelayCommand(_ => Logout());

            // Show dashboard by default
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            CurrentPageTitle = "Dashboard";
            CurrentPageSubtitle = "Your learning overview";
            CurrentView = new StudentDashboardView(
                ShowAvailableQuizzesCommand,
                ShowSubjectsCommand,
                ShowMyResultsCommand
            );
        }

        private void ShowAvailableQuizzes()
        {
            CurrentPageTitle = "Available Quizzes";
            CurrentPageSubtitle = "Quizzes you can take";
            CurrentView = new AvailableQuizzesView();
        }

        private void ShowMyResults()
        {
            CurrentPageTitle = "My Results";
            CurrentPageSubtitle = "Your quiz performance and history";
            CurrentView = new StudentResultsView();
        }

        private void ShowSavedQuizzes()
        {
            CurrentPageTitle = "Saved Quizzes";
            CurrentPageSubtitle = "Your favorite quizzes";
            CurrentView = new SavedQuizzesView();
        }

        private void ShowSubjects()
        {
            CurrentPageTitle = "Subjects";
            CurrentPageSubtitle = "Browse quizzes by subject";
            CurrentView = new StudentSubjectsView();
        }

        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                CurrentUserService.Instance.ClearCurrentUser();
                AppNavigationService.Instance.Navigate(new LoginPage());
            }
        }
    }
}