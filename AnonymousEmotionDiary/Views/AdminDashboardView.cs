using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Administrator dashboard for monitoring user emotion profiles.
    /// </summary>
    public class AdminDashboardView : Form
    {
        private readonly User _adminUser;
        private readonly MainWindow _mainWindow;
        private readonly AdminService _adminService;
        private readonly UserService _userService;
        private readonly DiaryService _diaryService;
        private readonly Dictionary<int, User> _usersById;
        private List<UserEmotionProfile> _profiles;
        private Label _summaryLabel;
        private DataGridView _grid;
        private Label _loadingLabel;

        public AdminDashboardView(User adminUser, MainWindow mainWindow = null)
        {
            _adminUser = adminUser;
            _mainWindow = mainWindow;
            _adminService = new AdminService();
            _userService = new UserService();
            _diaryService = new DiaryService();
            _usersById = _userService.GetAllRegularUsers().ToDictionary(u => u.UserId, u => u);
            _profiles = new List<UserEmotionProfile>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel heroCard = UiTheme.CreateCard(24, 24, 1180, 150);
            heroCard.BackColor = UiTheme.Secondary;
            Controls.Add(heroCard);

            heroCard.Controls.Add(new Label
            {
                Text = "管理员情绪监控台",
                Font = UiTheme.TitleFont(26),
                ForeColor = Color.White,
                Location = new Point(28, 28),
                Size = new Size(320, 40)
            });

            heroCard.Controls.Add(new Label
            {
                Text = $"当前账号：{_adminUser.Username}    可查看用户总情绪指数、风险等级和管理员联系记录。",
                Font = UiTheme.BodyFont(11),
                ForeColor = Color.FromArgb(233, 241, 245),
                Location = new Point(28, 78),
                Size = new Size(720, 26)
            });

            _summaryLabel = new Label
            {
                Font = UiTheme.BodyFont(11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(28, 108),
                Size = new Size(800, 24)
            };
            heroCard.Controls.Add(_summaryLabel);

            _loadingLabel = new Label
            {
                Font = UiTheme.BodyFont(9.5f),
                ForeColor = Color.FromArgb(233, 241, 245),
                Location = new Point(850, 114),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            heroCard.Controls.Add(_loadingLabel);

            Button refreshButton = new Button { Text = "刷新", Location = new Point(930, 52), Size = new Size(100, 40) };
            UiTheme.StylePrimaryButton(refreshButton);
            refreshButton.Click += async (s, e) => await LoadProfilesAsync();
            heroCard.Controls.Add(refreshButton);

            Button logoutButton = new Button { Text = "退出登录", Location = new Point(1046, 52), Size = new Size(100, 40) };
            UiTheme.StyleSecondaryButton(logoutButton);
            logoutButton.Click += (s, e) => _mainWindow?.HandleLogout();
            heroCard.Controls.Add(logoutButton);

            Panel tableCard = UiTheme.CreateCard(24, 196, 1180, 520);
            Controls.Add(tableCard);

            tableCard.Controls.Add(new Label
            {
                Text = "用户情绪总览",
                Font = UiTheme.TitleFont(18),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(24, 20),
                Size = new Size(220, 30)
            });

            _grid = new DataGridView
            {
                Location = new Point(24, 66),
                Size = new Size(1128, 382),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            UiTheme.StyleGrid(_grid);
            _grid.Columns.Add("UserId", "ID");
            _grid.Columns.Add("Username", "用户名");
            _grid.Columns.Add("ContactInfo", "联系方式");
            _grid.Columns.Add("DiaryCount", "日记数");
            _grid.Columns.Add("Average", "平均指数");
            _grid.Columns.Add("Overall", "总情绪指数");
            _grid.Columns.Add("RiskLevel", "风险等级");
            _grid.Columns.Add("LastDiaryAt", "最后一篇日记");
            _grid.Columns.Add("LastContactAt", "最后联系");
            tableCard.Controls.Add(_grid);

            Button reportButton = new Button { Text = "查看报告", Location = new Point(708, 462), Size = new Size(130, 36) };
            UiTheme.StyleSecondaryButton(reportButton);
            reportButton.Click += ViewReportButton_Click;
            tableCard.Controls.Add(reportButton);

            Button contactButton = new Button { Text = "联系用户", Location = new Point(854, 462), Size = new Size(130, 36) };
            UiTheme.StylePrimaryButton(contactButton);
            contactButton.Click += ContactButton_Click;
            tableCard.Controls.Add(contactButton);

            Button viewDiariesButton = new Button { Text = "查看原始日记", Location = new Point(1000, 462), Size = new Size(130, 36) };
            UiTheme.StyleSecondaryButton(viewDiariesButton);
            viewDiariesButton.Click += ViewDiariesButton_Click;
            tableCard.Controls.Add(viewDiariesButton);

            Load += async (s, e) => await LoadProfilesAsync();

            ResumeLayout(false);
        }

        private async Task LoadProfilesAsync()
        {
            SetLoadingState(true, "正在分析所有用户...");
            _profiles = await Task.Run(() => _adminService.GetAllUserEmotionProfiles());
            _grid.Rows.Clear();

            foreach (UserEmotionProfile profile in _profiles)
            {
                int rowIndex = _grid.Rows.Add(
                    profile.UserId,
                    profile.Username,
                    string.IsNullOrWhiteSpace(profile.ContactInfo) ? "未填写" : profile.ContactInfo,
                    profile.DiaryCount,
                    profile.AverageEmotionIndex,
                    profile.OverallEmotionIndex,
                    profile.RiskLevel,
                    profile.LastDiaryAt?.ToString("yyyy-MM-dd HH:mm") ?? "-",
                    profile.LastContactAt?.ToString("yyyy-MM-dd HH:mm") ?? "未联系");

                if (profile.RiskLevel == "极高" || profile.RiskLevel == "高")
                {
                    _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 236, 232);
                    _grid.Rows[rowIndex].DefaultCellStyle.ForeColor = UiTheme.Danger;
                }
            }

            int highRiskCount = _profiles.Count(p => p.RiskLevel == "高" || p.RiskLevel == "极高");
            _summaryLabel.Text = $"已分析 {_profiles.Count} 名用户，当前高风险用户 {highRiskCount} 名，建议优先处理总情绪指数最高的用户。";
            SetLoadingState(false, string.Empty);
        }

        private void ViewReportButton_Click(object sender, System.EventArgs e)
        {
            UserEmotionProfile profile = GetSelectedProfile();
            if (profile == null)
            {
                return;
            }

            List<AdminContactRecord> history = _adminService.GetContactHistory(profile.UserId);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"用户：{profile.Username}");
            builder.AppendLine($"联系方式：{(string.IsNullOrWhiteSpace(profile.ContactInfo) ? "未填写" : profile.ContactInfo)}");
            builder.AppendLine($"总情绪指数：{profile.OverallEmotionIndex}");
            builder.AppendLine($"平均情绪指数：{profile.AverageEmotionIndex}");
            builder.AppendLine($"最近一次情绪指数：{profile.LatestEmotionIndex}");
            builder.AppendLine($"高风险日记数：{profile.HighRiskCount}");
            builder.AppendLine($"风险等级：{profile.RiskLevel}");
            builder.AppendLine($"分析摘要：{profile.Summary}");
            builder.AppendLine($"建议动作：{profile.SuggestedAction}");
            builder.AppendLine();
            builder.AppendLine("管理员联系记录：");

            if (history.Count == 0)
            {
                builder.AppendLine("暂无联系记录。");
            }
            else
            {
                foreach (AdminContactRecord record in history.Take(5))
                {
                    builder.AppendLine($"- {record.CreatedAt:yyyy-MM-dd HH:mm} | {record.ContactMethod} | {record.ContactNote}");
                }
            }

            MessageBox.Show(builder.ToString(), "用户情绪报告", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ContactButton_Click(object sender, System.EventArgs e)
        {
            UserEmotionProfile profile = GetSelectedProfile();
            if (profile == null || !_usersById.TryGetValue(profile.UserId, out User targetUser))
            {
                MessageBox.Show("未找到目标用户。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using AdminContactDialog dialog = new AdminContactDialog(targetUser, profile);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            bool success = _adminService.RecordContact(_adminUser, targetUser, profile.OverallEmotionIndex, dialog.ContactMethod, dialog.ContactNote);
            if (success)
            {
                MessageBox.Show("管理员联系记录已保存。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _ = LoadProfilesAsync();
            }
            else
            {
                MessageBox.Show("保存联系记录失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewDiariesButton_Click(object sender, System.EventArgs e)
        {
            UserEmotionProfile profile = GetSelectedProfile();
            if (profile == null)
            {
                return;
            }

            List<Diary> diaries = _diaryService.GetUserDiaries(profile.UserId);
            if (diaries.Count == 0)
            {
                MessageBox.Show("该用户暂无日记。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StringBuilder builder = new StringBuilder();
            foreach (Diary diary in diaries.Take(8))
            {
                string preview = diary.Content.Length > 80 ? diary.Content.Substring(0, 80) + "..." : diary.Content;
                builder.AppendLine($"{diary.CreatedAt:yyyy-MM-dd HH:mm} | 情绪={diary.EmotionIndex} | 高风险={diary.IsHighRisk}");
                builder.AppendLine(preview);
                builder.AppendLine();
            }

            MessageBox.Show(builder.ToString(), $"用户 {profile.Username} 的最近日记", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private UserEmotionProfile GetSelectedProfile()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择用户。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            int userId = (int)_grid.SelectedRows[0].Cells["UserId"].Value;
            return _profiles.FirstOrDefault(p => p.UserId == userId);
        }

        private void SetLoadingState(bool isLoading, string message)
        {
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
            UseWaitCursor = isLoading;
            _loadingLabel.Text = message;
        }
    }
}
