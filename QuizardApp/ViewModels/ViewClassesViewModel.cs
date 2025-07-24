using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class ClassDetailViewModel : BaseViewModel
    {
        public Classroom Classroom { get; set; } = null!;
        public ObservableCollection<User> Students { get; set; } = new();
        public ObservableCollection<Quiz> AssignedQuizzes { get; set; } = new();
        public int StudentCount => Students.Count;
        public int QuizCount => AssignedQuizzes.Count;
        public string CreatedAtText => Classroom.CreatedAt?.ToString("MMM dd, yyyy") ?? "";
        public double AverageClassScore { get; set; }
        public string AverageScoreText => $"{AverageClassScore:F1}%";
    }

    public class StudentProgressViewModel : BaseViewModel
    {
        public User Student { get; set; } = null!;
        public int QuizzesTaken { get; set; }
        public int QuizzesAssigned { get; set; }
        public double AverageScore { get; set; }
        public DateTime? LastActivity { get; set; }
        public string StudentName => Student.FullName ?? Student.Username;
        public double CompletionRate => QuizzesAssigned > 0 ? (double)QuizzesTaken / QuizzesAssigned * 100 : 0;
        public string CompletionRateText => $"{CompletionRate:F0}%";
        public string AverageScoreText => $"{AverageScore:F1}%";
        public string LastActivityText => LastActivity?.ToString("MMM dd") ?? "Never";
        public string ProgressColor => CompletionRate >= 80 ? "Green" : CompletionRate >= 50 ? "Orange" : "Red";
    }

    public class ViewClassesViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private bool _isLoading = false;
        private ClassDetailViewModel? _selectedClass;
        private string _newClassName = string.Empty;
        private bool _isCreateClassVisible = false;

        public ViewClassesViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            MyClasses = new ObservableCollection<ClassDetailViewModel>();
            StudentProgress = new ObservableCollection<StudentProgressViewModel>();
            
            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await LoadClassesAsync());
            CreateClassCommand = new RelayCommand(async () => await CreateClassAsync(), CanCreateClass);
            SelectClassCommand = new RelayCommand<ClassDetailViewModel>(SelectClass);
            DeleteClassCommand = new RelayCommand<ClassDetailViewModel>(async (c) => await DeleteClassAsync(c));
            RemoveStudentCommand = new RelayCommand<User>(async (s) => await RemoveStudentAsync(s));
            AssignQuizCommand = new RelayCommand<ClassDetailViewModel>(AssignQuiz);
            BackToDashboardCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.TeacherDashboard));
            ToggleCreateClassCommand = new RelayCommand(ToggleCreateClass);
            
            // Load initial data
            _ = LoadClassesAsync();
        }

        public ObservableCollection<ClassDetailViewModel> MyClasses { get; }
        public ObservableCollection<StudentProgressViewModel> StudentProgress { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ClassDetailViewModel? SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsClassSelected));
                if (value != null)
                {
                    _ = LoadStudentProgressAsync(value);
                }
            }
        }

        public string NewClassName
        {
            get => _newClassName;
            set
            {
                _newClassName = value;
                OnPropertyChanged();
                ((RelayCommand)CreateClassCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsCreateClassVisible
        {
            get => _isCreateClassVisible;
            set
            {
                _isCreateClassVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsClassSelected => SelectedClass != null;
        public int TotalClasses => MyClasses.Count;
        public int TotalStudents => MyClasses.Sum(c => c.StudentCount);
        public double OverallAverageScore => MyClasses.Any() ? MyClasses.Average(c => c.AverageClassScore) : 0.0;

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand CreateClassCommand { get; }
        public ICommand SelectClassCommand { get; }
        public ICommand DeleteClassCommand { get; }
        public ICommand RemoveStudentCommand { get; }
        public ICommand AssignQuizCommand { get; }
        public ICommand BackToDashboardCommand { get; }
        public ICommand ToggleCreateClassCommand { get; }

        private async Task LoadClassesAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Load teacher's classes
                    var classes = context.Classrooms
                        .Where(c => c.TeacherId == currentUserId.Value)
                        .OrderByDescending(c => c.CreatedAt)
                        .ToList();

                    var classDetails = classes.Select(classroom =>
                    {
                        // Load students in this class
                        var students = context.Enrollments
                            .Where(e => e.ClassId == classroom.ClassId)
                            .Select(e => e.Student)
                            .ToList();

                        // Load assigned quizzes
                        var assignedQuizzes = context.QuizAssignments
                            .Where(qa => qa.ClassId == classroom.ClassId)
                            .Select(qa => qa.Quiz)
                            .Where(q => q != null)
                            .ToList();

                        // Calculate average class score
                        var classQuizIds = assignedQuizzes.Select(q => q!.QuizId).ToList();
                        var averageScore = context.StudentQuizzes
                            .Where(sq => classQuizIds.Contains(sq.QuizId))
                            .Average(sq => sq.Score) ?? 0.0;

                        var classDetail = new ClassDetailViewModel
                        {
                            Classroom = classroom,
                            AverageClassScore = averageScore
                        };

                        foreach (var student in students)
                            classDetail.Students.Add(student);

                        foreach (var quiz in assignedQuizzes)
                            if (quiz != null) classDetail.AssignedQuizzes.Add(quiz);

                        return classDetail;
                    }).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MyClasses.Clear();
                        foreach (var classDetail in classDetails)
                            MyClasses.Add(classDetail);

                        // Update statistics
                        OnPropertyChanged(nameof(TotalClasses));
                        OnPropertyChanged(nameof(TotalStudents));
                        OnPropertyChanged(nameof(OverallAverageScore));
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading classes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStudentProgressAsync(ClassDetailViewModel classDetail)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var studentProgress = classDetail.Students.Select(student =>
                    {
                        // Get quizzes assigned to this class
                        var assignedQuizIds = context.QuizAssignments
                            .Where(qa => qa.ClassId == classDetail.Classroom.ClassId)
                            .Select(qa => qa.QuizId)
                            .ToList();

                        // Get student's quiz attempts
                        var studentQuizzes = context.StudentQuizzes
                            .Where(sq => sq.StudentId == student.UserId && assignedQuizIds.Contains(sq.QuizId))
                            .ToList();

                        var averageScore = studentQuizzes.Any() ? studentQuizzes.Average(sq => sq.Score ?? 0) : 0.0;
                        var lastActivity = studentQuizzes.Any() ? studentQuizzes.Max(sq => sq.CompletedAt) : null;

                        return new StudentProgressViewModel
                        {
                            Student = student,
                            QuizzesTaken = studentQuizzes.Count,
                            QuizzesAssigned = assignedQuizIds.Count,
                            AverageScore = averageScore,
                            LastActivity = lastActivity
                        };
                    }).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StudentProgress.Clear();
                        foreach (var progress in studentProgress)
                            StudentProgress.Add(progress);
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading student progress: {ex.Message}";
            }
        }

        private bool CanCreateClass() => !IsLoading && !string.IsNullOrWhiteSpace(NewClassName);

        private async Task CreateClassAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var newClass = new Classroom
                    {
                        ClassName = NewClassName.Trim(),
                        TeacherId = currentUserId.Value,
                        CreatedAt = DateTime.Now
                    };

                    context.Classrooms.Add(newClass);
                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _mainCursor.StatusMessage = $"Class '{NewClassName}' created successfully!";
                        NewClassName = string.Empty;
                        IsCreateClassVisible = false;
                    });
                });

                // Refresh classes
                await LoadClassesAsync();
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error creating class: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SelectClass(ClassDetailViewModel? classDetail)
        {
            SelectedClass = classDetail;
        }

        private async Task DeleteClassAsync(ClassDetailViewModel? classDetail)
        {
            if (classDetail == null) return;

            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var classroom = context.Classrooms.Find(classDetail.Classroom.ClassId);
                    if (classroom != null)
                    {
                        // Remove enrollments first
                        var enrollments = context.Enrollments
                            .Where(e => e.ClassId == classroom.ClassId)
                            .ToList();
                        context.Enrollments.RemoveRange(enrollments);

                        // Remove quiz assignments
                        var assignments = context.QuizAssignments
                            .Where(qa => qa.ClassId == classroom.ClassId)
                            .ToList();
                        context.QuizAssignments.RemoveRange(assignments);

                        // Remove classroom
                        context.Classrooms.Remove(classroom);
                        context.SaveChanges();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _mainCursor.StatusMessage = $"Class '{classroom.ClassName}' deleted successfully.";
                            if (SelectedClass == classDetail)
                                SelectedClass = null;
                        });
                    }
                });

                // Refresh classes
                await LoadClassesAsync();
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error deleting class: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemoveStudentAsync(User? student)
        {
            if (student == null || SelectedClass == null) return;

            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var enrollment = context.Enrollments
                        .FirstOrDefault(e => e.ClassId == SelectedClass.Classroom.ClassId && e.StudentId == student.UserId);

                    if (enrollment != null)
                    {
                        context.Enrollments.Remove(enrollment);
                        context.SaveChanges();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _mainCursor.StatusMessage = $"Student '{student.FullName ?? student.Username}' removed from class.";
                        });
                    }
                });

                // Refresh classes and student progress
                await LoadClassesAsync();
                if (SelectedClass != null)
                {
                    await LoadStudentProgressAsync(SelectedClass);
                }
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error removing student: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AssignQuiz(ClassDetailViewModel? classDetail)
        {
            if (classDetail != null)
            {
                // Navigate to quiz assignment (could be part of CreateQuiz or separate view)
                _mainCursor.NavigateTo(AppState.CreateQuiz);
            }
        }

        private void ToggleCreateClass()
        {
            IsCreateClassVisible = !IsCreateClassVisible;
            if (!IsCreateClassVisible)
            {
                NewClassName = string.Empty;
            }
        }

        // Quick actions
        public void GenerateClassCode(ClassDetailViewModel classDetail)
        {
            // Generate a shareable class code (could be the class ID or a special code)
            var classCode = classDetail.Classroom.ClassId.ToString();
            _mainCursor.StatusMessage = $"Class code: {classCode} (share this with students)";
        }

        public void ViewClassStatistics(ClassDetailViewModel classDetail)
        {
            var stats = $@"
Class: {classDetail.Classroom.ClassName}
Students: {classDetail.StudentCount}
Assigned Quizzes: {classDetail.QuizCount}
Average Score: {classDetail.AverageScoreText}
Created: {classDetail.CreatedAtText}
";
            _mainCursor.StatusMessage = stats;
        }

        public void ExportClassData(ClassDetailViewModel classDetail)
        {
            // Future enhancement: Export class data to CSV
            _mainCursor.StatusMessage = "Export functionality coming soon!";
        }
    }
}