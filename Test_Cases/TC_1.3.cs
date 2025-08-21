/*
 * TC_1_3: IMS Re-registration on 3G Test Case
 * --------------------------------------------
 * Purpose:
 *   Verify that IMS re-registration occurs when the device is switched to 3G-only mode.
 * 
 * Steps:
 *   1. Start ADB radio log capture.
 *   2. Switch device to 3G-only mode.
 *   3. Wait for IMS re-registration.
 *   4. Reset device state.
 */

using FIT_Automation.Scripts;
using NLog;
using System;
using System.Threading;
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
            gclass = new GlobalVarClass(_deviceId, _outputRTB, null);
        }

        public void RunTest()
        {
            string result = "FAIL";

            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.3: IMS Re-registration on 3G");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Start ADB radio log capture ---
                gclass.UpdateOutput("[Step 1] Starting ADB Radio log capture...");
                gclass.RunAdbCommand("cmd.exe /c adb logcat -b radio -v threadtime > log_radio_TC13.txt");

                // --- Step 2: Switch to 3G-only mode ---
                gclass.UpdateOutput("[Step 2] Switching device to 3G-only mode...");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell svc data disable");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell settings put global preferred_network_mode 1");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell svc data enable");
                Thread.Sleep(10000);

                // --- Step 3: Wait for IMS re-registration ---
                gclass.UpdateOutput("[Step 3] Waiting for IMS re-registration...");
                if (WaitForIMSReRegistration())
                {
                    gclass.UpdateOutput("TC 1.3: IMS re-registered successfully on 3G. PASS");
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("TC 1.3: IMS re-registration failed on 3G. FAIL", true);
                    result = "FAIL";
                }

                // --- Step 4: Reset device state ---
                gclass.UpdateOutput("[Step 4] Resetting device state...");
                gclass.SetAirplaneMode(_deviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.3 failed: {ex.Message}", true);
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.3", _deviceId, result);
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
