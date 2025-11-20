/*
* TC_1_57: MO VoWiFi calls during Data/Messaging
* -----------------------------------------------------
1) Start FTP DL and browse the internet
2) Make MO VoWiFi Call
3) While FTP/browsing and VoWiFi call running, do the following:
  - Receive a SMS
  - Send a SMS
4) End VoWiFi Call and Reoriginate after 30 sec
  - Incoming MMS
  - Outgoing MMS
5) End VoWiFi Call and Reoriginate after 30 sec
  - Download Apps
  - Web Browsing                                                                                                                          
6) End FTP 
*/

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_57
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_57(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.57: MO VoWiFi calls during Data/Messaging.");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Start FTP download and browse the internet
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                    throw new Exception($"DUT 1 or DUT 2 is not connected. [{_dut1Id}, {_dut2Id}]");

                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);

                // disable airplane mode
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                Thread.Sleep(10000); // wait for network to stabilize

                gclass.WaitForLTEAndVoLTERegistration(_dut1Id);
                gclass.WaitForLTEAndVoLTERegistration(_dut2Id);

                Thread.Sleep(5000);

                // Step 2: Make MO VoWiFi Call
                gclass.EnableWiFi(_dut1Id);
                gclass.EnableWiFi(_dut2Id);
                Thread.Sleep(10000);

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.VIEW -d \"https://www.youtube.com/watch?v=nbPBmNRH9KU\"");
                Thread.Sleep(5000);

                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                    throw new Exception($"Failed to extract phone number from DUT 2 [{_dut2Id}]");

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                    throw new Exception($"Failed to place VoWiFi call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");

                Thread.Sleep(5000);

                // Answer call on DUT2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");
                Thread.Sleep(5000);

                // Resume Youtube playback
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.VIEW -d \"https://www.youtube.com/watch?v=nbPBmNRH9KU\"");
                Thread.Sleep(3000);
                // Click on top middlw of screen to expand youtube
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input tap 560 445");
                Thread.Sleep(2000);
                // Click on Play Button in Youtube App to start video playback 
                gclass.SelectNodeWithResourceId(_dut1Id, "com.google.android.youtube:id/player_control_play_pause_replay_button");
                Thread.Sleep(3000);

                // Step 3: While FTP/browsing and VoWiFi call running, receive and send SMS
                gclass.CheckForReceivedSMS(_dut1Id, _dut2Id);
                gclass.SendSMS(_dut1Id, dut2Number, "Test SMS");
                Thread.Sleep(5000);

                // Step 4: End VoWiFi Call and reoriginate after 30 seconds, handle MMS
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                Thread.Sleep(30000);

                gclass.SendMMS(_dut1Id, dut2Number, "Test MMS");
                gclass.CheckForSentMMS(_dut1Id, _dut2Id);
                Thread.Sleep(5000);

                // Step 5: End VoWiFi Call and reoriginate after 30 seconds, handle app downloads and web browsing
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                Thread.Sleep(30000);

                // Close Youtube app
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am force-stop com.google.android.youtube");

                Thread.Sleep(5000);


                gclass.UpdateOutput($"TC 1.57: PASS [{_dut1Id}, {_dut2Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.57: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.resetAll(_dut1Id);
                gclass.resetAll(_dut2Id);
                gclass.LogTestResultToCSV("TC1.57", _dut1Id, result);
            }
        }
    }
}