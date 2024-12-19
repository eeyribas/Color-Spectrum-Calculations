using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandardDeviation
{
    class Functions
    {
        public static void OpenSerialPort(SerialPort serialPort, string selectPortName, int baudRate)
        {
            serialPort.PortName = selectPortName;
            serialPort.BaudRate = baudRate;
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

        public static double SumAverageWaveLoopCount(SerialPort serialPort, string sendData, int averageValue, int sumLoopCount, int dataLenght, int dataCount, int firstPixel, double firstPixelD)
        {
            double sum = 0;
            for (int i = 0; i < sumLoopCount; i++)
            {
                double tmp = Functions.SumAverageWave(serialPort, sendData, averageValue, dataLenght, dataCount, firstPixel, firstPixelD);
                sum = sum + tmp;
            }
            sum = sum / sumLoopCount;
            sum = Math.Round(sum, 4);

            return sum;
        }

        public static double SumAverageWave(SerialPort serialPort, string sendData, int averageValue, int dataLenght, int dataCount, int firstPixel, double firstPixelD)
        {
            double[] pixelDataValue = new double[dataCount];
            double[] pixelSectionDataValue = new double[101];
            double[] nmDataValue = new double[dataCount];
            double[] nmSectionDataValue = new double[101];
            double sum = 0;
            int m = 0, n = 0;
            double searchNumber = 2;

            pixelDataValue = PixelDataProcess(serialPort, sendData, averageValue, dataLenght, dataCount, firstPixel);
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

            for (int j = 10; j < pixelSectionDataValue.Length - 30; j++)
                sum = sum + pixelSectionDataValue[j];

            return sum;
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
            int m = 0, n = 0;

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

        public static double[] NmCalculation(int dataCount, int firstPixel, double firstPixelD)
        {
            double slope = 1.9868;
            firstPixel = (int)firstPixelD - 1;
            double[] nmDataValueSum = new double[dataCount];
            for (int i = 0; i < dataCount; i++)
                nmDataValueSum[i] = Math.Round((((firstPixel + i) - firstPixelD) * slope) + 350, 2);

            return nmDataValueSum;
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
            int validation = serialPort.ReadByte();

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
            string[] pixelSettings = new string[2];
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            int k = 0;

            string text = streamReader.ReadLine();
            while (text != null)
            {
                pixelSettings[k] = text;
                text = streamReader.ReadLine();
                k++;
            }

            streamReader.Close();
            fileStream.Close();

            return pixelSettings;
        }

        public static void WriteFile(string fileName, string text1, string text2)
        {
            File.WriteAllText(fileName, String.Empty);
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamReader = new StreamWriter(fileStream);
            streamReader.WriteLine(text1);
            streamReader.WriteLine(text2);
            streamReader.Flush();
            streamReader.Close();
            fileStream.Close();
        }
    }
}
