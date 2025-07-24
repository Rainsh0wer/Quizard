using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class StudentSubjectsViewModel : BaseViewModel
    {
        private ObservableCollection<SubjectInfo> subjects = new();
        private SubjectInfo? selectedSubject;
        private string message = string.Empty;
        private string searchText = string.Empty;

        public ObservableCollection<SubjectInfo> Subjects
        {
            get => subjects;
            set => SetProperty(ref subjects, value);
        }

        public SubjectInfo? SelectedSubject {
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
                LoadSubjects();
            }
        }

        public ICommand ViewQuizzesCommand { get; }
        public ICommand RefreshCommand { get; }

        public StudentSubjectsViewModel()
        {
            Subjects = new ObservableCollection<SubjectInfo>();
            ViewQuizzesCommand = new RelayCommand(ExecuteViewQuizzes);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadSubjects();
        }

        private void LoadSubjects()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var query = context.Subjects
                        .Include(s => s.Quizzes.Where(q => q.IsPublic == true))
                        .ThenInclude(q => q.Questions)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(s => s.Name.Contains(SearchText) ||
                                               s.Description.Contains(SearchText));
                    }

                    var subjectsList = query.OrderBy(s => s.Name).ToList();
                    
                    Subjects.Clear();
                    foreach (var subject in subjectsList)
                    {
                        var subjectInfo = new SubjectInfo
                        {
                            SubjectId = subject.SubjectId,
                            Name = subject.Name,
                            Description = subject.Description,
                            CreatedAt = subject.CreatedAt,
                            QuizCount = subject.Quizzes.Count(q => q.IsPublic == true),
                            TotalQuestions = subject.Quizzes.Where(q => q.IsPublic == true)
                                                          .Sum(q => q.Questions.Count)
                        };
                        Subjects.Add(subjectInfo);
                    }

                    Message = $"Found {Subjects.Count} subjects";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading subjects: {ex.Message}";
            }
        }

        private void ExecuteViewQuizzes(object? obj)
        {
            if (SelectedSubject == null)
            {
                Message = "Please select a subject to view quizzes";
                return;
            }

            try
            {
                var quizzesBySubjectPage = new Views.QuizzesBySubjectView(SelectedSubject.SubjectId);
                AppNavigationService.Instance.Navigate(quizzesBySubjectPage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing quizzes: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadSubjects();
        }
    }

    public class SubjectInfo
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int QuizCount { get; set; }
        public int TotalQuestions { get; set; }
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy");
        public string QuizSummary => $"{QuizCount} quiz(s), {TotalQuestions} question(s)";
    }
}