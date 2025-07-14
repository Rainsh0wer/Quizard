using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class StudentAnswer
{
    public int AnswerId { get; set; }

    public int StudentQuizId { get; set; }

    public int QuestionId { get; set; }

    public string? SelectedOption { get; set; }

    public bool? IsCorrect { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual StudentQuiz StudentQuiz { get; set; } = null!;
}
