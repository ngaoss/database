using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace database
{
    public partial class AttenStatu : Form
    {
        SqlConnection connection;

        public AttenStatu()
        {
            InitializeComponent();
            connection = new SqlConnection("Server=DESKTOP-500TJGO\\MSSQLSERVERS;Database=StudentAttendance;Integrated Security = true;");
        }

        private void LoadCourses()
        {
            try
            {
                connection.Open();
                string query = "SELECT courseID, courseName FROM courses";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                comboBoxCourses.Items.Clear(); 
                while (reader.Read())
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Text = reader["courseName"].ToString(),
                        Value = reader["courseID"]
                    };
                    comboBoxCourses.Items.Add(item);
                }

                if (comboBoxCourses.Items.Count == 0)
                {
                    MessageBox.Show("no courses found.");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "error loading courses: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }


        private void comboBoxCourses_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void FillData(int courseId)
        {
            string tableName = GetAttendanceTableName(courseId);

            string query = $@"
         SELECT 
        s.studentID,
        s.name, 
        a.Date, 
        a.Status,
        c.courseName
         FROM 
        StudentsTable s 
         LEFT JOIN 
        {tableName} a 
         ON 
        s.studentID = a.studentID
         INNER JOIN 
        courses c
         ON 
        a.courseID = c.courseID";

            DataTable tbl = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter(query, connection);
            ad.Fill(tbl);
            dataGridView1.DataSource = tbl;

            connection.Close();
        }
        public void percentageOfStudent(int studentId, int courseId)
        {
            string tableName = GetAttendanceTableName(courseId);

            string query = $@"
          SELECT 
          COUNT(a.Status) AS totalClasses,
          SUM(CASE WHEN a.Status = 'present' THEN 1 ELSE 0 END) AS presentDays,
          (SUM(CASE WHEN a.Status = 'present' THEN 1 ELSE 0 END) * 100.0 / COUNT(a.Status)) AS attendancePercentage
          FROM 
          {tableName} a
          WHERE 
          a.studentID = @StudentID AND a.courseID = @CourseID";
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@CourseID", courseId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    double attendancePercentage = Convert.ToDouble(reader["attendancePercentage"]);
                    txPercentStudent.Text = $"{attendancePercentage:F1}%";
                }
                else
                {
                    txPercentStudent.Text = "ao attendance data available";
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "error calculating attendance percentage: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        private string GetAttendanceTableName(int courseId)
        {
            switch (courseId)
            {
                case 1: return "mathematicsAttendance";
                case 2: return "scienceAttendance";
                case 3: return "historyAttendance";
                default: throw new ArgumentException("invalid course ID");
            }
        }


        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm login = new LoginForm();
            login.ShowDialog();
            this.Dispose();
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

        private void AttenStatu_Load_1(object sender, EventArgs e)
        {
            LoadCourses();

        }

        private void comboBoxCourses_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBoxItem selectedCourse = comboBoxCourses.SelectedItem as ComboBoxItem;
            if (selectedCourse != null)
            {
                int courseId = (int)selectedCourse.Value;
                FillData(courseId);
                percentageOfCourse(courseId);  
            }

        }
        public void percentageOfCourse(int courseId)
        {
            string tableName = GetAttendanceTableName(courseId);

            string query = $@"
        SELECT 
        COUNT(a.Status) AS totalClasses,
        SUM(CASE WHEN a.Status = 'present' THEN 1 ELSE 0 END) AS presentDays,
        (SUM(CASE WHEN a.Status = 'present' THEN 1 ELSE 0 END) * 100.0 / COUNT(a.Status)) AS attendancePercentage
        FROM 
        {tableName} a
        WHERE 
        a.courseID = @CourseID";

            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@CourseID", courseId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    double attendancePercentage = Convert.ToDouble(reader["attendancePercentage"]);
                    txPercentCourses.Text = $"{attendancePercentage:F1}%";
                }
                else
                {
                    txPercentCourses.Text = "no attendance data available.";
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "error calculating attendance percentage: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void textBoxAttendancePercentageAllClasses_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                int studentId = Convert.ToInt32(row.Cells["Column1"].Value);
                string studentName = row.Cells["Collum2"].Value.ToString();
                int courseId = Convert.ToInt32(comboBoxCourses.SelectedItem != null ? ((ComboBoxItem)comboBoxCourses.SelectedItem).Value : 0);

                labelstudentName.Text = $"{studentName}";

                if (courseId > 0)
                {
                    percentageOfStudent(studentId, courseId);
                }
            }
        }
    }
}
