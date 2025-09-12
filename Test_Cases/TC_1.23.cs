/*
 * TC_1_23: Call Forwarding Unconditional (CFU) to Own Number Negative Test (XCAP)
 * -------------------------------------------------------------------------------
 * Purpose:
 *   Verify that a VoLTE device (DUT 1) is not able to perform CFU (Call Forwarding Unconditional)
 *   to its own number using XCAP (GBA-ME). The device should not show CFU as successful.
 * 
 * Steps:
 *   1. Ensure DUT 1 is IMS registered on LTE.
 *   2. On DUT 1, set up CFU to its own CTN (phone number).
 *   3. Verify the device does not show CFU as successful.
 */

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_23
    {
        private string _dut1Id;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once

        public TC_1_23(string dut1Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            result = "FAIL";

            // Log header ONCE (not per device)
            if (!headerLogged)
            {
                gclass.UpdateOutput("==================================================");
                gclass.UpdateOutput("Starting TC 1.23: Verify CFU to own number is not allowed (XCAP)...");
                gclass.UpdateOutput("==================================================\n");
                headerLogged = true;
            }

            try
            {
                if (!gclass.IsDeviceConnected(_dut1Id))
                    throw new Exception($"DUT is not connected. [{_dut1Id}]");

                gclass.SetAirplaneMode(_dut1Id, true);
                Thread.Sleep(3000);
                gclass.SetAirplaneMode(_dut1Id, false);
                Thread.Sleep(3000);

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id))
                    throw new Exception($"DUT failed to attach to LTE or register for VoLTE. [{_dut1Id}]");

                if (!gclass.WaitForIMSRegisteration(_dut1Id))
                    throw new Exception($"DUT is not IMS registered. [{_dut1Id}]");

                string ownNumber = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(ownNumber))
                    throw new Exception($"Failed to extract DUT phone number. [{_dut1Id}]");

                bool cfuResult = gclass.ForwardCalls(_dut1Id, ownNumber);
                Thread.Sleep(6000);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press Ok

                bool isCFUActive = gclass.IsCallForwardingActive(_dut1Id);

                if (isCFUActive)
                {
                    gclass.UpdateOutput($"TC 1.23: FAIL [{_dut1Id}]");
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.23: PASS [{_dut1Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.23: FAIL [{_dut1Id}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.DIAL -d tel:#21#"); // Disable CFU
            Thread.Sleep(6000);
            gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 567 1356"); // Press OK
            gclass.SetAirplaneMode(_dut1Id, true);

            // Log footer ONCE
            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.23", _dut1Id, result);
        }
    }
}