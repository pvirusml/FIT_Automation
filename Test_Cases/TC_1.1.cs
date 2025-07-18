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
            string result;
            gclass.UpdateOutput("Starting TC 1.1: Verify UE LTE/VoLTE attach");

            try
            {
                // Step 1: Check if the device is connected
                if (!gclass.IsDeviceConnected())
                {
                    gclass.UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Enable airplane mode
                gclass.SetAirplaneMode(true);
                gclass.UpdateOutput("Airplane mode enabled.");

                // Step 3: Verify APN is set
                if (!gclass.IsAPNSet())
                {
                    gclass.UpdateOutput("APN is not set correctly.", true);
                    throw new Exception("APN is not set correctly.");
                }

                // Step 4: Disable airplane mode
                gclass.SetAirplaneMode(false);
                gclass.UpdateOutput("Airplane mode disabled.");

                // Step 5: Wait for LTE and VoLTE registration
                if (gclass.WaitForLTEAndVoLTERegistration())
                {
                    gclass.UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
                    gclass.UpdateOutput("TC 1.1: Pass\n\n");
                    result = "PASS";
                    _testButton.BackColor = System.Drawing.Color.Green; // Change button color to green on success
                }
                else
                {
                    gclass.UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);
                    gclass.UpdateOutput("TC 1.1: Fail\n\n");
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red; // Change button color to red on failure
                }

                gclass.LogTestResultToCSV("TC1.1", _deviceId, result);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"Test case failed: {ex.Message}", true);
                gclass.UpdateOutput("TC 1.1: Fail");
                result = "FAIL";
            }

            
        }



    }

}
