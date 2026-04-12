using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Login screen.
    /// </summary>
    public partial class LoginView : Form
    {
        private readonly UserService _userService;
        private readonly MainWindow _mainWindow;
        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;
        private Label _errorLabel;

        public LoginView(MainWindow mainWindow = null)
        {
            _userService = new UserService();
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel heroPanel = UiTheme.CreateCard(70, 60, 500, 620);
            heroPanel.BackColor = UiTheme.Secondary;
            Controls.Add(heroPanel);

            Label appTitle = new Label
            {
                Text = "匿名情绪日记",
                Font = UiTheme.TitleFont(28),
                ForeColor = Color.White,
                Location = new Point(36, 56),
                Size = new Size(260, 40)
            };
            heroPanel.Controls.Add(appTitle);

            Label heroSubtitle = new Label
            {
                Text = "记录每日心情，自动完成单篇情绪识别与全量笔记聚合分析。",
                Font = UiTheme.BodyFont(12),
                ForeColor = Color.FromArgb(233, 241, 245),
                Location = new Point(36, 118),
                Size = new Size(410, 80)
            };
            heroPanel.Controls.Add(heroSubtitle);

            Label feature1 = CreateHeroFeature("AI 识别", "支持关键词与本地大模型双重分析。", 230);
            Label feature2 = CreateHeroFeature("风险预警", "高风险情绪自动提醒并提供支持建议。", 320);
            Label feature3 = CreateHeroFeature("管理员监控", "管理员可查看全量情绪画像并记录联系情况。", 410);
            heroPanel.Controls.Add(feature1);
            heroPanel.Controls.Add(feature2);
            heroPanel.Controls.Add(feature3);

            Panel loginCard = UiTheme.CreateCard(640, 100, 480, 540);
            Controls.Add(loginCard);

            Label titleLabel = new Label
            {
                Text = "欢迎回来",
                Font = UiTheme.TitleFont(24),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(42, 38),
                Size = new Size(220, 40)
            };
            loginCard.Controls.Add(titleLabel);

            Label tipLabel = new Label
            {
                Text = "默认管理员账号：admin  密码：Admin@12345，仅用于首次答辩演示。",
                Font = UiTheme.BodyFont(9.5f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(42, 86),
                Size = new Size(380, 32)
            };
            loginCard.Controls.Add(tipLabel);

            loginCard.Controls.Add(CreateInputLabel("用户名", 42, 150));
            _usernameTextBox = new TextBox { Location = new Point(42, 180), Size = new Size(390, 34) };
            UiTheme.StyleTextBox(_usernameTextBox);
            loginCard.Controls.Add(_usernameTextBox);

            loginCard.Controls.Add(CreateInputLabel("密码", 42, 240));
            _passwordTextBox = new TextBox
            {
                Location = new Point(42, 270),
                Size = new Size(390, 34),
                UseSystemPasswordChar = true
            };
            UiTheme.StyleTextBox(_passwordTextBox);
            loginCard.Controls.Add(_passwordTextBox);

            _errorLabel = new Label
            {
                ForeColor = UiTheme.Danger,
                Font = UiTheme.BodyFont(10),
                Location = new Point(42, 322),
                Size = new Size(390, 50)
            };
            loginCard.Controls.Add(_errorLabel);

            Button loginButton = new Button
            {
                Text = "登录系统",
                Location = new Point(42, 392),
                Size = new Size(390, 44)
            };
            UiTheme.StylePrimaryButton(loginButton);
            loginButton.Click += LoginButton_Click;
            loginCard.Controls.Add(loginButton);

            Button registerButton = new Button
            {
                Text = "创建新用户",
                Location = new Point(42, 454),
                Size = new Size(390, 44)
            };
            UiTheme.StyleSecondaryButton(registerButton);
            registerButton.Click += RegisterButton_Click;
            loginCard.Controls.Add(registerButton);

            ResumeLayout(false);
        }

        private static Label CreateHeroFeature(string title, string content, int top)
        {
            return new Label
            {
                Text = $"{title}\n{content}",
                Font = UiTheme.BodyFont(11.5f),
                ForeColor = Color.White,
                Location = new Point(36, top),
                Size = new Size(420, 64)
            };
        }

        private static Label CreateInputLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(left, top),
                Size = new Size(120, 22)
            };
        }

        private void LoginButton_Click(object sender, System.EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _errorLabel.Text = "请输入用户名和密码。";
                return;
            }

            User currentUser = _userService.LoginUser(username, password);
            if (currentUser == null)
            {
                _errorLabel.Text = "用户名或密码错误。";
                return;
            }

            _errorLabel.Text = string.Empty;

            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryList(currentUser);
            }
            else
            {
                new DiaryListView(currentUser).Show();
                Hide();
            }
        }

        private void RegisterButton_Click(object sender, System.EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToRegister();
            }
            else
            {
                new RegisterView().Show();
                Hide();
            }
        }
    }
}
