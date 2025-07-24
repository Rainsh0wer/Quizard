using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class QuizManagementViewModel : BaseViewModel
    {
        private ObservableCollection<QuizInfo> quizzes = new();
        private ObservableCollection<Subject> subjects = new();
        private QuizInfo? selectedQuiz;
        private Subject? selectedSubject;
        private string message = string.Empty;
        private string searchText = string.Empty;

        // Quiz Creation Properties
        private string newQuizTitle = string.Empty;
        private string newQuizDescription = string.Empty;
        private bool newQuizIsPublic = true;

        public ObservableCollection<QuizInfo> Quizzes
        {
            get => quizzes;
            set => SetProperty(ref quizzes, value);
        }

        public ObservableCollection<Subject> Subjects
        {
            get => subjects;
            set => SetProperty(ref subjects, value);
        }

        public QuizInfo? SelectedQuiz {
            get => selectedQuiz;
            set => SetProperty(ref selectedQuiz, value);
        }

        public Subject? SelectedSubject {
            get => selectedSubject;
            set => SetProperty(ref selectedSubject, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public string SearchText
        {
            get => searchText;
            set 
            {
                SetProperty(ref searchText, value);
                LoadQuizzes();
            }
        }

        public string NewQuizTitle
        {
            get => newQuizTitle;
            set => SetProperty(ref newQuizTitle, value);
        }

        public string NewQuizDescription
        {
            get => newQuizDescription;
            set => SetProperty(ref newQuizDescription, value);
        }

        public bool NewQuizIsPublic
        {
            get => newQuizIsPublic;
            set => SetProperty(ref newQuizIsPublic, value);
        }

        public ICommand CreateQuizCommand { get; }
        public ICommand EditQuizCommand { get; }
        public ICommand DeleteQuizCommand { get; }
        public ICommand ViewQuizDetailsCommand { get; }
        public ICommand AddQuestionsCommand { get; }
        public ICommand ViewResultsCommand { get; }
        public ICommand RefreshCommand { get; }

        public QuizManagementViewModel()
        {
            Quizzes = new ObservableCollection<QuizInfo>();
            Subjects = new ObservableCollection<Subject>();
            
            CreateQuizCommand = new RelayCommand(ExecuteCreateQuiz);
            EditQuizCommand = new RelayCommand(ExecuteEditQuiz);
            DeleteQuizCommand = new RelayCommand(ExecuteDeleteQuiz);
            ViewQuizDetailsCommand = new RelayCommand(ExecuteViewQuizDetails);
            AddQuestionsCommand = new RelayCommand(ExecuteAddQuestions);
            ViewResultsCommand = new RelayCommand(ExecuteViewResults);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            LoadSubjects();
            LoadQuizzes();
        }

        private void LoadSubjects()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var subjectsList = context.Subjects.OrderBy(s => s.Name).ToList();
                    
                    Subjects.Clear();
                    foreach (var subject in subjectsList)
                    {
                        Subjects.Add(subject);
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading subjects: {ex.Message}";
            }
        }

        private void LoadQuizzes()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var query = context.Quizzes
                        .Include(q => q.Subject)
                        .Include(q => q.Questions)
                        .Include(q => q.StudentQuizzes)
                        .Where(q => q.CreatedBy == currentUser.UserId);

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(q => q.Title.Contains(SearchText) ||
                                               q.Subject.Name.Contains(SearchText) ||
                                               q.Description.Contains(SearchText));
                    }

                    var quizzesList = query.OrderByDescending(q => q.CreatedAt).ToList();
                    
                    Quizzes.Clear();
                    foreach (var quiz in quizzesList)
                    {
                        var quizInfo = new QuizInfo
                        {
                            QuizId = quiz.QuizId,
                            Title = quiz.Title,
                            Description = quiz.Description,
                            SubjectName = quiz.Subject.Name,
                            QuestionCount = quiz.Questions.Count,
                            StudentAttempts = quiz.StudentQuizzes.Count(sq => sq.FinishedAt != null),
                            IsPublic = quiz.IsPublic ?? true,
                            CreatedAt = quiz.CreatedAt
                        };
                        Quizzes.Add(quizInfo);
                    }

                    Message = $"Found {Quizzes.Count} quizzes";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading quizzes: {ex.Message}";
            }
        }

        private void ExecuteCreateQuiz(object? obj)
        {
            if (string.IsNullOrWhiteSpace(NewQuizTitle))
            {
                Message = "Please enter quiz title";
                return;
            }

            if (SelectedSubject == null)
            {
                Message = "Please select a subject";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var newQuiz = new Quiz
                    {
                        Title = NewQuizTitle,
                        Description = NewQuizDescription,
                        SubjectId = SelectedSubject.SubjectId,
                        CreatedBy = currentUser.UserId,
                        IsPublic = NewQuizIsPublic,
                        CreatedAt = DateTime.Now
                    };

                    context.Quizzes.Add(newQuiz);
                    context.SaveChanges();

                    // Clear form
                    NewQuizTitle = string.Empty;
                    NewQuizDescription = string.Empty;
                    SelectedSubject = null;
                    NewQuizIsPublic = true;

                    LoadQuizzes();
                    Message = "Quiz created successfully";

                    // Navigate to add questions
                    var addQuestionsPage = new Views.AddQuestionsView(newQuiz.QuizId);
                    AppNavigationService.Instance.Navigate(addQuestionsPage);
                }
            }
            catch (Exception ex)
            {
                Message = $"Error creating quiz: {ex.Message}";
            }
        }

        private void ExecuteEditQuiz(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to edit";
                return;
            }

            try
            {
                var editPage = new Views.EditQuizView(SelectedQuiz.QuizId);
                AppNavigationService.Instance.Navigate(editPage);
            }
            catch (Exception ex)
            {
                Message = $"Error editing quiz: {ex.Message}";
            }
        }

        private void ExecuteDeleteQuiz(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to delete";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var quiz = context.Quizzes
                        .Include(q => q.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                        .Include(q => q.StudentQuizzes)
                        .ThenInclude(sq => sq.StudentAnswers)
                        .FirstOrDefault(q => q.QuizId == SelectedQuiz.QuizId);

                    if (quiz != null)
                    {
                        // Xóa tất cả student answers
                        foreach (var sq in quiz.StudentQuizzes)
                        {
                            context.StudentAnswers.RemoveRange(sq.StudentAnswers);
                        }

                        // Xóa student quizzes
                        context.StudentQuizzes.RemoveRange(quiz.StudentQuizzes);

                        // Xóa question options
                        foreach (var question in quiz.Questions)
                        {
                            context.QuestionOptions.RemoveRange(question.QuestionOptions);
                        }

                        // Xóa questions
                        context.Questions.RemoveRange(quiz.Questions);

                        // Xóa quiz
                        context.Quizzes.Remove(quiz);
                        context.SaveChanges();

                        LoadQuizzes();
                        Message = "Quiz deleted successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error deleting quiz: {ex.Message}";
            }
        }

        private void ExecuteViewQuizDetails(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to view details";
                return;
            }

            try
            {
                var detailsPage = new Views.QuizDetailsView(SelectedQuiz.QuizId);
                AppNavigationService.Instance.Navigate(detailsPage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing quiz details: {ex.Message}";
            }
        }

        private void ExecuteAddQuestions(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to add questions";
                return;
            }

            try
            {
                var addQuestionsPage = new Views.AddQuestionsView(SelectedQuiz.QuizId);
                AppNavigationService.Instance.Navigate(addQuestionsPage);
            }
            catch (Exception ex)
            {
                Message = $"Error adding questions: {ex.Message}";
            }
        }

        private void ExecuteViewResults(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to view results";
                return;
            }

            try
            {
                var resultsPage = new Views.QuizResultsView(SelectedQuiz.QuizId);
                AppNavigationService.Instance.Navigate(resultsPage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing results: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadQuizzes();
        }
    }

    public class QuizInfo
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int StudentAttempts { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string Status => IsPublic ? "Public" : "Private";
        public string Summary => $"{QuestionCount} question(s), {StudentAttempts} attempt(s)";
    }
}