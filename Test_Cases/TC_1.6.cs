/*
 * TC_1_6: MT SMS Reception over IMS Test Case
 * -------------------------------------------
 * Purpose:
 *   Verify that a Mobile Terminated (MT) SMS from another VoLTE device is received using SIP over IMS.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then OFF for both devices.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Send SMS from REF to DUT.
 *   5. Check for SMS reception.
 *   6. Reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_6
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_6(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
            _refDeviceId = refDeviceId;
        }

        public void RunTest()
        {
            result = "FAIL";

            // Log header ONCE (not per device pair)
            if (!headerLogged)
            {
                gclass.UpdateOutput("==================================================");
                gclass.UpdateOutput("Starting TC 1.6: Verify MT SMS from another VoLTE device is received using SIP over IMS");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }

            try
            {
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
                    gclass.UpdateOutput($"TC 1.6: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    gclass.LogTestResultToCSV("TC1.6", _deviceId, result);
                    return;
                }

                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForReceivedSMS(_deviceId, _refDeviceId);

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsSMSReceived)
                {
                    gclass.UpdateOutput($"TC 1.6: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.6: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.6: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            // Log footer ONCE
            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.6", _deviceId, result);
        }
    }
}
