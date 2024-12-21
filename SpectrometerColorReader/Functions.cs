using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrometerColorReader
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

        public static double[] NmCalculation(int dataCount, int firstPixel, double firstPixelD)
        {
            double slope = 1.9868;
            firstPixel = (int)firstPixelD - 1;
            double[] nmDataValueSum = new double[dataCount];
            for (int i = 0; i < dataCount; i++)
                nmDataValueSum[i] = Math.Round((((firstPixel + i) - firstPixelD) * slope) + 350, 2);

            return nmDataValueSum;
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
            int n = 0;
            int l = 0;

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
                dataPixel[l] = Int32.Parse(dataB, System.Globalization.NumberStyles.HexNumber);
                l++;
            }

            return dataPixel;
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

        public static string DataDefinition(SerialPort serialPort, string sendData, int dataDefinitionLenght)
        {
            int[] dataDefinition = new int[dataDefinitionLenght];
            string getDataDefinition = "";
            serialPort.Write(sendData);
            for (int i = 0; i < dataDefinition.Length; i++)
            {
                dataDefinition[i] = serialPort.ReadByte();
                getDataDefinition += (char)Convert.ToByte(dataDefinition[i]);
            }

            return getDataDefinition;
        }

        public static byte[] AnalogGainSettingByte(SerialPort serialPort, string analogGainValue, int valueLenght)
        {
            string analogGain = "*PARAmeter:PDAGain " + analogGainValue + "<CR>\r";
            byte[] buffer = new byte[valueLenght];
            int n = 0;
            int receiverCount = 0;

            serialPort.Write(analogGain);
            while (valueLenght > 0)
            {
                n = serialPort.Read(buffer, receiverCount, valueLenght);
                receiverCount += n;
                valueLenght -= n;
            }

            return buffer;
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
    }
}
