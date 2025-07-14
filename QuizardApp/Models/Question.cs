using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string Content { get; set; } = null!;

    public string CorrectOption { get; set; } = null!;

    public string? Explanation { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
