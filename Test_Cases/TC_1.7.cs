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
        private string result;

        public TC_1_7(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
            _targetNumber = "9729274060";//"2069726966"; // Replace with destination VoLTE test number
            gclass = new GlobalVarClass(_deviceId, _outputRTB, _testButton);
        }

        public void RunTest()
        {
            gclass.UpdateOutput("Starting TC 1.7:Verify MT SMS (on ICS) from another VoLTE device is received using SIP over IMS");
            try
            {
                if (!gclass.IsDeviceConnected())
                {
                    gclass.UpdateOutput("Device is not connected.");
                    throw new Exception("Device is not connected.");
                }

                gclass.SetAirplaneMode(true);
                gclass.UpdateOutput("Airplane mode enabled.");
                Thread.Sleep(3000);

                gclass.SetAirplaneMode(false);
                gclass.UpdateOutput("Airplane mode disabled.");
                Thread.Sleep(5000);

                if (!gclass.WaitForLTEAndVoLTERegistration())
                {
                    gclass.UpdateOutput("Device failed to attach to LTE or register for VoLTE.", true);
                    return;
                }

                gclass.UpdateOutput("Device successfully attached to LTE and registered for VoLTE.");

                // open messages to target number
                //gclass.RunAdbCommand($"adb shell am start -a android.intent.action.SENDTO -d sms:{_targetNumber}");

                // Step: Send SMS
                //string msg = "Hi"; // Message to send
                //gclass.RunAdbCommand($"adb shell am start -a android.intent.action.SENDTO -d sms:{_targetNumber} --es sms_body \"{msg}\"");
                SendSMS(_deviceId, _targetNumber, "Hello");
                CheckForReceivedSMS(_deviceId, gclass);

                if (gclass.IsSMSReceived)
                {
                    gclass.UpdateOutput("SMS successfully received. TC 1.7: Pass.");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
                else
                {
                    gclass.UpdateOutput("SMS not received. TC 1.7: Fail.", true);
                    _testButton.BackColor = System.Drawing.Color.Red;
                    result = "FAIL";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput("Exception in TC 1.7: " + ex.Message, true);
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
            }

            gclass.LogTestResultToCSV("TC1.7", _deviceId, result);
        }

        private void SendSMS(string senderSerial, string mtPhoneNumber, string message)
        {
            //string command = $"adb shell am start -a android.intent.action.SENDTO -d sms:{mtPhoneNumber} --es sms_body \"{message}\" ";
            //gclass.RunAdbCommand(command);
            gclass.RunAdbCommand($"adb shell am start -a android.intent.action.SENDTO -d sms:{mtPhoneNumber} --es sms_body \"{message}\"");

            Thread.Sleep(3000);

            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            /*
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            gclass.UpdateOutput($"Capturing UI dump to {uiDumpPath}");
            CaptureUIDump(senderSerial, outputPath);

            var (centerX, centerY) = FindNodeAndCalculateCenter(uiDumpPath);
            SendTap(senderSerial, centerX, centerY);
            */

            Thread.Sleep(5000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(senderSerial, outputPath);
            var (composeX, composeY) = FindNodeAndCalculateCenter(uiDumpPath);
            SendTap(senderSerial, composeX, composeY);

        }

        private void CheckForReceivedSMS(string mtDeviceSerial, GlobalVarClass gclass)
        {
            int retryCount = 0;
            while (retryCount < 10)
            {
                string output = gclass.RunAdbCommand("adb shell content query --uri content://sms --projection address,body"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+1{_targetNumber}";
                string targetBody = "Hello";

                string expectedRow = $"Row: 0 address={targetAddress}, body={targetBody}";
                if (output.Contains(expectedRow))
                {
                    gclass.IsSMSReceived = true;
                    gclass.RunAdbCommand("adb shell content delete --uri content://sms");
                    return;
                }
                /*
                if (output.Contains("Hello") && output.Contains($"address=+1{_targetNumber}"))
                {
                    gclass.IsSMSReceived = true;
                    gclass.RunAdbCommand("adb shell content delete --uri content://sms");
                    return;
                }
                */
                Thread.Sleep(2000);
                retryCount++;
            }
            gclass.IsSMSReceived = false;
        }


        private void CaptureUIDump(string senderSerial, string outputPath)
        {
            string command = "adb shell uiautomator dump /sdcard/ui_dump.xml";
            gclass.RunAdbCommand(command);

            Thread.Sleep(2000); // Let the dump finish writing to disk

            string uiDumpPath = Path.Combine(outputPath, "ui_dump.xml");
            string pullCommand = $"adb pull /sdcard/ui_dump.xml {uiDumpPath}";
            gclass.RunAdbCommand(pullCommand);

            // Validate the root element manually (defensive coding)
            string firstLine = File.ReadLines(uiDumpPath).FirstOrDefault();
            if (firstLine == null || !firstLine.Contains("<?xml"))
                throw new Exception("UI dump file is invalid or missing root element.");
        }

        private static (int, int) FindNodeAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            //XmlNode targetNode = doc.SelectSingleNode("//node[@text='SMS']") ??
            //                     doc.SelectSingleNode("//node[contains(@content-desc, 'Send')]");

            /*
            XmlNode targetNode = doc.SelectSingleNode("//node[@resource-id='com.google.android.apps.messaging:id/send_message_button']")
                     ?? doc.SelectSingleNode("//node[@text='SMS']")
                     ?? doc.SelectSingleNode("//node[contains(@content-desc, 'Send SMS')]");
            */
            //XmlNode targetNode = doc.SelectSingleNode("//node[@class='android.widget.Button' and @clickable='true' and contains(@content-desc, 'Send')]");

            XmlNode targetNode = doc.SelectSingleNode("//node[@content-desc='Send SMS']")
                     ?? doc.SelectSingleNode("//node[contains(@content-desc, 'Send') and @class='android.widget.Button']")
                     ?? doc.SelectSingleNode("//node[@clickable='true' and @bounds='[963,2150][1023,2210]']");


            if (targetNode == null)
                throw new Exception("Neither 'SMS' nor 'Send' node was found in the UI dump.");

            string bounds = targetNode.Attributes["bounds"].Value;
            if (string.IsNullOrEmpty(bounds))
                throw new Exception("Bounds attribute is missing or empty.");

            //string[] coordinates = bounds.Replace("[", "").Replace("]", "").Split(',');
            var match = Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
            if (!match.Success)
                throw new Exception("Invalid bounds format: " + bounds);

            int left = int.Parse(match.Groups[1].Value);
            int top = int.Parse(match.Groups[2].Value);
            int right = int.Parse(match.Groups[3].Value);
            int bottom = int.Parse(match.Groups[4].Value);

            int centerX = (left + right) / 2;
            int centerY = (top + bottom) / 2;
            return (centerX, centerY);
            
            /*
            if (coordinates.Length != 4)
                throw new Exception($"Invalid bounds format: {bounds}");

            int.TryParse(coordinates[0], out int left);
            int.TryParse(coordinates[1], out int top);
            int.TryParse(coordinates[2], out int right);
            int.TryParse(coordinates[3], out int bottom);

            int centerX = (left + right) / 2;
            int centerY = (top + bottom) / 2;
            return (centerX, centerY);
            */
        }

        private void SendTap(string senderSerial, int x, int y)
        {
            string tapCommand = $"adb shell input tap {x} {y}";
            gclass.RunAdbCommand(tapCommand); 
        }

    }
}
