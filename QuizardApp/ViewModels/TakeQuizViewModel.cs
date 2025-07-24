using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class QuestionViewModel : BaseViewModel
    {
        private string _selectedOption = string.Empty;

        public Question Question { get; set; } = null!;
        public ObservableCollection<QuestionOption> Options { get; set; } = new();

        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAnswered));
            }
        }

        public bool IsAnswered => !string.IsNullOrEmpty(SelectedOption);
        public bool IsCorrect => SelectedOption == Question?.CorrectOption;
    }

    public class TakeQuizViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private Quiz? _selectedQuiz;
        private int _currentQuestionIndex = 0;
        private bool _isLoading = false;
        private bool _isQuizCompleted = false;
        private DateTime _startTime;
        private int _score = 0;
        private string _timeElapsed = "00:00";

        public TakeQuizViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            Questions = new ObservableCollection<QuestionViewModel>();
            AvailableQuizzes = new ObservableCollection<Quiz>();
            
            // Initialize commands
            SelectQuizCommand = new RelayCommand<Quiz>(SelectQuiz);
            NextQuestionCommand = new RelayCommand(NextQuestion, CanGoNext);
            PreviousQuestionCommand = new RelayCommand(PreviousQuestion, CanGoPrevious);
            SubmitQuizCommand = new RelayCommand(async () => await SubmitQuizAsync(), CanSubmitQuiz);
            BackToDashboardCommand = new RelayCommand(() => _mainCursor.NavigateTo(
                _currentUserService.IsStudent ? AppState.StudentDashboard : AppState.TeacherDashboard));
            
            // Load available quizzes
            _ = LoadAvailableQuizzesAsync();
            
            // Start timer
            _startTime = DateTime.Now;
            StartTimer();
        }

        public ObservableCollection<QuestionViewModel> Questions { get; }
        public ObservableCollection<Quiz> AvailableQuizzes { get; }

        public Quiz? SelectedQuiz
        {
            get => _selectedQuiz;
            set
            {
                _selectedQuiz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuizTitle));
                OnPropertyChanged(nameof(QuizDescription));
                OnPropertyChanged(nameof(IsQuizSelected));
            }
        }

        public QuestionViewModel? CurrentQuestion => 
            Questions.Count > 0 && _currentQuestionIndex < Questions.Count 
                ? Questions[_currentQuestionIndex] 
                : null;

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
                OnPropertyChanged(nameof(ProgressPercentage));
                UpdateNavigationCommands();
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

        public bool IsQuizCompleted
        {
            get => _isQuizCompleted;
            set
            {
                _isQuizCompleted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsQuizInProgress));
            }
        }

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScorePercentage));
            }
        }

        public string TimeElapsed
        {
            get => _timeElapsed;
            set
            {
                _timeElapsed = value;
                OnPropertyChanged();
            }
        }

        // Computed properties
        public string QuizTitle => SelectedQuiz?.Title ?? "Select a Quiz";
        public string QuizDescription => SelectedQuiz?.Description ?? string.Empty;
        public bool IsQuizSelected => SelectedQuiz != null;
        public bool IsQuizInProgress => IsQuizSelected && !IsQuizCompleted && Questions.Count > 0;
        public int TotalQuestions => Questions.Count;
        public int CurrentQuestionNumber => CurrentQuestionIndex + 1;
        public int AnsweredQuestions => Questions.Count(q => q.IsAnswered);
        public double ProgressPercentage => TotalQuestions > 0 ? (double)CurrentQuestionNumber / TotalQuestions * 100 : 0;
        public double ScorePercentage => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;

        // Commands
        public ICommand SelectQuizCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand SubmitQuizCommand { get; }
        public ICommand BackToDashboardCommand { get; }

        private async Task LoadAvailableQuizzesAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Load quizzes assigned to the student
                    var quizzes = context.QuizAssignments
                        .Where(qa => qa.AssignedTo == currentUserId.Value || 
                                    context.Enrollments
                                        .Where(e => e.StudentId == currentUserId.Value)
                                        .Select(e => e.ClassId)
                                        .Contains(qa.ClassId ?? 0))
                        .Select(qa => qa.Quiz)
                        .Where(q => q != null)
                        .Distinct()
                        .ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableQuizzes.Clear();
                        foreach (var quiz in quizzes)
                            AvailableQuizzes.Add(quiz!);
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading quizzes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SelectQuiz(Quiz? quiz)
        {
            if (quiz == null) return;

            try
            {
                IsLoading = true;
                SelectedQuiz = quiz;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var questions = context.Questions
                        .Where(q => q.QuizId == quiz.QuizId)
                        .ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Questions.Clear();
                        foreach (var question in questions)
                        {
                            var questionVM = new QuestionViewModel
                            {
                                Question = question
                            };

                            // Load options
                            var options = context.QuestionOptions
                                .Where(o => o.QuestionId == question.QuestionId)
                                .ToList();

                            foreach (var option in options)
                                questionVM.Options.Add(option);

                            Questions.Add(questionVM);
                        }

                        CurrentQuestionIndex = 0;
                        OnPropertyChanged(nameof(TotalQuestions));
                        OnPropertyChanged(nameof(IsQuizInProgress));
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading quiz: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NextQuestion()
        {
            if (CanGoNext())
            {
                CurrentQuestionIndex++;
            }
        }

        private void PreviousQuestion()
        {
            if (CanGoPrevious())
            {
                CurrentQuestionIndex--;
            }
        }

        private bool CanGoNext() => CurrentQuestionIndex < Questions.Count - 1;
        private bool CanGoPrevious() => CurrentQuestionIndex > 0;
        private bool CanSubmitQuiz() => Questions.Count > 0 && Questions.All(q => q.IsAnswered);

        private async Task SubmitQuizAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue || SelectedQuiz == null) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Calculate score
                    var correctAnswers = Questions.Count(q => q.IsCorrect);
                    var totalQuestions = Questions.Count;
                    var scorePercentage = (double)correctAnswers / totalQuestions * 100;

                    // Save StudentQuiz
                    var studentQuiz = new StudentQuiz
                    {
                        StudentId = currentUserId.Value,
                        QuizId = SelectedQuiz.QuizId,
                        Score = scorePercentage,
                        CompletedAt = DateTime.Now,
                        TimeSpent = DateTime.Now - _startTime
                    };

                    context.StudentQuizzes.Add(studentQuiz);
                    context.SaveChanges();

                    // Save individual answers
                    foreach (var questionVM in Questions)
                    {
                        var studentAnswer = new StudentAnswer
                        {
                            StudentId = currentUserId.Value,
                            QuestionId = questionVM.Question.QuestionId,
                            SelectedOption = questionVM.SelectedOption,
                            IsCorrect = questionVM.IsCorrect
                        };

                        context.StudentAnswers.Add(studentAnswer);
                    }

                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Score = correctAnswers;
                        IsQuizCompleted = true;
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error submitting quiz: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateNavigationCommands()
        {
            ((RelayCommand)NextQuestionCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PreviousQuestionCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SubmitQuizCommand).RaiseCanExecuteChanged();
        }

        private void StartTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - _startTime;
                TimeElapsed = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            };
            
            timer.Start();
        }
    }
}