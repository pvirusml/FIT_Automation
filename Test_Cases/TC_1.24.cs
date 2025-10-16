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
using System.IO;
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
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();


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
            result = "FAIL";

            lock (_lockObject)
            {
                // Ensure thread-safe logging of header
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.24: Video Call Forwarding Downgrade to Audio...");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Check all devices are connected
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                // Step 2: Set Airplane mode ON, then OFF for DUT1 and DUT2
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                Thread.Sleep(5000);

                // Step 3: Wait for LTE/VoLTE and IMS registration
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                    throw new Exception($"DUT1 or DUT2 failed to attach to LTE or register for VoLTE. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                if (!gclass.WaitForIMSRegisteration(_dut1Id) || !gclass.WaitForIMSRegisteration(_dut2Id))
                    throw new Exception($"DUT1 or DUT2 failed to register for IMS. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                // Step 4: Set up call forwarding (CFU) on DUT 2
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception($"Failed to extract phone number from DUT2 [{_dut2Id}]");

                if (!gclass.ForwardCalls(_dut2Id, forwardToNumber))
                    throw new Exception($"Failed to forward calls on DUT2 [{_dut2Id}]");

                Thread.Sleep(8000);

                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "OK");
                Thread.Sleep(2000);

                // Step 5: Place a MO video call from DUT 1 to DUT 2
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT2 [{_dut2Id}]");

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.CALL -d tel:{dut2Number} --ei android.telecom.extra.START_CALL_WITH_VIDEO_STATE 3");
                Thread.Sleep(5000);

                // swipe down
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1600");
                Thread.Sleep(3000);
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Voice");

                Thread.Sleep(3000);
                //gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Ongoing call");
                //swipe up
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 612 2175 615 530");
                Thread.Sleep(3000);

                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Mute");
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Mute");
                Thread.Sleep(3000);

                // Step 6: Ensure the call is downgraded and connected as audio
                string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                bool isAudioCall = gclass.isInContentDescUIDump(dumpPath,"Video call"); 
                if (!isAudioCall)
                    throw new Exception($"Call was not downgraded to audio. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                // Step 7: Ensure audio is OK (call is connected and not dropped)
                bool callStillActive = true;
                int duration = 30;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.24: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}] - Call dropped early at {i} seconds.", true);
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
                    gclass.UpdateOutput($"TC 1.24: PASS [{_dut1Id}, {_dut2Id}, {_moCallerId}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.24: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }


            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.24: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Step 9: Disable call forwarding and reset device state
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
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 567 1356"); // Press Ok
                // Ensure all calls are ended and airplane mode is set
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }

            // Log footer ONCE
            //gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.24", _dut1Id, result);
        }
    }
}