using QuizardApp.Models;

namespace QuizardApp.Services
{
    public class CurrentUserService
    {
        private User? _currentUser;

        public User? CurrentUser => _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public bool IsStudent => _currentUser?.Role == "Student";

        public bool IsTeacher => _currentUser?.Role == "Teacher";

        public void SetCurrentUser(User? user)
        {
            _currentUser = user;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public int? GetCurrentUserId()
        {
            return _currentUser?.UserId;
        }

        public string? GetCurrentUserRole()
        {
            return _currentUser?.Role;
        }

        public string? GetCurrentUserName()
        {
            return _currentUser?.FullName ?? _currentUser?.Username;
        }
    }
}