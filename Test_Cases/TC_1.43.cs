/*
 * TC_1_43: Video Call Forwarding Downgrade to Audio Test Case
 * -----------------------------------------------------------
 * Purpose:
 *   Verify that when call VoWiFi call is made between two devices going from audio to video to audio and the audio call is maintained for 1 minute.
 * 
 * Steps:
 *1) Place a Video call from DUT 1 to DUT 2
2) Ensure video call is connected, audio and video is ok
3) Switch call to audio, verify Audio is Okay.
4) Upgrade call to Video and Verify the Audio and Video is Okay.
5) Maintain the call for 1min and end the call
 */

using FIT_Automation.Scripts;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_43
    {
        private string _dut1Id;
        private string _dut2Id;
        private RichTextBox _outputRTB;
        private Button _testButton;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false; // Static flag to ensure header is logged only once
        private static readonly object _lockObject = new object();


        public TC_1_43(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                // Ensure thread-safe logging of header
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.43: Video Call Forwarding Downgrade to Audio to Upgrade back to video...");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Check all devices are connected
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                    throw new Exception($"One or more devices are not connected. [{_dut1Id}, {_dut2Id}]");

                // Step 2: Set Airplane mode ON, then OFF for DUT1 and DUT2
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);

                // Step 3: Wait for LTE/VoLTE and IMS registration
                if (!gclass.WaitForLTEAndVoLTERegistration(_dut1Id) || !gclass.WaitForLTEAndVoLTERegistration(_dut2Id))
                    throw new Exception($"DUT1 or DUT2 failed to attach to LTE or register for VoLTE. [{_dut1Id}, {_dut2Id}]");

                if (!gclass.WaitForIMSRegisteration(_dut1Id) || !gclass.WaitForIMSRegisteration(_dut2Id))
                    throw new Exception($"DUT1 or DUT2 failed to register for IMS. [{_dut1Id}, {_dut2Id}]");

                gclass.EnableWiFi(_dut1Id);

                Thread.Sleep(21000);


                // Step 5: Place a MO video call from DUT 1 to DUT 2
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT2 [{_dut2Id}]");

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.CALL -d tel:{dut2Number} --ei android.telecom.extra.START_CALL_WITH_VIDEO_STATE 3");
                Thread.Sleep(5000);

                // swipe down
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1600");
                Thread.Sleep(3000);
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Video");

                Thread.Sleep(3000);
                //gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Ongoing call");
                //swipe up
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 612 2175 615 530");
                Thread.Sleep(5000);

                // tap screen in the middle 
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 540 960");
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 540 960");

                // Check sms and delete
                //Thread.Sleep(10000);
                //gclass.CheckForReceivedSMS(_dut1Id, _dut2Id);

                Thread.Sleep(1500);
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 214 1700");
                gclass.SelectNodeWithResourceId(_dut2Id, "com.android.dialer:id/videocall_mute_button");
                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/videocall_mute_button");
                gclass.SelectNodeWithResourceId(_dut2Id, "com.android.dialer:id/videocall_mute_video");
                Thread.Sleep(3000);

                // 

                for (int i = 0; i < 40; i++)
                {
                    string dumpPath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                    gclass.CaptureUIDump(_dut2Id, Path.GetDirectoryName(dumpPath1));

                    if (gclass.isInUIDumpWithExc(_dut2Id, dumpPath1, "VOICE ONLY"))
                    {
                        gclass.SelectNodeWithResourceId(_dut2Id, "com.android.dialer:id/button_2");
                        //gclass.SelectNodeWithTextFromUIDump(_dut2Id, "VOICE ONLY");
                        break;
                    }
                    Thread.Sleep(3000);
                }
                Thread.Sleep(39000);

                // Step 6: Ensure the call is downgraded and connected as audio
                string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
                gclass.CaptureUIDump(_dut2Id, Path.GetDirectoryName(dumpPath));
                bool isAudioCall = gclass.isInUIDumpWithExc(_dut2Id, dumpPath, "Video call");
                //bool isAudioCall = gclass.isInContentDescUIDump(dumpPath,"Video call"); 
                if (!isAudioCall)
                    throw new Exception($"Call was not downgraded to audio. [{_dut1Id}, {_dut2Id}]");
                else
                {
                    gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Video call");
                }

                Thread.Sleep(5000);
                // Swipe up to respond to video call upgrade
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 612 2175 615 530");
                Thread.Sleep(3000);

                // Step 7: Ensure audio is OK (call is connected and not dropped)
                bool callStillActive = true;
                int duration = 30;
                for (int i = 0; i < duration; i++)
                {
                    string output = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                    if (!output.Contains("callstate=2"))
                    {
                        callStillActive = false;
                        gclass.UpdateOutput($"TC 1.41: FAIL [{_dut1Id}, {_dut2Id}] - Call dropped early at {i} seconds.", true);
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
                    gclass.UpdateOutput($"TC 1.43: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput($"TC 1.43: FAIL [{_dut1Id}, {_dut2Id}]", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }


            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.43: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}", true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                Thread.Sleep(3000);
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input tap 567 1356"); // Press Ok
                // Ensure all calls are ended and airplane mode is set
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }

            // Log footer ONCE
            //gclass.UpdateOutput("\n__________________________________________________\n");
            gclass.LogTestResultToCSV("TC1.43", _dut1Id, result);
        }
    }
}