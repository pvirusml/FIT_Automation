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
    public class TC_1_8
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string _targetNumber = "2069726966"; // Replace with DUT 1 number
        private string _forwardToNumber = "2069726977"; // Replace with DUT 2 number
        private string result;

        public TC_1_8(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("Starting TC 1.8: Verify Call Forwarding Unconditional with XCAP GBA on LTE...");

            try
            {
                // Step 1: Check if device is connected
                if (!gclass.IsAPNSet(_deviceId))
                {
                    gclass.UpdateOutput("APN not set for netsvcs. CFU may fail.", true);
                }

                if (!gclass.IsDeviceConnected(_deviceId))
                {
                    gclass.UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Ensure LTE + IMS
                gclass.SetAirplaneMode(_deviceId, true);
                gclass.UpdateOutput("Airplane mode enabled.");
                Thread.Sleep(3000);
                gclass.SetAirplaneMode(_deviceId, false);
                gclass.UpdateOutput("Airplane mode disabled.");

                if (gclass.WaitForLTEAndVoLTERegistration(_deviceId))
                {
                    gclass.UpdateOutput("IMS successfully registered on LTE.");
                }
                else
                {
                    gclass.UpdateOutput("Failed to register IMS on LTE.", true);
                    throw new Exception("IMS not registered.");
                }

                // Step 3: Set CFU using XCAP/GBA
                gclass.UpdateOutput("Setting Call Forwarding using XCAP/GBA...");
                string ussdCommand = $"*21*{_forwardToNumber}#";
                string setCommand = $"adb shell am start -a android.intent.action.CALL -d tel:{ussdCommand}";
                gclass.RunAdbCommand(setCommand);
                Thread.Sleep(8000);
                gclass.RunAdbCommand("adb shell input keyevent 66"); // Press "Send" key
                gclass.UpdateOutput("CFU command sent.");

                // Step 4: Start MO call from another device (not DUT2) to DUT1
                gclass.UpdateOutput("Place MO call to check forwarding...");
                string callCommand = $"adb shell am start -a android.intent.action.CALL -d tel:{_targetNumber}";
                gclass.RunAdbCommand(callCommand);
                Thread.Sleep(5000); // Let it ring

                // Step 5: Maintain call for 1 minute
                gclass.UpdateOutput("Monitoring call for 60 seconds...");
                bool callStillActive = true;
                int duration = 60;

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();

                    if (output.Contains("callstate=2")) // 2 = OFFHOOK
                    {
                        // Call is ongoing
                    }
                    else
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.8: Fail.", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    Thread.Sleep(1000);
                }

                // Step 6: End call
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.8: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.8: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.8", _deviceId, result);
        }
    }
}