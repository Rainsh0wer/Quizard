using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class SavedQuizzesViewModel : BaseViewModel
    {
        private ObservableCollection<SavedQuizInfo> savedQuizzes;
        private SavedQuizInfo selectedQuiz;
        private string message;
        private string searchText;

        public ObservableCollection<SavedQuizInfo> SavedQuizzes
        {
            get => savedQuizzes;
            set => SetProperty(ref savedQuizzes, value);
        }

        public SavedQuizInfo SelectedQuiz
        {
            get => selectedQuiz;
            set => SetProperty(ref selectedQuiz, value);
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
                LoadSavedQuizzes();
            }
        }

        public ICommand TakeQuizCommand { get; }
        public ICommand RemoveFromSavedCommand { get; }
        public ICommand ViewQuizDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public SavedQuizzesViewModel()
        {
            SavedQuizzes = new ObservableCollection<SavedQuizInfo>();
            TakeQuizCommand = new RelayCommand(ExecuteTakeQuiz);
            RemoveFromSavedCommand = new RelayCommand(ExecuteRemoveFromSaved);
            ViewQuizDetailsCommand = new RelayCommand(ExecuteViewQuizDetails);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadSavedQuizzes();
        }

        private void LoadSavedQuizzes()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var query = context.SavedQuizzes
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Subject)
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.CreatedByNavigation)
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Questions)
                        .Where(sq => sq.StudentId == currentUser.UserId);

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(sq => sq.Quiz.Title.Contains(SearchText) ||
                                                 sq.Quiz.Subject.Name.Contains(SearchText) ||
                                                 sq.Quiz.Description.Contains(SearchText));
                    }

                    var savedQuizzes = query.OrderByDescending(sq => sq.SavedAt).ToList();
                    
                    SavedQuizzes.Clear();
                    foreach (var sq in savedQuizzes)
                    {
                        var savedQuizInfo = new SavedQuizInfo
                        {
                            SavedId = sq.SavedId,
                            QuizId = sq.QuizId,
                            Title = sq.Quiz.Title,
                            Description = sq.Quiz.Description,
                            SubjectName = sq.Quiz.Subject.Name,
                            CreatedBy = sq.Quiz.CreatedByNavigation.FullName,
                            QuestionCount = sq.Quiz.Questions.Count,
                            SavedAt = sq.SavedAt,
                            IsPublic = sq.Quiz.IsPublic ?? true
                        };
                        SavedQuizzes.Add(savedQuizInfo);
                    }

                    Message = $"Found {SavedQuizzes.Count} saved quizzes";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading saved quizzes: {ex.Message}";
            }
        }

        private void ExecuteTakeQuiz(object obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to take";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    // Kiểm tra xem học sinh đã làm quiz này chưa
                    var existingAttempt = context.StudentQuizzes
                        .FirstOrDefault(sq => sq.StudentId == currentUser.UserId && 
                                             sq.QuizId == SelectedQuiz.QuizId &&
                                             sq.FinishedAt != null);

                    if (existingAttempt != null)
                    {
                        Message = "You have already completed this quiz. You can retake it from Results page.";
                        return;
                    }

                    // Tạo mới StudentQuiz
                    var studentQuiz = new StudentQuiz
                    {
                        StudentId = currentUser.UserId,
                        QuizId = SelectedQuiz.QuizId,
                        StartedAt = DateTime.Now
                    };

                    context.StudentQuizzes.Add(studentQuiz);
                    context.SaveChanges();

                    // Navigate to quiz taking page
                    var quizTakingPage = new Views.QuizTakingView(studentQuiz.StudentQuizId);
                    AppNavigationService.Instance.Navigate(quizTakingPage);
                }
            }
            catch (Exception ex)
            {
                Message = $"Error starting quiz: {ex.Message}";
            }
        }

        private void ExecuteRemoveFromSaved(object obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to remove from saved list";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var savedQuiz = context.SavedQuizzes.Find(SelectedQuiz.SavedId);
                    if (savedQuiz != null)
                    {
                        context.SavedQuizzes.Remove(savedQuiz);
                        context.SaveChanges();
                        
                        SavedQuizzes.Remove(SelectedQuiz);
                        Message = "Quiz removed from saved list successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error removing quiz from saved list: {ex.Message}";
            }
        }

        private void ExecuteViewQuizDetails(object obj)
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

        private void ExecuteRefresh(object obj)
        {
            LoadSavedQuizzes();
        }
    }

    public class SavedQuizInfo
    {
        public int SavedId { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public string CreatedBy { get; set; }
        public int QuestionCount { get; set; }
        public DateTime SavedAt { get; set; }
        public bool IsPublic { get; set; }
        public string FormattedSavedDate => SavedAt.ToString("dd/MM/yyyy HH:mm");
        public string Status => IsPublic ? "Public" : "Private";
    }
}