using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Diary edit view for creating new diary entries.
    /// Allows users to write diary content with character count tracking and emotion analysis.
    /// </summary>
    public partial class DiaryEditView : Form
    {
        private readonly DiaryService _diaryService;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;
        private const int MaxDiaryContentLength = 5000;

        public DiaryEditView(User currentUser, MainWindow mainWindow = null)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _diaryService = new DiaryService();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Anonymous Emotion Diary - Write Diary";
            this.Width = 600;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "Write Your Diary";
            titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Size = new System.Drawing.Size(200, 30);
            this.Controls.Add(titleLabel);

            // Character count label
            Label charCountLabel = new Label();
            charCountLabel.Name = "CharCountLabel";
            charCountLabel.Text = "Characters: 0 / 5000";
            charCountLabel.Font = new System.Drawing.Font("Arial", 10);
            charCountLabel.ForeColor = System.Drawing.Color.Gray;
            charCountLabel.Location = new System.Drawing.Point(400, 25);
            charCountLabel.Size = new System.Drawing.Size(150, 20);
            charCountLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Controls.Add(charCountLabel);

            // Content label
            Label contentLabel = new Label();
            contentLabel.Text = "Diary Content:";
            contentLabel.Location = new System.Drawing.Point(20, 60);
            contentLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(contentLabel);

            // Content textbox
            TextBox contentTextBox = new TextBox();
            contentTextBox.Name = "ContentTextBox";
            contentTextBox.Location = new System.Drawing.Point(20, 85);
            contentTextBox.Size = new System.Drawing.Size(540, 300);
            contentTextBox.Multiline = true;
            contentTextBox.ScrollBars = ScrollBars.Vertical;
            contentTextBox.WordWrap = true;
            contentTextBox.TextChanged += ContentTextBox_TextChanged;
            this.Controls.Add(contentTextBox);

            // Error message label
            Label errorLabel = new Label();
            errorLabel.Name = "ErrorLabel";
            errorLabel.Text = "";
            errorLabel.ForeColor = System.Drawing.Color.Red;
            errorLabel.Location = new System.Drawing.Point(20, 395);
            errorLabel.Size = new System.Drawing.Size(540, 30);
            errorLabel.AutoSize = false;
            errorLabel.WordWrap = true;
            this.Controls.Add(errorLabel);

            // Publish button
            Button publishButton = new Button();
            publishButton.Name = "PublishButton";
            publishButton.Text = "Publish";
            publishButton.Location = new System.Drawing.Point(280, 435);
            publishButton.Size = new System.Drawing.Size(100, 30);
            publishButton.Enabled = false;
            publishButton.Click += PublishButton_Click;
            this.Controls.Add(publishButton);

            // Return button
            Button returnButton = new Button();
            returnButton.Name = "ReturnButton";
            returnButton.Text = "Return";
            returnButton.Location = new System.Drawing.Point(420, 435);
            returnButton.Size = new System.Drawing.Size(100, 30);
            returnButton.Click += ReturnButton_Click;
            this.Controls.Add(returnButton);

            this.ResumeLayout(false);
        }

        private void ContentTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox contentTextBox = (TextBox)this.Controls["ContentTextBox"];
            Label charCountLabel = (Label)this.Controls["CharCountLabel"];
            Button publishButton = (Button)this.Controls["PublishButton"];

            int currentLength = contentTextBox.Text.Length;
            int remainingLength = MaxDiaryContentLength - currentLength;

            // Update character count display
            charCountLabel.Text = $"Characters: {currentLength} / {MaxDiaryContentLength}";

            // Update publish button state
            publishButton.Enabled = currentLength > 0;

            // Change color if approaching limit
            if (remainingLength < 500)
            {
                charCountLabel.ForeColor = System.Drawing.Color.Orange;
            }
            else if (remainingLength < 0)
            {
                charCountLabel.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                charCountLabel.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void PublishButton_Click(object sender, EventArgs e)
        {
            TextBox contentTextBox = (TextBox)this.Controls["ContentTextBox"];
            Label errorLabel = (Label)this.Controls["ErrorLabel"];

            string content = contentTextBox.Text.Trim();

            // Validate content
            if (!_diaryService.ValidateDiaryContent(content))
            {
                errorLabel.Text = "Diary content must not be empty and not exceed 5000 characters.";
                return;
            }

            // Create diary
            Diary createdDiary = _diaryService.CreateDiary(_currentUser.UserId, content);

            if (createdDiary != null)
            {
                errorLabel.Text = "";
                MessageBox.Show("Diary published successfully!", "Success");
                
                // Check if high risk and show warning if needed
                if (createdDiary.IsHighRisk)
                {
                    ShowHighRiskWarning(createdDiary);
                }

                // Navigate back to diary list
                NavigateToDiaryList();
            }
            else
            {
                errorLabel.Text = "Failed to publish diary. Please try again.";
            }
        }

        private void ReturnButton_Click(object sender, EventArgs e)
        {
            // Navigate back to diary list
            NavigateToDiaryList();
        }

        private void ShowHighRiskWarning(Diary diary)
        {
            // Display the high-risk warning view
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToHighRiskWarning(diary);
            }
            else
            {
                HighRiskWarningView warningView = new HighRiskWarningView(diary, _currentUser);
                warningView.ShowDialog();
            }
        }

        private void NavigateToDiaryList()
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryList(_currentUser);
            }
            else
            {
                DiaryListView listView = new DiaryListView(_currentUser);
                listView.Show();
                this.Close();
            }
        }
    }
}
