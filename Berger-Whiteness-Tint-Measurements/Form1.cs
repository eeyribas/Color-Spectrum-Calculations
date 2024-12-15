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
using System.Windows.Forms.DataVisualization.Charting;

namespace Berger_Whiteness_Tint_Measurements
{
    public partial class Form1 : Form
    {
        public Thread thread;
        public bool threadState = false;

        public double[] standartPlate1 = new double[Parameters.dataCount];
        public double[] standartPlate2 = new double[Parameters.dataCount];
        public double[] ERsled = new double[Parameters.dataCount];
        public int[] nmCount = new int[Parameters.dataCount];

        public List<double> deltaEList1 = new List<double>();
        public List<double> deltaEList2 = new List<double>();
        public List<double> deltaEList3 = new List<double>();

        public int graphicsMaximumDelta = 0;
        public int firstTime = 0;
        public int deltaEValue = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GroupBoxEnableFalse();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadState = false;
            Functions.CloseSerialPort(serialPort1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Default";
            label1.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:DARKspectra " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
            double[] pixelSectionDataValue = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    if (Functions.DigitalGainSetting(serialPort1, Parameters.digitalGain) == 6 && 
                        Functions.AnalogGainSetting(serialPort1, Parameters.analogGain) == 6)
                        label1.Text = "True";
                    else
                        label1.Text = "False";

                    pixelSectionDataValue = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, Parameters.filter, Convert.ToInt32(Parameters.otherLoopCount), Parameters.dataLenght, Parameters.dataCount);
                }
                catch (Exception)
                {
                    label1.Text = "False";
                }
            }
            else
            {
                label1.Text = "Close";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label3.Text = "Default";
            label3.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
            double[] ERefLed = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    ERefLed = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, Parameters.filter, Convert.ToInt32(Parameters.otherLoopCount), Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < ERsled.Length; i++)
                    {
                        if (comboBox1.SelectedIndex == 0)
                            ERsled[i] = ERefLed[i] / standartPlate1[i];
                        else if (comboBox1.SelectedIndex == 1)
                            ERsled[i] = ERefLed[i] / standartPlate2[i];
                    }

                    label3.Text = "True";
                }
                catch (Exception)
                {
                    label3.Text = "False";
                }
            }
            else
            {
                label3.Text = "Close";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label8.Text = "Default";
            threadState = false;
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
            double[] RStandartSample = new double[Parameters.dataCount];
            double[] EStandartSample = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    EStandartSample = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount, Parameters.filter, Convert.ToInt32(Parameters.otherLoopCount), Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < RStandartSample.Length; i++)
                        RStandartSample[i] = EStandartSample[i] / ERsled[i];

                    double XStandartSample = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                             StandartValues.x2, StandartValues.y2, StandartValues.deltaLamda);
                    double YStandartSample = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                             StandartValues.y2, StandartValues.y2, StandartValues.deltaLamda);
                    double ZStandartSample = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                             StandartValues.z2, StandartValues.y2, StandartValues.deltaLamda);

                    double bergerValue = YStandartSample + (3.108 * ZStandartSample) - (3.831 * XStandartSample);
                    textBox1.Text = Math.Round(bergerValue, 4).ToString();

                    double XStandartSampleD65 = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.d65, 
                                                StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda);
                    double YStandartSampleD65 = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.d65, 
                                                StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda);
                    double ZStandartSampleD65 = Functions.XYZCalculation(RStandartSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.d65, 
                                                StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda);

                    double xyzSum = XStandartSampleD65 + YStandartSampleD65 + ZStandartSampleD65;
                    double x10 = XStandartSampleD65 / xyzSum;
                    double y10 = YStandartSampleD65 / xyzSum;
                    double WI = YStandartSampleD65 + (800 * (0.3138 - x10)) + (1700 * (0.3310 - y10));
                    textBox2.Text = Math.Round(WI, 4).ToString();

                    double TI = (900 * (0.3138 - x10)) - (650 * (0.3310 - y10));
                    textBox3.Text = Math.Round(TI, 4).ToString();

                    label4.Text = "True";
                }
                catch (Exception)
                {
                    label4.Text = "False";
                }
            }
            else
            {
                label4.Text = "Close";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox6.Text = "";
            textBox5.Text = "";
            textBox4.Text = "";

            label18.Text = "";
            label22.Text = "";
            label20.Text = "";
            label24.Text = "";
            label28.Text = "";
            label26.Text = "";

            chart3.Series["Series1"].Points.Clear();
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();

            deltaEList1.Clear();
            deltaEList2.Clear();
            deltaEList3.Clear();

            threadState = true;

            string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
            firstTime = (DateTime.Now.Hour * 60 * 60) + (DateTime.Now.Minute * 60) + (DateTime.Now.Second);

            if (serialPort1.IsOpen)
            {
                try
                {
                    label8.Text = "True";

                    if (thread != null && thread.IsAlive == true)
                        return;
                    thread = new Thread(() => Process(serialPort1, sendData, nmCount, Parameters.filter, Convert.ToInt32(Parameters.testLoopCount), Parameters.dataLenght, Parameters.dataCount));
                    thread.Start();
                }
                catch (Exception)
                {
                    label8.Text = "False";
                }
            }
            else
            {
                label8.Text += "Close";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            threadState = false;
            label8.Text = "Stop";
            label8.ForeColor = Color.Red;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReadStandartPlate1();
            ReadStandartPlate2();
            ReadSettings();

            if (Parameters.selectDevicePort == "A")
            {
                serialPort1.Close();
            }
            else
            {
                serialPort1.PortName = Parameters.selectDevicePort;
                Functions.OpenSerialPort(serialPort1, Parameters.selectDevicePort, 3000000);
            }

            if (!serialPort1.IsOpen)
            {
                GroupBoxEnableFalse();
            }
            else
            {
                ReadNmIndisRelatedPixel();

                comboBox1.SelectedIndex = 0;
                groupBox8.BringToFront();
                groupBox1.Enabled = true;
                groupBox3.Enabled = true;
                groupBox7.Enabled = true;
                groupBox8.Enabled = true;
                groupBox5.Enabled = true;
                groupBox2.Enabled = true;
                groupBox4.Enabled = true;
                groupBox6.Enabled = true;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Form3 form3 = new Form3();
            form3.ShowDialog();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Form4 form4 = new Form4();
            form4.ShowDialog();
        }

        private void Process(SerialPort serialPort, string sendData, int[] nmCountA, string choosingFilter, int loopCount, int dataLenght, int dataCount)
        {
            while (true)
            {
                if (threadState == true)
                {
                    double[] RTestSample = new double[dataCount];
                    double[] ETestSample = new double[dataCount];
                    int lastTime = 0;

                    ETestSample = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCountA, choosingFilter, loopCount, dataLenght, dataCount);
                    for (int i = 0; i < RTestSample.Length; i++)
                        RTestSample[i] = ETestSample[i] / ERsled[i];

                    double XTestSampleC = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                          StandartValues.x2, StandartValues.y2, StandartValues.deltaLamda);
                    double YTestSampleC = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                          StandartValues.y2, StandartValues.y2, StandartValues.deltaLamda);
                    double ZTestSampleC = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), StandartValues.CLight, 
                                          StandartValues.z2, StandartValues.y2, StandartValues.deltaLamda);

                    double bergerValue = YTestSampleC + (3.108 * ZTestSampleC) - (3.831 * XTestSampleC);
                    SetTextBox(textBox4, Math.Round(bergerValue, 4).ToString());

                    double XTestSampleD65 = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), 
                                            StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda);
                    double YTestSampleD65 = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), 
                                            StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda);
                    double ZTestSampleD65 = Functions.XYZCalculation(RTestSample, Convert.ToInt32(Parameters.firstNm), Convert.ToInt32(Parameters.lastNm), 
                                            StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda);

                    double xyzSum = XTestSampleD65 + YTestSampleD65 + ZTestSampleD65;
                    double x10 = XTestSampleD65 / xyzSum;
                    double y10 = YTestSampleD65 / xyzSum;
                    double WI = YTestSampleD65 + (800 * (0.3138 - x10)) + (1700 * (0.3310 - y10));
                    textBox5.Text = Math.Round(WI, 4).ToString();

                    double TI = (900 * (0.3138 - x10)) - (650 * (0.3310 - y10));
                    textBox6.Text = Math.Round(TI, 4).ToString();

                    SetLabel(label18, Math.Round(bergerValue, 4).ToString());
                    SetLabel(label22, Math.Round(WI, 4).ToString());
                    SetLabel(label26, Math.Round(TI, 4).ToString());

                    deltaEList1.Add(bergerValue);
                    deltaEList2.Add(WI);
                    deltaEList3.Add(TI);
                    SetLabel(label20, Math.Round(deltaEList1.Average(), 4).ToString());
                    SetLabel(label24, Math.Round(deltaEList2.Average(), 4).ToString());
                    SetLabel(label28, Math.Round(deltaEList3.Average(), 4).ToString());

                    SetChart(chart1, lastTime, Math.Round(bergerValue, 4));
                    SetChart(chart2, lastTime, Math.Round(WI, 4));
                    SetChart(chart3, lastTime, Math.Round(TI, 4));
                }
                else
                {
                    break;
                }
            }
        }

        private void ReadNmIndisRelatedPixel()
        {
            if (Parameters.selectDevicePort != "A")
                nmCount = Functions.NmRelatedPixelCalculation(serialPort1, Parameters.dataLenght - 1, Parameters.dataCount, Parameters.defaultFirstNm);
        }

        private void ReadSettings()
        {
            string fileName = "Settings.txt";
            int textCount = 10;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 9);

            Parameters.selectDevicePort = readData[0];
            Parameters.integrationTime = label13.Text = readData[1];
            Parameters.averageScan = label16.Text = readData[2];
            Parameters.digitalGain = readData[3];
            Parameters.analogGain = readData[4];
            Parameters.firstNm = readData[5];
            Parameters.lastNm = readData[6];
            Parameters.testLoopCount = readData[7];
            Parameters.otherLoopCount = readData[8];
            Parameters.filter = readData[9];
        }

        private void ReadStandartPlate1()
        {
            string fileName = "PlateValues1.txt";
            int textCount = 81;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 80);
            for (int i = 0; i < textCount; i++)
                standartPlate1[i] = Convert.ToDouble(readData[i]);
        }

        private void ReadStandartPlate2()
        {
            string fileName = "PlateValues2.txt";
            int textCount = 81;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 80);
            for (int i = 0; i < textCount; i++)
                standartPlate2[i] = Convert.ToDouble(readData[i]);
        }

        private void DisconnectionFunction()
        {
            threadState = false;
            Functions.CloseSerialPort(serialPort1);

            GroupBoxEnableFalse();

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox6.Text = "";
            textBox5.Text = "";
            textBox4.Text = "";

            label1.Text = "Default";
            label1.ForeColor = Color.Brown;
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;
            label3.Text = "Default";
            label3.ForeColor = Color.Brown;
            label8.Text = "Default";
            label8.ForeColor = Color.Brown;
            label13.Text = "X";
            label16.Text = "X";
            label18.Text = "X";
            label22.Text = "X";
            label20.Text = "X";
            label24.Text = "X";
            label28.Text = "X";
            label26.Text = "X";

            chart3.Series["Series1"].Points.Clear();
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();

            comboBox1.SelectedIndex = 0;
        }

        private void GroupBoxEnableFalse()
        {
            groupBox1.Enabled = false;
            groupBox3.Enabled = false;
            groupBox7.Enabled = false;
            groupBox5.Enabled = false;
            groupBox2.Enabled = false;
            groupBox8.Enabled = false;
            groupBox4.Enabled = false;
            groupBox6.Enabled = false;
        }

        delegate void SetChartCallback(Chart chart, int time, double value);
        private void SetChart(Chart chart, int time, double value)
        {
            if (chart.InvokeRequired)
            {
                SetChartCallback d = new SetChartCallback(_SetChart);
                chart.Invoke(d, new object[] { chart, time, value });
            }
            else
            {
                _SetChart(chart, time, value);
            }
        }

        private void _SetChart(Chart chart, int time, double value)
        {
            chart.Series[0].Points.AddXY(time, value);
        }

        delegate void SetTextBoxCallback(TextBox textBox, string text);
        private void SetTextBox(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                SetTextBoxCallback d = new SetTextBoxCallback(_SetTextBox);
                textBox.Invoke(d, new object[] { textBox, text });
            }
            else
            {
                _SetTextBox(textBox, text);
            }
        }

        private void _SetTextBox(TextBox textBox, string text)
        {
            textBox.Text = text;
        }

        delegate void SetLabelCallback(Label label, string text);
        private void SetLabel(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                SetLabelCallback d = new SetLabelCallback(_SetLabel);
                label.Invoke(d, new object[] { label, text });
            }
            else
            {
                _SetLabel(label, text);
            }
        }

        private void _SetLabel(Label label, string text)
        {
            label.Text = text;
        }
    }
}
