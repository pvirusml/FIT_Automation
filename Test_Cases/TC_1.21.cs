/*
 * TC_1_21: VoLTE to VoWiFi MO video call Test Case
 * -----------------------------------------
 * Purpose:
 *   Verify that a Mobile Originated (MO) VoLTE video call can be established and maintained for 60 seconds
 *   from a DUT (Device Under Test) to a REF (Reference Device), both registered on LTE/VoLTE and on VoWiFi enabled.
 *   Ensures video call setup, answer, maintenance, and teardown, with proper device state reset.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then OFF for both devices.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Enable WiFi for REF device.
 *   5. Extract REF phone number.
 *   6. Place and answer the video call.
 *   7. Maintain video call for 60 seconds.
 *   8. End video call and cleanup.
 */

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
    public class TC_1_21
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private string _refDeviceId;

        public TC_1_21(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            // ====== TC 1.21: VoLTE to VoLTE MO video call Test ======
            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.21: Verify MO VoLTE video video call to a VoWiFi device...");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                string moDevice = _deviceId;
                string refDevice = _refDeviceId;

                // --- Step 1: Check device connections ---
                gclass.UpdateOutput("[Step 1] Checking device connections...");
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF are not connected.", true);
                    throw new Exception("DUT & REF are not connected.");
                }

                // --- Step 2: Set Airplane mode ON, then OFF for both devices ---
                gclass.UpdateOutput("[Step 2] Cycling Airplane mode for DUT & REF...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.UpdateOutput("Airplane mode enabled for DUT & REF.");
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                gclass.UpdateOutput("Airplane mode disabled for DUT & REF.");
                Thread.Sleep(5000);

                // --- Step 3: Wait for LTE/VoLTE registration ---
                gclass.UpdateOutput("[Step 3] Waiting for LTE/VoLTE registration...");
                if (!gclass.WaitForLTEAndVoLTERegistration(_deviceId) || !gclass.WaitForLTEAndVoLTERegistration(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF failed to attach to LTE or register for VoLTE.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.21", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoLTE.");

                // --- Step 4: Enable WiFi for REF ---
                gclass.EnableWiFi(_refDeviceId);
                gclass.UpdateOutput("[Step 4] WiFi enabled for REF.");
                Thread.Sleep(5000);

                // --- Step 5: Extract REF phone number ---
                gclass.UpdateOutput("[Step 5] Extracting REF phone number...");
                string refPhoneNumber = gclass.ExtractPhoneNumber(refDevice);
                gclass.UpdateOutput($"Extracted REF phone number: {refPhoneNumber}");
                if (string.IsNullOrWhiteSpace(refPhoneNumber))
                    throw new Exception("Failed to extract phone number from REF device.");

                // --- Step 6: Place and answer the video call ---
                gclass.UpdateOutput("[Step 6] Placing video call from DUT to REF...");
                gclass.RunAdbCommand($"adb -s {moDevice} shell am start -a android.intent.action.CALL -d tel:{refPhoneNumber}" +
                    $" --ei android.telecom.extra.START_CALL_WITH_VIDEO_STATE 3");

                //gclass.RunAdbCommand($"adb -s {moDevice} shell am start -a android.intent.action.video call -d tel:{refPhoneNumber}");
                Thread.Sleep(5000); // Give REF time to respond

                gclass.UpdateOutput("[Step 7] Answering video call on REF device...");
                gclass.RunAdbCommand($"adb -s {refDevice} shell input keyevent KEYCODE_CALL");

                // --- Step 8: Maintain video call for 60 seconds ---
                gclass.UpdateOutput("[Step 8] Maintaining video call for 60 seconds...");
                bool callStillActive = true;
                int duration = 60;

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {moDevice} shell dumpsys telephony.registry").ToLower();

                    if (!output.Contains("callstate=2")) // 2 = call_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"video call dropped early at {i} seconds. TC 1.21: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    Thread.Sleep(1000); // check every second
                }

                // --- Step 9: End video call and cleanup ---
                if (callStillActive)
                {
                    gclass.UpdateOutput("video call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {moDevice} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("video call ended. TC 1.21: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                gclass.UpdateOutput("[Step 9] Resetting device states...");
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.21: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.21", _deviceId, result);
        }
    }
}


