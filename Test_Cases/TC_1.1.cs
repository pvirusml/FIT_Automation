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
    public class TC_1_1
    {
        private string _deviceId;
        private RichTextBox _outputRTB;
        GlobalVarClass gclass = new GlobalVarClass();

        public TC_1_1(string deviceId, RichTextBox outputRTB)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
        }

        public void RunTest()
        {
            UpdateOutput("Starting TC 1.1: Verify UE LTE/VoLTE attach");

            try
            {
                // Step 1: Check if the device is connected
                if (!IsDeviceConnected())
                {
                    UpdateOutput("Device is not connected.", true);
                    throw new Exception("Device is not connected.");
                }

                // Step 2: Enable airplane mode
                SetAirplaneMode(false);
                UpdateOutput("Airplane mode disabled.");

                // Step 3: Verify APN is set
                if (!IsAPNSet())
                {
                    UpdateOutput("APN is not set correctly.", true);
                    throw new Exception("APN is not set correctly.");
                }

                // Step 4: Disable airplane mode
                SetAirplaneMode(true);
                UpdateOutput("Airplane mode enabled.");

                // Step 5: Wait for LTE and VoLTE registration
                if (WaitForLTEAndVoLTERegistration())
                {
                    UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
                    UpdateOutput("TC 1.1: Pass");
                }
                else
                {
                    UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);
                    UpdateOutput("TC 1.1: Fail");
                }
            }
            catch (Exception ex)
            {
                UpdateOutput($"Test case failed: {ex.Message}", true);
                UpdateOutput("TC 1.1: Fail");
            }
        }

        private bool IsDeviceConnected()
        {
            string output = gclass.RunAdbCommand("devices");
            return output.Contains(_deviceId);
        }

        private void SetAirplaneMode(bool enable)
        {
            string state = enable ? "1" : "0";
            gclass.RunAdbCommand($"shell settings put global airplane_mode_on {state}");
            gclass.RunAdbCommand("shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
        }

        private bool IsAPNSet()
        {
            string output = gclass.RunAdbCommand("shell content query --uri content://telephony/carriers/preferapn");
            return output.Contains("apn");
        }

        private bool WaitForLTEAndVoLTERegistration()
        {
            int maxAttempts = 10;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                string output = gclass.RunAdbCommand("shell dumpsys telephony.registry");
                if (output.Contains("LTE") && output.Contains("VoLTE") && output.Contains("registered"))
                {
                    return true;
                }

                UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(5000); // Wait for 5 seconds before retrying
                attempt++;
            }

            return false;
        }


        //private string RunADBCommand(string arguments)
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo
        //    {
        //        FileName = "adb",
        //        Arguments = $"-s {_deviceId} {arguments}",
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using (Process process = Process.Start(psi))
        //    {
        //        using (System.IO.StreamReader reader = process.StandardOutput)
        //        {
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}

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

    //PREVIOUS SAMEPLE CODE
    //public class TC_1_1
    //{
    //    private string _deviceId;
    //    private RichTextBox _outputRTB;

    //    public TC_1_1(string deviceId, RichTextBox outputRTB)
    //    {
    //        _deviceId = deviceId;
    //        _outputRTB = outputRTB;
    //    }

    //    public void RunTest()
    //    {
    //        UpdateOutput("Starting TC 1.1: Verify UE LTE/VoLTE attach");

    //        try
    //        {
    //            // Step 1: Check if the device is connected
    //            if (!IsDeviceConnected())
    //            {
    //                UpdateOutput("Device is not connected.", true);
    //                throw new Exception("Device is not connected.");
    //            }

    //            // Step 2: Enable airplane mode
    //            SetAirplaneMode(true);
    //            UpdateOutput("Airplane mode enabled.");

    //            // Step 3: Verify APN is set
    //            if (!IsAPNSet())
    //            {
    //                UpdateOutput("APN is not set correctly.", true);
    //                throw new Exception("APN is not set correctly.");
    //            }

    //            // Step 4: Disable airplane mode
    //            SetAirplaneMode(false);
    //            UpdateOutput("Airplane mode disabled.");

    //            // Step 5: Wait for LTE and VoLTE registration
    //            if (WaitForLTEAndVoLTERegistration())
    //            {
    //                UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");
    //                UpdateOutput("TC 1.1: Pass");
    //            }
    //            else
    //            {
    //                UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);
    //                UpdateOutput("TC 1.1: Fail");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            UpdateOutput($"Test case failed: {ex.Message}", true);
    //            UpdateOutput("TC 1.1: Fail");
    //        }
    //    }

    //    private bool IsDeviceConnected()
    //    {
    //        string output = RunADBCommand("devices");
    //        return output.Contains(_deviceId);
    //    }

    //    private void SetAirplaneMode(bool enable)
    //    {
    //        string state = enable ? "1" : "0";
    //        RunADBCommand($"shell settings put global airplane_mode_on {state}");
    //        RunADBCommand("shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
    //    }

    //    private bool IsAPNSet()
    //    {
    //        string output = RunADBCommand("shell content query --uri content://telephony/carriers/preferapn");
    //        return output.Contains("apn");
    //    }

    //    private bool WaitForLTEAndVoLTERegistration()
    //    {
    //        int maxAttempts = 10;
    //        int attempt = 0;

    //        while (attempt < maxAttempts)
    //        {
    //            string output = RunADBCommand("shell dumpsys telephony.registry");
    //            if (output.Contains("LTE") && output.Contains("VoLTE") && output.Contains("registered"))
    //            {
    //                return true;
    //            }

    //            UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
    //            Thread.Sleep(5000); // Wait for 5 seconds before retrying
    //            attempt++;
    //        }

    //        return false;
    //    }

    //    private string RunADBCommand(string arguments)
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = "adb",
    //            Arguments = $"-s {_deviceId} {arguments}",
    //            RedirectStandardOutput = true,
    //            UseShellExecute = false,
    //            CreateNoWindow = true
    //        };

    //        using (Process process = Process.Start(psi))
    //        {
    //            using (System.IO.StreamReader reader = process.StandardOutput)
    //            {
    //                return reader.ReadToEnd();
    //            }
    //        }
    //    }

    //    private void UpdateOutput(string message, bool isError = false)
    //    {
    //        if (_outputRTB.InvokeRequired)
    //        {
    //            _outputRTB.Invoke(new Action(() => UpdateOutput(message, isError)));
    //        }
    //        else
    //        {
    //            _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
    //            if (isError)
    //            {
    //                _outputRTB.SelectionColor = System.Drawing.Color.Red;
    //            }
    //            else
    //            {
    //                _outputRTB.SelectionColor = System.Drawing.Color.Black;
    //            }
    //            _outputRTB.ScrollToCaret(); // Auto-scroll to the latest message
    //        }
    //    }
    //}

}
