using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Berger_Whiteness_Tint_Measurements.Classes
{
    public class Functions
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

        public static double[] LoopPixelDataProcess(SerialPort serialPort, string sendData, int[] nmCount, string choosingFilter, int loopCount, int dataLenght, int dataCount)
        {
            double[] pixelDataValue = new double[dataCount];
            double[] dataValue = new double[dataCount];
            for (int i = 0; i < loopCount; i++)
            {
                if (choosingFilter == "Normal")
                    dataValue = NormalDataProcess(serialPort, sendData, nmCount, dataLenght, dataCount);
                else if (choosingFilter == "MOA")
                    dataValue = MOADataProcess(serialPort, sendData, nmCount, dataLenght, dataCount);

                for (int j = 0; j < pixelDataValue.Length; j++)
                    pixelDataValue[j] = pixelDataValue[j] + dataValue[j];
            }

            for (int k = 0; k < pixelDataValue.Length; k++)
                pixelDataValue[k] = pixelDataValue[k] / loopCount;

            return pixelDataValue;
        }

        public static double[] NormalDataProcess(SerialPort serialPort, string sendData, int[] nmCount, int dataLenght, int dataCount)
        {
            int dataLenghtTwo = dataLenght * 2;
            string[] dataHex = new string[dataLenghtTwo];
            byte[] buffer = new byte[dataLenghtTwo];
            double[] dataValue = new double[dataLenght];
            double[] pixelData = new double[dataCount];

            string dataA = "";
            string dataB = "";
            int receiverCount = 0;
            int pixelInteger = 0;
            int pixelSquence = 0;
            int m = 0;
            int n = 0;

            serialPort.Write(sendData);
            while (dataLenghtTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataLenghtTwo);
                receiverCount += n;
                dataLenghtTwo -= n;
            }

            for (int i = 0; i < dataHex.Length; i++)
            {
                dataA = buffer[i].ToString("X");
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
                pixelInteger = Int32.Parse(dataB, NumberStyles.HexNumber);
                dataValue[m] = Convert.ToDouble(pixelInteger);
                m++;
            }

            for (int p = 0; p < pixelData.Length; p++)
            {
                pixelSquence = nmCount[p];
                pixelData[p] = dataValue[pixelSquence];
            }

            return pixelData;
        }

        public static double[] MOADataProcess(SerialPort serialPort, string sendData, int[] nmCount, int dataLenght, int dataCount)
        {
            int dataLenghtTwo = dataLenght * 2;
            string[] dataHex = new string[dataLenghtTwo];
            byte[] buffer = new byte[dataLenghtTwo];
            double[] dataValue = new double[dataLenght];
            double[] MOADataValue = new double[dataLenght];
            double[] pixelData = new double[dataCount];

            string dataA = "";
            int receiverCount = 0;
            string pixelHex = "";
            int pixelInteger = 0;
            int pixelSquence = 0;
            int m = 0;
            int n = 0;

            serialPort.Write(sendData);
            while (dataLenghtTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataLenghtTwo);
                receiverCount += n;
                dataLenghtTwo -= n;
            }

            for (int i = 0; i < dataHex.Length; i++)
            {
                dataA = buffer[i].ToString("X");
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
                pixelHex = dataHex[j] + dataHex[j + 1];
                pixelInteger = Int32.Parse(pixelHex, NumberStyles.HexNumber);
                dataValue[m] = Convert.ToDouble(pixelInteger);
                m++;
            }

            MOADataValue[0] = dataValue[0];
            MOADataValue[1] = dataValue[1];
            MOADataValue[2] = dataValue[2];
            MOADataValue[3] = dataValue[3];
            for (int k = 4; k < MOADataValue.Length; k++)
                MOADataValue[k] = (dataValue[k - 4] + dataValue[k - 3] + dataValue[k - 2] + dataValue[k - 1] + dataValue[k]) / 5;

            for (int p = 0; p < pixelData.Length; p++)
            {
                pixelSquence = nmCount[p];
                pixelData[p] = MOADataValue[pixelSquence];
            }

            return pixelData;
        }

        public static int[] NmRelatedPixelCalculation(SerialPort serialPort, int nmArrayLenght, int nmCountLenght, int firstNm)
        {
            string sendData = "";
            double[] nmArray = new double[nmArrayLenght];
            int[] nmCount = new int[nmCountLenght];
            int m = 0;
            int n = 0;
            double searchNumber = 2;

            sendData = "*PARAmeter:FIT0?<CR>\r";
            Parameters.eNmArguman = Functions.PixelNmArguman(serialPort, sendData);
            sendData = "*PARAmeter:FIT1?<CR>\r";
            Parameters.dNmArguman = Functions.PixelNmArguman(serialPort, sendData);
            sendData = "*PARAmeter:FIT2?<CR>\r";
            Parameters.cNmArguman = Functions.PixelNmArguman(serialPort, sendData);
            sendData = "*PARAmeter:FIT3?<CR>\r";
            Parameters.bNmArguman = Functions.PixelNmArguman(serialPort, sendData);
            sendData = "*PARAmeter:FIT4?<CR>\r";
            Parameters.aNmArguman = Functions.PixelNmArguman(serialPort, sendData);

            for (int i = 0; i < nmArray.Length; i++)
            {
                nmArray[i] = (Parameters.aNmArguman * Math.Pow(i, 4)) + (Parameters.bNmArguman * Math.Pow(i, 3)) +
                             (Parameters.cNmArguman * Math.Pow(i, 2)) + (Parameters.dNmArguman * Math.Pow(i, 1)) +
                             (Parameters.eNmArguman * Math.Pow(i, 0));
            }

            while (m < nmArray.Length && n < nmCount.Length)
            {
                searchNumber = Math.Abs(nmArray[m] - (firstNm + (n * 5)));
                if (searchNumber < 1)
                {
                    nmCount[n] = m;
                    m = 0;
                    n++;
                }
                else
                {
                    m++;
                }
            }

            return nmCount;
        }

        public static double PixelNmArguman(SerialPort serialPort, string sendData)
        {
            int valueLenght = 0;
            int n = 0;
            int receiverCount = 0;
            string getDataString = "";

            serialPort.Write(sendData);
            while (valueLenght < 13)
                valueLenght = serialPort.BytesToRead;

            byte[] buffer = new byte[valueLenght];
            while (valueLenght > 0)
            {
                n = serialPort.Read(buffer, receiverCount, valueLenght);
                receiverCount += n;
                valueLenght -= n;
            }

            for (int i = 0; i < buffer.Length; i++)
                getDataString += (char)Convert.ToByte(buffer[i]);
            getDataString = getDataString.Replace(".", ",");
            double getData = Convert.ToDouble(getDataString);

            return getData;
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

        public static double XYZCalculation(double[] RStandartSample, int firstNm, int lastNm, double[] d65, double[] a10, double[] y10, double deltaLamda)
        {
            double xyzMulti = 0;
            double xyzSample = 0;

            int firstIndex = (firstNm - 380) / 5;
            int lastIndex = (lastNm - 380) / 5;
            for (int i = firstIndex; i <= lastIndex; i++)
                xyzMulti = xyzMulti + (RStandartSample[i] * d65[i] * a10[i] * deltaLamda);
            xyzSample = xyzMulti * KFunction(firstIndex, lastIndex, d65, y10, deltaLamda);

            return xyzSample;
        }

        public static double KFunction(int firstIndex, int lastIndex, double[] d65, double[] y10, double deltaLamda)
        {
            double sumK = 0;
            for (int i = firstIndex; i <= lastIndex; i++)
                sumK = sumK + (d65[i] * y10[i] * deltaLamda);
            double sumResultK = 100 / sumK;

            return sumResultK;
        }

        public static void ReadFile(string fileName, int textCount, string[] texts, int firstText, int lastText)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            int k = 0;
            int m = 0;
            string[] textData = new string[textCount];

            string text = streamReader.ReadLine();
            while (text != null)
            {
                textData[k] = text;
                text = streamReader.ReadLine();
                k++;
            }

            streamReader.Close();
            fileStream.Close();

            for (int i = firstText; i <= lastText; i++)
            {
                texts[m] = textData[i];
                m++;
            }
        }

        public static void writeFile(string fileName, string[] texts)
        {
            File.WriteAllText(fileName, String.Empty);
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            for (int i = 0; i < texts.Length; i++)
                streamWriter.WriteLine(texts[i]);
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static void DrawGraphics(Chart chart, double[] pixelSectionDataValue, string graphicName, int seriesIndex, int seriesIndexType, int graphicMinimum, int graphicMaximum, int aColor, int bColor, int cColor)
        {
            Random random = new Random();

            chart.Series.Add(graphicName + (seriesIndexType).ToString());
            chart.ChartAreas["ChartArea1"].AxisX.Minimum = 380;
            chart.ChartAreas["ChartArea1"].AxisX.Maximum = 780;
            chart.ChartAreas["ChartArea1"].AxisY.Minimum = graphicMinimum;
            chart.ChartAreas["ChartArea1"].AxisY.Maximum = graphicMaximum;

            for (int j = 0; j < Parameters.dataCount; j++)
            {
                chart.Series[seriesIndex].ChartType = SeriesChartType.Line;
                chart.Series[seriesIndex].Color = Color.FromArgb(random.Next(0, aColor), random.Next(0, bColor), random.Next(0, cColor));
                chart.Series[seriesIndex].Points.AddXY(380 + (j * 5), pixelSectionDataValue[j]);
            }
        }

        public static double GraphicMinimumMethod(double[] pixelSectionDataValue)
        {
            double graphicsMinumumValue = pixelSectionDataValue[0];
            for (int i = 0; i < pixelSectionDataValue.Length; i++)
            {
                if (graphicsMinumumValue > pixelSectionDataValue[i])
                    graphicsMinumumValue = pixelSectionDataValue[i];
            }

            return graphicsMinumumValue;
        }

        public static double GraphicMaximumMethod(double[] pixelSectionDataValue)
        {
            double graphicsMaximumValue = pixelSectionDataValue[0];
            for (int i = 0; i < pixelSectionDataValue.Length; i++)
            {
                if (graphicsMaximumValue < pixelSectionDataValue[i])
                    graphicsMaximumValue = pixelSectionDataValue[i];
            }

            return graphicsMaximumValue;
        }
    }
}
