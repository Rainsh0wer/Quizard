using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int StudentId { get; set; }

    public int QuizId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
