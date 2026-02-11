/*
 * TC_2_08: SMS During VoWiFi Call to voLTE on AP Mode Test Case 
 * ------------------------------------------
 * Purpose:
 *   Verify that an SMS can be recieved during a VoWiFi call and is received by the reference device.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, enable WiFi for both devices.
 *   3. Place and answer call.
 *   4. Send SMS during call.
 *   5. End call and reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_2_08
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();


        public TC_2_08(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            _refDeviceId = refDeviceId;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                // Ensure thread-safe logging of header
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 2.08: Verify recieved SMS during VoWiFi call while calling VoLTE device...");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT or REF not connected. [{_deviceId}, {_refDeviceId}]");

                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME");

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.EnableWiFi(_deviceId);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                Thread.Sleep(13000);

                gclass.WaitForLTEAndVoLTERegistration(_refDeviceId);

                // Place and answer call
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                if (string.IsNullOrWhiteSpace(targetNumber))
                    throw new Exception($"REF number missing. [{_refDeviceId}]");

                string callCmd = $"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{targetNumber}";
                gclass.RunAdbCommand(callCmd);
                Thread.Sleep(11000);
                gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(6000);

                // Confirm call is OFFHOOK
                bool callActive = false;
                for (int i = 0; i < 10; i++)
                {
                    string state = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();
                    if (state.Contains("callstate=2"))
                    {
                        callActive = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }
                if (!callActive)
                {
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput($"TC 2.08: FAIL [{_deviceId}, {_refDeviceId}] - Call was not established.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC2.08", _deviceId, result);
                    return;
                }

                // Send SMS during call
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForSentSMS(_refDeviceId, _deviceId);
                Thread.Sleep(5000);

                // End call and reset device state
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");

                if (gclass.IsSMSSent)
                {
                    gclass.UpdateOutput($"TC 2.08: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 2.08: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 2.08: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Ensure devices are reset to a known state
                gclass.resetAll(_deviceId);
                gclass.resetAll(_refDeviceId);
            }

            // Log footer ONCE
            //gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC2.08", _deviceId, result);
        }
    }
}
