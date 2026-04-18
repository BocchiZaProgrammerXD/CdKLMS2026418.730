using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using ColegioLibrarySystem.Helpers;
using ColegioLibrarySystem.Models;
using MySql.Data.MySqlClient;

namespace ColegioLibrarySystem.Views
{
    public partial class ReturnBooksForm : Form
    {
        private DataGridView gridBorrowed;
        private Button btnReturn, btnBack, btnLogout;
        private DatabaseHelper dbHelper;
        private int selectedTransID = 0;
        private int selectedBookID = 0;
        private int selectedQty = 0;

        public ReturnBooksForm()
        {
            dbHelper = new DatabaseHelper();

            // Guard: must be logged-in
            if (Session.UserID == 0)
            {
                MessageBox.Show("You must be logged in to view borrowed books.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoginForm login = new LoginForm();
                login.Show();
                return;
            }

            InitializeForm();
            LoadBorrowedBooks();
        }

        private void InitializeForm()
        {
            this.Text = "My Borrowed Books";
            this.Size = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Label lblHeader = new Label() { Text = $"{Session.FullName}'s Borrowed Books", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblHeader);

            btnBack = new Button() { Text = "⬅ BACK TO DASHBOARD", Location = new Point(300, 20), Width = 180, Height = 30, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);

            btnLogout = new Button() { Text = "LOGOUT", Location = new Point(490, 20), Width = 160, Height = 30, BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += BtnLogout_Click;
            this.Controls.Add(btnLogout);

            gridBorrowed = new DataGridView() { Location = new Point(20, 70), Size = new Size(640, 280), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridBorrowed.CellClick += GridBorrowed_CellClick;
            this.Controls.Add(gridBorrowed);

            btnReturn = new Button() { Text = "RETURN SELECTED BOOK", Location = new Point(20, 360), Width = 640, Height = 40, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Enabled = false };
            btnReturn.Click += BtnReturn_Click;
            this.Controls.Add(btnReturn);
        }

        private void LoadBorrowedBooks()
        {
            string query = @"SELECT t.TransactionID, t.BookID, b.Title, b.Author, t.Quantity, t.Status 
                             FROM Transactions t
                             JOIN Books b ON t.BookID = b.BookID
                             WHERE t.UserID = @uid AND t.Status = 'Borrowed'";

            MySqlParameter[] parameters = { new MySqlParameter("@uid", Session.UserID) };
            gridBorrowed.DataSource = dbHelper.ExecuteQuery(query, parameters);

            if (gridBorrowed.Columns["TransactionID"] != null) gridBorrowed.Columns["TransactionID"].Visible = false;
            if (gridBorrowed.Columns["BookID"] != null) gridBorrowed.Columns["BookID"].Visible = false;

            btnReturn.Enabled = false; 
            selectedTransID = 0;
        }

        private void GridBorrowed_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridBorrowed.Rows[e.RowIndex];
                selectedTransID = Convert.ToInt32(row.Cells["TransactionID"].Value);
                selectedBookID = Convert.ToInt32(row.Cells["BookID"].Value);
                selectedQty = Convert.ToInt32(row.Cells["Quantity"].Value);
                btnReturn.Enabled = true;
            }
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            if (selectedTransID == 0) return;

            string updateTrans = "UPDATE Transactions SET Status = 'Returned' WHERE TransactionID = @tid";
            dbHelper.ExecuteNonQuery(updateTrans, new MySqlParameter[] { new MySqlParameter("@tid", selectedTransID) });

            string updateBook = "UPDATE Books SET AvailableCopies = AvailableCopies + @qty WHERE BookID = @bid";
            dbHelper.ExecuteNonQuery(updateBook, new MySqlParameter[] { new MySqlParameter("@qty", selectedQty), new MySqlParameter("@bid", selectedBookID) });

            MessageBox.Show("Book returned successfully! Thank you.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBorrowedBooks(); 
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            LibraryDashboard libDash = new LibraryDashboard();
            libDash.Show();
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