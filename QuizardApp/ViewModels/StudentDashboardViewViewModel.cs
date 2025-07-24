using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class StudentDashboardViewViewModel : BaseViewModel
    {
        private int totalQuizzesAvailable;
        private int totalQuizzesTaken;
        private int totalQuizzesPassed;
        private double averageScore;
        private int savedQuizzesCount;
        private string message = string.Empty;
        private ObservableCollection<RecentQuizResult> recentResults = new();
        private ObservableCollection<RecommendedQuiz> recommendedQuizzes = new();

        public int TotalQuizzesAvailable
        {
            get => totalQuizzesAvailable;
            set => SetProperty(ref totalQuizzesAvailable, value);
        }

        public int TotalQuizzesTaken
        {
            get => totalQuizzesTaken;
            set => SetProperty(ref totalQuizzesTaken, value);
        }

        public int TotalQuizzesPassed
        {
            get => totalQuizzesPassed;
            set => SetProperty(ref totalQuizzesPassed, value);
        }

        public double AverageScore
        {
            get => averageScore;
            set => SetProperty(ref averageScore, value);
        }

        public int SavedQuizzesCount
        {
            get => savedQuizzesCount;
            set => SetProperty(ref savedQuizzesCount, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public ObservableCollection<RecentQuizResult> RecentResults
        {
            get => recentResults;
            set => SetProperty(ref recentResults, value);
        }

        public ObservableCollection<RecommendedQuiz> RecommendedQuizzes
        {
            get => recommendedQuizzes;
            set => SetProperty(ref recommendedQuizzes, value);
        }

        public string CurrentUserName => CurrentUserService.Instance.CurrentUser?.FullName ?? "Student";
        public string WelcomeMessage => $"Welcome back, {CurrentUserName}!";

        public ICommand ViewAllQuizzesCommand { get; }
        public ICommand ViewAllResultsCommand { get; }
        public ICommand TakeQuizCommand { get; }
        public ICommand RefreshCommand { get; }
        // Command điều hướng từ ViewModel cha
        public ICommand? ShowAvailableQuizzesCommand { get; set; }
        public ICommand? ShowSubjectsCommand { get; set; }
        public ICommand? ShowMyResultsCommand { get; set; }
        // Command trung gian để gọi command cha
        public ICommand ShowAvailableQuizzesRelayCommand { get; }
        public ICommand ShowSubjectsRelayCommand { get; }
        public ICommand ShowMyResultsRelayCommand { get; }

        public StudentDashboardViewViewModel(
            ICommand? showAvailableQuizzesCommand = null,
            ICommand? showSubjectsCommand = null,
            ICommand? showMyResultsCommand = null)
        {
            RecentResults = new ObservableCollection<RecentQuizResult>();
            RecommendedQuizzes = new ObservableCollection<RecommendedQuiz>();
            ViewAllQuizzesCommand = new RelayCommand(ExecuteViewAllQuizzes);
            ViewAllResultsCommand = new RelayCommand(ExecuteViewAllResults);
            TakeQuizCommand = new RelayCommand(ExecuteTakeQuiz);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            // Gán command điều hướng nếu có
            ShowAvailableQuizzesCommand = showAvailableQuizzesCommand;
            ShowSubjectsCommand = showSubjectsCommand;
            ShowMyResultsCommand = showMyResultsCommand;
            // Command trung gian
            ShowAvailableQuizzesRelayCommand = new RelayCommand(_ => ShowAvailableQuizzesCommand?.Execute(null));
            ShowSubjectsRelayCommand = new RelayCommand(_ => ShowSubjectsCommand?.Execute(null));
            ShowMyResultsRelayCommand = new RelayCommand(_ => ShowMyResultsCommand?.Execute(null));
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    if (currentUser == null) return;

                    // Load statistics
                    TotalQuizzesAvailable = context.Quizzes.Count(q => q.IsPublic == true);
                    
                    var studentQuizzes = context.StudentQuizzes
                        .Where(sq => sq.StudentId == currentUser.UserId && sq.FinishedAt != null)
                        .ToList();
                    
                    TotalQuizzesTaken = studentQuizzes.Count;
                    TotalQuizzesPassed = studentQuizzes.Count(sq => sq.Score >= 5.0);
                    AverageScore = studentQuizzes.Any() ? studentQuizzes.Average(sq => sq.Score ?? 0) : 0;

                    SavedQuizzesCount = context.SavedQuizzes
                        .Count(sq => sq.StudentId == currentUser.UserId);

                    // Load recent results
                    LoadRecentResults(context, currentUser);
                    
                    // Load recommended quizzes
                    LoadRecommendedQuizzes(context, currentUser);

                    Message = "Dashboard loaded successfully";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading dashboard: {ex.Message}";
            }
        }

        private void LoadRecentResults(QuizardContext context, User currentUser)
        {
            var recentResults = context.StudentQuizzes
                .Include(sq => sq.Quiz)
                .ThenInclude(q => q.Subject)
                .Where(sq => sq.StudentId == currentUser.UserId && sq.FinishedAt != null)
                .OrderByDescending(sq => sq.FinishedAt)
                .Take(5)
                .ToList();

            RecentResults.Clear();
            foreach (var result in recentResults)
            {
                RecentResults.Add(new RecentQuizResult
                {
                    QuizTitle = result.Quiz.Title,
                    SubjectName = result.Quiz.Subject.Name,
                    Score = result.Score ?? 0,
                    CompletedDate = result.FinishedAt?.ToString("dd/MM/yyyy") ?? "",
                    Grade = GetGrade(result.Score ?? 0)
                });
            }
        }

        private void LoadRecommendedQuizzes(QuizardContext context, User currentUser)
        {
            // Get subjects student has taken quizzes in
            var takenSubjects = context.StudentQuizzes
                .Include(sq => sq.Quiz)
                .Where(sq => sq.StudentId == currentUser.UserId)
                .Select(sq => sq.Quiz.SubjectId)
                .Distinct()
                .ToList();

            // Get quizzes student hasn't taken yet, prioritizing familiar subjects
            var recommendedQuizzes = context.Quizzes
                .Include(q => q.Subject)
                .Include(q => q.Questions)
                .Where(q => q.IsPublic == true && 
                           !context.StudentQuizzes.Any(sq => sq.StudentId == currentUser.UserId && sq.QuizId == q.QuizId))
                .OrderByDescending(q => takenSubjects.Contains(q.SubjectId))
                .ThenByDescending(q => q.CreatedAt)
                .Take(5)
                .ToList();

            RecommendedQuizzes.Clear();
            foreach (var quiz in recommendedQuizzes)
            {
                RecommendedQuizzes.Add(new RecommendedQuiz
                {
                    QuizId = quiz.QuizId,
                    Title = quiz.Title,
                    SubjectName = quiz.Subject.Name,
                    QuestionCount = quiz.Questions.Count,
                    Description = quiz.Description
                });
            }
        }

        private string GetGrade(double score)
        {
            if (score >= 8.5) return "Excellent";
            if (score >= 7.0) return "Good";
            if (score >= 5.0) return "Pass";
            return "Fail";
        }

        private void ExecuteViewAllQuizzes(object? obj)
        {
            // Navigate to available quizzes - will be handled by parent dashboard
        }

        private void ExecuteViewAllResults(object? obj)
        {
            // Navigate to results - will be handled by parent dashboard
        }

        private void ExecuteTakeQuiz(object? obj)
        {
            if (obj is RecommendedQuiz quiz)
            {
                try
                {
                    using (var context = new QuizardContext())
                    {
                        var currentUser = CurrentUserService.Instance.CurrentUser;
                        
                        var studentQuiz = new StudentQuiz
                        {
                            StudentId = currentUser.UserId,
                            QuizId = quiz.QuizId,
                            StartedAt = DateTime.Now
                        };

                        context.StudentQuizzes.Add(studentQuiz);
                        context.SaveChanges();

                        var quizTakingPage = new Views.QuizTakingView(studentQuiz.StudentQuizId);
                        AppNavigationService.Instance.Navigate(quizTakingPage);
                    }
                }
                catch (Exception ex)
                {
                    Message = $"Error starting quiz: {ex.Message}";
                }
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadDashboardData();
        }
    }

    public class RecentQuizResult
    {
        public string QuizTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string CompletedDate { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string ScoreDisplay => $"{Score:F1}/10";
    }

    public class RecommendedQuiz
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Summary => $"{QuestionCount} questions";
    }
}