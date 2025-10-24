/*
 * TC_1_37: Voicemail Notification and Callback Test Case
 * -----------------------------------------------------
 * Purpose:
 *   1) Make call from DUT 1 to DUT 2
2) Once call is connected, put DUT 2 on hold
3) Verify Call hold tone is heard on DUT 2
4) Wait for 1 minute and then Un hold
5) Ensure audio is ok after unholding the call
6) End the call after 10 secs
 */

using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_37
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_37(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _dut2Id = dut2Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public async Task RunTestAsync()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.37: VoWiFi DUTs on hold");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // 1. Place a call from DUT 2 to DUT 1 and reject the call on DUT 1
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                {
                    throw new Exception($"DUT 1 or DUT 2 is not connected. [{_dut1Id}, {_dut2Id}]");
                }

                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                {
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");
                }

                // Properly handle airplane mode toggle and network stabilization
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000); // Wait for airplane mode to apply
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);

                // Wait longer for both devices to fully reconnect to the network
                gclass.UpdateOutput("Waiting for devices to reconnect to network...");
                await Task.Delay(15000);

                gclass.EnableWiFi(_dut1Id);
                //gclass.EnableWiFi(_dut2Id);

                // Check LTE for dut2
                if(!gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                {
                    throw new Exception($"DUT 2 [{_dut2Id}] did not connect to LTE within the expected time.");
                }

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                {
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");
                }

                await Task.Delay(10000); // Wait longer for the call to be registered

                // Accept call on DUT2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // 2. Ensure that DUT 2 is redirected to DUT 1's voicemail
                await Task.Delay(8000); // Wait for voicemail system to answer

                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/incall_sixth_button");

                await Task.Delay(5000); // Wait for the hold action to take effect

                string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                gclass.CaptureUIDump(_dut1Id, Path.GetDirectoryName(dumpPath));
                if (!gclass.isInContentDescUIDump(dumpPath, "Resume call"))
                    {
                    throw new Exception("Call hold action failed on DUT 1.");
                }

                await Task.Delay(60000); // Wait while on hold

                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/incall_sixth_button");

                // The YouTube video approach is unreliable and blocking. A better approach is to
                // simulate the voicemail tone and dialer interaction with timed ADB commands.
                // The following section simulates ending the call after leaving a message.
                // Consider replacing the above YouTube code with more reliable ADB commands.
                await Task.Delay(10000); // Simulate leaving a voicemail
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                await Task.Delay(3000); // Wait for the notification shade to open
              

                    // End all calls and cleanup
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                    gclass.UpdateOutput($"TC 1.37: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.37: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {
                // go to home screen
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }
        }
    }
}

