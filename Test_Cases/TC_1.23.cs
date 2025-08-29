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

        public TC_1_23(string dut1Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("==================================================");
            gclass.UpdateOutput("Starting TC 1.23: Verify CFU to own number is not allowed (XCAP)...");
            gclass.UpdateOutput("==================================================\n");

            try
            {
                // --- Step 1: Ensure DUT 1 is IMS registered on LTE ---
                gclass.UpdateOutput("[Step 1] Ensuring DUT 1 is IMS registered on LTE...");
                if (!gclass.IsDeviceConnected(_dut1Id))
                {
                    gclass.UpdateOutput("DUT 1 is not connected.", true);
                    throw new Exception("DUT 1 is not connected.");
                }

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id))
                {
                    gclass.UpdateOutput("DUT 1 failed to attach to LTE or register for VoLTE.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    gclass.LogTestResultToCSV("TC1.23", _dut1Id, result);
                    return;
                }

                if (!gclass.WaitForIMSRegisteration(_dut1Id))
                {
                    gclass.UpdateOutput("DUT 1 is not IMS registered.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                    gclass.LogTestResultToCSV("TC1.23", _dut1Id, result);
                    return;
                }
                gclass.UpdateOutput("DUT 1 is IMS registered on LTE.");

                // --- Step 2: On DUT 1, set up CFU to its own CTN ---
                gclass.UpdateOutput("[Step 2] Attempting to set CFU to own number on DUT 1...");
                string ownNumber = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(ownNumber))
                    throw new Exception("Failed to extract DUT 1 phone number.");

                bool cfuResult = gclass.ForwardCalls(_dut1Id, ownNumber);
                Thread.Sleep(3000); // Wait for UI/response

                // --- Step 3: Verify the device does not show CFU as successful ---
                gclass.UpdateOutput("[Step 3] Verifying device does not show CFU as successful...");
                bool isCFUActive = gclass.IsCallForwardingActive(_dut1Id);

                if (cfuResult || isCFUActive)
                {
                    gclass.UpdateOutput("CFU to own number was set or reported as active. TC 1.23: Fail", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
                else
                {
                    gclass.UpdateOutput("CFU to own number was not set (as expected). TC 1.23: Pass");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.23: Fail - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.23", _dut1Id, result);
        }
    }
}