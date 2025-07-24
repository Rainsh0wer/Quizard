USE [master]

/*******************************************************************************
   Drop database if it exists
********************************************************************************/
IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'Quizard')
BEGIN
    ALTER DATABASE [Quizard] SET OFFLINE WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [Quizard] SET ONLINE;
    DROP DATABASE [Quizard];
END

GO

CREATE DATABASE Quizard
GO


USE [Quizard]
Select * from User
GO

-- 1. Tài khoản người dùng
CREATE TABLE [User] (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Role VARCHAR(10) CHECK (Role IN ('teacher', 'student')) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- 2. Môn học
CREATE TABLE Subject (
    SubjectID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. Bài kiểm tra
CREATE TABLE Quiz (
    QuizID INT PRIMARY KEY IDENTITY(1,1),
    SubjectID INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedBy INT NOT NULL, -- UserID của giáo viên
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsPublic BIT DEFAULT 1,
    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID),
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

-- 4. Câu hỏi
CREATE TABLE Question (
    QuestionID INT PRIMARY KEY IDENTITY(1,1),
    QuizID INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CorrectOption CHAR(1) NOT NULL CHECK (CorrectOption IN ('A','B','C','D')),
    Explanation NVARCHAR(MAX), -- giải thích nếu học sinh sai
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID)
);

-- 5. Các đáp án của câu hỏi
CREATE TABLE QuestionOption (
    OptionID INT PRIMARY KEY IDENTITY(1,1),
    QuestionID INT NOT NULL,
    OptionLabel CHAR(1) NOT NULL CHECK (OptionLabel IN ('A','B','C','D')),
    Content NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID)
);

-- 6. Học sinh bắt đầu làm quiz
CREATE TABLE StudentQuiz (
    StudentQuizID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT NOT NULL,
    QuizID INT NOT NULL,
    StartedAt DATETIME DEFAULT GETDATE(),
    FinishedAt DATETIME,
    Score FLOAT CHECK (Score >= 0),
    FOREIGN KEY (StudentID) REFERENCES [User](UserID),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID)
);

-- 7. Câu trả lời từng câu hỏi của học sinh
CREATE TABLE StudentAnswer (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    StudentQuizID INT NOT NULL,
    QuestionID INT NOT NULL,
    SelectedOption CHAR(1) CHECK (SelectedOption IN ('A','B','C','D')),
    IsCorrect BIT,
    AnsweredAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentQuizID) REFERENCES StudentQuiz(StudentQuizID),
    FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID)
);

CREATE TABLE Classroom (
    ClassID INT PRIMARY KEY IDENTITY(1,1),
    ClassName NVARCHAR(100) NOT NULL,
    TeacherID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TeacherID) REFERENCES [User](UserID)
);

CREATE TABLE Enrollment (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
    ClassID INT NOT NULL,
    StudentID INT NOT NULL,
    EnrolledAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClassID) REFERENCES Classroom(ClassID),
    FOREIGN KEY (StudentID) REFERENCES [User](UserID),
    UNIQUE (ClassID, StudentID)
);


CREATE TABLE QuizAssignment (
    AssignmentID INT PRIMARY KEY IDENTITY(1,1),
    QuizID INT NOT NULL,
    ClassID INT NULL,           -- nếu giao cho cả lớp
    StudentID INT NULL,         -- nếu giao riêng 1 học sinh
    DueDate DATETIME,
    AssignedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID),
    FOREIGN KEY (ClassID) REFERENCES Classroom(ClassID),
    FOREIGN KEY (StudentID) REFERENCES [User](UserID)
);


CREATE TABLE Tag (
    TagID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE QuestionTag (
    QuestionID INT NOT NULL,
    TagID INT NOT NULL,
    PRIMARY KEY (QuestionID, TagID),
    FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID),
    FOREIGN KEY (TagID) REFERENCES Tag(TagID)
);


CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT NOT NULL,
    QuizID INT NOT NULL,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(500),
    SubmittedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentID) REFERENCES [User](UserID),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID)
);


CREATE TABLE QuizLike (
    LikeID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    QuizID INT NOT NULL,
    LikedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID),
    UNIQUE (UserID, QuizID) -- mỗi user chỉ like 1 lần
);


CREATE TABLE SavedQuiz (
    SavedID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT NOT NULL,
    QuizID INT NOT NULL,
    SavedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentID) REFERENCES [User](UserID),
    FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID),
    UNIQUE (StudentID, QuizID) -- mỗi quiz chỉ được lưu 1 lần bởi mỗi học sinh
);



-- 1. User
INSERT INTO [User] (Username, Email, PasswordHash, FullName, Role) VALUES
('charlesmartinez', 'lgonzalez@gmail.com', '(DX5PA_a', N'Danielle Haley', 'teacher'),
('jose49', 'manningthomas@gmail.com', '#Z6@aTGn', N'Denise Hall', 'student'),
('bbrooks', 'jeremy34@hotmail.com', 'a%j*u4Vl', N'Nicole Calhoun', 'teacher'),
('shammond', 'acarr@snyder-adams.org', 'f!07gC^n', N'Anthony Dixon', 'teacher'),
('virginiastone', 'parksbrian@hotmail.com', ')+v4Ci1k', N'Devin Dixon', 'teacher'),
('psmith', 'loganjones@walsh.org', '^oy6F1rb', N'Sheila Shaw', 'teacher'),
('richard45', 'jonescatherine@james.net', '#R4CRq#!', N'Mackenzie York', 'teacher'),
('crosssara', 'thomasdonna@hotmail.com', '$2YW$l%q', N'Marcus Wilson', 'student'),
('fscott', 'christinekennedy@burke-compton.com', '*mR8t+Ej', N'Scott Francis', 'student'),
('sandersjennifer', 'harrisomar@brown.info', 'W%Yx0BSl', N'Amanda Marsh', 'student'),
('william52', 'erika81@wallace.org', '#8%2WQ1p', N'Monica Wilson', 'teacher'),
('dorseyjonathan', 'brandon58@wells-smith.com', 'Qo@2v^f1', N'Matthew Gray', 'teacher'),
('suttonjessica', 'robertscody@bradley.org', '4$v6Wh3n', N'Brian Vega', 'student'),
('kellimartin', 'daniel07@wong.net', 'aH#20t%c', N'Laura Dawson', 'student'),
('carrie69', 'phillipsholly@christian.com', 'sPl@^rH9', N'Samantha Cohen', 'teacher'),
('jordan74', 'johnsonamy@owen.biz', 'hY#j9U0q', N'Michelle Young', 'teacher'),
('gabriel22', 'anthony00@pitts.biz', 'Df@93qV!', N'Johnny Andrews', 'student'),
('fernandezshawn', 'jared66@dixon.com', 'nT%fR7p!', N'Raymond Morton', 'student'),
('jenniferlopez', 'woodsmarvin@yahoo.com', 'kR4%uh7Q', N'Kristen Greer', 'teacher'),
('samanthaprice', 'powellshane@gmail.com', 'Fb8^6Lc!', N'Jordan Rivera', 'student'),
('tanderson', 'colemangrace@bauer-moss.com', 'M1!pzAxe', N'Angelica Dunn', 'teacher'),
('millerjeffrey', 'karenwright@castillo.info', 'Z0v!@8Hy', N'Logan Drake', 'student'),
('oliver19', 'schmidtmichael@yahoo.com', 'Bp!3xO%f', N'Andrew Burton', 'student'),
('marierose', 'kellyfreeman@hotmail.com', 'Kf4^vR0!', N'Alicia Morgan', 'student'),
('kevinmurphy', 'jacksoncory@griffin.com', 'L2x%6Cy!', N'Jonathan Hogan', 'teacher'),
('valerie56', 'sandrawhite@smith.org', 'Rz@8Fw#c', N'Angela Barber', 'teacher'),
('heather64', 'alexandersusan@gmail.com', 'G5%jRw9!', N'Victoria Wolfe', 'student'),
('jared28', 'daniel60@ferguson.org', 'Tz2@eWy1', N'Diana Peters', 'student'),
('cory45', 'stephaniebennett@chan.com', 'Nv7!qyLs', N'Peter Vaughn', 'student'),
('shelly22', 'tyler75@gmail.com', 'Jr4^ePx!', N'Mariah Park', 'teacher');

-- Thêm nhiều dữ liệu mẫu cho User
INSERT INTO [User] (Username, Email, PasswordHash, FullName, Role) VALUES
('user31', 'user31@example.com', 'pass31', N'User 31', 'student'),
('user32', 'user32@example.com', 'pass32', N'User 32', 'student'),
('user33', 'user33@example.com', 'pass33', N'User 33', 'teacher'),
('user34', 'user34@example.com', 'pass34', N'User 34', 'student'),
('user35', 'user35@example.com', 'pass35', N'User 35', 'teacher'),
('user36', 'user36@example.com', 'pass36', N'User 36', 'student'),
('user37', 'user37@example.com', 'pass37', N'User 37', 'student'),
('user38', 'user38@example.com', 'pass38', N'User 38', 'teacher'),
('user39', 'user39@example.com', 'pass39', N'User 39', 'student'),
('user40', 'user40@example.com', 'pass40', N'User 40', 'teacher');


-- 2. Subject
INSERT INTO Subject (Name, Description) VALUES
(N'Math', N'Mathematics'),
(N'Physics', N'Physics subject'),
(N'Chemistry', N'Chemistry subject'),
(N'Biology', N'Biology basics'),
(N'History', N'World history'),
(N'Geography', N'Geographical features'),
(N'English', N'Grammar and writing'),
(N'CS', N'Computer Science'),
(N'Economics', N'Economic theory'),
(N'Art', N'Creative arts');

-- Thêm nhiều Subject
INSERT INTO Subject (Name, Description) VALUES
(N'Literature', N'Literature subject'),
(N'Music', N'Music subject'),
(N'Sport', N'Sport subject'),
(N'Philosophy', N'Philosophy subject'),
(N'Psychology', N'Psychology subject');

-- 3. Quiz
INSERT INTO Quiz (SubjectID, Title, Description, CreatedBy) VALUES
(1, N'Quiz 1', N'Description 1', 1),
(2, N'Quiz 2', N'Description 2', 2),
(3, N'Quiz 3', N'Description 3', 3),
(4, N'Quiz 4', N'Description 4', 1),
(5, N'Quiz 5', N'Description 5', 2),
(6, N'Quiz 6', N'Description 6', 3),
(7, N'Quiz 7', N'Description 7', 1),
(8, N'Quiz 8', N'Description 8', 2),
(9, N'Quiz 9', N'Description 9', 3),
(10, N'Quiz 10', N'Description 10', 1);

-- Thêm nhiều Quiz
INSERT INTO Quiz (SubjectID, Title, Description, CreatedBy) VALUES
(11, N'Quiz 11', N'Description 11', 31),
(12, N'Quiz 12', N'Description 12', 32),
(13, N'Quiz 13', N'Description 13', 33),
(14, N'Quiz 14', N'Description 14', 34),
(15, N'Quiz 15', N'Description 15', 35);

-- 4. Question
INSERT INTO Question (QuizID, Content, CorrectOption, Explanation) VALUES
(1, N'Câu hỏi 1?', 'A', N'Giải thích 1'),
(2, N'Câu hỏi 2?', 'B', N'Giải thích 2'),
(3, N'Câu hỏi 3?', 'C', N'Giải thích 3'),
(4, N'Câu hỏi 4?', 'D', N'Giải thích 4'),
(5, N'Câu hỏi 5?', 'A', N'Giải thích 5'),
(6, N'Câu hỏi 6?', 'B', N'Giải thích 6'),
(7, N'Câu hỏi 7?', 'C', N'Giải thích 7'),
(8, N'Câu hỏi 8?', 'D', N'Giải thích 8'),
(9, N'Câu hỏi 9?', 'A', N'Giải thích 9'),
(10, N'Câu hỏi 10?', 'B', N'Giải thích 10');

-- Thêm nhiều Question
INSERT INTO Question (QuizID, Content, CorrectOption, Explanation) VALUES
(11, N'Câu hỏi 11?', 'A', N'Giải thích 11'),
(12, N'Câu hỏi 12?', 'B', N'Giải thích 12'),
(13, N'Câu hỏi 13?', 'C', N'Giải thích 13'),
(14, N'Câu hỏi 14?', 'D', N'Giải thích 14'),
(15, N'Câu hỏi 15?', 'A', N'Giải thích 15');

-- 5. QuestionOption
INSERT INTO QuestionOption (QuestionID, OptionLabel, Content)
SELECT QuestionID, X.OptionLabel, CONCAT(N'Option ', X.OptionLabel, N' for Question ', QuestionID)
FROM Question CROSS JOIN (SELECT 'A' AS OptionLabel UNION SELECT 'B' UNION SELECT 'C' UNION SELECT 'D') AS X;

-- Thêm nhiều QuestionOption
INSERT INTO QuestionOption (QuestionID, OptionLabel, Content) VALUES
(11, 'A', N'Option A for Question 11'),
(11, 'B', N'Option B for Question 11'),
(11, 'C', N'Option C for Question 11'),
(11, 'D', N'Option D for Question 11'),
(12, 'A', N'Option A for Question 12'),
(12, 'B', N'Option B for Question 12'),
(12, 'C', N'Option C for Question 12'),
(12, 'D', N'Option D for Question 12'),
(13, 'A', N'Option A for Question 13'),
(13, 'B', N'Option B for Question 13'),
(13, 'C', N'Option C for Question 13'),
(13, 'D', N'Option D for Question 13'),
(14, 'A', N'Option A for Question 14'),
(14, 'B', N'Option B for Question 14'),
(14, 'C', N'Option C for Question 14'),
(14, 'D', N'Option D for Question 14'),
(15, 'A', N'Option A for Question 15'),
(15, 'B', N'Option B for Question 15'),
(15, 'C', N'Option C for Question 15'),
(15, 'D', N'Option D for Question 15');

-- 6. StudentQuiz
INSERT INTO StudentQuiz (StudentID, QuizID, Score) VALUES
(4, 1, 8.5),
(5, 2, 7.0),
(6, 3, 6.5),
(7, 4, 9.0),
(8, 5, 5.5),
(9, 6, 6.0),
(10, 7, 7.5),
(4, 8, 8.0),
(5, 9, 6.8),
(6, 10, 9.2);

-- Thêm nhiều StudentQuiz
INSERT INTO StudentQuiz (StudentID, QuizID, Score) VALUES
(31, 11, 8.0),
(32, 12, 7.5),
(33, 13, 9.0),
(34, 14, 6.5),
(35, 15, 7.0);

-- 7. StudentAnswer
INSERT INTO StudentAnswer (StudentQuizID, QuestionID, SelectedOption, IsCorrect) VALUES
(1, 1, 'A', 1),
(2, 2, 'B', 1),
(3, 3, 'A', 0),
(4, 4, 'C', 0),
(5, 5, 'A', 1),
(6, 6, 'B', 1),
(7, 7, 'C', 1),
(8, 8, 'D', 1),
(9, 9, 'A', 1),
(10, 10, 'B', 1);

-- Thêm nhiều StudentAnswer
INSERT INTO StudentAnswer (StudentQuizID, QuestionID, SelectedOption, IsCorrect) VALUES
(11, 11, 'A', 1),
(12, 12, 'B', 1),
(13, 13, 'C', 1),
(14, 14, 'D', 1),
(15, 15, 'A', 1);

-- 8. Classroom
INSERT INTO Classroom (ClassName, TeacherID) VALUES
(N'Class A', 1),
(N'Class B', 2),
(N'Class C', 3),
(N'Class D', 1),
(N'Class E', 2),
(N'Class F', 3),
(N'Class G', 1),
(N'Class H', 2),
(N'Class I', 3),
(N'Class J', 1);

-- Thêm nhiều Classroom
INSERT INTO Classroom (ClassName, TeacherID) VALUES
(N'Class K', 33),
(N'Class L', 35);

-- 9. Enrollment
INSERT INTO Enrollment (ClassID, StudentID) VALUES
(1, 4), (1, 5),
(2, 6), (2, 7),
(3, 8), (3, 9),
(4, 10), (5, 4),
(6, 5), (7, 6);

-- Thêm nhiều Enrollment
INSERT INTO Enrollment (ClassID, StudentID) VALUES
(11, 31), (12, 32);

-- 10. QuizAssignment
INSERT INTO QuizAssignment (QuizID, ClassID, DueDate) VALUES
(1, 1, GETDATE()+7),
(2, 2, GETDATE()+7),
(3, 3, GETDATE()+7),
(4, 4, GETDATE()+7),
(5, 5, GETDATE()+7),
(6, 6, GETDATE()+7),
(7, 7, GETDATE()+7),
(8, 8, GETDATE()+7),
(9, 9, GETDATE()+7),
(10, 10, GETDATE()+7);

-- Thêm nhiều QuizAssignment
INSERT INTO QuizAssignment (QuizID, ClassID, DueDate) VALUES
(11, 11, GETDATE()+7),
(12, 12, GETDATE()+7);

-- 11. Tag
INSERT INTO Tag (Name) VALUES
(N'Algebra'), (N'Geometry'), (N'Physics'), (N'Chemistry'), (N'History'),
(N'Biology'), (N'English'), (N'Programming'), (N'Basics'), (N'Advanced');

-- Thêm nhiều Tag
INSERT INTO Tag (Name) VALUES
(N'Logic'), (N'Critical Thinking');

-- 12. QuestionTag
INSERT INTO QuestionTag (QuestionID, TagID) VALUES
(1,1),(2,2),(3,3),(4,4),(5,5),(6,6),(7,7),(8,8),(9,9),(10,10);

-- Thêm nhiều QuestionTag
INSERT INTO QuestionTag (QuestionID, TagID) VALUES
(11,11),(12,12);

-- 13. Feedback
INSERT INTO Feedback (StudentID, QuizID, Rating, Comment) VALUES
(4, 1, 5, N'Good!'),
(5, 2, 4, N'Nice'),
(6, 3, 5, N'Interesting'),
(7, 4, 3, N'Too hard'),
(8, 5, 4, N'Well designed'),
(9, 6, 5, N'Love it'),
(10, 7, 4, N'Challenging'),
(4, 8, 5, N'Useful'),
(5, 9, 3, N'So so'),
(6, 10, 4, N'Fair');

-- Thêm nhiều Feedback
INSERT INTO Feedback (StudentID, QuizID, Rating, Comment) VALUES
(31, 11, 5, N'Excellent!'),
(32, 12, 4, N'Good!');

-- 14. QuizLike
INSERT INTO QuizLike (UserID, QuizID) VALUES
(4,1),(5,2),(6,3),(7,4),(8,5),(9,6),(10,7),(4,8),(5,9),(6,10);

-- Thêm nhiều QuizLike
INSERT INTO QuizLike (UserID, QuizID) VALUES
(31,11),(32,12);

-- 15. SavedQuiz
INSERT INTO SavedQuiz (StudentID, QuizID) VALUES
(4,1),(5,2),(6,3),(7,4),(8,5),(9,6),(10,7),(4,8),(5,9),(6,10);

-- Thêm nhiều SavedQuiz
INSERT INTO SavedQuiz (StudentID, QuizID) VALUES
(31,11),(32,12);
