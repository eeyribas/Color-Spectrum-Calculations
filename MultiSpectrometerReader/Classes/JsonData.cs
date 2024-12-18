using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSpectrometerReader.Classes
{
    public class JsonData
    {
        public string SelectDevicePort1 { get; set; }
        public string SelectDevicePort2 { get; set; }
        public string SelectDevicePort3 { get; set; }
        public string IntegrationTime { get; set; }
        public string AverageScan { get; set; }
        public string DigitalGain { get; set; }
        public string AnalogGain { get; set; }
        public string TestLoopCount { get; set; }
        public string OtherLoopCount { get; set; }
        public string Filter { get; set; }
        public string CmcType { get; set; }
        public string TimerValue { get; set; }
        public string DeltaELimit { get; set; }
        public string GraphicsLimit { get; set; }

        public JsonData()
        {
            SelectDevicePort1 = "A";
            SelectDevicePort2 = "B";
            SelectDevicePort3 = "C";
            IntegrationTime = "20";
            AverageScan = "5";
            DigitalGain = "Low";
            AnalogGain = "1.0";
            TestLoopCount = "1";
            OtherLoopCount = "1";
            Filter = "MOA";
            CmcType = "Normal";
            TimerValue = "100";
            DeltaELimit = "1";
            GraphicsLimit = "100";
        }

        public void Save()
        {
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(Shared.jsonData);
            File.WriteAllText(Parameters.jsonFilePath + Parameters.jsonName, data);
        }
    }
}
