using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int SubjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsPublic { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();

    public virtual ICollection<QuizLike> QuizLikes { get; set; } = new List<QuizLike>();

    public virtual ICollection<SavedQuiz> SavedQuizzes { get; set; } = new List<SavedQuiz>();

    public virtual ICollection<StudentQuiz> StudentQuizzes { get; set; } = new List<StudentQuiz>();

    public virtual Subject Subject { get; set; } = null!;
}
