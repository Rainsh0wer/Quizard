using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class ResultsViewModel : BaseViewModel
    {
        private ObservableCollection<QuizResultSummary> quizResults = new();
        private ObservableCollection<Quiz> teacherQuizzes = new();
        private QuizResultSummary? selectedResult;
        private Quiz? selectedQuiz;
        private string message = string.Empty;
        private string searchText = string.Empty;

        public ObservableCollection<QuizResultSummary> QuizResults
        {
            get => quizResults;
            set => SetProperty(ref quizResults, value);
        }

        public ObservableCollection<Quiz> TeacherQuizzes
        {
            get => teacherQuizzes;
            set => SetProperty(ref teacherQuizzes, value);
        }

        public QuizResultSummary? SelectedResult {
            get => selectedResult;
            set => SetProperty(ref selectedResult, value);
        }

        public Quiz? SelectedQuiz {
            get => selectedQuiz;
            set 
            {
                SetProperty(ref selectedQuiz, value);
                if (value != null)
                    LoadQuizResults();
            }
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
                LoadQuizResults();
            }
        }

        public ICommand ViewDetailedResultCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ViewStudentPerformanceCommand { get; }
        public ICommand RefreshCommand { get; }

        public ResultsViewModel()
        {
            QuizResults = new ObservableCollection<QuizResultSummary>();
            TeacherQuizzes = new ObservableCollection<Quiz>();
            
            ViewDetailedResultCommand = new RelayCommand(ExecuteViewDetailedResult);
            ExportResultsCommand = new RelayCommand(ExecuteExportResults);
            ViewStudentPerformanceCommand = new RelayCommand(ExecuteViewStudentPerformance);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            LoadTeacherQuizzes();
        }

        private void LoadTeacherQuizzes()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var quizzes = context.Quizzes
                        .Include(q => q.Subject)
                        .Where(q => q.CreatedBy == currentUser.UserId)
                        .OrderByDescending(q => q.CreatedAt)
                        .ToList();

                    TeacherQuizzes.Clear();
                    foreach (var quiz in quizzes)
                    {
                        TeacherQuizzes.Add(quiz);
                    }

                    Message = $"Found {quizzes.Count} quizzes";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading quizzes: {ex.Message}";
            }
        }

        private void LoadQuizResults()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var query = context.StudentQuizzes
                        .Include(sq => sq.Student)
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Subject)
                        .Include(sq => sq.StudentAnswers)
                        .Where(sq => sq.Quiz.CreatedBy == currentUser.UserId && sq.FinishedAt != null);

                    if (SelectedQuiz != null)
                    {
                        query = query.Where(sq => sq.QuizId == SelectedQuiz.QuizId);
                    }

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(sq => sq.Student.FullName.Contains(SearchText) ||
                                                 sq.Quiz.Title.Contains(SearchText) ||
                                                 sq.Quiz.Subject.Name.Contains(SearchText));
                    }

                    var results = query.OrderByDescending(sq => sq.FinishedAt).ToList();
                    
                    QuizResults.Clear();
                    foreach (var result in results)
                    {
                        var correctAnswers = result.StudentAnswers.Count(sa => sa.IsCorrect == true);
                        var totalQuestions = result.StudentAnswers.Count;
                        
                        var resultSummary = new QuizResultSummary
                        {
                            StudentQuizId = result.StudentQuizId,
                            StudentName = result.Student.FullName,
                            StudentUsername = result.Student.Username,
                            QuizTitle = result.Quiz.Title,
                            SubjectName = result.Quiz.Subject.Name,
                            Score = result.Score ?? 0,
                            CorrectAnswers = correctAnswers,
                            TotalQuestions = totalQuestions,
                            StartedAt = result.StartedAt ?? DateTime.MinValue,
                            FinishedAt = result.FinishedAt,
                            Duration = result.FinishedAt.HasValue && result.StartedAt.HasValue ? 
                                      FormatTimeSpan(result.FinishedAt.Value - result.StartedAt.Value) : 
                                      "N/A"
                        };
                        QuizResults.Add(resultSummary);
                    }

                    Message = $"Found {QuizResults.Count} completed quiz attempts";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading quiz results: {ex.Message}";
            }
        }

        private void ExecuteViewDetailedResult(object? obj)
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
                Message = $"Error viewing detailed result: {ex.Message}";
            }
        }

        private void ExecuteExportResults(object? obj)
        {
            if (SelectedQuiz == null)
            {
                Message = "Please select a quiz to export results";
                return;
            }

            try
            {
                // Implement export functionality
                Message = "Export functionality not implemented yet";
            }
            catch (Exception ex)
            {
                Message = $"Error exporting results: {ex.Message}";
            }
        }

        private void ExecuteViewStudentPerformance(object? obj)
        {
            if (SelectedResult == null)
            {
                Message = "Please select a result to view student performance";
                return;
            }

            try
            {
                var performancePage = new Views.StudentPerformanceView(SelectedResult.StudentUsername);
                AppNavigationService.Instance.Navigate(performancePage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing student performance: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadTeacherQuizzes();
            LoadQuizResults();
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
    }

    public class QuizResultSummary
    {
        public int StudentQuizId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentUsername { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public double Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string ScoreDisplay => $"{Score:F1}/10";
        public string AccuracyDisplay => TotalQuestions > 0 ? $"{CorrectAnswers}/{TotalQuestions} ({(CorrectAnswers * 100.0 / TotalQuestions):F1}%)" : "N/A";
        public string FormattedStartDate => StartedAt.ToString("dd/MM/yyyy HH:mm");
        public string FormattedFinishDate => FinishedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Not completed";
        public string Grade => Score >= 8 ? "Excellent" : Score >= 6.5 ? "Good" : Score >= 5 ? "Average" : "Poor";
    }
}