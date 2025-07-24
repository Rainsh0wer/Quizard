using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;

namespace QuizardApp.ViewModels
{
    public class SubjectSearchResult : BaseViewModel
    {
        public Subject Subject { get; set; } = null!;
        public ObservableCollection<Quiz> RelatedQuizzes { get; set; } = new();
        public int QuizCount => RelatedQuizzes.Count;
        public string QuizCountText => $"{QuizCount} quiz{(QuizCount != 1 ? "es" : "")}";
    }

    public class SearchSubjectsViewModel : BaseViewModel
    {
        private readonly MainCursorViewModel _mainCursor;
        private readonly CurrentUserService _currentUserService;
        private string _searchText = string.Empty;
        private bool _isLoading = false;
        private bool _hasSearched = false;
        private SubjectSearchResult? _selectedSubject;

        public SearchSubjectsViewModel(MainCursorViewModel mainCursor, CurrentUserService currentUserService)
        {
            _mainCursor = mainCursor;
            _currentUserService = currentUserService;
            
            // Initialize collections
            SearchResults = new ObservableCollection<SubjectSearchResult>();
            PopularSubjects = new ObservableCollection<SubjectSearchResult>();
            
            // Initialize commands
            SearchCommand = new RelayCommand(async () => await SearchAsync(), CanSearch);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            SelectSubjectCommand = new RelayCommand<SubjectSearchResult>(SelectSubject);
            TakeQuizCommand = new RelayCommand<Quiz>(TakeQuiz);
            BackToDashboardCommand = new RelayCommand(() => _mainCursor.NavigateTo(
                _currentUserService.IsStudent ? AppState.StudentDashboard : AppState.TeacherDashboard));
            
            // Load popular subjects initially
            _ = LoadPopularSubjectsAsync();
        }

        public ObservableCollection<SubjectSearchResult> SearchResults { get; }
        public ObservableCollection<SubjectSearchResult> PopularSubjects { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
                
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
                OnPropertyChanged(nameof(ShowPopularSubjects));
            }
        }

        public SubjectSearchResult? SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSubjectSelected));
            }
        }

        public bool ShowSearchResults => HasSearched && SearchResults.Count > 0;
        public bool ShowPopularSubjects => !HasSearched && PopularSubjects.Count > 0;
        public bool IsSubjectSelected => SelectedSubject != null;
        public int TotalSearchResults => SearchResults.Count;
        public string SearchResultsText => 
            TotalSearchResults == 0 ? "No subjects found" :
            TotalSearchResults == 1 ? "1 subject found" :
            $"{TotalSearchResults} subjects found";

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand SelectSubjectCommand { get; }
        public ICommand TakeQuizCommand { get; }
        public ICommand BackToDashboardCommand { get; }

        private async Task LoadPopularSubjectsAsync()
        {
            try
            {
                IsLoading = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Get subjects with the most quizzes
                    var popularSubjects = context.Subjects
                        .Select(s => new
                        {
                            Subject = s,
                            QuizCount = context.Quizzes.Count(q => q.SubjectId == s.SubjectId)
                        })
                        .Where(x => x.QuizCount > 0)
                        .OrderByDescending(x => x.QuizCount)
                        .Take(10)
                        .ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        PopularSubjects.Clear();
                        foreach (var item in popularSubjects)
                        {
                            var subjectResult = new SubjectSearchResult
                            {
                                Subject = item.Subject
                            };

                            // Load related quizzes
                            var quizzes = context.Quizzes
                                .Where(q => q.SubjectId == item.Subject.SubjectId)
                                .Take(5)
                                .ToList();

                            foreach (var quiz in quizzes)
                                subjectResult.RelatedQuizzes.Add(quiz);

                            PopularSubjects.Add(subjectResult);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error loading popular subjects: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSearch() => !IsLoading && !string.IsNullOrWhiteSpace(SearchText);

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            try
            {
                IsLoading = true;
                HasSearched = true;

                await Task.Run(() =>
                {
                    using var context = new QuizardContext();
                    
                    // Search subjects by name (case-insensitive)
                    var searchResults = context.Subjects
                        .Where(s => s.Name.ToLower().Contains(SearchText.ToLower()))
                        .OrderBy(s => s.Name)
                        .ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SearchResults.Clear();
                        foreach (var subject in searchResults)
                        {
                            var subjectResult = new SubjectSearchResult
                            {
                                Subject = subject
                            };

                            // Load related quizzes
                            var quizzes = context.Quizzes
                                .Where(q => q.SubjectId == subject.SubjectId)
                                .OrderByDescending(q => q.CreatedAt)
                                .ToList();

                            foreach (var quiz in quizzes)
                                subjectResult.RelatedQuizzes.Add(quiz);

                            SearchResults.Add(subjectResult);
                        }

                        OnPropertyChanged(nameof(TotalSearchResults));
                        OnPropertyChanged(nameof(SearchResultsText));
                    });
                });
            }
            catch (Exception ex)
            {
                _mainCursor.StatusMessage = $"Error searching subjects: {ex.Message}";
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
            
            // Check if search text is still the same (user hasn't typed more)
            var currentSearchText = SearchText;
            await Task.Delay(200);
            
            if (currentSearchText == SearchText && !string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchAsync();
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            HasSearched = false;
            SelectedSubject = null;
            SearchResults.Clear();
        }

        private void SelectSubject(SubjectSearchResult? subjectResult)
        {
            SelectedSubject = subjectResult;
        }

        private void TakeQuiz(Quiz? quiz)
        {
            if (quiz != null && _currentUserService.IsStudent)
            {
                // Navigate to take quiz (could pass quiz ID through some mechanism)
                _mainCursor.NavigateTo(AppState.TakeQuiz);
            }
        }

        // Quick search methods
        public async Task QuickSearchAsync(string searchTerm)
        {
            SearchText = searchTerm;
            await SearchAsync();
        }

        public void ViewQuizDetails(Quiz quiz)
        {
            // Navigate to quiz details
            _mainCursor.NavigateTo(AppState.QuizDetails);
        }

        public void FilterByDifficulty(string difficulty)
        {
            // Future enhancement: filter by difficulty
        }

        public void SortResults(string sortBy)
        {
            // Future enhancement: sort results by name, popularity, etc.
            var results = SearchResults.ToList();
            
            switch (sortBy.ToLower())
            {
                case "name":
                    results = results.OrderBy(r => r.Subject.Name).ToList();
                    break;
                case "quizcount":
                    results = results.OrderByDescending(r => r.QuizCount).ToList();
                    break;
            }

            SearchResults.Clear();
            foreach (var result in results)
                SearchResults.Add(result);
        }
    }
}