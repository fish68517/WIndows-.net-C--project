using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Main application window that serves as a container for all views.
    /// Manages navigation between different screens (Login, Register, Diary List, Diary Edit, Diary Detail).
    /// Provides centralized control flow for the entire application.
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
            this.SuspendLayout();

            // Form properties
            this.Text = "Anonymous Emotion Diary";
            this.Width = 900;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Content panel to hold different views
            _contentPanel = new Panel();
            _contentPanel.Name = "ContentPanel";
            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Size = new System.Drawing.Size(900, 700);
            _contentPanel.Dock = DockStyle.Fill;
            this.Controls.Add(_contentPanel);

            this.ResumeLayout(false);

            // Load login view on startup
            this.Load += MainWindow_Load;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            NavigateToLogin();
        }

        /// <summary>
        /// Navigates to the login view.
        /// </summary>
        public void NavigateToLogin()
        {
            _currentUser = null;
            ClearContentPanel();
            LoginView loginView = new LoginView(this);
            AddViewToPanel(loginView);
        }

        /// <summary>
        /// Navigates to the register view.
        /// </summary>
        public void NavigateToRegister()
        {
            ClearContentPanel();
            RegisterView registerView = new RegisterView(this);
            AddViewToPanel(registerView);
        }

        /// <summary>
        /// Navigates to the diary list view.
        /// </summary>
        /// <param name="user">The currently logged-in user.</param>
        public void NavigateToDiaryList(User user)
        {
            _currentUser = user;
            ClearContentPanel();
            DiaryListView diaryListView = new DiaryListView(user, this);
            AddViewToPanel(diaryListView);
        }

        /// <summary>
        /// Navigates to the diary edit view for creating a new diary.
        /// </summary>
        public void NavigateToDiaryEdit()
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            DiaryEditView editView = new DiaryEditView(_currentUser, this);
            AddViewToPanel(editView);
        }

        /// <summary>
        /// Navigates to the diary detail view.
        /// </summary>
        /// <param name="diary">The diary to display.</param>
        public void NavigateToDiaryDetail(Diary diary)
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            DiaryDetailView detailView = new DiaryDetailView(diary, _currentUser, this);
            AddViewToPanel(detailView);
        }

        /// <summary>
        /// Navigates to the high-risk warning view.
        /// </summary>
        /// <param name="diary">The high-risk diary.</param>
        public void NavigateToHighRiskWarning(Diary diary)
        {
            if (_currentUser == null)
            {
                NavigateToLogin();
                return;
            }

            ClearContentPanel();
            HighRiskWarningView warningView = new HighRiskWarningView(diary, _currentUser, this);
            AddViewToPanel(warningView);
        }

        /// <summary>
        /// Handles user logout and returns to login view.
        /// </summary>
        public void HandleLogout()
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _currentUser = null;
                NavigateToLogin();
            }
        }

        /// <summary>
        /// Clears the content panel by removing all controls.
        /// </summary>
        private void ClearContentPanel()
        {
            _contentPanel.Controls.Clear();
        }

        /// <summary>
        /// Adds a view (Form) to the content panel.
        /// </summary>
        /// <param name="view">The view to add.</param>
        private void AddViewToPanel(Form view)
        {
            view.TopLevel = false;
            view.FormBorderStyle = FormBorderStyle.None;
            view.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(view);
            view.Show();
        }

        /// <summary>
        /// Gets the currently logged-in user.
        /// </summary>
        public User GetCurrentUser()
        {
            return _currentUser;
        }
    }
}
