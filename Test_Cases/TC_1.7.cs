using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_7
    {
        private string _deviceId;
        private string _targetNumber;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string _refDeviceId;
        private string result;

        public TC_1_7(string deviceId, RichTextBox outputRTB, Button testButton, string refDeviceId)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            _refDeviceId = refDeviceId;
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("Starting TC 1.7:Verify MT SMS (on ICS) from another VoLTE device is received using SIP over IMS");
            try
            {
                if (!gclass.IsDeviceConnected(_deviceId) && !gclass.IsDeviceConnected(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF are not connected.");
                    throw new Exception("DUT & REF are not connected.");
                }

                gclass.SetAirplaneMode(_deviceId, true);
                gclass.SetAirplaneMode(_refDeviceId, true);
                gclass.UpdateOutput("Airplane mode enabled for DUT & REF.");
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(_deviceId, false);
                gclass.SetAirplaneMode(_refDeviceId, false);
                gclass.UpdateOutput("Airplane mode disabled for DUT & REF.");
                Thread.Sleep(5000);

                if (!gclass.WaitForLTEAndVoLTERegistration(_deviceId) && !gclass.WaitForLTEAndVoLTERegistration(_refDeviceId))
                {
                    gclass.UpdateOutput("DUT & REF failed to attach to LTE or register for VoLTE.", true);
                    return;
                }

                gclass.UpdateOutput("DUT & REF successfully attached to LTE and registered for VoLTE.");

                // open messages to target number
                //gclass.RunAdbCommand($"adb shell am start -a android.intent.action.SENDTO -d sms:{_targetNumber}");

                // Step: Send SMS
                //string msg = "Hi"; // Message to send
                //gclass.RunAdbCommand($"adb shell am start -a android.intent.action.SENDTO -d sms:{_targetNumber} --es sms_body \"{msg}\"");
                string targetNumber = gclass.ExtractPhoneNumber(_refDeviceId);
                gclass.SendSMS(_deviceId, targetNumber, "Hello");
                gclass.CheckForReceivedSMS(_deviceId, _refDeviceId);

                if (gclass.IsSMSReceived)
                {
                    gclass.UpdateOutput("SMS successfully received. TC 1.6: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("SMS not received. TC 1.6: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.6: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.7", _deviceId, result);
        }

    }
}
