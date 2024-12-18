using MultiSpectrometerReader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MultiSpectrometerReader.Forms
{
    public partial class MainForm : Form
    {
        public Thread thread1;
        public Thread thread2;
        public Thread thread3;
        public Thread thread4;
        public Thread thread5;
        public bool threadState1 = false;
        public bool threadState2 = false;
        public bool threadState3 = false;
        public bool threadState4 = false;
        public bool threadState5 = false;

        public double[] standartPlate1 = new double[Parameters.dataCount];
        public double[] standartPlate2 = new double[Parameters.dataCount];
        public double[] ERsled1 = new double[Parameters.dataCount];
        public double[] ERsled2 = new double[Parameters.dataCount];
        public double[] ERsled3 = new double[Parameters.dataCount];

        public int[] nmCount1 = new int[Parameters.dataCount];
        public int[] nmCount2 = new int[Parameters.dataCount];
        public int[] nmCount3 = new int[Parameters.dataCount];

        public List<double> deltaEList1 = new List<double>();
        public List<double> deltaEList2 = new List<double>();
        public List<double> deltaEList3 = new List<double>();
        public List<double> deltaEListAverage1 = new List<double>();
        public List<double> deltaEListAverage2 = new List<double>();
        public List<double> deltaEListAverage3 = new List<double>();
        public List<double> deltaEList12 = new List<double>();
        public List<double> deltaEList13 = new List<double>();
        public List<double> deltaEList23 = new List<double>();
        public List<double> deltaEListAverage13 = new List<double>();
        public List<double> deltaEListAverage12 = new List<double>();
        public List<double> deltaEListAverage23 = new List<double>();

        public List<double> deltaEDatabaseList1 = new List<double>();
        public List<double> deltaEDatabaseList2 = new List<double>();
        public List<double> deltaEDatabaseList3 = new List<double>();
        public List<double> deltaEDatabaseList12 = new List<double>();
        public List<double> deltaEDatabaseList13 = new List<double>();
        public List<double> deltaEDatabaseList23 = new List<double>();

        public List<double> lTestSampleList1 = new List<double>();
        public List<double> aTestSampleList1 = new List<double>();
        public List<double> bTestSampleList1 = new List<double>();
        public List<double> lTestSampleList2 = new List<double>();
        public List<double> aTestSampleList2 = new List<double>();
        public List<double> bTestSampleList2 = new List<double>();
        public List<double> lTestSampleList3 = new List<double>();
        public List<double> aTestSampleList3 = new List<double>();
        public List<double> bTestSampleList3 = new List<double>();

        public int graphicsMaximumDelta = 0;
        public int firstTime = 0;
        public int deltaEValue = 1;

        private Object serialPortLock = new Object();
        public byte[] buff = new byte[7];

        public string mainFileLog = Path.Combine(@"C:\", @"Data Log");
        public string measFileLog = Path.Combine(@"C:\", @"Data Log", "MEASUREMENT");
        public string interfaceFileLog = Path.Combine(@"C:\", @"Data Log", "LogFile.txt");

        private KeyValuePair<Process, int> processIDToCall = new KeyValuePair<Process, int>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string driveLetter = @"C:\";
            if (!Directory.Exists(driveLetter))
            {
                mainFileLog = Path.Combine(driveLetter, @"Data Log");
                SetText(interfaceFileLog, "Data Log folder has been created.");
            }
            if (!Directory.Exists(measFileLog))
            {
                Directory.CreateDirectory(measFileLog);
                SetText(interfaceFileLog, "Data Log-Log File.txt text file has been created.");
            }

            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            List<int> processIDs = ProcessID(appName);
            if (processIDs.Count > 0)
            {
                Process process = Process.GetProcessById(processIDs[0]);
                processIDToCall = new KeyValuePair<Process, int>(process, processIDs[0]);
            }
            SetText(interfaceFileLog, "Process ID value has been defined.");

            SetText(interfaceFileLog, "Main Form has been opened.");
            GroupBoxEnableFalse();
            CreateStorage();
            ReadSettings();

            if (CheckSerialPort() == true)
                SetText(interfaceFileLog, "Card connection has been established.");
            else
                SetText(interfaceFileLog, "Card connection error occurred.");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetText(interfaceFileLog, "Main Form is being closed.");
            threadState1 = false;
            threadState2 = false;
            threadState3 = false;
            threadState4 = false;
            threadState5 = false;
            Functions.CloseSerialPort(serialPort1);
            Functions.CloseSerialPort(serialPort2);
            Functions.CloseSerialPort(serialPort3);
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Default";
            label1.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:DARKspectra " + Shared.jsonData.IntegrationTime + " " + Shared.jsonData.AverageScan + " format<CR>\r";
            double[] pixelSectionDataValue1 = new double[Parameters.dataCount];
            double[] pixelSectionDataValue2 = new double[Parameters.dataCount];
            double[] pixelSectionDataValue3 = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    if (Functions.DigitalGainSetting(serialPort1, Shared.jsonData.DigitalGain) == 6 && 
                        Functions.AnalogGainSetting(serialPort1, Shared.jsonData.AnalogGain) == 6)
                        label1.Text = "True(1),";
                    else
                        label1.Text = "False(1),";

                    pixelSectionDataValue1 = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount1, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                             Parameters.dataLenght, Parameters.dataCount);
                }
                catch (Exception)
                {
                    label1.Text = "False(1),";
                }
            }
            else
            {
                label1.Text = "Close(1),";
            }

            if (serialPort2.IsOpen)
            {
                try
                {
                    if (Functions.DigitalGainSetting(serialPort2, Shared.jsonData.DigitalGain) == 6 && 
                        Functions.AnalogGainSetting(serialPort2, Shared.jsonData.AnalogGain) == 6)
                        label1.Text += "True(2),";
                    else
                        label1.Text += "False(2),";

                    pixelSectionDataValue2 = Functions.LoopPixelDataProcess(serialPort2, sendData, nmCount2, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                             Parameters.dataLenght, Parameters.dataCount);
                }
                catch (Exception)
                {
                    label1.Text += "False(2),";
                }
            }
            else
            {
                label1.Text += "Close(2),";
            }

            if (serialPort3.IsOpen)
            {
                try
                {
                    if (Functions.DigitalGainSetting(serialPort3, Shared.jsonData.DigitalGain) == 6 && 
                        Functions.AnalogGainSetting(serialPort3, Shared.jsonData.AnalogGain) == 6)
                        label1.Text += "True(3)";
                    else
                        label1.Text += "False(3)";

                    pixelSectionDataValue3 = Functions.LoopPixelDataProcess(serialPort3, sendData, nmCount3, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                             Parameters.dataLenght, Parameters.dataCount);
                }
                catch (Exception)
                {
                    label1.Text += "False(3)";
                }
            }
            else
            {
                label1.Text += "Close(3)";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label3.Text = "Default";
            label3.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + Shared.jsonData.IntegrationTime + " " + Shared.jsonData.AverageScan + " format<CR>\r";
            double[] ERefLed1 = new double[Parameters.dataCount];
            double[] ERefLed2 = new double[Parameters.dataCount];
            double[] ERefLed3 = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    ERefLed1 = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount1, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                               Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < ERsled1.Length; i++)
                    {
                        if (comboBox1.SelectedIndex == 0)
                            ERsled1[i] = ERefLed1[i] / standartPlate1[i];
                        else if (comboBox1.SelectedIndex == 1)
                            ERsled1[i] = ERefLed1[i] / standartPlate2[i];
                    }
                    label3.Text = "True(1), ";
                }

                catch (Exception)
                {
                    label3.Text = "False(1), ";
                }
            }
            else
            {
                label3.Text = "Close(1), ";
            }

            if (serialPort2.IsOpen)
            {
                try
                {
                    ERefLed2 = Functions.LoopPixelDataProcess(serialPort2, sendData, nmCount2, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                               Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < ERsled2.Length; i++)
                    {
                        if (comboBox1.SelectedIndex == 0)
                            ERsled2[i] = ERefLed2[i] / standartPlate1[i];
                        else if (comboBox1.SelectedIndex == 1)
                            ERsled2[i] = ERefLed2[i] / standartPlate2[i];
                    }
                    label3.Text += "True(2),";
                }
                catch (Exception)
                {
                    label3.Text += "False(2),";
                }
            }
            else
            {
                label3.Text += "Close(2),";
            }

            if (serialPort3.IsOpen)
            {
                try
                {
                    ERefLed3 = Functions.LoopPixelDataProcess(serialPort3, sendData, nmCount3, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                               Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < ERsled3.Length; i++)
                    {
                        if (comboBox1.SelectedIndex == 0)
                            ERsled3[i] = ERefLed3[i] / standartPlate1[i];
                        else if (comboBox1.SelectedIndex == 1)
                            ERsled3[i] = ERefLed3[i] / standartPlate2[i];
                    }
                    label3.Text += "True(3)";
                }
                catch (Exception)
                {
                    label3.Text += "False(3)";
                }
            }
            else
            {
                label3.Text += "Close(3)";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;
            this.Refresh();
            Thread.Sleep(200);

            string sendData = "*MEASure:REFERence " + Shared.jsonData.IntegrationTime + " " + Shared.jsonData.AverageScan + " format<CR>\r";
            double[] RStandartSample1 = new double[Parameters.dataCount];
            double[] EStandartSample1 = new double[Parameters.dataCount];
            double[] RStandartSample2 = new double[Parameters.dataCount];
            double[] EStandartSample2 = new double[Parameters.dataCount];
            double[] RStandartSample3 = new double[Parameters.dataCount];
            double[] EStandartSample3 = new double[Parameters.dataCount];

            if (serialPort1.IsOpen)
            {
                try
                {
                    EStandartSample1 = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCount1, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                       Parameters.dataLenght, Parameters.dataCount);
                    for (int i = 0; i < RStandartSample1.Length; i++)
                        RStandartSample1[i] = EStandartSample1[i] / ERsled1[i];

                    double XStandartSample1 = Functions.XYZCalculation(RStandartSample1, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YStandartSample1 = Functions.XYZCalculation(RStandartSample1, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZStandartSample1 = Functions.XYZCalculation(RStandartSample1, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    Parameters.lStandartSample1 = (116 * Math.Pow(YStandartSample1, 1.0 / 3.0)) - 16;
                    Parameters.aStandartSample1 = 500 * (Math.Pow(XStandartSample1, 1.0 / 3.0) - Math.Pow(YStandartSample1, 1.0 / 3.0));
                    Parameters.bStandartSample1 = 200 * (Math.Pow(YStandartSample1, 1.0 / 3.0) - Math.Pow(ZStandartSample1, 1.0 / 3.0));

                    double cSample1 = Math.Pow(Parameters.aStandartSample1, 2) + Math.Pow(Parameters.bStandartSample1, 2);
                    cSample1 = Math.Sqrt(cSample1);

                    double bDeviceA1 = 0;
                    if (Parameters.aStandartSample1 > 0 && Parameters.bStandartSample1 > 0)
                    {
                        bDeviceA1 = Math.Abs(Parameters.bStandartSample1) / Math.Abs(Parameters.aStandartSample1);
                        Parameters.hSample1 = Math.Atan(bDeviceA1) * (180 / Math.PI);
                    }
                    else if (Parameters.aStandartSample1 < 0 && Parameters.bStandartSample1 > 0)
                    {
                        bDeviceA1 = Math.Abs(Parameters.bStandartSample1) / Math.Abs(Parameters.aStandartSample1);
                        Parameters.hSample1 = Math.Atan(bDeviceA1) * (180 / Math.PI);
                        Parameters.hSample1 = Parameters.hSample1 + 90;
                    }
                    else if (Parameters.aStandartSample1 < 0 && Parameters.bStandartSample1 < 0)
                    {
                        bDeviceA1 = Math.Abs(Parameters.bStandartSample1) / Math.Abs(Parameters.aStandartSample1);
                        Parameters.hSample1 = Math.Atan(bDeviceA1) * (180 / Math.PI);
                        Parameters.hSample1 = Parameters.hSample1 + 180;
                    }
                    else if (Parameters.aStandartSample1 > 0 && Parameters.bStandartSample1 < 0)
                    {
                        bDeviceA1 = Math.Abs(Parameters.bStandartSample1) / Math.Abs(Parameters.aStandartSample1);
                        Parameters.hSample1 = Math.Atan(bDeviceA1) * (180 / Math.PI);
                        Parameters.hSample1 = Parameters.hSample1 + 270;
                    }
                    else
                    {
                        Parameters.hSample1 = 0;
                    }

                    if (Parameters.lStandartSample1 < 16)
                        Parameters.SLSample1 = 0.511;
                    else
                        Parameters.SLSample1 = (0.04097 * Parameters.lStandartSample1) / (1 + (0.0165 * Parameters.lStandartSample1));

                    Parameters.SCSample1 = ((0.0638 * Parameters.cSample1) / (1 + (0.0131 * Parameters.cSample1))) + 0.638;
                    double T1 = 0;
                    double F1 = 0;
                    if (Parameters.hSample1 > 164 && Parameters.hSample1 < 345)
                        T1 = 0.56 + Math.Abs((0.2 * (Math.Cos(Parameters.hSample1 + 168))));
                    else
                        T1 = 0.38 + Math.Abs((0.4 * (Math.Cos(Parameters.hSample1 + 35))));
                    F1 = Math.Pow(Parameters.cSample1, 4) / (Math.Pow(Parameters.cSample1, 4) + 1900);
                    F1 = Math.Sqrt(F1);
                    Parameters.SHSample1 = Parameters.SCSample1 * ((T1 * F1) + 1 - F1);

                    textBox1.Text = Math.Round(Parameters.lStandartSample1, 4).ToString();
                    textBox2.Text = Math.Round(Parameters.aStandartSample1, 4).ToString();
                    textBox3.Text = Math.Round(Parameters.bStandartSample1, 4).ToString();
                    label4.Text = "True(1),";
                }
                catch (Exception)
                {
                    label4.Text = "False(1),";
                }
            }
            else
            {
                label4.Text = "Close(1),";
            }

            if (serialPort2.IsOpen)
            {
                try
                {
                    EStandartSample2 = Functions.LoopPixelDataProcess(serialPort2, sendData, nmCount2, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                       Parameters.dataLenght, Parameters.dataCount);
                    for (int j = 0; j < RStandartSample2.Length; j++)
                        RStandartSample2[j] = EStandartSample2[j] / ERsled2[j];

                    double XStandartSample2 = Functions.XYZCalculation(RStandartSample2, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YStandartSample2 = Functions.XYZCalculation(RStandartSample2, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZStandartSample2 = Functions.XYZCalculation(RStandartSample2, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    Parameters.lStandartSample2 = (116 * Math.Pow(YStandartSample2, 1.0 / 3.0)) - 16;
                    Parameters.aStandartSample2 = 500 * (Math.Pow(XStandartSample2, 1.0 / 3.0) - Math.Pow(YStandartSample2, 1.0 / 3.0));
                    Parameters.bStandartSample2 = 200 * (Math.Pow(YStandartSample2, 1.0 / 3.0) - Math.Pow(ZStandartSample2, 1.0 / 3.0));

                    Parameters.cSample2 = Math.Pow(Parameters.aStandartSample2, 2) + Math.Pow(Parameters.bStandartSample2, 2);
                    Parameters.cSample2 = Math.Sqrt(Parameters.cSample2);

                    double bDeviceA2 = 0;
                    if (Parameters.aStandartSample2 > 0 && Parameters.bStandartSample2 > 0)
                    {
                        bDeviceA2 = Math.Abs(Parameters.bStandartSample2) / Math.Abs(Parameters.aStandartSample2);
                        Parameters.hSample2 = Math.Atan(bDeviceA2) * (180 / Math.PI);
                    }
                    else if (Parameters.aStandartSample2 < 0 && Parameters.bStandartSample2 > 0)
                    {
                        bDeviceA2 = Math.Abs(Parameters.bStandartSample2) / Math.Abs(Parameters.aStandartSample2);
                        Parameters.hSample2 = Math.Atan(bDeviceA2) * (180 / Math.PI);
                        Parameters.hSample2 = Parameters.hSample2 + 90;
                    }
                    else if (Parameters.aStandartSample2 < 0 && Parameters.bStandartSample2 < 0)
                    {
                        bDeviceA2 = Math.Abs(Parameters.bStandartSample2) / Math.Abs(Parameters.aStandartSample2);
                        Parameters.hSample2 = Math.Atan(bDeviceA2) * (180 / Math.PI);
                        Parameters.hSample2 = Parameters.hSample2 + 180;
                    }
                    else if (Parameters.aStandartSample2 > 0 && Parameters.bStandartSample2 < 0)
                    {
                        bDeviceA2 = Math.Abs(Parameters.bStandartSample2) / Math.Abs(Parameters.aStandartSample2);
                        Parameters.hSample2 = Math.Atan(bDeviceA2) * (180 / Math.PI);
                        Parameters.hSample2 = Parameters.hSample2 + 270;
                    }
                    else
                    {
                        Parameters.hSample2 = 0;
                    }

                    if (Parameters.lStandartSample2 < 16)
                        Parameters.SLSample2 = 0.511;
                    else
                        Parameters.SLSample2 = (0.04097 * Parameters.lStandartSample2) / (1 + (0.0165 * Parameters.lStandartSample2));

                    Parameters.SCSample2 = ((0.0638 * Parameters.cSample2) / (1 + (0.0131 * Parameters.cSample2))) + 0.638;
                    double T2 = 0;
                    double F2 = 0;
                    if (Parameters.hSample2 > 164 && Parameters.hSample2 < 345)
                        T2 = 0.56 + Math.Abs((0.2 * (Math.Cos(Parameters.hSample2 + 168))));
                    else
                        T2 = 0.38 + Math.Abs((0.4 * (Math.Cos(Parameters.hSample2 + 35))));
                    F2 = Math.Pow(Parameters.cSample2, 4) / (Math.Pow(Parameters.cSample2, 4) + 1900);
                    F2 = Math.Sqrt(F2);
                    Parameters.SHSample2 = Parameters.SCSample2 * ((T2 * F2) + 1 - F2);

                    textBox4.Text = Math.Round(Parameters.lStandartSample2, 4).ToString();
                    textBox5.Text = Math.Round(Parameters.aStandartSample2, 4).ToString();
                    textBox6.Text = Math.Round(Parameters.bStandartSample2, 4).ToString();
                    label4.Text += "True(2),";
                }
                catch (Exception)
                {
                    label4.Text += "False(2),";
                }
            }
            else
            {
                label4.Text += "Close(2),";
            }

            if (serialPort3.IsOpen)
            {
                try
                {
                    EStandartSample3 = Functions.LoopPixelDataProcess(serialPort3, sendData, nmCount3, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.OtherLoopCount), 
                                       Parameters.dataLenght, Parameters.dataCount);
                    for (int k = 0; k < RStandartSample3.Length; k++)
                        RStandartSample3[k] = EStandartSample3[k] / ERsled3[k];

                    double XStandartSample3 = Functions.XYZCalculation(RStandartSample3, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YStandartSample3 = Functions.XYZCalculation(RStandartSample3, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZStandartSample3 = Functions.XYZCalculation(RStandartSample3, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    Parameters.lStandartSample3 = (116 * Math.Pow(YStandartSample3, 1.0 / 3.0)) - 16;
                    Parameters.aStandartSample3 = 500 * (Math.Pow(XStandartSample3, 1.0 / 3.0) - Math.Pow(YStandartSample3, 1.0 / 3.0));
                    Parameters.bStandartSample3 = 200 * (Math.Pow(YStandartSample3, 1.0 / 3.0) - Math.Pow(ZStandartSample3, 1.0 / 3.0));

                    Parameters.cSample3 = Math.Pow(Parameters.aStandartSample3, 2) + Math.Pow(Parameters.bStandartSample3, 2);
                    Parameters.cSample3 = Math.Sqrt(Parameters.cSample3);

                    double bDeviceA3 = 0;
                    if (Parameters.aStandartSample3 > 0 && Parameters.bStandartSample3 > 0)
                    {
                        bDeviceA3 = Math.Abs(Parameters.bStandartSample3) / Math.Abs(Parameters.aStandartSample3);
                        Parameters.hSample3 = Math.Atan(bDeviceA3) * (180 / Math.PI);
                    }
                    else if (Parameters.aStandartSample3 < 0 && Parameters.bStandartSample3 > 0)
                    {
                        bDeviceA3 = Math.Abs(Parameters.bStandartSample3) / Math.Abs(Parameters.aStandartSample3);
                        Parameters.hSample3 = Math.Atan(bDeviceA3) * (180 / Math.PI);
                        Parameters.hSample3 = Parameters.hSample3 + 90;
                    }
                    else if (Parameters.aStandartSample3 < 0 && Parameters.bStandartSample3 < 0)
                    {
                        bDeviceA3 = Math.Abs(Parameters.bStandartSample3) / Math.Abs(Parameters.aStandartSample3);
                        Parameters.hSample3 = Math.Atan(bDeviceA3) * (180 / Math.PI);
                        Parameters.hSample3 = Parameters.hSample3 + 180;
                    }
                    else if (Parameters.aStandartSample3 > 0 && Parameters.bStandartSample3 < 0)
                    {
                        bDeviceA3 = Math.Abs(Parameters.bStandartSample3) / Math.Abs(Parameters.aStandartSample3);
                        Parameters.hSample3 = Math.Atan(bDeviceA3) * (180 / Math.PI);
                        Parameters.hSample3 = Parameters.hSample3 + 270;
                    }
                    else
                    {
                        Parameters.hSample3 = 0;
                    }

                    if (Parameters.lStandartSample3 < 16)
                        Parameters.SLSample3 = 0.511;
                    else
                        Parameters.SLSample3 = (0.04097 * Parameters.lStandartSample3) / (1 + (0.0165 * Parameters.lStandartSample3));

                    Parameters.SCSample3 = ((0.0638 * Parameters.cSample3) / (1 + (0.0131 * Parameters.cSample3))) + 0.638;
                    double T3 = 0;
                    double F3 = 0;
                    if (Parameters.hSample3 > 164 && Parameters.hSample3 < 345)
                        T3 = 0.56 + Math.Abs((0.2 * (Math.Cos(Parameters.hSample3 + 168))));
                    else
                        T3 = 0.38 + Math.Abs((0.4 * (Math.Cos(Parameters.hSample3 + 35))));
                    F3 = Math.Pow(Parameters.cSample3, 4) / (Math.Pow(Parameters.cSample3, 4) + 1900);
                    F3 = Math.Sqrt(F3);
                    Parameters.SHSample3 = Parameters.SCSample3 * ((T3 * F3) + 1 - F3);

                    textBox7.Text = Math.Round(Parameters.lStandartSample3, 4).ToString();
                    textBox8.Text = Math.Round(Parameters.aStandartSample3, 4).ToString();
                    textBox9.Text = Math.Round(Parameters.bStandartSample3, 4).ToString();
                    label4.Text += "True(3)";
                }
                catch (Exception)
                {
                    label4.Text += "False(3)";
                }
            }
            else
            {
                label4.Text += "Close(3)";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox22.Text = "";
            textBox23.Text = "";
            textBox24.Text = "";
            textBox12.Text = "";
            textBox11.Text = "";
            textBox10.Text = "";
            textBox13.Text = "";
            textBox17.Text = "";
            textBox16.Text = "";
            textBox15.Text = "";
            textBox14.Text = "";
            textBox21.Text = "";
            textBox20.Text = "";
            textBox19.Text = "";
            textBox18.Text = "";

            label24.Text = "X";
            label25.Text = "X";
            label43.Text = "";
            label42.Text = "";
            label41.Text = "";
            label49.Text = "";
            label29.Text = "";
            label30.Text = "";
            label48.Text = "";
            label47.Text = "";
            label35.Text = "";
            label36.Text = "";
            label37.Text = "";
            label31.Text = "";

            ClearChartFunction();
            AxisXChartValueFunction();

            deltaEList1.Clear();
            deltaEList2.Clear();
            deltaEList3.Clear();
            deltaEList12.Clear();
            deltaEList13.Clear();
            deltaEList23.Clear();
            deltaEListAverage1.Clear();
            deltaEListAverage2.Clear();
            deltaEListAverage3.Clear();
            deltaEListAverage12.Clear();
            deltaEListAverage13.Clear();
            deltaEListAverage23.Clear();

            threadState1 = true;
            threadState2 = true;
            threadState3 = true;
            threadState4 = true;
            threadState5 = true;

            Parameters.logTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
            Parameters.logName = Parameters.logTime + "-" + "Data.txt";
            Parameters.logMeasFile = Path.Combine(measFileLog, Parameters.logName);

            checkBox1.Checked = false;
            timer1.Enabled = true;
            timer1.Interval = Convert.ToInt32(Shared.jsonData.TimerValue);

            string sendData = "*MEASure:REFERence " + Shared.jsonData.IntegrationTime + " " + Shared.jsonData.AverageScan + " format<CR>\r";
            firstTime = (DateTime.Now.Hour * 60 * 60) + (DateTime.Now.Minute * 60) + (DateTime.Now.Second);

            if (serialPort1.IsOpen)
            {
                try
                {
                    label11.Text = "True(1),";

                    if (thread1 != null && thread1.IsAlive == true)
                        return;
                    thread1 = new Thread(()=>Process1(serialPort1, sendData, nmCount1, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.TestLoopCount), 
                                             Parameters.dataLenght, Parameters.dataCount));
                    thread1.Start();
                }
                catch (Exception)
                {
                    label11.Text = "False(1),";
                }
            }
            else
            {
                label11.Text += "Close(1),";
            }

            if (serialPort2.IsOpen)
            {
                try
                {
                    label11.Text += "True(2),";
                    if (thread2 != null && thread2.IsAlive == true)
                        return;
                    thread2 = new Thread(()=>Process2(serialPort2, sendData, nmCount2, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.TestLoopCount), 
                              Parameters.dataLenght, Parameters.dataCount));
                    thread2.Start();
                }
                catch (Exception)
                {
                    label11.Text += "False(2),";
                }
            }
            else
            {
                label11.Text += "Close(2),";
            }

            if (serialPort3.IsOpen)
            {
                try
                {
                    label11.Text += "True(3)";
                    if (thread3 != null && thread3.IsAlive == true)
                        return;

                    thread3 = new Thread(()=>Process3(serialPort3, sendData, nmCount3, Shared.jsonData.Filter, Convert.ToInt32(Shared.jsonData.TestLoopCount), 
                              Parameters.dataLenght, Parameters.dataCount));
                    thread3.Start();
                }
                catch (Exception)
                {
                    label11.Text += "False(3),";
                }
            }
            else
            {
                label11.Text += "Close(3)";
            }

            if (thread4 != null && thread4.IsAlive == true)
                return;
            thread4 = new Thread(()=>Process4());
            thread4.Start();

            if (thread5 != null && thread5.IsAlive == true)
                return;
            thread5 = new Thread(()=>Process5());
            thread5.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            threadState1 = false;
            threadState2 = false;
            threadState3 = false;
            threadState4 = false;
            threadState5 = false;
            label11.Text = "Stop";
            label11.ForeColor = Color.Red;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SendSerialMessage(true, true);
            SetText(interfaceFileLog, "Reset button was pressed to reset the card values.");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetText(interfaceFileLog, "Main Form has been minimized to the taskbar.");
            this.WindowState = FormWindowState.Minimized;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetText(interfaceFileLog, "Main Form kapatılıyor.");
            threadState1 = false;
            threadState2 = false;
            threadState3 = false;
            threadState4 = false;
            threadState5 = false;
            Functions.CloseSerialPort(serialPort1);
            Functions.CloseSerialPort(serialPort2);
            Functions.CloseSerialPort(serialPort3);
            Application.Exit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReadWhitePlate();
            ReadGrayPlate();
            ReadParameterId();
            timer1.Enabled = false;

            if (Shared.jsonData.SelectDevicePort1 != Shared.jsonData.SelectDevicePort2 &&
                Shared.jsonData.SelectDevicePort1 != Shared.jsonData.SelectDevicePort3 &&
                Shared.jsonData.SelectDevicePort2 != Shared.jsonData.SelectDevicePort3)
            {
                if (Shared.jsonData.SelectDevicePort1 == "A")
                {
                    serialPort1.Close();
                }
                else
                {
                    serialPort1.PortName = Shared.jsonData.SelectDevicePort1;
                    Functions.OpenSerialPort(serialPort1, Shared.jsonData.SelectDevicePort1, 3000000);
                }

                if (Shared.jsonData.SelectDevicePort2 == "B")
                {
                    serialPort2.Close();
                }
                else
                {
                    serialPort2.PortName = Shared.jsonData.SelectDevicePort2;
                    Functions.OpenSerialPort(serialPort2, Shared.jsonData.SelectDevicePort2, 3000000);
                }

                if (Shared.jsonData.SelectDevicePort3 == "C")
                {
                    serialPort3.Close();
                }
                else
                {
                    serialPort3.PortName = Shared.jsonData.SelectDevicePort3;
                    Functions.OpenSerialPort(serialPort3, Shared.jsonData.SelectDevicePort3, 3000000);
                }

                if (!serialPort1.IsOpen && !serialPort2.IsOpen && !serialPort3.IsOpen)
                {
                    GroupBoxEnableFalse();
                }
                else
                {
                    ReadNmIndisRelatedPixel();

                    checkBox1.Checked = false;
                    comboBox1.SelectedIndex = 0;
                    groupBox10.BringToFront();

                    groupBox9.Enabled = true;
                    groupBox1.Enabled = true;
                    groupBox3.Enabled = true;
                    groupBox10.Enabled = true;
                    groupBox11.Enabled = true;
                    groupBox7.Enabled = true;
                    groupBox5.Enabled = true;
                    groupBox2.Enabled = true;
                    groupBox8.Enabled = true;
                    groupBox4.Enabled = true;
                    groupBox6.Enabled = true;
                    SetText(interfaceFileLog, "One or more serial ports have been opened.");
                }
            }
            else
            {
                GroupBoxEnableFalse();
                SetText(interfaceFileLog, "Serial ports could not be opened.");
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Shared.listDataForm.ShowDialog();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Shared.deviceSettingsForm.ShowDialog();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Shared.parameterSettingsForm.ShowDialog();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            DisconnectionFunction();
            Shared.measGraphicsForm.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                checkBox1.Text = "Wing Difference";
                groupBox11.BringToFront();
            }
            else
            {
                checkBox1.Text = "Color Filter";
                groupBox10.BringToFront();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double deltaEValueResult1 = 0;
            double lTestSamp1 = 0;
            double aTestSamp1 = 0;
            double bTestSamp1 = 0;
            double deltaEValueAve1 = 0;
            double deltaEValueResult2 = 0;
            double lTestSamp2 = 0;
            double aTestSamp2 = 0;
            double bTestSamp2 = 0;
            double deltaEValueAve2 = 0;
            double deltaEValueResult3 = 0;
            double lTestSamp3 = 0;
            double aTestSamp3 = 0;
            double bTestSamp3 = 0;
            double deltaEValueAve3 = 0;
            double deltaEResult12 = 0;
            double deltaEResult13 = 0;
            double deltaEResult23 = 0;
            double deltaEValueAve12 = 0;
            double deltaEValueAve13 = 0;
            double deltaEValueAve23 = 0;

            if (serialPort1.IsOpen == true && deltaEList1.Count != 0)
            {
                lTestSamp1 = lTestSampleList1.Average();
                aTestSamp1 = aTestSampleList1.Average();
                bTestSamp1 = bTestSampleList1.Average();
                SetTextBox(textBox10, Math.Round(lTestSamp1, 5).ToString());
                SetTextBox(textBox11, Math.Round(aTestSamp1, 5).ToString());
                SetTextBox(textBox12, Math.Round(bTestSamp1, 5).ToString());

                lTestSampleList1.Clear();
                aTestSampleList1.Clear();
                bTestSampleList1.Clear();
                deltaEValueResult1 = deltaEList1.Average();
                SetTextBox(textBox13, Math.Round(deltaEValueResult1, 5).ToString());
                SetLabel(label29, Math.Round(deltaEValueResult1, 5).ToString());
                SetChart(chart1, 0, Math.Round(deltaEValueResult1, 5));
                SetChart(chart1, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));
                
                deltaEList1.Clear();
                deltaEValueAve1 = deltaEListAverage1.Average();
                SetLabel(label35, Math.Round(deltaEValueAve1, 5).ToString());
                double deltaELimitDoub = Convert.ToDouble(Shared.jsonData.DeltaELimit);
                if (deltaEValueResult1 > deltaELimitDoub)
                    DrawGraphicsChart(chart4, 0);
                else
                    DrawGraphicsChart(chart4, 4);
            }

            if (serialPort2.IsOpen == true && deltaEList2.Count != 0)
            {
                lTestSamp2 = lTestSampleList2.Average();
                aTestSamp2 = aTestSampleList2.Average();
                bTestSamp2 = bTestSampleList2.Average();
                SetTextBox(textBox14, Math.Round(lTestSamp2, 5).ToString());
                SetTextBox(textBox15, Math.Round(aTestSamp2, 5).ToString());
                SetTextBox(textBox16, Math.Round(bTestSamp2, 5).ToString());

                lTestSampleList2.Clear();
                aTestSampleList2.Clear();
                bTestSampleList2.Clear();
                deltaEValueResult2 = deltaEList2.Average();
                SetTextBox(textBox17, Math.Round(deltaEValueResult2, 5).ToString());
                SetLabel(label30, Math.Round(deltaEValueResult2, 5).ToString());
                SetChart(chart2, 0, Math.Round(deltaEValueResult2, 5));
                SetChart(chart2, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));
                
                deltaEList2.Clear();
                deltaEValueAve2 = deltaEListAverage2.Average();
                SetLabel(label36, Math.Round(deltaEValueAve2, 5).ToString());
                double deltaELimitDoub = Convert.ToDouble(Shared.jsonData.DeltaELimit);
                if (deltaEValueResult2 > deltaELimitDoub)
                    DrawGraphicsChart(chart5, 0);
                else
                    DrawGraphicsChart(chart5, 4);
            }

            if (serialPort3.IsOpen == true && deltaEList3.Count != 0)
            {
                lTestSamp3 = lTestSampleList3.Average();
                aTestSamp3 = aTestSampleList3.Average();
                bTestSamp3 = bTestSampleList3.Average();
                SetTextBox(textBox18, Math.Round(lTestSamp3, 5).ToString());
                SetTextBox(textBox19, Math.Round(aTestSamp3, 5).ToString());
                SetTextBox(textBox20, Math.Round(bTestSamp3, 5).ToString());
                
                lTestSampleList3.Clear();
                aTestSampleList3.Clear();
                bTestSampleList3.Clear();
                deltaEValueResult3 = deltaEList3.Average();
                SetTextBox(textBox21, Math.Round(deltaEValueResult3, 5).ToString());
                SetLabel(label31, Math.Round(deltaEValueResult3, 5).ToString());
                SetChart(chart3, 0, Math.Round(deltaEValueResult3, 5));
                SetChart(chart3, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));
                
                deltaEList3.Clear();
                deltaEValueAve3 = deltaEListAverage3.Average();
                SetLabel(label37, Math.Round(deltaEValueAve3, 5).ToString());
                double deltaELimitDoub = Convert.ToDouble(Shared.jsonData.DeltaELimit);
                if (deltaEValueResult3 > deltaELimitDoub)
                    DrawGraphicsChart(chart6, 0);
                else
                    DrawGraphicsChart(chart6, 4);
            }

            if (serialPort1.IsOpen == true && serialPort2.IsOpen == true && serialPort3.IsOpen == true)
            {
                if (deltaEList12.Count != 0 && deltaEList13.Count != 0 && deltaEList23.Count != 0)
                {
                    deltaEResult12 = deltaEList12.Average();
                    deltaEResult13 = deltaEList13.Average();
                    deltaEResult23 = deltaEList23.Average();
                    SetTextBox(textBox22, deltaEResult12.ToString());
                    SetTextBox(textBox23, deltaEResult13.ToString());
                    SetTextBox(textBox24, deltaEResult23.ToString());
                    SetLabel(label41, deltaEResult12.ToString());
                    SetLabel(label42, deltaEResult13.ToString());
                    SetLabel(label43, deltaEResult23.ToString());
                    SetChart(chart7, 0, deltaEResult12);
                    SetChart(chart7, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));
                    SetChart(chart8, 0, deltaEResult13);
                    SetChart(chart8, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));
                    SetChart(chart9, 0, deltaEResult23);
                    SetChart(chart9, 1, Convert.ToDouble(Shared.jsonData.DeltaELimit));

                    double deltaELimitDoub = Convert.ToDouble(Shared.jsonData.DeltaELimit);
                    if (deltaEResult12 > deltaELimitDoub)
                        DrawGraphicsChart(chart10, 0);
                    else
                        DrawGraphicsChart(chart10, 4);

                    if (deltaEResult13 > deltaELimitDoub)
                        DrawGraphicsChart(chart11, 0);
                    else
                        DrawGraphicsChart(chart11, 4);

                    if (deltaEResult23 > deltaELimitDoub)
                        DrawGraphicsChart(chart12, 0);
                    else
                        DrawGraphicsChart(chart12, 4);

                    deltaEList12.Clear();
                    deltaEList13.Clear();
                    deltaEList23.Clear();
                    deltaEValueAve12 = deltaEListAverage12.Average();
                    deltaEValueAve13 = deltaEListAverage13.Average();
                    deltaEValueAve23 = deltaEListAverage23.Average();
                    SetLabel(label47, Math.Round(deltaEValueAve12, 5).ToString());
                    SetLabel(label48, Math.Round(deltaEValueAve13, 5).ToString());
                    SetLabel(label49, Math.Round(deltaEValueAve23, 5).ToString());
                }
            }
            else
            {
                SetTextBox(textBox22, "-");
                SetTextBox(textBox23, "-");
                SetTextBox(textBox24, "-");

                SetLabel(label41, "-");
                SetLabel(label42, "-");
                SetLabel(label43, "-");

                SetLabel(label47, "-");
                SetLabel(label48, "-");
                SetLabel(label49, "-");
            }
        }

        private void serialPort4_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (serialPortLock)
            {
                try
                {
                    SerialPort sp = (SerialPort)sender;
                    int serialRecvCount = sp.Read(buff, Parameters.leftCount, buff.Length - Parameters.leftCount);
                    Parameters.leftCount += serialRecvCount;

                    if (Parameters.leftCount == buff.Length && buff[0] == 123)
                    {
                        Parameters.machineRunning = Convert.ToBoolean((buff[1]) & 0x01);
                        if (Parameters.machineRunning == true)
                            SetLabel(label25, "Run");
                        else
                            SetLabel(label25, "Stop");
                        if (Parameters.machineRunning != Parameters.machineRunningOld)
                            SetText(interfaceFileLog, "Card machine running signal error occurred.");

                        Parameters.externalInput = Convert.ToBoolean((buff[1] >> 2) & 0x01);
                        if (Parameters.externalInput != Parameters.externalInputOld)
                            SetText(interfaceFileLog, "Card machine external input error occurred.");

                        int rotaryFrequence = (int)((((int)buff[3]) << 8) + (buff[2]));
                        Parameters.currentMeter = (((int)buff[5]) << 8) + buff[4] + (((double)buff[6]) * 0.01F);
                        SetLabel(label24, Parameters.currentMeter.ToString("0.00"));

                        Parameters.externalInputOld = Parameters.externalInput;
                        Parameters.machineRunningOld = Parameters.machineRunning;
                        Parameters.currentMeterOld = Parameters.currentMeter;
                        for (int i = 0; i < buff.Length; i++)
                            buff[i] = 0;

                        Parameters.leftCount = 0;
                    }
                    else
                    {
                        if (Parameters.leftCount == buff.Length)
                        {
                            serialPort4.Close();
                            Parameters.leftCount = 0;
                            serialPort4.Open();
                        }
                    }
                }
                catch (Exception)
                {
                    SetText(interfaceFileLog, "Card serial port error occurred.");
                }
            }
        }

        private void Process1(SerialPort serialPort, string sendData, int[] nmCountA, string choosingFilter, int loopCount, int dataLenght, int dataCount)
        {
            while (true)
            {
                if (threadState1 == true)
                {
                    double[] RTestSample = new double[dataCount];
                    double[] ETestSample = new double[dataCount];

                    ETestSample = Functions.LoopPixelDataProcess(serialPort1, sendData, nmCountA, choosingFilter, loopCount, dataLenght, dataCount);
                    for (int i = 0; i < RTestSample.Length; i++)
                        RTestSample[i] = ETestSample[i] / ERsled1[i];

                    double XTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    double lTestSample = (116 * Math.Pow(YTestSample, 1.0 / 3.0)) - 16;
                    double aTestSample = 500 * (Math.Pow(XTestSample, 1.0 / 3.0) - Math.Pow(YTestSample, 1.0 / 3.0));
                    double bTestSample = 200 * (Math.Pow(YTestSample, 1.0 / 3.0) - Math.Pow(ZTestSample, 1.0 / 3.0));
                    lTestSampleList1.Add(lTestSample);
                    aTestSampleList1.Add(aTestSample);
                    bTestSampleList1.Add(bTestSample);

                    double lDifferenceAbs = lTestSample - Parameters.lStandartSample1;
                    lDifferenceAbs = Math.Abs(lDifferenceAbs);
                    double lDifference = Math.Pow(lDifferenceAbs, 2);
                    double aDifferenceAbs = aTestSample - Parameters.aStandartSample1;
                    aDifferenceAbs = Math.Abs(aDifferenceAbs);
                    double aDifference = Math.Pow(aDifferenceAbs, 2);
                    double bDifferenceAbs = bTestSample - Parameters.bStandartSample1;
                    bDifferenceAbs = Math.Abs(bDifferenceAbs);
                    double bDifference = Math.Pow(bDifferenceAbs, 2);
                    Parameters.deltaEResult1 = Math.Sqrt(lDifference + aDifference + bDifference);

                    Parameters.cTest1 = Math.Pow(aTestSample, 2) + Math.Pow(bTestSample, 2);
                    Parameters.cTest1 = Math.Sqrt(Parameters.cTest1);
                    Parameters.deltaEH1 = Math.Pow(Parameters.deltaEResult1, 2) - lDifference - Math.Pow((Parameters.cTest1 - Parameters.cSample1), 2);
                    Parameters.deltaEH1 = Math.Sqrt(Parameters.deltaEH1);

                    Parameters.deltaECMC11Result1 = Math.Pow((lDifference / (1 * Parameters.SLSample1)), 2) + Math.Pow(((Parameters.cTest1 - Parameters.cSample1) / 
                                                    (1 * Parameters.SCSample1)), 2) + Math.Pow((Parameters.deltaEH1 / Parameters.SHSample1), 2);
                    Parameters.deltaECMC11Result1 = Math.Sqrt(Parameters.deltaECMC11Result1);
                    Parameters.deltaECMC21Result1 = Math.Pow((lDifference / (2 * Parameters.SLSample1)), 2) + Math.Pow(((Parameters.cTest1 - Parameters.cSample1) / 
                                                    (1 * Parameters.SCSample1)), 2) + Math.Pow((Parameters.deltaEH1 / Parameters.SHSample1), 2);
                    Parameters.deltaECMC21Result1 = Math.Sqrt(Parameters.deltaECMC21Result1);

                    if (Shared.jsonData.CmcType == "Normal")
                    {
                        deltaEList1.Add(Parameters.deltaEResult1);
                        deltaEListAverage1.Add(Parameters.deltaEResult1);
                        deltaEDatabaseList1.Add(Parameters.deltaEResult1);
                    }
                    else if (Shared.jsonData.CmcType == "CMC1:1")
                    {
                        deltaEList1.Add(Parameters.deltaECMC11Result1);
                        deltaEListAverage1.Add(Parameters.deltaECMC11Result1);
                        deltaEDatabaseList1.Add(Parameters.deltaECMC11Result1);
                    }
                    else if (Shared.jsonData.CmcType == "CMC2:1")
                    {
                        deltaEList1.Add(Parameters.deltaECMC21Result1);
                        deltaEListAverage1.Add(Parameters.deltaECMC21Result1);
                        deltaEDatabaseList1.Add(Parameters.deltaECMC11Result1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Process2(SerialPort serialPort, string sendData, int[] nmCountB, string choosingFilter, int loopCount, int dataLenght, int dataCount)
        {
            while (true)
            {
                if (threadState2 == true)
                {
                    double[] RTestSample = new double[dataCount];
                    double[] ETestSample = new double[dataCount];

                    ETestSample = Functions.LoopPixelDataProcess(serialPort, sendData, nmCountB, choosingFilter, loopCount, dataLenght, dataCount);
                    for (int i = 0; i < RTestSample.Length; i++)
                        RTestSample[i] = ETestSample[i] / ERsled2[i];

                    double XTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    double lTestSample = (116 * Math.Pow(YTestSample, 1.0 / 3.0)) - 16;
                    double aTestSample = 500 * (Math.Pow(XTestSample, 1.0 / 3.0) - Math.Pow(YTestSample, 1.0 / 3.0));
                    double bTestSample = 200 * (Math.Pow(YTestSample, 1.0 / 3.0) - Math.Pow(ZTestSample, 1.0 / 3.0));
                    lTestSampleList2.Add(lTestSample);
                    aTestSampleList2.Add(aTestSample);
                    bTestSampleList2.Add(bTestSample);

                    double lDifferenceAbs = lTestSample - Parameters.lStandartSample2;
                    lDifferenceAbs = Math.Abs(lDifferenceAbs);
                    double lDifference = Math.Pow(lDifferenceAbs, 2);
                    double aDifferenceAbs = aTestSample - Parameters.aStandartSample2;
                    aDifferenceAbs = Math.Abs(aDifferenceAbs);
                    double aDifference = Math.Pow(aDifferenceAbs, 2);
                    double bDifferenceAbs = bTestSample - Parameters.bStandartSample2;
                    bDifferenceAbs = Math.Abs(bDifferenceAbs);
                    double bDifference = Math.Pow(bDifferenceAbs, 2);
                    Parameters.deltaEResult2 = Math.Sqrt(lDifference + aDifference + bDifference);

                    Parameters.cTest2 = Math.Pow(aTestSample, 2) + Math.Pow(bTestSample, 2);
                    Parameters.cTest2 = Math.Sqrt(Parameters.cTest2);
                    Parameters.deltaEH2 = Math.Pow(Parameters.deltaEResult2, 2) - lDifference - Math.Pow((Parameters.cTest2 - Parameters.cSample2), 2);
                    Parameters.deltaEH2 = Math.Sqrt(Parameters.deltaEH2);

                    Parameters.deltaECMC11Result2 = Math.Pow((lDifference / (1 * Parameters.SLSample2)), 2) + Math.Pow(((Parameters.cTest2 - Parameters.cSample2) / 
                                                    (1 * Parameters.SCSample2)), 2) + Math.Pow((Parameters.deltaEH2 / Parameters.SHSample2), 2);
                    Parameters.deltaECMC11Result2 = Math.Sqrt(Parameters.deltaECMC11Result2);
                    Parameters.deltaECMC21Result2 = Math.Pow((lDifference / (2 * Parameters.SLSample2)), 2) + Math.Pow(((Parameters.cTest2 - Parameters.cSample2) / 
                                                    (1 * Parameters.SCSample2)), 2) + Math.Pow((Parameters.deltaEH2 / Parameters.SHSample2), 2);
                    Parameters.deltaECMC21Result2 = Math.Sqrt(Parameters.deltaECMC21Result2);

                    if (Shared.jsonData.CmcType == "Normal")
                    {
                        deltaEList2.Add(Parameters.deltaEResult2);
                        deltaEListAverage2.Add(Parameters.deltaEResult2);
                        deltaEDatabaseList2.Add(Parameters.deltaEResult2);
                    }
                    else if (Shared.jsonData.CmcType == "CMC1:1")
                    {
                        deltaEList2.Add(Parameters.deltaECMC11Result2);
                        deltaEListAverage2.Add(Parameters.deltaECMC11Result2);
                        deltaEDatabaseList2.Add(Parameters.deltaECMC11Result2);
                    }
                    else if (Shared.jsonData.CmcType == "CMC2:1")
                    {
                        deltaEList2.Add(Parameters.deltaECMC21Result2);
                        deltaEListAverage2.Add(Parameters.deltaECMC21Result2);
                        deltaEDatabaseList2.Add(Parameters.deltaECMC21Result2);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Process3(SerialPort serialPort, string sendData, int[] nmCountC, string choosingFilter, int loopCount, int dataLenght, int dataCount)
        {
            while (true)
            {
                if (threadState3 == true)
                {
                    double[] RTestSample = new double[dataCount];
                    double[] ETestSample = new double[dataCount];

                    ETestSample = Functions.LoopPixelDataProcess(serialPort, sendData, nmCountC, choosingFilter, loopCount, dataLenght, dataCount);
                    for (int i = 0; i < RTestSample.Length; i++)
                        RTestSample[i] = ETestSample[i] / ERsled3[i];

                    double XTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.x10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10x;
                    double YTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.y10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10y;
                    double ZTestSample = Functions.XYZCalculation(RTestSample, StandartValues.d65, StandartValues.z10, StandartValues.y10, StandartValues.deltaLamda) / StandartValues.W10z;
                    double lTestSample = (116 * Math.Pow(YTestSample, 1.0 / 3.0)) - 16;
                    double aTestSample = 500 * (Math.Pow(XTestSample, 1.0 / 3.0) - Math.Pow(YTestSample, 1.0 / 3.0));
                    double bTestSample = 200 * (Math.Pow(YTestSample, 1.0 / 3.0) - Math.Pow(ZTestSample, 1.0 / 3.0));
                    lTestSampleList3.Add(lTestSample);
                    aTestSampleList3.Add(aTestSample);
                    bTestSampleList3.Add(bTestSample);

                    double lDifferenceAbs = lTestSample - Parameters.lStandartSample3;
                    lDifferenceAbs = Math.Abs(lDifferenceAbs);
                    double lDifference = Math.Pow(lDifferenceAbs, 2);
                    double aDifferenceAbs = aTestSample - Parameters.aStandartSample3;
                    aDifferenceAbs = Math.Abs(aDifferenceAbs);
                    double aDifference = Math.Pow(aDifferenceAbs, 2);
                    double bDifferenceAbs = bTestSample - Parameters.bStandartSample3;
                    bDifferenceAbs = Math.Abs(bDifferenceAbs);
                    double bDifference = Math.Pow(bDifferenceAbs, 2);
                    Parameters.deltaEResult3 = Math.Sqrt(lDifference + aDifference + bDifference);

                    Parameters.cTest3 = Math.Pow(aTestSample, 2) + Math.Pow(bTestSample, 2);
                    Parameters.cTest3 = Math.Sqrt(Parameters.cTest3);
                    Parameters.deltaEH3 = Math.Pow(Parameters.deltaEResult3, 2) - lDifference - Math.Pow((Parameters.cTest3 - Parameters.cSample3), 2);
                    Parameters.deltaEH3 = Math.Sqrt(Parameters.deltaEH3);

                    Parameters.deltaECMC11Result3 = Math.Pow((lDifference / (1 * Parameters.SLSample3)), 2) + Math.Pow(((Parameters.cTest3 - Parameters.cSample3) / 
                                                    (1 * Parameters.SCSample3)), 2) + Math.Pow((Parameters.deltaEH3 / Parameters.SHSample3), 2);
                    Parameters.deltaECMC11Result3 = Math.Sqrt(Parameters.deltaECMC11Result3);
                    Parameters.deltaECMC21Result3 = Math.Pow((lDifference / (2 * Parameters.SLSample3)), 2) + Math.Pow(((Parameters.cTest3 - Parameters.cSample3) / 
                                                    (1 * Parameters.SCSample3)), 2) + Math.Pow((Parameters.deltaEH3 / Parameters.SHSample3), 2);
                    Parameters.deltaECMC21Result3 = Math.Sqrt(Parameters.deltaECMC21Result3);

                    if (Shared.jsonData.CmcType == "Normal")
                    {
                        deltaEList3.Add(Parameters.deltaEResult3);
                        deltaEListAverage3.Add(Parameters.deltaEResult3);
                        deltaEDatabaseList3.Add(Parameters.deltaEResult3);
                    }
                    else if (Shared.jsonData.CmcType == "CMC1:1")
                    {
                        deltaEList3.Add(Parameters.deltaECMC11Result3);
                        deltaEListAverage3.Add(Parameters.deltaECMC11Result3);
                        deltaEDatabaseList3.Add(Parameters.deltaECMC11Result3);
                    }
                    else if (Shared.jsonData.CmcType == "CMC2:1")
                    {
                        deltaEList3.Add(Parameters.deltaECMC21Result3);
                        deltaEListAverage3.Add(Parameters.deltaECMC21Result3);
                        deltaEDatabaseList3.Add(Parameters.deltaECMC21Result3);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Process4()
        {
            while (true)
            {
                if (threadState4 == true)
                {
                    if (serialPort1.IsOpen == true && serialPort2.IsOpen == true && serialPort3.IsOpen == true)
                    {
                        if (deltaEList12.Count == deltaEValue && deltaEList13.Count == deltaEValue && deltaEList23.Count == deltaEValue)
                        {
                            Parameters.deltaE12 = deltaEList1[deltaEValue - 1] - deltaEList2[deltaEValue - 1];
                            deltaEList12.Add(Parameters.deltaE12);
                            deltaEListAverage12.Add(Parameters.deltaE12);
                            deltaEDatabaseList12.Add(Parameters.deltaE12);

                            Parameters.deltaE13 = deltaEList1[deltaEValue - 1] - deltaEList3[deltaEValue - 1];
                            deltaEList13.Add(Parameters.deltaE13);
                            deltaEListAverage13.Add(Parameters.deltaE13);
                            deltaEDatabaseList13.Add(Parameters.deltaE13);

                            Parameters.deltaE23 = deltaEList2[deltaEValue - 1] - deltaEList3[deltaEValue - 1];
                            deltaEList23.Add(Parameters.deltaE23);
                            deltaEListAverage23.Add(Parameters.deltaE23);
                            deltaEDatabaseList23.Add(Parameters.deltaE23);

                            deltaEValue++;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Process5()
        {
            while (true)
            {
                if (threadState5 == true)
                {
                    double deltaEDatabaseValue1 = 0;
                    double deltaEDatabaseValue2 = 0;
                    double deltaEDatabaseValue3 = 0;
                    double deltaEDatabaseValue12 = 0;
                    double deltaEDatabaseValue13 = 0;
                    double deltaEDatabaseValue23 = 0;

                    if (deltaEDatabaseList1.Count != 0)
                        deltaEDatabaseValue1 = deltaEDatabaseList1.Average();

                    if (deltaEDatabaseList2.Count != 0)
                        deltaEDatabaseValue2 = deltaEDatabaseList2.Average();

                    if (deltaEDatabaseList3.Count != 0)
                        deltaEDatabaseValue3 = deltaEDatabaseList3.Average();

                    if (deltaEDatabaseList12.Count != 0)
                        deltaEDatabaseValue12 = deltaEDatabaseList12.Average();

                    if (deltaEDatabaseList13.Count != 0)
                        deltaEDatabaseValue13 = deltaEDatabaseList13.Average();

                    if (deltaEDatabaseList23.Count != 0)
                        deltaEDatabaseValue23 = deltaEDatabaseList23.Average();

                    if (deltaEDatabaseList1.Count > 0 || deltaEDatabaseList2.Count > 0 || deltaEDatabaseList3.Count > 0 ||
                        deltaEDatabaseList12.Count > 0 || deltaEDatabaseList13.Count > 0 || deltaEDatabaseList23.Count > 0)
                    {
                        DateTime time = DateTime.Now;
                        string timeString = time.ToString();

                        string connectionString = "DataSource=" + Parameters.nameDatabase + "; Version = 3;";
                        string requestCode = "insert into " + Parameters.nameTable + "(id, date, deltaE1, deltaE2, deltaE3, kf12, kf13, kf23, meterInf) values (" + Parameters.id + ",'" +
                                             timeString + "','" + Math.Round(deltaEDatabaseValue1, 5).ToString() + "','" + Math.Round(deltaEDatabaseValue2, 5).ToString() + "','" +
                                             Math.Round(deltaEDatabaseValue3, 5).ToString() + "','" + Math.Round(deltaEDatabaseValue12, 5).ToString() + "','" +
                                             Math.Round(deltaEDatabaseValue13, 5).ToString() + "','" + Math.Round(deltaEDatabaseValue23, 5).ToString() + "','" +
                                             Parameters.currentMeter + "')";

                        Parameters.command = new SQLiteCommand();
                        Parameters.connection = new SQLiteConnection(connectionString);
                        Parameters.connection.Open();
                        if (Parameters.connection.State == ConnectionState.Open)
                        {
                            Parameters.command.Connection = Parameters.connection;
                            Parameters.command.CommandText = requestCode;
                            Parameters.command.ExecuteNonQuery();
                            Parameters.connection.Close();
                            Parameters.id++;
                        }
                        else
                        {
                            SetText(interfaceFileLog, "Database data saving error occurred.");
                        }

                        deltaEDatabaseList1.Clear();
                        deltaEDatabaseList2.Clear();
                        deltaEDatabaseList3.Clear();
                        deltaEDatabaseList12.Clear();
                        deltaEDatabaseList13.Clear();
                        deltaEDatabaseList23.Clear();

                        SetText(Parameters.logMeasFile, "Delta E1 = " + Math.Round(deltaEDatabaseValue1, 5).ToString() +
                                " Delta E2 = " + Math.Round(deltaEDatabaseValue2, 5).ToString() +
                                " Delta E3 = " + Math.Round(deltaEDatabaseValue3, 5).ToString() +
                                " WD12 = " + Math.Round(deltaEDatabaseValue12, 5).ToString() +
                                " WD13 = " + Math.Round(deltaEDatabaseValue13, 5).ToString() +
                                " WD23 = " + Math.Round(deltaEDatabaseValue23, 5).ToString() +
                                " Lenght = " + Parameters.currentMeter);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ReadParameterId()
        {
            string connectionString = "DataSource=" + Parameters.nameDatabase + "; Version = 3;";
            string requestCode = "select * from " + Parameters.nameTable;

            Parameters.command = new SQLiteCommand();
            Parameters.connection = new SQLiteConnection(connectionString);
            Parameters.connection.Open();
            if (Parameters.connection.State == ConnectionState.Open)
            {
                Parameters.command.Connection = Parameters.connection;
                Parameters.command.CommandText = requestCode;
                Parameters.command.ExecuteNonQuery();
                SQLiteDataReader reader = Parameters.command.ExecuteReader();
                while (reader.Read())
                    Parameters.id = Convert.ToInt32(reader["id"].ToString());
                Parameters.connection.Close();
                Parameters.id++;
                SetText(interfaceFileLog, "Database, ID value read.");
            }
            else
            {
                SetText(interfaceFileLog, "Database, ID value error occurred.");
            }
        }

        public bool CheckSerialPort()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (serialPort4.IsOpen == false)
                        serialPort4.Open();
                });

                return serialPort4.IsOpen;
            }
            catch (Exception ee)
            {
                SetText(interfaceFileLog, "Card serial port opening error occurred.");
                ee.Data.Clear();

                return false;
            }
        }

        private void SendSerialMessage(bool resetMetraj, bool triggerOnOff)
        {
            if (CheckSerialPort() == true)
            {
                byte[] serialBuffer = new byte[7];

                int serialPwmFreq = 0;
                byte byte0 = 0;
                byte byte1 = 0;
                byte byte2 = 0;
                byte byte3 = 0;
                byte byte4 = 0;
                byte byte5 = 0;
                byte byte6 = 0;
                byte byte7 = Convert.ToByte(resetMetraj);
                byte controlBits = (byte)((byte7 << 7) + (byte6 << 6) + (byte5 << 5) + (byte4 << 4) +
                                   (byte3 << 3) + (byte2 << 2) + (byte1 << 1) + byte0);

                serialBuffer[0] = 123;
                serialBuffer[1] = controlBits;
                serialBuffer[2] = (byte)(serialPwmFreq & 0xFF);
                serialBuffer[3] = (byte)((serialPwmFreq >> 8) & 0xFF);
                serialBuffer[4] = 0;
                int minFreq = 0;
                serialBuffer[5] = (byte)(minFreq & 0xFF);
                serialBuffer[6] = (byte)((minFreq >> 8) & 0xFF);

                serialPort4.Write(serialBuffer, 0, serialBuffer.Length);
            }
        }

        private void ReadWhitePlate()
        {
            string filenameRead = "WhitePlate.txt";
            int textCount = 61;
            string[] readData = new string[textCount];
            Functions.ReadFile(filenameRead, textCount, readData, 0, 60);
            for (int i = 0; i < textCount; i++)
                standartPlate1[i] = Convert.ToDouble(readData[i]);
            SetText(interfaceFileLog, "White plate values have been read.");
        }

        private void ReadGrayPlate()
        {
            string filenameRead = "GrayPlate.txt";
            int textCount = 61;
            string[] readData = new string[textCount];
            Functions.ReadFile(filenameRead, textCount, readData, 0, 60);
            for (int i = 0; i < textCount; i++)
                standartPlate2[i] = Convert.ToDouble(readData[i]);
            SetText(interfaceFileLog, "Gray plate values have been read.");
        }

        private void ReadSettings()
        {
            if (Directory.Exists(Parameters.jsonFilePath))
            {
                SetText(interfaceFileLog, "Json file is available.");
            }
            else
            {
                Directory.CreateDirectory(Parameters.jsonFilePath);
                string writeJsonString = Newtonsoft.Json.JsonConvert.SerializeObject(Shared.jsonData);
                File.WriteAllText(Parameters.jsonFilePath + Parameters.jsonName, writeJsonString);
            }

            string readJsonString = File.ReadAllText(Parameters.jsonFilePath + Parameters.jsonName);
            Shared.jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(readJsonString);
        }

        private void ReadNmIndisRelatedPixel()
        {
            if (Shared.jsonData.SelectDevicePort1 != "A")
            {
                nmCount1 = Functions.NmRelatedPixelCalculation(serialPort1, Parameters.dataLenght - 1, Parameters.dataCount, Parameters.firstNm);
                SetText(interfaceFileLog, "Serial Port - 1 nm index values have been read.");
            }

            if (Shared.jsonData.SelectDevicePort2 != "B")
            {
                nmCount2 = Functions.NmRelatedPixelCalculation(serialPort2, Parameters.dataLenght - 1, Parameters.dataCount, Parameters.firstNm);
                SetText(interfaceFileLog, "Serial Port - 2 nm index values have been read.");
            }

            if (Shared.jsonData.SelectDevicePort3 != "C")
            {
                nmCount3 = Functions.NmRelatedPixelCalculation(serialPort3, Parameters.dataLenght - 1, Parameters.dataCount, Parameters.firstNm);
                SetText(interfaceFileLog, "Serial Port - 3 nm index values have been read.");
            }
        }

        private void CreateStorage()
        {
            string connectionString = "DataSource=" + Parameters.nameDatabase + "; Version = 3;";
            string createDateTable = @"create table if not exists " + Parameters.nameTable + "(id INTEGER PRIMARY KEY, " +
                "date varchar(50) NOT NULL, deltaE1 varchar(50) NOT NULL, deltaE2 varchar(50) NOT NULL, deltaE3 varchar(50) NOT NULL, " +
                "kf12 varchar(50) NOT NULL, kf13 varchar(50) NOT NULL, kf23 varchar(50) NOT NULL, meterInf varchar(50) NOT NULL);";

            if (File.Exists(Parameters.nameDatabase + ".sqlite"))
            {
                SQLiteConnection.CreateFile(Parameters.nameDatabase);
                SetText(interfaceFileLog, "Database has been created.");
            }
            else
            {
                Parameters.connection = new SQLiteConnection(connectionString);
                Parameters.connection.Open();
                if (Parameters.connection.State == ConnectionState.Open)
                {
                    SetText(interfaceFileLog, "Database is being checked.");
                    Parameters.command = new SQLiteCommand(createDateTable, Parameters.connection);
                    Parameters.command.ExecuteNonQuery();
                    Parameters.connection.Close();
                }
                else
                {
                    SetText(interfaceFileLog, "Database could not be created.");
                }
            }
        }

        private void GroupBoxEnableFalse()
        {
            groupBox9.Enabled = false;
            groupBox1.Enabled = false;
            groupBox3.Enabled = false;
            groupBox11.Enabled = false;
            groupBox7.Enabled = false;
            groupBox5.Enabled = false;
            groupBox2.Enabled = false;
            groupBox10.Enabled = false;
            groupBox8.Enabled = false;
            groupBox4.Enabled = false;
            groupBox6.Enabled = false;
        }

        static List<int> ProcessID(string appName)
        {
            List<int> processList = new List<int>();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.StartsWith(appName))
                    processList.Add(process.Id);
            }

            return processList;
        }

        public void SetText(string filePath, string text)
        {
            string dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            string message = dateTime + " - " + text;
            
            if (!File.Exists(filePath))
                File.Create(filePath);
            File.AppendAllText(filePath, message + Environment.NewLine);
        }

        private void DisconnectionFunction()
        {
            threadState1 = false;
            threadState2 = false;
            threadState3 = false;
            threadState4 = false;
            threadState5 = false;

            Functions.CloseSerialPort(serialPort1);
            Functions.CloseSerialPort(serialPort2);
            Functions.CloseSerialPort(serialPort3);

            GroupBoxEnableFalse();

            textBox22.Text = "";
            textBox23.Text = "";
            textBox1.Text = "";
            textBox24.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox12.Text = "";
            textBox11.Text = "";
            textBox10.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox13.Text = "";
            textBox17.Text = "";
            textBox16.Text = "";
            textBox15.Text = "";
            textBox14.Text = "";
            textBox21.Text = "";
            textBox20.Text = "";
            textBox19.Text = "";
            textBox18.Text = "";

            label24.Text = "X";
            label25.Text = "X";
            label1.Text = "Default";
            label1.ForeColor = Color.Brown;
            label4.Text = "Default";
            label4.ForeColor = Color.Brown;
            label3.Text = "Default";
            label3.ForeColor = Color.Brown;
            label11.Text = "Default";
            label11.ForeColor = Color.Brown;
            label43.Text = "";
            label42.Text = "";
            label41.Text = "";
            label49.Text = "";
            label29.Text = "X";
            label30.Text = "X";
            label48.Text = "";
            label47.Text = "";
            label35.Text = "X";
            label36.Text = "X";
            label37.Text = "X";
            label31.Text = "X";

            comboBox1.SelectedIndex = 0;

            ClearChartFunction();
            chart3.Series["Series1"].Points.Clear();
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();
            chart9.Series["Series1"].Points.Clear();
            chart8.Series["Series1"].Points.Clear();
            chart7.Series["Series1"].Points.Clear();
        }

        private void DrawGraphicsChart(Chart chart, int selectionSeries)
        {
            List<int> graphicsSeriesCount = new List<int>();
            graphicsSeriesCount.Add(0);
            graphicsSeriesCount.Add(4);

            InsertXYChart(chart, selectionSeries, 10);
            graphicsSeriesCount.Remove(selectionSeries);
            for (int i = 0; i < graphicsSeriesCount.Count; i++)
                InsertXYChart(chart, graphicsSeriesCount[i], 0);
        }

        public void ClearChartFunction()
        {
            ClearChart(chart3, 1, 1, 0, 0, 0);
            ClearChart(chart1, 1, 1, 0, 0, 0);
            ClearChart(chart2, 1, 1, 0, 0, 0);
            ClearChart(chart9, 1, 1, 0, 0, 0);
            ClearChart(chart8, 1, 1, 0, 0, 0);
            ClearChart(chart7, 1, 1, 0, 0, 0);
            ClearChart(chart10, 1, 0, 0, 0, 1);
            ClearChart(chart11, 1, 0, 0, 0, 1);
            ClearChart(chart12, 1, 0, 0, 0, 1);
            ClearChart(chart4, 1, 0, 0, 0, 1);
            ClearChart(chart5, 1, 0, 0, 0, 1);
            ClearChart(chart6, 1, 0, 0, 0, 1);
        }

        private void AxisXChartValueFunction()
        {
            AxisXChartValue(chart3, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart1, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart2, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart10, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart9, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart8, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart7, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart11, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart12, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart4, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart5, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
            AxisXChartValue(chart6, 0, Convert.ToInt32(Shared.jsonData.GraphicsLimit));
        }

        delegate void AxisXChartValueCallback(Chart chart, int minValue, int maxValue);
        private void AxisXChartValue(Chart chart, int minValue, int maxValue)
        {
            if (chart.InvokeRequired)
            {
                AxisXChartValueCallback d = new AxisXChartValueCallback(_AxisXChartValue);
                chart.Invoke(d, new object[] { chart, minValue, maxValue });
            }
            else
            {
                _AxisXChartValue(chart, minValue, maxValue);
            }
        }

        private void _AxisXChartValue(Chart chart, int minValue, int maxValue)
        {
            chart.ChartAreas[0].AxisX.Minimum = minValue;
            chart.ChartAreas[0].AxisX.Maximum = maxValue;
        }

        delegate void InsertXYChartCallback(Chart chart, int selectionSeries, int value);
        private void InsertXYChart(Chart chart, int selectionSeries, int value)
        {
            if (chart.InvokeRequired)
            {
                InsertXYChartCallback d = new InsertXYChartCallback(_InsertXYChart);
                chart.Invoke(d, new object[] { chart, selectionSeries, value });
            }
            else
            {
                _InsertXYChart(chart, selectionSeries, value);
            }
        }

        private void _InsertXYChart(Chart chart, int selectionSeries, int value)
        {
            double graphicsLim = Convert.ToInt32(Shared.jsonData.GraphicsLimit);
            if (chart.Series[selectionSeries].Points.Count > graphicsLim)
                chart.Series[selectionSeries].Points.RemoveAt(0);

            chart.Series[selectionSeries].Points.Add(value);
        }

        delegate void ClearChartCallback(Chart chart, int series1, int series2, int series3, int series4, int series5);
        private void ClearChart(Chart chart, int series1, int series2, int series3, int series4, int series5)
        {
            if (chart.InvokeRequired)
            {
                ClearChartCallback d = new ClearChartCallback(_ClearChart);
                chart.Invoke(d, new object[] { chart, series1, series2, series3, series4, series5 });
            }
            else
            {
                _ClearChart(chart, series1, series2, series3, series4, series5);
            }
        }

        private void _ClearChart(Chart chart, int series1, int series2, int series3, int series4, int series5)
        {
            if (series1 == 1)
                chart.Series["Series1"].Points.Clear();

            if (series2 == 1)
                chart.Series["Series2"].Points.Clear();

            if (series3 == 1)
                chart.Series["Series3"].Points.Clear();

            if (series4 == 1)
                chart.Series["Series4"].Points.Clear();

            if (series5 == 1)
                chart.Series["Series5"].Points.Clear();
        }

        delegate void SetChartCallback(Chart chart, int seriesCount, double value);
        private void SetChart(Chart chart, int seriesCount, double value)
        {
            if (chart.InvokeRequired)
            {
                SetChartCallback d = new SetChartCallback(_SetChart);
                chart.Invoke(d, new object[] { chart, value });
            }
            else
            {
                _SetChart(chart, seriesCount, value);
            }
        }

        private void _SetChart(Chart chart, int seriesCount, double value)
        {
            if (chart.Series[seriesCount].Points.Count > 100)
                chart.Series[seriesCount].Points.RemoveAt(0);
            chart.Series[seriesCount].Points.Add(value);

            if (graphicsMaximumDelta < value)
            {
                graphicsMaximumDelta = (int)value + 1;
                chart.ChartAreas["ChartArea1"].AxisY.Maximum = (int)value + 1;
                chart.ChartAreas["ChartArea1"].AxisY.Minimum = 0;
            }
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