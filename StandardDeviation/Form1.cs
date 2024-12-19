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

namespace StandardDeviation
{
    public partial class Form1 : Form
    {
        private List<double> waveList = new List<double>();
        private List<double> waveDiffPowList = new List<double>();

        public Thread thread;
        private bool threadState = true;

        private double firstPixelD = 0;
        private double lastPixelD = 0;
        private static double sumXt = 0;
        private static int parameterSettingErrorState = 2;
        private int firstTime = 0;
        private double trackbarValue = 0;

        public Form1()
        {
            InitializeComponent();

            groupBox4.Enabled = false;
            groupBox6.Enabled = false;
            groupBox3.Enabled = false;
            groupBox2.Enabled = false;
            groupBox5.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] serialPorts = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string serialPort in serialPorts)
                comboBox1.Items.Add(serialPort);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectPortName = comboBox1.Text;
            Functions.OpenSerialPort(serialPort1, selectPortName, 3000000);

            if (serialPort1.IsOpen)
            {
                label2.Text = "On";
                label2.ForeColor = Color.Blue;
                groupBox4.Enabled = true;
                groupBox6.Enabled = true;
                groupBox3.Enabled = true;
                groupBox2.Enabled = true;
                groupBox5.Enabled = true;
                textBox3.Text = "10";
                textBox4.Text = "1";
                comboBox2.SelectedIndex = 0;

                string[] pixelSettings = new string[2];
                string fileName = "PixelSettings.txt";
                pixelSettings = Functions.ReadFile(fileName);
                firstPixelD = Convert.ToDouble(pixelSettings[0]);
                label8.Text = pixelSettings[0];
                lastPixelD = Convert.ToDouble(pixelSettings[1]);
                label9.Text = pixelSettings[1];
            }
            else
            {
                label2.Text = "Off";
                label2.ForeColor = Color.Red;
                groupBox4.Enabled = false;
                groupBox6.Enabled = false;
                groupBox3.Enabled = false;
                groupBox2.Enabled = false;
                groupBox5.Enabled = false;
            }

            Functions.CloseSerialPort(serialPort1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            groupBox4.Enabled = false;
            groupBox6.Enabled = false;
            groupBox3.Enabled = false;
            groupBox2.Enabled = false;
            groupBox5.Enabled = false;

            label2.Text = "Default";
            label2.ForeColor = Color.FromArgb(128, 64, 0);
            label19.Text = "Default";
            label19.ForeColor = Color.FromArgb(128, 64, 0);
            label16.Text = "Default";
            label16.ForeColor = Color.FromArgb(128, 64, 0);
            label5.Text = "Default";
            label5.ForeColor = Color.FromArgb(128, 64, 0);
            label8.Text = "X";
            label9.Text = "X";

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                Functions.WriteFile("PixelSettings.txt", textBox1.Text, textBox2.Text);
                label5.Text = "Setting";
                label5.ForeColor = Color.FromArgb(59, 131, 27);
            }
            else
            {
                label5.Text = "Error";
                label5.ForeColor = Color.FromArgb(255, 0, 0);
            }

            string[] pixelSettings = new string[2];
            pixelSettings = Functions.ReadFile("PixelSettings.txt");
            firstPixelD = Convert.ToDouble(pixelSettings[0]);
            label8.Text = pixelSettings[0];
            lastPixelD = Convert.ToDouble(pixelSettings[1]);
            label9.Text = pixelSettings[1];

            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string choosingIntegration = textBox3.Text;
            string choosingDigitalGain = comboBox2.SelectedIndex.ToString();
            string choosingAnalogGain = label15.Text;
            int dataLenght = 2049;
            string sendData = "*MEASure:DARKspectra tint av format<CR>\r";

            string selectPortName = comboBox1.Text;
            Functions.OpenSerialPort(serialPort1, selectPortName, 3000000);

            parameterSettingErrorState = 2;
            if (Functions.IntegrationTimeSetting(serialPort1, choosingIntegration) == 6 && 
                Functions.DigitalGainSetting(serialPort1, choosingDigitalGain) == 6 && 
                Functions.AnalogGainSetting(serialPort1, choosingAnalogGain) == 6)
                parameterSettingErrorState = 1;
            else
                parameterSettingErrorState = 0;

            if (parameterSettingErrorState == 1)
            {
                label16.Text = "True";
                label16.ForeColor = Color.Blue;
            }
            else if (parameterSettingErrorState == 0)
            {
                label16.Text = "False";
                label16.ForeColor = Color.Red;
            }
            else if (parameterSettingErrorState == 2)
            {
                label16.Text = "Error";
                label16.ForeColor = Color.HotPink;
            }

            Functions.ShortDataProcess(serialPort1, sendData, dataLenght);
            Functions.CloseSerialPort(serialPort1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button7.Enabled = true;
            groupBox3.Enabled = false;
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series1"].Color = Color.Blue;
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            waveList.Clear();
            waveDiffPowList.Clear();

            label19.Text = "Start";
            label19.ForeColor = Color.Blue;
            label16.Text = "Default";
            label16.ForeColor = Color.FromArgb(128, 64, 0);

            parameterSettingErrorState = 2;

            int firstPixel = (int)firstPixelD - 1;
            int lastPixel = (int)lastPixelD;
            int dataCount = lastPixel - firstPixel;

            string sendData = "*MEASure:REFERence tint av format<CR>\r";
            int dataLenght = 2049;
            int averageValue = Convert.ToInt32(textBox4.Text);
            int sumLoopCount = Convert.ToInt32(textBox5.Text);
            string selectPortName = comboBox1.Text;
            Functions.OpenSerialPort(serialPort1, selectPortName, 3000000);

            listBox1.Items.Add("------------");
            listBox1.Items.Add(DateTime.Now.ToString());
            firstTime = (DateTime.Now.Hour * 60 * 60) + (DateTime.Now.Minute * 60) + (DateTime.Now.Second);

            threadState = true;
            if (thread != null && thread.IsAlive == true)
                return;
            thread = new Thread(() => Process(serialPort1, sendData, averageValue, sumLoopCount, dataLenght, dataCount, firstPixel));
            thread.Start();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            threadState = false;

            button5.Enabled = false;
            button6.Enabled = true;
            label19.Text = "Stop";
            label19.ForeColor = Color.Red;

            label18.Text = "Default";
            label18.ForeColor = Color.FromArgb(128, 64, 0);
            label16.Text = "Default";
            label16.ForeColor = Color.FromArgb(128, 64, 0);

            groupBox3.Enabled = true;

            for (int i = listBox1.Items.Count - 1; i >= 0; i = -2)
                Functions.WriteFile("Data.txt", listBox1.Items[i].ToString(), listBox1.Items[i - 1].ToString());
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label15.Text = trackbarValue.ToString();
        }

        private void Process(SerialPort serialPort, string sendData, int averageValue, int sumLoopCount, int dataLenght, int dataCount, int firstPixel)
        {
            while (true)
            {
                if (threadState == true)
                {
                    double averageWave = 0;
                    double wave = 0;
                    double wavePow = 0;
                    double standardDevisionWave = 0;
                    int lastTime = 0;

                    parameterSettingErrorState = 2;

                    wave = Functions.SumAverageWaveLoopCount(serialPort, sendData, averageValue, sumLoopCount, 
                           dataLenght, dataCount, firstPixel, firstPixelD);
                    SetText(listBox2, wave.ToString());

                    if (waveList.Count > 100)
                    {
                        waveList.Add(wave);
                        averageWave = waveList.Average();
                        waveDiffPowList.Clear();
                        for (int i = 0; i < waveList.Count; i++)
                        {
                            wavePow = waveList[i] - averageWave;
                            wavePow = Math.Pow(wavePow, 2);
                            waveDiffPowList.Add(wavePow);
                        }

                        standardDevisionWave = (waveDiffPowList.Sum() / (waveDiffPowList.Count - 1));
                        standardDevisionWave = Math.Sqrt(standardDevisionWave);
                        standardDevisionWave = Math.Round(standardDevisionWave, 4);

                        lastTime = (DateTime.Now.Hour * 60 * 60) + (DateTime.Now.Minute * 60) + (DateTime.Now.Second);
                        lastTime = lastTime - firstTime;

                        SetText(listBox1, standardDevisionWave.ToString());
                        SetChart(chart1, lastTime, standardDevisionWave);
                    }
                    else
                    {
                        waveList.Add(wave);
                    }
                }
                else
                {
                    Functions.CloseSerialPort(serialPort1);
                    break;
                }
            }
        }

        delegate void SetTextCallback(ListBox label, string text);
        private void SetText(ListBox label, string text)
        {
            if (label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(_SetText);
                label.Invoke(d, new object[] { label, text });
            }
            else
            {
                _SetText(label, text);
            }
        }

        private void _SetText(ListBox label, string text)
        {
            label.Items.Insert(0, text);
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
