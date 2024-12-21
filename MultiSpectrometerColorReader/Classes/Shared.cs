using MultiSpectrometerColorReader.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSpectrometerColorReader.Classes
{
    public class Shared
    {
        public static JsonData jsonData;
        public static MainForm mainForm = null;
        public static ParameterSettingsForm parameterSettingsForm = null;
        public static ListDataForm listDataForm = null;
        public static MeasGraphicsForm measGraphicsForm = null;
        public static DeviceSettingsForm deviceSettingsForm = null;

        public static void Initialize()
        {
            jsonData = new JsonData();
            mainForm = new MainForm();
            parameterSettingsForm = new ParameterSettingsForm();
            listDataForm = new ListDataForm();
            measGraphicsForm = new MeasGraphicsForm();
            deviceSettingsForm = new DeviceSettingsForm();
        }
    }
}
