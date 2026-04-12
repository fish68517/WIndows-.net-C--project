using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Diary editor for creating a new diary or continuing an existing one.
    /// </summary>
    public partial class DiaryEditView : Form
    {
        private readonly DiaryService _diaryService;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;
        private readonly Diary _editingDiary;
        private TextBox _contentTextBox;
        private Label _charCountLabel;
        private Label _errorLabel;
        private Button _backButton;
        private Button _publishButton;

        public DiaryEditView(User currentUser, MainWindow mainWindow = null, Diary diaryToEdit = null)
        {
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _editingDiary = diaryToEdit;
            _diaryService = new DiaryService();
            InitializeComponent();
        }

        private bool IsEditingExistingDiary => _editingDiary != null;

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel editorCard = UiTheme.CreateCard(120, 40, 980, 660);
            Controls.Add(editorCard);

            editorCard.Controls.Add(new Label
            {
                Text = IsEditingExistingDiary ? "继续书写这篇日记" : "写下今天的心情",
                Font = UiTheme.TitleFont(24),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(34, 24),
                Size = new Size(320, 38)
            });

            editorCard.Controls.Add(new Label
            {
                Text = IsEditingExistingDiary
                    ? "已自动带入原日记内容，你可以继续补充、修改，保存后会重新计算情绪指数。"
                    : "系统会在发布后自动完成单篇情绪分析，并将本次内容纳入你的全量情绪画像。",
                Font = UiTheme.BodyFont(10.5f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(34, 68),
                Size = new Size(760, 24)
            });

            _charCountLabel = new Label
            {
                Text = $"0 / {ConfigurationHelper.DiaryContentMaxLength}",
                Font = UiTheme.BodyFont(10f, FontStyle.Bold),
                ForeColor = UiTheme.Secondary,
                Location = new Point(820, 68),
                Size = new Size(120, 24),
                TextAlign = ContentAlignment.MiddleRight
            };
            editorCard.Controls.Add(_charCountLabel);

            _contentTextBox = new TextBox
            {
                Location = new Point(34, 116),
                Size = new Size(910, 430),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = _editingDiary?.Content ?? string.Empty
            };
            UiTheme.StyleTextBox(_contentTextBox, true);
            _contentTextBox.TextChanged += ContentTextBox_TextChanged;
            editorCard.Controls.Add(_contentTextBox);

            _errorLabel = new Label
            {
                ForeColor = UiTheme.Danger,
                Font = UiTheme.BodyFont(),
                Location = new Point(34, 560),
                Size = new Size(760, 24)
            };
            editorCard.Controls.Add(_errorLabel);

            _backButton = new Button { Text = "返回", Location = new Point(676, 596), Size = new Size(120, 40) };
            UiTheme.StyleSecondaryButton(_backButton);
            _backButton.Click += async (s, e) => await NavigateBackAsync();
            editorCard.Controls.Add(_backButton);

            _publishButton = new Button
            {
                Text = IsEditingExistingDiary ? "保存续写并重算" : "发布并分析",
                Location = new Point(824, 596),
                Size = new Size(120, 40)
            };
            UiTheme.StylePrimaryButton(_publishButton);
            _publishButton.Click += PublishButton_Click;
            editorCard.Controls.Add(_publishButton);

            ContentTextBox_TextChanged(this, System.EventArgs.Empty);
            ResumeLayout(false);
        }

        private void ContentTextBox_TextChanged(object sender, System.EventArgs e)
        {
            int currentLength = _contentTextBox.Text.Length;
            _charCountLabel.Text = $"{currentLength} / {ConfigurationHelper.DiaryContentMaxLength}";

            if (currentLength > ConfigurationHelper.DiaryContentMaxLength)
            {
                _charCountLabel.ForeColor = UiTheme.Danger;
            }
            else if (currentLength > ConfigurationHelper.DiaryContentMaxLength - 500)
            {
                _charCountLabel.ForeColor = UiTheme.Accent;
            }
            else
            {
                _charCountLabel.ForeColor = UiTheme.Secondary;
            }
        }

        private async void PublishButton_Click(object sender, System.EventArgs e)
        {
            string content = _contentTextBox.Text.Trim();
            if (!_diaryService.ValidateDiaryContent(content))
            {
                _errorLabel.Text = "日记内容不能为空，且不能超过 5000 字。";
                return;
            }

            SetBusyState(true, IsEditingExistingDiary ? "正在保存续写内容并重新分析..." : "正在调用模型分析并保存日记，请稍候...");

            Diary resultDiary = IsEditingExistingDiary
                ? await Task.Run(() => _diaryService.UpdateDiary(_editingDiary.DiaryId, content))
                : await Task.Run(() => _diaryService.CreateDiary(_currentUser.UserId, content));

            SetBusyState(false, string.Empty);

            if (resultDiary == null)
            {
                _errorLabel.Text = IsEditingExistingDiary ? "保存失败，请稍后重试。" : "发布失败，请稍后重试。";
                return;
            }

            string successMessage = IsEditingExistingDiary
                ? $"续写保存成功，新的情绪指数为 {resultDiary.EmotionIndex}。"
                : $"发布成功，情绪指数为 {resultDiary.EmotionIndex}。";
            MessageBox.Show(successMessage, "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (resultDiary.IsHighRisk)
            {
                if (_mainWindow != null)
                {
                    _mainWindow.NavigateToHighRiskWarning(resultDiary);
                }
                else
                {
                    new HighRiskWarningView(resultDiary, _currentUser).ShowDialog();
                }
                return;
            }

            await NavigateBackAsync();
        }

        private async Task NavigateBackAsync()
        {
            SetBusyState(true, "正在加载日记列表和情绪画像...");

            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryList(_currentUser);
            }
            else
            {
                new DiaryListView(_currentUser).Show();
                Close();
            }

            await Task.Yield();
            SetBusyState(false, string.Empty);
        }

        private void SetBusyState(bool isBusy, string message)
        {
            Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
            UseWaitCursor = isBusy;
            if (_publishButton != null) _publishButton.Enabled = !isBusy;
            if (_backButton != null) _backButton.Enabled = !isBusy;
            _errorLabel.Text = message;
        }
    }
}
