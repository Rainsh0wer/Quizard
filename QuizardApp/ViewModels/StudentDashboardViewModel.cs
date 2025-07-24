using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private bool _isLoading = false;
        private string _searchSubjectText = string.Empty;

        public StudentDashboardViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            AssignedQuizzes = new ObservableCollection<Quiz>();
            AvailableSubjects = new ObservableCollection<Subject>();
            MyClasses = new ObservableCollection<Classroom>();
            RecentResults = new ObservableCollection<StudentQuiz>();
            
            // Initialize commands
            TakeQuizCommand = new RelayCommand<Quiz>(TakeQuiz);
            SearchSubjectsCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.SearchSubjects));
            JoinClassCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.JoinClass));
            ViewResultsCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.ViewResults));
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            
            // Load initial data
            _ = LoadDataAsync();
        }

        public ObservableCollection<Quiz> AssignedQuizzes { get; }
        public ObservableCollection<Subject> AvailableSubjects { get; }
        public ObservableCollection<Classroom> MyClasses { get; }
        public ObservableCollection<StudentQuiz> RecentResults { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string SearchSubjectText
        {
            get => _searchSubjectText;
            set
            {
                _searchSubjectText = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _ = SearchSubjectsAsync(value);
                }
            }
        }

        public string WelcomeMessage => $"Welcome back, {_currentUserService.GetCurrentUserName()}!";

        public int TotalQuizzesAvailable => AssignedQuizzes.Count;
        public int CompletedQuizzes => RecentResults.Count;
        public int EnrolledClasses => MyClasses.Count;
        public double AverageScore => RecentResults.Any() ? RecentResults.Average(r => r.Score ?? 0) : 0.0;

        // Commands
        public ICommand TakeQuizCommand { get; }
        public ICommand SearchSubjectsCommand { get; }
        public ICommand JoinClassCommand { get; }
        public ICommand ViewResultsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Load assigned quizzes through class enrollments
                    var assignedQuizzes = context.QuizAssignments
                        .Where(qa => qa.AssignedTo == currentUserId.Value || 
                                    context.Enrollments
                                        .Where(e => e.StudentId == currentUserId.Value)
                                        .Select(e => e.ClassId)
                                        .Contains(qa.ClassId ?? 0))
                        .Select(qa => qa.Quiz)
                        .Where(q => q != null)
                        .ToList();

                    // Load enrolled classes
                    var enrolledClasses = context.Enrollments
                        .Where(e => e.StudentId == currentUserId.Value)
                        .Select(e => e.Class)
                        .ToList();

                    // Load recent quiz results
                    var recentResults = context.StudentQuizzes
                        .Where(sq => sq.StudentId == currentUserId.Value)
                        .OrderByDescending(sq => sq.CompletedAt)
                        .Take(5)
                        .ToList();

                    // Load available subjects
                    var subjects = context.Subjects.Take(10).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AssignedQuizzes.Clear();
                        foreach (var quiz in assignedQuizzes)
                            AssignedQuizzes.Add(quiz);

                        MyClasses.Clear();
                        foreach (var classroom in enrolledClasses)
                            MyClasses.Add(classroom);

                        RecentResults.Clear();
                        foreach (var result in recentResults)
                            RecentResults.Add(result);

                        AvailableSubjects.Clear();
                        foreach (var subject in subjects)
                            AvailableSubjects.Add(subject);

                        // Refresh computed properties
                        OnPropertyChanged(nameof(TotalQuizzesAvailable));
                        OnPropertyChanged(nameof(CompletedQuizzes));
                        OnPropertyChanged(nameof(EnrolledClasses));
                        OnPropertyChanged(nameof(AverageScore));
                    });
                });
            }
            catch (Exception ex)
            {
                // Handle error
                App.Current.Dispatcher.Invoke(() =>
                {
                    _mainCursor.StatusMessage = $"Error loading data: {ex.Message}";
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchSubjectsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return;

            await Task.Run(() =>
            {
                using var context = new QuizardContext();
                var subjects = context.Subjects
                    .Where(s => s.Name.Contains(searchText))
                    .Take(10)
                    .ToList();

                App.Current.Dispatcher.Invoke(() =>
                {
                    AvailableSubjects.Clear();
                    foreach (var subject in subjects)
                        AvailableSubjects.Add(subject);
                });
            });
        }

        private void TakeQuiz(Quiz? quiz)
        {
            if (quiz != null)
            {
                // Pass quiz data to TakeQuizViewModel through some mechanism
                // For now, we'll navigate and let TakeQuizViewModel handle quiz selection
                _mainCursor.NavigateTo(AppState.TakeQuiz);
            }
        }

        // Quick action methods
        public void QuickJoinClass()
        {
            _mainCursor.NavigateTo(AppState.JoinClass);
        }

        public void QuickSearchSubjects()
        {
            _mainCursor.NavigateTo(AppState.SearchSubjects);
        }

        public void ViewQuizDetails(Quiz quiz)
        {
            // Navigate to quiz details with selected quiz
            _mainCursor.NavigateTo(AppState.QuizDetails);
        }
    }
}