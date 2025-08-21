/*
 * TC_1_2: IMS Registration Trigger Test Case
 * -------------------------------------------
 * Purpose:
 *   Verify that IMS registration is triggered by powering up the device or toggling Airplane Mode while on LTE.
 * 
 * Steps:
 *   1. Check device connection.
 *   2. Enable airplane mode.
 *   3. Disable airplane mode.
 *   4. Wait for IMS registration.
 *   5. Reset device state.
 */

using FIT_Automation.Scripts;
using NLog;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_2
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        GlobalVarClass gclass;

        public TC_1_2(string deviceId, RichTextBox outputRTB, Button testButton)
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
            gclass.UpdateOutput("Starting TC 1.2: Trigger IMS registration by powering up the device or toggling Airplane Mode while on LTE");
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
                Thread.Sleep(5000);

                // --- Step 3: Disable airplane mode ---
                gclass.UpdateOutput("[Step 3] Disabling airplane mode...");
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode disabled.");

                // --- Step 4: Wait for IMS registration ---
                gclass.UpdateOutput("[Step 4] Waiting for IMS registration...");
                if (gclass.WaitForIMSRegisteration(_deviceId))
                {
                    gclass.UpdateOutput("TC 1.2: Pass - IMS registration successful.\n\n");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("TC 1.2: Fail - IMS registration failed.\n\n", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    throw new Exception("IMS registration failed");
                }

                // --- Step 5: Reset device state ---
                gclass.UpdateOutput("[Step 5] Resetting device state...");
                gclass.SetAirplaneMode(_deviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.2 failed: {ex.Message}", true);
                gclass.UpdateOutput("TC 1.2: Fail");
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.2", _deviceId, result);
        }
    }
}
