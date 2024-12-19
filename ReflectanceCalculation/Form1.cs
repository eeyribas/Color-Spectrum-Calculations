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
using System.Windows.Forms.DataVisualization.Charting;

namespace ReflectanceCalculation
{
    public partial class Form1 : Form
    {
        private double[] pixelSectionData = new double[61];

        private int firstNm = 400;
        private int lastNm = 700;
        private double firstPixelD = 0;
        private double lastPixelD = 0;
        private double trackbarValue = 0;

        private int seriesIndex = 0;
        private int refSeriesIndex1 = 0;
        private int refSeriesIndex2 = 0;
        private int refSeriesIndex3 = 0;
        private double graphicMinimum = 0;
        private double graphicMaximum = 0;
        
        public Form1()
        {
            InitializeComponent();

            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
            groupBox3.Enabled = false;
            groupBox2.Enabled = false;
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

            if (serialPort1.IsOpen)
            {
                label2.Text = "On";
                label2.ForeColor = Color.Green;

                groupBox4.Enabled = true;
                groupBox5.Enabled = true;
                groupBox3.Enabled = true;
                groupBox2.Enabled = true;
                groupBox6.Enabled = true;

                comboBox2.SelectedIndex = 0;

                string[] pixelSetting = new string[2];
                string filenameRead = "PixelSettings.txt";
                pixelSetting = Functions.ReadFile(filenameRead);
                firstPixelD = Convert.ToDouble(pixelSetting[0]);
                label7.Text = pixelSetting[0];
                lastPixelD = Convert.ToDouble(pixelSetting[1]);
                label8.Text = pixelSetting[1];
            }
            else
            {
                label2.Text = "Off";
                label2.ForeColor = Color.Red;

                groupBox4.Enabled = false;
                groupBox5.Enabled = false;
                groupBox3.Enabled = false;
                groupBox2.Enabled = false;
                groupBox6.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Functions.CloseSerialPort(serialPort1);

            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
            groupBox3.Enabled = false;
            groupBox2.Enabled = false;
            groupBox6.Enabled = false;

            comboBox2.SelectedIndex = 0;

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();

            textBox4.Text = "10";
            textBox5.Text = "1";
            textBox3.Text = "1";

            label2.Text = "Off";
            label2.ForeColor = Color.Red;
            label7.Text = "X";
            label8.Text = "X";

            label19.Text = "Default";
            label19.ForeColor = Color.FromArgb(128, 64, 0);
            label18.Text = "Default";
            label18.ForeColor = Color.FromArgb(128, 64, 0);
            label9.Text = "Default";
            label9.ForeColor = Color.FromArgb(128, 64, 0);
            label20.Text = "Default";
            label20.ForeColor = Color.FromArgb(128, 64, 0);
            label23.Text = "Default";
            label23.ForeColor = Color.FromArgb(128, 64, 0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                Functions.WriteFile("PixelSettings.txt", textBox1.Text, textBox2.Text);
                label9.Text = "Setting";
                label9.ForeColor = Color.Green;
            }
            else
            {
                label9.Text = "Error";
                label9.ForeColor = Color.Red;
            }

            string[] pixelSettings = new string[2];
            pixelSettings = Functions.ReadFile("PixelSettings.txt");
            firstPixelD = Convert.ToDouble(pixelSettings[0]);
            label7.Text = pixelSettings[0];
            lastPixelD = Convert.ToDouble(pixelSettings[1]);
            label8.Text = pixelSettings[1];

            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                string sendData = "*MEASure:DARKspectra tint av format<CR>\r";
                string choosingIntegration = textBox4.Text;
                string choosingDigitalGain = comboBox2.SelectedIndex.ToString();
                string choosingAnalogGain = label17.Text;
                int dataLenght = 2049;

                if (Functions.IntegrationTimeSetting(serialPort1, choosingIntegration) == 6 && 
                    Functions.DigitalGainSetting(serialPort1, choosingDigitalGain) == 6 && 
                    Functions.AnalogGainSetting(serialPort1, choosingAnalogGain) == 6)
                {
                    label18.Text = "True";
                    label18.ForeColor = Color.Green;
                }
                else
                {
                    label18.Text = "False";
                    label18.ForeColor = Color.Red;
                }

                Functions.ShortDataProcess(serialPort1, sendData, dataLenght);
                label19.Text = "True";
                label19.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label19.Text = "False";
                label19.ForeColor = Color.Red;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                chart1.Series.Clear();
                listBox1.Items.Clear();
                listBox2.Items.Clear();

                string sendData = "*MEASure:REFERence tint av format<CR>\r";
                int averageValue = Convert.ToInt32(textBox5.Text);
                int loopCount = Convert.ToInt32(textBox3.Text);
                int firstPixel = (int)firstPixelD - 1;
                int lastPixel = (int)lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelNmSectionDataValue = new double[section * 2];
                double[] nmSectionDataValue = new double[61];
                int s = 0;
                int dataLenght = 2049;
                seriesIndex = chart1.Series.Count;
                refSeriesIndex1 = refSeriesIndex1 + 1;

                pixelNmSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, averageValue, section, loopCount, dataLenght, dataCount, firstPixel, firstPixelD);

                for (int m = 20; m < (pixelSectionData.Length * 2) + 20; m = m + 2)
                {
                    pixelSectionData[s] = pixelNmSectionDataValue[m];
                    nmSectionDataValue[s] = pixelNmSectionDataValue[m + 1];
                    listBox2.Items.Add(pixelSectionData[s]);
                    listBox1.Items.Add(nmSectionDataValue[s]);
                    s++;
                }

                chart1.Series.Add("Standart " + (refSeriesIndex1).ToString());
                chart1.ChartAreas[0].AxisX.Interval = 10;
                chart1.ChartAreas[0].AxisX.Minimum = firstNm;
                chart1.ChartAreas[0].AxisX.Maximum = lastNm;

                graphicMinimum = pixelSectionData[0];
                graphicMaximum = pixelSectionData[0];
                for (int j = 0; j < pixelSectionData.Length; j++)
                {
                    if (graphicMinimum > pixelSectionData[j])
                        graphicMinimum = pixelSectionData[j];
                    if (graphicMaximum < pixelSectionData[j])
                        graphicMaximum = pixelSectionData[j];
                }

                chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
                chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
                for (int i = 0; i < pixelSectionData.Length; i++)
                {
                    chart1.Series[seriesIndex].ChartType = SeriesChartType.Line;
                    chart1.Series[seriesIndex].Color = Color.FromArgb(255, 0, 0);
                    chart1.Series[seriesIndex].Points.AddXY(nmSectionDataValue[i], pixelSectionData[i]);
                }

                label20.Text = "True";
                label20.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label20.Text = "False";
                label20.ForeColor = Color.Red;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                listBox3.Items.Clear();
                listBox4.Items.Clear();
                listBox5.Items.Clear();

                string sendData = "*MEASure:REFERence tint av format<CR>\r";
                int averageValue = Convert.ToInt32(textBox5.Text);
                int loopCount = Convert.ToInt32(textBox3.Text);
                int firstPixel = (int)firstPixelD - 1;
                int lastPixel = (int)lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelNmSectionDataValue = new double[section * 2];
                double[] pixelSectionDataValue = new double[61];
                double[] nmSectionDataValue = new double[61];
                int s = 0;
                int dataLenght = 2049;
                seriesIndex = chart1.Series.Count;
                refSeriesIndex2 = refSeriesIndex2 + 1;
                refSeriesIndex3 = refSeriesIndex3 + 1;

                pixelNmSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, averageValue, section, loopCount, 
                                          dataLenght, dataCount, firstPixel, firstPixelD);

                for (int m = 20; m < (pixelSectionDataValue.Length * 2) + 20; m = m + 2)
                {
                    pixelSectionDataValue[s] = pixelNmSectionDataValue[m];
                    nmSectionDataValue[s] = pixelNmSectionDataValue[m + 1];
                    listBox4.Items.Add(pixelSectionDataValue[s]);
                    listBox3.Items.Add(nmSectionDataValue[s]);
                    s++;
                }

                chart1.Series.Add("Sample " + (refSeriesIndex2).ToString());
                chart1.ChartAreas[0].AxisX.Interval = 10;
                chart1.ChartAreas[0].AxisX.Minimum = firstNm;
                chart1.ChartAreas[0].AxisX.Maximum = lastNm;

                graphicMinimum = pixelSectionDataValue[0];
                graphicMaximum = pixelSectionDataValue[0];
                for (int j = 0; j < pixelSectionDataValue.Length; j++)
                {
                    if (graphicMinimum > pixelSectionDataValue[j])
                        graphicMinimum = pixelSectionDataValue[j];
                    if (graphicMaximum < pixelSectionDataValue[j])
                        graphicMaximum = pixelSectionDataValue[j];
                }

                chart1.ChartAreas[0].AxisY.Minimum = graphicMinimum;
                chart1.ChartAreas[0].AxisY.Maximum = graphicMaximum;
                for (int i = 0; i < pixelSectionDataValue.Length; i++)
                {
                    chart1.Series[seriesIndex].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart1.Series[seriesIndex].Color = Color.FromArgb(0, 255, 0);
                    chart1.Series[seriesIndex].Points.AddXY(nmSectionDataValue[i], pixelSectionDataValue[i]);
                }

                double[] pixelRange = new double[61];
                for (int k = 0; k < pixelSectionDataValue.Length; k++)
                {
                    pixelRange[k] = (pixelSectionDataValue[k] / pixelSectionData[k]) * Functions.whitePlateValues[k];
                    listBox5.Items.Add(pixelRange[k]);
                }

                Functions.WriteFile("Data.txt", nmSectionDataValue, pixelRange);

                label23.Text = "True";
                label23.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label23.Text = "False";
                label23.ForeColor = Color.Red;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            refSeriesIndex1 = 0;
            refSeriesIndex2 = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackbarValue = trackBar1.Value;
            trackbarValue = trackbarValue / 10;
            label17.Text = trackbarValue.ToString();
        }
    }
}
