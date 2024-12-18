using MultiSpectrometerReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSpectrometerReader.Forms
{
    public partial class ListDataForm : Form
    {
        public ListDataForm()
        {
            InitializeComponent();
        }

        private void ListDataForm_Load(object sender, EventArgs e)
        {
            string connectionString = "DataSource=" + Parameters.nameDatabase + "; Version = 3;";
            string requestCode = "select * from " + Parameters.nameTable;
            Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "List Data Form has been opened. (List Data)");

            Parameters.command = new SQLiteCommand();
            Parameters.connection = new SQLiteConnection(connectionString);
            Parameters.connection.Open();
            if (Parameters.connection.State == ConnectionState.Open)
            {
                Parameters.command.Connection = Parameters.connection;
                Parameters.command.CommandText = requestCode;
                Parameters.command.ExecuteNonQuery();
                SQLiteDataReader reader = Parameters.command.ExecuteReader();
                while (reader.Read())
                {
                    dataGridView1.Rows.Add(reader["id"].ToString(), reader["date"].ToString(), reader["deltaE1"].ToString(),
                        reader["deltaE2"].ToString(), reader["deltaE3"].ToString(), reader["kf12"].ToString(),
                        reader["kf13"].ToString(), reader["kf23"].ToString(), reader["meterInf"].ToString());
                }
                Parameters.connection.Close();
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "The values from the database have been displayed in the DataGridView. (List Data)");
            }
            else
            {
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "The values from the database could not be displayed in the DataGridView. (List Data)");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "DataSource=" + Parameters.nameDatabase + "; Version = 3;";
            string requestCode = "delete from " + Parameters.nameTable;

            Parameters.command = new SQLiteCommand();
            Parameters.connection = new SQLiteConnection(connectionString);
            Parameters.connection.Open();
            if (Parameters.connection.State == ConnectionState.Open)
            {
                Parameters.command.Connection = Parameters.connection;
                Parameters.command.CommandText = requestCode;
                Parameters.command.ExecuteNonQuery();
                Parameters.connection.Close();
                dataGridView1.Rows.Clear();
                Parameters.id = 1;
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "The values in the database have been deleted. (List Data)");
            }
            else
            {
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "An error occurred while deleting the values in the database. (List Data)");
            }
        }
    }
}
