using System;
using System.Drawing;
using System.Windows.Forms;
using ColegioLibrarySystem.Helpers;
using ColegioLibrarySystem.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ColegioLibrarySystem.Views
{
    public partial class AdminDashboard : Form
    {
        private DataGridView gridBooks;
        private TextBox txtTitle, txtAuthor, txtQty;
        private ComboBox cmbCategory;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh, btnLogout, btnEnrollUsers;
        private DatabaseHelper dbHelper;
        private int selectedBookID = 0;

        public AdminDashboard()
        {
            dbHelper = new DatabaseHelper();
            InitializeDashboard();
            LoadCategories();
            LoadBooks();
        }

        private void InitializeDashboard()
        {
            this.Text = "Admin Dashboard - Manage Books";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Label lblHeader = new Label() { Text = "Library Inventory", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblHeader);

            btnEnrollUsers = new Button() { Text = "👥 ENROLL USERS", Location = new Point(620, 20), Width = 130, Height = 30, BackColor = Color.DarkMagenta, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEnrollUsers.Click += BtnEnrollUsers_Click;
            this.Controls.Add(btnEnrollUsers);

            btnLogout = new Button() { Text = "LOGOUT", Location = new Point(760, 20), Width = 100, Height = 30, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += BtnLogout_Click;
            this.Controls.Add(btnLogout);

            gridBooks = new DataGridView() { Location = new Point(20, 60), Size = new Size(550, 480), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridBooks.CellClick += GridBooks_CellClick;
            this.Controls.Add(gridBooks);

            Panel pnlInputs = new Panel() { Location = new Point(600, 60), Size = new Size(260, 480), BackColor = Color.White };
            this.Controls.Add(pnlInputs);

            Label lblTitle = new Label() { Text = "Book Title", Location = new Point(10, 20) };
            txtTitle = new TextBox() { Location = new Point(10, 45), Width = 230 };

            Label lblAuthor = new Label() { Text = "Author", Location = new Point(10, 80) };
            txtAuthor = new TextBox() { Location = new Point(10, 105), Width = 230 };

            Label lblCat = new Label() { Text = "Category", Location = new Point(10, 140) };
            cmbCategory = new ComboBox() { Location = new Point(10, 165), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblQty = new Label() { Text = "Total Copies", Location = new Point(10, 200) };
            txtQty = new TextBox() { Location = new Point(10, 225), Width = 100 };

            btnAdd = new Button() { Text = "Add Book", Location = new Point(10, 270), Width = 230, Height = 40, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button() { Text = "Update", Location = new Point(10, 320), Width = 110, Height = 40, BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button() { Text = "Delete", Location = new Point(130, 320), Width = 110, Height = 40, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button() { Text = "Clear / Refresh", Location = new Point(10, 370), Width = 230, Height = 40, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += BtnRefresh_Click;

            pnlInputs.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblAuthor, txtAuthor, lblCat, cmbCategory, lblQty, txtQty, btnAdd, btnUpdate, btnDelete, btnRefresh });
        }

        private void LoadCategories()
        {
            string query = "SELECT CategoryID, CategoryName FROM Categories";
            cmbCategory.DataSource = dbHelper.ExecuteQuery(query);
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryID";
        }

        private void LoadBooks()
        {
            string query = @"SELECT b.BookID, b.Title, b.Author, c.CategoryName, b.TotalCopies, b.AvailableCopies 
                             FROM Books b 
                             LEFT JOIN Categories c ON b.CategoryID = c.CategoryID";
            gridBooks.DataSource = dbHelper.ExecuteQuery(query);
        }

        private void GridBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridBooks.Rows[e.RowIndex];
                selectedBookID = Convert.ToInt32(row.Cells["BookID"].Value);
                txtTitle.Text = row.Cells["Title"].Value.ToString();
                txtAuthor.Text = row.Cells["Author"].Value.ToString();
                cmbCategory.Text = row.Cells["CategoryName"].Value.ToString();
                txtQty.Text = row.Cells["TotalCopies"].Value.ToString();

                btnAdd.Enabled = false;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || cmbCategory.SelectedValue == null) return;
            int qty; if (!int.TryParse(txtQty.Text, out qty)) qty = 1;

            string query = "INSERT INTO Books (Title, CategoryID, Author, TotalCopies, AvailableCopies) VALUES (@title, @cat, @auth, @qty, @qty)";
            MySqlParameter[] parameters = {
                new MySqlParameter("@title", txtTitle.Text), new MySqlParameter("@cat", cmbCategory.SelectedValue),
                new MySqlParameter("@auth", txtAuthor.Text), new MySqlParameter("@qty", qty)
            };

            dbHelper.ExecuteNonQuery(query, parameters);
            BtnRefresh_Click(null, null);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedBookID == 0) return;
            int qty; if (!int.TryParse(txtQty.Text, out qty)) qty = 1;

            string query = "UPDATE Books SET Title=@title, CategoryID=@cat, Author=@auth, TotalCopies=@qty WHERE BookID=@id";
            MySqlParameter[] parameters = {
                new MySqlParameter("@title", txtTitle.Text), new MySqlParameter("@cat", cmbCategory.SelectedValue),
                new MySqlParameter("@auth", txtAuthor.Text), new MySqlParameter("@qty", qty),
                new MySqlParameter("@id", selectedBookID)
            };

            dbHelper.ExecuteNonQuery(query, parameters);
            MessageBox.Show("Book updated!");
            BtnRefresh_Click(null, null);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedBookID == 0) return;

            // Check for active borrow transactions tied to this book
            string checkQuery = "SELECT COUNT(*) AS cnt FROM Transactions WHERE BookID = @id AND Status = 'Borrowed'";
            MySqlParameter[] checkParams = { new MySqlParameter("@id", selectedBookID) };
            DataTable dtCheck = dbHelper.ExecuteQuery(checkQuery, checkParams);
            int activeBorrows = 0;
            if (dtCheck.Rows.Count > 0)
            {
                activeBorrows = Convert.ToInt32(dtCheck.Rows[0]["cnt"]);
            }

            if (activeBorrows > 0)
            {
                MessageBox.Show("Cannot delete this book because there are active borrow transactions tied to it. Please ensure all borrowed copies are returned first.", "Constraint Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Books WHERE BookID=@id";
                    MySqlParameter[] parameters = { new MySqlParameter("@id", selectedBookID) };
                    dbHelper.ExecuteNonQuery(query, parameters);
                    BtnRefresh_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot delete this book. It is likely tied to student borrow records. \n\nError: " + ex.Message, "Database Constraint", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtTitle.Clear(); txtAuthor.Clear(); txtQty.Clear();
            selectedBookID = 0;
            btnAdd.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            LoadBooks();
        }

        private void BtnEnrollUsers_Click(object sender, EventArgs e)
        {
            UserManagementForm userForm = new UserManagementForm();
            userForm.Show();
            this.Hide();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            LoginForm login = new LoginForm();
            login.Show();
            this.Hide();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { Application.Exit(); }
    }
}