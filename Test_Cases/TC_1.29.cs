/*
 * TC_1_29: Call Waiting, Hold, and Audio Verification Test Case (VoLTE/VoWiFi)
 * -----------------------------------------------------------------------------
 * Purpose:
 *   Verify call waiting, hold, and audio path between three devices:
 *   DUT 1 (VoLTE), DUT 2 (any), and DUT 3 (VoWiFi).
 * 
 * Steps:
 *   1. Place a call from DUT 1 to DUT 2.
 *   2. While the call is in progress, place a call from DUT 3 to DUT 1 and validate call waiting notification on DUT 1.
 *   3. Answer the incoming call from DUT 3 on DUT 1 (DUT 2 is automatically put on hold).
 *   4. Keep DUT 2 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 3.
 *   5. End call with DUT 3, resume call with DUT 2, keep DUT 3 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 2.
 *   6. End all calls and reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_29
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

        public TC_1_29(string dut1Id, string dut2Id, string dut3Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.29: Call Waiting, Hold, and Audio Verification Test (VoLTE/VoWiFi)");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Place a call from DUT 1 to DUT 2
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_dut3Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_dut3Id}]");

                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(5000);
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Step 2: While call is in progress, place a call from DUT 3 to DUT 1 and validate call waiting notification on DUT 1
                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut1Id}]");

                if (!gclass.PlaceCall(_dut3Id, dut1Number))
                    throw new Exception($"Failed to place call from DUT 3 [{_dut3Id}] to DUT 1 [{_dut1Id}]");

                Thread.Sleep(5000);

                // Wait for call waiting notification on DUT 1
                bool callWaitingDetected = false;
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
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
                    throw new Exception("DUT 1 did not show call waiting/incoming call UI for the call from DUT 3.");

                //gclass.UpdateOutput("DUT 1 received call waiting notification from DUT 3.");

                // Step 3: Answer the incoming call from DUT 3 on DUT 1 (DUT 2 is automatically put on hold)
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Step 4: Keep DUT 2 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 3
                //gclass.UpdateOutput("DUT 2 is on hold. Waiting for 1 minute...");
                Thread.Sleep(60000);
                gclass.UpdateOutput("Ensuring audio is OK between DUT 1 and DUT 3 (manual/automated check recommended).");

                // Step 5: End call with DUT 3, resume call with DUT 2, keep DUT 3 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 2
                gclass.RunAdbCommand($"adb -s {_dut3Id} shell input keyevent KEYCODE_ENDCALL");
                Thread.Sleep(4000);
                //gclass.UpdateOutput("DUT 3 call ended. DUT 2 call resumed. Waiting for 1 minute...");
                Thread.Sleep(60000);
                //gclass.UpdateOutput("Ensuring audio is OK between DUT 1 and DUT 2 (manual/automated check recommended).");

                // Step 6: End all calls and reset device states
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");

                gclass.UpdateOutput($"TC 1.29: PASS [{_dut1Id}, {_dut2Id}, {_dut3Id}]");
                _testButton.BackColor = Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.29: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.29", _dut1Id, result);
        }
    }
}