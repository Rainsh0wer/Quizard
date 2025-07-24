using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class QuestionCreateViewModel : BaseViewModel
    {
        private string _content = string.Empty;
        private string _explanation = string.Empty;
        private string _correctOption = "A";
        private string _optionA = string.Empty;
        private string _optionB = string.Empty;
        private string _optionC = string.Empty;
        private string _optionD = string.Empty;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string Explanation
        {
            get => _explanation;
            set
            {
                _explanation = value;
                OnPropertyChanged();
            }
        }

        public string CorrectOption
        {
            get => _correctOption;
            set
            {
                _correctOption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string OptionA
        {
            get => _optionA;
            set
            {
                _optionA = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string OptionB
        {
            get => _optionB;
            set
            {
                _optionB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string OptionC
        {
            get => _optionC;
            set
            {
                _optionC = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string OptionD
        {
            get => _optionD;
            set
            {
                _optionD = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Content) &&
            !string.IsNullOrWhiteSpace(OptionA) &&
            !string.IsNullOrWhiteSpace(OptionB) &&
            !string.IsNullOrWhiteSpace(OptionC) &&
            !string.IsNullOrWhiteSpace(OptionD) &&
            new[] { "A", "B", "C", "D" }.Contains(CorrectOption);
    }

    public class CreateQuizViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private Subject? _selectedSubject;
        private bool _isPublic = true;
        private bool _isLoading = false;
        private int _currentQuestionIndex = 0;

        public CreateQuizViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            Questions = new ObservableCollection<QuestionCreateViewModel>();
            AvailableSubjects = new ObservableCollection<Subject>();
            
            // Initialize commands
            AddQuestionCommand = new RelayCommand(AddQuestion);
            RemoveQuestionCommand = new RelayCommand<QuestionCreateViewModel>(RemoveQuestion);
            SaveQuizCommand = new RelayCommand(async () => await SaveQuizAsync(), CanSaveQuiz);
            CancelCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.TeacherDashboard));
            NextQuestionCommand = new RelayCommand(NextQuestion, CanGoNext);
            PreviousQuestionCommand = new RelayCommand(PreviousQuestion, CanGoPrevious);
            
            // Add first question
            AddQuestion();
            
            // Load subjects
            _ = LoadSubjectsAsync();
        }

        public ObservableCollection<QuestionCreateViewModel> Questions { get; }
        public ObservableCollection<Subject> AvailableSubjects { get; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsQuizValid));
                ((RelayCommand)SaveQuizCommand).RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public Subject? SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsQuizValid));
                ((RelayCommand)SaveQuizCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsPublic
        {
            get => _isPublic;
            set
            {
                _isPublic = value;
                OnPropertyChanged();
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

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
                UpdateNavigationCommands();
            }
        }

        public QuestionCreateViewModel? CurrentQuestion =>
            Questions.Count > 0 && _currentQuestionIndex < Questions.Count
                ? Questions[_currentQuestionIndex]
                : null;

        // Computed properties
        public bool IsQuizValid =>
            !string.IsNullOrWhiteSpace(Title) &&
            SelectedSubject != null &&
            Questions.Count > 0 &&
            Questions.All(q => q.IsValid);

        public int TotalQuestions => Questions.Count;
        public int CurrentQuestionNumber => CurrentQuestionIndex + 1;
        public int ValidQuestions => Questions.Count(q => q.IsValid);

        // Commands
        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand SaveQuizCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }

        private async Task LoadSubjectsAsync()
        {
            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    var subjects = context.Subjects.OrderBy(s => s.Name).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableSubjects.Clear();
                        foreach (var subject in subjects)
                            AvailableSubjects.Add(subject);
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading subjects: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddQuestion()
        {
            var newQuestion = new QuestionCreateViewModel();
            Questions.Add(newQuestion);
            CurrentQuestionIndex = Questions.Count - 1;
            
            OnPropertyChanged(nameof(TotalQuestions));
            OnPropertyChanged(nameof(ValidQuestions));
            OnPropertyChanged(nameof(IsQuizValid));
            UpdateNavigationCommands();
        }

        private void RemoveQuestion(QuestionCreateViewModel? question)
        {
            if (question != null && Questions.Count > 1)
            {
                var index = Questions.IndexOf(question);
                Questions.Remove(question);
                
                if (CurrentQuestionIndex >= Questions.Count)
                    CurrentQuestionIndex = Questions.Count - 1;
                else if (CurrentQuestionIndex > index)
                    CurrentQuestionIndex--;
                
                OnPropertyChanged(nameof(TotalQuestions));
                OnPropertyChanged(nameof(ValidQuestions));
                OnPropertyChanged(nameof(IsQuizValid));
                UpdateNavigationCommands();
            }
        }

        private void NextQuestion()
        {
            if (CanGoNext())
                CurrentQuestionIndex++;
        }

        private void PreviousQuestion()
        {
            if (CanGoPrevious())
                CurrentQuestionIndex--;
        }

        private bool CanGoNext() => CurrentQuestionIndex < Questions.Count - 1;
        private bool CanGoPrevious() => CurrentQuestionIndex > 0;
        private bool CanSaveQuiz() => IsQuizValid && !IsLoading;

        private async Task SaveQuizAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue || SelectedSubject == null) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Create quiz
                    var quiz = new Quiz
                    {
                        Title = Title,
                        Description = Description,
                        SubjectId = SelectedSubject.SubjectId,
                        CreatedBy = currentUserId.Value,
                        CreatedAt = DateTime.Now,
                        IsPublic = IsPublic
                    };

                    context.Quizzes.Add(quiz);
                    context.SaveChanges();

                    // Create questions and options
                    foreach (var questionVM in Questions)
                    {
                        var question = new Question
                        {
                            QuizId = quiz.QuizId,
                            Content = questionVM.Content,
                            CorrectOption = questionVM.CorrectOption,
                            Explanation = questionVM.Explanation,
                            CreatedAt = DateTime.Now
                        };

                        context.Questions.Add(question);
                        context.SaveChanges();

                        // Create options
                        var options = new[]
                        {
                            new QuestionOption { QuestionId = question.QuestionId, OptionLabel = "A", OptionText = questionVM.OptionA },
                            new QuestionOption { QuestionId = question.QuestionId, OptionLabel = "B", OptionText = questionVM.OptionB },
                            new QuestionOption { QuestionId = question.QuestionId, OptionLabel = "C", OptionText = questionVM.OptionC },
                            new QuestionOption { QuestionId = question.QuestionId, OptionLabel = "D", OptionText = questionVM.OptionD }
                        };

                        context.QuestionOptions.AddRange(options);
                    }

                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _mainCursor.StatusMessage = "Quiz created successfully!";
                        _mainCursor.NavigateTo(AppState.TeacherDashboard);
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error creating quiz: {ex.Message}";
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
            ((RelayCommand)SaveQuizCommand).RaiseCanExecuteChanged();
        }

        // Quick actions
        public void DuplicateCurrentQuestion()
        {
            if (CurrentQuestion != null)
            {
                var duplicate = new QuestionCreateViewModel
                {
                    Content = CurrentQuestion.Content,
                    OptionA = CurrentQuestion.OptionA,
                    OptionB = CurrentQuestion.OptionB,
                    OptionC = CurrentQuestion.OptionC,
                    OptionD = CurrentQuestion.OptionD,
                    CorrectOption = CurrentQuestion.CorrectOption,
                    Explanation = CurrentQuestion.Explanation
                };

                Questions.Insert(CurrentQuestionIndex + 1, duplicate);
                CurrentQuestionIndex++;
            }
        }

        public void ClearCurrentQuestion()
        {
            if (CurrentQuestion != null)
            {
                CurrentQuestion.Content = string.Empty;
                CurrentQuestion.OptionA = string.Empty;
                CurrentQuestion.OptionB = string.Empty;
                CurrentQuestion.OptionC = string.Empty;
                CurrentQuestion.OptionD = string.Empty;
                CurrentQuestion.Explanation = string.Empty;
                CurrentQuestion.CorrectOption = "A";
            }
        }
    }
}