using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class QuestionOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionLabel { get; set; } = null!;

    public string Content { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
