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
    public class TC_1_15
    {
        private string _deviceId;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;

        public TC_1_15(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            _refDeviceId = refDeviceId;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            string result = "FAIL";
            gclass.UpdateOutput("Starting TC 1.15: Verify 1 min VoWiFi call between two VoWifi devices...");

            try
            {
                // 1. Check connections
                if (!gclass.IsDeviceConnected(_deviceId) || !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT or REF not connected.", true);
                    throw new Exception("DUT or REF not connected.");
                }

                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_HOME"); // Ensure home screen is active

                // 2. Set Airplane mode ON, enable WiFi for VoWiFi
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.EnableWiFi(_deviceId);
                gclass.EnableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode ON, WiFi enabled for DUT & REF.");
                Thread.Sleep(11000);



                // 4. Extract REF phone number
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.UpdateOutput($"Extracted REF number: {targetNumber}");
                if (string.IsNullOrWhiteSpace(targetNumber))
                {
                    gclass.UpdateOutput("REF number missing.", true);
                    gclass.LogTestResultToCSV("TC1.15", _deviceId, result);
                    return;
                }

                // 5. Make call from DUT to REF
                gclass.UpdateOutput($"Calling REF ({targetNumber}) from DUT...");
                string callCmd = $"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{targetNumber}";
                gclass.RunAdbCommand(callCmd);
                Thread.Sleep(9000);

                // 6. Answer on REF
                gclass.UpdateOutput("Answering call on REF...");
                gclass.RunAdbCommand($"adb -s {_refDeviceId} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Step 7: Maintain call for 60 seconds or until dropped
                bool callStillActive = true;
                int duration = 60;

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();

                    if (!output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.4: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    Thread.Sleep(1000); // check every second
                }

                // Step 8: End call if still active
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.15: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                // 10. Reset both devices
                gclass.DisableWiFi(_deviceId);
                gclass.DisableWiFi(_refDeviceId);
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.15: " + ex.Message, true);
                _testButton.BackColor = Color.Red;
            }

            gclass.LogTestResultToCSV("TC1.15", _deviceId, result);
        }
    }
}
