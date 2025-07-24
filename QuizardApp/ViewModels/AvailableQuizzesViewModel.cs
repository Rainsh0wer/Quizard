using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class AvailableQuizzesViewModel : BaseViewModel
    {
        private ObservableCollection<Quiz> availableQuizzes = new();
        private Quiz? selectedQuiz;
        private string searchText = string.Empty;
        private string message = string.Empty;

        public ObservableCollection<Quiz> AvailableQuizzes
        {
            get => availableQuizzes;
            set => SetProperty(ref availableQuizzes, value);
        }

        public Quiz? SelectedQuiz
        {
            get => selectedQuiz;
            set => SetProperty(ref selectedQuiz, value);
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

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public ICommand TakeQuizCommand { get; }
        public ICommand ViewQuizDetailsCommand { get; }
        public ICommand SaveQuizCommand { get; }
        public ICommand LikeQuizCommand { get; }
        public ICommand RefreshCommand { get; }

        public AvailableQuizzesViewModel()
        {
            AvailableQuizzes = new ObservableCollection<Quiz>();
            TakeQuizCommand = new RelayCommand(ExecuteTakeQuiz);
            ViewQuizDetailsCommand = new RelayCommand(ExecuteViewQuizDetails);
            SaveQuizCommand = new RelayCommand(ExecuteSaveQuiz);
            LikeQuizCommand = new RelayCommand(ExecuteLikeQuiz);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var query = context.Quizzes
                        .Include(q => q.Subject)
                        .Include(q => q.CreatedByNavigation)
                        .Include(q => q.Questions)
                        .Where(q => q.IsPublic == true);

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(q => q.Title.Contains(SearchText) || 
                                               q.Subject.Name.Contains(SearchText) ||
                                               q.Description.Contains(SearchText));
                    }

                    var quizzes = query.OrderByDescending(q => q.CreatedAt).ToList();
                    
                    AvailableQuizzes.Clear();
                    foreach (var quiz in quizzes)
                    {
                        AvailableQuizzes.Add(quiz);
                    }

                    Message = $"Found {quizzes.Count} available quizzes";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading quizzes: {ex.Message}";
            }
        }

        private void ExecuteTakeQuiz(object? obj)
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
                        Message = "You have already completed this quiz. View results instead.";
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

        private void ExecuteSaveQuiz(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to save";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var existingSave = context.SavedQuizzes
                        .FirstOrDefault(sq => sq.StudentId == currentUser.UserId && 
                                             sq.QuizId == SelectedQuiz.QuizId);

                    if (existingSave != null)
                    {
                        // Unsave
                        context.SavedQuizzes.Remove(existingSave);
                        Message = "Quiz removed from saved list";
                    }
                    else
                    {
                        // Save
                        var savedQuiz = new SavedQuiz
                        {
                            StudentId = currentUser.UserId,
                            QuizId = SelectedQuiz.QuizId,
                            SavedAt = DateTime.Now
                        };
                        context.SavedQuizzes.Add(savedQuiz);
                        Message = "Quiz saved successfully";
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Message = $"Error saving quiz: {ex.Message}";
            }
        }

        private void ExecuteLikeQuiz(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to like";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var existingLike = context.QuizLikes
                        .FirstOrDefault(ql => ql.UserId == currentUser.UserId && 
                                             ql.QuizId == SelectedQuiz.QuizId);

                    if (existingLike != null)
                    {
                        // Unlike
                        context.QuizLikes.Remove(existingLike);
                        Message = "Quiz unliked";
                    }
                    else
                    {
                        // Like
                        var quizLike = new QuizLike
                        {
                            UserId = currentUser.UserId,
                            QuizId = SelectedQuiz.QuizId,
                            LikedAt = DateTime.Now
                        };
                        context.QuizLikes.Add(quizLike);
                        Message = "Quiz liked successfully";
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Message = $"Error liking quiz: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadQuizzes();
        }
    }
}