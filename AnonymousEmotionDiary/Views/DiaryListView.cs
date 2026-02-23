using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Diary list view for displaying user's diary entries.
    /// Shows diary summaries, emotion indices, and high-risk indicators.
    /// Allows users to view details, delete, and create new diaries.
    /// </summary>
    public partial class DiaryListView : Form
    {
        private readonly DiaryService _diaryService;
        private readonly User _currentUser;
        private readonly MainWindow _mainWindow;
        private List<Diary> _diaries;
        private const int SummaryLength = 100;

        public DiaryListView(User currentUser, MainWindow mainWindow = null)
        {
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _diaryService = new DiaryService();
            _diaries = new List<Diary>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            string username = _currentUser?.Username ?? "Unknown";
            this.Text = $"Anonymous Emotion Diary - Diary List ({username})";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "My Diaries";
            titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Size = new System.Drawing.Size(200, 30);
            this.Controls.Add(titleLabel);

            // Diary count label
            Label countLabel = new Label();
            countLabel.Name = "CountLabel";
            countLabel.Text = "Total: 0 diaries";
            countLabel.Font = new System.Drawing.Font("Arial", 10);
            countLabel.ForeColor = System.Drawing.Color.Gray;
            countLabel.Location = new System.Drawing.Point(600, 25);
            countLabel.Size = new System.Drawing.Size(150, 20);
            countLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Controls.Add(countLabel);

            // DataGridView for diary list
            DataGridView diaryGridView = new DataGridView();
            diaryGridView.Name = "DiaryGridView";
            diaryGridView.Location = new System.Drawing.Point(20, 60);
            diaryGridView.Size = new System.Drawing.Size(740, 400);
            diaryGridView.AllowUserToAddRows = false;
            diaryGridView.AllowUserToDeleteRows = false;
            diaryGridView.ReadOnly = true;
            diaryGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            diaryGridView.MultiSelect = false;
            diaryGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            diaryGridView.RowHeadersVisible = false;

            // Configure columns
            diaryGridView.Columns.Add("DiaryId", "ID");
            diaryGridView.Columns.Add("Summary", "Summary");
            diaryGridView.Columns.Add("CreatedAt", "Date");
            diaryGridView.Columns.Add("EmotionIndex", "Emotion");
            diaryGridView.Columns.Add("HighRisk", "Risk");

            // Set column widths
            diaryGridView.Columns["DiaryId"].Width = 50;
            diaryGridView.Columns["Summary"].Width = 350;
            diaryGridView.Columns["CreatedAt"].Width = 120;
            diaryGridView.Columns["EmotionIndex"].Width = 70;
            diaryGridView.Columns["HighRisk"].Width = 80;

            this.Controls.Add(diaryGridView);

            // Error message label
            Label errorLabel = new Label();
            errorLabel.Name = "ErrorLabel";
            errorLabel.Text = "";
            errorLabel.ForeColor = System.Drawing.Color.Red;
            errorLabel.Location = new System.Drawing.Point(20, 470);
            errorLabel.Size = new System.Drawing.Size(740, 30);
            errorLabel.AutoSize = false;
            this.Controls.Add(errorLabel);

            // View Details button
            Button viewButton = new Button();
            viewButton.Name = "ViewButton";
            viewButton.Text = "View Details";
            viewButton.Location = new System.Drawing.Point(200, 510);
            viewButton.Size = new System.Drawing.Size(100, 30);
            viewButton.Click += ViewButton_Click;
            this.Controls.Add(viewButton);

            // Delete button
            Button deleteButton = new Button();
            deleteButton.Name = "DeleteButton";
            deleteButton.Text = "Delete";
            deleteButton.Location = new System.Drawing.Point(320, 510);
            deleteButton.Size = new System.Drawing.Size(100, 30);
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            // New Diary button
            Button newButton = new Button();
            newButton.Name = "NewButton";
            newButton.Text = "New Diary";
            newButton.Location = new System.Drawing.Point(440, 510);
            newButton.Size = new System.Drawing.Size(100, 30);
            newButton.Click += NewButton_Click;
            this.Controls.Add(newButton);

            // Logout button
            Button logoutButton = new Button();
            logoutButton.Name = "LogoutButton";
            logoutButton.Text = "Logout";
            logoutButton.Location = new System.Drawing.Point(660, 510);
            logoutButton.Size = new System.Drawing.Size(100, 30);
            logoutButton.Click += LogoutButton_Click;
            this.Controls.Add(logoutButton);

            this.ResumeLayout(false);

            // Load diaries when form loads
            this.Load += DiaryListView_Load;
        }

        private void DiaryListView_Load(object sender, EventArgs e)
        {
            LoadDiaries();
        }

        private void LoadDiaries()
        {
            try
            {
                if (_currentUser == null)
                {
                    return;
                }
                _diaries = _diaryService.GetUserDiaries(_currentUser.UserId);
                RefreshDiaryGrid();
            }
            catch (Exception ex)
            {
                Label errorLabel = (Label)this.Controls["ErrorLabel"];
                if (errorLabel != null)
                {
                    errorLabel.Text = $"Error loading diaries: {ex.Message}";
                }
            }
        }

        private void RefreshDiaryGrid()
        {
            DataGridView diaryGridView = (DataGridView)this.Controls["DiaryGridView"];
            Label countLabel = (Label)this.Controls["CountLabel"];

            diaryGridView.Rows.Clear();

            foreach (Diary diary in _diaries)
            {
                string summary = diary.Content.Length > SummaryLength
                    ? diary.Content.Substring(0, SummaryLength) + "..."
                    : diary.Content;

                string riskStatus = diary.IsHighRisk ? "⚠ HIGH RISK" : "Normal";
                System.Drawing.Color riskColor = diary.IsHighRisk
                    ? System.Drawing.Color.Red
                    : System.Drawing.Color.Black;

                int rowIndex = diaryGridView.Rows.Add(
                    diary.DiaryId,
                    summary,
                    diary.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    diary.EmotionIndex,
                    riskStatus
                );

                // Color high-risk rows
                if (diary.IsHighRisk)
                {
                    diaryGridView.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightPink;
                    diaryGridView.Rows[rowIndex].DefaultCellStyle.ForeColor = System.Drawing.Color.DarkRed;
                }
            }

            countLabel.Text = $"Total: {_diaries.Count} diaries";
        }

        private void ViewButton_Click(object sender, EventArgs e)
        {
            DataGridView diaryGridView = (DataGridView)this.Controls["DiaryGridView"];
            Label errorLabel = (Label)this.Controls["ErrorLabel"];

            if (diaryGridView.SelectedRows.Count == 0)
            {
                errorLabel.Text = "Please select a diary to view.";
                return;
            }

            int diaryId = (int)diaryGridView.SelectedRows[0].Cells["DiaryId"].Value;
            Diary selectedDiary = _diaryService.GetDiaryById(diaryId);

            if (selectedDiary != null)
            {
                if (_mainWindow != null)
                {
                    _mainWindow.NavigateToDiaryDetail(selectedDiary);
                }
                else
                {
                    ShowDiaryDetail(selectedDiary);
                }
            }
            else
            {
                errorLabel.Text = "Failed to load diary details.";
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DataGridView diaryGridView = (DataGridView)this.Controls["DiaryGridView"];
            Label errorLabel = (Label)this.Controls["ErrorLabel"];

            if (diaryGridView.SelectedRows.Count == 0)
            {
                errorLabel.Text = "Please select a diary to delete.";
                return;
            }

            int diaryId = (int)diaryGridView.SelectedRows[0].Cells["DiaryId"].Value;

            // Confirm deletion
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this diary? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                if (_diaryService.DeleteDiary(diaryId))
                {
                    errorLabel.Text = "";
                    MessageBox.Show("Diary deleted successfully.", "Success");
                    LoadDiaries();
                }
                else
                {
                    errorLabel.Text = "Failed to delete diary.";
                }
            }
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryEdit();
            }
            else
            {
                DiaryEditView editView = new DiaryEditView(_currentUser);
                editView.FormClosed += (s, args) =>
                {
                    LoadDiaries();
                };
                editView.Show();
                this.Hide();
            }
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.HandleLogout();
            }
            else
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Confirm Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    LoginView loginView = new LoginView();
                    loginView.Show();
                    this.Close();
                }
            }
        }

        private void ShowDiaryDetail(Diary diary)
        {
            string riskWarning = diary.IsHighRisk
                ? "\n\n⚠ HIGH RISK EMOTION DETECTED\nPlease consider reaching out for support."
                : "";

            string detailMessage = $"Diary ID: {diary.DiaryId}\n" +
                                  $"Date: {diary.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"Emotion Index: {diary.EmotionIndex}\n" +
                                  $"---\n" +
                                  $"{diary.Content}" +
                                  riskWarning;

            MessageBox.Show(detailMessage, "Diary Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
