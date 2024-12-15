using Berger_Whiteness_Tint_Measurements.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Berger_Whiteness_Tint_Measurements
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            ReadSettings();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] newData = new string[10];

            if (textBox1.Text != "" && textBox1.Text != " ")
                Parameters.integrationTime = textBox1.Text;

            if (textBox2.Text != "" && textBox2.Text != " ")
                Parameters.averageScan = textBox2.Text;

            if (comboBox1.SelectedIndex == 0)
                Parameters.digitalGain = "Low";
            else
                Parameters.digitalGain = "High";

            Parameters.analogGain = label19.Text;

            if (textBox3.Text != "" && textBox3.Text != " ")
                Parameters.firstNm = textBox3.Text;
            if (textBox4.Text != "" && textBox4.Text != " ")
                Parameters.lastNm = textBox4.Text;

            if (textBox5.Text != "" && textBox5.Text != " ")
                Parameters.testLoopCount = textBox5.Text;
            if (textBox6.Text != "" && textBox6.Text != " ")
                Parameters.otherLoopCount = textBox6.Text;

            if (comboBox2.SelectedIndex == 0)
                Parameters.filter = "Normal";
            else if (comboBox2.SelectedIndex == 1)
                Parameters.filter = "MOA";

            try
            {
                newData[0] = Parameters.selectDevicePort;
                newData[1] = label2.Text = Parameters.integrationTime;
                newData[2] = label5.Text = Parameters.averageScan;
                newData[3] = label7.Text = Parameters.digitalGain;
                newData[4] = label9.Text = Parameters.analogGain;
                newData[5] = label11.Text = Parameters.firstNm;
                newData[6] = label13.Text = Parameters.lastNm;
                newData[7] = label24.Text = Parameters.testLoopCount;
                newData[8] = label26.Text = Parameters.otherLoopCount;
                newData[9] = label31.Text = Parameters.filter;
                Functions.writeFile("Settings.txt", newData);

                trackBar1.Value = 11;
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 1;
                textBox1.Text = "";
                textBox2.Text = "";
                textBox6.Text = "";
                textBox5.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";

                label22.Text = "Default";
                label22.ForeColor = Color.Brown;
                label29.Text = "Default";
                label29.ForeColor = Color.Brown;
                label33.Text = "Default";
                label33.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(250);
                label22.Text = "Save";
                label22.ForeColor = Color.Green;
                label29.Text = "Save";
                label29.ForeColor = Color.Green;
                label33.Text = "Save";
                label33.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label22.Text = "Error";
                label22.ForeColor = Color.Red;
                label29.Text = "Error";
                label29.ForeColor = Color.Red;
                label33.Text = "Error";
                label33.ForeColor = Color.Red;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label19.Text = trackbarValue.ToString();
        }

        private void ReadSettings()
        {
            string fileName = "Settings.txt";
            int textCount = 10;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 9);

            Parameters.selectDevicePort = readData[0];
            Parameters.integrationTime = label2.Text = readData[1];
            Parameters.averageScan = label5.Text = readData[2];
            Parameters.digitalGain = label7.Text = readData[3];
            Parameters.analogGain = label9.Text = readData[4];
            Parameters.firstNm = label11.Text = readData[5];
            Parameters.lastNm = label13.Text = readData[6];
            Parameters.testLoopCount = label24.Text = readData[7];
            Parameters.otherLoopCount = label26.Text = readData[8];
            Parameters.filter = label31.Text = readData[9];
        }
    }
}
