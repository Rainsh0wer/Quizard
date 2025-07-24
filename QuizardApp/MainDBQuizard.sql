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


-- Thêm nhiều dữ liệu mẫu vào các bảng

-- Thêm thêm 50 user
INSERT INTO [User] (Username, Email, PasswordHash, FullName, Role)
SELECT CONCAT('user', n), CONCAT('user', n, '@mail.com'), 'pass', CONCAT(N'User ', n), CASE WHEN n % 2 = 0 THEN 'student' ELSE 'teacher' END
FROM (SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 30 AS n FROM sys.objects) AS nums;

-- Thêm thêm 20 subject
INSERT INTO Subject (Name, Description)
SELECT CONCAT(N'Subject ', n), CONCAT(N'Description for subject ', n)
FROM (SELECT TOP 20 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 100 quiz
INSERT INTO Quiz (SubjectID, Title, Description, CreatedBy)
SELECT (ABS(CHECKSUM(NEWID())) % 20) + 1, CONCAT(N'Quiz ', n), CONCAT(N'Description for quiz ', n), (ABS(CHECKSUM(NEWID())) % 50) + 1
FROM (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 300 question
INSERT INTO Question (QuizID, Content, CorrectOption, Explanation)
SELECT (ABS(CHECKSUM(NEWID())) % 100) + 1, CONCAT(N'Question ', n, N' content?'), CHAR(65 + (ABS(CHECKSUM(NEWID())) % 4)), CONCAT(N'Explanation for question ', n)
FROM (SELECT TOP 300 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 1200 question option (4 option mỗi question)
INSERT INTO QuestionOption (QuestionID, OptionLabel, Content)
SELECT q.QuestionID, o.OptionLabel, CONCAT(N'Option ', o.OptionLabel, N' for Question ', q.QuestionID)
FROM Question q
CROSS JOIN (SELECT 'A' AS OptionLabel UNION SELECT 'B' UNION SELECT 'C' UNION SELECT 'D') o
WHERE q.QuestionID > 10;

-- Thêm thêm 200 student quiz
INSERT INTO StudentQuiz (StudentID, QuizID, Score)
SELECT (ABS(CHECKSUM(NEWID())) % 25) + 4, (ABS(CHECKSUM(NEWID())) % 100) + 1, (ABS(CHECKSUM(NEWID())) % 10) + 1
FROM (SELECT TOP 200 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 800 student answer
INSERT INTO StudentAnswer (StudentQuizID, QuestionID, SelectedOption, IsCorrect)
SELECT (ABS(CHECKSUM(NEWID())) % 200) + 1, (ABS(CHECKSUM(NEWID())) % 300) + 1, CHAR(65 + (ABS(CHECKSUM(NEWID())) % 4)), (ABS(CHECKSUM(NEWID())) % 2)
FROM (SELECT TOP 800 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 30 classroom
INSERT INTO Classroom (ClassName, TeacherID)
SELECT CONCAT(N'Class ', n), (ABS(CHECKSUM(NEWID())) % 25) + 1
FROM (SELECT TOP 30 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 100 enrollment
INSERT INTO Enrollment (ClassID, StudentID)
SELECT (ABS(CHECKSUM(NEWID())) % 30) + 1, (ABS(CHECKSUM(NEWID())) % 25) + 4
FROM (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 50 quiz assignment
INSERT INTO QuizAssignment (QuizID, ClassID, DueDate)
SELECT (ABS(CHECKSUM(NEWID())) % 100) + 1, (ABS(CHECKSUM(NEWID())) % 30) + 1, GETDATE() + (ABS(CHECKSUM(NEWID())) % 30)
FROM (SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 20 tag
INSERT INTO Tag (Name)
SELECT CONCAT(N'Tag ', n)
FROM (SELECT TOP 20 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 200 question tag
INSERT INTO QuestionTag (QuestionID, TagID)
SELECT (ABS(CHECKSUM(NEWID())) % 300) + 1, (ABS(CHECKSUM(NEWID())) % 20) + 1
FROM (SELECT TOP 200 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 100 feedback
INSERT INTO Feedback (StudentID, QuizID, Rating, Comment)
SELECT (ABS(CHECKSUM(NEWID())) % 25) + 4, (ABS(CHECKSUM(NEWID())) % 100) + 1, (ABS(CHECKSUM(NEWID())) % 5) + 1, CONCAT(N'Feedback for quiz ', n)
FROM (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 100 quiz like
INSERT INTO QuizLike (UserID, QuizID)
SELECT (ABS(CHECKSUM(NEWID())) % 50) + 1, (ABS(CHECKSUM(NEWID())) % 100) + 1
FROM (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Thêm thêm 100 saved quiz
INSERT INTO SavedQuiz (StudentID, QuizID)
SELECT (ABS(CHECKSUM(NEWID())) % 25) + 4, (ABS(CHECKSUM(NEWID())) % 100) + 1
FROM (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 10 AS n FROM sys.objects) AS nums;

-- Kết thúc thêm dữ liệu mẫu

-- BỔ SUNG THÊM DỮ LIỆU MẪU CHO TEST

-- Thêm 100 user mới (50 teacher, 50 student)
INSERT INTO [User] (Username, Email, PasswordHash, FullName, Role, IsActive) VALUES
('teacher_nguyen', 'nguyen.teacher@school.edu', 'pass123', N'Nguyễn Văn Minh', 'teacher', 1),
('teacher_tran', 'tran.teacher@school.edu', 'pass123', N'Trần Thị Lan', 'teacher', 1),
('teacher_le', 'le.teacher@school.edu', 'pass123', N'Lê Hoàng Nam', 'teacher', 1),
('teacher_pham', 'pham.teacher@school.edu', 'pass123', N'Phạm Thị Mai', 'teacher', 1),
('teacher_hoang', 'hoang.teacher@school.edu', 'pass123', N'Hoàng Văn Đức', 'teacher', 1),

('student_an', 'an.student@student.edu', 'pass123', N'Nguyễn Văn An', 'student', 1),
('student_binh', 'binh.student@student.edu', 'pass123', N'Trần Văn Bình', 'student', 1),
('student_chi', 'chi.student@student.edu', 'pass123', N'Lê Thị Chi', 'student', 1),
('student_duc', 'duc.student@student.edu', 'pass123', N'Phạm Văn Đức', 'student', 1),
('student_em', 'em.student@student.edu', 'pass123', N'Hoàng Thị Em', 'student', 1),
('student_phong', 'phong.student@student.edu', 'pass123', N'Vũ Văn Phong', 'student', 1),
('student_giang', 'giang.student@student.edu', 'pass123', N'Đỗ Thị Giang', 'student', 1),
('student_hung', 'hung.student@student.edu', 'pass123', N'Bùi Văn Hùng', 'student', 1),
('student_linh', 'linh.student@student.edu', 'pass123', N'Ngô Thị Linh', 'student', 1),
('student_minh', 'minh.student@student.edu', 'pass123', N'Đinh Văn Minh', 'student', 1);

-- Thêm nhiều môn học mới
INSERT INTO Subject (Name, Description) VALUES
(N'Ngữ Văn', N'Môn Ngữ Văn Việt Nam'),
(N'Lịch Sử', N'Lịch sử Việt Nam và Thế giới'),
(N'Địa Lý', N'Địa lý Việt Nam và Thế giới'),
(N'Sinh Học', N'Sinh học cơ bản và nâng cao'),
(N'Hóa Học', N'Hóa học cơ bản'),
(N'Vật Lý', N'Vật lý cơ bản và nâng cao'),
(N'Tiếng Anh', N'Tiếng Anh giao tiếp và học thuật'),
(N'Tin Học', N'Tin học cơ bản và lập trình'),
(N'Giáo Dục Công Dân', N'Giáo dục công dân và pháp luật'),
(N'Thể Dục', N'Giáo dục thể chất');

-- Thêm nhiều quiz mới với nội dung chi tiết
INSERT INTO Quiz (SubjectID, Title, Description, CreatedBy, IsPublic) VALUES
-- Toán học
(1, N'Đại số cơ bản', N'Kiểm tra kiến thức về phương trình và bất phương trình', 81, 1),
(1, N'Hình học phẳng', N'Bài kiểm tra về tam giác và tứ giác', 81, 1),
(1, N'Lượng giác', N'Kiểm tra về sin, cos, tan và các hàm lượng giác', 82, 1),
(1, N'Vi phân tích phân', N'Bài kiểm tra về đạo hàm và tích phân cơ bản', 82, 1),

-- Vật lý
(2, N'Cơ học chất điểm', N'Kiểm tra về chuyển động thẳng và chuyển động tròn', 83, 1),
(2, N'Nhiệt học', N'Bài kiểm tra về nhiệt độ, nhiệt lượng và các quá trình nhiệt', 83, 1),
(2, N'Điện học cơ bản', N'Kiểm tra về điện tích, điện trường và mạch điện', 84, 1),
(2, N'Sóng và dao động', N'Bài kiểm tra về dao động điều hòa và sóng cơ', 84, 1),

-- Hóa học
(3, N'Bảng tuần hoàn', N'Kiểm tra về cấu tạo nguyên tử và bảng tuần hoàn', 85, 1),
(3, N'Liên kết hóa học', N'Bài kiểm tra về liên kết ion, cộng hóa trị và kim loại', 85, 1),
(3, N'Phản ứng hóa học', N'Kiểm tra về các loại phản ứng và cân bằng hóa học', 81, 1),
(3, N'Hóa học hữu cơ', N'Bài kiểm tra về hydrocacbon và dẫn xuất', 82, 1),

-- Sinh học  
(4, N'Tế bào học', N'Kiểm tra về cấu tạo và chức năng của tế bào', 83, 1),
(4, N'Di truyền học', N'Bài kiểm tra về DNA, RNA và di truyền', 84, 1),
(4, N'Sinh thái học', N'Kiểm tra về hệ sinh thái và môi trường', 85, 1),
(4, N'Tiến hóa', N'Bài kiểm tra về thuyết tiến hóa và chọn lọc tự nhiên', 81, 1),

-- Ngữ văn
(21, N'Văn học Việt Nam cổ điển', N'Kiểm tra về thơ ca và truyện cổ', 82, 1),
(21, N'Văn học hiện đại', N'Bài kiểm tra về văn học thế kỷ 20', 83, 1),
(21, N'Kỹ năng viết', N'Kiểm tra kỹ năng viết văn nghị luận', 84, 1),

-- Tiếng Anh
(27, N'Grammar Basics', N'Test on basic English grammar rules', 85, 1),
(27, N'Vocabulary Building', N'Test on common English vocabulary', 81, 1),
(27, N'Reading Comprehension', N'Test on English reading skills', 82, 1),
(27, N'Listening Skills', N'Test on English listening comprehension', 83, 1);

-- Thêm nhiều câu hỏi chi tiết cho các quiz
-- Câu hỏi cho quiz Đại số cơ bản (QuizID = 111)
INSERT INTO Question (QuizID, Content, CorrectOption, Explanation) VALUES
(111, N'Nghiệm của phương trình 2x + 5 = 11 là:', 'B', N'2x = 11 - 5 = 6, nên x = 3'),
(111, N'Điều kiện xác định của biểu thức √(x-2) là:', 'C', N'Biểu thức dưới dấu căn phải không âm: x - 2 ≥ 0'),
(111, N'Tập nghiệm của bất phương trình x + 3 > 7 là:', 'A', N'x > 7 - 3, tức là x > 4'),
(111, N'Giá trị của x² - 3x + 2 khi x = 2 là:', 'D', N'Thay x = 2: 4 - 6 + 2 = 0'),
(111, N'Phương trình x² - 5x + 6 = 0 có nghiệm là:', 'B', N'Sử dụng công thức nghiệm hoặc phân tích: (x-2)(x-3) = 0');

-- Thêm options cho các câu hỏi trên
INSERT INTO QuestionOption (QuestionID, OptionLabel, Content) VALUES
-- Câu hỏi 1
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Nghiệm của phương trình 2x + 5 = 11%'), 'A', N'x = 2'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Nghiệm của phương trình 2x + 5 = 11%'), 'B', N'x = 3'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Nghiệm của phương trình 2x + 5 = 11%'), 'C', N'x = 4'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Nghiệm của phương trình 2x + 5 = 11%'), 'D', N'x = 5'),

-- Câu hỏi 2
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Điều kiện xác định%'), 'A', N'x > 2'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Điều kiện xác định%'), 'B', N'x < 2'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Điều kiện xác định%'), 'C', N'x ≥ 2'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Điều kiện xác định%'), 'D', N'x ≤ 2'),

-- Câu hỏi 3
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Tập nghiệm của bất phương trình%'), 'A', N'x > 4'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Tập nghiệm của bất phương trình%'), 'B', N'x < 4'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Tập nghiệm của bất phương trình%'), 'C', N'x ≥ 4'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Tập nghiệm của bất phương trình%'), 'D', N'x ≤ 4'),

-- Câu hỏi 4
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Giá trị của x² - 3x + 2%'), 'A', N'1'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Giá trị của x² - 3x + 2%'), 'B', N'2'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Giá trị của x² - 3x + 2%'), 'C', N'3'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Giá trị của x² - 3x + 2%'), 'D', N'0'),

-- Câu hỏi 5
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Phương trình x² - 5x + 6%'), 'A', N'x = 1, x = 6'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Phương trình x² - 5x + 6%'), 'B', N'x = 2, x = 3'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Phương trình x² - 5x + 6%'), 'C', N'x = 1, x = 5'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Phương trình x² - 5x + 6%'), 'D', N'Vô nghiệm');

-- Thêm câu hỏi cho quiz Vật lý - Cơ học chất điểm
INSERT INTO Question (QuizID, Content, CorrectOption, Explanation) VALUES
(115, N'Đơn vị của vận tốc trong hệ SI là:', 'A', N'Vận tốc = quãng đường / thời gian = m/s'),
(115, N'Công thức tính gia tốc là:', 'C', N'Gia tốc = thay đổi vận tốc / thời gian'),
(115, N'Vật rơi tự do có gia tốc bằng:', 'B', N'Gia tốc rơi tự do g ≈ 9.8 m/s²'),
(115, N'Chuyển động thẳng đều có đặc điểm:', 'D', N'Vận tốc không đổi theo thời gian'),
(115, N'Lực ma sát luôn có hướng:', 'A', N'Lực ma sát ngược chiều với chuyển động');

-- Thêm options cho quiz Vật lý
INSERT INTO QuestionOption (QuestionID, OptionLabel, Content) VALUES
-- Đơn vị vận tốc
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Đơn vị của vận tốc%'), 'A', N'm/s'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Đơn vị của vận tốc%'), 'B', N'km/h'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Đơn vị của vận tốc%'), 'C', N'm/s²'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Đơn vị của vận tốc%'), 'D', N'N'),

-- Công thức gia tốc
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Công thức tính gia tốc%'), 'A', N'a = v × t'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Công thức tính gia tốc%'), 'B', N'a = s / t'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Công thức tính gia tốc%'), 'C', N'a = Δv / Δt'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Công thức tính gia tốc%'), 'D', N'a = F / m'),

-- Gia tốc rơi tự do
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Vật rơi tự do%'), 'A', N'10 m/s²'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Vật rơi tự do%'), 'B', N'9.8 m/s²'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Vật rơi tự do%'), 'C', N'8.9 m/s²'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Vật rơi tự do%'), 'D', N'11 m/s²'),

-- Chuyển động thẳng đều
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Chuyển động thẳng đều%'), 'A', N'Gia tốc không đổi'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Chuyển động thẳng đều%'), 'B', N'Quãng đường không đổi'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Chuyển động thẳng đều%'), 'C', N'Gia tốc bằng 0'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Chuyển động thẳng đều%'), 'D', N'Vận tốc không đổi'),

-- Lực ma sát
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Lực ma sát%'), 'A', N'Ngược chiều chuyển động'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Lực ma sát%'), 'B', N'Cùng chiều chuyển động'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Lực ma sát%'), 'C', N'Vuông góc với chuyển động'),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Lực ma sát%'), 'D', N'Hướng tùy ý');

-- Thêm lớp học
INSERT INTO Classroom (ClassName, TeacherID) VALUES
(N'Lớp 10A1 - Toán', 81),
(N'Lớp 10A2 - Toán', 81),
(N'Lớp 11B1 - Vật Lý', 83),
(N'Lớp 11B2 - Vật Lý', 83),
(N'Lớp 12C1 - Hóa Học', 85),
(N'Lớp 12C2 - Hóa Học', 85),
(N'Lớp 9A - Toán Cơ Bản', 82),
(N'Lớp 9B - Vật Lý Cơ Bản', 84);

-- Thêm học sinh vào lớp
INSERT INTO Enrollment (ClassID, StudentID) VALUES
-- Lớp 10A1
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), 86),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), 87),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), 88),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), 89),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), 90),

-- Lớp 10A2  
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A2 - Toán'), 91),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A2 - Toán'), 92),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A2 - Toán'), 93),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A2 - Toán'), 94),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A2 - Toán'), 95),

-- Lớp 11B1
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), 96),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), 86),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), 88),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), 90),

-- Lớp 11B2
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B2 - Vật Lý'), 87),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B2 - Vật Lý'), 89),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B2 - Vật Lý'), 91),
((SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B2 - Vật Lý'), 93);

-- Tạo nhiều kết quả làm bài mẫu
DECLARE @StudentID INT, @QuizID INT, @Score FLOAT, @StartTime DATETIME, @FinishTime DATETIME;

-- Học sinh làm quiz Đại số cơ bản
SET @QuizID = 111;
SET @StartTime = DATEADD(DAY, -5, GETDATE());
SET @FinishTime = DATEADD(MINUTE, 15, @StartTime);

INSERT INTO StudentQuiz (StudentID, QuizID, StartedAt, FinishedAt, Score) VALUES
(86, @QuizID, @StartTime, @FinishTime, 8.5),
(87, @QuizID, DATEADD(HOUR, 1, @StartTime), DATEADD(MINUTE, 20, DATEADD(HOUR, 1, @StartTime)), 7.2),
(88, @QuizID, DATEADD(HOUR, 2, @StartTime), DATEADD(MINUTE, 18, DATEADD(HOUR, 2, @StartTime)), 9.1),
(89, @QuizID, DATEADD(HOUR, 3, @StartTime), DATEADD(MINUTE, 25, DATEADD(HOUR, 3, @StartTime)), 6.8),
(90, @QuizID, DATEADD(HOUR, 4, @StartTime), DATEADD(MINUTE, 22, DATEADD(HOUR, 4, @StartTime)), 5.5);

-- Học sinh làm quiz Cơ học chất điểm  
SET @QuizID = 115;
SET @StartTime = DATEADD(DAY, -3, GETDATE());

INSERT INTO StudentQuiz (StudentID, QuizID, StartedAt, FinishedAt, Score) VALUES
(96, @QuizID, @StartTime, DATEADD(MINUTE, 12, @StartTime), 9.2),
(86, @QuizID, DATEADD(HOUR, 1, @StartTime), DATEADD(MINUTE, 15, DATEADD(HOUR, 1, @StartTime)), 7.8),
(88, @QuizID, DATEADD(HOUR, 2, @StartTime), DATEADD(MINUTE, 18, DATEADD(HOUR, 2, @StartTime)), 8.4),
(90, @QuizID, DATEADD(HOUR, 3, @StartTime), DATEADD(MINUTE, 20, DATEADD(HOUR, 3, @StartTime)), 6.2);

-- Thêm câu trả lời chi tiết cho các bài làm
-- Đây chỉ là ví dụ cho một vài bài làm
DECLARE @StudentQuizID INT;

-- Lấy StudentQuizID đầu tiên và thêm câu trả lời
SELECT TOP 1 @StudentQuizID = StudentQuizID FROM StudentQuiz WHERE StudentID = 86 AND QuizID = 111;

INSERT INTO StudentAnswer (StudentQuizID, QuestionID, SelectedOption, IsCorrect, AnsweredAt) VALUES
(@StudentQuizID, (SELECT TOP 1 QuestionID FROM Question WHERE QuizID = 111 AND Content LIKE N'Nghiệm của phương trình%'), 'B', 1, GETDATE()),
(@StudentQuizID, (SELECT TOP 1 QuestionID FROM Question WHERE QuizID = 111 AND Content LIKE N'Điều kiện xác định%'), 'C', 1, GETDATE()),
(@StudentQuizID, (SELECT TOP 1 QuestionID FROM Question WHERE QuizID = 111 AND Content LIKE N'Tập nghiệm của bất phương trình%'), 'A', 1, GETDATE()),
(@StudentQuizID, (SELECT TOP 1 QuestionID FROM Question WHERE QuizID = 111 AND Content LIKE N'Giá trị của x²%'), 'D', 1, GETDATE()),
(@StudentQuizID, (SELECT TOP 1 QuestionID FROM Question WHERE QuizID = 111 AND Content LIKE N'Phương trình x² - 5x%'), 'A', 0, GETDATE());

-- Thêm một số quiz được lưu
INSERT INTO SavedQuiz (StudentID, QuizID, SavedAt) VALUES
(86, 112, GETDATE()),
(86, 113, GETDATE()),
(87, 111, GETDATE()),
(87, 115, GETDATE()),
(88, 114, GETDATE()),
(89, 116, GETDATE()),
(90, 117, GETDATE());

-- Thêm một số like cho quiz
INSERT INTO QuizLike (UserID, QuizID, LikedAt) VALUES
(86, 111, GETDATE()),
(87, 111, GETDATE()),
(88, 111, GETDATE()),
(86, 115, GETDATE()),
(96, 115, GETDATE()),
(90, 115, GETDATE()),
(89, 112, GETDATE()),
(91, 113, GETDATE());

-- Thêm feedback
INSERT INTO Feedback (StudentID, QuizID, Rating, Comment, SubmittedAt) VALUES
(86, 111, 5, N'Bài kiểm tra rất hay và phù hợp với chương trình học', GETDATE()),
(87, 111, 4, N'Câu hỏi tốt nhưng có thể thêm một số ví dụ', GETDATE()),
(88, 111, 5, N'Excellente! Très bon quiz pour réviser', GETDATE()),
(96, 115, 5, N'Quiz về vật lý rất thú vị và bổ ích', GETDATE()),
(86, 115, 4, N'Cần thêm hình ảnh minh họa cho dễ hiểu hơn', GETDATE());

-- Gán quiz cho lớp học
INSERT INTO QuizAssignment (QuizID, ClassID, DueDate, AssignedAt) VALUES
(111, (SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), DATEADD(DAY, 7, GETDATE()), GETDATE()),
(112, (SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 10A1 - Toán'), DATEADD(DAY, 14, GETDATE()), GETDATE()),
(115, (SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), DATEADD(DAY, 10, GETDATE()), GETDATE()),
(116, (SELECT TOP 1 ClassID FROM Classroom WHERE ClassName = N'Lớp 11B1 - Vật Lý'), DATEADD(DAY, 21, GETDATE()), GETDATE());

-- Thêm tags cho câu hỏi
INSERT INTO Tag (Name) VALUES
(N'Phương trình'),
(N'Bất phương trình'),
(N'Hàm số'),
(N'Cơ học'),
(N'Nhiệt học'),
(N'Điện học'),
(N'Nguyên tử'),
(N'Phân tử'),
(N'Tế bào'),
(N'Di truyền');

-- Gán tag cho câu hỏi
INSERT INTO QuestionTag (QuestionID, TagID) VALUES
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Nghiệm của phương trình%'), (SELECT TagID FROM Tag WHERE Name = N'Phương trình')),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Tập nghiệm của bất phương trình%'), (SELECT TagID FROM Tag WHERE Name = N'Bất phương trình')),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Đơn vị của vận tốc%'), (SELECT TagID FROM Tag WHERE Name = N'Cơ học')),
((SELECT TOP 1 QuestionID FROM Question WHERE Content LIKE N'Công thức tính gia tốc%'), (SELECT TagID FROM Tag WHERE Name = N'Cơ học'));

PRINT 'Đã bổ sung thêm dữ liệu mẫu thành công!'

