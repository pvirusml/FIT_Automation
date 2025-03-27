using FIT_Automation.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FIT_Automation.Test_Cases
{
    public class SMS
    {
        public static void RunTest(DataGridView DeviceDataGridView, CheckedListBox deviceChkBox, CheckedListBox mtDeviceChkBox)
        {
            GlobalVarClass gclass = new GlobalVarClass();

            if (deviceChkBox.CheckedItems.Count == 0 || mtDeviceChkBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select both sending and receiving devices.");
                return;
            }

            string mtDeviceSerial = mtDeviceChkBox.CheckedItems[0].ToString();
            string mtPhoneNumber = gclass.GetPhoneNumber(mtDeviceSerial, DeviceDataGridView);

            if (string.IsNullOrEmpty(mtPhoneNumber))
            {
                MessageBox.Show("Could not find phone number for the selected MT device.");
                return;
            }
            // Create a list of tasks to run the test cases in parallel
            List<Task> tasks = new List<Task>();

            foreach (var item in deviceChkBox.CheckedItems)
            {
                string senderDeviceSerial = item.ToString();
                tasks.Add(Task.Run(() => RunTestForDevice(senderDeviceSerial, mtPhoneNumber, mtDeviceSerial, gclass)));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());
        }
        private static void RunTestForDevice(string senderDeviceSerial, string mtPhoneNumber, string mtDeviceSerial, GlobalVarClass gclass)
        {
            ToggleAirplaneMode(senderDeviceSerial, true);
            Thread.Sleep(10000);
            ToggleAirplaneMode(senderDeviceSerial, false);
            Thread.Sleep(10000);

            SendSMS(senderDeviceSerial, mtPhoneNumber, "Hello\\ SMS\\ test");
            CheckForReceivedSMS(mtDeviceSerial, gclass);
        }
        //private static string GetPhoneNumber(string Serial, DataGridView dgv)
        //{
        //    foreach (DataGridViewRow row in dgv.Rows)
        //    {
        //        if (row.Cells["Serial"].Value?.ToString() == Serial)
        //        {
        //            return row.Cells["PhoneNumber"].Value?.ToString();
        //        }
        //    }
        //    return null;
        //}

        private static void ToggleAirplaneMode(string serial, bool enable)
        {
            string state = enable ? "1" : "0";
            RunADBCommand(serial, $"shell settings put global airplane_mode_on {state}");
            RunADBCommand(serial, "shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + (enable ? "true" : "false"));
        }

        private static void CaptureUIDump(string senderSerial, string outputPath)
        {
            string command = $"shell uiautomator dump /sdcard/ui_dump.xml";
            RunADBCommand(senderSerial, command);

            // Pull the UI dump file to the specified path
            string pullCommand = $"pull /sdcard/ui_dump.xml {outputPath}";
            RunADBCommand(senderSerial, pullCommand);
        }
        private static (int x, int y) FindNodeAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            // Try to find the node with text "SMS"
            XmlNode targetNode = doc.SelectSingleNode("//node[@text='SMS']");

            // If "SMS" node is not found, try to find the node with content-desc="Send encrypted message as"
            if (targetNode == null)
            {
                Console.WriteLine("SMS node not found. Searching for 'Send encrypted message as'...");
                targetNode = doc.SelectSingleNode("//node[contains(@content-desc, 'Send')]");

                if (targetNode == null)
                {
                    throw new Exception("Neither 'SMS' nor 'Send encrypted message as' node was found in the UI dump.");
                }
            }

            // Extract the bounds attribute
            string bounds = targetNode.Attributes["bounds"]?.Value;
            if (string.IsNullOrEmpty(bounds))
            {
                throw new Exception("Bounds attribute is missing or empty.");
            }

            // Log the bounds value for debugging
            Console.WriteLine($"Bounds Found: {bounds}");

            // Normalize bounds format to ensure four values
            string formattedBounds = bounds.Replace("][", ",").Replace("[", "").Replace("]", "");

            // Split the bounds into coordinates
            string[] coordinates = formattedBounds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (coordinates.Length != 4)
            {
                throw new Exception($"Invalid bounds format: {bounds}. Expected 4 coordinates, found {coordinates.Length}.");
            }

            // Parse coordinates
            if (!int.TryParse(coordinates[0], out int left) ||
                !int.TryParse(coordinates[1], out int top) ||
                !int.TryParse(coordinates[2], out int right) ||
                !int.TryParse(coordinates[3], out int bottom))
            {
                throw new Exception("Failed to parse bounds coordinates.");
            }

            // Log the parsed coordinates for debugging
            Console.WriteLine($"Parsed Coordinates - Left: {left}, Top: {top}, Right: {right}, Bottom: {bottom}");

            // Calculate center point
            int centerX = (left + right) / 2;
            int centerY = (top + bottom) / 2;

            // Log center for debugging
            Console.WriteLine($"Center - X: {centerX}, Y: {centerY}");

            return (centerX, centerY);
        }

        private static void SendTap(string senderSerial, int x, int y)
        {
            string tapCommand = $"shell input tap {x} {y}";
            RunADBCommand(senderSerial, tapCommand);
        }
        private static void SendSMS(string senderSerial, string mtPhonenumber, string message)
        {
            string command = $"shell am start -a android.intent.action.SENDTO -d sms:{mtPhonenumber} --es sms_body \"{message}\" --ez exit_on_sent true";

            // Step 1: Initiate SMS app and keep message ready
            RunADBCommand(senderSerial, command);

            // Step 2: Wait for the UI to load
            Thread.Sleep(3000);

            // Step 3: Capture the UI dump
            string outpathh = @"C:\";
            string uiDumpPath = @"C:\ui_dump.xml";
            CaptureUIDump(senderSerial, outpathh);

            // Step 4: Parse the UI dump and find the center of the SMS node
            var (centerX, centerY) = FindNodeAndCalculateCenter(uiDumpPath);

            // Step 5: Send the tap event
            SendTap(senderSerial, centerX, centerY);
        }
        
        private static void CheckForReceivedSMS(string mtDeviceSerial, GlobalVarClass gclass)
        {
            int retryCount = 0;
            while (retryCount < 10)
            {
                string output = RunADBCommand(mtDeviceSerial, "shell content query --uri content://sms/inbox --projection body");
                if (output.Contains("Hello SMS test"))
                {
                    //MessageBox.Show("MT device received the SMS!");
                    gclass.IsSMSReceived = true;
                    RunADBCommand(mtDeviceSerial, "shell content delete --uri content://sms ");
                    return;
                }
                Thread.Sleep(2000);
                retryCount++;
            }
            MessageBox.Show("SMS was not received within the expected time frame Or user has changed the SMS body.");
            gclass.IsSMSReceived = false;
        }

        private static string RunADBCommand(string serial, string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    //WorkingDirectory = @"C:\\Users\\PulkitPatel\\",
                    Arguments = $"/C adb -s {serial} {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }
}

// Method to send SMS
//private static void SendSMS(string senderSerial, string mtPhonenumber, string message)
//{
//    string command = $"am start -a android.intent.action.SENDTO -d sms:{mtPhonenumber} --es sms_body \"{message}\" --ez exit_on_sent true";

//    // Step 1: Initiate SMS app and keep message ready
//    RunADBCommand(senderSerial, command);

//    // Wait for the SMS app to load
//    Thread.Sleep(3000);

//    // Step 2: Capture the UI Dump
//    RunADBCommand(senderSerial, "uiautomator dump /sdcard/window_dump.xml");
//    RunADBCommand(senderSerial, "pull /sdcard/window_dump.xml .");

//    // Step 3: Parse the UI Dump and Find the Node
//    string bounds = FindNodeBounds("window_dump.xml", "SMS");

//    if (!string.IsNullOrEmpty(bounds))
//    {
//        // Step 4: Calculate the Center Coordinates
//        var center = CalculateCenter(bounds);

//        // Step 5: Send the Tap Command
//        string sendTap = $"input tap {center.X} {center.Y}";
//        RunADBCommand(senderSerial, sendTap);
//    }
//    else
//    {
//        Console.WriteLine("Could not find the SMS node in the UI dump.");
//    }
//}

//private static string FindNodeBounds(string filePath, string nodeText)
//{
//    XmlDocument doc = new XmlDocument();
//    doc.Load(filePath);

//    XmlNodeList nodes = doc.SelectNodes($"//node[@text='{nodeText}']");

//    foreach (XmlNode node in nodes)
//    {
//        if (node.Attributes?["bounds"] != null)
//        {
//            return node.Attributes["bounds"].Value;
//        }
//    }

//    return null;
//}

//private static (int X, int Y) CalculateCenter(string bounds)
//{
//    // bounds format: "[x1,y1][x2,y2]"
//    string[] parts = bounds.Split(new[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);

//    int x1 = int.Parse(parts[0]);
//    int y1 = int.Parse(parts[1]);
//    int x2 = int.Parse(parts[2]);
//    int y2 = int.Parse(parts[3]);

//    int centerX = (x1 + x2) / 2;
//    int centerY = (y1 + y2) / 2;

//    return (centerX, centerY);
//}


//// Method to run ADB commands
//private static void RunADBCommand(string senderSerial, string command)
//{
//    System.Diagnostics.Process process = new System.Diagnostics.Process();
//    process.StartInfo.FileName = "cmd.exe";
//    process.StartInfo.Arguments = $"/c adb -s {senderSerial} shell {command}";
//    process.StartInfo.RedirectStandardOutput = true;
//    process.StartInfo.UseShellExecute = false;
//    process.StartInfo.CreateNoWindow = true;
//    process.Start();

//    string output = process.StandardOutput.ReadToEnd();
//    process.WaitForExit();
//}

//

