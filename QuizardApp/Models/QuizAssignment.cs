using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class QuizAssignment
{
    public int AssignmentId { get; set; }

    public int QuizId { get; set; }

    public int? ClassId { get; set; }

    public int? StudentId { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Classroom? Class { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User? Student { get; set; }
}
