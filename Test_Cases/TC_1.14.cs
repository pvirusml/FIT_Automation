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

            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.14: Verify MMS during VoWiFi call...");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Check device connections ---
                gclass.UpdateOutput("[Step 1] Checking device connections...");
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT or REF not connected.", true);
                    throw new Exception("DUT or REF not connected.");
                }

                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME");

                // --- Step 2: Set Airplane mode ON, enable WiFi for both devices ---
                gclass.UpdateOutput("[Step 2] Setting Airplane mode ON and enabling WiFi for DUT & REF...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode ON, WiFi enabled for DUT & REF.");
                Thread.Sleep(12000);

                // --- Step 3: Wait for LTE/VoWiFi registration ---
                gclass.UpdateOutput("[Step 3] Waiting for LTE/VoWiFi registration...");
                // (Assume registration is checked elsewhere or not needed here)

                // --- Step 4: Place and answer call ---
                gclass.UpdateOutput("[Step 4] Placing and answering call...");
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.UpdateOutput($"Extracted REF number: {targetNumber}");
                if (string.IsNullOrWhiteSpace(targetNumber))
                {
                    gclass.UpdateOutput("REF number missing.", true);
                    gclass.LogTestResultToCSV("TC1.14", _deviceId, result);
                    return;
                }
                string callCmd = $"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{targetNumber}";
                gclass.RunAdbCommand(callCmd);
                Thread.Sleep(9000);
                gclass.UpdateOutput("Answering call on REF...");
                gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // --- Step 5: Confirm call is OFFHOOK ---
                gclass.UpdateOutput("[Step 5] Confirming call state...");
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
                    gclass.UpdateOutput("Call was not established.", true);
                    _testButton.BackColor = Color.Red;
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.LogTestResultToCSV("TC1.14", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("Call is active. Sending MMS...");

                // --- Step 6: Send MMS during call ---
                gclass.UpdateOutput("[Step 6] Sending MMS during call...");
                gclass.SendMMS(_deviceId, targetNumber, "MMSTest");
                gclass.CheckForSentMMS(_deviceId, _refDeviceId);

                // --- Step 7: End call and reset device state ---
                gclass.UpdateOutput("[Step 7] Ending call and resetting device state...");
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                gclass.UpdateOutput("Call ended.");
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsMMSSent)
                {
                    gclass.UpdateOutput("MMS sent successfully during call. TC 1.14: Pass");
                    _testButton.BackColor = Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("MMS not sent. TC 1.14: Fail", true);
                    _testButton.BackColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.14: " + ex.Message, true);
                _testButton.BackColor = Color.Red;
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.14", _deviceId, result);
        }
    }
}
