using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary.Views
{
    /// <summary>
    /// Register view for user account creation.
    /// Allows users to create new accounts with username and password validation.
    /// </summary>
    public partial class RegisterView : Form
    {
        private readonly UserService _userService;
        private readonly MainWindow _mainWindow;

        public RegisterView(MainWindow mainWindow = null)
        {
            InitializeComponent();
            _userService = new UserService();
            _mainWindow = mainWindow;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Anonymous Emotion Diary - Register";
            this.Width = 400;
            this.Height = 380;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "Register";
            titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(140, 20);
            titleLabel.Size = new System.Drawing.Size(120, 30);
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

            // Username hint label
            Label usernameHintLabel = new Label();
            usernameHintLabel.Text = "(3-20 characters, alphanumeric and underscore only)";
            usernameHintLabel.Font = new System.Drawing.Font("Arial", 8);
            usernameHintLabel.ForeColor = System.Drawing.Color.Gray;
            usernameHintLabel.Location = new System.Drawing.Point(120, 90);
            usernameHintLabel.Size = new System.Drawing.Size(230, 15);
            this.Controls.Add(usernameHintLabel);

            // Password label
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new System.Drawing.Point(30, 120);
            passwordLabel.Size = new System.Drawing.Size(80, 20);
            this.Controls.Add(passwordLabel);

            // Password textbox
            TextBox passwordTextBox = new TextBox();
            passwordTextBox.Name = "PasswordTextBox";
            passwordTextBox.Location = new System.Drawing.Point(120, 120);
            passwordTextBox.Size = new System.Drawing.Size(230, 20);
            passwordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(passwordTextBox);

            // Password hint label
            Label passwordHintLabel = new Label();
            passwordHintLabel.Text = "(at least 8 characters)";
            passwordHintLabel.Font = new System.Drawing.Font("Arial", 8);
            passwordHintLabel.ForeColor = System.Drawing.Color.Gray;
            passwordHintLabel.Location = new System.Drawing.Point(120, 140);
            passwordHintLabel.Size = new System.Drawing.Size(230, 15);
            this.Controls.Add(passwordHintLabel);

            // Confirm password label
            Label confirmPasswordLabel = new Label();
            confirmPasswordLabel.Text = "Confirm Password:";
            confirmPasswordLabel.Location = new System.Drawing.Point(30, 170);
            confirmPasswordLabel.Size = new System.Drawing.Size(80, 20);
            this.Controls.Add(confirmPasswordLabel);

            // Confirm password textbox
            TextBox confirmPasswordTextBox = new TextBox();
            confirmPasswordTextBox.Name = "ConfirmPasswordTextBox";
            confirmPasswordTextBox.Location = new System.Drawing.Point(120, 170);
            confirmPasswordTextBox.Size = new System.Drawing.Size(230, 20);
            confirmPasswordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(confirmPasswordTextBox);

            // Error message label
            Label errorLabel = new Label();
            errorLabel.Name = "ErrorLabel";
            errorLabel.Text = "";
            errorLabel.ForeColor = System.Drawing.Color.Red;
            errorLabel.Location = new System.Drawing.Point(30, 210);
            errorLabel.Size = new System.Drawing.Size(320, 50);
            errorLabel.AutoSize = false;
            errorLabel.WordWrap = true;
            this.Controls.Add(errorLabel);

            // Register button
            Button registerButton = new Button();
            registerButton.Name = "RegisterButton";
            registerButton.Text = "Register";
            registerButton.Location = new System.Drawing.Point(120, 280);
            registerButton.Size = new System.Drawing.Size(100, 30);
            registerButton.Click += RegisterButton_Click;
            this.Controls.Add(registerButton);

            // Back button
            Button backButton = new Button();
            backButton.Name = "BackButton";
            backButton.Text = "Back";
            backButton.Location = new System.Drawing.Point(250, 280);
            backButton.Size = new System.Drawing.Size(100, 30);
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);

            this.ResumeLayout(false);
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            TextBox usernameTextBox = (TextBox)this.Controls["UsernameTextBox"];
            TextBox passwordTextBox = (TextBox)this.Controls["PasswordTextBox"];
            TextBox confirmPasswordTextBox = (TextBox)this.Controls["ConfirmPasswordTextBox"];
            Label errorLabel = (Label)this.Controls["ErrorLabel"];

            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;
            string confirmPassword = confirmPasswordTextBox.Text;

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                errorLabel.Text = "Please fill in all fields.";
                return;
            }

            // Validate username format
            if (!_userService.ValidateUsername(username))
            {
                errorLabel.Text = "Username must be 3-20 characters long and contain only alphanumeric characters and underscores.";
                return;
            }

            // Validate password format
            if (!_userService.ValidatePassword(password))
            {
                errorLabel.Text = "Password must be at least 8 characters long.";
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                errorLabel.Text = "Passwords do not match.";
                return;
            }

            // Attempt registration
            User newUser = _userService.RegisterUser(username, password);

            if (newUser != null)
            {
                errorLabel.Text = "";
                MessageBox.Show($"Registration successful! Welcome, {newUser.Username}. You can now login.", "Success");
                // Navigate back to login view
                NavigateToLogin();
            }
            else
            {
                errorLabel.Text = "Registration failed. Username may already exist or an error occurred.";
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            // Navigate back to login view
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
                LoginView loginView = new LoginView();
                loginView.Show();
                this.Close();
            }
        }
    }
}
