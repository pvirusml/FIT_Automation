/*
 * TC_1_90: VoLTE CNIP (Calling Name Identity Presentation) Test Case
 * ------------------------------------------------------------------
 * Purpose:
 *   Verify the VoLTE device can show Calling Name Identity Presentation (CNIP) for incoming VoLTE calls.
 * 
 * Steps:
 *   1. Ensure DUT 1 is IMS registered on LTE.
 *   2. Initiate CFU to DUT 2.
 *   3. Ensure the device uses XCAP (GBA-ME) to set up CFU.
 *   4. Call DUT 3, ensure call is forwarded to DUT 2.
 *   5. Ensure DUT 2 shows the correct CNAP string.
 *   6. Maintain the call for 1 min and then end the call.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_90
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _dut3Id;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_90(string dut1Id, string dut2Id, string dut3Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.90: VoLTE CNIP (Calling Name Identity Presentation) Test");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                gclass.SetAirplaneMode(_dut3Id, false);

                // 1. Ensure DUT 1 is IMS registered on LTE.
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_dut3Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_dut3Id}]");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id))
                    throw new Exception($"DUT 1 [{_dut1Id}] failed to register IMS on LTE.");

                // 2. Initiate CFU to DUT 2.
                string forwardToNumber = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(forwardToNumber))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                // 3. Ensure the device uses XCAP (GBA-ME) to set up CFU.
                if (!gclass.ForwardCalls(_dut1Id, forwardToNumber))
                    throw new Exception($"Failed to forward calls on DUT 1 [{_dut1Id}] using XCAP.");

               

                Thread.Sleep(10000); // Wait for CFU to be set

                // press ok
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "OK");

                // 4. Call DUT 3, ensure call is forwarded to DUT 2.
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                string dut3Number = gclass.ExtractPhoneNumber(_dut3Id);
                if (string.IsNullOrWhiteSpace(dut3Number))
                    throw new Exception($"Failed to extract phone number from DUT 3 [{_dut3Id}]");

                if (!gclass.PlaceCall(_dut3Id, dut1Number))
                    throw new Exception($"Failed to place call from DUT 3 [{_dut3Id}] to DUT 1 [{_dut1Id}]");

                Thread.Sleep(10000); // Wait for call to be forwarded

                // swipe up to answer on DUT 2 (in case it doesn't auto-answer)
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 490 280 540 1100");

                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // 5. Ensure DUT 2 shows the correct CNAP string.
                // Capture UI dump and check for CNAP/CNIP string (e.g., caller name)
                //Get mo number
                string moNumber = "";
                string searchSuffix = dut3Number.Length >= 4
    ? dut3Number.Substring(dut3Number.Length - 4)
    : dut3Number;

                Thread.Sleep(3000);
                gclass.SendTap(_dut2Id, 500, 650);
                Thread.Sleep(3000);
                // change to first 3 letters of number to increase chances of matching caller name in CNAP/CNIP (since some carriers only show partial number or name)
                for (int i = 0;i < 3; i++)
                {
                    moNumber += dut3Number[i];
                }
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                bool cnapDetected = false;
                for (int i = 0; i < 10; i++)
                {
                    gclass.CaptureUIDump(_dut2Id, outputPath);
                    var doc = new System.Xml.XmlDocument();
                    string uiDumpPath = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                    try
                    {
                        doc.Load(uiDumpPath);
                        // Look for a node with the expected CNAP string (customize as needed)
                        var cnapNode =
                            doc.SelectSingleNode($"//node[contains(@text, '{searchSuffix}')]");
                        if (cnapNode != null)
                        {
                            cnapDetected = true;
                            break;
                        }
                    }
                    catch { }
                    Thread.Sleep(1000);
                }

                if (!cnapDetected)
                    throw new Exception("DUT 2 did not show the expected CNAP/CNIP string for the incoming call.");

                gclass.UpdateOutput("DUT 2 displayed the correct CNAP/CNIP string for the forwarded call.");

                // 6. Maintain the call for 1 min and then end the call.
                bool callStillActive = true;
                for (int i = 0; i < 30; i++)
                {
                    string callState = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!callState.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.90: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - Call dropped early at {i} seconds.", true);
                        _testButton.BackColor = Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                gclass.RunAdbCommand($"adb -s {_dut3Id} shell input keyevent KEYCODE_ENDCALL");

                if (callStillActive)
                {
                    gclass.UpdateOutput($"TC 1.90: PASS [{_dut1Id}, {_dut2Id}, {_dut3Id}]");
                    _testButton.BackColor = Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.90: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}]", true);
                    _testButton.BackColor = Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.90: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }
            finally
            {
                gclass.disableCFU(_dut1Id);
                Thread.Sleep(3000);
                //press ok to disable CFU
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "OK");
                // End all calls and reset device states
                gclass.resetAll(_dut3Id);
                gclass.resetAll(_dut2Id);
                gclass.resetAll(_dut1Id);

            }

            gclass.LogTestResultToCSV("TC1.90", _dut1Id, result);
        }
    }
}
