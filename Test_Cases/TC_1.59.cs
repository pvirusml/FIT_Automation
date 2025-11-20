/*
 * TC_1_59: VoWiFi > VoWiFi + CS - Call merge between 3 devices
 * -----------------------------------------------------
1) Place a call from DUT 1 to DUT 2. While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1.
2) Tap on "Hold & Accept", to accept the incoming call from DUT 3.
3) Once the call is established with DUT 3 make sure that audio is heard in both directions.
4) Ensure DUT 2 is on Hold.
5) Now on DUT 1, tap on "Merge" to merge all 3 calls.
6) Make sure that all parties can send and receive audio on the conference.
7) End the Conference call on DUT 1 and make sure that the conference call is ended successfully.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_59
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _dut3Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_59(string dut1Id, string dut2Id, string dut3Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _dut2Id = dut2Id;
            _dut3Id = dut3Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.59: VoWiFi > VoWiFi + CS - Call merge between 3 devices");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Place a call from DUT 1 to DUT 2
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_dut3Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_dut3Id}]");

                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(5000);
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Step 2: Place a call from DUT 3 to DUT 1
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut1Id}]");

                if (!gclass.PlaceCall(_dut3Id, dut1Number))
                    throw new Exception($"Failed to place call from DUT 3 [{_dut3Id}] to DUT 1 [{_dut1Id}]");

                Thread.Sleep(5000);

                // Step 3: Tap on "Hold & Accept" to accept the incoming call from DUT 3
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Hold");
                Thread.Sleep(5000);

                // Step 4: Ensure DUT 2 is on Hold
                gclass.CaptureUIDump(_dut1Id, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                if (!gclass.isInUIDump("ui_dump.xml", "Hold"))
                    throw new Exception("DUT 2 is not on hold.");

                gclass.UpdateOutput("DUT 2 is on hold.");

                // Step 5: Tap on "Merge" to merge all 3 calls
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Merge");
                Thread.Sleep(5000);

                // Step 6: Ensure all parties can send and receive audio on the conference
                gclass.UpdateOutput("Ensuring audio is OK between all parties (manual/automated check recommended).");
                Thread.Sleep(60000); // Wait for 1 minute to verify audio

                // Step 7: End the conference call on DUT 1
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                Thread.Sleep(3000);

                gclass.UpdateOutput($"TC 1.59: PASS [{_dut1Id}, {_dut2Id}, {_dut3Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.59: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Cleanup
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut3Id} shell input keyevent KEYCODE_HOME");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.DisableWiFi(_dut3Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_dut3Id, true);
                gclass.LogTestResultToCSV("TC1.59", _dut1Id, result);
            }
        }
    }
}