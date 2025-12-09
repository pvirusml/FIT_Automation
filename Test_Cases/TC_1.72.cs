/*
* TC_1_72: Verify that there is no impact on Registration when the WI-FI is turned ON
* -----------------------------------------------------
1. Verify device supports both Cellular and WiFi preferred polices.
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
    public class TC_1_72
    {
        private string _dut1Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_72(string dut1Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.72: Verify device supports both Cellular and WiFi preferred polices.");
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
                gclass.SetAirplaneMode(_dut1Id, false);

                //Check Volte Registration
                if(gclass.WaitVoLTERegistration( _dut1Id))
                {
                    gclass.UpdateOutput($"DUT stayed on VoLTE and did not de-register from VoLTE [{_dut1Id}]");
                }
                else
                {
                    throw new Exception($"DUT de-registered from VoLTE after WiFi was turned on [{_dut1Id}]");
                }

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


                gclass.UpdateOutput($"TC 1.72: PASS [{_dut1Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.72: FAIL [{_dut1Id}] - {ex.Message}");
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

            gclass.LogTestResultToCSV("TC1.72", _dut1Id, result);
        }
    }
}