using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class TeacherDashboardViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private bool _isLoading = false;

        public TeacherDashboardViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            MyQuizzes = new ObservableCollection<Quiz>();
            MyClasses = new ObservableCollection<Classroom>();
            RecentResults = new ObservableCollection<StudentQuiz>();
            QuizStatistics = new ObservableCollection<QuizStatistic>();
            
            // Initialize commands
            CreateQuizCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.CreateQuiz));
            ViewResultsCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.ViewResults));
            ViewClassesCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.ViewClasses));
            SearchSubjectsCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.SearchSubjects));
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            EditQuizCommand = new RelayCommand<Quiz>(EditQuiz);
            DeleteQuizCommand = new RelayCommand<Quiz>(DeleteQuiz);
            
            // Load initial data
            _ = LoadDataAsync();
        }

        public ObservableCollection<Quiz> MyQuizzes { get; }
        public ObservableCollection<Classroom> MyClasses { get; }
        public ObservableCollection<StudentQuiz> RecentResults { get; }
        public ObservableCollection<QuizStatistic> QuizStatistics { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string WelcomeMessage => $"Welcome back, {_currentUserService.GetCurrentUserName()}!";

        // Dashboard statistics
        public int TotalQuizzes => MyQuizzes.Count;
        public int TotalClasses => MyClasses.Count;
        public int TotalStudents => MyClasses.Sum(c => c.Enrollments.Count);
        public int QuizzesTakenThisWeek => RecentResults.Count(r => r.CompletedAt >= DateTime.Now.AddDays(-7));

        // Commands
        public ICommand CreateQuizCommand { get; }
        public ICommand ViewResultsCommand { get; }
        public ICommand ViewClassesCommand { get; }
        public ICommand SearchSubjectsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand EditQuizCommand { get; }
        public ICommand DeleteQuizCommand { get; }

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
                    
                    // Load teacher's quizzes
                    var quizzes = context.Quizzes
                        .Where(q => q.CreatedBy == currentUserId.Value)
                        .OrderByDescending(q => q.CreatedAt)
                        .ToList();

                    // Load teacher's classes
                    var classes = context.Classrooms
                        .Where(c => c.TeacherId == currentUserId.Value)
                        .ToList();

                    // Load recent quiz results from teacher's quizzes
                    var quizIds = quizzes.Select(q => q.QuizId).ToList();
                    var recentResults = context.StudentQuizzes
                        .Where(sq => quizIds.Contains(sq.QuizId))
                        .OrderByDescending(sq => sq.CompletedAt)
                        .Take(10)
                        .ToList();

                    // Calculate quiz statistics
                    var statistics = quizzes.Select(quiz => new QuizStatistic
                    {
                        Quiz = quiz,
                        TotalAttempts = context.StudentQuizzes.Count(sq => sq.QuizId == quiz.QuizId),
                        AverageScore = context.StudentQuizzes
                            .Where(sq => sq.QuizId == quiz.QuizId)
                            .Average(sq => sq.Score) ?? 0,
                        LastTaken = context.StudentQuizzes
                            .Where(sq => sq.QuizId == quiz.QuizId)
                            .Max(sq => sq.CompletedAt)
                    }).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MyQuizzes.Clear();
                        foreach (var quiz in quizzes)
                            MyQuizzes.Add(quiz);

                        MyClasses.Clear();
                        foreach (var classroom in classes)
                            MyClasses.Add(classroom);

                        RecentResults.Clear();
                        foreach (var result in recentResults)
                            RecentResults.Add(result);

                        QuizStatistics.Clear();
                        foreach (var stat in statistics)
                            QuizStatistics.Add(stat);

                        // Refresh computed properties
                        OnPropertyChanged(nameof(TotalQuizzes));
                        OnPropertyChanged(nameof(TotalClasses));
                        OnPropertyChanged(nameof(TotalStudents));
                        OnPropertyChanged(nameof(QuizzesTakenThisWeek));
                    });
                });
            }
            catch (Exception ex)
            {
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

        private void EditQuiz(Quiz? quiz)
        {
            if (quiz != null)
            {
                // Navigate to edit quiz (could be same as create quiz with different mode)
                _mainCursor.NavigateTo(AppState.CreateQuiz);
            }
        }

        private async void DeleteQuiz(Quiz? quiz)
        {
            if (quiz == null) return;

            try
            {
                IsLoading = true;
                
                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    var quizToDelete = context.Quizzes.Find(quiz.QuizId);
                    if (quizToDelete != null)
                    {
                        context.Quizzes.Remove(quizToDelete);
                        context.SaveChanges();
                    }
                });

                // Refresh data
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error deleting quiz: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ViewQuizDetails(Quiz quiz)
        {
            // Navigate to quiz details
            _mainCursor.NavigateTo(AppState.QuizDetails);
        }

        public void ViewClassDetails(Classroom classroom)
        {
            // Navigate to class details
            _mainCursor.NavigateTo(AppState.ViewClasses);
        }
    }

    public class QuizStatistic
    {
        public Quiz Quiz { get; set; } = null!;
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public DateTime? LastTaken { get; set; }
        public string AverageScoreText => $"{AverageScore:F1}%";
        public string LastTakenText => LastTaken?.ToString("MMM dd, yyyy") ?? "Never";
    }
}