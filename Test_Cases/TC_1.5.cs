using FIT_Automation.Scripts;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FIT_Automation.Test_Cases
{
    public class TC_1_5
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private bool isICS = false; // Flag to check if ICS or HD Voice is active
        private string _targetNumber = "2069726966"; // <-- Replace with destination VoLTE test number
        private string result;

        public TC_1_5(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("Starting TC 1.5: Verify MO VoLTE to ICS WB AMR capable ue...");

            try
            {
                // Step 1: Check if the device is connected
                if (!gclass.IsDeviceConnected())
                {
                    gclass.UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Enable airplane mode
                gclass.SetAirplaneMode(true);
                gclass.UpdateOutput("Airplane mode enabled.");

                // Step 3: Disable airplane mode
                gclass.SetAirplaneMode(false);
                gclass.UpdateOutput("Airplane mode disabled.");

                // Step 4: Wait for LTE and VoLTE registration
                if (gclass.WaitForLTEAndVoLTERegistration())
                    gclass.UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
                else
                    gclass.UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);

                Thread.Sleep(5000); // Wait for 5 seconds to ensure the device is registered

                // Step 5: Start VoLTE call
                gclass.RunAdbCommand($"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{_targetNumber}");
                gclass.UpdateOutput($"Call initiated to {_targetNumber}.");

                // Step 6: Give 5 seconds to respond to the call
                Thread.Sleep(5000);

                // Step 7:Maintain call for 1 minute
                //Thread.Sleep(60000); // 60 seconds
               // gclass.UpdateOutput("Call maintained for 60 seconds.");
                bool callStillActive = true;
                int duration = 60; // seconds

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();

                    //string audioOutput = gclass.RunAdbCommand($"adb -s {_deviceId} logcat -b main -v threadtime -d").ToLower();

                    // Check if call is still ongoing
                    if (!output.Contains("callstate=2")) //|| audioOutput.Contains("calllogqueryhandler.fetchvoicemailstatus - fetching voicemail status")) // 2 = CALL_STATE_OFFHOOK 
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.5: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }

                    if (isICS && i <= 58)
                    {
                        // Grab latest log output
                        string omyOutput = gclass.RunAdbCommand("adb logcat -b radio -v threadtime -d").ToLower();
                        gclass.UpdateOutput("OMY Raw Output: " + omyOutput.Substring(0, Math.Min(500, omyOutput.Length)));

                        // Check for presence of OM=Y
                        if (Regex.IsMatch(omyOutput, @"rilhdvoicestatus.*om=y", RegexOptions.IgnoreCase))
                        {
                            gclass.UpdateOutput("OM=Y detected – ICS or HD Voice active.");
                            isICS = true;
                        }
                        else
                        {
                            gclass.UpdateOutput("OM=Y not detected.");
                        }
                    }


                    Thread.Sleep(1000); // Check every second
                }

                /*
                // Step B: Check OM=Y once after 60 seconds
                if (!isICS)
                {
                    string omyOutput = gclass.RunAdbCommand("adb logcat -b radio -v threadtime -d").ToLower();
                    gclass.UpdateOutput(omyOutput);
                    if (omyOutput.Contains("om=y"))
                    {
                        gclass.UpdateOutput("OM=Y detected — ICS or HD Voice active.");
                        isICS = true;
                    }
                    else
                    {
                        gclass.UpdateOutput("OM=Y not detected.");
                    }
                }
                */

                // Step 8: End call
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    if (isICS)
                    {
                        gclass.UpdateOutput("Call ended. TC 1.5: Pass");
                        _testButton.BackColor = System.Drawing.Color.Green;
                        result = "PASS";
                    }
                    else
                    {
                        gclass.UpdateOutput("Call ended. TC 1.5: Fail - OM=Y not detected");
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                    }
                }

            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.5: Fail - {ex.Message}", true);
            }

            gclass.LogTestResultToCSV("TC1.5", _deviceId, result);
        }

    }
}


