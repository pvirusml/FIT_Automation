/*
* TC_1_54: Verify that there is no impact on Registration when the WI-FI is turned ON
* -----------------------------------------------------
1. Ensure Dut 1 is IMS registered on LTE.
2. Turn on WiFi and connect to WiFi.
3. Ensure the device stays on VoLTE and does not de-register from VoLTE
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
    public class TC_1_54
    {
        private string _dut1Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_54(string dut1Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.54: Verify that there is no impact on Registration when the WI-FI is turned ON.");
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
                Thread.Sleep(20000); // Wait for network stabilization

                if (gclass.CheckIMSRegistrationWithDiagTrace(_dut1Id))
                {
                    gclass.UpdateOutput($"DUT 1 is IMS registered on LTE [{_dut1Id}]");
                }
                else
                {
                    throw new Exception($"DUT 1 failed to IMS register on LTE [{_dut1Id}]");
                }


                // Turn on WiFi
                gclass.EnableWiFi(_dut1Id);
                Thread.Sleep(10000); // Wait for WiFi to connect

                //Check Volte Registration
                if(gclass.WaitVoLTERegistration( _dut1Id))
                {
                    gclass.UpdateOutput($"DUT stayed on VoLTE and did not de-register from VoLTE [{_dut1Id}]");
                }
                else
                {
                    throw new Exception($"DUT de-registered from VoLTE after WiFi was turned on [{_dut1Id}]");
                }


                gclass.UpdateOutput($"TC 1.54: PASS [{_dut1Id}]");
                _testButton.BackColor = System.Drawing.Color.Green;
                result = "PASS";
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.54: FAIL [{_dut1Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {
                //EndCommandResponse Diag Trace
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell pm clear com.tmobile.echolocate");
                // Return to home screen
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");

                // go to home screen
                gclass.resetAll(_dut1Id);
            }
        }
    }
}