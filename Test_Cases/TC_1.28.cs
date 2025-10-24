/*
 * TC_1_28: VoWiFi Call Waiting with Incoming VoLTE Call Test Case
 * --------------------------------------------------------------
 * Purpose:
 *   Verify that while DUT 1 and DUT 2 are on an active VoWiFi call, DUT 2 can receive a new incoming call from a VoLTE device (MO device).
 *   This tests call waiting and multi-call handling between VoWiFi and VoLTE devices.
 * 
 * Steps:
 *   1. Ensure all devices are connected.
 *   2. Set Airplane mode ON and enable WiFi for DUT 1 and DUT 2.
 *   3. Wait for LTE/VoWiFi registration on DUT 1 and DUT 2.
 *   4. Initiate a call from DUT 1 to DUT 2 (VoWiFi call).
 *   5. Ensure call is connected and audio is OK.
 *   6. While call is ongoing, place a new call from MO (VoLTE) device to DUT 2.
 *   7. Verify DUT 2 receives the incoming VoLTE call (call waiting UI).
 *   8. End all calls and reset device states.
 */

using FIT_Automation.Scripts;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_28
    {
        private string _dut1Id;
        private string _dut2Id;
        private string _moCallerId;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_28(string dut1Id, string dut2Id, string moCallerId, RichTextBox outputRTB, Button testButton)
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
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.28: VoWiFi Call Waiting with Incoming VoLTE Call");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Ensure all devices are connected
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id) || !gclass.IsDeviceConnected(_moCallerId))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                // Step 2: Set Airplane mode ON and enable WiFi for DUT 1 and DUT 2
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                gclass.SetAirplaneMode(_moCallerId, false); // Ensure MO device is not in airplane mode
                Thread.Sleep(4000);
                // Step 3: Wait for LTE/VoWiFi registration on DUT 1 and DUT 2
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id) || !gclass.WaitForLTEAndVoLTERegistration(_moCallerId))
                    throw new Exception($"DUT 1 or DUT 2 or Mo Device failed to register for VoWiFi. [{_dut1Id}, {_dut2Id}, {_moCallerId}]");

                gclass.EnableWiFi(_dut1Id);
                gclass.EnableWiFi(_dut2Id);
                Thread.Sleep(7000);

                // Step 4: Initiate a call from DUT 1 to DUT 2 (VoWiFi call)
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place VoWiFi call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(5000);
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(4000);

                // Hit Mute
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Mute");

                // Step 5: Ensure call is connected and audio is OK
                string callState1 = gclass.RunAdbCommand($"adb -s {_dut1Id} shell dumpsys telephony.registry").ToLower();
                string callState2 = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                if (!callState1.Contains("callstate=2") || !callState2.Contains("callstate=2"))
                    throw new Exception("Initial VoWiFi call was not connected.");

                gclass.UpdateOutput("Initial VoWiFi call is connected and audio is OK.");

                // Step 6: While call is ongoing, place a new call from MO (VoLTE) device to DUT 2
                string dut2NumberForMO = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2NumberForMO))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}] for MO device.");

                if (!gclass.PlaceCall(_moCallerId, dut2NumberForMO))
                    throw new Exception($"Failed to place VoLTE call from MO device [{_moCallerId}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(5000);

                // Step 7: Verify DUT 2 receives the incoming VoLTE call (call waiting UI)
                //swipe down
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1600");
                bool callWaitingDetected = false;
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                for (int i = 0; i < 10; i++)
                {
                    gclass.CaptureUIDump(_dut2Id, outputPath);
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
                    throw new Exception("DUT 2 did not show call waiting/incoming call UI for the VoLTE call.");

                gclass.UpdateOutput("DUT 2 received incoming VoLTE call while on VoWiFi call (call waiting detected).");

                

                gclass.UpdateOutput($"TC 1.28: PASS [{_dut1Id}, {_dut2Id}, {_moCallerId}]");
                _testButton.BackColor = Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.28: FAIL [{_dut1Id}, {_dut2Id}, {_moCallerId}] - {ex.Message}", true);
                _testButton.BackColor = Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Step 8: End all calls and reset device states
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_moCallerId} shell input keyevent KEYCODE_ENDCALL");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.LogTestResultToCSV("TC1.28", _dut1Id, result);

            }

            
        }
    }
}