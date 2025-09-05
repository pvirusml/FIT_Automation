/*
 * TC_1_19: VoLTE to VoLTE(with data turned off) MT Call Test Case
 * -----------------------------------------
 * Purpose:
 *   Verify that a Mobile Originated (MT) VoLTE call can be established and maintained for 60 seconds
 *   from a DUT (Device Under Test) from a REF (Reference Device), both registered on LTE/VoLTE and REF has data turned off.
 *   Ensures call setup, answer, maintenance, and teardown, with proper device state reset.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then OFF for both devices.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Extract REF phone number.
 *   5. Turn REF data off
 *   6. Place and answer the call.
 *   7. Maintain call for 60 seconds.
 *   8. End call and cleanup.
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
    public class TC_1_19
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private string _refDeviceId;

        public TC_1_19(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            // ====== TC 1.19: VoLTE to VoLTE MO Call Test ======
            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.19: Verify MT VoLTE call from another VoLTE device with data turned off...");
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
                    gclass.LogTestResultToCSV("TC1.19", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoLTE.");

                // --- Step 4: Extract DUT phone number ---
                gclass.UpdateOutput("[Step 4] Extracting REF phone number...");
                string dutPhoneNumber = gclass.ExtractPhoneNumber(_deviceId);
                gclass.UpdateOutput($"Extracted REF phone number: {dutPhoneNumber}");
                if (string.IsNullOrWhiteSpace(dutPhoneNumber))
                    throw new Exception("Failed to extract phone number from REF device.");

                // --- Step 5: Turn REF data off ---
                gclass.UpdateOutput("[Step 5] Turn REF data off...");
                gclass.RunAdbCommand($"adb -s {refDevice} shell svc data disable");
                Thread.Sleep(11000); // Wait for data to turn off

                // --- Step 6: Place and answer the call ---
                gclass.UpdateOutput("[Step 6] Placing call from REF to DUT...");
                gclass.RunAdbCommand($"adb -s {refDevice} shell am start -a android.intent.action.CALL -d tel:{dutPhoneNumber}");
                Thread.Sleep(6000); // Give DUT time to respond

                gclass.UpdateOutput("[Step 7] Answering call on DUT device...");
                gclass.RunAdbCommand($"adb -s {moDevice} shell input keyevent KEYCODE_CALL");

                // --- Step 8: Maintain call for 60 seconds ---
                gclass.UpdateOutput("[Step 8] Maintaining call for 60 seconds...");
                bool callStillActive = true;
                int duration = 60;

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {moDevice} shell dumpsys telephony.registry").ToLower();

                    if (!output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.19: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    Thread.Sleep(1000); // check every second
                }

                // --- Step 9: End call and cleanup ---
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {moDevice} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.19: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                gclass.UpdateOutput("[Step 9] Resetting device states...");
                gclass.RunAdbCommand($"adb -s {refDevice} shell svc data enable"); // Turn REF data on
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.19: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.19", _deviceId, result);
        }
    }
}


