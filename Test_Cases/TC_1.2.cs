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
    public class TC_1_2
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        GlobalVarClass gclass = new GlobalVarClass();

        public TC_1_2(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
        }

        public void RunTest()
        {
            UpdateOutput("Starting TC 1.2: Trigger IMS registration by powering up the device or toggling Airplane Mode while on LTE");

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

                Thread.Sleep(5000); // Wait for 5 seconds to ensure airplane mode is applied

                // Step 3: Disable airplane mode
                SetAirplaneMode(false);
                UpdateOutput("Airplane mode disabled.");

                if (WaitForIMSRegisteration())
                {
                    UpdateOutput("TC 1.2: Pass - IMS registration successful.\n\n");
                    _testButton.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    UpdateOutput("TC 1.2: Fail - IMS registration failed.\n\n", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    throw new Exception("IMS registration failed");
                }

            }
            catch (Exception ex)
            {
                UpdateOutput($"TC 1.2 failed: {ex.Message}", true);
                UpdateOutput("TC 1.2: Fail");
            }
        }

        private bool IsDeviceConnected()
        {
            string output = gclass.RunAdbCommand("adb devices");
            return output.Contains(_deviceId);
        }

        private void SetAirplaneMode(bool enable)
        {
            string state = enable ? "1" : "0";
            gclass.RunAdbCommand($"adb shell settings put global airplane_mode_on {state}");
            gclass.RunAdbCommand("adb shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
        }

        private bool WaitForIMSRegisteration()
        {
            int maxAttempts = 10;
            int attempt = 0;

            while(attempt < maxAttempts)
            {
                string output = gclass.RunAdbCommand("adb shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = gclass.RunAdbCommand("adb shell getprop gsm.network.type").ToLower();
                UpdateOutput("Current RAT: " + ratOutput);

                bool onLte = ratOutput.Contains("lte");
                bool voiceReady = lowerOutput.Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                bool dataAttached = lowerOutput.Contains("mdataregstate=0"); // 0 means data attached
                bool radioIsLte = lowerOutput.Contains("getrilvoiceradiotechnology=14"); // 14 means LTE

                if (!onLte && !voiceReady && !dataAttached && !radioIsLte)
                    return false;

                if (onLte && lowerOutput.Contains("apnsetting") && lowerOutput.Contains("ims") && lowerOutput.Contains("state: connected"))
                    return true;
              
                UpdateOutput($"Waiting for IMS registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 5 seconds before retrying
                attempt++;
            }

            return false;
        }

        //FUNCTION CALLS
        private void UpdateOutput(string message, bool isError = false)
        {
            if (_outputRTB.InvokeRequired)
            {
                _outputRTB.Invoke(new Action(() => UpdateOutput(message, isError)));
            }
            else
            {
                _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
                if (isError)
                {
                    _outputRTB.SelectionColor = System.Drawing.Color.Red;
                }
                else
                {
                    _outputRTB.SelectionColor = System.Drawing.Color.Black;
                }
                
                _outputRTB.ScrollToCaret(); // Auto-scroll to the latest message
            }
        }
    }

}
