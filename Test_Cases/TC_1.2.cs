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
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static bool footerLogged = false;
        private static readonly object _lockObject = new object();

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

            lock (_lockObject)
            {
                // Log header ONCE (not per device)
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.2: Trigger IMS registration by powering up the device or toggling Airplane Mode while on LTE");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                if (!gclass.IsDeviceConnected(_deviceId))
                    throw new Exception($"{_deviceId} is not connected.");

                gclass.SetAirplaneMode(_deviceId, true);
                Thread.Sleep(5000);

                gclass.SetAirplaneMode(_deviceId, false);

                if (gclass.WaitForIMSRegisteration(_deviceId))
                {
                    result = "PASS";
                    _testButton.BackColor = System.Drawing.Color.Green;
                    gclass.UpdateOutput($"TC 1.2: {result} [{_deviceId}]");
                }
                else
                {
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 1.2: {result} [{_deviceId}]", true);
                }

                gclass.SetAirplaneMode(_deviceId, true);
            }
            catch (Exception ex)
            {
                result = "FAIL";
                _testButton.BackColor = System.Drawing.Color.Red;
                gclass.UpdateOutput($"TC 1.2: {result} [{_deviceId}] - {ex.Message}", true);
            }

            gclass.LogTestResultToCSV("TC1.2", _deviceId, result);


            // Log footer ONCE
            /*
            lock (_lockObject)
            {
                if (!footerLogged)
                {
                    Thread.Sleep(11000); // Ensure all output is logged before footer
                    gclass.UpdateOutput("\n__________________________________________________\n");
                    footerLogged = true;
                }
            }
            */

        }


    }
}
