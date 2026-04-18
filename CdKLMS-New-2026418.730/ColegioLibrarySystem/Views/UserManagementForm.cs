using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using ColegioLibrarySystem.Helpers;
using MySql.Data.MySqlClient;
using ColegioLibrarySystem.Models;

namespace ColegioLibrarySystem.Views
{
    public partial class UserManagementForm : Form
    {
        private DataGridView gridUsers;
        private TextBox txtFullName, txtUsername, txtPassword;
        private ComboBox cmbRole;
        private Button btnAddUser, btnUpdate, btnDelete, btnClear, btnBack;
        private DatabaseHelper dbHelper;
        private int selectedUserID = 0; 

        public UserManagementForm()
        {
            dbHelper = new DatabaseHelper();
            InitializeForm();
            LoadUsers();
        }

        private void InitializeForm()
        {
            this.Text = "Admin - Enroll Users";
            this.Size = new Size(800, 530);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Label lblHeader = new Label() { Text = "User Management", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblHeader);

            btnBack = new Button() { Text = "⬅ BACK TO DASHBOARD", Location = new Point(580, 20), Width = 180, Height = 30, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);

            gridUsers = new DataGridView() { Location = new Point(20, 70), Size = new Size(450, 400), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridUsers.CellClick += GridUsers_CellClick; 
            this.Controls.Add(gridUsers);

            Panel pnlInputs = new Panel() { Location = new Point(490, 70), Size = new Size(270, 400), BackColor = Color.White };
            this.Controls.Add(pnlInputs);

            Label lblName = new Label() { Text = "Full Name", Location = new Point(10, 20) };
            txtFullName = new TextBox() { Location = new Point(10, 45), Width = 240 };

            Label lblUser = new Label() { Text = "Username", Location = new Point(10, 80) };
            txtUsername = new TextBox() { Location = new Point(10, 105), Width = 240 };

            Label lblPass = new Label() { Text = "Password", Location = new Point(10, 140) };
            txtPassword = new TextBox() { Location = new Point(10, 165), Width = 240, PasswordChar = '•' };

            Label lblRole = new Label() { Text = "Role", Location = new Point(10, 200) };
            cmbRole = new ComboBox() { Location = new Point(10, 225), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Student", "Instructor", "Admin" });
            cmbRole.SelectedIndex = 0;

            btnAddUser = new Button() { Text = "Enroll User", Location = new Point(10, 270), Width = 240, Height = 35, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnAddUser.Click += BtnAddUser_Click;

            btnUpdate = new Button() { Text = "Update", Location = new Point(10, 315), Width = 115, Height = 35, BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button() { Text = "Delete", Location = new Point(135, 315), Width = 115, Height = 35, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button() { Text = "Clear Fields", Location = new Point(10, 360), Width = 240, Height = 30, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClear.Click += BtnClear_Click;

            pnlInputs.Controls.AddRange(new Control[] { lblName, txtFullName, lblUser, txtUsername, lblPass, txtPassword, lblRole, cmbRole, btnAddUser, btnUpdate, btnDelete, btnClear });
        }

        private void LoadUsers()
        {
            string query = "SELECT UserID, FullName, Username, Role, Password FROM Users";
            gridUsers.DataSource = dbHelper.ExecuteQuery(query);

            if (gridUsers.Columns["Password"] != null)
                gridUsers.Columns["Password"].Visible = false;
        }

        private void GridUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridUsers.Rows[e.RowIndex];
                selectedUserID = Convert.ToInt32(row.Cells["UserID"].Value);

                txtFullName.Text = row.Cells["FullName"].Value.ToString();
                txtUsername.Text = row.Cells["Username"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
                cmbRole.Text = row.Cells["Role"].Value.ToString();

                btnAddUser.Enabled = false; 
                btnUpdate.Enabled = true;   
                btnDelete.Enabled = true;  

                if (selectedUserID == Session.UserID)
                {
                    btnDelete.Enabled = false;
                }
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all fields."); return;
            }

            try
            {
                string query = "INSERT INTO Users (Username, Password, Role, FullName) VALUES (@user, @pass, @role, @name)";
                MySqlParameter[] parameters = {
                    new MySqlParameter("@user", txtUsername.Text.Trim()),
                    new MySqlParameter("@pass", txtPassword.Text.Trim()),
                    new MySqlParameter("@role", cmbRole.SelectedItem.ToString()),
                    new MySqlParameter("@name", txtFullName.Text.Trim())
                };

                dbHelper.ExecuteNonQuery(query, parameters);
                BtnClear_Click(null, null); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating user. That username might already be taken. \n\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0) return;
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Fields cannot be empty for an update."); return;
            }

            try
            {
                string query = "UPDATE Users SET Username=@user, Password=@pass, Role=@role, FullName=@name WHERE UserID=@id";
                MySqlParameter[] parameters = {
                    new MySqlParameter("@user", txtUsername.Text.Trim()),
                    new MySqlParameter("@pass", txtPassword.Text.Trim()),
                    new MySqlParameter("@role", cmbRole.SelectedItem.ToString()),
                    new MySqlParameter("@name", txtFullName.Text.Trim()),
                    new MySqlParameter("@id", selectedUserID)
                };

                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("User account updated successfully!");
                BtnClear_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed. Make sure the username isn't taken by someone else.\n\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0) return;

            // Prevent deleting a user with active borrow transactions
            string checkQuery = "SELECT COUNT(*) AS cnt FROM Transactions WHERE UserID = @id AND Status = 'Borrowed'";
            MySqlParameter[] checkParams = { new MySqlParameter("@id", selectedUserID) };
            DataTable dtCheck = dbHelper.ExecuteQuery(checkQuery, checkParams);
            int activeBorrows = 0;
            if (dtCheck.Rows.Count > 0)
            {
                activeBorrows = Convert.ToInt32(dtCheck.Rows[0]["cnt"]);
            }

            if (activeBorrows > 0)
            {
                MessageBox.Show("Cannot delete this user because they have active borrowed books. Please ensure all borrowed copies are returned first.", "Constraint Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to permanently delete this user?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Users WHERE UserID=@id";
                    MySqlParameter[] parameters = { new MySqlParameter("@id", selectedUserID) };

                    dbHelper.ExecuteNonQuery(query, parameters);
                    BtnClear_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot delete this user because they may have related records or there was a database error. \n\n" + ex.Message, "Constraint Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtFullName.Clear(); txtUsername.Clear(); txtPassword.Clear();
            cmbRole.SelectedIndex = 0;
            selectedUserID = 0;

            btnAddUser.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;

            LoadUsers();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDash = new AdminDashboard();
            adminDash.Show();
            this.Hide();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}