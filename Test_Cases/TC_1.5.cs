using FIT_Automation.Scripts;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private string _targetNumber = "2069726966"; // <-- Replace with destination VoLTE test number

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
                gclass.UpdateOutput("Call maintained for 60 seconds.");
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
                        break;
                    }


                    Thread.Sleep(1000); // Check every second
                }

                // Step 8: End call
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.5: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                }

            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.5: Fail - {ex.Message}", true);
            }
        }

    }
}


