using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Classroom
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int TeacherId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();

    public virtual User Teacher { get; set; } = null!;
}
