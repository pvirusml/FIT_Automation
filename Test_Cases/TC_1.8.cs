/*
 * TC_1_8: Call Forwarding Unconditional (CFU) with XCAP GBA-ME Test Case
 * ----------------------------------------------------------------------
 * Purpose:
 *   Verify that CFU can be set using XCAP (GBA-ME) and that a call to DUT1 is forwarded to DUT2,
 *   and the call is maintained for 60 seconds.
 * 
 * Steps:
 *   1. Check all devices are connected.
 *   2. Set Airplane mode ON, then OFF for DUT1 and DUT2.
 *   3. Wait for LTE/VoLTE registration.
 *   4. Set CFU on DUT1 to DUT2.
 *   5. Place call from MO caller to DUT1 (should be forwarded to DUT2).
 *   6. Answer call on DUT2.
 *   7. Maintain call for 60 seconds.
 *   8. Disable CFU and reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_8
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _moCallerId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_8(string dut1Id, string dut2Id, string moCallerId, RichTextBox outputRTB, Button testButton)
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
            result = "FAIL";

            // Log header ONCE (not per device set)
            if (!headerLogged)
            {
                gclass.UpdateOutput("==================================================");
                gclass.UpdateOutput("Starting TC 1.8: Verify CFU with XCAP GBA-ME on LTE...");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }

            try
            {
                // Step 1: Check all devices are connected
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_moCallerId))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                // Step 2: Set Airplane mode ON, then OFF for DUT1, DUT2, MO
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_moCallerId, true);
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                gclass.SetAirplaneMode(_moCallerId, false);
                Thread.Sleep(5000);

                // Step 3: Wait for LTE/VoLTE registration
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                {
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.UpdateOutput($"TC 1.8: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}]", true);
                    gclass.LogTestResultToCSV("TC1.8", _dut1Id, result);
                    return;
                }

                // Step 4: Set CFU on DUT1 to DUT2
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception($"Failed to extract phone number from DUT2 [{_dut2Id}]");

                if (!gclass.ForwardCalls(_dut1Id, forwardToNumber))
                    throw new Exception($"Failed to forward calls on DUT1 [{_dut1Id}]");

                Thread.Sleep(6000);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press Ok

                // Step 5: Place call from MO caller to DUT1 (should be forwarded to DUT2)
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception($"Failed to extract phone number from DUT1 [{_dut1Id}]");

                if (!gclass.PlaceCall(_moCallerId, dut1Number))
                    throw new Exception($"Failed to place call from MO caller [{_moCallerId}] to DUT1 [{_dut1Id}]");

                Thread.Sleep(5000);

                // Step 6: Answer call on DUT2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // Step 7: Maintain call for 60 seconds
                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.8: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}] - Call dropped early at {i} seconds.", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                // Step 8: End call and cleanup
                if (callStillActive)
                {
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput($"TC 1.8: PASS [{_dut1Id}, {_dut2Id}, {_moCallerId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                // Disable CFU and reset device state
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
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press Ok
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_moCallerId, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.8: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            // Log footer ONCE
            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.8", _dut1Id, result);
        }
    }
}