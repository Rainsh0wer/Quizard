using System;
using System.Linq;
using QuizardApp.Models;

namespace QuizardApp.TestData
{
    public static class ComprehensiveDataCreator
    {
        public static void CreateComprehensiveData()
        {
            try
            {
                using var context = new QuizardContext();
                
                // Clear existing data if needed (optional)
                // context.Database.EnsureDeleted();
                // context.Database.EnsureCreated();

                // Create Users first
                CreateUsers(context);
                
                // Create Subjects (12 subjects)
                CreateSubjects(context);
                
                // Create comprehensive quizzes for each subject
                CreateQuizzesAndQuestions(context);
                
                // Create classrooms and enrollments
                CreateClassroomsAndEnrollments(context);

                System.Diagnostics.Debug.WriteLine("Comprehensive data created successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating comprehensive data: {ex.Message}");
                throw;
            }
        }

        private static void CreateUsers(QuizardContext context)
        {
            if (context.Users.Any()) return;

            var users = new[]
            {
                // Teachers
                new User { Username = "teacher_math", Email = "math.teacher@quizard.com", PasswordHash = "123", FullName = "Nguyễn Văn Toán", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "teacher_physics", Email = "physics.teacher@quizard.com", PasswordHash = "123", FullName = "Trần Thị Lý", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "teacher_chemistry", Email = "chemistry.teacher@quizard.com", PasswordHash = "123", FullName = "Lê Văn Hóa", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "teacher_biology", Email = "biology.teacher@quizard.com", PasswordHash = "123", FullName = "Phạm Thị Sinh", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "teacher_english", Email = "english.teacher@quizard.com", PasswordHash = "123", FullName = "Johnson Smith", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "teacher_literature", Email = "literature.teacher@quizard.com", PasswordHash = "123", FullName = "Vũ Thị Văn", Role = "teacher", IsActive = true, CreatedAt = DateTime.Now },
                
                // Students
                new User { Username = "student1", Email = "student1@quizard.com", PasswordHash = "123", FullName = "Nguyễn Văn An", Role = "student", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "student2", Email = "student2@quizard.com", PasswordHash = "123", FullName = "Trần Thị Bình", Role = "student", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "student3", Email = "student3@quizard.com", PasswordHash = "123", FullName = "Lê Văn Cường", Role = "student", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "student4", Email = "student4@quizard.com", PasswordHash = "123", FullName = "Phạm Thị Dung", Role = "student", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "student5", Email = "student5@quizard.com", PasswordHash = "123", FullName = "Hoàng Văn Em", Role = "student", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "admin", Email = "admin@quizard.com", PasswordHash = "123", FullName = "Administrator", Role = "admin", IsActive = true, CreatedAt = DateTime.Now }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void CreateSubjects(QuizardContext context)
        {
            if (context.Subjects.Any()) return;

            var subjects = new[]
            {
                new Subject { Name = "Toán học", Description = "Môn Toán học từ cơ bản đến nâng cao", CreatedAt = DateTime.Now },
                new Subject { Name = "Vật lý", Description = "Môn Vật lý và các hiện tượng tự nhiên", CreatedAt = DateTime.Now },
                new Subject { Name = "Hóa học", Description = "Môn Hóa học và phản ứng hóa học", CreatedAt = DateTime.Now },
                new Subject { Name = "Sinh học", Description = "Môn Sinh học và sự sống", CreatedAt = DateTime.Now },
                new Subject { Name = "Tiếng Anh", Description = "Môn Tiếng Anh giao tiếp và ngữ pháp", CreatedAt = DateTime.Now },
                new Subject { Name = "Ngữ văn", Description = "Môn Ngữ văn và văn học Việt Nam", CreatedAt = DateTime.Now },
                new Subject { Name = "Lịch sử", Description = "Môn Lịch sử Việt Nam và thế giới", CreatedAt = DateTime.Now },
                new Subject { Name = "Địa lý", Description = "Môn Địa lý tự nhiên và kinh tế", CreatedAt = DateTime.Now },
                new Subject { Name = "Tin học", Description = "Môn Tin học và công nghệ thông tin", CreatedAt = DateTime.Now },
                new Subject { Name = "Giáo dục công dân", Description = "Môn Giáo dục công dân và đạo đức", CreatedAt = DateTime.Now },
                new Subject { Name = "Thể dục", Description = "Môn Thể dục và thể thao", CreatedAt = DateTime.Now },
                new Subject { Name = "Âm nhạc", Description = "Môn Âm nhạc và nghệ thuật", CreatedAt = DateTime.Now }
            };

            context.Subjects.AddRange(subjects);
            context.SaveChanges();
        }

        private static void CreateQuizzesAndQuestions(QuizardContext context)
        {
            if (context.Quizzes.Any()) return;

            var subjects = context.Subjects.ToList();
            var teachers = context.Users.Where(u => u.Role == "teacher").ToList();

            // Toán học quizzes
            CreateMathQuizzes(context, subjects.First(s => s.Name == "Toán học"), teachers[0]);
            
            // Vật lý quizzes
            CreatePhysicsQuizzes(context, subjects.First(s => s.Name == "Vật lý"), teachers[1]);
            
            // Hóa học quizzes
            CreateChemistryQuizzes(context, subjects.First(s => s.Name == "Hóa học"), teachers[2]);
            
            // Sinh học quizzes
            CreateBiologyQuizzes(context, subjects.First(s => s.Name == "Sinh học"), teachers[3]);
            
            // Tiếng Anh quizzes
            CreateEnglishQuizzes(context, subjects.First(s => s.Name == "Tiếng Anh"), teachers[4]);
            
            // Ngữ văn quizzes
            CreateLiteratureQuizzes(context, subjects.First(s => s.Name == "Ngữ văn"), teachers[5]);
            
            // Các môn khác
            CreateOtherSubjectQuizzes(context, subjects, teachers);
        }

        private static void CreateMathQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quizzes = new[]
            {
                new Quiz
                {
                    SubjectId = subject.SubjectId,
                    Title = "Đại số cơ bản",
                    Description = "Kiểm tra kiến thức đại số cơ bản",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                },
                new Quiz
                {
                    SubjectId = subject.SubjectId,
                    Title = "Hình học phẳng",
                    Description = "Bài kiểm tra về hình học phẳng",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                },
                new Quiz
                {
                    SubjectId = subject.SubjectId,
                    Title = "Lượng giác",
                    Description = "Các công thức lượng giác cơ bản",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                }
            };

            context.Quizzes.AddRange(quizzes);
            context.SaveChanges();

            // Create questions for math quizzes
            var algebraQuiz = quizzes[0];
            CreateMathQuestions(context, algebraQuiz);
        }

        private static void CreateMathQuestions(QuizardContext context, Quiz quiz)
        {
            var questions = new[]
            {
                new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "Giải phương trình: 2x + 5 = 13",
                    CorrectOption = 'B',
                    Explanation = "2x = 13 - 5 = 8, nên x = 4",
                    CreatedAt = DateTime.Now
                },
                new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "Tính đạo hàm của hàm số f(x) = x² + 3x",
                    CorrectOption = 'A',
                    Explanation = "f'(x) = 2x + 3",
                    CreatedAt = DateTime.Now
                },
                new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "Căn bậc hai của 64 là:",
                    CorrectOption = 'C',
                    Explanation = "√64 = 8",
                    CreatedAt = DateTime.Now
                }
            };

            context.Questions.AddRange(questions);
            context.SaveChanges();

            // Create options for each question
            CreateQuestionOptions(context, questions[0], new[] { "x = 3", "x = 4", "x = 5", "x = 6" });
            CreateQuestionOptions(context, questions[1], new[] { "2x + 3", "x + 3", "2x + 6", "x² + 3" });
            CreateQuestionOptions(context, questions[2], new[] { "6", "7", "8", "9" });
        }

        private static void CreatePhysicsQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quizzes = new[]
            {
                new Quiz
                {
                    SubjectId = subject.SubjectId,
                    Title = "Cơ học Newton",
                    Description = "Các định luật Newton và ứng dụng",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                },
                new Quiz
                {
                    SubjectId = subject.SubjectId,
                    Title = "Điện học cơ bản",
                    Description = "Điện áp, điện trở, định luật Ohm",
                    CreatedBy = teacher.UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                }
            };

            context.Quizzes.AddRange(quizzes);
            context.SaveChanges();

            CreatePhysicsQuestions(context, quizzes[0]);
        }

        private static void CreatePhysicsQuestions(QuizardContext context, Quiz quiz)
        {
            var questions = new[]
            {
                new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "Định luật Newton thứ nhất còn được gọi là:",
                    CorrectOption = 'A',
                    Explanation = "Định luật Newton thứ nhất còn gọi là định luật quán tính",
                    CreatedAt = DateTime.Now
                },
                new Question
                {
                    QuizId = quiz.QuizId,
                    Content = "Công thức tính lực F theo định luật Newton thứ hai là:",
                    CorrectOption = 'B',
                    Explanation = "F = ma (lực = khối lượng × gia tốc)",
                    CreatedAt = DateTime.Now
                }
            };

            context.Questions.AddRange(questions);
            context.SaveChanges();

            CreateQuestionOptions(context, questions[0], new[] { "Định luật quán tính", "Định luật gia tốc", "Định luật tác dụng", "Định luật bảo toàn" });
            CreateQuestionOptions(context, questions[1], new[] { "F = m/a", "F = ma", "F = a/m", "F = m + a" });
        }

        private static void CreateChemistryQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quiz = new Quiz
            {
                SubjectId = subject.SubjectId,
                Title = "Bảng tuần hoàn",
                Description = "Kiến thức về bảng tuần hoàn các nguyên tố",
                CreatedBy = teacher.UserId,
                CreatedAt = DateTime.Now,
                IsPublic = true
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var question = new Question
            {
                QuizId = quiz.QuizId,
                Content = "Nguyên tố có ký hiệu hóa học H là:",
                CorrectOption = 'A',
                Explanation = "H là ký hiệu của nguyên tố Hydro",
                CreatedAt = DateTime.Now
            };

            context.Questions.Add(question);
            context.SaveChanges();

            CreateQuestionOptions(context, question, new[] { "Hydro", "Heli", "Hafni", "Holmi" });
        }

        private static void CreateBiologyQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quiz = new Quiz
            {
                SubjectId = subject.SubjectId,
                Title = "Tế bào học",
                Description = "Cấu trúc và chức năng của tế bào",
                CreatedBy = teacher.UserId,
                CreatedAt = DateTime.Now,
                IsPublic = true
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var question = new Question
            {
                QuizId = quiz.QuizId,
                Content = "Bào quan nào có chức năng sản xuất ATP?",
                CorrectOption = 'C',
                Explanation = "Ty thể có chức năng sản xuất ATP cho tế bào",
                CreatedAt = DateTime.Now
            };

            context.Questions.Add(question);
            context.SaveChanges();

            CreateQuestionOptions(context, question, new[] { "Nhân tế bào", "Ribosome", "Ty thể", "Lưới nội chất" });
        }

        private static void CreateEnglishQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quiz = new Quiz
            {
                SubjectId = subject.SubjectId,
                Title = "Grammar Basics",
                Description = "Basic English grammar and vocabulary",
                CreatedBy = teacher.UserId,
                CreatedAt = DateTime.Now,
                IsPublic = true
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var question = new Question
            {
                QuizId = quiz.QuizId,
                Content = "What is the past tense of 'go'?",
                CorrectOption = 'B',
                Explanation = "The past tense of 'go' is 'went'",
                CreatedAt = DateTime.Now
            };

            context.Questions.Add(question);
            context.SaveChanges();

            CreateQuestionOptions(context, question, new[] { "goed", "went", "gone", "going" });
        }

        private static void CreateLiteratureQuizzes(QuizardContext context, Subject subject, User teacher)
        {
            var quiz = new Quiz
            {
                SubjectId = subject.SubjectId,
                Title = "Văn học Việt Nam",
                Description = "Tác phẩm văn học Việt Nam nổi tiếng",
                CreatedBy = teacher.UserId,
                CreatedAt = DateTime.Now,
                IsPublic = true
            };

            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var question = new Question
            {
                QuizId = quiz.QuizId,
                Content = "Tác giả của 'Truyện Kiều' là ai?",
                CorrectOption = 'A',
                Explanation = "Nguyễn Du là tác giả của Truyện Kiều",
                CreatedAt = DateTime.Now
            };

            context.Questions.Add(question);
            context.SaveChanges();

            CreateQuestionOptions(context, question, new[] { "Nguyễn Du", "Hồ Xuân Hương", "Nguyễn Khuyến", "Tố Hữu" });
        }

        private static void CreateOtherSubjectQuizzes(QuizardContext context, System.Collections.Generic.List<Subject> subjects, System.Collections.Generic.List<User> teachers)
        {
            // Tạo quiz cho các môn còn lại
            var historySubject = subjects.First(s => s.Name == "Lịch sử");
            var geographySubject = subjects.First(s => s.Name == "Địa lý");
            var itSubject = subjects.First(s => s.Name == "Tin học");

            var moreQuizzes = new[]
            {
                new Quiz
                {
                    SubjectId = historySubject.SubjectId,
                    Title = "Lịch sử Việt Nam",
                    Description = "Các sự kiện lịch sử quan trọng",
                    CreatedBy = teachers[0].UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                },
                new Quiz
                {
                    SubjectId = geographySubject.SubjectId,
                    Title = "Địa lý Việt Nam",
                    Description = "Địa hình và khí hậu Việt Nam",
                    CreatedBy = teachers[1].UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                },
                new Quiz
                {
                    SubjectId = itSubject.SubjectId,
                    Title = "Tin học cơ bản",
                    Description = "Kiến thức tin học và lập trình",
                    CreatedBy = teachers[2].UserId,
                    CreatedAt = DateTime.Now,
                    IsPublic = true
                }
            };

            context.Quizzes.AddRange(moreQuizzes);
            context.SaveChanges();
        }

        private static void CreateQuestionOptions(QuizardContext context, Question question, string[] optionTexts)
        {
            var options = new[]
            {
                new QuestionOption { QuestionId = question.QuestionId, OptionLabel = 'A', Content = optionTexts[0] },
                new QuestionOption { QuestionId = question.QuestionId, OptionLabel = 'B', Content = optionTexts[1] },
                new QuestionOption { QuestionId = question.QuestionId, OptionLabel = 'C', Content = optionTexts[2] },
                new QuestionOption { QuestionId = question.QuestionId, OptionLabel = 'D', Content = optionTexts[3] }
            };

            context.QuestionOptions.AddRange(options);
            context.SaveChanges();
        }

        private static void CreateClassroomsAndEnrollments(QuizardContext context)
        {
            if (context.Classrooms.Any()) return;

            var teachers = context.Users.Where(u => u.Role == "teacher").ToList();
            var students = context.Users.Where(u => u.Role == "student").ToList();

            var classrooms = new[]
            {
                new Classroom { ClassName = "Lớp Toán 12A1", TeacherId = teachers[0].UserId, CreatedAt = DateTime.Now },
                new Classroom { ClassName = "Lớp Lý 12A2", TeacherId = teachers[1].UserId, CreatedAt = DateTime.Now },
                new Classroom { ClassName = "Lớp Hóa 12A3", TeacherId = teachers[2].UserId, CreatedAt = DateTime.Now },
                new Classroom { ClassName = "Lớp Sinh 12B1", TeacherId = teachers[3].UserId, CreatedAt = DateTime.Now },
                new Classroom { ClassName = "Lớp Anh 12B2", TeacherId = teachers[4].UserId, CreatedAt = DateTime.Now }
            };

            context.Classrooms.AddRange(classrooms);
            context.SaveChanges();

            // Enroll students in classrooms
            var enrollments = new System.Collections.Generic.List<Enrollment>();
            foreach (var classroom in classrooms)
            {
                foreach (var student in students)
                {
                    enrollments.Add(new Enrollment
                    {
                        ClassId = classroom.ClassId,
                        StudentId = student.UserId,
                        EnrolledAt = DateTime.Now
                    });
                }
            }

            context.Enrollments.AddRange(enrollments);
            context.SaveChanges();
        }
    }
}