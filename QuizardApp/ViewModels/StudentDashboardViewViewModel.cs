using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class StudentDashboardViewViewModel : BaseViewModel
    {
        private ObservableCollection<StudentQuiz> _recentQuizzes;
        private int _quizzesCompleted;
        private int _savedQuizzes;
        private string _averageScore;
        private string _bestScore;

        public string WelcomeMessage => $"Welcome back, {CurrentUserService.Instance.CurrentUser?.FullName ?? "Student"}!";

        public ObservableCollection<StudentQuiz> RecentQuizzes
        {
            get => _recentQuizzes;
            set => SetProperty(ref _recentQuizzes, value);
        }

        public int QuizzesCompleted
        {
            get => _quizzesCompleted;
            set => SetProperty(ref _quizzesCompleted, value);
        }

        public int SavedQuizzes
        {
            get => _savedQuizzes;
            set => SetProperty(ref _savedQuizzes, value);
        }

        public string AverageScore
        {
            get => _averageScore;
            set => SetProperty(ref _averageScore, value);
        }

        public string BestScore
        {
            get => _bestScore;
            set => SetProperty(ref _bestScore, value);
        }

        public ICommand TakeQuizCommand { get; }
        public ICommand BrowseSubjectsCommand { get; }
        public ICommand ViewResultsCommand { get; }

        public StudentDashboardViewViewModel()
        {
            TakeQuizCommand = new RelayCommand(_ => TakeQuiz());
            BrowseSubjectsCommand = new RelayCommand(_ => BrowseSubjects());
            ViewResultsCommand = new RelayCommand(_ => ViewResults());

            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                using var context = new QuizardContext();
                var currentUserId = CurrentUserService.Instance.CurrentUser?.UserId ?? 0;

                // Load statistics
                QuizzesCompleted = await context.StudentQuizzes
                    .Where(sq => sq.StudentId == currentUserId && sq.FinishedAt.HasValue)
                    .CountAsync();

                SavedQuizzes = await context.SavedQuizzes
                    .Where(sq => sq.StudentId == currentUserId)
                    .CountAsync();

                // Calculate scores
                var scores = await context.StudentQuizzes
                    .Where(sq => sq.StudentId == currentUserId && sq.Score.HasValue)
                    .Select(sq => sq.Score.Value)
                    .ToListAsync();

                if (scores.Any())
                {
                    AverageScore = $"{scores.Average():F1}%";
                    BestScore = $"{scores.Max():F1}%";
                }
                else
                {
                    AverageScore = "N/A";
                    BestScore = "N/A";
                }

                // Load recent quizzes
                var recentQuizzes = await context.StudentQuizzes
                    .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Subject)
                    .Where(sq => sq.StudentId == currentUserId && sq.FinishedAt.HasValue)
                    .OrderByDescending(sq => sq.FinishedAt)
                    .Take(5)
                    .ToListAsync();

                RecentQuizzes = new ObservableCollection<StudentQuiz>(recentQuizzes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student dashboard data: {ex.Message}");
            }
        }

        private void TakeQuiz()
        {
            // Navigate to available quizzes
        }

        private void BrowseSubjects()
        {
            // Navigate to subjects view
        }

        private void ViewResults()
        {
            // Navigate to results view
        }
    }
}