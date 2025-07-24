using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuizardApp.Models;
using QuizardApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;
using System.Collections.Generic;

namespace QuizardApp.ViewModels
{
    public class QuizTakingViewModel : BaseViewModel
    {
        private int studentQuizId;
        private ObservableCollection<QuestionViewModel> questions;
        private QuestionViewModel currentQuestion;
        private int currentQuestionIndex;
        private string message;
        private string quizTitle;
        private string timeRemaining;
        private DispatcherTimer timer;
        private DateTime startTime;
        private bool isQuizCompleted;

        public ObservableCollection<QuestionViewModel> Questions
        {
            get => questions;
            set => SetProperty(ref questions, value);
        }

        public QuestionViewModel CurrentQuestion
        {
            get => currentQuestion;
            set => SetProperty(ref currentQuestion, value);
        }

        public int CurrentQuestionIndex
        {
            get => currentQuestionIndex;
            set => SetProperty(ref currentQuestionIndex, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public string QuizTitle
        {
            get => quizTitle;
            set => SetProperty(ref quizTitle, value);
        }

        public string TimeRemaining
        {
            get => timeRemaining;
            set => SetProperty(ref timeRemaining, value);
        }

        public bool IsQuizCompleted
        {
            get => isQuizCompleted;
            set => SetProperty(ref isQuizCompleted, value);
        }

        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand SubmitQuizCommand { get; }
        public ICommand SelectAnswerCommand { get; }

        public QuizTakingViewModel(int studentQuizId)
        {
            this.studentQuizId = studentQuizId;
            Questions = new ObservableCollection<QuestionViewModel>();
            
            NextQuestionCommand = new RelayCommand(ExecuteNextQuestion);
            PreviousQuestionCommand = new RelayCommand(ExecutePreviousQuestion);
            SubmitQuizCommand = new RelayCommand(ExecuteSubmitQuiz);
            SelectAnswerCommand = new RelayCommand(ExecuteSelectAnswer);
            
            LoadQuiz();
            StartTimer();
        }

        private void LoadQuiz()
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var studentQuiz = context.StudentQuizzes
                        .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                        .FirstOrDefault(sq => sq.StudentQuizId == studentQuizId);

                    if (studentQuiz == null)
                    {
                        Message = "Quiz not found";
                        return;
                    }

                    QuizTitle = studentQuiz.Quiz.Title;
                    startTime = studentQuiz.StartedAt;

                    Questions.Clear();
                    foreach (var question in studentQuiz.Quiz.Questions.OrderBy(q => q.QuestionId))
                    {
                        var questionViewModel = new QuestionViewModel
                        {
                            QuestionId = question.QuestionId,
                            Content = question.Content,
                            Options = question.QuestionOptions.OrderBy(o => o.OptionLabel).Select(o => new OptionViewModel
                            {
                                OptionId = o.OptionId,
                                Label = o.OptionLabel,
                                Content = o.Content,
                                IsSelected = false
                            }).ToList(),
                            SelectedAnswer = null
                        };
                        Questions.Add(questionViewModel);
                    }

                    if (Questions.Count > 0)
                    {
                        CurrentQuestion = Questions[0];
                        CurrentQuestionIndex = 0;
                    }

                    Message = $"Quiz loaded successfully. {Questions.Count} questions to answer.";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error loading quiz: {ex.Message}";
            }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - startTime;
            TimeRemaining = $"Time: {elapsed:hh\\:mm\\:ss}";
        }

        private void ExecuteNextQuestion(object obj)
        {
            if (CurrentQuestionIndex < Questions.Count - 1)
            {
                CurrentQuestionIndex++;
                CurrentQuestion = Questions[CurrentQuestionIndex];
            }
        }

        private void ExecutePreviousQuestion(object obj)
        {
            if (CurrentQuestionIndex > 0)
            {
                CurrentQuestionIndex--;
                CurrentQuestion = Questions[CurrentQuestionIndex];
            }
        }

        private void ExecuteSelectAnswer(object obj)
        {
            if (obj is string selectedOption && CurrentQuestion != null)
            {
                // Clear previous selections
                foreach (var option in CurrentQuestion.Options)
                {
                    option.IsSelected = false;
                }

                // Set new selection
                var selectedOptionViewModel = CurrentQuestion.Options.FirstOrDefault(o => o.Label == selectedOption);
                if (selectedOptionViewModel != null)
                {
                    selectedOptionViewModel.IsSelected = true;
                    CurrentQuestion.SelectedAnswer = selectedOption;
                }
            }
        }

        private void ExecuteSubmitQuiz(object obj)
        {
            try
            {
                using (var context = new QuizardContext())
                {
                    var studentQuiz = context.StudentQuizzes.Find(studentQuizId);
                    if (studentQuiz == null)
                    {
                        Message = "Quiz not found";
                        return;
                    }

                    // Save answers
                    int correctAnswers = 0;
                    int totalQuestions = Questions.Count;

                    foreach (var question in Questions)
                    {
                        if (question.SelectedAnswer != null)
                        {
                            // Get correct answer from database
                            var dbQuestion = context.Questions.Find(question.QuestionId);
                            bool isCorrect = dbQuestion?.CorrectOption == question.SelectedAnswer;
                            
                            if (isCorrect) correctAnswers++;

                            var studentAnswer = new StudentAnswer
                            {
                                StudentQuizId = studentQuizId,
                                QuestionId = question.QuestionId,
                                SelectedOption = question.SelectedAnswer,
                                IsCorrect = isCorrect,
                                AnsweredAt = DateTime.Now
                            };

                            context.StudentAnswers.Add(studentAnswer);
                        }
                    }

                    // Calculate score (out of 10)
                    double score = totalQuestions > 0 ? (correctAnswers * 10.0 / totalQuestions) : 0;

                    // Update student quiz
                    studentQuiz.FinishedAt = DateTime.Now;
                    studentQuiz.Score = score;

                    context.SaveChanges();

                    // Stop timer
                    timer?.Stop();

                    IsQuizCompleted = true;
                    Message = $"Quiz completed! Score: {score:F1}/10 ({correctAnswers}/{totalQuestions} correct)";

                    // Navigate back to results or dashboard
                    var resultsPage = new Views.QuizResultDetailView(studentQuizId);
                    AppNavigationService.Instance.Navigate(resultsPage);
                }
            }
            catch (Exception ex)
            {
                Message = $"Error submitting quiz: {ex.Message}";
            }
        }
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public List<OptionViewModel> Options { get; set; }
        public string SelectedAnswer { get; set; }
    }

    public class OptionViewModel
    {
        public int OptionId { get; set; }
        public string Label { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; }
    }
}