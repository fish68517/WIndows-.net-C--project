using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// High-risk warning page.
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
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel card = UiTheme.CreateCard(180, 60, 860, 560);
            card.BackColor = Color.FromArgb(255, 248, 245);
            Controls.Add(card);

            card.Controls.Add(new Label
            {
                Text = "高风险情绪提醒",
                Font = UiTheme.TitleFont(26),
                ForeColor = UiTheme.Danger,
                Location = new Point(34, 28),
                Size = new Size(300, 40)
            });

            card.Controls.Add(new Label
            {
                Text = $"系统检测到本篇日记情绪指数为 {_diary.EmotionIndex}/100，已达到高风险阈值。",
                Font = UiTheme.BodyFont(12, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(34, 84),
                Size = new Size(620, 26)
            });

            TextBox warningBox = new TextBox
            {
                Location = new Point(34, 130),
                Size = new Size(792, 310),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = new EmotionDetectionService().GetRiskWarning(_diary.EmotionIndex)
            };
            UiTheme.StyleTextBox(warningBox, true);
            card.Controls.Add(warningBox);

            Label footer = new Label
            {
                Text = "如果你有持续的自伤、自杀念头，请优先联系身边可信赖的人，并尽快寻求线下专业帮助。",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.Danger,
                Location = new Point(34, 458),
                Size = new Size(720, 24)
            };
            card.Controls.Add(footer);

            Button backButton = new Button { Text = "返回列表", Location = new Point(706, 496), Size = new Size(120, 40) };
            UiTheme.StylePrimaryButton(backButton);
            backButton.Click += (s, e) =>
            {
                if (_mainWindow != null)
                {
                    _mainWindow.NavigateToDiaryList(_currentUser);
                }
                else
                {
                    Close();
                }
            };
            card.Controls.Add(backButton);

            ResumeLayout(false);
        }
    }
}
