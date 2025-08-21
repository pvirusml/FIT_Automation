/*
 * TC_1_7: MT SMS Reception on ICS Test Case
 * -----------------------------------------
 * Purpose:
 *   Verify that a Mobile Terminated (MT) SMS (on ICS) from another VoLTE device is received using SIP over IMS.
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
    public class TC_1_7
    {
        private string _deviceId;
        private string _targetNumber;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;

        public TC_1_7(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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
            gclass.UpdateOutput("Starting TC 1.7: Verify MT SMS (on ICS) from another VoLTE device is received using SIP over IMS");
            gclass.UpdateOutput("==================================================\n");

            try
            {
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
                    gclass.LogTestResultToCSV("TC1.7", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoLTE.");

                // --- Step 4: Send SMS from REF to DUT ---
                gclass.UpdateOutput("[Step 4] Sending SMS from REF to DUT...");
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForReceivedSMS(_deviceId, _refDeviceId);

                // --- Step 5: Reset device state ---
                gclass.UpdateOutput("[Step 5] Resetting device state...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsSMSReceived)
                {
                    gclass.UpdateOutput("SMS successfully received. TC 1.7: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("SMS not received. TC 1.7: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.7: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.7", _deviceId, result);
        }
    }
}
