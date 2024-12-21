using MultiSpectrometerColorReader.Classes;
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

namespace MultiSpectrometerColorReader.Forms
{
    public partial class MeasGraphicsForm : Form
    {
        private int[] nmCount = new int[Parameters.dataCount];

        private int graphicMinimum = 0;
        private int graphicMaximum = 0;
        private int seriesIndex = 0;
        private int darkSeriesIndex = 0;
        private int ELedSeriesIndex = 0;
        private int standartSampleSeriesIndex = 0;

        public MeasGraphicsForm()
        {
            InitializeComponent();
        }

        private void MeasGraphicsForm_Load(object sender, EventArgs e)
        {
            Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "Meas Graphics Form opened. (Meas Graphics)");
            
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            string[] serialPorts = SerialPort.GetPortNames();
            foreach (string serialPort in serialPorts)
                comboBox1.Items.Add(serialPort);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "10";
            textBox2.Text = "5";
            comboBox2.SelectedIndex = 0;
            trackBar1.Value = 10;

            try
            {
                string selectPortName = comboBox1.Text;
                Functions.OpenSerialPort(serialPort1, selectPortName, 3000000);

                if (serialPort1.IsOpen)
                {
                    label1.Text = "Connect";
                    label1.ForeColor = Color.Green;
                    groupBox2.Enabled = true;
                    groupBox3.Enabled = true;
                    groupBox4.Enabled = true;
                    ReadNmIndisRelatedPixel();
                    Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "Connected to the device on the selected serial port. (Meas Graphics)");
                }
                else
                {
                    label1.Text = "Disconnect";
                    label1.ForeColor = Color.Red;
                    groupBox2.Enabled = false;
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = false;
                    Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "Could not connect to the device on the selected serial port. (Meas Graphics)");
                }
            }
            catch (Exception)
            {
                Functions.CloseSerialPort(serialPort1);

                label1.Text = "Error";
                label1.ForeColor = Color.Red;
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "An error occurred while connecting to the device on the selected serial port. (Meas Graphics)");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Functions.CloseSerialPort(serialPort1);

            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;

            label1.Text = "Disconnect";
            label1.ForeColor = Color.Red;
            label9.Text = "Default";
            label9.ForeColor = Color.Brown;
            label17.Text = "Default";
            label17.ForeColor = Color.Brown;
            label18.Text = "Default";
            label18.ForeColor = Color.Brown;

            textBox1.Text = "10";
            textBox2.Text = "5";

            comboBox1.ResetText();
            comboBox2.SelectedIndex = 0;
            trackBar1.Value = 11;

            chart6.Series.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label9.Text = "Default";
            label9.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:DARKspectra " + textBox1.Text + " " + textBox2.Text + " format<CR>\r";
            double[] pixelData = new double[Parameters.dataCount];
            string choosingFilter = "Normal";
            int loopCount = 30;

            if (serialPort1.IsOpen)
            {
                try
                {
                    if (Functions.DigitalGainSetting(serialPort1, comboBox2.SelectedIndex.ToString()) == 6 && 
                        Functions.AnalogGainSetting(serialPort1, label15.Text) == 6)
                    {
                        label9.Text = "True,";
                        label9.ForeColor = Color.Green;
                    }
                    else
                    {
                        label9.Text = "False,";
                        label9.ForeColor = Color.Red;
                    }

                    pixelData = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, choosingFilter, loopCount, Parameters.dataLenght, Parameters.dataCount);

                    seriesIndex = chart6.Series.Count;
                    darkSeriesIndex = darkSeriesIndex + 1;

                    double graphicMinumumValue = Functions.GraphicMinimumMethod(pixelData);
                    double graphicMaximumValue = Functions.GraphicMaximumMethod(pixelData);
                    if (graphicMinumumValue < graphicMinimum)
                        graphicMinimum = (int)graphicMinumumValue + 1;
                    if (graphicMaximumValue > graphicMaximum)
                        graphicMaximum = (int)graphicMaximumValue + 1;
                    Functions.DrawGraphics(chart6, pixelData, "D", seriesIndex, darkSeriesIndex, graphicMinimum, graphicMaximum, 0, 0, 255);

                    label9.Text += "True";
                }
                catch (Exception)
                {
                    label9.Text = "Error";
                    label9.ForeColor = Color.Red;
                }
            }
            else
            {
                label9.Text = "Port Error";
                label9.ForeColor = Color.Red;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            label17.Text = "Default";
            label17.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + textBox1.Text + " " + textBox2.Text + " format<CR>\r";
            double[] ERefLed = new double[Parameters.dataCount];
            string choosingFilter = "Normal";
            int loopCount = 30;

            if (serialPort1.IsOpen)
            {
                try
                {
                    ERefLed = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, choosingFilter, loopCount, Parameters.dataLenght, Parameters.dataCount);

                    seriesIndex = chart6.Series.Count;
                    ELedSeriesIndex = ELedSeriesIndex + 1;

                    double graphicMinumumValue = Functions.GraphicMinimumMethod(ERefLed);
                    double graphicMaximumValue = Functions.GraphicMaximumMethod(ERefLed);
                    if (graphicMinumumValue < graphicMinimum)
                        graphicMinimum = (int)graphicMinumumValue + 1;
                    if (graphicMaximumValue > graphicMaximum)
                        graphicMaximum = (int)graphicMaximumValue + 1;
                    Functions.DrawGraphics(chart6, ERefLed, "E", seriesIndex, ELedSeriesIndex, graphicMinimum, graphicMaximum, 0, 255, 0);
                    
                    label17.Text = "True";
                    label17.ForeColor = Color.Green;
                }
                catch (Exception)
                {
                    label17.Text = "False";
                    label17.ForeColor = Color.Red;
                }
            }
            else
            {
                label17.Text = "Port Error";
                label17.ForeColor = Color.Red;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label18.Text = "Default";
            label18.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + textBox1.Text + " " + textBox2.Text + " format<CR>\r";
            double[] ERefLed = new double[Parameters.dataCount];
            string choosingFilter = "Normal";
            int loopCount = 30;

            if (serialPort1.IsOpen)
            {
                try
                {
                    ERefLed = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, choosingFilter, loopCount, Parameters.dataLenght, Parameters.dataCount);

                    seriesIndex = chart6.Series.Count;
                    standartSampleSeriesIndex = standartSampleSeriesIndex + 1;

                    double graphicMinumumValue = Functions.GraphicMinimumMethod(ERefLed);
                    double graphicMaximumValue = Functions.GraphicMaximumMethod(ERefLed);
                    if (graphicMinumumValue < graphicMinimum)
                        graphicMinimum = (int)graphicMinumumValue + 1;
                    if (graphicMaximumValue > graphicMaximum)
                        graphicMaximum = (int)graphicMaximumValue + 1;
                    Functions.DrawGraphics(chart6, ERefLed, "S", seriesIndex, standartSampleSeriesIndex, graphicMinimum, graphicMaximum, 255, 0, 0);
                    
                    label18.Text = "True";
                    label18.ForeColor = Color.Green;
                }
                catch (Exception)
                {
                    label18.Text = "False";
                    label18.ForeColor = Color.Red;
                }
            }
            else
            {
                label18.Text = "Port Error";
                label18.ForeColor = Color.Red;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            label9.Text = "Default";
            label9.ForeColor = Color.Brown;
            label17.Text = "Default";
            label17.ForeColor = Color.Brown;
            label18.Text = "Default";
            label18.ForeColor = Color.Brown;

            graphicMinimum = 0;
            graphicMaximum = 0;
            seriesIndex = 0;
            darkSeriesIndex = 0;
            ELedSeriesIndex = 0;
            standartSampleSeriesIndex = 0;
            chart6.Series.Clear();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label15.Text = trackbarValue.ToString();
        }

        private void ReadNmIndisRelatedPixel()
        {
            if (serialPort1.IsOpen)
            {
                nmCount = Functions.NmRelatedPixelCalculation(serialPort1, Parameters.dataLenght - 1, Parameters.dataCount, Parameters.firstNm);
                Shared.mainForm.SetText(Shared.mainForm.interfaceFileLog, "The nm values of the spectrometer to be measured have been read. (Meas Graphics)");
            }
        }
    }
}