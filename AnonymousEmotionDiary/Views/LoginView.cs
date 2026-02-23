using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Login view for user authentication.
    /// Allows users to enter credentials and login or navigate to registration.
    /// </summary>
    public partial class LoginView : Form
    {
        private readonly UserService _userService;
        private User _currentUser;
        private readonly MainWindow _mainWindow;

        public LoginView(MainWindow mainWindow = null)
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUser = null;
            _mainWindow = mainWindow;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Anonymous Emotion Diary - Login";
            this.Width = 400;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "Login";
            titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(150, 20);
            titleLabel.Size = new System.Drawing.Size(100, 30);
            this.Controls.Add(titleLabel);

            // Username label
            Label usernameLabel = new Label();
            usernameLabel.Text = "Username:";
            usernameLabel.Location = new System.Drawing.Point(30, 70);
            usernameLabel.Size = new System.Drawing.Size(80, 20);
            this.Controls.Add(usernameLabel);

            // Username textbox
            TextBox usernameTextBox = new TextBox();
            usernameTextBox.Name = "UsernameTextBox";
            usernameTextBox.Location = new System.Drawing.Point(120, 70);
            usernameTextBox.Size = new System.Drawing.Size(230, 20);
            this.Controls.Add(usernameTextBox);

            // Password label
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new System.Drawing.Point(30, 110);
            passwordLabel.Size = new System.Drawing.Size(80, 20);
            this.Controls.Add(passwordLabel);

            // Password textbox
            TextBox passwordTextBox = new TextBox();
            passwordTextBox.Name = "PasswordTextBox";
            passwordTextBox.Location = new System.Drawing.Point(120, 110);
            passwordTextBox.Size = new System.Drawing.Size(230, 20);
            passwordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(passwordTextBox);

            // Error message label
            Label errorLabel = new Label();
            errorLabel.Name = "ErrorLabel";
            errorLabel.Text = "";
            errorLabel.ForeColor = System.Drawing.Color.Red;
            errorLabel.Location = new System.Drawing.Point(30, 150);
            errorLabel.Size = new System.Drawing.Size(320, 40);
            errorLabel.AutoSize = false;
            this.Controls.Add(errorLabel);

            // Login button
            Button loginButton = new Button();
            loginButton.Name = "LoginButton";
            loginButton.Text = "Login";
            loginButton.Location = new System.Drawing.Point(120, 200);
            loginButton.Size = new System.Drawing.Size(100, 30);
            loginButton.Click += LoginButton_Click;
            this.Controls.Add(loginButton);

            // Register button
            Button registerButton = new Button();
            registerButton.Name = "RegisterButton";
            registerButton.Text = "Register";
            registerButton.Location = new System.Drawing.Point(250, 200);
            registerButton.Size = new System.Drawing.Size(100, 30);
            registerButton.Click += RegisterButton_Click;
            this.Controls.Add(registerButton);

            this.ResumeLayout(false);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            TextBox usernameTextBox = (TextBox)this.Controls["UsernameTextBox"];
            TextBox passwordTextBox = (TextBox)this.Controls["PasswordTextBox"];
            Label errorLabel = (Label)this.Controls["ErrorLabel"];

            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                errorLabel.Text = "Please enter both username and password.";
                return;
            }

            // Attempt login
            _currentUser = _userService.LoginUser(username, password);

            if (_currentUser != null)
            {
                errorLabel.Text = "";
                // Navigate to diary list view
                NavigateToDiaryList();
            }
            else
            {
                errorLabel.Text = "Invalid username or password.";
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            // Navigate to register view
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToRegister();
            }
            else
            {
                RegisterView registerView = new RegisterView();
                registerView.Show();
                this.Hide();
            }
        }

        private void NavigateToDiaryList()
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDiaryList(_currentUser);
            }
            else
            {
                DiaryListView diaryListView = new DiaryListView(_currentUser);
                diaryListView.Show();
                this.Hide();
            }
        }
    }
}
