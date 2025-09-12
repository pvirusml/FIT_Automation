/*
 * TC_1_1: LTE/VoLTE Attach Test Case
 * -----------------------------------
 * Purpose:
 *   Verify that the device can attach to LTE and register for VoLTE.
 * 
 * Steps:
 *   1. Check device connection.
 *   2. Enable airplane mode.
 *   3. Verify APN is set.
 *   4. Disable airplane mode.
 *   5. Wait for LTE/VoLTE registration.
 *   6. Reset device state.
 */

using FIT_Automation.Scripts;
using NLog;
using System;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_1
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        GlobalVarClass gclass;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_1(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            string result = "FAIL";

            // Log header ONCE (not per device)
            if (!headerLogged)
            {
                gclass.UpdateOutput("==================================================");
                gclass.UpdateOutput("Starting TC 1.1: Verify UE LTE/VoLTE attach");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }
            try
            {
                if (!gclass.IsDeviceConnected(_deviceId))
                    throw new Exception($"{_deviceId} is not connected.");

                gclass.SetAirplaneMode(_deviceId, true);

                if (!gclass.IsAPNSet(_deviceId))
                    throw new Exception("APN is not set correctly.");

                gclass.SetAirplaneMode(_deviceId, false);

                if (gclass.WaitForLTEAndVoLTERegistration(_deviceId))
                {
                    result = "PASS";
                    _testButton.BackColor = System.Drawing.Color.Green;
                    gclass.UpdateOutput($"TC 1.1: {result} [{_deviceId}]");
                }
                else
                {
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 1.1: {result} [{_deviceId}]", true);
                }

                gclass.SetAirplaneMode(_deviceId, true);
            }
            catch (Exception ex)
            {
                result = "FAIL";
                _testButton.BackColor = System.Drawing.Color.Red;
                gclass.UpdateOutput($"TC 1.1: {result} [{_deviceId}] - {ex.Message}", true);
            }

            // Log footer ONCE
                gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.1", _deviceId, result);
        }
    }
}
