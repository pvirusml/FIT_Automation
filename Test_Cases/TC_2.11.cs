/*
 * TC_2_11: Call Waiting, Hold, and Swap Test Case (VoLTE/VoWiFi)
 * --------------------------------------------------------------
 * Purpose:
 *   Verify call waiting, hold, and swap functionality between three devices:
 *   DUT 1 (VoLTE), DUT 2 (any), and DUT 3 (VoWiFi).
 * 
 * Steps:
 *   1. Place a call from DUT 1 to DUT 2.
 *   2. While the call is in progress, place a call from DUT 3 to DUT 1 and validate call waiting notification on DUT 1.
 *   3. Hold current call and answer incoming call from DUT 3 (using UI dump to answer).
 *   4. DUT 2 is on hold, keep it on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 3.
 *   5. Use UI dump to find and tap "Swap" on DUT 1, putting DUT 3 on hold, keep DUT 3 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 2.
 *   6. Use UI dump to find and tap "Swap" again on DUT 1, putting DUT 2 on hold, keep DUT 2 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 3.
 *   7. End all calls and reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_2_11
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

        public TC_2_11(string dut1Id, string dut2Id, string dut3Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 2.11: Call Waiting, Hold, and Swap Test (VoLTE/VoWiFi)");
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

                // Turn off airplane mode
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                gclass.SetAirplaneMode(_dut3Id, false);

                Thread.Sleep(7000); // Wait for network registration
                if(!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut3Id))
                    throw new Exception($"DUT 1 or DUT 3 failed to attach to LTE or register for VoLTE. [{_dut1Id}, {_dut3Id}]");

                gclass.EnableWiFi(_dut3Id);

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

            
                //swipe down
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 540 0 540 1600");
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
                    throw new Exception("DUT 1 did not show call waiting/incoming call UI for the call from DUT 3.");

                

                // Step 3: Hold current call and answer incoming call from DUT 3 (using UI dump to answer)
                gclass.CaptureUIDump(_dut1Id, outputPath);
                var docAnswer = new System.Xml.XmlDocument();
                string uiDumpPathAnswer = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                docAnswer.Load(uiDumpPathAnswer);
                var answerNode =
                    docAnswer.SelectSingleNode("//node[contains(@text, 'Answer')]") ??
                    docAnswer.SelectSingleNode("//node[contains(@content-desc, 'Answer')]") ??
                    docAnswer.SelectSingleNode("//node[contains(@text, 'answer')]") ??
                    docAnswer.SelectSingleNode("//node[contains(@content-desc, 'answer')]");
                if (answerNode == null)
                    throw new Exception("Answer button not found in UI dump for incoming call on DUT 1.");
                string bounds = answerNode.Attributes["bounds"].Value;
                var match = System.Text.RegularExpressions.Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
                if (!match.Success)
                    throw new Exception("Invalid bounds format for Answer button: " + bounds);
                int left = int.Parse(match.Groups[1].Value);
                int top = int.Parse(match.Groups[2].Value);
                int right = int.Parse(match.Groups[3].Value);
                int bottom = int.Parse(match.Groups[4].Value);
                int centerX = (left + right) / 2;
                int centerY = (top + bottom) / 2;
                gclass.SendTap(_dut1Id, centerX, centerY);
                Thread.Sleep(4000);

                // swip up
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 612 2175 615 530");
                Thread.Sleep(1000);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 612 2175 615 530");

                Thread.Sleep(4000);


                // Step 4: DUT2 is on hold, keep it on hold for 1 minute and ensure audio is OK between DUT1 and DUT3
                gclass.UpdateOutput("DUT 2 is on hold. Waiting for 1 minute...");
                Thread.Sleep(55000);
                gclass.UpdateOutput("Ensuring audio is OK between DUT 1 and DUT 3 (manual/automated check recommended).");

                // Step 5: Use UI dump to find and tap "Swap" on DUT 1, putting DUT 3 on hold, keep DUT 3 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 2
                gclass.UpdateOutput("Swapping calls so DUT 3 is on hold.");
                gclass.CaptureUIDump(_dut1Id, outputPath);
                var docSwap = new System.Xml.XmlDocument();
                string uiDumpPathSwap = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                docSwap.Load(uiDumpPathSwap);
                var swapNode =
                    docSwap.SelectSingleNode("//node[contains(@text, 'Swap')]") ??
                    docSwap.SelectSingleNode("//node[contains(@content-desc, 'Swap')]") ??
                    docSwap.SelectSingleNode("//node[contains(@text, 'swap')]") ??
                    docSwap.SelectSingleNode("//node[contains(@content-desc, 'swap')]");
                if (swapNode == null)
                    throw new Exception("Swap button not found in UI dump for call swap on DUT 1.");
                string swapBounds = swapNode.Attributes["bounds"].Value;
                var swapMatch = System.Text.RegularExpressions.Regex.Match(swapBounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
                if (!swapMatch.Success)
                    throw new Exception("Invalid bounds format for Swap button: " + swapBounds);
                int swapLeft = int.Parse(swapMatch.Groups[1].Value);
                int swapTop = int.Parse(swapMatch.Groups[2].Value);
                int swapRight = int.Parse(swapMatch.Groups[3].Value);
                int swapBottom = int.Parse(swapMatch.Groups[4].Value);
                int swapCenterX = (swapLeft + swapRight) / 2;
                int swapCenterY = (swapTop + swapBottom) / 2;
                gclass.SendTap(_dut1Id, swapCenterX, swapCenterY);
                Thread.Sleep(4000);

                gclass.UpdateOutput("DUT 3 is on hold. Waiting for 1 minute...");
                Thread.Sleep(60000);
                gclass.UpdateOutput("Ensuring audio is OK between DUT 1 and DUT 2 (manual/automated check recommended).");

                // Step 6: Use UI dump to find and tap "Swap" again on DUT 1, putting DUT 2 on hold, keep DUT 2 on hold for 1 minute and ensure audio is OK between DUT 1 and DUT 3
                gclass.UpdateOutput("Swapping calls so DUT 2 is on hold again.");
                gclass.CaptureUIDump(_dut1Id, outputPath);
                var docSwap2 = new System.Xml.XmlDocument();
                string uiDumpPathSwap2 = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                docSwap2.Load(uiDumpPathSwap2);
                var swapNode2 =
                    docSwap2.SelectSingleNode("//node[contains(@text, 'Swap')]") ??
                    docSwap2.SelectSingleNode("//node[contains(@content-desc, 'Swap')]") ??
                    docSwap2.SelectSingleNode("//node[contains(@text, 'swap')]") ??
                    docSwap2.SelectSingleNode("//node[contains(@content-desc, 'swap')]");
                if (swapNode2 == null)
                    throw new Exception("Swap button not found in UI dump for call swap on DUT 1 (second swap).");
                string swapBounds2 = swapNode2.Attributes["bounds"].Value;
                var swapMatch2 = System.Text.RegularExpressions.Regex.Match(swapBounds2, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
                if (!swapMatch2.Success)
                    throw new Exception("Invalid bounds format for Swap button: " + swapBounds2);
                int swapLeft2 = int.Parse(swapMatch2.Groups[1].Value);
                int swapTop2 = int.Parse(swapMatch2.Groups[2].Value);
                int swapRight2 = int.Parse(swapMatch2.Groups[3].Value);
                int swapBottom2 = int.Parse(swapMatch2.Groups[4].Value);
                int swapCenterX2 = (swapLeft2 + swapRight2) / 2;
                int swapCenterY2 = (swapTop2 + swapBottom2) / 2;
                gclass.SendTap(_dut1Id, swapCenterX2, swapCenterY2);
                Thread.Sleep(4000);

                gclass.UpdateOutput("DUT 2 is on hold again. Waiting for 1 minute...");
                Thread.Sleep(60000);
                gclass.UpdateOutput("Ensuring audio is OK between DUT 1 and DUT 3 (manual/automated check recommended).");



                gclass.UpdateOutput($"TC 2.11: PASS [{_dut1Id}, {_dut2Id}, {_dut3Id}]");
                _testButton.BackColor = Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 2.11: FAIL [{_dut1Id}, {_dut2Id}, {_dut3Id}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Step 7: End all calls and reset device states
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.resetAll(_dut1Id);
                gclass.resetAll(_dut2Id);
                gclass.resetAll(_dut3Id);
                gclass.LogTestResultToCSV("TC2.11", _dut1Id, result);
            }

            
        }
    }
}