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
            string result;
            gclass.UpdateOutput("Starting TC 1.2: Trigger IMS registration by powering up the device or toggling Airplane Mode while on LTE");

            try
            {
                // Step 1: Check if the device is connected
                if (!gclass.IsDeviceConnected(_deviceId))
                {
                    gclass.UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Enable airplane mode
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.UpdateOutput("Airplane mode enabled.");

                Thread.Sleep(5000); // Wait for 5 seconds to ensure airplane mode is applied

                // Step 3: Disable airplane mode
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode disabled.");

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

            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.2 failed: {ex.Message}", true);
                gclass.UpdateOutput("TC 1.2: Fail");
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.2", _deviceId, result);
        }

    }

}
