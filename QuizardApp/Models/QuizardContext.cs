using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace QuizardApp.Models;

public partial class QuizardContext : DbContext
{
    public QuizardContext()
    {
    }

    public QuizardContext(DbContextOptions<QuizardContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Classroom> Classrooms { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAssignment> QuizAssignments { get; set; }

    public virtual DbSet<QuizLike> QuizLikes { get; set; }

    public virtual DbSet<SavedQuiz> SavedQuizzes { get; set; }

    public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

    public virtual DbSet<StudentQuiz> StudentQuizzes { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("DB"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classroo__CB1927A057138C76");

            entity.ToTable("Classroom");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classroom__Teach__48CFD27E");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F6877FB36ACE50F");

            entity.ToTable("Enrollment");

            entity.HasIndex(e => new { e.ClassId, e.StudentId }, "UQ__Enrollme__483575064318FB46").IsUnique();

            entity.Property(e => e.EnrollmentId).HasColumnName("EnrollmentID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Class__4D94879B");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Stude__4E88ABD4");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF68CDED774");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__QuizID__60A75C0F");

            entity.HasOne(d => d.Student).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Studen__5FB337D6");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8C11F66B8C");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CorrectOption)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Question__QuizID__35BCFE0A");

            entity.HasMany(d => d.Tags).WithMany(p => p.Questions)
                .UsingEntity<Dictionary<string, object>>(
                    "QuestionTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__QuestionT__TagID__5AEE82B9"),
                    l => l.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__QuestionT__Quest__59FA5E80"),
                    j =>
                    {
                        j.HasKey("QuestionId", "TagId").HasName("PK__Question__DB97A02854C2CCE5");
                        j.ToTable("QuestionTag");
                        j.IndexerProperty<int>("QuestionId").HasColumnName("QuestionID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                    });
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Question__92C7A1DF317E52BF");

            entity.ToTable("QuestionOption");

            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.OptionLabel)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuestionO__Quest__398D8EEE");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quiz__8B42AE6E92FA22A6");

            entity.ToTable("Quiz");

            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quiz__CreatedBy__30F848ED");

            entity.HasOne(d => d.Subject).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quiz__SubjectID__300424B4");
        });

        modelBuilder.Entity<QuizAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__QuizAssi__32499E57C786F41F");

            entity.ToTable("QuizAssignment");

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Class).WithMany(p => p.QuizAssignments)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__QuizAssig__Class__534D60F1");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAssignments)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizAssig__QuizI__52593CB8");

            entity.HasOne(d => d.Student).WithMany(p => p.QuizAssignments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__QuizAssig__Stude__5441852A");
        });

        modelBuilder.Entity<QuizLike>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__QuizLike__A2922CF44F0E48BF");

            entity.ToTable("QuizLike");

            entity.HasIndex(e => new { e.UserId, e.QuizId }, "UQ__QuizLike__EF3CE64B49003B61").IsUnique();

            entity.Property(e => e.LikeId).HasColumnName("LikeID");
            entity.Property(e => e.LikedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizLikes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizLike__QuizID__66603565");

            entity.HasOne(d => d.User).WithMany(p => p.QuizLikes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizLike__UserID__656C112C");
        });

        modelBuilder.Entity<SavedQuiz>(entity =>
        {
            entity.HasKey(e => e.SavedId).HasName("PK__SavedQui__0B058FFCAAA5A9E7");

            entity.ToTable("SavedQuiz");

            entity.HasIndex(e => new { e.StudentId, e.QuizId }, "UQ__SavedQui__CA71009EDE59FCA8").IsUnique();

            entity.Property(e => e.SavedId).HasColumnName("SavedID");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.SavedQuizzes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedQuiz__QuizI__6C190EBB");

            entity.HasOne(d => d.Student).WithMany(p => p.SavedQuizzes)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedQuiz__Stude__6B24EA82");
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__StudentA__D48250241E92C21D");

            entity.ToTable("StudentAnswer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.SelectedOption)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StudentQuizId).HasColumnName("StudentQuizID");

            entity.HasOne(d => d.Question).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAn__Quest__44FF419A");

            entity.HasOne(d => d.StudentQuiz).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.StudentQuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAn__Stude__440B1D61");
        });

        modelBuilder.Entity<StudentQuiz>(entity =>
        {
            entity.HasKey(e => e.StudentQuizId).HasName("PK__StudentQ__0C414D8A755955BE");

            entity.ToTable("StudentQuiz");

            entity.Property(e => e.StudentQuizId).HasColumnName("StudentQuizID");
            entity.Property(e => e.FinishedAt).HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.StudentQuizzes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentQu__QuizI__3F466844");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentQuizzes)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentQu__Stude__3E52440B");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subject__AC1BA388DBFD7EED");

            entity.ToTable("Subject");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tag__657CFA4C7CB1D48A");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "UQ__Tag__737584F680A549DD").IsUnique();

            entity.Property(e => e.TagId).HasColumnName("TagID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC572AE7CC");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E44A25D33F").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534CE117085").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
