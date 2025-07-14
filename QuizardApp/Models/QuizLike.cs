using System;
using System.Collections.Generic;

namespace QuizardApp.Models;

public partial class QuizLike
{
    public int LikeId { get; set; }

    public int UserId { get; set; }

    public int QuizId { get; set; }

    public DateTime? LikedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
