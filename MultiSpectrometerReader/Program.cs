using MultiSpectrometerReader.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSpectrometerReader
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{5eeb9a00-f180-4f91-b330-7bdf123d30b3}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Shared.Initialize();
                Application.Run(Shared.mainForm);
            }
            else
            {
                string appName = "MultiSpectrometerReader";
                List<int> processIDCalls = ProcessID(appName);
                if (processIDCalls.Count != 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Application is already running!\nDo you want to shut down the application?", 
                                                "Are you sure!", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        for (int i = 0; i < processIDCalls.Count; i++)
                        {
                            Process process = Process.GetProcessById(processIDCalls[i]);
                            process.Kill();
                        }
                    }
                }
            }
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
    }
}
