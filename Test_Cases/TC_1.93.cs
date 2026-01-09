/*
 * TC_1_93: Verify Call Waiting Notification for Incoming Video VoLTE Call
 * -----------------------------------------------------------------------
 * Purpose:
 *   Verify call waiting notification while on a VoLTE video call for another incoming VoLTE video call.
 * 
 * Steps:
 *   1. Ensure DUT 1 is IMS registered on LTE.
 *   2. Setup a video call between DUT 1 and DUT 2.
 *   3. From DUT 3, call DUT 1 while DUT 1 and DUT 2 are in an active video call.
 *   4. Verify that DUT 1 receives a call waiting notification.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_93
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

        public TC_1_93(string dut1Id, string dut2Id, string dut3Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.93: Verify Call Waiting Notification for Incoming Video VoLTE Call");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // 1. Ensure DUT 1 is IMS registered on LTE.
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_dut3Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_dut3Id}]");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id))
                    throw new Exception($"DUT 1 [{_dut1Id}] failed to register IMS on LTE.");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                    throw new Exception($"DUT 2 [{_dut2Id}] failed to register IMS on LTE.");

                if (!gclass.WaitForLTEAndVoLTERegistration(_dut3Id))
                    throw new Exception($"DUT 3 [{_dut3Id}] failed to register IMS on LTE.");

                // 2. Setup a video call between DUT 1 and DUT 2.
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place video call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(10000); // Wait for the call to connect

                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // 3. From DUT 3, call DUT 1 while DUT 1 and DUT 2 are in an active video call.
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut1Id}]");

                if (!gclass.PlaceCall(_dut3Id, dut1Number))
                    throw new Exception($"Failed to place video call from DUT 3 [{_dut3Id}] to DUT 1 [{_dut1Id}]");

                Thread.Sleep(10000); // Wait for the call waiting notification

                // 4. Verify that DUT 1 receives a call waiting notification.
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                bool callWaitingDetected = false;
                for (int i = 0; i < 10; i++)
                {
                    gclass.CaptureUIDump(_dut1Id, outputPath);
                    var doc = new System.Xml.XmlDocument();
                    string uiDumpPath = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                    try
                    {
                        doc.Load(uiDumpPath);
                        var waitingNode =
                            doc.SelectSingleNode("//node[contains(@text, 'call waiting')]") ??
                            doc.SelectSingleNode("//node[contains(@content-desc, 'call waiting')]") ??
                            doc.SelectSingleNode("//node[contains(@text, 'incoming call')]") ??
                            doc.SelectSingleNode("//node[contains(@text, 'Answer')]") ??
                            doc.SelectSingleNode("//node[contains(@content-desc, 'incoming call')]");
                        if (waitingNode != null)
                        {
                            callWaitingDetected = true;
                            break;
                        }
                    }
                    catch { }
                    Thread.Sleep(1000);
                }

                if (!callWaitingDetected)
                    throw new Exception("DUT 1 did not show call waiting notification for the incoming video call from DUT 3.");

                gclass.UpdateOutput("DUT 1 successfully displayed the call waiting notification for the incoming video call from DUT 3.");

                gclass.UpdateOutput($"TC 1.93: PASS [{_dut1Id}, {_dut2Id}, {_dut3Id}]");
                _testButton.BackColor = Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.93: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }
            finally
            {
                // End all calls and reset device states
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut3Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.DisableWiFi(_dut3Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.SetAirplaneMode(_dut3Id, true);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut3Id} shell input keyevent KEYCODE_HOME");
                gclass.LogTestResultToCSV("TC1.93", _dut1Id, result);
            }
        }
    }
}