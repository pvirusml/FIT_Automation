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
        GlobalVarClass gclass;

        public TC_1_3(string deviceId, RichTextBox outputRTB)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, null); // No button needed for this test
        }

        public void RunTest()
        {
            gclass.UpdateOutput("Starting TC 1.3: IMS Re-registration on 3G");

            try
            {
                gclass.UpdateOutput("Starting ADB Radio log capture...");
                gclass.RunAdbCommand("cmd.exe /c adb logcat -b radio -v threadtime > log_radio_TC13.txt"); // Clear previous logs

                // Step 1: Switch to 3G
                gclass.UpdateOutput("Switching device to 3G-only mode...");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell svc data disable");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell settings put global preferred_network_mode 1");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell svc data enable");

                Thread.Sleep(10000); // Wait for 3G switch

                // Step 2: Check IMS re-registration
                if (WaitForIMSReRegistration())
                {
                    gclass.UpdateOutput("TC 1.3: IMS re-registered successfully on 3G. PASS");
                }
                else
                {
                    gclass.UpdateOutput("TC 1.3: IMS re-registration failed on 3G. FAIL", true);
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.3 failed: {ex.Message}", true);
            }
        }

        private bool WaitForIMSReRegistration()
        {
            int attempts = 10;
            for (int i = 0; i < attempts; i++)
            {
                string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry");

                if (output.Contains("VoLTE") && output.Contains("registered"))
                    return true;

                Thread.Sleep(5000);
            }
            return false;
        }

    }
}
