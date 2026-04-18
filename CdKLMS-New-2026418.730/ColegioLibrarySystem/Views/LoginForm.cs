using System;
using System.Drawing;
using System.Windows.Forms;
using ColegioLibrarySystem.Helpers;
using ColegioLibrarySystem.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ColegioLibrarySystem.Views
{
    public partial class LoginForm : Form
    {
        private Panel glassPanel;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private DatabaseHelper dbHelper;

        public LoginForm()
        {
            dbHelper = new DatabaseHelper();
            InitializeModernUI();
        }

        private void InitializeModernUI()
        {
            this.Text = "Colegio de Kidapawan - LMS";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; 
            this.BackColor = Color.FromArgb(28, 28, 28); 

            glassPanel = new Panel();
            glassPanel.Size = new Size(350, 400);
            glassPanel.Location = new Point((this.Width - glassPanel.Width) / 2, (this.Height - glassPanel.Height) / 2);
            glassPanel.BackColor = Color.FromArgb(180, 45, 45, 48); 
            this.Controls.Add(glassPanel);

            Label lblTitle = new Label();
            lblTitle.Text = "CdK Library";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(80, 40);
            glassPanel.Controls.Add(lblTitle);

            Label lblUser = new Label() { Text = "Username", ForeColor = Color.LightGray, Location = new Point(40, 110), Font = new Font("Segoe UI", 10) };
            txtUsername = new TextBox() { Location = new Point(40, 135), Width = 270, Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle };

            Label lblPass = new Label() { Text = "Password", ForeColor = Color.LightGray, Location = new Point(40, 180), Font = new Font("Segoe UI", 10) };
            txtPassword = new TextBox() { Location = new Point(40, 205), Width = 270, Font = new Font("Segoe UI", 12), PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            btnLogin = new Button() { Text = "LOGIN", Location = new Point(40, 270), Width = 270, Height = 40, BackColor = Color.DodgerBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button() { Text = "EXIT", Location = new Point(40, 320), Width = 270, Height = 40, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => Application.Exit();

            glassPanel.Controls.Add(lblUser);
            glassPanel.Controls.Add(txtUsername);
            glassPanel.Controls.Add(lblPass);
            glassPanel.Controls.Add(txtPassword);
            glassPanel.Controls.Add(btnLogin);
            glassPanel.Controls.Add(btnExit);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            string query = "SELECT UserID, Username, Role, FullName FROM Users WHERE Username = @user AND Password = @pass";
            MySqlParameter[] parameters = {
                new MySqlParameter("@user", user),
                new MySqlParameter("@pass", pass)
            };

            DataTable dt = dbHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                Session.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                Session.Username = dt.Rows[0]["Username"].ToString();
                Session.Role = dt.Rows[0]["Role"].ToString();
                Session.FullName = dt.Rows[0]["FullName"].ToString();

                MessageBox.Show($"Welcome {Session.FullName}! Role: {Session.Role}");
   
                if (Session.Role == "Admin")
                {
                    AdminDashboard adminForm = new AdminDashboard();
                    adminForm.Show();
                }
                else
                {                  
                    LibraryDashboard libForm = new LibraryDashboard();
                    libForm.Show();
                }
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid Credentials!");
            }
        }
    }
}