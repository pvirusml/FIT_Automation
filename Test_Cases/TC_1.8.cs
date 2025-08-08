using FIT_Automation.Scripts;
using System;
using System.Text.RegularExpressions;
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
            gclass.UpdateOutput("Starting TC 1.8: Verify CFU with XCAP GBA-ME on LTE...");

            try
            {
                // Step 1: Check all devices are connected
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_moCallerId))
                {
                    gclass.UpdateOutput("One or more devices are not connected.", true);
                    throw new Exception("One or more devices are not connected.");
                }

                // Step 2: Airplane mode cycle for DUT1 and DUT2
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.UpdateOutput("Airplane mode enabled for DUT1 & DUT2.");
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                gclass.UpdateOutput("Airplane mode disabled for DUT1 & DUT2.");
                Thread.Sleep(5000);

                // Step 3: Wait for LTE/IMS registration
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                {
                    gclass.UpdateOutput("DUT1 or DUT2 failed to attach to LTE or register for VoLTE.", true);
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.8", _dut1Id, result);
                    return;
                }
                gclass.UpdateOutput("DUT1 & DUT2 successfully attached to LTE and registered for VoLTE.");

                // Step 4: Set CFU on DUT1 to DUT2 using XCAP (GBA-ME)
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception("Failed to extract phone number from DUT2.");

                if (!gclass.ForwardCalls(_dut1Id, forwardToNumber))
                    throw new Exception("Failed to forward calls on DUT1.");

                Thread.Sleep(5000); // Wait for network to process forwarding

                // Step 5: Check if call forwarding is active on DUT1 (with retry)
                bool forwardingActive = false;
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    if (gclass.IsCallForwardingActive(_dut1Id))
                    {
                        forwardingActive = true;
                        break;
                    }
                    Thread.Sleep(3000); // Wait 3 seconds before retrying
                }
                if (!forwardingActive)
                {
                    gclass.UpdateOutput("Call forwarding is not active on DUT1.", true);
                    result = "FAIL";
                    _testButton.BackColor = System.Drawing.Color.Red;
                    gclass.LogTestResultToCSV("TC1.8", _dut1Id, result);
                    return;
                }
                gclass.UpdateOutput("Call forwarding is active on DUT1.");

                // Step 6: Place call from MO caller to DUT1 (should be forwarded to DUT2)
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception("Failed to extract phone number from DUT1.");

                if (!gclass.PlaceCall(_moCallerId, dut1Number))
                    throw new Exception("Failed to place call from MO caller to DUT1.");

                Thread.Sleep(5000); // Let it ring and forward

                // Step 7: Answer call on DUT2 (forwarded-to device)
                gclass.UpdateOutput("Answering forwarded call on DUT2...");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // Step 8: Maintain call for 60 seconds or until dropped
                bool callStillActive = true;
                int duration = 60;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2")) // 2 = CALL_STATE_OFFHOOK
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"Call dropped early at {i} seconds. TC 1.8: Fail", true);
                        _testButton.BackColor = System.Drawing.Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                // Step 9: End call if still active
                if (callStillActive)
                {
                    gclass.UpdateOutput("Call maintained for 60 seconds.");
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                    gclass.UpdateOutput("Call ended. TC 1.8: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }

                // Put in Airplane mode
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.8: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.8", _dut1Id, result);
        }

        // --- XCAP-style helper methods ---

       
    }
}