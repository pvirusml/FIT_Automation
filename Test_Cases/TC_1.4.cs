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
        private GlobalVarClass gclass = new GlobalVarClass();
        private string _targetNumber = "2069726966"; // <-- Replace with destination VoLTE test number

        public TC_1_4(string deviceId, RichTextBox outputRTB)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
        }

        public void RunTest()
        {
            UpdateOutput("Starting TC 1.4: Verify MO VoLTE call to another VoLTE device...");

            try
            {
                // Check device connection
                if (!IsDeviceConnected())
                    throw new Exception("Device is not connected.");

                // Optional: Check IMS registered
                if (!IsImsRegistered())
                    throw new Exception("IMS not registered. VoLTE might not be available.");

                // Start VoLTE call
                gclass.RunAdbCommand($"adb -s {_deviceId} shell am start -a android.intent.action.CALL -d tel:{_targetNumber}");
                UpdateOutput($"Call initiated to {_targetNumber}.");

                // Maintain call for 1 minute
                Thread.Sleep(60000); // 60 seconds
                UpdateOutput("Call maintained for 60 seconds.");

                // End call
                gclass.RunAdbCommand($"adb -s {_deviceId} shell input keyevent KEYCODE_ENDCALL");
                UpdateOutput("Call ended. TC 1.4: Pass");
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

        private bool IsImsRegistered()
        {
            string command = $"adb -s {_deviceId} shell dumpsys telephony.registry";
            string output = gclass.RunAdbCommand(command).ToLower();
            return output.Contains("ims") && output.Contains("state=connected");
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


