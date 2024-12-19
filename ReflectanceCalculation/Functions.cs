using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectanceCalculation
{
    class Functions
    {
        public static void OpenSerialPort(SerialPort serialPort, string selectPortName, int baudRate)
        {
            serialPort.PortName = selectPortName;
            serialPort.BaudRate = 3000000;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = 5000;
            serialPort.WriteTimeout = 5000;

            serialPort.Open();
        }

        public static void CloseSerialPort(SerialPort serialPort)
        {
            serialPort.Close();
        }

        public static double[] WaveCalculation(SerialPort serialPort, string sendData, int averageValue, int section, int loopCount, int dataLenght, int dataCount, int firstPixel, double firstPixelD)
        {
            double[] pixelDataValue = new double[dataCount];
            double[] pixelSectionDataValue = new double[section];
            double[] nmDataValue = new double[dataCount];
            double[] nmSectionDataValue = new double[section];
            double[] pixelNmSectionDataValue = new double[section * 2];
            int k = 0, m = 0, n = 0, s = 0;
            double searchNumber = 2;

            pixelDataValue = LoopPixelDataProcess(serialPort, sendData, averageValue, loopCount, dataLenght, dataCount, firstPixel);
            nmDataValue = NmCalculation(dataCount, firstPixel, firstPixelD);

            while (m < nmDataValue.Length)
            {
                searchNumber = Math.Abs(nmDataValue[m] - (350 + (n * 5)));
                if (searchNumber < 1)
                {
                    nmSectionDataValue[n] = 350 + (n * 5);
                    pixelSectionDataValue[n] = pixelDataValue[m];
                    m = 0;
                    n++;
                }
                else
                {
                    m++;
                }
            }

            while (k < pixelNmSectionDataValue.Length)
            {
                pixelNmSectionDataValue[k] = pixelSectionDataValue[s];
                pixelNmSectionDataValue[k + 1] = nmSectionDataValue[s];
                k = k + 2;
                s++;
            }

            return pixelNmSectionDataValue;
        }

        public static double[] NmCalculation(int dataCount, int firstPixel, double firstPixelD)
        {
            double slope = 1.9868;
            firstPixel = (int)firstPixelD - 1;

            double[] nmDataValueSum = new double[dataCount];
            for (int i = 0; i < dataCount; i++)
                nmDataValueSum[i] = Math.Round((((firstPixel + i) - firstPixelD) * slope) + 350, 2);

            return nmDataValueSum;
        }

        public static double[] LoopPixelDataProcess(SerialPort serialPort, string sendData, int averageValue, int loopCount, int dataLenght, int dataCount, int firstPixel)
        {
            double[] loopPixelDataValue = new double[dataCount];
            double[] dataValue = new double[dataCount];

            for (int i = 0; i < loopCount; i++)
            {
                dataValue = PixelDataProcess(serialPort, sendData, averageValue, dataLenght, dataCount, firstPixel);
                for (int j = 0; j < dataCount; j++)
                    loopPixelDataValue[j] = loopPixelDataValue[j] + dataValue[j];
            }

            for (int k = 0; k < dataCount; k++)
            {
                loopPixelDataValue[k] = loopPixelDataValue[k] / loopCount;
                loopPixelDataValue[k] = Math.Round(loopPixelDataValue[k], 4);
            }

            return loopPixelDataValue;
        }

        public static double[] PixelDataProcess(SerialPort serialPort, string sendData, int averageValue, int dataLenght, int dataCount, int firstPixel)
        {
            double[] pixelDataValue = new double[dataCount];
            int[] dataValue = new int[dataCount];

            for (int i = 0; i < averageValue; i++)
            {
                dataValue = DataProcess(serialPort, sendData, dataLenght, dataCount, firstPixel);
                for (int j = 0; j < dataCount; j++)
                    pixelDataValue[j] = pixelDataValue[j] + dataValue[j];
            }

            for (int k = 0; k < dataCount; k++)
            {
                pixelDataValue[k] = pixelDataValue[k] / averageValue;
                pixelDataValue[k] = Math.Round(pixelDataValue[k], 4);
            }

            return pixelDataValue;
        }

        public static int[] DataProcess(SerialPort serialPort, string sendData, int dataLenght, int dataCount, int firstPixel)
        {
            int dataHexLenght = dataCount * 2;
            int dataCountTwo = dataLenght * 2;
            string[] dataHex = new string[dataHexLenght];
            int[] dataPixel = new int[dataCount];
            byte[] buffer = new byte[dataCountTwo];
            string dataA = "";
            string dataB = "";
            int receiverCount = 0;
            int pixelHex = 0;
            int m = 0;
            int n = 0;

            serialPort.Write(sendData);
            while (dataCountTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataCountTwo);
                receiverCount += n;
                dataCountTwo -= n;
            }

            for (int i = 0; i < dataHex.Length; i++)
            {
                pixelHex = (firstPixel * 2) + i;
                dataA = buffer[pixelHex].ToString("X");
                if (dataA.Length != 2)
                {
                    dataA = "0" + dataA;
                    dataHex[i] = dataA;
                }
                else
                {
                    dataHex[i] = dataA;
                }
            }

            for (int j = 0; j < dataHex.Length; j += 2)
            {
                dataB = dataHex[j] + dataHex[j + 1];
                dataPixel[m] = Int32.Parse(dataB, NumberStyles.HexNumber);
                m++;
            }

            return dataPixel;
        }

        public static void ShortDataProcess(SerialPort serialPort, string sendData, int dataLenght)
        {
            int dataCountTwo = dataLenght * 2;
            byte[] buffer = new byte[dataCountTwo];
            int receiverCount = 0;
            int n = 0;

            serialPort.Write(sendData);
            while (dataCountTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataCountTwo);
                receiverCount += n;
                dataCountTwo -= n;
            }
        }

        public static int IntegrationTimeSetting(SerialPort serialPort, string choosingIntegration)
        {
            string setIntegrationTime = "*PARAmeter:TINT " + choosingIntegration + "<CR>\r";
            serialPort.Write(setIntegrationTime);
            int validation = serialPort.ReadByte();

            return validation;
        }

        public static int DigitalGainSetting(SerialPort serialPort, string choosingDigitalGain)
        {
            string digitalGain = "";
            if (choosingDigitalGain == "0")
                digitalGain = "*PARAmeter:PDAGain " + "0" + "<CR>\r";
            else if (choosingDigitalGain == "1")
                digitalGain = "*PARAmeter:PDAGain " + "1" + "<CR>\r";
            else
                digitalGain = "*PARAmeter:PDAGain " + "0" + "<CR>\r";

            serialPort.Write(digitalGain);
            int  validation = serialPort.ReadByte();

            return validation;
        }

        public static int AnalogGainSetting(SerialPort serialPort, string analogGainValue)
        {
            string analogGain = "*PARAmeter:GAIN " + analogGainValue + "<CR>\r";
            serialPort.Write(analogGain);
            int validation = serialPort.ReadByte();

            return validation;
        }

        public static string[] ReadFile(string fileName)
        {
            string[] pixelSetting = new string[2];
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            int k = 0;

            string text = streamReader.ReadLine();
            while (text != null)
            {
                pixelSetting[k] = text;
                text = streamReader.ReadLine();
                k++;
            }
            streamReader.Close();
            fileStream.Close();

            return pixelSetting;
        }

        public static void WriteFile(string fileName, string text1, string text2)
        {
            File.WriteAllText(fileName, String.Empty);
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(text1);
            streamWriter.WriteLine(text2);
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static void WriteFile(string fileName, double[] value1, double[] value2)
        {
            File.WriteAllText(fileName, String.Empty);
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine("------------");
            for (int i = 0; i < value1.Length; i++)
            {
                streamWriter.Write(value1[i]);
                streamWriter.Write("-");
                streamWriter.WriteLine(value2[i]);
            }
            streamWriter.WriteLine("------------");
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static double[] whitePlateValues = { 0.85800, 0.86810, 0.87820, 0.88565, 0.89310, 0.89670, 0.90030, 0.90605, 0.91180, 0.91350,
                                                    0.91520, 0.91745, 0.91970, 0.91955, 0.91940, 0.92260, 0.92580, 0.92235, 0.91890, 0.92265,
                                                    0.92640, 0.92855, 0.93070, 0.93495, 0.93920, 0.93810, 0.93700, 0.93515, 0.93330, 0.93470,
                                                    0.93610, 0.93900, 0.94190, 0.94155, 0.94120, 0.94090, 0.94060, 0.94165, 0.94270, 0.94590,
                                                    0.94910, 0.94875, 0.94840, 0.95125, 0.95410, 0.95655, 0.95900, 0.95940, 0.95980, 0.95595,
                                                    0.95210, 0.95440, 0.95670, 0.96190, 0.96710, 0.96710, 0.96710, 0.96650, 0.96590, 0.96810,
                                                    0.97030};
    }
}
