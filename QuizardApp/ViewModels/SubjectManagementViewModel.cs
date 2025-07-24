using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizardApp.ViewModels
{
    public class SubjectManagementViewModel : BaseViewModel
    {
        private ObservableCollection<SubjectDetail> subjects = new();
        private SubjectDetail? selectedSubject;
        private string message = string.Empty;
        private string searchText = string.Empty;

        // Subject Creation Properties
        private string newSubjectName = string.Empty;
        private string newSubjectDescription = string.Empty;

        public ObservableCollection<SubjectDetail> Subjects
        {
            get => subjects;
            set => SetProperty(ref subjects, value);
        }

        public SubjectDetail? SelectedSubject {
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

        public string NewSubjectName
        {
            get => newSubjectName;
            set => SetProperty(ref newSubjectName, value);
        }

        public string NewSubjectDescription
        {
            get => newSubjectDescription;
            set => SetProperty(ref newSubjectDescription, value);
        }

        public ICommand CreateSubjectCommand { get; }
        public ICommand EditSubjectCommand { get; }
        public ICommand DeleteSubjectCommand { get; }
        public ICommand ViewSubjectQuizzesCommand { get; }
        public ICommand RefreshCommand { get; }

        public SubjectManagementViewModel()
        {
            Subjects = new ObservableCollection<SubjectDetail>();
            
            CreateSubjectCommand = new RelayCommand(ExecuteCreateSubject);
            EditSubjectCommand = new RelayCommand(ExecuteEditSubject);
            DeleteSubjectCommand = new RelayCommand(ExecuteDeleteSubject);
            ViewSubjectQuizzesCommand = new RelayCommand(ExecuteViewSubjectQuizzes);
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
                        .Include(s => s.Quizzes)
                        .ThenInclude(q => q.Questions)
                        .Include(s => s.Quizzes)
                        .ThenInclude(q => q.StudentQuizzes)
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
                        var subjectDetail = new SubjectDetail
                        {
                            SubjectId = subject.SubjectId,
                            Name = subject.Name,
                            Description = subject.Description,
                            CreatedAt = subject.CreatedAt,
                            TotalQuizzes = subject.Quizzes.Count,
                            PublicQuizzes = subject.Quizzes.Count(q => q.IsPublic == true),
                            TotalQuestions = subject.Quizzes.Sum(q => q.Questions.Count),
                            TotalAttempts = subject.Quizzes.Sum(q => q.StudentQuizzes.Count(sq => sq.FinishedAt != null))
                        };
                        Subjects.Add(subjectDetail);
                    }

                    Message = $"Found {Subjects.Count} subjects";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading subjects: {ex.Message}";
            }
        }

        private void ExecuteCreateSubject(object? obj)
        {
            if (string.IsNullOrWhiteSpace(NewSubjectName))
            {
                Message = "Please enter subject name";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    // Kiểm tra tên môn học đã tồn tại chưa
                    var existingSubject = context.Subjects
                        .FirstOrDefault(s => s.Name.ToLower() == NewSubjectName.ToLower());

                    if (existingSubject != null)
                    {
                        Message = "Subject with this name already exists";
                        return;
                    }

                    var newSubject = new Subject
                    {
                        Name = NewSubjectName,
                        Description = NewSubjectDescription,
                        CreatedAt = DateTime.Now
                    };

                    context.Subjects.Add(newSubject);
                    context.SaveChanges();

                    // Clear form
                    NewSubjectName = string.Empty;
                    NewSubjectDescription = string.Empty;

                    LoadSubjects();
                    Message = "Subject created successfully";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error creating subject: {ex.Message}";
            }
        }

        private void ExecuteEditSubject(object? obj)
        {
            if (SelectedSubject == null)
            {
                Message = "Please select a subject to edit";
                return;
            }

            try
            {
                var editPage = new Views.EditSubjectView(SelectedSubject.SubjectId);
                AppNavigationService.Instance.Navigate(editPage);
            }
            catch (Exception ex)
            {
                Message = $"Error editing subject: {ex.Message}";
            }
        }

        private void ExecuteDeleteSubject(object? obj)
        {
            if (SelectedSubject == null)
            {
                Message = "Please select a subject to delete";
                return;
            }

            if (SelectedSubject.TotalQuizzes > 0)
            {
                Message = "Cannot delete subject that has quizzes. Please delete all quizzes first.";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var subject = context.Subjects.Find(SelectedSubject.SubjectId);
                    if (subject != null)
                    {
                        context.Subjects.Remove(subject);
                        context.SaveChanges();

                        LoadSubjects();
                        Message = "Subject deleted successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error deleting subject: {ex.Message}";
            }
        }

        private void ExecuteViewSubjectQuizzes(object? obj)
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
                Message = $"Error viewing subject quizzes: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadSubjects();
        }
    }

    public class SubjectDetail
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalQuizzes { get; set; }
        public int PublicQuizzes { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string QuizSummary => $"{TotalQuizzes} quiz(s) ({PublicQuizzes} public)";
        public string StatsSummary => $"{TotalQuestions} question(s), {TotalAttempts} attempt(s)";
    }
}