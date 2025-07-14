using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
