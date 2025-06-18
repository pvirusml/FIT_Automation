using FIT_Automation.Scripts;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FIT_Automation.Test_Cases
{
    public class TC_1_3
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        GlobalVarClass gClass = new GlobalVarClass();

        public TC_1_3(string deviceId, RichTextBox outputRTB)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
        }

        public void RunTest()
        {
            UpdateOutput("Starting TC 1.3: IMS Re-registration on 3G");

            try
            {
                // Step 1: Switch to 3G
                UpdateOutput("Switching device to 3G-only mode...");
                gClass.RunAdbCommand($"adb -s {_deviceId} shell svc data disable");
                gClass.RunAdbCommand($"adb -s {_deviceId} shell settings put global preferred_network_mode 1");
                gClass.RunAdbCommand($"adb -s {_deviceId} shell svc data enable");

                Thread.Sleep(10000); // Wait for 3G switch

                // Step 2: Check IMS re-registration
                if (WaitForIMSReRegistration())
                {
                    UpdateOutput("TC 1.3: IMS re-registered successfully on 3G. PASS");
                }
                else
                {
                    UpdateOutput("TC 1.3: IMS re-registration failed on 3G. FAIL", true);
                }
            }
            catch (Exception ex)
            {
                UpdateOutput($"TC 1.3 failed: {ex.Message}", true);
            }
        }

        private bool WaitForIMSReRegistration()
        {
            int attempts = 10;
            for (int i = 0; i < attempts; i++)
            {
                string output = gClass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry");

                if (output.Contains("VoLTE") && output.Contains("registered"))
                    return true;

                Thread.Sleep(5000);
            }
            return false;
        }

        private void UpdateOutput(string message, bool isError = false)
        {
            if (_outputRTB.InvokeRequired)
            {
                _outputRTB.Invoke(new Action(() => UpdateOutput(message, isError)));
            }
            else
            {
                _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
                _outputRTB.SelectionColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                _outputRTB.ScrollToCaret();
            }
        }
    }
}
