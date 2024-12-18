using MultiSpectrometerReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSpectrometerReader.Forms
{
    public partial class ParameterSettingsForm : Form
    {
        public ParameterSettingsForm()
        {
            InitializeComponent();

            if (Directory.Exists(Parameters.jsonFilePath))
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "Json file was found. (Parameter Setting)");
            else
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "Json file could not be found. (Parameter Setting)");
        }

        private void ParameterSettingsForm_Load(object sender, EventArgs e)
        {
            ReadSettings();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox1.Text != " ")
                Shared.jsonData.IntegrationTime = textBox1.Text;

            if (textBox2.Text != "" && textBox2.Text != " ")
                Shared.jsonData.AverageScan = textBox2.Text;

            if (comboBox1.SelectedIndex == 0)
                Shared.jsonData.DigitalGain = "Low";
            else
                Shared.jsonData.DigitalGain = "High";
            Shared.jsonData.AnalogGain = label15.Text;

            if (textBox3.Text != "" && textBox3.Text != " ")
                Shared.jsonData.TestLoopCount = textBox3.Text;
            if (textBox4.Text != "" && textBox4.Text != " ")
                Shared.jsonData.OtherLoopCount = textBox4.Text;

            if (comboBox2.SelectedIndex == 0)
                Shared.jsonData.Filter = "Normal";
            else if (comboBox2.SelectedIndex == 1)
                Shared.jsonData.Filter = "MOA";

            if (comboBox3.SelectedIndex == 0)
                Shared.jsonData.CmcType = "Normal";
            else if (comboBox3.SelectedIndex == 1)
                Shared.jsonData.CmcType = "CMC1:1";
            else if (comboBox3.SelectedIndex == 2)
                Shared.jsonData.CmcType = "CMC2:1";

            if (textBox5.Text != "" && textBox5.Text != " ")
                Shared.jsonData.TimerValue = textBox5.Text;
            if (textBox6.Text != "" && textBox6.Text != " ")
                Shared.jsonData.DeltaELimit = textBox6.Text;
            if (textBox7.Text != "" && textBox7.Text != " ")
                Shared.jsonData.GraphicsLimit = textBox7.Text;

            try
            {
                Shared.jsonData.Save();
                ReadSettings();

                trackBar1.Value = 11;
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 1;
                textBox1.Text = "";
                textBox2.Text = "";
                textBox4.Text = "";
                textBox3.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";

                label16.Text = "Default";
                label16.ForeColor = Color.Brown;
                label23.Text = "Default";
                label23.ForeColor = Color.Brown;
                label27.Text = "Default";
                label27.ForeColor = Color.Brown;
                label31.Text = "Default";
                label31.ForeColor = Color.Brown;
                label35.Text = "Default";
                label35.ForeColor = Color.Brown;
                label39.Text = "Default";
                label39.ForeColor = Color.Brown;
                label43.Text = "Default";
                label43.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(250);
                label16.Text = "Save";
                label16.ForeColor = Color.Green;
                label23.Text = "Save";
                label23.ForeColor = Color.Green;
                label27.Text = "Save";
                label27.ForeColor = Color.Green;
                label31.Text = "Save";
                label31.ForeColor = Color.Green;
                label35.Text = "Save";
                label35.ForeColor = Color.Green;
                label39.Text = "Save";
                label39.ForeColor = Color.Green;
                label43.Text = "Save";
                label43.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label16.Text = "Error";
                label16.ForeColor = Color.Red;
                label23.Text = "Error";
                label23.ForeColor = Color.Red;
                label27.Text = "Error";
                label27.ForeColor = Color.Red;
                label31.Text = "Error";
                label31.ForeColor = Color.Red;
                label35.Text = "Error";
                label35.ForeColor = Color.Red;
                label39.Text = "Error";
                label39.ForeColor = Color.Red;
                label43.Text = "Error";
                label43.ForeColor = Color.Red;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label15.Text = trackbarValue.ToString();
        }

        private void ReadSettings()
        {
            label5.Text = Shared.jsonData.IntegrationTime;
            label6.Text = Shared.jsonData.AverageScan;
            label7.Text = Shared.jsonData.DigitalGain;
            label8.Text = Shared.jsonData.AnalogGain;
            label19.Text = Shared.jsonData.TestLoopCount;
            label20.Text = Shared.jsonData.OtherLoopCount;
            label25.Text = Shared.jsonData.Filter;
            label29.Text = Shared.jsonData.CmcType;
            label33.Text = Shared.jsonData.TimerValue;
            label37.Text = Shared.jsonData.DeltaELimit;
            label41.Text = Shared.jsonData.GraphicsLimit;
        }
    }
}