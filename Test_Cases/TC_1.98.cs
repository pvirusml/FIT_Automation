/*
 * TC_1_98: VoWiFi MMS to VoWiFi Device Test Case
 * -----------------------------------------------
 * Purpose:
 *   Verify that an MMS sent from a VoLTE device (camping on cellular) to VoWIfi device.
 * 
 * Steps:
 *   1. Confirm both devices are connected.
 *   2. Set Airplane mode ON, enable WiFi for both devices.
 *   3. Extract DUT phone number.
 *   4. Send MMS from DUT to REF.
 *   5. Reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_98
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();


        public TC_1_98(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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

            lock (_lockObject)
            {
                // Ensure thread-safe logging of header
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.98: Verify MMS from VoLTE device camping on cellular is sent to VoWiFi device...");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Confirm both devices are connected
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT & REF are not connected. [{_deviceId}, {_refDeviceId}]");

                // Step 2: Set Airplane mode ON, enable WiFi for both devices
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                gclass.EnableWiFi(_deviceId);
                Thread.Sleep(11000);

                
                // Step 3: Wait for LTE/VoWiFi registration
                bool dutRegistered = gclass.WaitForLTEAndVoLTERegistration(_refDeviceId);
                if (!dutRegistered)
                {
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 1.98: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    gclass.LogTestResultToCSV("TC1.98", _deviceId, result);
                    return;
                }
                

                // Step 4: Extract DUT phone number
                string targetNumber = gclass.ExtractPhoneNumber(_deviceId);
                if (string.IsNullOrWhiteSpace(targetNumber))
                    throw new Exception($"Failed to extract phone number from DUT device [{_deviceId}]");

                // Step 5: Send MMS from REF to DUT
                string msg = "MMSTEST";
                gclass.SendMMS(_refDeviceId, targetNumber, msg);
                gclass.CheckForSentMMS(_refDeviceId, _deviceId);



                if (gclass.IsMMSSent)
                {
                    gclass.UpdateOutput($"TC 1.98: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.98: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.98: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Step 6: Reset device states
                gclass.resetAll(_deviceId);
                gclass.resetAll(_refDeviceId);
            }

            // Log footer ONCE
            //gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.98", _deviceId, result);
        }
    }
}
