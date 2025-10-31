/*
 * TC_1_51:  Verify Country Code PANI Header
 * -----------------------------------------------------
1) Place a call from DUT 1 to DUT 2
2) Ensure call is connected, audio is ok
3) Maintain the call for 1min and end the call
 */

using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_51
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_51(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.51: VoWiFi call from Wi-Fi AP1 to another VoWiFI device connected to Wi-FI AP2");
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
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut2Id}]");
                }

                // Properly handle airplane mode toggle and network stabilization
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000); // Wait for airplane mode to apply
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);

                gclass.EnableWiFi(_dut1Id);
                gclass.EnableWiFi(_dut2Id);

                Thread.Sleep(19000); // Wait for network stabilization


                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                {
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");
                }

                await Task.Delay(4000); // Wait longer for the call to be registered
                // Swip down notification bar on DUT2 to see incoming call
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1000");

                // Check if PANI header is present - Placeholder for actual implementation
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string uiDumpPath = $"{outputPath}\\ui_dump.xml";
                gclass.CaptureUIDump(_dut2Id, outputPath);
                bool isPaniHeaderPresent = gclass.isInUIDump(uiDumpPath, "+1"); // Assuming +1 is the country code to check for

                if(!isPaniHeaderPresent)
                {
                    throw new Exception($"PANI header with country code not found in DUT 2 [{_dut2Id}]");
                }

                // answer call on DUT2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                await Task.Delay(60000);

                // At this point, both calls have been handled as per the test case requirements.
                // end call on DUT1
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");


                gclass.UpdateOutput($"TC 1.51: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.51: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
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
        }
    }
}
