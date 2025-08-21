/*
 * TC_1_11: VoWiFi SMS to VoWiFi Device Test Case
 * -----------------------------------------------
 * Purpose:
 *   Verify that an SMS from a VoWiFi device camping on cellular is sent to a Unison (VoWiFi) device.
 * 
 * Steps:
 *   1. Check device connections.
 *   2. Set Airplane mode ON, then enable WiFi for both devices.
 *   3. Wait for LTE/VoWiFi registration.
 *   4. Send SMS from DUT to REF.
 *   5. Reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_11
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;

        public TC_1_11(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.11: Verify SMS from VoWiFi device camping on cellular is sent to Unison (VoWiFi) device");
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

                // --- Step 2: Set Airplane mode ON, then enable WiFi for both devices ---
                gclass.UpdateOutput("[Step 2] Setting Airplane mode ON and enabling WiFi for DUT & REF...");
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.UpdateOutput("Airplane mode disabled for DUT and WiFi enabled for DUT & REF.");
                Thread.Sleep(5000);

                // --- Step 3: Wait for LTE/VoWiFi registration ---
                gclass.UpdateOutput("[Step 3] Waiting for LTE/VoWiFi registration...");
                bool dutRegistered = gclass.WaitForLTEAndVoLTERegistration(_deviceId);
                if (!dutRegistered)
                {
                    gclass.UpdateOutput("DUT failed to attach to LTE or register for VoWiFi.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.11", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoWiFi.");

                // --- Step 4: Send SMS from DUT to REF ---
                gclass.UpdateOutput("[Step 4] Sending SMS from DUT to REF...");
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForSentSMS(_deviceId, _refDeviceId);

                // --- Step 5: Reset device state ---
                gclass.UpdateOutput("[Step 5] Resetting device state...");
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsSMSSent)
                {
                    gclass.UpdateOutput("SMS successfully sent. TC 1.11: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("SMS not sent. TC 1.11: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.11: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.11", _deviceId, result);
        }
    }
}
