using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();

    public virtual ICollection<QuizLike> QuizLikes { get; set; } = new List<QuizLike>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<SavedQuiz> SavedQuizzes { get; set; } = new List<SavedQuiz>();

    public virtual ICollection<StudentQuiz> StudentQuizzes { get; set; } = new List<StudentQuiz>();
}
