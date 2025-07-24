using System;
using System.Linq;
using QuizardApp.Models;

namespace QuizardApp.TestData
{
    public static class SampleDataCreator
    {
        public static void CreateSampleData()
        {
            try
            {
                using var context = new QuizardContext();
                
                // Check if data already exists
                if (context.Users.Any()) return;

                // Create sample users
                var teacher = new User
                {
                    Username = "teacher1",
                    Email = "teacher@quizard.com",
                    PasswordHash = "password123", // In production, use proper hashing
                    FullName = "John Teacher",
                    Role = "teacher",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var student = new User
                {
                    Username = "student1",
                    Email = "student@quizard.com",
                    PasswordHash = "password123", // In production, use proper hashing
                    FullName = "Jane Student",
                    Role = "student",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                context.Users.AddRange(teacher, student);
                context.SaveChanges();

                // Create sample subjects
                var mathSubject = new Subject
                {
                    Name = "Mathematics",
                    Description = "Math quizzes and exercises",
                    CreatedAt = DateTime.Now
                };

                var scienceSubject = new Subject
                {
                    Name = "Science",
                    Description = "Science quizzes and experiments",
                    CreatedAt = DateTime.Now
                };

                context.Subjects.AddRange(mathSubject, scienceSubject);
                context.SaveChanges();

                // Create sample quiz
                var quiz = new Quiz
                {
                    SubjectId = mathSubject.SubjectId,
                    Title = "Basic Arithmetic",
                    Description = "Simple addition and subtraction",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                };

                context.Quizzes.Add(quiz);
                context.SaveChanges();

                // Create sample questions
                var question1 = new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "What is 2 + 2?",
                    CorrectOption = 'A',
                    Explanation = "2 + 2 = 4",
                    CreatedAt = DateTime.Now
                };

                context.Questions.Add(question1);
                context.SaveChanges();

                // Create sample question options
                var options = new[]
                {
                    new QuestionOption { QuestionId = question1.QuestionId, OptionLabel = 'A', Content = "4" },
                    new QuestionOption { QuestionId = question1.QuestionId, OptionLabel = 'B', Content = "3" },
                    new QuestionOption { QuestionId = question1.QuestionId, OptionLabel = 'C', Content = "5" },
                    new QuestionOption { QuestionId = question1.QuestionId, OptionLabel = 'D', Content = "6" }
                };

                context.QuestionOptions.AddRange(options);
                context.SaveChanges();

                // Create sample classroom
                var classroom = new Classroom
                {
                    ClassName = "Math Class 2024",
                    TeacherId = teacher.UserId,
                    CreatedAt = DateTime.Now
                };

                context.Classrooms.Add(classroom);
                context.SaveChanges();

                // Enroll student in classroom
                var enrollment = new Enrollment
                {
                    ClassId = classroom.ClassId,
                    StudentId = student.UserId,
                    EnrolledAt = DateTime.Now
                };

                context.Enrollments.Add(enrollment);
                context.SaveChanges();

                System.Diagnostics.Debug.WriteLine("Sample data created successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating sample data: {ex.Message}");
            }
        }
    }
}