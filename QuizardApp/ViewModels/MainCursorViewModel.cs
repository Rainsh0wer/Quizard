using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public enum AppState
    {
        Login,
        Register,
        StudentDashboard,
        TeacherDashboard,
        TakeQuiz,
        CreateQuiz,
        ViewResults,
        SearchSubjects,
        JoinClass,
        ViewClasses,
        QuizDetails,
        Loading
    }

    public class MainCursorViewModel : INotifyPropertyChanged
    {
        private readonly CurrentUserService _currentUserService;
        private AppState _currentState;
        private BaseViewModel? _currentViewModel;
        private User? _currentUser;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public MainCursorViewModel(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            _currentState = AppState.Login;
            
            // Initialize commands
            NavigateCommand = new RelayCommand<AppState>(NavigateTo);
            LogoutCommand = new RelayCommand(Logout);
            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            
            // Initialize with login view
            NavigateTo(AppState.Login);
        }

        public AppState CurrentState
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                OnPropertyChanged();
                OnStateChanged();
            }
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsStudent));
                OnPropertyChanged(nameof(IsTeacher));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsStudent => CurrentUser?.Role == "Student";
        public bool IsTeacher => CurrentUser?.Role == "Teacher";

        // Commands
        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoBackCommand { get; }

        // Navigation stack for back functionality
        private readonly Stack<AppState> _navigationStack = new Stack<AppState>();

        public void NavigateTo(AppState newState)
        {
            if (CurrentState != newState)
            {
                _navigationStack.Push(CurrentState);
                CurrentState = newState;
            }
        }

        private void OnStateChanged()
        {
            IsLoading = true;
            StatusMessage = $"Loading {CurrentState}...";

            try
            {
                CurrentViewModel = CreateViewModelForState(CurrentState);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }

            // Update command states
            ((RelayCommand)GoBackCommand).RaiseCanExecuteChanged();
        }

        private BaseViewModel CreateViewModelForState(AppState state)
        {
            return state switch
            {
                AppState.Login => new LoginViewModel(this),
                AppState.Register => new RegisterViewModel(this),
                AppState.StudentDashboard => new StudentDashboardViewModel(this, _currentUserService),
                AppState.TeacherDashboard => new TeacherDashboardViewModel(this, _currentUserService),
                AppState.TakeQuiz => new TakeQuizViewModel(this, _currentUserService),
                AppState.CreateQuiz => new CreateQuizViewModel(this, _currentUserService),
                AppState.ViewResults => new ViewResultsViewModel(this, _currentUserService),
                AppState.SearchSubjects => new SearchSubjectsViewModel(this, _currentUserService),
                AppState.JoinClass => new JoinClassViewModel(this, _currentUserService),
                AppState.ViewClasses => new ViewClassesViewModel(this, _currentUserService),
                AppState.QuizDetails => new QuizDetailsViewModel(this, _currentUserService),
                _ => throw new ArgumentException($"Unknown state: {state}")
            };
        }

        public void OnUserLoggedIn(User user)
        {
            CurrentUser = user;
            _currentUserService.SetCurrentUser(user);
            
            // Navigate to appropriate dashboard based on role
            var targetState = user.Role switch
            {
                "Student" => AppState.StudentDashboard,
                "Teacher" => AppState.TeacherDashboard,
                _ => AppState.Login
            };
            
            NavigateTo(targetState);
        }

        private void Logout()
        {
            CurrentUser = null;
            _currentUserService.SetCurrentUser(null);
            _navigationStack.Clear();
            NavigateTo(AppState.Login);
        }

        private void GoBack()
        {
            if (CanGoBack())
            {
                var previousState = _navigationStack.Pop();
                CurrentState = previousState;
            }
        }

        private bool CanGoBack()
        {
            return _navigationStack.Count > 0 && CurrentState != AppState.Login;
        }

        // State machine logic for valid transitions
        public bool CanNavigateTo(AppState targetState)
        {
            return targetState switch
            {
                AppState.Login => true,
                AppState.Register => CurrentState == AppState.Login,
                AppState.StudentDashboard => IsStudent,
                AppState.TeacherDashboard => IsTeacher,
                AppState.TakeQuiz => IsStudent,
                AppState.CreateQuiz => IsTeacher,
                AppState.ViewResults => IsLoggedIn,
                AppState.SearchSubjects => IsLoggedIn,
                AppState.JoinClass => IsStudent,
                AppState.ViewClasses => IsTeacher,
                AppState.QuizDetails => IsLoggedIn,
                _ => false
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}