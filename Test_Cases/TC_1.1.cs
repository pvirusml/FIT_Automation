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

            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.1: Verify UE LTE/VoLTE attach");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Check device connection ---
                gclass.UpdateOutput("[Step 1] Checking device connection...");
                if (!gclass.IsDeviceConnected(_deviceId))
                {
                    gclass.UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // --- Step 2: Enable airplane mode ---
                gclass.UpdateOutput("[Step 2] Enabling airplane mode...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.UpdateOutput("Airplane mode enabled.");

                // --- Step 3: Verify APN is set ---
                gclass.UpdateOutput("[Step 3] Verifying APN...");
                if (!gclass.IsAPNSet(_deviceId))
                {
                    gclass.UpdateOutput("APN is not set correctly.", true);
                    throw new Exception("APN is not set correctly.");
                }

                // --- Step 4: Disable airplane mode ---
                gclass.UpdateOutput("[Step 4] Disabling airplane mode...");
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode disabled.");

                // --- Step 5: Wait for LTE/VoLTE registration ---
                gclass.UpdateOutput("[Step 5] Waiting for LTE/VoLTE registration...");
                if (gclass.WaitForLTEAndVoLTERegistration(_deviceId))
                {
                    gclass.UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
                    gclass.UpdateOutput("TC 1.1: Pass\n\n");
                    result = "PASS";
                    _testButton.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    gclass.UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);
                    gclass.UpdateOutput("TC 1.1: Fail\n\n");
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                }

                // --- Step 6: Reset device state ---
                gclass.UpdateOutput("[Step 6] Resetting device state...");
                gclass.SetAirplaneMode(_deviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"Test case failed: {ex.Message}", true);
                gclass.UpdateOutput("TC 1.1: Fail");
                result = "FAIL";
            }

            gclass.UpdateOutput("\n==================================================\n");
            gclass.LogTestResultToCSV("TC1.1", _deviceId, result);
        }
    }
}
