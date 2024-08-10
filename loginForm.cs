using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace database
{
    public partial class LoginForm : Form
    {
        SqlConnection connection;

        public LoginForm()
        {
            InitializeComponent();
            connection = new SqlConnection("Server=DESKTOP-500TJGO\\MSSQLSERVERS;Database=product_management;Integrated Security = true;");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string query = "select * from account where username =@username and u_password =@password";
            connection.Open();
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            cmd.Parameters["@username"].Value = username;
            cmd.Parameters.AddWithValue("@password", SqlDbType.VarChar);
            cmd.Parameters["@password"].Value = password;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string role = reader["u_role"].ToString();
                if (role.Equals("admin"))
                {
                    MessageBox.Show(this, "login successful!", "result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    AttenSystem system = new AttenSystem();
                    system.ShowDialog();
                    this.Dispose();
                }
                else if (role.Equals("user"))
                {
                    MessageBox.Show(this, "login successful!", "result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    AttenStatu status = new AttenStatu();
                    status.ShowDialog();
                    this.Dispose();
                }
                else
                    lblError.Text = "you are not allowed to access";
            }
            else
            {
                lblError.Text = "wrong username or password";
            }
            connection.Close();

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
      
    }
}
