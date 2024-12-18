using MultiSpectrometerReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSpectrometerReader.Forms
{
    public partial class DeviceSettingsForm : Form
    {
        public DeviceSettingsForm()
        {
            InitializeComponent();

            if (Directory.Exists(Parameters.jsonFilePath))
            {
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "JSON file found. (Device Settings)");
            }
            else
            {
                label4.Text = "Fail";
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "JSON file not found. (Device Settings)");
            }
        }

        private void DeviceSettingsForm_Load(object sender, EventArgs e)
        {
            ReadSettings();
            Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "JSON file parameters read. (Device Settings)");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;

            comboBox1.ResetText();
            comboBox2.ResetText();
            comboBox3.ResetText();

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();

            string[] serialPorts = SerialPort.GetPortNames();
            foreach (string serialPort in serialPorts)
            {
                comboBox1.Items.Add(serialPort);
                comboBox2.Items.Add(serialPort);
                comboBox3.Items.Add(serialPort);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "" && comboBox1.Text != " ")
                Shared.jsonData.SelectDevicePort1 = comboBox1.Text;
            else
                Shared.jsonData.SelectDevicePort1 = "A";

            if (comboBox2.Text != "" && comboBox2.Text != " ")
                Shared.jsonData.SelectDevicePort2 = comboBox2.Text;
            else
                Shared.jsonData.SelectDevicePort2 = "B";

            if (comboBox3.Text != "" && comboBox3.Text != " ")
                Shared.jsonData.SelectDevicePort3 = comboBox3.Text;
            else
                Shared.jsonData.SelectDevicePort3 = "C";

            try
            {
                Shared.jsonData.Save();
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "New data has been saved to the JSON file. (Device Settings)");
                ReadSettings();
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "The JSON file has been read with the new data. (Device Settings)");

                comboBox1.ResetText();
                comboBox2.ResetText();
                comboBox3.ResetText();

                label4.Text = "Default";
                label4.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(200);
                label4.Text = "Save";
                label4.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label4.Text = "Default";
                label4.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(200);
                label4.Text = "Error";
                label4.ForeColor = Color.Red;
            }
        }

        private void ReadSettings()
        {
            label11.Text = Shared.jsonData.SelectDevicePort1;
            label12.Text = Shared.jsonData.SelectDevicePort2;
            label13.Text = Shared.jsonData.SelectDevicePort3;
        }
    }
}
