using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

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
            gclass.UpdateOutput("Starting TC 1.11: Verify SMS from VoWiFi device camping on cellular is sent to Unison (VoWiFi) device");

            try
            {
                // Step 1: Confirm both devices are connected
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF are not connected.", true);
                    throw new Exception("DUT & REF are not connected.");
                }

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                // Step 2: Disable Airplane Mode and Enable WiFi on both devices
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.UpdateOutput("Airplane mode disabled for DUT and WiFi enabled for DUT & REF.");
                Thread.Sleep(5000);

                // Step 3: Wait for LTE and VoWiFi registration
                bool dutRegistered = gclass.WaitForLTEAndVoLTERegistration(_deviceId);

                if (!dutRegistered)
                {
                    gclass.UpdateOutput("DUT failed to attach to LTE or register for VoWiFi.", true);
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.11", _deviceId, result);
                    return;
                }

                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoWiFi.");

                // Add a VoWiFi-specific check here 

                // Step 4: Get REF number 
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                if (string.IsNullOrWhiteSpace(targetNumber))
                    throw new Exception("Failed to extract phone number from REF device.");

                // Step 5: Send SMS from DUT to REF
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForSentSMS(_deviceId, _refDeviceId);

                // Step 6: Disable Wifi
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);

                // Put in Airplane mode
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

            gclass.LogTestResultToCSV("TC1.11", _deviceId, result);
        }
    }
}
