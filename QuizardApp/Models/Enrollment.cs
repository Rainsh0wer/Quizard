using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int ClassId { get; set; }

    public int StudentId { get; set; }

    public DateTime? EnrolledAt { get; set; }

    public virtual Classroom Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
