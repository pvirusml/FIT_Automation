/*
 * TC_1_4: VoLTE to VoLTE MO Call Test Case
 * -----------------------------------------
 * Purpose:
 *   Verify that a Mobile Originated (MO) VoLTE call can be established and maintained for 60 seconds
 *   from a DUT (Device Under Test) to a REF (Reference Device), both registered on LTE/VoLTE.
 *   Ensures call setup, answer, maintenance, and teardown, with proper device state reset.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then OFF for both devices.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Extract REF phone number.
 *   5. Place and answer the call.
 *   6. Maintain call for 60 seconds.
 *   7. End call and cleanup.
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
    public class TC_1_4
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private string _refDeviceId;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_4(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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
                gclass.UpdateOutput("Starting TC 1.4: Verify MO VoLTE call to another VoLTE device...");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }

            try
            {
                string moDevice = _deviceId;
                string refDevice = _refDeviceId;

                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT & REF are not connected. [{_deviceId}, {_refDeviceId}]");

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                Thread.Sleep(5000);

                if (!gclass.WaitForLTEAndVoLTERegistration(_deviceId) || !gclass.WaitForLTEAndVoLTERegistration(_refDeviceId))
                {
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 1.4: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    gclass.LogTestResultToCSV("TC1.4", _deviceId, result);
                    return;
                }

                string refPhoneNumber = gclass.ExtractPhoneNumber(refDevice);
                if (string.IsNullOrWhiteSpace(refPhoneNumber))
                    throw new Exception($"Failed to extract phone number from REF device [{_refDeviceId}]");

                gclass.RunAdbCommand($"adb -s {moDevice} shell am start -a android.intent.action.CALL -d tel:{refPhoneNumber}");
                Thread.Sleep(5000);

                gclass.RunAdbCommand($"adb -s {refDevice} shell input keyevent KEYCODE_CALL");

                bool callStillActive = true;
                int duration = 60;

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {moDevice} shell dumpsys telephony.registry").ToLower();

                    if (!output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.4: FAIL [{_deviceId}, {_refDeviceId}] - Call dropped early at {i} seconds.", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    Thread.Sleep(1000);
                }

                if (callStillActive)
                {
                    gclass.RunAdbCommand($"adb -s {moDevice} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput($"TC 1.4: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.4: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            // Log footer ONCE
            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.4", _deviceId, result);
        }
    }
}


