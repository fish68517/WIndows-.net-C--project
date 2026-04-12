using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Registration screen.
    /// </summary>
    public partial class RegisterView : Form
    {
        private readonly UserService _userService;
        private readonly MainWindow _mainWindow;
        private TextBox _usernameTextBox;
        private TextBox _contactTextBox;
        private TextBox _passwordTextBox;
        private TextBox _confirmPasswordTextBox;
        private Label _errorLabel;

        public RegisterView(MainWindow mainWindow = null)
        {
            _userService = new UserService();
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = UiTheme.Background;

            Panel registerCard = UiTheme.CreateCard(290, 60, 700, 640);
            Controls.Add(registerCard);

            Label titleLabel = new Label
            {
                Text = "创建匿名账号",
                Font = UiTheme.TitleFont(24),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(46, 34),
                Size = new Size(280, 40)
            };
            registerCard.Controls.Add(titleLabel);

            Label subtitleLabel = new Label
            {
                Text = "用户名用于登录；联系方式为可选项，仅在管理员发现高风险情绪时进行人工联系。",
                Font = UiTheme.BodyFont(10.5f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(46, 84),
                Size = new Size(570, 36)
            };
            registerCard.Controls.Add(subtitleLabel);

            registerCard.Controls.Add(CreateLabel("用户名", 46, 146));
            _usernameTextBox = new TextBox { Location = new Point(46, 176), Size = new Size(600, 34) };
            UiTheme.StyleTextBox(_usernameTextBox);
            registerCard.Controls.Add(_usernameTextBox);

            Label usernameHint = new Label
            {
                Text = "3-20 位，仅允许字母、数字和下划线",
                Font = UiTheme.BodyFont(9f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(46, 214),
                Size = new Size(280, 20)
            };
            registerCard.Controls.Add(usernameHint);

            registerCard.Controls.Add(CreateLabel("联系方式（可选）", 46, 256));
            _contactTextBox = new TextBox { Location = new Point(46, 286), Size = new Size(600, 34) };
            UiTheme.StyleTextBox(_contactTextBox);
            registerCard.Controls.Add(_contactTextBox);

            Label contactHint = new Label
            {
                Text = "可填写手机号、邮箱、QQ 或微信号，便于管理员在高风险情况下联系你",
                Font = UiTheme.BodyFont(9f),
                ForeColor = UiTheme.TextMuted,
                Location = new Point(46, 324),
                Size = new Size(500, 20)
            };
            registerCard.Controls.Add(contactHint);

            registerCard.Controls.Add(CreateLabel("密码", 46, 366));
            _passwordTextBox = new TextBox
            {
                Location = new Point(46, 396),
                Size = new Size(600, 34),
                UseSystemPasswordChar = true
            };
            UiTheme.StyleTextBox(_passwordTextBox);
            registerCard.Controls.Add(_passwordTextBox);

            registerCard.Controls.Add(CreateLabel("确认密码", 46, 456));
            _confirmPasswordTextBox = new TextBox
            {
                Location = new Point(46, 486),
                Size = new Size(600, 34),
                UseSystemPasswordChar = true
            };
            UiTheme.StyleTextBox(_confirmPasswordTextBox);
            registerCard.Controls.Add(_confirmPasswordTextBox);

            _errorLabel = new Label
            {
                ForeColor = UiTheme.Danger,
                Font = UiTheme.BodyFont(),
                Location = new Point(46, 534),
                Size = new Size(600, 40)
            };
            registerCard.Controls.Add(_errorLabel);

            Button registerButton = new Button
            {
                Text = "完成注册",
                Location = new Point(46, 580),
                Size = new Size(286, 40)
            };
            UiTheme.StylePrimaryButton(registerButton);
            registerButton.Click += RegisterButton_Click;
            registerCard.Controls.Add(registerButton);

            Button backButton = new Button
            {
                Text = "返回登录",
                Location = new Point(360, 580),
                Size = new Size(286, 40)
            };
            UiTheme.StyleSecondaryButton(backButton);
            backButton.Click += BackButton_Click;
            registerCard.Controls.Add(backButton);

            ResumeLayout(false);
        }

        private static Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Font = UiTheme.BodyFont(10.5f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(left, top),
                Size = new Size(180, 22)
            };
        }

        private void RegisterButton_Click(object sender, System.EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string contactInfo = _contactTextBox.Text.Trim();
            string password = _passwordTextBox.Text;
            string confirmPassword = _confirmPasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                _errorLabel.Text = "请完整填写用户名和密码。";
                return;
            }

            if (!_userService.ValidateUsername(username))
            {
                _errorLabel.Text = "用户名格式不合法。";
                return;
            }

            if (!_userService.ValidatePassword(password))
            {
                _errorLabel.Text = "密码至少需要 8 位。";
                return;
            }

            if (password != confirmPassword)
            {
                _errorLabel.Text = "两次输入的密码不一致。";
                return;
            }

            User newUser = _userService.RegisterUser(username, password, contactInfo);
            if (newUser == null)
            {
                _errorLabel.Text = "注册失败，用户名可能已存在。";
                return;
            }

            MessageBox.Show("注册成功，现可返回登录。", "注册成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NavigateToLogin();
        }

        private void BackButton_Click(object sender, System.EventArgs e)
        {
            NavigateToLogin();
        }

        private void NavigateToLogin()
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToLogin();
            }
            else
            {
                new LoginView().Show();
                Close();
            }
        }
    }
}
