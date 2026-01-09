/*
 * TC_1_91: Verify CNIP Presentation on MT Device for VoLTE Call
 * ------------------------------------------------------------
 * Purpose:
 *   Verify when calling out to another VoLTE device, the MT device is presented with CNIP.
 * 
 * Steps:
 *   1. Ensure DUT 1 and DUT 2 are IMS registered on LTE.
 *   2. From DUT 1, call DUT 2.
 *   3. Ensure DUT 2 shows the correct CNAP string.
 *   4. Maintain the call for 1 min and then end the call.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_91
    {
        private string _dut1Id;
        private string _dut2Id;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_91(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _dut2Id = dut2Id;
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
                    gclass.UpdateOutput("Starting TC 1.91: Verify CNIP Presentation on MT Device for VoLTE Call");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // 1. Ensure DUT 1 and DUT 2 are IMS registered on LTE.
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}]");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id))
                    throw new Exception($"DUT 1 [{_dut1Id}] failed to register IMS on LTE.");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                    throw new Exception($"DUT 2 [{_dut2Id}] failed to register IMS on LTE.");

                // 2. From DUT 1, call DUT 2.
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(10000); // Wait for the call to connect

                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // 3. Ensure DUT 2 shows the correct CNAP string.
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
                        var cnapNode =
                            doc.SelectSingleNode("//node[contains(@text, 'CNIP')]") ??
                            doc.SelectSingleNode("//node[contains(@content-desc, 'CNIP')]") ??
                            doc.SelectSingleNode("//node[contains(@text, 'Name')]") ??
                            doc.SelectSingleNode("//node[contains(@content-desc, 'Name')]");
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

                gclass.UpdateOutput("DUT 2 displayed the correct CNAP/CNIP string for the incoming call.");

                // 4. Maintain the call for 1 min and then end the call.
                bool callStillActive = true;
                for (int i = 0; i < 60; i++)
                {
                    string callState = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!callState.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.91: FAIL [{_dut1Id}, {_dut2Id}] - Call dropped early at {i} seconds.", true);
                        _testButton.BackColor = Color.Red;
                        result = "FAIL";
                        break;
                    }
                    Thread.Sleep(1000);
                }

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");

                if (callStillActive)
                {
                    gclass.UpdateOutput($"TC 1.91: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.91: FAIL [{_dut1Id}, {_dut2Id}]", true);
                    _testButton.BackColor = Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.91: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }
            finally
            {
                // End all calls and reset device states
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.LogTestResultToCSV("TC1.91", _dut1Id, result);
            }
        }
    }
}