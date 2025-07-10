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
    public class TC_1_4
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass = new GlobalVarClass();
        private string _targetNumber = "2069726966"; // <-- Replace with destination VoLTE test number

        public TC_1_4(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
        }

        public void RunTest()
        {
            UpdateOutput("Starting TC 1.4: Verify MO VoLTE call to another VoLTE device...");

            try
            {
                // Step 1: Check if the device is connected
                if (!IsDeviceConnected())
                {
                    UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Enable airplane mode
                SetAirplaneMode(true);
                UpdateOutput("Airplane mode enabled.");

                // Step 3: Disable airplane mode
                SetAirplaneMode(false);
                UpdateOutput("Airplane mode disabled.");

                // Step 4: Wait for LTE and VoLTE registration
                if (WaitForLTEAndVoLTERegistration())
                    UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
                else
                    UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);

                Thread.Sleep(5000); // Wait for 5 seconds to ensure the device is registered

                // Step 5: Start VoLTE call
                gclass.RunAdbCommand($"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{_targetNumber}");
                UpdateOutput($"Call initiated to {_targetNumber}.");

                // Step 6: Give 5 seconds to respond to the call
                Thread.Sleep(5000);

                // Step 7:Maintain call for 1 minute
                //Thread.Sleep(60000); // 60 seconds
                //UpdateOutput("Call maintained for 60 seconds.");
                bool callStillActive = true;
                int duration = 60; // seconds

                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_deviceId} shell dumpsys telephony.registry").ToLower();

                    // Check if call is still ongoing
                    if (!output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        UpdateOutput($"Call dropped early at {i} seconds. TC 1.4: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        break;
                    }

                    // CHECK CELL STATE 2
                    // remove for loop

                    Thread.Sleep(1000); // Check every second
                }

                // Step 8: End call
                if (callStillActive)
                {
                    UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                    UpdateOutput("Call ended. TC 1.4: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                }

            }
            catch (Exception ex)
            {
                UpdateOutput($"TC 1.4: Fail - {ex.Message}", true);
            }
        }

        private bool IsDeviceConnected()
        {
            string output = gclass.RunAdbCommand("adb devices");
            return output.Contains(_deviceId);
        }

        private bool WaitForLTEAndVoLTERegistration()
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {

                string output = gclass.RunAdbCommand("adb shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = gclass.RunAdbCommand("adb shell getprop gsm.network.type").ToLower();
                UpdateOutput("Current RAT: " + ratOutput);

                /*
                  * mVoiceRegState=0 indicates VOLTE- ready voice 
                  * mDataRegState=0 indicates data is attached
                  * getRilVoiceRadioTechnology=14 indicates LTE
                */

                bool onLte = ratOutput.Contains("lte");
                bool voiceReady = lowerOutput.Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                bool dataAttached = lowerOutput.Contains("mdataregstate=0"); // 0 means data attached
                bool radioIsLte = lowerOutput.Contains("getrilvoiceradiotechnology=14"); // 14 means LTE

                if (onLte && voiceReady && dataAttached && radioIsLte)
                {
                    return true;
                }



                UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 10 seconds before retrying
                attempt++;
            }

            return false;
        }


        private void SetAirplaneMode(bool enable)
        {
            string state = enable ? "1" : "0";
            gclass.RunAdbCommand($"adb shell settings put global airplane_mode_on {state}");
            gclass.RunAdbCommand("adb shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
        }

        private void UpdateOutput(string message, bool isError = false)
        {
            if (_outputRTB.InvokeRequired)
            {
                _outputRTB.Invoke(new Action(() => UpdateOutput(message, isError)));
            }
            else
            {
                _outputRTB.SelectionColor = isError
                    ? System.Drawing.Color.Red
                    : message.ToLower().Contains("pass") ? System.Drawing.Color.Green : System.Drawing.Color.Black;

                _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
                _outputRTB.ScrollToCaret();
            }
        }
    }
}


