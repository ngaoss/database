using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace database
{
    public partial class AttenSystem : Form

    {
        SqlConnection connection;

        public AttenSystem()
        {
            InitializeComponent();
            connection = new SqlConnection("Server=DESKTOP-500TJGO\\MSSQLSERVERS;Database=StudentAttendance;Integrated Security = true;");

        }

        public void FillData(int courseId)
        {
            string tableName = getAttendanceTable(courseId);
            string query = $@"
             SELECT a.attendanceID, s.studentID, s.name AS StudentName, s.gender AS Gender, s.age AS Age, 
               c.courseName AS CourseName, a.Date, a.Status 
             FROM {tableName} a
             INNER JOIN studentsTable s ON a.studentID = s.studentID
             INNER JOIN courses c ON a.courseID = c.courseID
             WHERE a.courseID = @CourseID";

            DataTable tbl = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter(query, connection);
            ad.SelectCommand.Parameters.AddWithValue("@CourseID", courseId);
            ad.Fill(tbl);

            dataGridView1.DataSource = tbl;
        }


        private string getAttendanceTable(int courseId)
        {
            switch (courseId)
            {
                case 1: return "mathematicsAttendance";
                case 2: return "scienceAttendance";
                case 3: return "historyAttendance";
                default: throw new ArgumentException("Invalid course ID");
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && comboBoxCourses.SelectedItem != null)
            {
                ComboBoxItem selectedCourse = comboBoxCourses.SelectedItem as ComboBoxItem;
                int courseId = (int)selectedCourse.Value;
                string tableName = getAttendanceTable(courseId);

                string updateQuery = $@"
                   UPDATE {tableName}
                  SET Status = @Status
                  WHERE studentID = @ID AND Date = @Date";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                try
                {
                    connection.Open();
                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        int id = Convert.ToInt32(row.Cells["ColumnID"].Value);
                        string attendanceStatus = comboBoxAttendanceStatus.SelectedItem.ToString();
                        DateTime attendanceDate = dateTimePickerAttendanceDate.Value.Date;

                        cmd.CommandText = updateQuery;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Date", attendanceDate);
                        cmd.Parameters.AddWithValue("@Status", attendanceStatus);
                        cmd.ExecuteNonQuery();
                    }

                    FillData(courseId);
                    MessageBox.Show(this, "updated successful", "result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show(this, "please select a student", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
           LoadCourses();
        }

    
        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm login = new LoginForm();
            login.ShowDialog();
            this.Dispose();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                labelName.Text = row.Cells["CollumName"].Value.ToString();
                comboBoxAttendanceStatus.SelectedItem = row.Cells["ColumnStatus"].Value.ToString();
            }
        }

        private void comboBoxAttendanceStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void LoadCourses()
        {
            try
            {
                connection.Open();
                string query = "SELECT courseID, courseName FROM courses";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Text = reader["courseName"].ToString(),
                        Value = reader["courseID"]
                    };
                    comboBoxCourses.Items.Add(item);
                }

                reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "error loading courses: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void comboBoxCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItem selectedCourse = comboBoxCourses.SelectedItem as ComboBoxItem;
            if (selectedCourse != null)
            {
                int courseId = (int)selectedCourse.Value;
                FillData(courseId);
            }
        }
    }
}
