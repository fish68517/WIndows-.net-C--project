using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Dialog for recording administrator follow-up.
    /// </summary>
    public class AdminContactDialog : Form
    {
        private ComboBox _methodComboBox;
        private TextBox _noteTextBox;

        public string ContactMethod => _methodComboBox.SelectedItem?.ToString() ?? "人工联系";

        public string ContactNote => _noteTextBox.Text.Trim();

        public AdminContactDialog(User targetUser, Models.UserEmotionProfile profile)
        {
            InitializeComponent(targetUser, profile);
        }

        private void InitializeComponent(User targetUser, Models.UserEmotionProfile profile)
        {
            SuspendLayout();

            Text = "记录管理员联系";
            Width = 560;
            Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UiTheme.Background;

            Panel card = UiTheme.CreateCard(20, 20, 500, 330);
            Controls.Add(card);

            Label title = new Label
            {
                Text = $"联系用户：{targetUser.Username}",
                Font = UiTheme.TitleFont(18),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(24, 20),
                Size = new Size(300, 30)
            };
            card.Controls.Add(title);

            Label info = new Label
            {
                Text = $"综合情绪指数：{profile.OverallEmotionIndex}    风险等级：{profile.RiskLevel}\n联系方式：{(string.IsNullOrWhiteSpace(targetUser.ContactInfo) ? "未填写" : targetUser.ContactInfo)}",
                Font = UiTheme.BodyFont(10),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(24, 62),
                Size = new Size(440, 50)
            };
            card.Controls.Add(info);

            Label methodLabel = new Label
            {
                Text = "联系方式",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(24, 128),
                Size = new Size(120, 22)
            };
            card.Controls.Add(methodLabel);

            _methodComboBox = new ComboBox
            {
                Location = new Point(24, 156),
                Size = new Size(220, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.BodyFont()
            };
            _methodComboBox.Items.AddRange(new object[] { "电话", "短信", "邮件", "微信/QQ", "线下沟通", "人工联系" });
            _methodComboBox.SelectedIndex = 0;
            card.Controls.Add(_methodComboBox);

            Label noteLabel = new Label
            {
                Text = "联系记录",
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(24, 208),
                Size = new Size(120, 22)
            };
            card.Controls.Add(noteLabel);

            _noteTextBox = new TextBox
            {
                Location = new Point(24, 236),
                Size = new Size(448, 72),
                Multiline = true
            };
            UiTheme.StyleTextBox(_noteTextBox, true);
            _noteTextBox.Text = "已查看用户近期情绪画像，准备进行人工跟进。";
            card.Controls.Add(_noteTextBox);

            Button confirmButton = new Button
            {
                Text = "保存记录",
                DialogResult = DialogResult.OK,
                Location = new Point(132, 364),
                Size = new Size(120, 36)
            };
            UiTheme.StylePrimaryButton(confirmButton);
            Controls.Add(confirmButton);

            Button cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(286, 364),
                Size = new Size(120, 36)
            };
            UiTheme.StyleSecondaryButton(cancelButton);
            Controls.Add(cancelButton);

            AcceptButton = confirmButton;
            CancelButton = cancelButton;

            ResumeLayout(false);
        }
    }
}
