using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectrometerReader
{
    public partial class Form1 : Form
    {
        private Random random = new Random();
        private int firstNm = 350;
        private int lastNm = 850;

        private double firstPixelValue = 0;
        private double lastPixelValue = 0;

        private double graphicMinimum = 0;
        private double graphicMaximum = 0;

        private int seriesIndex = 0;
        private int darkSeriesIndex = 0;
        private int referenceSeriesIndex = 0;
        private int sampleSeriesIndex = 0;
        private int graphicSeriesIndex = 0;

        private double trackbarValue = 0;
        private int count = 0;
        private int countValue = 0;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            groupBox2.Enabled = false;
            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
            groupBox6.Enabled = false;
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

            if (!serialPort1.IsOpen)
            {
                label2.Text = "Off";
                label2.ForeColor = Color.Red;
                groupBox2.Enabled = false;
                groupBox4.Enabled = false;
                groupBox5.Enabled = false;
                groupBox6.Enabled = false;
                textBox7.Text = "";
            }
            else
            {
                label2.Text = "On";
                label2.ForeColor = Color.Green;
                groupBox2.Enabled = true;
                groupBox4.Enabled = true;
                groupBox5.Enabled = true;
                groupBox6.Enabled = true;

                DeviceInformations();
                textBox5.Text = "10";
                textBox6.Text = "1";
                textBox7.Text = "100";
                button5.Enabled = false;
                button6.Enabled = false;

                string[] pixelSetting = new string[2];
                string fileName = "PixelSettings.txt";
                pixelSetting = Functions.ReadFile(fileName);
                firstPixelValue = Convert.ToDouble(pixelSetting[0]);
                label24.Text = pixelSetting[0];
                lastPixelValue = Convert.ToDouble(pixelSetting[1]);
                label25.Text = pixelSetting[1];
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Functions.CloseSerialPort(serialPort1);

            groupBox2.Enabled = false;
            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
            groupBox6.Enabled = false;

            label2.Text = "Off";
            label2.ForeColor = Color.Red;
            label3.Text = "Default";
            label3.ForeColor = Color.FromArgb(128, 64, 0);
            label15.Text = "1.1";
            label16.Text = "Default";
            label20.Text = "Device ID:  ";
            label21.Text = "Version:  ";
            label23.Text = "X";
            label22.Text = "X";
            label24.Text = "";
            label25.Text = "";

            textBox4.Text = "";
            textBox3.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";

            chart1.Series.Clear();
            comboBox2.SelectedIndex = -1;
            trackBar1.Value = 11;
            checkBox1.Checked = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button5.Enabled = true;

            string choosingIntegration = textBox5.Text;
            string choosingDigitalGain = comboBox2.SelectedIndex.ToString();
            string choosingAnalogGain = label15.Text;
            int averageValue = Convert.ToInt32(textBox6.Text);

            if (Functions.IntegrationTimeSetting(serialPort1, choosingIntegration) == 6 && 
                Functions.DigitalGainSetting(serialPort1, choosingDigitalGain) == 6 && 
                Functions.AnalogGainSetting(serialPort1, choosingAnalogGain) == 6)
            {
                label16.Text = "True";
                label16.ForeColor = Color.Blue;
            }
            else
            {
                label16.Text = "False";
                label16.ForeColor = Color.Red;
            }

            string sendData = "*MEASure:DARKspectra tint av format<CR>\r";
            int firstPixel = (int)firstPixelValue - 1;
            int lastPixel = (int)lastPixelValue;
            int dataCount = lastPixel - firstPixel;
            double[] pixelDataValue = new double[dataCount];
            double[] nmDataValue = new double[dataCount];

            int darkDataLenght = 2049;
            seriesIndex = chart1.Series.Count;
            darkSeriesIndex = darkSeriesIndex + 1;

            pixelDataValue = Functions.PixelDataProcess(serialPort1, sendData, averageValue, darkDataLenght, dataCount, firstPixel);
            nmDataValue = Functions.NmCalculation(dataCount, firstPixel, firstPixelValue);

            chart1.Series.Add("Dark " + (darkSeriesIndex).ToString());
            chart1.ChartAreas[0].AxisX.Interval = 10;
            chart1.ChartAreas[0].AxisX.Minimum = firstNm;
            chart1.ChartAreas[0].AxisX.Maximum = lastNm;

            graphicMinimum = pixelDataValue[0];
            graphicMaximum = pixelDataValue[0];
            for (int j = 0; j < pixelDataValue.Length; j++)
            {
                if (graphicMinimum > pixelDataValue[j])
                    graphicMinimum = pixelDataValue[j];
                if (graphicMaximum < pixelDataValue[j])
                    graphicMaximum = pixelDataValue[j];
            }

            chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
            chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
            for (int i = 0; i < pixelDataValue.Length; i++)
            {
                chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[seriesIndex].Color = Color.FromArgb(random.Next(0, 180), random.Next(0, 180), random.Next(0, 180));
                chart1.Series[seriesIndex].Points.AddXY(nmDataValue[i], pixelDataValue[i]);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button6.Enabled = true;

            string sendData = "*MEASure:REFERence tint av format<CR>\r";
            int averageValue = Convert.ToInt32(textBox6.Text);
            int firstPixel = (int)firstPixelValue - 1;
            int lastPixel = (int)lastPixelValue;
            int dataCount = lastPixel - firstPixel;
            double[] pixelDataValue = new double[dataCount];
            double[] nmDataValue = new double[dataCount];

            int referenceDataLenght = 2049;
            seriesIndex = chart1.Series.Count;
            referenceSeriesIndex = referenceSeriesIndex + 1;

            pixelDataValue = Functions.PixelDataProcess(serialPort1, sendData, averageValue, referenceDataLenght, dataCount, firstPixel);
            nmDataValue = Functions.NmCalculation(dataCount, firstPixel, firstPixelValue);

            chart1.Series.Add("Reference " + (referenceSeriesIndex).ToString());
            chart1.ChartAreas[0].AxisX.Interval = 10;
            chart1.ChartAreas[0].AxisX.Minimum = firstNm;
            chart1.ChartAreas[0].AxisX.Maximum = lastNm;

            graphicMinimum = pixelDataValue[0];
            graphicMaximum = pixelDataValue[0];
            for (int j = 0; j < pixelDataValue.Length; j++)
            {
                if (graphicMinimum > pixelDataValue[j])
                    graphicMinimum = pixelDataValue[j];
                if (graphicMaximum < pixelDataValue[j])
                    graphicMaximum = pixelDataValue[j];
            }

            chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
            chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
            for (int i = 0; i < pixelDataValue.Length; i++)
            {
                chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[seriesIndex].Color = Color.FromArgb(random.Next(0, 180), random.Next(0, 180), random.Next(0, 180));
                chart1.Series[seriesIndex].Points.AddXY(nmDataValue[i], pixelDataValue[i]);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string sendData = "*MEASure:REFERence tint av format<CR>\r";
            int averageValue = Convert.ToInt32(textBox6.Text);
            int firstPixel = (int)firstPixelValue - 1;
            int lastPixel = (int)lastPixelValue;
            int dataCount = lastPixel - firstPixel;
            double[] pixelDataValue = new double[dataCount];
            double[] nmDataValue = new double[dataCount];

            int sampleDataLenght = 2049;
            seriesIndex = chart1.Series.Count;
            sampleSeriesIndex = sampleSeriesIndex + 1;

            pixelDataValue = Functions.PixelDataProcess(serialPort1, sendData, averageValue, sampleDataLenght, dataCount, firstPixel);
            nmDataValue = Functions.NmCalculation(dataCount, firstPixel, firstPixelValue);

            chart1.Series.Add("Sample " + (sampleSeriesIndex).ToString());
            chart1.ChartAreas[0].AxisX.Interval = 10;
            chart1.ChartAreas[0].AxisX.Minimum = firstNm;
            chart1.ChartAreas[0].AxisX.Maximum = lastNm;

            graphicMinimum = pixelDataValue[0];
            graphicMaximum = pixelDataValue[0];
            for (int j = 0; j < pixelDataValue.Length; j++)
            {
                if (graphicMinimum > pixelDataValue[j])
                    graphicMinimum = pixelDataValue[j];
                if (graphicMaximum < pixelDataValue[j])
                    graphicMaximum = pixelDataValue[j];
            }

            chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
            chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
            for (int i = 0; i < pixelDataValue.Length; i++)
            {
                chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[seriesIndex].Color = Color.FromArgb(random.Next(0, 180), random.Next(0, 180), random.Next(0, 180));
                chart1.Series[seriesIndex].Points.AddXY(nmDataValue[i], pixelDataValue[i]);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                string writeFileName = "PixelSettings.txt";
                Functions.WriteFile(writeFileName, textBox1.Text, textBox2.Text);
                label6.Text = "Setting";
                label6.ForeColor = Color.FromArgb(59, 131, 27);
            }
            else
            {
                label6.Text = "Error";
                label6.ForeColor = Color.FromArgb(255, 0, 0);
            }

            string[] pixelSetting = new string[2];
            string fileName = "pixelsetting.txt";
            pixelSetting = Functions.ReadFile(fileName);
            firstPixelValue = Convert.ToDouble(pixelSetting[0]);
            label24.Text = pixelSetting[0];
            lastPixelValue = Convert.ToDouble(pixelSetting[1]);
            label25.Text = pixelSetting[1];

            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Minimum = Convert.ToInt32(textBox3.Text);
            chart1.ChartAreas[0].AxisX.Maximum = Convert.ToInt32(textBox4.Text);
            chart1.ChartAreas[0].AxisX.Interval = 10;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            darkSeriesIndex = 0;
            referenceSeriesIndex = 0;
            sampleSeriesIndex = 0;
            textBox3.Text = "";
            textBox4.Text = "";
            button5.Enabled = false;
            button6.Enabled = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                groupBox2.Enabled = false;
                groupBox5.Enabled = false;
                darkSeriesIndex = 0;
                referenceSeriesIndex = 0;
                sampleSeriesIndex = 0;
                textBox3.Text = "";
                textBox4.Text = "";
                label19.Text = "0";
                chart1.Series.Clear();

                timer1.Enabled = true;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
                timer1.Enabled = false;

                groupBox2.Enabled = true;
                groupBox5.Enabled = true;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label15.Text = trackbarValue.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (textBox7.Text == "")
                countValue = 0;
            else
                countValue = Convert.ToInt32(textBox7.Text);


            if (count < countValue)
            {
                DataGraphics();
                count++;
            }
            else
            {
                count = 0;
                DataGraphicsZero();
            }

            label19.Text = count.ToString();
        }

        private void DeviceInformations()
        {
            string idn = "*IDN?<CR>\r";
            string version = "*VERSion?<CR>\r";
            string spnumberSerialNumber = "*PARAmeter:SPNUMber?<CR>\r";
            string deviceID = "*PARAmeter:SERNumber?<CR>\r";

            label20.Text = "Device ID:  " + Functions.DataDefinition(serialPort1, idn, 20);
            label21.Text = "Version:  " + Functions.DataDefinition(serialPort1, version, 33);
            label23.Text = Functions.DataDefinition(serialPort1, spnumberSerialNumber, 9);
            label22.Text = Functions.DataDefinition(serialPort1, deviceID, 9);

        }

        private void DataGraphics()
        {
            string sendDataG = "*MEASure:DARKspectra tint av format<CR>\r";
            int averageValue = Convert.ToInt32(textBox6.Text);
            int firstPixel = (int)firstPixelValue - 1;
            int lastPixel = (int)lastPixelValue;
            int dataCountG = lastPixel - firstPixel;
            double[] pixelDataValueG = new double[dataCountG];
            double[] nmDataValueG = new double[dataCountG];

            int graphicDataLenghtG = 2049;
            seriesIndex = chart1.Series.Count;
            graphicSeriesIndex = graphicSeriesIndex + 1;

            pixelDataValueG = Functions.PixelDataProcess(serialPort1, sendDataG, averageValue, graphicDataLenghtG, dataCountG, firstPixel);
            nmDataValueG = Functions.NmCalculation(dataCountG, firstPixel, firstPixelValue);

            chart1.Series.Add("Series" + (graphicSeriesIndex).ToString());
            chart1.ChartAreas[0].AxisX.Interval = 10;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisX.Minimum = firstNm;
            chart1.ChartAreas[0].AxisX.Maximum = lastNm;

            graphicMinimum = pixelDataValueG[0];
            graphicMaximum = pixelDataValueG[0];
            for (int j = 0; j < pixelDataValueG.Length; j++)
            {
                if (graphicMinimum > pixelDataValueG[j])
                    graphicMinimum = pixelDataValueG[j];
                if (graphicMaximum < pixelDataValueG[j])
                    graphicMaximum = pixelDataValueG[j];
            }

            chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
            chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
            for (int i = 0; i < pixelDataValueG.Length; i++)
            {
                chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[seriesIndex].Color = Color.FromArgb(84, 84, 84);
                chart1.Series[seriesIndex].Points.AddXY(nmDataValueG[i], pixelDataValueG[i]);
                chart1.Series[seriesIndex].IsVisibleInLegend = false;
            }
        }

        private void DataGraphicsZero()
        {
            chart1.Series.Clear();

            string choosingIntegration = textBox5.Text;
            string choosingDigitalGain = comboBox2.SelectedIndex.ToString();
            string choosingAnalogGain = label15.Text;
            int averageValue = Convert.ToInt32(textBox6.Text);

            if (Functions.IntegrationTimeSetting(serialPort1, choosingIntegration) == 6 && 
                Functions.DigitalGainSetting(serialPort1, choosingDigitalGain) == 6 && 
                Functions.AnalogGainSetting(serialPort1, choosingAnalogGain) == 6)
            {
                label16.Text = "True";
                label16.ForeColor = Color.Blue;
            }
            else
            {
                label16.Text = "False";
                label16.ForeColor = Color.Red;
            }

            string sendDataZ = "*MEASure:DARKspectra tint av format<CR>\r";
            int firstPixel = (int)firstPixelValue - 1;
            int lastPixel = (int)lastPixelValue;
            int dataCountZ = lastPixel - firstPixel;

            double[] pixelDataValueZ = new double[dataCountZ];
            double[] nmDataValueZ = new double[dataCountZ];

            int graphicDataLenghtZ = 2049;
            seriesIndex = chart1.Series.Count;
            graphicSeriesIndex = graphicSeriesIndex + 1;

            pixelDataValueZ = Functions.PixelDataProcess(serialPort1, sendDataZ, averageValue, graphicDataLenghtZ, dataCountZ, firstPixel);
            nmDataValueZ = Functions.NmCalculation(dataCountZ, firstPixel, firstPixelValue);

            chart1.Series.Add("Series" + (graphicSeriesIndex).ToString());
            chart1.ChartAreas[0].AxisX.Interval = 10;
            chart1.ChartAreas[0].AxisX.Minimum = firstNm;
            chart1.ChartAreas[0].AxisX.Maximum = lastNm;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            graphicMinimum = pixelDataValueZ[0];
            graphicMaximum = pixelDataValueZ[0];
            for (int j = 0; j < pixelDataValueZ.Length; j++)
            {
                if (graphicMinimum > pixelDataValueZ[j])
                    graphicMinimum = pixelDataValueZ[j];
                if (graphicMaximum < pixelDataValueZ[j])
                    graphicMaximum = pixelDataValueZ[j];
            }

            chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
            chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
            for (int i = 0; i < pixelDataValueZ.Length; i++)
            {
                chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[seriesIndex].Color = Color.FromArgb(84, 84, 84);
                chart1.Series[seriesIndex].Points.AddXY(nmDataValueZ[i], 0);
                chart1.Series[seriesIndex].IsVisibleInLegend = false;
            }
        }
    }
}
