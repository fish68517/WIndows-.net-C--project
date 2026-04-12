using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Diary detail page.
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
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel card = UiTheme.CreateCard(120, 40, 980, 660);
            Controls.Add(card);

            card.Controls.Add(new Label
            {
                Text = "日记详情",
                Font = UiTheme.TitleFont(24),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(34, 24),
                Size = new Size(180, 38)
            });

            card.Controls.Add(new Label
            {
                Text = $"日期：{_diary.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.Secondary,
                Location = new Point(34, 80),
                Size = new Size(280, 24)
            });

            card.Controls.Add(new Label
            {
                Text = $"情绪指数：{_diary.EmotionIndex}",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = _diary.IsHighRisk ? UiTheme.Danger : UiTheme.Success,
                Location = new Point(360, 80),
                Size = new Size(180, 24)
            });

            card.Controls.Add(new Label
            {
                Text = _diary.IsHighRisk ? "状态：高风险，需要关注" : "状态：正常",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = _diary.IsHighRisk ? UiTheme.Danger : UiTheme.TextMuted,
                Location = new Point(560, 80),
                Size = new Size(220, 24)
            });

            TextBox contentBox = new TextBox
            {
                Location = new Point(34, 124),
                Size = new Size(910, 460),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = _diary.Content
            };
            UiTheme.StyleTextBox(contentBox, true);
            card.Controls.Add(contentBox);

            Button backButton = new Button { Text = "返回列表", Location = new Point(824, 604), Size = new Size(120, 40) };
            UiTheme.StyleSecondaryButton(backButton);
            backButton.Click += (s, e) => _mainWindow.NavigateToDiaryList(_currentUser);
            card.Controls.Add(backButton);

            ResumeLayout(false);
        }
    }
}
