using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class SavedQuiz
{
    public int SavedId { get; set; }

    public int StudentId { get; set; }

    public int QuizId { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
