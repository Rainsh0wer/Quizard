using System.Windows.Input;
using QuizardApp.Services;
using QuizardApp.Views;
using System.Windows.Controls;
using System.Windows;

namespace QuizardApp.ViewModels
{
    public class TeacherDashboardViewModel : BaseViewModel
    {
        private string _currentPageTitle;
        private string _currentPageSubtitle;
        private object _currentView;

        public string CurrentUserName => CurrentUserService.Instance.CurrentUser?.FullName ?? "Teacher";

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
        public ICommand ShowSubjectsCommand { get; }
        public ICommand ShowQuizzesCommand { get; }
        public ICommand ShowClassroomsCommand { get; }
        public ICommand ShowResultsCommand { get; }
        public ICommand LogoutCommand { get; }

        public TeacherDashboardViewModel()
        {
            ShowDashboardCommand = new RelayCommand(_ => ShowDashboard());
            ShowSubjectsCommand = new RelayCommand(_ => ShowSubjects());
            ShowQuizzesCommand = new RelayCommand(_ => ShowQuizzes());
            ShowClassroomsCommand = new RelayCommand(_ => ShowClassrooms());
            ShowResultsCommand = new RelayCommand(_ => ShowResults());
            LogoutCommand = new RelayCommand(_ => Logout());

            // Show dashboard by default
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            CurrentPageTitle = "Dashboard";
            CurrentPageSubtitle = "Overview of your teaching activities";
            CurrentView = new TeacherDashboardView();
        }

        private void ShowSubjects()
        {
            CurrentPageTitle = "Subjects";
            CurrentPageSubtitle = "Manage subjects and topics";
            CurrentView = new SubjectManagementView();
        }

        private void ShowQuizzes()
        {
            CurrentPageTitle = "My Quizzes";
            CurrentPageSubtitle = "Create and manage your quizzes";
            CurrentView = new QuizManagementView();
        }

        private void ShowClassrooms()
        {
            CurrentPageTitle = "Classrooms";
            CurrentPageSubtitle = "Manage your classrooms and students";
            CurrentView = new ClassroomManagementView();
        }

        private void ShowResults()
        {
            CurrentPageTitle = "Results";
            CurrentPageSubtitle = "View student quiz results and analytics";
            CurrentView = new ResultsView();
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