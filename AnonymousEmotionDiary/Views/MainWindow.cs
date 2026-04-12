using System.Drawing;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Main application shell.
    /// </summary>
    public partial class MainWindow : Form
    {
        private Panel _contentPanel;
        private User _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            _currentUser = null;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "匿名情绪日记系统";
            Width = 1280;
            Height = 860;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 760);
            BackColor = UiTheme.Background;

            _contentPanel = new Panel
            {
                Name = "ContentPanel",
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(24)
            };

            Controls.Add(_contentPanel);
            Load += MainWindow_Load;

            ResumeLayout(false);
        }

        private void MainWindow_Load(object sender, System.EventArgs e)
        {
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
            _currentUser = null;
            ClearContentPanel();
            AddViewToPanel(new LoginView(this));
        }

        public void NavigateToRegister()
        {
            ClearContentPanel();
            AddViewToPanel(new RegisterView(this));
        }

        public void NavigateToDiaryList(User user)
        {
            _currentUser = user;
            ClearContentPanel();

            Form nextView = user != null && user.IsAdmin
                ? new AdminDashboardView(user, this)
                : new DiaryListView(user, this);

            AddViewToPanel(nextView);
        }

        public void NavigateToDiaryEdit(Diary diary = null)
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            AddViewToPanel(new DiaryEditView(_currentUser, this, diary));
        }

        public void NavigateToDiaryDetail(Diary diary)
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            AddViewToPanel(new DiaryDetailView(diary, _currentUser, this));
        }

        public void NavigateToHighRiskWarning(Diary diary)
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            AddViewToPanel(new HighRiskWarningView(diary, _currentUser, this));
        }

        public void HandleLogout()
        {
            DialogResult result = MessageBox.Show(
                "确认退出当前账号？",
                "退出登录",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _currentUser = null;
                NavigateToLogin();
            }
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        private void ClearContentPanel()
        {
            _contentPanel.Controls.Clear();
        }

        private void AddViewToPanel(Form view)
        {
            view.TopLevel = false;
            view.FormBorderStyle = FormBorderStyle.None;
            view.Dock = DockStyle.Fill;
            view.BackColor = UiTheme.Background;
            _contentPanel.Controls.Add(view);
            view.Show();
        }
    }
}
