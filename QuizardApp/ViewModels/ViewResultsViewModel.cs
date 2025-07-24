using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class QuizResultViewModel : BaseViewModel
    {
        public StudentQuiz StudentQuiz { get; set; } = null!;
        public Quiz Quiz { get; set; } = null!;
        public User Student { get; set; } = null!;
        public string StudentName => Student.FullName ?? Student.Username;
        public string QuizTitle => Quiz.Title;
        public double Score => StudentQuiz.Score ?? 0;
        public string ScoreText => $"{Score:F1}%";
        public string ScoreColor => Score >= 80 ? "Green" : Score >= 60 ? "Orange" : "Red";
        public DateTime CompletedAt => StudentQuiz.CompletedAt ?? DateTime.MinValue;
        public string CompletedAtText => CompletedAt.ToString("MMM dd, yyyy HH:mm");
        public TimeSpan TimeSpent => StudentQuiz.TimeSpent ?? TimeSpan.Zero;
        public string TimeSpentText => $"{TimeSpent.TotalMinutes:F0} min";
        public string Grade => Score >= 90 ? "A" : Score >= 80 ? "B" : Score >= 70 ? "C" : Score >= 60 ? "D" : "F";
    }

    public class QuestionResultViewModel : BaseViewModel
    {
        public Question Question { get; set; } = null!;
        public StudentAnswer? StudentAnswer { get; set; }
        public ObservableCollection<QuestionOption> Options { get; set; } = new();
        public string QuestionContent => Question.Content;
        public string CorrectOption => Question.CorrectOption;
        public string SelectedOption => StudentAnswer?.SelectedOption ?? "";
        public bool IsCorrect => StudentAnswer?.IsCorrect ?? false;
        public string ResultIcon => IsCorrect ? "✓" : "✗";
        public string ResultColor => IsCorrect ? "Green" : "Red";
        public string Explanation => Question.Explanation ?? "";
        public bool HasExplanation => !string.IsNullOrWhiteSpace(Explanation);
    }

    public class ViewResultsViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private bool _isLoading = false;
        private QuizResultViewModel? _selectedResult;
        private string _filterText = string.Empty;
        private string _selectedFilter = "All";

        public ViewResultsViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            QuizResults = new ObservableCollection<QuizResultViewModel>();
            FilteredResults = new ObservableCollection<QuizResultViewModel>();
            QuestionResults = new ObservableCollection<QuestionResultViewModel>();
            
            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await LoadResultsAsync());
            ViewDetailsCommand = new RelayCommand<QuizResultViewModel>(ViewDetails);
            BackToDashboardCommand = new RelayCommand(() => _mainCursor.NavigateTo(
                _currentUserService.IsStudent ? AppState.StudentDashboard : AppState.TeacherDashboard));
            FilterCommand = new RelayCommand(ApplyFilters);
            ExportResultsCommand = new RelayCommand(ExportResults);
            
            // Load initial data
            _ = LoadResultsAsync();
        }

        public ObservableCollection<QuizResultViewModel> QuizResults { get; }
        public ObservableCollection<QuizResultViewModel> FilteredResults { get; }
        public ObservableCollection<QuestionResultViewModel> QuestionResults { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public QuizResultViewModel? SelectedResult
        {
            get => _selectedResult;
            set
            {
                _selectedResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsResultSelected));
                if (value != null)
                {
                    _ = LoadQuestionResultsAsync(value);
                }
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public bool IsResultSelected => SelectedResult != null;
        public bool IsStudent => _currentUserService.IsStudent;
        public bool IsTeacher => _currentUserService.IsTeacher;

        // Statistics
        public int TotalResults => QuizResults.Count;
        public double AverageScore => QuizResults.Any() ? QuizResults.Average(r => r.Score) : 0.0;
        public double HighestScore => QuizResults.Any() ? QuizResults.Max(r => r.Score) : 0.0;
        public double LowestScore => QuizResults.Any() ? QuizResults.Min(r => r.Score) : 0.0;
        public int PassingResults => QuizResults.Count(r => r.Score >= 60);
        public double PassRate => TotalResults > 0 ? (double)PassingResults / TotalResults * 100 : 0.0;

        public string[] FilterOptions => new[] { "All", "Recent", "High Score", "Low Score", "This Week", "This Month" };

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand BackToDashboardCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ExportResultsCommand { get; }

        private async Task LoadResultsAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    IQueryable<StudentQuiz> studentQuizzesQuery;

                    if (_currentUserService.IsStudent)
                    {
                        // Load student's own results
                        studentQuizzesQuery = context.StudentQuizzes
                            .Where(sq => sq.StudentId == currentUserId.Value);
                    }
                    else
                    {
                        // Load results for teacher's quizzes
                        var teacherQuizIds = context.Quizzes
                            .Where(q => q.CreatedBy == currentUserId.Value)
                            .Select(q => q.QuizId)
                            .ToList();

                        studentQuizzesQuery = context.StudentQuizzes
                            .Where(sq => teacherQuizIds.Contains(sq.QuizId));
                    }

                    var studentQuizzes = studentQuizzesQuery
                        .OrderByDescending(sq => sq.CompletedAt)
                        .ToList();

                    var resultViewModels = studentQuizzes.Select(sq =>
                    {
                        var quiz = context.Quizzes.Find(sq.QuizId);
                        var student = context.Users.Find(sq.StudentId);
                        
                        return new QuizResultViewModel
                        {
                            StudentQuiz = sq,
                            Quiz = quiz!,
                            Student = student!
                        };
                    }).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        QuizResults.Clear();
                        foreach (var result in resultViewModels)
                            QuizResults.Add(result);

                        ApplyFilters();
                        
                        // Update statistics
                        OnPropertyChanged(nameof(TotalResults));
                        OnPropertyChanged(nameof(AverageScore));
                        OnPropertyChanged(nameof(HighestScore));
                        OnPropertyChanged(nameof(LowestScore));
                        OnPropertyChanged(nameof(PassingResults));
                        OnPropertyChanged(nameof(PassRate));
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading results: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadQuestionResultsAsync(QuizResultViewModel result)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Load questions for this quiz
                    var questions = context.Questions
                        .Where(q => q.QuizId == result.Quiz.QuizId)
                        .OrderBy(q => q.QuestionId)
                        .ToList();

                    // Load student answers
                    var studentAnswers = context.StudentAnswers
                        .Where(sa => sa.StudentId == result.Student.UserId &&
                                    questions.Select(q => q.QuestionId).Contains(sa.QuestionId))
                        .ToList();

                    var questionResults = questions.Select(question =>
                    {
                        var studentAnswer = studentAnswers.FirstOrDefault(sa => sa.QuestionId == question.QuestionId);
                        var questionResult = new QuestionResultViewModel
                        {
                            Question = question,
                            StudentAnswer = studentAnswer
                        };

                        // Load options
                        var options = context.QuestionOptions
                            .Where(o => o.QuestionId == question.QuestionId)
                            .OrderBy(o => o.OptionLabel)
                            .ToList();

                        foreach (var option in options)
                            questionResult.Options.Add(option);

                        return questionResult;
                    }).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        QuestionResults.Clear();
                        foreach (var questionResult in questionResults)
                            QuestionResults.Add(questionResult);
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading question results: {ex.Message}";
            }
        }

        private void ViewDetails(QuizResultViewModel? result)
        {
            if (result != null)
            {
                SelectedResult = result;
            }
        }

        private void ApplyFilters()
        {
            var filtered = QuizResults.AsEnumerable();

            // Apply text filter
            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                filtered = filtered.Where(r => 
                    r.QuizTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    r.StudentName.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply category filter
            filtered = SelectedFilter switch
            {
                "Recent" => filtered.OrderByDescending(r => r.CompletedAt).Take(10),
                "High Score" => filtered.Where(r => r.Score >= 80),
                "Low Score" => filtered.Where(r => r.Score < 60),
                "This Week" => filtered.Where(r => r.CompletedAt >= DateTime.Now.AddDays(-7)),
                "This Month" => filtered.Where(r => r.CompletedAt >= DateTime.Now.AddDays(-30)),
                _ => filtered
            };

            FilteredResults.Clear();
            foreach (var result in filtered)
                FilteredResults.Add(result);
        }

        private void ExportResults()
        {
            // Future enhancement: Export results to CSV or PDF
            _mainCursor.StatusMessage = "Export functionality coming soon!";
        }

        // Quick analysis methods
        public void ShowStatistics()
        {
            var stats = $@"
Quiz Results Statistics:
Total Results: {TotalResults}
Average Score: {AverageScore:F1}%
Highest Score: {HighestScore:F1}%
Lowest Score: {LowestScore:F1}%
Pass Rate: {PassRate:F1}%
";
            _mainCursor.StatusMessage = stats;
        }

        public void FilterByGrade(string grade)
        {
            var filtered = grade switch
            {
                "A" => QuizResults.Where(r => r.Score >= 90),
                "B" => QuizResults.Where(r => r.Score >= 80 && r.Score < 90),
                "C" => QuizResults.Where(r => r.Score >= 70 && r.Score < 80),
                "D" => QuizResults.Where(r => r.Score >= 60 && r.Score < 70),
                "F" => QuizResults.Where(r => r.Score < 60),
                _ => QuizResults.AsEnumerable()
            };

            FilteredResults.Clear();
            foreach (var result in filtered)
                FilteredResults.Add(result);
        }

        public void SortResults(string sortBy)
        {
            var sorted = sortBy.ToLower() switch
            {
                "score" => FilteredResults.OrderByDescending(r => r.Score),
                "date" => FilteredResults.OrderByDescending(r => r.CompletedAt),
                "student" => FilteredResults.OrderBy(r => r.StudentName),
                "quiz" => FilteredResults.OrderBy(r => r.QuizTitle),
                _ => FilteredResults.AsEnumerable()
            };

            var sortedList = sorted.ToList();
            FilteredResults.Clear();
            foreach (var result in sortedList)
                FilteredResults.Add(result);
        }
    }
}