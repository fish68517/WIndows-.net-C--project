using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// High-risk warning view for displaying emotion analysis alerts.
    /// Shows warning information and psychological support resources when high-risk emotions are detected.
    /// </summary>
    public partial class HighRiskWarningView : Form
    {
        private readonly Diary _diary;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;

        public HighRiskWarningView(Diary diary, User currentUser, MainWindow mainWindow = null)
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
            this.Text = "高风险情绪警告 - Anonymous Emotion Diary";
            this.Width = 700;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;

            // Warning icon and title panel
            Panel headerPanel = new Panel();
            headerPanel.BackColor = System.Drawing.Color.FromArgb(255, 200, 0); // Warning yellow
            headerPanel.Location = new System.Drawing.Point(0, 0);
            headerPanel.Size = new System.Drawing.Size(700, 80);
            this.Controls.Add(headerPanel);

            // Warning title
            Label warningTitle = new Label();
            warningTitle.Text = "⚠ 高风险情绪检测到";
            warningTitle.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            warningTitle.ForeColor = System.Drawing.Color.DarkRed;
            warningTitle.Location = new System.Drawing.Point(20, 15);
            warningTitle.Size = new System.Drawing.Size(660, 40);
            warningTitle.AutoSize = false;
            warningTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            headerPanel.Controls.Add(warningTitle);

            // Emotion index display
            Label emotionLabel = new Label();
            emotionLabel.Text = $"情绪指数: {_diary.EmotionIndex}/100";
            emotionLabel.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            emotionLabel.ForeColor = System.Drawing.Color.DarkRed;
            emotionLabel.Location = new System.Drawing.Point(20, 50);
            emotionLabel.Size = new System.Drawing.Size(300, 20);
            headerPanel.Controls.Add(emotionLabel);

            // Main content panel
            Panel contentPanel = new Panel();
            contentPanel.Location = new System.Drawing.Point(20, 100);
            contentPanel.Size = new System.Drawing.Size(660, 480);
            contentPanel.AutoScroll = true;
            this.Controls.Add(contentPanel);

            // Alert message
            Label alertMessage = new Label();
            alertMessage.Text = "您的日记条目已被检测为包含高风险情绪。 " +
                               "如果您正在经历情绪困扰，请考虑寻求支持。";
            alertMessage.Font = new System.Drawing.Font("Arial", 11);
            alertMessage.ForeColor = System.Drawing.Color.Black;
            alertMessage.Location = new System.Drawing.Point(0, 0);
            alertMessage.Size = new System.Drawing.Size(640, 60);
            alertMessage.AutoSize = false;
            contentPanel.Controls.Add(alertMessage);

            // Support resources section
            Label resourcesTitle = new Label();
            resourcesTitle.Text = "📞 心理支持资源:";
            resourcesTitle.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            resourcesTitle.ForeColor = System.Drawing.Color.DarkBlue;
            resourcesTitle.Location = new System.Drawing.Point(0, 70);
            resourcesTitle.Size = new System.Drawing.Size(640, 25);
            contentPanel.Controls.Add(resourcesTitle);

            // Hotline resources
            Label hotlineLabel = new Label();
            hotlineLabel.Text = "危机热线:\n" +
                               "• National Mental Health Hotline: 400-161-9995\n" +
                               "• Life Support Hotline: 400-821-1215\n" +
                               "• Crisis Text Line: Text HOME to 741741";
            hotlineLabel.Font = new System.Drawing.Font("Arial", 10);
            hotlineLabel.ForeColor = System.Drawing.Color.Black;
            hotlineLabel.Location = new System.Drawing.Point(0, 100);
            hotlineLabel.Size = new System.Drawing.Size(640, 80);
            hotlineLabel.AutoSize = false;
            contentPanel.Controls.Add(hotlineLabel);

            // Online resources
            Label onlineLabel = new Label();
            onlineLabel.Text = "在线咨询服务:\n" +
                              "• Mental Health Support Platform: https://www.xinli.com\n" +
                              "• Psychological Consultation Services: https://www.xlzx.cn\n" +
                              "• Campus Counseling Center (if available)";
            onlineLabel.Font = new System.Drawing.Font("Arial", 10);
            onlineLabel.ForeColor = System.Drawing.Color.Black;
            onlineLabel.Location = new System.Drawing.Point(0, 190);
            onlineLabel.Size = new System.Drawing.Size(640, 80);
            onlineLabel.AutoSize = false;
            contentPanel.Controls.Add(onlineLabel);

            // Professional help
            Label professionalLabel = new Label();
            professionalLabel.Text = "专业医疗帮助:\n" +
                                    "• Contact your local hospital's psychology or psychiatry department\n" +
                                    "• Visit your campus counseling center\n" +
                                    "• Schedule an appointment with a mental health professional";
            professionalLabel.Font = new System.Drawing.Font("Arial", 10);
            professionalLabel.ForeColor = System.Drawing.Color.Black;
            professionalLabel.Location = new System.Drawing.Point(0, 280);
            professionalLabel.Size = new System.Drawing.Size(640, 80);
            professionalLabel.AutoSize = false;
            contentPanel.Controls.Add(professionalLabel);

            // Encouragement message
            Label encouragementLabel = new Label();
            encouragementLabel.Text = "Remember: Seeking help is a sign of strength, not weakness. " +
                                     "You are not alone, and support is available.";
            encouragementLabel.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Italic);
            encouragementLabel.ForeColor = System.Drawing.Color.DarkGreen;
            encouragementLabel.Location = new System.Drawing.Point(0, 370);
            encouragementLabel.Size = new System.Drawing.Size(640, 60);
            encouragementLabel.AutoSize = false;
            contentPanel.Controls.Add(encouragementLabel);

            // Close button
            Button closeButton = new Button();
            closeButton.Name = "CloseButton";
            closeButton.Text = "返回到列表";
            closeButton.Location = new System.Drawing.Point(280, 600);
            closeButton.Size = new System.Drawing.Size(140, 35);
            closeButton.Font = new System.Drawing.Font("Arial", 11);
            closeButton.BackColor = System.Drawing.Color.FromArgb(70, 130, 180); // Steel blue
            closeButton.ForeColor = System.Drawing.Color.White;
            closeButton.Click += CloseButton_Click;
            this.Controls.Add(closeButton);

            this.ResumeLayout(false);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            // Close the warning view and return to diary list
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryList(_currentUser);
            }
            else
            {
                this.Close();
            }
        }
    }
}
