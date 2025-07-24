using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class TeacherDashboardViewViewModel : BaseViewModel
    {
        private int totalQuizzesCreated;
        private int totalQuestionsCreated;
        private int totalStudentAttempts;
        private int totalClassrooms;
        private int totalStudentsEnrolled;
        private double averageQuizScore;
        private string message = string.Empty;
        private ObservableCollection<RecentQuizActivity> recentActivities = new();
        private ObservableCollection<PopularQuiz> popularQuizzes = new();
        private ObservableCollection<SubjectStatistic> subjectStats = new();

        public int TotalQuizzesCreated
        {
            get => totalQuizzesCreated;
            set => SetProperty(ref totalQuizzesCreated, value);
        }

        public int TotalQuestionsCreated
        {
            get => totalQuestionsCreated;
            set => SetProperty(ref totalQuestionsCreated, value);
        }

        public int TotalStudentAttempts
        {
            get => totalStudentAttempts;
            set => SetProperty(ref totalStudentAttempts, value);
        }

        public int TotalClassrooms
        {
            get => totalClassrooms;
            set => SetProperty(ref totalClassrooms, value);
        }

        public int TotalStudentsEnrolled
        {
            get => totalStudentsEnrolled;
            set => SetProperty(ref totalStudentsEnrolled, value);
        }

        public double AverageQuizScore
        {
            get => averageQuizScore;
            set => SetProperty(ref averageQuizScore, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public ObservableCollection<RecentQuizActivity> RecentActivities
        {
            get => recentActivities;
            set => SetProperty(ref recentActivities, value);
        }

        public ObservableCollection<PopularQuiz> PopularQuizzes
        {
            get => popularQuizzes;
            set => SetProperty(ref popularQuizzes, value);
        }

        public ObservableCollection<SubjectStatistic> SubjectStats
        {
            get => subjectStats;
            set => SetProperty(ref subjectStats, value);
        }

        public string CurrentUserName => CurrentUserService.Instance.CurrentUser?.FullName ?? "Teacher";
        public string WelcomeMessage => $"Welcome back, {CurrentUserName}!";

        public ICommand CreateQuizCommand { get; }
        public ICommand ViewAllResultsCommand { get; }
        public ICommand ManageClassroomsCommand { get; }
        public ICommand ViewQuizDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public TeacherDashboardViewViewModel()
        {
            RecentActivities = new ObservableCollection<RecentQuizActivity>();
            PopularQuizzes = new ObservableCollection<PopularQuiz>();
            SubjectStats = new ObservableCollection<SubjectStatistic>();
            
            CreateQuizCommand = new RelayCommand(ExecuteCreateQuiz);
            ViewAllResultsCommand = new RelayCommand(ExecuteViewAllResults);
            ManageClassroomsCommand = new RelayCommand(ExecuteManageClassrooms);
            ViewQuizDetailsCommand = new RelayCommand(ExecuteViewQuizDetails);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
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

                    // Load basic statistics
                    TotalQuizzesCreated = context.Quizzes
                        .Count(q => q.CreatedBy == currentUser.UserId);

                    TotalQuestionsCreated = context.Quizzes
                        .Where(q => q.CreatedBy == currentUser.UserId)
                        .Sum(q => q.Questions.Count);

                    TotalStudentAttempts = context.StudentQuizzes
                        .Count(sq => sq.Quiz.CreatedBy == currentUser.UserId && sq.FinishedAt != null);

                    TotalClassrooms = context.Classrooms
                        .Count(c => c.TeacherId == currentUser.UserId);

                    TotalStudentsEnrolled = context.Enrollments
                        .Count(e => e.Class.TeacherId == currentUser.UserId);

                    // Calculate average quiz score
                    var allScores = context.StudentQuizzes
                        .Where(sq => sq.Quiz.CreatedBy == currentUser.UserId && 
                                    sq.FinishedAt != null && 
                                    sq.Score != null)
                        .Select(sq => sq.Score.Value)
                        .ToList();

                    AverageQuizScore = allScores.Any() ? allScores.Average() : 0;

                    // Load recent activities
                    LoadRecentActivities(context, currentUser);
                    
                    // Load popular quizzes
                    LoadPopularQuizzes(context, currentUser);
                    
                    // Load subject statistics
                    LoadSubjectStatistics(context, currentUser);

                    Message = "Dashboard loaded successfully";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading dashboard: {ex.Message}";
            }
        }

        private void LoadRecentActivities(QuizardContext context, User currentUser)
        {
            var recentActivities = context.StudentQuizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Quiz)
                .ThenInclude(q => q.Subject)
                .Where(sq => sq.Quiz.CreatedBy == currentUser.UserId && sq.FinishedAt != null)
                .OrderByDescending(sq => sq.FinishedAt)
                .Take(10)
                .ToList();

            RecentActivities.Clear();
            foreach (var activity in recentActivities)
            {
                RecentActivities.Add(new RecentQuizActivity
                {
                    StudentName = activity.Student.FullName,
                    QuizTitle = activity.Quiz.Title,
                    SubjectName = activity.Quiz.Subject.Name,
                    Score = activity.Score ?? 0,
                    CompletedDate = activity.FinishedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    Grade = GetGrade(activity.Score ?? 0)
                });
            }
        }

        private void LoadPopularQuizzes(QuizardContext context, User currentUser)
        {
            var popularQuizzes = context.Quizzes
                .Include(q => q.Subject)
                .Include(q => q.StudentQuizzes)
                .Where(q => q.CreatedBy == currentUser.UserId)
                .Select(q => new {
                    Quiz = q,
                    AttemptCount = q.StudentQuizzes.Count(sq => sq.FinishedAt != null),
                    AverageScore = q.StudentQuizzes.Where(sq => sq.FinishedAt != null && sq.Score != null)
                                                  .Average(sq => (double?)sq.Score) ?? 0
                })
                .OrderByDescending(x => x.AttemptCount)
                .Take(5)
                .ToList();

            PopularQuizzes.Clear();
            foreach (var item in popularQuizzes)
            {
                PopularQuizzes.Add(new PopularQuiz
                {
                    QuizId = item.Quiz.QuizId,
                    Title = item.Quiz.Title,
                    SubjectName = item.Quiz.Subject.Name,
                    AttemptCount = item.AttemptCount,
                    AverageScore = item.AverageScore,
                    QuestionCount = item.Quiz.Questions.Count
                });
            }
        }

        private void LoadSubjectStatistics(QuizardContext context, User currentUser)
        {
            var subjectStats = context.Subjects
                .Where(s => s.Quizzes.Any(q => q.CreatedBy == currentUser.UserId))
                .Select(s => new {
                    Subject = s,
                    QuizCount = s.Quizzes.Count(q => q.CreatedBy == currentUser.UserId),
                    AttemptCount = s.Quizzes.Where(q => q.CreatedBy == currentUser.UserId)
                                           .Sum(q => q.StudentQuizzes.Count(sq => sq.FinishedAt != null)),
                    AverageScore = s.Quizzes.Where(q => q.CreatedBy == currentUser.UserId)
                                           .SelectMany(q => q.StudentQuizzes)
                                           .Where(sq => sq.FinishedAt != null && sq.Score != null)
                                           .Average(sq => (double?)sq.Score) ?? 0
                })
                .OrderByDescending(x => x.AttemptCount)
                .ToList();

            SubjectStats.Clear();
            foreach (var stat in subjectStats)
            {
                SubjectStats.Add(new SubjectStatistic
                {
                    SubjectName = stat.Subject.Name,
                    QuizCount = stat.QuizCount,
                    AttemptCount = stat.AttemptCount,
                    AverageScore = stat.AverageScore
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

        private void ExecuteCreateQuiz(object? obj)
        {
            // Navigate to quiz management - will be handled by parent dashboard
        }

        private void ExecuteViewAllResults(object? obj)
        {
            // Navigate to results - will be handled by parent dashboard
        }

        private void ExecuteManageClassrooms(object? obj)
        {
            // Navigate to classroom management - will be handled by parent dashboard
        }

        private void ExecuteViewQuizDetails(object? obj)
        {
            if (obj is PopularQuiz quiz)
            {
                try
                {
                    var detailsPage = new Views.QuizDetailsView(quiz.QuizId);
                    AppNavigationService.Instance.Navigate(detailsPage);
                }
                catch (Exception ex)
                {
                    Message = $"Error viewing quiz details: {ex.Message}";
                }
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadDashboardData();
        }
    }

    public class RecentQuizActivity
    {
        public string StudentName { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string CompletedDate { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string ScoreDisplay => $"{Score:F1}/10";
    }

    public class PopularQuiz
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
        public int QuestionCount { get; set; }
        public string Summary => $"{AttemptCount} attempts, avg: {AverageScore:F1}/10";
    }

    public class SubjectStatistic
    {
        public string SubjectName { get; set; } = string.Empty;
        public int QuizCount { get; set; }
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
        public string Summary => $"{QuizCount} quiz(s), {AttemptCount} attempt(s)";
        public string ScoreDisplay => $"{AverageScore:F1}/10";
    }
}