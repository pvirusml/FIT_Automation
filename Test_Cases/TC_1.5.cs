/*
 * TC_1_5: VoLTE to ICS/HD Voice Call Test Case
 * ---------------------------------------------
 * Purpose:
 *   Verify that a Mobile Originated (MO) VoLTE call to an ICS or HD Voice capable device is established,
 *   maintained for 60 seconds, and OM=Y (HD Voice) is detected in logs.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then OFF for both devices.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Extract REF phone number.
 *   5. Place and answer the call.
 *   6. Maintain call for 60 seconds.
 *   7. Check for OM=Y in logs.
 *   8. End call and cleanup.
 */

using FIT_Automation.Scripts;
using NLog;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_5
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private bool isICS = false;
        private string result;
        private string _refDeviceId;

        public TC_1_5(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.5: Verify MO VoLTE to ICS WB AMR capable ue...");
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
                    gclass.LogTestResultToCSV("TC1.5", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoLTE.");

                // --- Step 4: Extract REF phone number ---
                gclass.UpdateOutput("[Step 4] Extracting REF phone number...");
                string refPhoneNumber = gclass.ExtractPhoneNumber(refDevice);
                gclass.UpdateOutput($"Extracted REF phone number: {refPhoneNumber}");
                if (string.IsNullOrWhiteSpace(refPhoneNumber))
                    throw new Exception("Failed to extract phone number from REF device.");

                // --- Step 5: Place and answer the call ---
                gclass.UpdateOutput("[Step 5] Placing call from DUT to REF...");
                gclass.RunAdbCommand($"adb -s {moDevice} shell am start -a android.intent.action.CALL -d tel:{refPhoneNumber}");
                Thread.Sleep(5000);

                gclass.UpdateOutput("[Step 6] Answering call on REF device...");
                gclass.RunAdbCommand($"adb -s {refDevice} shell input keyevent KEYCODE_CALL");

                // --- Step 6: Maintain call for 60 seconds ---
                gclass.UpdateOutput("[Step 7] Maintaining call for 60 seconds...");
                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {moDevice} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.5: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                // --- Step 7: Check for OM=Y in logs ---
                if (isICS)
                {
                    string omyOutput = gclass.RunAdbCommand("adb logcat -b radio -v threadtime -d").ToLower();
                    gclass.UpdateOutput("OMY Raw Output: " + omyOutput.Substring(0, Math.Min(500, omyOutput.Length)));
                    if (Regex.IsMatch(omyOutput, @"rilhdvoicestatus.*om=y", RegexOptions.IgnoreCase))
                    {
                        gclass.UpdateOutput("OM=Y detected – ICS or HD Voice active.");
                        isICS = true;
                    }
                    else
                    {
                        gclass.UpdateOutput("OM=Y not detected.");
                    }
                }

                // --- Step 8: End call and cleanup ---
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    if (isICS)
                    {
                        gclass.UpdateOutput("Call ended. TC 1.5: Pass");
                        _testButton.BackColor = System.Drawing.Color.Green;
                        result = "PASS";
                    }
                    else
                    {
                        gclass.UpdateOutput("Call ended. TC 1.5: Fail - OM=Y not detected");
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                    }
                }

                gclass.UpdateOutput("[Step 9] Resetting device states...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.5: Fail - {ex.Message}", true);
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.5", _deviceId, result);
        }
    }
}


