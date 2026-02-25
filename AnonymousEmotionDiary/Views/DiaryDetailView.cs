using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Diary detail view for displaying the complete content of a diary entry.
    /// Shows diary content, creation time, emotion index, and high-risk indicators.
    /// Allows users to return to the diary list.
    /// </summary>
    public partial class DiaryDetailView : Form
    {
        private readonly Diary _diary;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;

        public DiaryDetailView(Diary diary, User currentUser, MainWindow mainWindow)
        {
            _diary = diary;
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Anonymous Emotion Diary - Diary Detail";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "日记详情";
            titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Size = new System.Drawing.Size(200, 30);
            this.Controls.Add(titleLabel);

            // Date label
            Label dateLabel = new Label();
            dateLabel.Text = $"日期: {_diary.CreatedAt:yyyy-MM-dd HH:mm:ss}";
            dateLabel.Font = new System.Drawing.Font("Arial", 10);
            dateLabel.ForeColor = System.Drawing.Color.Gray;
            dateLabel.Location = new System.Drawing.Point(20, 60);
            dateLabel.Size = new System.Drawing.Size(300, 20);
            this.Controls.Add(dateLabel);

            // Emotion index label
            Label emotionLabel = new Label();
            emotionLabel.Text = $"Emotion Index: {_diary.EmotionIndex}";
            emotionLabel.Font = new System.Drawing.Font("Arial", 10);
            emotionLabel.ForeColor = System.Drawing.Color.Gray;
            emotionLabel.Location = new System.Drawing.Point(350, 60);
            emotionLabel.Size = new System.Drawing.Size(200, 20);
            this.Controls.Add(emotionLabel);

            // High-risk indicator
            if (_diary.IsHighRisk)
            {
                Label riskLabel = new Label();
                riskLabel.Text = "⚠ HIGH RISK EMOTION";
                riskLabel.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
                riskLabel.ForeColor = System.Drawing.Color.Red;
                riskLabel.Location = new System.Drawing.Point(600, 60);
                riskLabel.Size = new System.Drawing.Size(150, 20);
                this.Controls.Add(riskLabel);
            }

            // Content label
            Label contentLabel = new Label();
            contentLabel.Text = "Content:";
            contentLabel.Location = new System.Drawing.Point(20, 100);
            contentLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(contentLabel);

            // Content textbox (read-only)
            TextBox contentTextBox = new TextBox();
            contentTextBox.Name = "ContentTextBox";
            contentTextBox.Location = new System.Drawing.Point(20, 125);
            contentTextBox.Size = new System.Drawing.Size(740, 350);
            contentTextBox.Multiline = true;
            contentTextBox.ScrollBars = ScrollBars.Vertical;
            contentTextBox.WordWrap = true;
            contentTextBox.ReadOnly = true;
            contentTextBox.Text = _diary.Content;
            this.Controls.Add(contentTextBox);

            // Return button
            Button returnButton = new Button();
            returnButton.Name = "ReturnButton";
            returnButton.Text = "返回到列表";
            returnButton.Location = new System.Drawing.Point(660, 490);
            returnButton.Size = new System.Drawing.Size(100, 30);
            returnButton.Click += ReturnButton_Click;
            this.Controls.Add(returnButton);

            this.ResumeLayout(false);
        }

        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _mainWindow.NavigateToDiaryList(_currentUser);
        }
    }
}
