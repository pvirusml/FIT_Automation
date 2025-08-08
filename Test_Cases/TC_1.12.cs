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
    public class TC_1_12
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;

        public TC_1_12(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
            _refDeviceId = refDeviceId;
        }

        public void RunTest()
        {
            string result = "FAIL";
            gclass.UpdateOutput("Starting TC 1.12: Verify SMS from VoWiFi device camping on cellular is sent to Unison (VoWiFi) device");

            try
            {
                // 1. Check both devices are connected
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF are not connected.", true);
                    throw new Exception("DUT & REF are not connected.");
                }

                // 2. Reset both devices to a known state
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                Thread.Sleep(2000);

                // 3. Bring both devices online and enable WiFi
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.UpdateOutput("Airplane mode disabled and WiFi enabled for DUT & REF.");
                Thread.Sleep(5000);

                // 4. Wait for both devices to register (especially REF)
                gclass.UpdateOutput("Waiting for DUT registration...");
                bool dutRegistered = gclass.WaitForLTEAndVoLTERegistration(_deviceId);
                if (!dutRegistered)
                {
                    gclass.UpdateOutput("DUT failed to attach to LTE or register for VoWiFi.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.12", _deviceId, result);
                    return;
                }

                gclass.UpdateOutput("DUT successfully attached to LTE and registered for VoWiFi.");

                // 6. Get REF number
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.UpdateOutput($"Extracted REF phone number: {targetNumber}");
                if (string.IsNullOrWhiteSpace(targetNumber))
                {
                    gclass.UpdateOutput("Failed to extract phone number from REF device.", true);
                    gclass.LogTestResultToCSV("TC1.12", _deviceId, result);
                    return;
                }

                // 7. Place call from DUT to REF
                gclass.UpdateOutput($"Placing call from {_deviceId} to {targetNumber}");
                string callOutput = gclass.RunAdbCommand($"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{targetNumber}");
                gclass.UpdateOutput($"Call command output: {callOutput}");
                Thread.Sleep(9000); // Wait for call to start ringing

                // 8. Answer call on REF device
                gclass.UpdateOutput($"Answering call on REF device {_refDeviceId}");
                string answerOutput = gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(2000);

                // 9. Wait for call to be OFFHOOK (active)
                gclass.UpdateOutput("Waiting for call to be active (OFFHOOK)...");
                bool callActive = false;
                for (int i = 0; i < 10; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();
                    gclass.UpdateOutput($"Call state check {i + 1}: {output}");
                    if (output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callActive = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (!callActive)
                {
                    gclass.UpdateOutput("Call was not established.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.LogTestResultToCSV("TC1.12", _deviceId, result);
                    return;
                }

                gclass.UpdateOutput("Call is active. Sending SMS...");

                // 10. Send SMS from DUT to REF while call is active
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForSentSMS(_deviceId, _refDeviceId);

                // 11. End call after SMS is sent/checked
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                gclass.UpdateOutput("Call ended.");

                // 12. Disable Wifi and set Airplane mode
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);

                if (gclass.IsSMSSent)
                {
                    gclass.UpdateOutput("SMS successfully sent. TC 1.12: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("SMS not sent. TC 1.12: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.12: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
            }

            gclass.LogTestResultToCSV("TC1.12", _deviceId, result);
        }
    }
}
