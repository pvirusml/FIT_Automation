/*
 * TC_1_13: VoWiFi MMS to VoWiFi Device Test Case
 * -----------------------------------------------
 * Purpose:
 *   Verify that an MMS sent from a VoWiFi device (camping on cellular) is received by another VoWiFi device.
 * 
 * Steps:
 *   1. Confirm both devices are connected.
 *   2. Set Airplane mode ON, enable WiFi for both devices.
 *   3. Wait for LTE/VoWiFi registration.
 *   4. Extract REF phone number.
 *   5. Send MMS from DUT to REF.
 *   6. Reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_13
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;

        public TC_1_13(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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
            gclass.UpdateOutput("Starting TC 1.13: Verify MMS from VoWiFi device camping on cellular is sent to Unison (VoWiFi) device...");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Confirm both devices are connected ---
                gclass.UpdateOutput("[Step 1] Checking device connections...");
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF are not connected.", true);
                    throw new Exception("DUT & REF are not connected.");
                }

                // --- Step 2: Set Airplane mode ON, enable WiFi for both devices ---
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
                    gclass.LogTestResultToCSV("TC1.13", _deviceId, result);
                    return;
                }
                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoWiFi.");

                // --- Step 4: Extract REF phone number ---
                gclass.UpdateOutput("[Step 4] Extracting REF phone number...");
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.UpdateOutput($"Extracted REF phone number: {targetNumber}");
                if (string.IsNullOrWhiteSpace(targetNumber))
                    throw new Exception("Failed to extract phone number from REF device.");

                // --- Step 5: Send MMS from DUT to REF ---
                gclass.UpdateOutput("[Step 5] Sending MMS from DUT to REF...");
                string msg = "MMSTEST";
                gclass.SendMMS(_deviceId, targetNumber, msg);
                gclass.CheckForSentMMS(_deviceId, _refDeviceId);

                // --- Step 6: Reset device states ---
                gclass.UpdateOutput("[Step 6] Resetting device states...");
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsMMSSent)
                {
                    gclass.UpdateOutput("MMS successfully sent. TC 1.13: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("MMS not sent. TC 1.13: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.13: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.13", _deviceId, result);
        }
    }
}
