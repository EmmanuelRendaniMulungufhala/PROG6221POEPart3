using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CybersecurityChatbot;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private Class1 chatbot = new Class1();

        private List<(string Title, string Description, DateTime? Reminder)> tasks = new List<(string, string, DateTime?)>();
        private List<(string Question, string[] Options, int AnswerIndex, string Explanation)> quizQuestions;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private List<string> activityLog = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeQuiz();
            UpdateQuestion();
            PlayWelcomeAudio();
        }

        private void PlayWelcomeAudio()
        {
            string filePath = @"C:\Users\lab_services_student\Desktop\mulu.wav";
            SoundPlayer player = null;
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    player = new SoundPlayer(filePath);
                    player.Load();
                    player.Play();
                    AddToActivityLog("🔊 Played welcome audio.");
                }
                else
                {
                    AddToActivityLog("[!] Audio file not found.");
                }
            }
            catch (Exception ex)
            {
                AddToActivityLog("[!] Error playing audio: " + ex.Message);
            }
            finally
            {
                player?.Dispose();
            }
        }

        private void AddToActivityLog(string message)
        {
            activityLog.Add($"{DateTime.Now:g} - {message}");
            UpdateActivityLog();
        }

        private void UpdateActivityLog()
        {
            ActivityLogList.Items.Clear();
            int count = activityLog.Count;
            var recent = (count <= 15) ? activityLog : activityLog.GetRange(count - 15, 15);
            foreach (var entry in recent)
            {
                ActivityLogList.Items.Add(entry);
            }
        }

        // TASK HANDLERS
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskInputBox.Text.Trim();
            string description = TaskDescriptionBox.Text.Trim();
            DateTime? reminder = null;

            if (DateTime.TryParse(ReminderInputBox.Text.Trim(), out DateTime parsedDate))
                reminder = parsedDate;

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(description))
            {
                tasks.Add((title, description, reminder));

                string displayText = $"{title} - {description}";
                if (reminder.HasValue)
                    displayText += $" (Remind at: {reminder.Value:g})";

                TaskList.Items.Add(displayText);

                string logMessage = $"📌 Task added: Title='{title}', Description='{description}'";
                if (reminder.HasValue)
                    logMessage += $", Reminder='{reminder.Value:g}'";
                AddToActivityLog(logMessage);

                TaskInputBox.Clear();
                TaskDescriptionBox.Clear();
                ReminderInputBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter both task title and description.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            int index = TaskList.SelectedIndex;
            if (index >= 0 && index < tasks.Count)
            {
                var removedTask = tasks[index];
                string logMessage = $"❌ Task removed: Title='{removedTask.Title}', Description='{removedTask.Description}'";
                if (removedTask.Reminder.HasValue)
                    logMessage += $", Reminder='{removedTask.Reminder.Value:g}'";

                AddToActivityLog(logMessage);

                tasks.RemoveAt(index);
                TaskList.Items.RemoveAt(index);
            }
        }

        // CHAT HANDLERS
        private void ChatSendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = ChatInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                ChatResponseLabel.Content = "Please enter a message.";
                return;
            }

            ChatInputBox.Clear();
            string response = chatbot.GetBotResponse(input);
            ChatResponseLabel.Content = response;

            AddToActivityLog($"👤 User: {input}");
            AddToActivityLog($"🤖 Bot: {response}");
        }

        // QUIZ HANDLERS
        private void InitializeQuiz()
        {
            quizQuestions = new List<(string Question, string[] Options, int AnswerIndex, string Explanation)>
            {
                ("What should you do if you receive an email asking for your password?",
                    new [] {"Reply with your password", "Delete the email", "Report as phishing", "Ignore it"}, 2,
                    "Legitimate organizations will never ask for your password via email. Always report suspicious emails."),
                ("What is a strong password?",
                    new [] {"123456", "MyName2023", "A long password with symbols, numbers, and letters", "Password1"}, 2,
                    "Strong passwords should be at least 12 characters long and include uppercase, lowercase, numbers, and symbols."),
                ("What is phishing?",
                    new [] {"A way to secure a website", "An attempt to steal sensitive information", "A network protocol", "A coding technique"}, 1,
                    "Phishing is a cyberattack where criminals impersonate legitimate entities to trick victims into revealing sensitive information."),
                ("How can you protect your online accounts?",
                    new [] {"Use the same password for every site", "Share your password with friends", "Enable two-factor authentication", "Write down your password"}, 2,
                    "Two-factor authentication adds an extra layer of security requiring a second verification step."),
                ("What is malware?",
                    new [] {"A software used to protect files", "Malicious software intended to damage or disrupt systems", "A hardware device", "A secure protocol"}, 1,
                    "Malware includes viruses, ransomware, and spyware designed to harm devices or steal data."),
                ("Why should you update software regularly?",
                    new [] {"To slow down your device", "To add new features and fix security vulnerabilities", "To use old versions forever", "To fill up storage"}, 1,
                    "Software updates often contain critical security patches that fix vulnerabilities."),
                ("What is a firewall?",
                    new [] {"A part of a computer screen", "A security system that monitors network traffic", "A browser plugin", "A mobile app"}, 1,
                    "Firewalls act as barriers controlling incoming and outgoing network traffic."),
                ("Why is it important to use strong, unique passwords for each site?",
                    new [] {"It's easier to remember", "It protects you if one site is breached", "To save time", "To match site requirements only"}, 1,
                    "Unique passwords prevent a breach on one site from compromising other accounts."),
                ("What is social engineering?",
                    new [] {"A coding technique", "A psychological tactic used by hackers to manipulate people", "A firewall feature", "A hardware design approach"}, 1,
                    "Social engineering uses psychological manipulation rather than technical hacking."),
                ("What should you do when using public WiFi?",
                    new [] {"Only visit trusted websites and use a VPN", "Access your bank account openly", "Share files openly", "Disable firewall"}, 0,
                    "Public WiFi is often insecure; a VPN encrypts your connection protecting your data.")
            };

            currentQuestionIndex = 0;
            score = 0;
            AddToActivityLog("📘 Quiz initialized.");
            UpdateProgress();
        }

        private void UpdateQuestion()
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                QuestionLabel.Text = "Quiz Completed!";
                OptionsList.Items.Clear();
                ResultLabel.Content = $"Final Score: {score}/{quizQuestions.Count}";
                CorrectAnswerLabel.Content = "";
                ExplanationLabel.Content = "";
                SubmitAnswerButton.IsEnabled = false;
                NextQuestionButton.Visibility = Visibility.Collapsed;
                AddToActivityLog($"✅ Quiz completed. Final Score: {score}/{quizQuestions.Count}");
                return;
            }

            var question = quizQuestions[currentQuestionIndex];
            QuestionLabel.Text = $"Question {currentQuestionIndex + 1}: {question.Question}";
            OptionsList.Items.Clear();

            foreach (var option in question.Options)
                OptionsList.Items.Add(option);

            OptionsList.SelectedIndex = -1;
            ResultLabel.Content = "";
            CorrectAnswerLabel.Content = "";
            ExplanationLabel.Content = "";
            SubmitAnswerButton.IsEnabled = true;
            NextQuestionButton.Visibility = Visibility.Collapsed;
            OptionsList.IsEnabled = true;
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an answer.", "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var question = quizQuestions[currentQuestionIndex];
            bool isCorrect = OptionsList.SelectedIndex == question.AnswerIndex;
            int selectedIndex = OptionsList.SelectedIndex;

            for (int i = 0; i < OptionsList.Items.Count; i++)
            {
                ListBoxItem item = OptionsList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item != null)
                {
                    item.ClearValue(Control.BackgroundProperty);
                    if (i == question.AnswerIndex)
                        item.Background = Brushes.LightGreen;
                    else if (i == selectedIndex && !isCorrect)
                        item.Background = Brushes.LightCoral;
                }
            }

            if (isCorrect)
            {
                ResultLabel.Content = "✅ Correct!";
                ResultLabel.Foreground = Brushes.Green;
                ExplanationLabel.Content = question.Explanation;
                CorrectAnswerLabel.Content = "";
                score++;
            }
            else
            {
                ResultLabel.Content = "❌ Incorrect.";
                ResultLabel.Foreground = Brushes.Red;
                CorrectAnswerLabel.Content = $"✔ Correct answer: {question.Options[question.AnswerIndex]}";
                ExplanationLabel.Content = question.Explanation;
            }

            AddToActivityLog($"Answered question {currentQuestionIndex + 1}: {(isCorrect ? "Correct" : "Incorrect")}");
            SubmitAnswerButton.IsEnabled = false;
            NextQuestionButton.Visibility = Visibility.Visible;
            OptionsList.IsEnabled = false;
            UpdateProgress();
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            UpdateQuestion();
        }

        private void RestartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            score = 0;
            currentQuestionIndex = 0;
            AddToActivityLog("🔁 Quiz restarted.");
            UpdateProgress();
            UpdateQuestion();
        }

        private void UpdateProgress()
        {
            double progress = ((double)currentQuestionIndex / quizQuestions.Count) * 100;
            QuizProgressBar.Value = progress;
        }
    }
}
