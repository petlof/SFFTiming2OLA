using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SSFTimingOlaSync
{
    public partial class FrmSelectClass : Form
    {
        public DateTime StartDate { get; set; }

        public EventClass Class
        {
            get
            {
                return comboBox1.SelectedItem as EventClass;
            }
        }

        public bool EventWithHeats
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public EventClass Heat1
        {
            get
            {
                return cmbHeat1.SelectedItem as EventClass;
            }
        }

        public EventClass Heat2
        {
            get
            {
                return cmbHeat2.SelectedItem as EventClass;
            }
        }
        public EventClass Heat3
        {
            get
            {
                return cmbHeat3.SelectedItem as EventClass;
            }
        }

        public FrmSelectClass()
        {
            InitializeComponent();

            var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString);
            olaConnection.Open();

            using (IDbCommand cmd = olaConnection.CreateCommand())
            {
                cmd.CommandText = "select eventid, name, startDate from Events";

                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    StartDate = Convert.ToDateTime(reader["startDate"]);
                    lblInfo.Text = "Event: " + reader["name"] + ", Date: " + reader["startDate"];
                }
                reader.Close();


                cmd.CommandText = "select baseClassId, name, sex from eventClasses";

                comboBox1.Items.Clear();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboBox1.Items.Add(new EventClass { Id = Convert.ToInt32(reader["baseClassId"]), Name = reader["name"] as string, Sex = reader["sex"] as string });
                    cmbHeat1.Items.Add(new EventClass { Id = Convert.ToInt32(reader["baseClassId"]), Name = reader["name"] as string, Sex = reader["sex"] as string });
                    cmbHeat2.Items.Add(new EventClass { Id = Convert.ToInt32(reader["baseClassId"]), Name = reader["name"] as string, Sex = reader["sex"] as string });
                    cmbHeat3.Items.Add(new EventClass { Id = Convert.ToInt32(reader["baseClassId"]), Name = reader["name"] as string, Sex = reader["sex"] as string });
                }
                comboBox1.SelectedIndex = 0;
                cmbHeat1.SelectedIndex = 0;
                cmbHeat2.SelectedIndex = 0;
                cmbHeat3.SelectedIndex = 0;
                reader.Close();
                cmd.Dispose();
            }

            olaConnection.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label2.Visible = label3.Visible = label4.Visible = cmbHeat1.Visible = cmbHeat2.Visible = cmbHeat3.Visible = checkBox1.Checked;
        }
    }

    public class EventClass
    {
        public string Name;
        public int Id;
        public string Sex;

        public override string ToString()
        {
            return Name;
        }
    }
}
