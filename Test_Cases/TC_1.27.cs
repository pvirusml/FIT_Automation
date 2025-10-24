/*
 * TC_1_27: Call Drop on W-Fi Calling Off and Call Failure Notification Test Case
 * -----------------------------------------------------------------------
 * Purpose:
 *   Verify that when a call is placed from DUT 1 to DUT 2 over Wi-Fi, disabling WiFi on DUT 1
 *   causes a call drop and both devices display a "Call failure" notification (DUT 2 after RTP/RTCP timeout).
 * 
 * Steps:
 *   1. Place a call from DUT 1 to DUT 2.
 *   2. Ensure call is connected and audio is OK.
 *   3. While call is ongoing, enable AP mode DUT 1.
 *   4. DUT 1 should show "Call failure" UI notification.
 *   5. On DUT 2, call should fail after RTP/RTCP timeout value (e.g., 10s), and show "Call failure" notification.
 */

using FIT_Automation.Scripts;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_27
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();

        public TC_1_27(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
            _refDeviceId = refDeviceId;
        }

        public void RunTest()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                // Log header ONCE (not per device pair)
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.27: Call drop on Wi-Fi Calling loss and call failure notification");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT & REF are not connected. [{_deviceId}, {_refDeviceId}]");

                // Place a call from DUT 1 to DUT 2
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                if (string.IsNullOrWhiteSpace(targetNumber))
                    throw new Exception($"Failed to extract phone number from REF device [{_refDeviceId}]");

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                Thread.Sleep(21000);

                gclass.PlaceCall(_deviceId, targetNumber);
                Thread.Sleep(5000);
                gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Ensure call is connected and audio is OK
                string callState = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();
                if (!callState.Contains("callstate=2"))
                    throw new Exception("Call was not connected on DUT 1.");

                callState = gclass.RunAdbCommand($"adb -s {_refDeviceId} shell dumpsys telephony.registry").ToLower();
                if (!callState.Contains("callstate=2"))
                    throw new Exception("Call was not connected on DUT 2.");

                //gclass.UpdateOutput("Call is connected and audio is OK.");

                // While call is ongoing, disable Wi-Fi on DUT 1
                gclass.DisableWiFi(_deviceId);
                //gclass.UpdateOutput("Wi-Fi disabled on DUT 1. Waiting for call failure notification...");
                // Wait for "Call failure" UI notification on DUT 2 (after RTP/RTCP timeout)
                Thread.Sleep(4000); // Wait for RTP/RTCP timeout (e.g., 10s) plus buffer
                string dumpPath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                gclass.CaptureUIDump(_refDeviceId, Path.GetDirectoryName(dumpPath1));

                bool dut2CallFailure = !gclass.isInUIDumpWithExc(_refDeviceId, dumpPath1, "Mute")
                    && !gclass.isInUIDumpWithExc(_refDeviceId, dumpPath1, "Speaker") && !gclass.isInUIDumpWithExc(_refDeviceId, dumpPath1, "Hold");
                if (!dut2CallFailure)
                    throw new Exception("Call failure notification not detected on DUT 2 after RTP/RTCP timeout.");

                // Wait for "Call failure" UI notification on DUT 1
                string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                gclass.CaptureUIDump(_deviceId, Path.GetDirectoryName(dumpPath));

                bool dut1CallFailure = gclass.isInUIDumpWithExc(_deviceId, dumpPath, "Mobile network is not available. Connect to a wireless network to make a call.");//gclass.WaitForCallFailureNotification(_deviceId, 15);
                Thread.Sleep(3000);
                if (!dut1CallFailure)
                    throw new Exception("Call failure notification not detected on DUT 1.");

                //gclass.UpdateOutput("Call failure notification detected on DUT 1. Waiting for DUT 2 to detect call failure...");

                /*
                // While call is ongoing, disable Wi-Fi Calling on DUT 1
                //swipe down notification shade and disable Wi-Fi calling
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME");
                Thread.Sleep(2000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell am start -a android.intent.action.DIAL");
                Thread.Sleep(2000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input tap 600 700"); // Cick on nywhere to ensure we are on main page
                Thread.Sleep(2000);
                gclass.SelectMoreOptionsOnDialerApp(_deviceId);
                Thread.Sleep(3000);
                gclass.SelectCallsInSettingsInMoreOptionsOnDialerApp(_deviceId);
                Thread.Sleep(3000);
                gclass.SelectWiFiCallingInCallsInSettingsInMoreOptionsOnDialerApp(_deviceId);
                Thread.Sleep(3000);
                gclass.SelectReadyForCallsInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerApp(_deviceId);
                Thread.Sleep(3000);

                /*
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 500 500 500 1500");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 500 500 500 1500");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 797 603 325 620");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input tap 325 620"); // press wifi calling to disable
                Thread.Sleep(4000);
                */
                

                gclass.UpdateOutput($"TC 1.27: PASS [{_deviceId}, {_refDeviceId}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.27: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                /*
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 500 500 500 1500");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 500 500 500 1500");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input swipe 797 603 325 620");
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input tap 325 620"); // press wifi calling to disable
                Thread.Sleep(4000);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME"); // tap outside to close notification shade
                */

                gclass.DisableWiFi(_refDeviceId);
                gclass.DisableWiFi(_deviceId);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_HOME");
                gclass.LogTestResultToCSV("TC1.27", _deviceId, result);
            }
            
        }

        
    }
}
