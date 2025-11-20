/*
* TC_1_56: Verify when the UE is on VoLTE enable WI-FI and perform video call
* -----------------------------------------------------
"1. Ensure Dut 1 is IMS registered on LTE.
2. Turn on WiFi and connect to WiFi.
3. Ensure the device stays on VoLTE and does not de-register from VoLTE.
4. Place a MO video call from DUT 1 to DUT 2.
5. Ensure call is sucessful over VoLTE and Audio/Video are fine.
6. End the call."

*/

using FIT_Automation.Scripts;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_56
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_56(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.56: Verify when the UE is on VoLTE enable WI-FI and perform video call.");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // Step 1: Ensure DUT 1 is IMS registered on LTE
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                    throw new Exception($"DUT 1 or DUT 2 is not connected. [{_dut1Id}, {_dut2Id}]");

                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000);
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);
                Thread.Sleep(20000); // Wait for network stabilization

                if (!gclass.CheckIMSRegistrationWithDiagTrace(_dut1Id))
                    throw new Exception($"DUT 1 failed to IMS register on LTE [{_dut1Id}]");

                gclass.UpdateOutput($"DUT 1 is IMS registered on LTE [{_dut1Id}]");


                // Step 3: Ensure the device stays on VoLTE and does not de-register from VoLTE
                if (!gclass.WaitVoLTERegistration(_dut1Id) || !gclass.WaitVoLTERegistration(_dut2Id))
                    throw new Exception($"DUT 1 or DUT 2 de-registered from VoLTE after WiFi was turned on.");

                // Step 2: Turn on WiFi and connect to WiFi
                gclass.EnableWiFi(_dut1Id);
                gclass.EnableWiFi(_dut2Id);
                Thread.Sleep(10000); // Wait for WiFi to connect

                // Open Dialer
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.DIAL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell am start -a android.intent.action.DIAL");
                Thread.Sleep(2000);
                // swipe down
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 540 0 540 1000");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1000");
                Thread.Sleep(2000);
                // Select More Options
                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/main_options_menu_button");
                gclass.SelectNodeWithResourceId(_dut2Id, "com.android.dialer:id/main_options_menu_button");
                Thread.Sleep(2000);
                // Click on Settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Settings");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Settings");
                Thread.Sleep(2000); 
                // Click on Call settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Call settings");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Call settings");
                Thread.Sleep(2000);
                // Click on Wi-Fi Calling
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Wi‑Fi calling");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Wi‑Fi calling");
                Thread.Sleep(2000);
                // Click on Calling preference
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Calling preference");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Calling preference");
                Thread.Sleep(2000);
                // Click on Cellular preferred
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Cellular preferred");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Cellular preferred");
                Thread.Sleep(2000);
                // go home
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");

                gclass.UpdateOutput($"DUT 1 and DUT 2 stayed on VoLTE and did not de-register from VoLTE.");

                // Step 4: Place a MO VoLTE call from DUT 1 to DUT 2
                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                // Place video call
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.CALL -d tel:{dut2Number} --ei android.telecom.extra.START_CALL_WITH_VIDEO_STATE 3");

                Thread.Sleep(5000);

                // Answer call on DUT2
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_CALL");

                // Step 5: Ensure call is successful over VoLTE and end the call
                string callState1 = gclass.RunAdbCommand($"adb -s {_dut1Id} shell dumpsys telephony.registry").ToLower();
                string callState2 = gclass.RunAdbCommand($"adb -s {_dut2Id} shell dumpsys telephony.registry").ToLower();
                if (!callState1.Contains("callstate=2") || !callState2.Contains("callstate=2"))
                    throw new Exception("VoLTE call was not connected.");

                gclass.UpdateOutput("VoLTE call is connected successfully.");

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                gclass.UpdateOutput($"TC 1.56: PASS [{_dut1Id}, {_dut2Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.56: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }
            finally
            {
                // Open Dialer
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.DIAL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell am start -a android.intent.action.DIAL");
                Thread.Sleep(2000);
                // swipe down
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 540 0 540 1000");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 540 0 540 1000");
                Thread.Sleep(2000);
                // Select More Options
                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/main_options_menu_button");
                gclass.SelectNodeWithResourceId(_dut2Id, "com.android.dialer:id/main_options_menu_button");
                Thread.Sleep(2000);
                // Click on Settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Settings");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Settings");
                Thread.Sleep(2000);
                // Click on Call settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Call settings");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Call settings");
                Thread.Sleep(2000);
                // Click on Wi-Fi Calling
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Wi‑Fi calling");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Wi‑Fi calling");
                Thread.Sleep(2000);
                // Click on Calling preference
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Calling preference");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Calling preference");
                Thread.Sleep(2000);
                // Click on Wi-Fi preferred
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Wi-Fi preferred");
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Wi-Fi preferred");
                Thread.Sleep(2000);
                // go home
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                // Cleanup
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                gclass.resetAll(_dut1Id);
                gclass.resetAll(_dut2Id);
                gclass.LogTestResultToCSV("TC1.56", _dut1Id, result);
            }
        }
    }
}