/*
 * TC_1_48: Starting TC 1.48: Recieve call from Unknown Caller
 * -----------------------------------------------------
1) Turn off "Show my caller ID" on DUT 2
2) Place a call from DUT 2 to DUT 1
3) Called ID should be shown as "Unknown Caller"
4) Accept the call, check audio is ok
5) Maintain the call for 1min and end the call
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
    public class TC_1_48
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_48(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.48: Recieve call from Unknown Caller");
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

                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                {
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut2Id}]");
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
                gclass.EnableWiFi(_dut2Id);

                Thread.Sleep(15000); // Wait for network stabilization

                // Make call from unkown origin
                //Note: %23 = #
                string anonymousNumber = "%2331%23" + dut1Number; // Prefix to block caller ID

                if (!gclass.PlaceCall(_dut2Id, anonymousNumber))
                {
                    throw new Exception($"Failed to place call from DUT 2 [{_dut2Id}] to DUT 1 [{_dut1Id}]");
                }

                await Task.Delay(4000); // Wait longer for the call to be registered


                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 300 0 300 1000 500");

                await Task.Delay(2000); // Wait for the notification shade to open
                // answer call on DUT1
                string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                gclass.CaptureUIDump(_dut1Id, Path.GetDirectoryName(dumpPath));
                if (gclass.isInUIDump(dumpPath, "Private number"))
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_CALL");
                else
                    throw new Exception("Unknown Caller ID not shown on DUT 1.");

                // Wait 1 minute to maintain the call
                //await Task.Delay(60000);

                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.44: FAIL [{_dut1Id}, {_dut2Id}] - Call dropped early at {i} seconds.", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    //Thread.Sleep(1000);
                    await Task.Delay(1000);
                }

                // end call on DUT1
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");


                await Task.Delay(2000); // Wait for the notification shade to open





                gclass.UpdateOutput($"TC 1.48: PASS [{_dut1Id}, {_dut2Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";

            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.48: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {

                // Return to home screen
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                // go to home screen
                gclass.resetAll(_dut2Id);
                gclass.resetAll(_dut1Id);
            }

            gclass.LogTestResultToCSV("TC1.48", _dut1Id, result);
        }
    }
}
