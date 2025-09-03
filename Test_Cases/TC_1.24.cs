/*
 * TC_1_24: Video Call Forwarding Downgrade to Audio Test Case
 * -----------------------------------------------------------
 * Purpose:
 *   Verify that when call forwarding is set up on DUT 2, a MO video call from DUT 1 to DUT 2
 *   is forwarded and downgraded to an audio call, and the audio call is maintained for 1 minute.
 * 
 * Steps:
 *   1. Check all devices are connected.
 *   2. Set Airplane mode ON, then OFF for DUT1 and DUT2.
 *   3. Wait for LTE/VoLTE registration and IMS registration.
 *   4. Set up call forwarding (CFU) on DUT 2 to a valid number (e.g., DUT 2's own number or a test number).
 *   5. Place a MO video call from DUT 1 to DUT 2.
 *   6. Ensure the call is downgraded and connected as audio.
 *   7. Ensure audio is OK (basic check: call is connected and not dropped).
 *   8. Maintain the call for 1 minute and end the call.
 *   9. Disable call forwarding and reset device state.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_24
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _moCallerId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;

        public TC_1_24(string dut1Id, string dut2Id, string moCallerId, RichTextBox outputRTB, Button testButton)
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
            gclass.UpdateOutput("Starting TC 1.24: Video Call Forwarding Downgrade to Audio...");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Check all devices are connected ---
                gclass.UpdateOutput("[Step 1] Checking device connections...");
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                {
                    gclass.UpdateOutput("One or more devices are not connected.", true);
                    throw new Exception("One or more devices are not connected.");
                }

                // --- Step 2: Set Airplane mode ON, then OFF for DUT1 and DUT2 ---
                gclass.UpdateOutput("[Step 2] Cycling Airplane mode for DUT1 & DUT2...");
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                Thread.Sleep(5000);

                // --- Step 3: Wait for LTE/VoLTE and IMS registration ---
                gclass.UpdateOutput("[Step 3] Waiting for LTE/VoLTE and IMS registration...");
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                {
                    gclass.UpdateOutput("DUT1 or DUT2 failed to attach to LTE or register for VoLTE.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    gclass.LogTestResultToCSV("TC1.24", _dut1Id, result);
                    return;
                }
                if (!gclass.WaitForIMSRegisteration(_dut1Id) || !gclass.WaitForIMSRegisteration(_dut2Id))
                {
                    gclass.UpdateOutput("DUT1 or DUT2 failed to register for IMS.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    gclass.LogTestResultToCSV("TC1.24", _dut1Id, result);
                    return;
                }
                gclass.UpdateOutput("DUT1 & DUT2 successfully attached to LTE and registered for VoLTE/IMS.");

                // --- Step 4: Set up call forwarding (CFU) on DUT 2 ---
                gclass.UpdateOutput("[Step 4] Setting up call forwarding (CFU) on DUT2...");
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id); // Forward to own number or a test number
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception("Failed to extract phone number from DUT2.");

                if (!gclass.ForwardCalls(_dut2Id, forwardToNumber))
                    throw new Exception("Failed to forward calls on DUT2.");

                Thread.Sleep(8000);

                // --- Step 5: Place a MO video call from DUT 1 to DUT 2 ---
                gclass.UpdateOutput("[Step 5] Placing MO video call from DUT1 to DUT2...");
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception("Failed to extract phone number from DUT2.");

                // Place a video call 
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.CALL -d tel:{dut2Number}" +
                   $" --ei android.telecom.extra.START_CALL_WITH_VIDEO_STATE 3");

                Thread.Sleep(5000);

                gclass.UpdateOutput("Answering video call on REF device...");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // --- Step 6: Ensure the call is downgraded and connected as audio
                gclass.UpdateOutput("[Step 6] Verifying call is downgraded to audio...");
                // ensure call is connected and not video 
                bool isAudioCall = !gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower().Contains("videocall");
                if (!isAudioCall)
                {
                    gclass.UpdateOutput("Call was not downgraded to audio. TC 1.24: Fail", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    gclass.LogTestResultToCSV("TC1.24", _dut1Id, result);
                    return;
                }

                // --- Step 7: Ensure audio is OK (call is connected and not dropped) ---
                gclass.UpdateOutput("[Step 7] Ensuring audio call is connected and stable...");
                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.24: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                // --- Step 8: End call and cleanup ---
                if (callStillActive)
                {
                    gclass.UpdateOutput("Audio call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.24: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                // --- Step 9: Disable call forwarding and reset device state ---
                gclass.UpdateOutput("[Step 9] Disabling call forwarding and resetting device states...");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell am start -a android.intent.action.DIAL -d tel:#21#");
                Thread.Sleep(2000);
                for (int i = 0; i < 12; i++)
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 1036 1364");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 902 2010"); // Press #
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 550 1505"); // Press 2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 213 1547"); // Press 1
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 902 2010"); // Press #
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 551 2188"); // Press Call button
                Thread.Sleep(6000);
                gclass.UpdateOutput("Call forwarding is disabled on DUT2.");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 567 1356"); // Press Ok
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.24: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.24", _dut1Id, result);
        }
    }
}