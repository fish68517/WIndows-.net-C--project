using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// User diary dashboard.
    /// </summary>
    public partial class DiaryListView : Form
    {
        private readonly DiaryService _diaryService;
        private readonly EmotionDetectionService _emotionDetectionService;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;
        private List<Diary> _diaries;
        private UserEmotionProfile _profile;
        private DataGridView _grid;
        private Label _summaryLabel;
        private Label _actionLabel;
        private Label _statsLabel;
        private Label _loadingLabel;

        public DiaryListView(User currentUser, MainWindow mainWindow = null)
        {
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _diaryService = new DiaryService();
            _emotionDetectionService = new EmotionDetectionService();
            _diaries = new List<Diary>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel heroCard = UiTheme.CreateCard(24, 24, 1180, 180);
            heroCard.BackColor = UiTheme.SurfaceStrong;
            Controls.Add(heroCard);

            Label titleLabel = new Label
            {
                Text = $"你好，{_currentUser?.Username}",
                Font = UiTheme.TitleFont(26),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(30, 24),
                Size = new Size(260, 38)
            };
            heroCard.Controls.Add(titleLabel);

            _statsLabel = new Label
            {
                Font = UiTheme.BodyFont(12, FontStyle.Bold),
                ForeColor = UiTheme.Secondary,
                Location = new Point(30, 72),
                Size = new Size(560, 24)
            };
            heroCard.Controls.Add(_statsLabel);

            _summaryLabel = new Label
            {
                Font = UiTheme.BodyFont(10.5f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(30, 106),
                Size = new Size(760, 28)
            };
            heroCard.Controls.Add(_summaryLabel);

            _actionLabel = new Label
            {
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.Accent,
                Location = new Point(30, 138),
                Size = new Size(780, 24)
            };
            heroCard.Controls.Add(_actionLabel);

            _loadingLabel = new Label
            {
                Font = UiTheme.BodyFont(10),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(850, 144),
                Size = new Size(300, 24),
                TextAlign = ContentAlignment.MiddleRight
            };
            heroCard.Controls.Add(_loadingLabel);

            Button newDiaryButton = new Button { Text = "写新日记", Location = new Point(876, 42), Size = new Size(130, 42) };
            UiTheme.StylePrimaryButton(newDiaryButton);
            newDiaryButton.Click += (s, e) => NavigateToDiaryEdit(null);
            heroCard.Controls.Add(newDiaryButton);

            Button refreshButton = new Button { Text = "刷新画像", Location = new Point(1022, 42), Size = new Size(130, 42) };
            UiTheme.StyleSecondaryButton(refreshButton);
            refreshButton.Click += async (s, e) => await LoadDiariesAsync();
            heroCard.Controls.Add(refreshButton);

            Button logoutButton = new Button { Text = "退出", Location = new Point(1022, 98), Size = new Size(130, 42) };
            UiTheme.StyleSecondaryButton(logoutButton);
            logoutButton.Click += (s, e) => _mainWindow?.HandleLogout();
            heroCard.Controls.Add(logoutButton);

            Panel listCard = UiTheme.CreateCard(24, 224, 1180, 500);
            Controls.Add(listCard);

            listCard.Controls.Add(new Label
            {
                Text = "我的日记记录",
                Font = UiTheme.TitleFont(18),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(24, 18),
                Size = new Size(220, 30)
            });

            _grid = new DataGridView
            {
                Location = new Point(24, 60),
                Size = new Size(1128, 360),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            UiTheme.StyleGrid(_grid);
            _grid.Columns.Add("DiaryId", "ID");
            _grid.Columns.Add("CreatedAt", "日期");
            _grid.Columns.Add("EmotionIndex", "情绪指数");
            _grid.Columns.Add("Risk", "风险状态");
            _grid.Columns.Add("Summary", "内容摘要");
            listCard.Controls.Add(_grid);

            Button detailButton = new Button { Text = "查看详情", Location = new Point(744, 434), Size = new Size(120, 36) };
            UiTheme.StyleSecondaryButton(detailButton);
            detailButton.Click += DetailButton_Click;
            listCard.Controls.Add(detailButton);

            Button deleteButton = new Button { Text = "删除日记", Location = new Point(880, 434), Size = new Size(120, 36) };
            UiTheme.StyleDangerButton(deleteButton);
            deleteButton.Click += DeleteButton_Click;
            listCard.Controls.Add(deleteButton);

            Button writeButton = new Button { Text = "继续书写", Location = new Point(1016, 434), Size = new Size(120, 36) };
            UiTheme.StylePrimaryButton(writeButton);
            writeButton.Click += ContinueWriteButton_Click;
            listCard.Controls.Add(writeButton);

            Load += async (s, e) => await LoadDiariesAsync();

            ResumeLayout(false);
        }

        private async Task LoadDiariesAsync()
        {
            SetLoadingState(true, "正在加载情绪画像...");

            (List<Diary> diaries, UserEmotionProfile profile) result = await Task.Run(() =>
            {
                List<Diary> diaries = _currentUser == null ? new List<Diary>() : _diaryService.GetUserDiaries(_currentUser.UserId);
                UserEmotionProfile profile = _emotionDetectionService.AnalyzeUserEmotionProfile(_currentUser, diaries);
                return (diaries, profile);
            });

            _diaries = result.diaries;
            _profile = result.profile;

            _statsLabel.Text = $"总情绪指数 {_profile.OverallEmotionIndex} / 100    风险等级 {_profile.RiskLevel}    日记 {_profile.DiaryCount} 篇";
            _summaryLabel.Text = _profile.Summary;
            _actionLabel.Text = $"建议：{_profile.SuggestedAction}";

            _grid.Rows.Clear();
            foreach (Diary diary in _diaries)
            {
                string preview = diary.Content.Length > 70 ? diary.Content.Substring(0, 70) + "..." : diary.Content;
                string risk = diary.IsHighRisk ? "高风险" : "正常";
                int rowIndex = _grid.Rows.Add(diary.DiaryId, diary.CreatedAt.ToString("yyyy-MM-dd HH:mm"), diary.EmotionIndex, risk, preview);

                if (diary.IsHighRisk)
                {
                    _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 241, 237);
                    _grid.Rows[rowIndex].DefaultCellStyle.ForeColor = UiTheme.Danger;
                }
            }

            SetLoadingState(false, string.Empty);
        }

        private void DetailButton_Click(object sender, System.EventArgs e)
        {
            Diary diary = GetSelectedDiary();
            if (diary == null)
            {
                return;
            }

            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryDetail(diary);
            }
            else
            {
                new DiaryDetailView(diary, _currentUser, _mainWindow).Show();
                Hide();
            }
        }

        private void DeleteButton_Click(object sender, System.EventArgs e)
        {
            Diary diary = GetSelectedDiary();
            if (diary == null)
            {
                return;
            }

            DialogResult result = MessageBox.Show("确认删除这篇日记？删除后不可恢复。", "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return;
            }

            if (_diaryService.DeleteDiary(diary.DiaryId))
            {
                _ = LoadDiariesAsync();
            }
            else
            {
                MessageBox.Show("删除失败，请重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ContinueWriteButton_Click(object sender, System.EventArgs e)
        {
            Diary diary = GetSelectedDiary();
            if (diary == null)
            {
                return;
            }

            NavigateToDiaryEdit(diary);
        }

        private Diary GetSelectedDiary()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一篇日记。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            int diaryId = (int)_grid.SelectedRows[0].Cells["DiaryId"].Value;
            return _diaries.FirstOrDefault(d => d.DiaryId == diaryId);
        }

        private void NavigateToDiaryEdit(Diary diary)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryEdit(diary);
            }
            else
            {
                new DiaryEditView(_currentUser, null, diary).Show();
                Hide();
            }
        }

        private void SetLoadingState(bool isLoading, string message)
        {
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
            UseWaitCursor = isLoading;
            _loadingLabel.Text = message;
        }
    }
}
