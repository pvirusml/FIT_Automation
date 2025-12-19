/*
* TC_1_84: Verify IMS registration by enabling "Wi-Fi Calling" switch with AP oN & SMS and Wi-Fi Calling Check
* -----------------------------------------------------
1)DUT is IMS registered for Voice and other supported services over Wi-Fi
2) Enable AirPlane mode (ON).
3) Enable Wi-Fi (ON).
4) Verify Wi-Fi calling and SMS works.
*/

using FIT_Automation.Scripts;
using OpenQA.Selenium.BiDi.Input;
using OpenQA.Selenium.DevTools.V138.Tracing;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_84
    {
        private string _dut1Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_84(string dut1Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public async Task RunTestAsync()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Verify IMS registration by enabling Wi-Fi Calling switch with AP oN & SMS and Wi-Fi Calling Check");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // 1. Place a call from DUT 2 to DUT 1 and reject the call on DUT 1
                if (!gclass.IsDeviceConnected(_dut1Id))
                {
                    throw new Exception($"DUT 1  is not connected. [{_dut1Id}]");
                }

                

                // Properly handle airplane mode toggle and network stabilization
                gclass.SetAirplaneMode(_dut1Id, true);
                Thread.Sleep(8000);

                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.settings.SETTINGS");
                Thread.Sleep(2000);
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Search settings");
                Thread.Sleep(2000);
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input text \"preferences\"");
                Thread.Sleep(2000);
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Network preferences");
                Thread.Sleep(2000);
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Network preferences");
                Thread.Sleep(2000);
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string uiDumpPath = $"{outputPath}\\ui_dump.xml";
                gclass.CaptureUIDump(_dut1Id, outputPath);
                if (gclass.isInUIDumpWithExc(_dut1Id, uiDumpPath,  "Turn on Wi-Fi automatically"))
                {
                    gclass.UpdateOutput($"DUT supports both Cellular and WiFi preferred polices [{_dut1Id}]");
                }
                else
                {
                    throw new Exception($"DUT does not support both Cellular and WiFi preferred polices [{_dut1Id}]");
                }

                gclass.CaptureUIDump(_dut1Id, outputPath);
                if (gclass.IsInUiDumpBasedOnResourceIdAndIsChecked(uiDumpPath, "com.android.settings:id/switchWidget"))
                {
                    gclass.UpdateOutput($"WiFi preferred policy is used by [{_dut1Id}] by default");
                }
                else
                {
                    throw new Exception($"WiFi preferred policy is not used by [{_dut1Id}] by default");
                }

                gclass.EnableWiFi(_dut1Id);
                Thread.Sleep(10000);

                // Open Dialer
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell am start -a android.intent.action.DIAL");
                Thread.Sleep(2000);
                // swipe down
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 540 0 540 1000");
                Thread.Sleep(2000);
                // Select More Options
                gclass.SelectNodeWithResourceId(_dut1Id, "com.android.dialer:id/main_options_menu_button");
                Thread.Sleep(2000);
                // Click on Settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Settings");
                Thread.Sleep(2000);
                // Click on Call settings
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Calls");
                Thread.Sleep(2000);
                // Click on Wi-Fi Calling
                gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Wi‑Fi Calling");
                Thread.Sleep(2000);
                // Click on Calling preference
                gclass.CaptureUIDump(_dut1Id, outputPath);
                if (gclass.isInUIDumpWithExc(_dut1Id, uiDumpPath, "Ready for calls"))
                {
                    gclass.UpdateOutput($"[{_dut1Id}] has Wi-Fi Calling On");
                }
                else
                {
                    throw new Exception($"[{_dut1Id}] does not have Wi-Fi Calling on");
                }

                // Make a call to 2069726966
                string wifiCallNumber = "2069726966";
                if (!gclass.PlaceCall(_dut1Id, wifiCallNumber))
                {
                    throw new Exception($"Failed to place WiFi call from DUT 1 [{_dut1Id}] to {wifiCallNumber}");
                }

                await Task.Delay(3000); // Wait longer for the call to be registered

                // end call
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                Thread.Sleep(3000);

                // Send sms
                gclass.SendSMS(_dut1Id, wifiCallNumber, "Hello");
                gclass.CheckForSentSMS(_dut1Id, wifiCallNumber);

                gclass.UpdateOutput($"TC 1.84: PASS [{_dut1Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.84: FAIL [{_dut1Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {
                // Return to home screen
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");

                // go to home screen
                gclass.resetAll(_dut1Id);
            }

            gclass.LogTestResultToCSV("TC1.84", _dut1Id, result);
        }
    }
}