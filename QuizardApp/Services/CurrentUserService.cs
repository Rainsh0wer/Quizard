using QuizardApp.Models;

namespace QuizardApp.Services
{
    public class CurrentUserService
    {
        private static CurrentUserService _instance;
        public static CurrentUserService Instance => _instance ??= new CurrentUserService();

        public User CurrentUser { get; private set; }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn => CurrentUser != null;

        public bool IsTeacher => CurrentUser?.Role == "teacher";

        public bool IsStudent => CurrentUser?.Role == "student";
    }
}