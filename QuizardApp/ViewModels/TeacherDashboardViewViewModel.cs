using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class TeacherDashboardViewViewModel : BaseViewModel
    {
        private ObservableCollection<Quiz> _recentQuizzes;
        private int _totalQuizzes;
        private int _totalStudents;
        private int _totalClassrooms;
        private string _averageScore;

        public string WelcomeMessage => $"Welcome back, {CurrentUserService.Instance.CurrentUser?.FullName ?? "Teacher"}!";

        public ObservableCollection<Quiz> RecentQuizzes
        {
            get => _recentQuizzes;
            set => SetProperty(ref _recentQuizzes, value);
        }

        public int TotalQuizzes
        {
            get => _totalQuizzes;
            set => SetProperty(ref _totalQuizzes, value);
        }

        public int TotalStudents
        {
            get => _totalStudents;
            set => SetProperty(ref _totalStudents, value);
        }

        public int TotalClassrooms
        {
            get => _totalClassrooms;
            set => SetProperty(ref _totalClassrooms, value);
        }

        public string AverageScore
        {
            get => _averageScore;
            set => SetProperty(ref _averageScore, value);
        }

        public ICommand CreateQuizCommand { get; }
        public ICommand CreateClassroomCommand { get; }
        public ICommand ViewResultsCommand { get; }

        public TeacherDashboardViewViewModel()
        {
            CreateQuizCommand = new RelayCommand(_ => CreateQuiz());
            CreateClassroomCommand = new RelayCommand(_ => CreateClassroom());
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
                TotalQuizzes = await context.Quizzes
                    .Where(q => q.CreatedBy == currentUserId)
                    .CountAsync();

                TotalClassrooms = await context.Classrooms
                    .Where(c => c.TeacherId == currentUserId)
                    .CountAsync();

                TotalStudents = await context.Enrollments
                    .Where(e => e.Class.TeacherId == currentUserId)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .CountAsync();

                // Calculate average score
                var scores = await context.StudentQuizzes
                    .Where(sq => sq.Quiz.CreatedBy == currentUserId && sq.Score.HasValue)
                    .Select(sq => sq.Score.Value)
                    .ToListAsync();

                AverageScore = scores.Any() ? $"{scores.Average():F1}%" : "N/A";

                // Load recent quizzes
                var quizzes = await context.Quizzes
                    .Include(q => q.Subject)
                    .Where(q => q.CreatedBy == currentUserId)
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                RecentQuizzes = new ObservableCollection<Quiz>(quizzes);
            }
            catch (Exception ex)
            {
                // Handle error - in production you might want to show a message to the user
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private void CreateQuiz()
        {
            // Navigate to quiz creation
            // This will be implemented when we create the QuizManagementView
        }

        private void CreateClassroom()
        {
            // Navigate to classroom creation
            // This will be implemented when we create the ClassroomManagementView
        }

        private void ViewResults()
        {
            // Navigate to results view
            // This will be implemented when we create the ResultsView
        }
    }
}