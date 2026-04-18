using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using ColegioLibrarySystem.Helpers;
using ColegioLibrarySystem.Models;
using MySql.Data.MySqlClient;

namespace ColegioLibrarySystem.Views
{
    public partial class LibraryDashboard : Form
    {
        private DataGridView gridBooks;
        private ComboBox cmbFilterCategory;
        private TextBox txtSearchTitle;
        private NumericUpDown numBorrowQty;
        private Button btnBorrow, btnSearch, btnLogout, btnViewBorrowed;
        private DatabaseHelper dbHelper;
        private int selectedBookID = 0;
        private int availableCopies = 0;

        public LibraryDashboard()
        {
            dbHelper = new DatabaseHelper();

            // Guard: require logged-in user and disallow Admins here (Admins use AdminDashboard)
            if (Session.UserID == 0)
            {
                MessageBox.Show("You must be logged in to access the library.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoginForm login = new LoginForm();
                login.Show();
                return;
            }

            if (Session.Role == "Admin")
            {
                MessageBox.Show("Admins cannot access the Library Dashboard. Redirecting to Admin Dashboard.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AdminDashboard admin = new AdminDashboard();
                admin.Show();
                return;
            }

            InitializeDashboard();
            LoadCategories();
            LoadBooks();
        }

        private void InitializeDashboard()
        {
            this.Text = $"{Session.Role} Dashboard - Colegio de Kidapawan LMS";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Label lblHeader = new Label() { Text = $"Welcome, {Session.FullName} ({Session.Role})", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblHeader);
       
            btnViewBorrowed = new Button() { Text = "📚 MY BORROWED BOOKS", Location = new Point(560, 20), Width = 190, Height = 30, BackColor = Color.DarkMagenta, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnViewBorrowed.Click += BtnViewBorrowed_Click;
            this.Controls.Add(btnViewBorrowed);

            btnLogout = new Button() { Text = "LOGOUT", Location = new Point(760, 20), Width = 100, Height = 30, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += BtnLogout_Click;
            this.Controls.Add(btnLogout);

            Label lblFilter = new Label() { Text = "Category:", Location = new Point(20, 70), AutoSize = true };
            cmbFilterCategory = new ComboBox() { Location = new Point(80, 65), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblSearch = new Label() { Text = "Search Title:", Location = new Point(250, 70), AutoSize = true };
            txtSearchTitle = new TextBox() { Location = new Point(330, 65), Width = 200 };

            btnSearch = new Button() { Text = "Search", Location = new Point(540, 63), Width = 80, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += BtnSearch_Click;

            this.Controls.AddRange(new Control[] { lblFilter, cmbFilterCategory, lblSearch, txtSearchTitle, btnSearch });

            gridBooks = new DataGridView() { Location = new Point(20, 100), Size = new Size(580, 440), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridBooks.CellClick += GridBooks_CellClick;
            this.Controls.Add(gridBooks);

            Panel pnlBorrow = new Panel() { Location = new Point(620, 100), Size = new Size(240, 200), BackColor = Color.White };
            this.Controls.Add(pnlBorrow);

            Label lblAction = new Label() { Text = "Borrow Book", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            Label lblQty = new Label() { Text = "Quantity to Borrow:", Location = new Point(10, 50), AutoSize = true };
            numBorrowQty = new NumericUpDown() { Location = new Point(10, 75), Width = 210, Minimum = 1, Maximum = 100, Value = 1 };

            // Student UI: locked to 1 copy
            if (Session.Role == "Student")
            {
                numBorrowQty.Value = 1;
                numBorrowQty.Minimum = 1;
                numBorrowQty.Maximum = 1;
                numBorrowQty.Enabled = false;
                Label lblNotice = new Label() { Text = "*Students restricted to 1 copy", ForeColor = Color.Red, Location = new Point(10, 100), AutoSize = true };
                pnlBorrow.Controls.Add(lblNotice);
            }
            else if (Session.Role == "Instructor")
            {
                // Instructor may change quantity; leave enabled. maximum will be updated on selection.
                numBorrowQty.Enabled = true;
            }
            else
            {
                // For any other roles, disable by default.
                numBorrowQty.Enabled = false;
            }

            btnBorrow = new Button() { Text = "CONFIRM BORROW", Location = new Point(10, 140), Width = 210, Height = 40, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Enabled = false };
            btnBorrow.Click += BtnBorrow_Click;

            pnlBorrow.Controls.AddRange(new Control[] { lblAction, lblQty, numBorrowQty, btnBorrow });
        }

        private void LoadCategories()
        {
            DataTable dt = dbHelper.ExecuteQuery("SELECT CategoryID, CategoryName FROM Categories");
            DataRow row = dt.NewRow(); row["CategoryID"] = 0; row["CategoryName"] = "-- All Categories --";
            dt.Rows.InsertAt(row, 0);
            cmbFilterCategory.DataSource = dt; cmbFilterCategory.DisplayMember = "CategoryName"; cmbFilterCategory.ValueMember = "CategoryID";
        }

        private void LoadBooks(string searchQuery = "", int categoryId = 0)
        {
            string query = @"SELECT b.BookID, b.Title, b.Author, c.CategoryName, b.AvailableCopies 
                             FROM Books b 
                             LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
                             WHERE b.Title LIKE @search";
            if (categoryId > 0) query += " AND b.CategoryID = @catId";

            MySqlParameter[] parameters = { new MySqlParameter("@search", "%" + searchQuery + "%"), new MySqlParameter("@catId", categoryId) };
            gridBooks.DataSource = dbHelper.ExecuteQuery(query, parameters);
            btnBorrow.Enabled = false;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            int catId = Convert.ToInt32(cmbFilterCategory.SelectedValue);
            LoadBooks(txtSearchTitle.Text.Trim(), catId);
        }

        private void GridBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridBooks.Rows[e.RowIndex];
                selectedBookID = Convert.ToInt32(row.Cells["BookID"].Value);
                availableCopies = Convert.ToInt32(row.Cells["AvailableCopies"].Value);
                btnBorrow.Enabled = true;

                if (Session.Role == "Instructor")
                {
                    numBorrowQty.Enabled = true;
                    numBorrowQty.Maximum = availableCopies > 0 ? availableCopies : 1;
                    if (numBorrowQty.Value > numBorrowQty.Maximum) numBorrowQty.Value = numBorrowQty.Maximum;
                }
                else if (Session.Role == "Student")
                {
                    // ensure student is locked to 1 and cannot increase
                    numBorrowQty.Value = 1;
                    numBorrowQty.Minimum = 1;
                    numBorrowQty.Maximum = 1;
                    numBorrowQty.Enabled = false;
                }
                else
                {
                    // other roles: leave borrow disabled if not allowed
                    numBorrowQty.Enabled = false;
                }
            }
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            int qtyToBorrow = (int)numBorrowQty.Value;

            // Server-side enforcement for student rule: max 1 per borrow and one copy per book
            if (Session.Role == "Student" && qtyToBorrow > 1)
            {
                MessageBox.Show("Students are restricted to borrowing 1 copy.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (availableCopies < qtyToBorrow)
            {
                MessageBox.Show($"Cannot borrow. Only {availableCopies} copies available.", "Inventory Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            if (Session.Role == "Student")
            {
                string checkQuery = "SELECT * FROM Transactions WHERE UserID = @uid AND BookID = @bid AND Status = 'Borrowed'";
                DataTable dtCheck = dbHelper.ExecuteQuery(checkQuery, new MySqlParameter[] {
                    new MySqlParameter("@uid", Session.UserID),
                    new MySqlParameter("@bid", selectedBookID)
                });

                if (dtCheck.Rows.Count > 0)
                {
                    MessageBox.Show("You already have this book borrowed. Return it before borrowing again.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Instructor: verify requested qty does not exceed available copies (server-side)
            if (Session.Role == "Instructor")
            {
                if (qtyToBorrow < 1 || qtyToBorrow > availableCopies)
                {
                    MessageBox.Show($"Instructors may borrow up to {availableCopies} copies.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Core inventory update: insert transaction and decrement available copies
            // Note: consider moving these to a DB transaction/stored-proc to avoid race conditions
            dbHelper.ExecuteNonQuery("INSERT INTO Transactions (UserID, BookID, Quantity, Status) VALUES (@uid, @bid, @qty, 'Borrowed')",
                new MySqlParameter[] { new MySqlParameter("@uid", Session.UserID), new MySqlParameter("@bid", selectedBookID), new MySqlParameter("@qty", qtyToBorrow) });

            dbHelper.ExecuteNonQuery("UPDATE Books SET AvailableCopies = AvailableCopies - @qty WHERE BookID = @bid",
                new MySqlParameter[] { new MySqlParameter("@qty", qtyToBorrow), new MySqlParameter("@bid", selectedBookID) });

            MessageBox.Show("Book(s) borrowed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            BtnSearch_Click(null, null);
        }

        private void BtnViewBorrowed_Click(object sender, EventArgs e)
        {
            ReturnBooksForm returnForm = new ReturnBooksForm();
            returnForm.Show();
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