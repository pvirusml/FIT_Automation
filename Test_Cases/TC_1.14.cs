/*
 * TC_1_14: MMS During VoWiFi Call Test Case
 * ------------------------------------------
 * Purpose:
 *   Verify that an MMS can be sent during a VoWiFi call and is received by the reference device.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, enable WiFi for both devices.
 *   3. Wait for LTE/VoWiFi registration.
 *   4. Place and answer call.
 *   5. Send MMS during call.
 *   6. End call and reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_14
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_14(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            // Log header ONCE (not per device pair)
            if (!headerLogged)
            {
                gclass.UpdateOutput("==================================================");
                gclass.UpdateOutput("Starting TC 1.14: Verify MMS during VoWiFi call...");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }

            try
            {
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT or REF not connected. [{_deviceId}, {_refDeviceId}]");

                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME");

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, false);
                Thread.Sleep(12000);

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
                    gclass.UpdateOutput($"TC 1.14: FAIL [{_deviceId}, {_refDeviceId}] - Call was not established.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.14", _deviceId, result);
                    return;
                }

                // Send MMS during call
                gclass.SendMMS(_deviceId, targetNumber, "MMSTest");
                gclass.CheckForSentMMS(_deviceId, _refDeviceId);

                // End call and reset device state
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsMMSSent)
                {
                    gclass.UpdateOutput($"TC 1.14: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.14: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.14: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            // Log footer ONCE
            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.14", _deviceId, result);
        }
    }
}
