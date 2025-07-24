using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class StudentResultsViewModel : BaseViewModel
    {
        private ObservableCollection<StudentQuizResult> results;
        private StudentQuizResult selectedResult;
        private string message;
        private string searchText;

        public ObservableCollection<StudentQuizResult> Results
        {
            get => results;
            set => SetProperty(ref results, value);
        }

        public StudentQuizResult SelectedResult
        {
            get => selectedResult;
            set => SetProperty(ref selectedResult, value);
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
                LoadResults();
            }
        }

        public ICommand ViewDetailCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand RetakeQuizCommand { get; }

        public StudentResultsViewModel()
        {
            Results = new ObservableCollection<StudentQuizResult>();
            ViewDetailCommand = new RelayCommand(ExecuteViewDetail);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            RetakeQuizCommand = new RelayCommand(ExecuteRetakeQuiz);
            LoadResults();
        }

        private void LoadResults()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var query = context.StudentQuizzes
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Subject)
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Questions)
                        .Where(sq => sq.StudentId == currentUser.UserId && sq.FinishedAt != null);

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(sq => sq.Quiz.Title.Contains(SearchText) ||
                                                 sq.Quiz.Subject.Name.Contains(SearchText));
                    }

                    var studentQuizzes = query.OrderByDescending(sq => sq.FinishedAt).ToList();
                    
                    Results.Clear();
                    foreach (var sq in studentQuizzes)
                    {
                        var result = new StudentQuizResult
                        {
                            StudentQuizId = sq.StudentQuizId,
                            QuizTitle = sq.Quiz.Title,
                            SubjectName = sq.Quiz.Subject.Name,
                            Score = sq.Score ?? 0,
                            TotalQuestions = sq.Quiz.Questions.Count,
                            StartedAt = sq.StartedAt,
                            FinishedAt = sq.FinishedAt,
                            Duration = sq.FinishedAt.HasValue ? 
                                      (sq.FinishedAt.Value - sq.StartedAt).ToString(@"hh\:mm\:ss") : 
                                      "N/A"
                        };
                        Results.Add(result);
                    }

                    Message = $"Found {Results.Count} completed quizzes";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading results: {ex.Message}";
            }
        }

        private void ExecuteViewDetail(object obj)
        {
            if (SelectedResult == null)
            {
                Message = "Please select a result to view details";
                return;
            }

            try
            {
                var detailPage = new Views.QuizResultDetailView(SelectedResult.StudentQuizId);
                AppNavigationService.Instance.Navigate(detailPage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing result details: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object obj)
        {
            LoadResults();
        }

        private void ExecuteRetakeQuiz(object obj)
        {
            if (SelectedResult == null)
            {
                Message = "Please select a quiz to retake";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    var studentQuiz = context.StudentQuizzes.Find(SelectedResult.StudentQuizId);
                    
                    if (studentQuiz != null)
                    {
                        // Tạo mới StudentQuiz cho lần làm bài mới
                        var newStudentQuiz = new StudentQuiz
                        {
                            StudentId = currentUser.UserId,
                            QuizId = studentQuiz.QuizId,
                            StartedAt = DateTime.Now
                        };

                        context.StudentQuizzes.Add(newStudentQuiz);
                        context.SaveChanges();

                        // Navigate to quiz taking page
                        var quizTakingPage = new Views.QuizTakingView(newStudentQuiz.StudentQuizId);
                        AppNavigationService.Instance.Navigate(quizTakingPage);
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error retaking quiz: {ex.Message}";
            }
        }
    }

    public class StudentQuizResult
    {
        public int StudentQuizId { get; set; }
        public string QuizTitle { get; set; }
        public string SubjectName { get; set; }
        public double Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Duration { get; set; }
        public string ScorePercentage => $"{Score:F1}/10";
        public string FormattedStartDate => StartedAt.ToString("dd/MM/yyyy HH:mm");
        public string FormattedFinishDate => FinishedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Not completed";
    }
}