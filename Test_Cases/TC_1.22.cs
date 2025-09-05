/*
 * TC_1_22: CFU with 3 VoWiFi Devices Test Case
 * ------------------------------------------------
 * Purpose:
 *   Verify that a 1-minute VoWiFi call can be established and maintained between a VoWiFi device (DUT)
 *   and another VoWiFi device through CFU (Call Forwarding Unconditional) on a third VoWiFi device.
 *   The test ensures all devices are properly registered, the call is established, maintained for 60 seconds,
 *   and then properly ended. The test also resets device states at the end.
 * 
 * Steps:
 *   1. Check all device connections.
 *   2. Set Airplane mode ON for all devices.
 *   3. Enable WiFi for all devices.
 *   4. Set up CFU (forwarding) from DUT1 to DUT2.
 *   5. Place call from MO device to DUT1 (should be forwarded to DUT2) and answer on DUT2.
 *   6. Maintain call for 60 seconds.
 *   7. End call, disable CFU, and reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_22
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _moCallerId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;

        public TC_1_22(string dut1Id, string dut2Id, string moCallerId, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _dut2Id = dut2Id;
            _moCallerId = moCallerId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.22: Verify CFU with 3 VoWiFi devices");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Check all device connections ---
                gclass.UpdateOutput("[Step 1] Checking device connections...");
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_moCallerId))
                {
                    gclass.UpdateOutput("One or more devices are not connected.", true);
                    throw new Exception("One or more devices are not connected.");
                }

                // --- Step 2: Set Airplane mode ON for all devices, then OFF for all devices ---
                gclass.UpdateOutput("[Step 2] Enabling Airplane mode for DUT, REF, and MO device...");
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_moCallerId, true);
                gclass.UpdateOutput("Airplane mode enabled for DUT, REF & MO device.");
                Thread.Sleep(3000);

                // --- Step 3: Enable WiFi for all devices ---
                gclass.EnableWiFi(_dut1Id);
                gclass.EnableWiFi(_dut2Id);
                gclass.EnableWiFi(_moCallerId);
                gclass.UpdateOutput("[Step 3] WiFi enabled for DUT, REF & MO device.");
                Thread.Sleep(11000);

                // --- Step 4: Set up CFU (forwarding) from DUT1 to DUT2 ---
                gclass.UpdateOutput("[Step 4] Setting CFU on DUT1 to DUT2...");
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception("Failed to extract phone number from DUT2.");

                if (!gclass.ForwardCalls(_dut1Id, forwardToNumber))
                    throw new Exception("Failed to forward calls on DUT1.");

                Thread.Sleep(10000);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press Ok

                // --- Step 5: Place call from MO device to DUT1 (should be forwarded to DUT2) and answer on DUT2 ---
                gclass.UpdateOutput("[Step 5] Placing call from MO device to DUT1 (should be forwarded to DUT2)...");
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception("Failed to extract phone number from DUT1.");

                if (!gclass.PlaceCall(_moCallerId, dut1Number))
                    throw new Exception("Failed to place call from MO device to DUT1.");

                Thread.Sleep(10000);

                gclass.UpdateOutput("[Step 6] Answering forwarded call on DUT2...");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // --- Step 7: Maintain call for 60 seconds ---
                gclass.UpdateOutput("[Step 7] Maintaining call for 60 seconds...");
                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.22: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                // --- Step 8: End call, disable CFU, and reset device states ---
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.22: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                gclass.UpdateOutput("[Step 8] Disabling call forwarding and resetting device states...");
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.DIAL -d tel:#21#");
                Thread.Sleep(2000);
                for (int i = 0; i < 12; i++)
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 1036 1364");
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 902 2010"); // Press #
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 550 1505"); // Press 2
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 213 1547"); // Press 1
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 902 2010"); // Press #
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 551 2188"); // Press Call button
                Thread.Sleep(6000);
                gclass.UpdateOutput("Call forwarding is disabled on DUT1.");
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press Ok

                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.DisableWiFi(_moCallerId);
                gclass.UpdateOutput("WiFi disabled for DUT, REF & MO device.");
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_moCallerId, true);
                gclass.UpdateOutput("Airplane mode enabled for DUT, REF & MO device.");

            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.22: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

          

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.22", _dut1Id, result);
        }
    }
}
