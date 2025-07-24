using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class QuizDetailsViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private Quiz? _quiz;
        private bool _isLoading = false;
        private bool _canTakeQuiz = false;
        private bool _hasAlreadyTaken = false;

        public QuizDetailsViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            Questions = new ObservableCollection<QuestionDetailsViewModel>();
            RecentAttempts = new ObservableCollection<QuizResultViewModel>();
            QuizStatistics = new ObservableCollection<QuizStatisticItem>();
            
            // Initialize commands
            TakeQuizCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.TakeQuiz), () => CanTakeQuiz);
            EditQuizCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.CreateQuiz), () => IsQuizOwner);
            DeleteQuizCommand = new RelayCommand(async () => await DeleteQuizAsync(), () => IsQuizOwner);
            BackCommand = new RelayCommand(() => _mainCursor.NavigateTo(
                _currentUserService.IsStudent ? AppState.StudentDashboard : AppState.TeacherDashboard));
            ViewResultsCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.ViewResults));
            RefreshCommand = new RelayCommand(async () => await LoadQuizDetailsAsync());
            
            // Load quiz details (in real implementation, quiz would be passed via parameter)
            _ = LoadQuizDetailsAsync();
        }

        public ObservableCollection<QuestionDetailsViewModel> Questions { get; }
        public ObservableCollection<QuizResultViewModel> RecentAttempts { get; }
        public ObservableCollection<QuizStatisticItem> QuizStatistics { get; }

        public Quiz? Quiz
        {
            get => _quiz;
            set
            {
                _quiz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuizTitle));
                OnPropertyChanged(nameof(QuizDescription));
                OnPropertyChanged(nameof(IsQuizLoaded));
                OnPropertyChanged(nameof(IsQuizOwner));
                UpdateCommands();
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

        public bool CanTakeQuiz
        {
            get => _canTakeQuiz;
            set
            {
                _canTakeQuiz = value;
                OnPropertyChanged();
                UpdateCommands();
            }
        }

        public bool HasAlreadyTaken
        {
            get => _hasAlreadyTaken;
            set
            {
                _hasAlreadyTaken = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TakeQuizButtonText));
            }
        }

        // Computed properties
        public string QuizTitle => Quiz?.Title ?? "Quiz Details";
        public string QuizDescription => Quiz?.Description ?? "";
        public bool IsQuizLoaded => Quiz != null;
        public bool IsQuizOwner => Quiz != null && _currentUserService.GetCurrentUserId() == Quiz.CreatedBy;
        public bool IsStudent => _currentUserService.IsStudent;
        public bool IsTeacher => _currentUserService.IsTeacher;
        public string TakeQuizButtonText => HasAlreadyTaken ? "Retake Quiz" : "Take Quiz";

        // Quiz information
        public string CreatedByText { get; private set; } = "";
        public string CreatedAtText { get; private set; } = "";
        public string SubjectText { get; private set; } = "";
        public int TotalQuestions => Questions.Count;
        public string VisibilityText => Quiz?.IsPublic == true ? "Public" : "Private";

        // Statistics
        public int TotalAttempts { get; private set; }
        public double AverageScore { get; private set; }
        public double HighestScore { get; private set; }
        public double LowestScore { get; private set; }
        public string AverageScoreText => $"{AverageScore:F1}%";
        public string HighestScoreText => $"{HighestScore:F1}%";
        public string LowestScoreText => $"{LowestScore:F1}%";

        // Commands
        public ICommand TakeQuizCommand { get; }
        public ICommand EditQuizCommand { get; }
        public ICommand DeleteQuizCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ViewResultsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadQuizDetailsAsync()
        {
            try
            {
                IsLoading = true;
                
                // In a real implementation, we would get the quiz ID from navigation parameters
                // For now, let's load the first available quiz or one from the current user
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    Quiz? selectedQuiz = null;

                    if (_currentUserService.IsStudent)
                    {
                        // Get first available quiz for student
                        var availableQuizIds = context.QuizAssignments
                            .Where(qa => qa.AssignedTo == currentUserId.Value || 
                                        context.Enrollments
                                            .Where(e => e.StudentId == currentUserId.Value)
                                            .Select(e => e.ClassId)
                                            .Contains(qa.ClassId ?? 0))
                            .Select(qa => qa.QuizId)
                            .ToList();

                        selectedQuiz = context.Quizzes
                            .FirstOrDefault(q => availableQuizIds.Contains(q.QuizId));
                    }
                    else
                    {
                        // Get first quiz created by teacher
                        selectedQuiz = context.Quizzes
                            .FirstOrDefault(q => q.CreatedBy == currentUserId.Value);
                    }

                    if (selectedQuiz == null) return;

                    // Load quiz details
                    var creator = context.Users.Find(selectedQuiz.CreatedBy);
                    var subject = context.Subjects.Find(selectedQuiz.SubjectId);

                    // Load questions
                    var questions = context.Questions
                        .Where(q => q.QuizId == selectedQuiz.QuizId)
                        .OrderBy(q => q.QuestionId)
                        .ToList();

                    var questionDetails = questions.Select(q => new QuestionDetailsViewModel
                    {
                        Question = q,
                        OptionCount = context.QuestionOptions.Count(o => o.QuestionId == q.QuestionId)
                    }).ToList();

                    // Check if student has already taken this quiz
                    var hasAlreadyTaken = false;
                    if (_currentUserService.IsStudent)
                    {
                        hasAlreadyTaken = context.StudentQuizzes
                            .Any(sq => sq.QuizId == selectedQuiz.QuizId && sq.StudentId == currentUserId.Value);
                    }

                    // Load statistics
                    var attempts = context.StudentQuizzes
                        .Where(sq => sq.QuizId == selectedQuiz.QuizId)
                        .ToList();

                    var recentAttempts = attempts
                        .OrderByDescending(sq => sq.CompletedAt)
                        .Take(5)
                        .Select(sq => new QuizResultViewModel
                        {
                            StudentQuiz = sq,
                            Quiz = selectedQuiz,
                            Student = context.Users.Find(sq.StudentId)!
                        })
                        .ToList();

                    var totalAttempts = attempts.Count;
                    var averageScore = attempts.Any() ? attempts.Average(a => a.Score ?? 0) : 0.0;
                    var highestScore = attempts.Any() ? attempts.Max(a => a.Score ?? 0) : 0.0;
                    var lowestScore = attempts.Any() ? attempts.Min(a => a.Score ?? 0) : 0.0;

                    // Create statistics items
                    var statistics = new[]
                    {
                        new QuizStatisticItem { Label = "Total Attempts", Value = totalAttempts.ToString() },
                        new QuizStatisticItem { Label = "Average Score", Value = $"{averageScore:F1}%" },
                        new QuizStatisticItem { Label = "Highest Score", Value = $"{highestScore:F1}%" },
                        new QuizStatisticItem { Label = "Lowest Score", Value = $"{lowestScore:F1}%" },
                        new QuizStatisticItem { Label = "Questions", Value = questions.Count.ToString() },
                        new QuizStatisticItem { Label = "Created", Value = selectedQuiz.CreatedAt?.ToString("MMM dd, yyyy") ?? "" }
                    };

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Quiz = selectedQuiz;
                        CreatedByText = creator?.FullName ?? creator?.Username ?? "Unknown";
                        CreatedAtText = selectedQuiz.CreatedAt?.ToString("MMMM dd, yyyy") ?? "";
                        SubjectText = subject?.Name ?? "Unknown Subject";
                        HasAlreadyTaken = hasAlreadyTaken;
                        CanTakeQuiz = _currentUserService.IsStudent && !IsLoading;

                        // Update statistics
                        TotalAttempts = totalAttempts;
                        AverageScore = averageScore;
                        HighestScore = highestScore;
                        LowestScore = lowestScore;

                        // Update collections
                        Questions.Clear();
                        foreach (var question in questionDetails)
                            Questions.Add(question);

                        RecentAttempts.Clear();
                        foreach (var attempt in recentAttempts)
                            RecentAttempts.Add(attempt);

                        QuizStatistics.Clear();
                        foreach (var stat in statistics)
                            QuizStatistics.Add(stat);

                        // Update computed properties
                        OnPropertyChanged(nameof(TotalQuestions));
                        OnPropertyChanged(nameof(CreatedByText));
                        OnPropertyChanged(nameof(CreatedAtText));
                        OnPropertyChanged(nameof(SubjectText));
                        OnPropertyChanged(nameof(TotalAttempts));
                        OnPropertyChanged(nameof(AverageScoreText));
                        OnPropertyChanged(nameof(HighestScoreText));
                        OnPropertyChanged(nameof(LowestScoreText));
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading quiz details: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteQuizAsync()
        {
            if (Quiz == null) return;

            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var quiz = context.Quizzes.Find(Quiz.QuizId);
                    if (quiz != null)
                    {
                        // Remove related data
                        var questions = context.Questions.Where(q => q.QuizId == quiz.QuizId).ToList();
                        foreach (var question in questions)
                        {
                            var options = context.QuestionOptions.Where(o => o.QuestionId == question.QuestionId);
                            context.QuestionOptions.RemoveRange(options);
                        }
                        context.Questions.RemoveRange(questions);

                        var studentQuizzes = context.StudentQuizzes.Where(sq => sq.QuizId == quiz.QuizId);
                        context.StudentQuizzes.RemoveRange(studentQuizzes);

                        var assignments = context.QuizAssignments.Where(qa => qa.QuizId == quiz.QuizId);
                        context.QuizAssignments.RemoveRange(assignments);

                        context.Quizzes.Remove(quiz);
                        context.SaveChanges();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _mainCursor.StatusMessage = "Quiz deleted successfully.";
                            _mainCursor.NavigateTo(AppState.TeacherDashboard);
                        });
                    }
                });
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

        private void UpdateCommands()
        {
            ((RelayCommand)TakeQuizCommand).RaiseCanExecuteChanged();
            ((RelayCommand)EditQuizCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteQuizCommand).RaiseCanExecuteChanged();
        }

        // Helper methods
        public void ShareQuiz()
        {
            if (Quiz != null)
            {
                var shareText = $"Check out this quiz: {Quiz.Title}";
                _mainCursor.StatusMessage = $"Share text: {shareText}";
            }
        }

        public void PreviewQuiz()
        {
            if (Quiz != null)
            {
                // Navigate to take quiz in preview mode
                _mainCursor.NavigateTo(AppState.TakeQuiz);
            }
        }

        public void DuplicateQuiz()
        {
            if (Quiz != null)
            {
                // Navigate to create quiz with data from current quiz
                _mainCursor.NavigateTo(AppState.CreateQuiz);
            }
        }
    }

    public class QuestionDetailsViewModel : BaseViewModel
    {
        public Question Question { get; set; } = null!;
        public int OptionCount { get; set; }
        public string QuestionText => Question.Content;
        public string CorrectOptionText => $"Correct Answer: {Question.CorrectOption}";
        public bool HasExplanation => !string.IsNullOrWhiteSpace(Question.Explanation);
        public string ExplanationText => Question.Explanation ?? "";
    }

    public class QuizStatisticItem
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }
}