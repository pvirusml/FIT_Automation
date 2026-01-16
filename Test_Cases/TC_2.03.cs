/*
 * TC_2_03: VoWiFi SMS to VoLTE Device Test Case
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
    public class TC_2_03
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();


        public TC_2_03(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
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
                // Log header ONCE (not per device pair)
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 2.03: Verify SMS from VoWiFi device camping on cellular is sent to voLTE device");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                    throw new Exception($"DUT & REF are not connected. [{_deviceId}, {_refDeviceId}]");

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                gclass.EnableWiFi(_deviceId);
                Thread.Sleep(5000);


                bool dutRegistered = gclass.WaitForLTEAndVoLTERegistration(_refDeviceId);
                if (!dutRegistered)
                {
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 2.03: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    gclass.LogTestResultToCSV("TC2.03", _deviceId, result);
                    return;
                }

                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForSentSMS(_deviceId, _refDeviceId);

                if (gclass.IsSMSSent)
                {
                    gclass.UpdateOutput($"TC 2.03: PASS [{_deviceId}, {_refDeviceId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 2.03: FAIL [{_deviceId}, {_refDeviceId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 2.03: FAIL [{_deviceId}, {_refDeviceId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Reset device states in case of failure
                gclass.resetAll(_deviceId);
                gclass.resetAll(_refDeviceId);
            }

            // Log footer ONCE
            //gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC2.03", _deviceId, result);
        }
    }
}
