using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace QuizardApp.ViewModels
{
    public class ClassroomManagementViewModel : BaseViewModel
    {
        private ObservableCollection<ClassroomInfo> classrooms = new();
        private ObservableCollection<User> availableStudents = new();
        private ClassroomInfo? selectedClassroom;
        private User? selectedStudent;
        private string message = string.Empty;
        private string searchText = string.Empty;

        // Classroom Creation Properties
        private string newClassName = string.Empty;

        public ObservableCollection<ClassroomInfo> Classrooms
        {
            get => classrooms;
            set => SetProperty(ref classrooms, value);
        }

        public ObservableCollection<User> AvailableStudents
        {
            get => availableStudents;
            set => SetProperty(ref availableStudents, value);
        }

        public ClassroomInfo? SelectedClassroom {
            get => selectedClassroom;
            set 
            {
                SetProperty(ref selectedClassroom, value);
                if (value != null)
                    LoadAvailableStudents();
            }
        }

        public User? SelectedStudent {
            get => selectedStudent;
            set => SetProperty(ref selectedStudent, value);
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
                LoadClassrooms();
            }
        }

        public string NewClassName
        {
            get => newClassName;
            set => SetProperty(ref newClassName, value);
        }

        public ICommand CreateClassroomCommand { get; }
        public ICommand DeleteClassroomCommand { get; }
        public ICommand ViewClassroomDetailsCommand { get; }
        public ICommand AddStudentToClassCommand { get; }
        public ICommand RemoveStudentFromClassCommand { get; }
        public ICommand AssignQuizToClassCommand { get; }
        public ICommand RefreshCommand { get; }

        public ClassroomManagementViewModel()
        {
            Classrooms = new ObservableCollection<ClassroomInfo>();
            AvailableStudents = new ObservableCollection<User>();
            
            CreateClassroomCommand = new RelayCommand(ExecuteCreateClassroom);
            DeleteClassroomCommand = new RelayCommand(ExecuteDeleteClassroom);
            ViewClassroomDetailsCommand = new RelayCommand(ExecuteViewClassroomDetails);
            AddStudentToClassCommand = new RelayCommand(ExecuteAddStudentToClass);
            RemoveStudentFromClassCommand = new RelayCommand(ExecuteRemoveStudentFromClass);
            AssignQuizToClassCommand = new RelayCommand(ExecuteAssignQuizToClass);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            LoadClassrooms();
        }

        private void LoadClassrooms()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var query = context.Classrooms
                        .Include(c => c.Enrollments)
                        .ThenInclude(e => e.Student)
                        .Where(c => c.TeacherId == currentUser.UserId);

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        query = query.Where(c => c.ClassName.Contains(SearchText));
                    }

                    var classroomsList = query.OrderByDescending(c => c.CreatedAt).ToList();
                    
                    Classrooms.Clear();
                    foreach (var classroom in classroomsList)
                    {
                        var classroomInfo = new ClassroomInfo
                        {
                            ClassId = classroom.ClassId,
                            ClassName = classroom.ClassName,
                            CreatedAt = classroom.CreatedAt ?? DateTime.MinValue,
                            StudentCount = classroom.Enrollments.Count,
                            Students = classroom.Enrollments.Select(e => e.Student.FullName).ToList()
                        };
                        Classrooms.Add(classroomInfo);
                    }

                    Message = $"Found {Classrooms.Count} classrooms";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading classrooms: {ex.Message}";
            }
        }

        private void LoadAvailableStudents()
        {
            if (SelectedClassroom == null) return;

            try
            {
                using (var context = new QuizardContext())
                {
                    // Lấy danh sách học sinh chưa tham gia lớp này
                    var enrolledStudentIds = context.Enrollments
                        .Where(e => e.ClassId == SelectedClassroom.ClassId)
                        .Select(e => e.StudentId)
                        .ToList();

                    var availableStudentsList = context.Users
                        .Where(u => u.Role == "student" && 
                                   u.IsActive == true && 
                                   !enrolledStudentIds.Contains(u.UserId))
                        .OrderBy(u => u.FullName)
                        .ToList();

                    AvailableStudents.Clear();
                    foreach (var student in availableStudentsList)
                    {
                        AvailableStudents.Add(student);
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading available students: {ex.Message}";
            }
        }

        private void ExecuteCreateClassroom(object? obj)
        {
            if (string.IsNullOrWhiteSpace(NewClassName))
            {
                Message = "Please enter classroom name";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var currentUser = CurrentUserService.Instance.CurrentUser;
                    
                    var newClassroom = new Classroom
                    {
                        ClassName = NewClassName,
                        TeacherId = currentUser.UserId,
                        CreatedAt = DateTime.Now
                    };

                    context.Classrooms.Add(newClassroom);
                    context.SaveChanges();

                    // Clear form
                    NewClassName = string.Empty;

                    LoadClassrooms();
                    Message = "Classroom created successfully";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error creating classroom: {ex.Message}";
            }
        }

        private void ExecuteDeleteClassroom(object? obj)
        {
            if (SelectedClassroom == null)
            {
                Message = "Please select a classroom to delete";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var classroom = context.Classrooms
                        .Include(c => c.Enrollments)
                        .Include(c => c.QuizAssignments)
                        .FirstOrDefault(c => c.ClassId == SelectedClassroom.ClassId);

                    if (classroom != null)
                    {
                        // Xóa quiz assignments
                        context.QuizAssignments.RemoveRange(classroom.QuizAssignments);
                        
                        // Xóa enrollments
                        context.Enrollments.RemoveRange(classroom.Enrollments);
                        
                        // Xóa classroom
                        context.Classrooms.Remove(classroom);
                        context.SaveChanges();

                        LoadClassrooms();
                        Message = "Classroom deleted successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error deleting classroom: {ex.Message}";
            }
        }

        private void ExecuteViewClassroomDetails(object? obj)
        {
            if (SelectedClassroom == null)
            {
                Message = "Please select a classroom to view details";
                return;
            }

            try
            {
                var detailsPage = new Views.ClassroomDetailsView(SelectedClassroom.ClassId);
                AppNavigationService.Instance.Navigate(detailsPage);
            }
            catch (Exception ex)
            {
                Message = $"Error viewing classroom details: {ex.Message}";
            }
        }

        private void ExecuteAddStudentToClass(object? obj)
        {
            if (SelectedClassroom == null)
            {
                Message = "Please select a classroom";
                return;
            }

            if (SelectedStudent == null)
            {
                Message = "Please select a student to add";
                return;
            }

            try
            {
                using (var context = new QuizardContext())
                {
                    var enrollment = new Enrollment
                    {
                        ClassId = SelectedClassroom.ClassId,
                        StudentId = SelectedStudent.UserId,
                        EnrolledAt = DateTime.Now
                    };

                    context.Enrollments.Add(enrollment);
                    context.SaveChanges();

                    LoadClassrooms();
                    LoadAvailableStudents();
                    Message = $"Student {SelectedStudent.FullName} added to classroom successfully";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error adding student to classroom: {ex.Message}";
            }
        }

        private void ExecuteRemoveStudentFromClass(object? obj)
        {
            if (SelectedClassroom == null)
            {
                Message = "Please select a classroom";
                return;
            }

            try
            {
                var removeStudentPage = new Views.RemoveStudentView(SelectedClassroom.ClassId);
                AppNavigationService.Instance.Navigate(removeStudentPage);
            }
            catch (Exception ex)
            {
                Message = $"Error removing student from classroom: {ex.Message}";
            }
        }

        private void ExecuteAssignQuizToClass(object? obj)
        {
            if (SelectedClassroom == null)
            {
                Message = "Please select a classroom to assign quiz";
                return;
            }

            try
            {
                var assignQuizPage = new Views.AssignQuizView(SelectedClassroom.ClassId);
                AppNavigationService.Instance.Navigate(assignQuizPage);
            }
            catch (Exception ex)
            {
                Message = $"Error assigning quiz to classroom: {ex.Message}";
            }
        }

        private void ExecuteRefresh(object? obj)
        {
            LoadClassrooms();
        }
    }

    public class ClassroomInfo
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int StudentCount { get; set; }
        public List<string> Students { get; set; } = new();
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string StudentSummary => $"{StudentCount} student(s)";
        public string StudentsDisplay => Students != null && Students.Any() 
            ? string.Join(", ", Students.Take(3)) + (Students.Count > 3 ? "..." : "")
            : "No students";
    }
}