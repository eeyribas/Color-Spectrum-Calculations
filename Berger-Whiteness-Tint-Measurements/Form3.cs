using Berger_Whiteness_Tint_Measurements.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Berger_Whiteness_Tint_Measurements
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            ReadSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;
            comboBox1.ResetText();
            comboBox1.Items.Clear();

            string[] serialPorts = SerialPort.GetPortNames();
            foreach (string serialPort in serialPorts)
                comboBox1.Items.Add(serialPort);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] newData = new string[10];

            if (comboBox1.Text != "" && comboBox1.Text != " ")
                Parameters.selectDevicePort = comboBox1.Text;
            else
                Parameters.selectDevicePort = "A";

            try
            {
                newData[0] = label2.Text = Parameters.selectDevicePort;
                newData[1] = Parameters.integrationTime;
                newData[2] = Parameters.averageScan;
                newData[3] = Parameters.digitalGain;
                newData[4] = Parameters.analogGain;
                newData[5] = Parameters.firstNm;
                newData[6] = Parameters.lastNm;
                newData[7] = Parameters.testLoopCount;
                newData[8] = Parameters.otherLoopCount;
                newData[9] = Parameters.filter;
                Functions.writeFile("Settings.txt", newData);

                comboBox1.ResetText();
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
            string fileName = "Settings.txt";
            int textCount = 10;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 9);

            Parameters.selectDevicePort = label2.Text = readData[0];
            Parameters.integrationTime = readData[1];
            Parameters.averageScan = readData[2];
            Parameters.digitalGain = readData[3];
            Parameters.analogGain = readData[4];
            Parameters.firstNm = readData[5];
            Parameters.lastNm = readData[6];
            Parameters.testLoopCount = readData[7];
            Parameters.otherLoopCount = readData[8];
            Parameters.filter = readData[9];
        }
    }
}
