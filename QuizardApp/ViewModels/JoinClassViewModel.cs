using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class ClassSearchResult : BaseViewModel
    {
        public Classroom Classroom { get; set; } = null!;
        public bool IsAlreadyEnrolled { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string StudentCountText => $"{StudentCount} student{(StudentCount != 1 ? "s" : "")}";
        public string EnrollmentStatusText => IsAlreadyEnrolled ? "Already Enrolled" : "Join Class";
    }

    public class JoinClassViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private string _classCode = string.Empty;
        private string _searchText = string.Empty;
        private bool _isLoading = false;
        private bool _hasSearched = false;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public JoinClassViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            AvailableClasses = new ObservableCollection<ClassSearchResult>();
            MyClasses = new ObservableCollection<Classroom>();
            
            // Initialize commands
            JoinByCodeCommand = new RelayCommand(async () => await JoinByCodeAsync(), CanJoinByCode);
            SearchClassesCommand = new RelayCommand(async () => await SearchClassesAsync(), CanSearchClasses);
            JoinClassCommand = new RelayCommand<ClassSearchResult>(async (c) => await JoinClassAsync(c));
            LeaveClassCommand = new RelayCommand<Classroom>(async (c) => await LeaveClassAsync(c));
            BackToDashboardCommand = new RelayCommand(() => _mainCursor.NavigateTo(AppState.StudentDashboard));
            ClearSearchCommand = new RelayCommand(ClearSearch);
            
            // Load current enrollments
            _ = LoadMyClassesAsync();
        }

        public ObservableCollection<ClassSearchResult> AvailableClasses { get; }
        public ObservableCollection<Classroom> MyClasses { get; }

        public string ClassCode
        {
            get => _classCode;
            set
            {
                _classCode = value?.Trim().ToUpper() ?? string.Empty;
                OnPropertyChanged();
                ((RelayCommand)JoinByCodeCommand).RaiseCanExecuteChanged();
                ClearMessages();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ((RelayCommand)SearchClassesCommand).RaiseCanExecuteChanged();
                ClearMessages();
                
                // Auto-search after user stops typing
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _ = DelayedSearchAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                _hasSearched = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowSearchResults));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuccessMessage));
            }
        }

        public bool ShowSearchResults => HasSearched && AvailableClasses.Count > 0;
        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public bool HasSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);
        public int TotalSearchResults => AvailableClasses.Count;
        public int MyClassesCount => MyClasses.Count;
        public string SearchResultsText => 
            TotalSearchResults == 0 ? "No classes found" :
            TotalSearchResults == 1 ? "1 class found" :
            $"{TotalSearchResults} classes found";

        // Commands
        public ICommand JoinByCodeCommand { get; }
        public ICommand SearchClassesCommand { get; }
        public ICommand JoinClassCommand { get; }
        public ICommand LeaveClassCommand { get; }
        public ICommand BackToDashboardCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private async Task LoadMyClassesAsync()
        {
            try
            {
                IsLoading = true;
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var enrolledClasses = context.Enrollments
                        .Where(e => e.StudentId == currentUserId.Value)
                        .Select(e => e.Class)
                        .ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MyClasses.Clear();
                        foreach (var classroom in enrolledClasses)
                            MyClasses.Add(classroom);

                        OnPropertyChanged(nameof(MyClassesCount));
                    });
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading classes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanJoinByCode() => !IsLoading && !string.IsNullOrWhiteSpace(ClassCode);

        private async Task JoinByCodeAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Find class by code (assuming ClassCode is stored somewhere, or we can use ClassId)
                    // For now, let's assume we're using ClassId as the code
                    if (!int.TryParse(ClassCode, out int classId))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Invalid class code format. Please enter a valid class ID.";
                        });
                        return;
                    }

                    var classroom = context.Classrooms.Find(classId);
                    if (classroom == null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Class not found. Please check the class code and try again.";
                        });
                        return;
                    }

                    // Check if already enrolled
                    var existingEnrollment = context.Enrollments
                        .FirstOrDefault(e => e.ClassId == classId && e.StudentId == currentUserId.Value);

                    if (existingEnrollment != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "You are already enrolled in this class.";
                        });
                        return;
                    }

                    // Create enrollment
                    var enrollment = new Enrollment
                    {
                        ClassId = classId,
                        StudentId = currentUserId.Value,
                        EnrolledAt = DateTime.Now
                    };

                    context.Enrollments.Add(enrollment);
                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SuccessMessage = $"Successfully joined '{classroom.ClassName}'!";
                        ClassCode = string.Empty;
                    });
                });

                // Refresh my classes
                await LoadMyClassesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error joining class: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSearchClasses() => !IsLoading && !string.IsNullOrWhiteSpace(SearchText);

        private async Task SearchClassesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            try
            {
                IsLoading = true;
                HasSearched = true;
                ClearMessages();
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Search classes by name
                    var searchResults = context.Classrooms
                        .Where(c => c.ClassName.ToLower().Contains(SearchText.ToLower()))
                        .ToList();

                    // Get current user's enrollments
                    var enrolledClassIds = context.Enrollments
                        .Where(e => e.StudentId == currentUserId.Value)
                        .Select(e => e.ClassId)
                        .ToHashSet();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableClasses.Clear();
                        foreach (var classroom in searchResults)
                        {
                            var teacherName = context.Users
                                .Where(u => u.UserId == classroom.TeacherId)
                                .Select(u => u.FullName ?? u.Username)
                                .FirstOrDefault() ?? "Unknown";

                            var studentCount = context.Enrollments
                                .Count(e => e.ClassId == classroom.ClassId);

                            var classResult = new ClassSearchResult
                            {
                                Classroom = classroom,
                                IsAlreadyEnrolled = enrolledClassIds.Contains(classroom.ClassId),
                                TeacherName = teacherName,
                                StudentCount = studentCount
                            };

                            AvailableClasses.Add(classResult);
                        }

                        OnPropertyChanged(nameof(TotalSearchResults));
                        OnPropertyChanged(nameof(SearchResultsText));
                    });
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error searching classes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DelayedSearchAsync()
        {
            // Wait for user to stop typing
            await Task.Delay(500);
            
            // Check if search text is still the same
            var currentSearchText = SearchText;
            await Task.Delay(200);
            
            if (currentSearchText == SearchText && !string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchClassesAsync();
            }
        }

        private async Task JoinClassAsync(ClassSearchResult? classResult)
        {
            if (classResult == null || classResult.IsAlreadyEnrolled) return;

            try
            {
                IsLoading = true;
                ClearMessages();
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Check if already enrolled (double-check)
                    var existingEnrollment = context.Enrollments
                        .FirstOrDefault(e => e.ClassId == classResult.Classroom.ClassId && e.StudentId == currentUserId.Value);

                    if (existingEnrollment != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "You are already enrolled in this class.";
                        });
                        return;
                    }

                    // Create enrollment
                    var enrollment = new Enrollment
                    {
                        ClassId = classResult.Classroom.ClassId,
                        StudentId = currentUserId.Value,
                        EnrolledAt = DateTime.Now
                    };

                    context.Enrollments.Add(enrollment);
                    context.SaveChanges();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SuccessMessage = $"Successfully joined '{classResult.Classroom.ClassName}'!";
                        classResult.IsAlreadyEnrolled = true;
                        OnPropertyChanged(nameof(classResult.EnrollmentStatusText));
                    });
                });

                // Refresh my classes
                await LoadMyClassesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error joining class: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LeaveClassAsync(Classroom? classroom)
        {
            if (classroom == null) return;

            try
            {
                IsLoading = true;
                ClearMessages();
                var currentUserId = _currentUserService.GetCurrentUserId();
                
                if (!currentUserId.HasValue) return;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    var enrollment = context.Enrollments
                        .FirstOrDefault(e => e.ClassId == classroom.ClassId && e.StudentId == currentUserId.Value);

                    if (enrollment != null)
                    {
                        context.Enrollments.Remove(enrollment);
                        context.SaveChanges();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            SuccessMessage = $"Successfully left '{classroom.ClassName}'.";
                        });
                    }
                });

                // Refresh my classes and search results
                await LoadMyClassesAsync();
                if (HasSearched)
                {
                    await SearchClassesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error leaving class: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            HasSearched = false;
            AvailableClasses.Clear();
            ClearMessages();
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        // Quick actions
        public async Task QuickJoinAsync(string classCode)
        {
            ClassCode = classCode;
            await JoinByCodeAsync();
        }

        public void ViewClassDetails(Classroom classroom)
        {
            // Navigate to class details view
            _mainCursor.NavigateTo(AppState.ViewClasses);
        }
    }
}