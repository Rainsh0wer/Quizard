using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class StudentQuiz
{
    public int StudentQuizId { get; set; }

    public int StudentId { get; set; }

    public int QuizId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeSpan? TimeSpent { get; set; }

    public double? Score { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
