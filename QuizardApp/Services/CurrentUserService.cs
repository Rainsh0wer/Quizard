using QuizardApp.Models;

namespace QuizardApp.Services
{
    public static class CurrentUserService
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static void Clear()
        {
            UserId = 0;
            Username = null;
            Role = null;
        }
    }
}