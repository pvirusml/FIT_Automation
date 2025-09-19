using FIT_Automation.Test_Cases;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;


namespace FIT_Automation.Scripts
{
    public  class GlobalVarClass
    {
        public static string Gdevices = "adb devices";
        public static string MOcallnumber = "2069726966";
        public string bit = "";

//        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\FIT_Inventory.mdf;Integrated Security=True";
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PulkitPatel\source\repos\FIT_Automation\FIT_Inventory.mdf;Integrated Security=True";
        private RichTextBox _outputRTB;
        private Button _testButton;
        private string _deviceId;

        public GlobalVarClass(string deviceId, RichTextBox outputRTB, Button testButton)
        {
            _deviceId = deviceId;
            _outputRTB = outputRTB;
            _testButton = testButton;
        }

        public string GetCodeName(string deviceSerial, string prod_name)
        {
            // Run ADB command to get product model
            //string prod_name = RunAdbCommand($"adb -s {deviceSerial} shell getprop ro.product.model").Trim();

            string code_name = null;
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    //MessageBox.Show(prod_name);
                    string query = "SELECT Code_Name FROM Model_Code WHERE Product = @prod_name";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@prod_name", prod_name);
                        object result = cmd.ExecuteScalar();
                        
                        if (result != null)
                        {
                            code_name = result.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            return code_name ?? "Code name not found";
        }

        public bool IsSMSReceived { get; set; }
        public bool IsSMSSent{ get; set; }
        public bool IsMMSSent { get; set; }
        public string ExtractPhoneNumber(string deviceId)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = $"-s {deviceId} shell service call iphonesubinfo 15",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Extract the data within the quotes (') using regex
            Regex regex = new Regex(@"'([^']*)'");
            MatchCollection matches = regex.Matches(output);

            // Join all extracted parts together
            string phoneNumber = "";
            foreach (Match match in matches)
            {
                phoneNumber += match.Groups[1].Value;
            }

            // Remove dots and any unwanted characters
            phoneNumber = phoneNumber.Replace(".", "").Trim();

            return phoneNumber;
        }

        public string RunAdbCommand(string command)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running ADB command: " + ex.Message);
                return string.Empty;
            }
        }

        public string RunAdbroot(string command)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running ADB command: " + ex.Message);
                return string.Empty;
            }
        }

        public string GetPhoneNumber(string Serial, DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Serial"].Value?.ToString() == Serial)
                {
                    return row.Cells["PhoneNumber"].Value?.ToString();
                }
            }
            return null;
        }

        public bool WaitForIMSRegisteration(string deviceId)
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                string output = RunAdbCommand($"adb -s {deviceId} shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = RunAdbCommand($"adb -s {deviceId} shell getprop gsm.network.type").ToLower();
                //UpdateOutput("Current RAT: " + ratOutput);

                // Use regex to match all timestamped blocks
                Regex blockRegex = new Regex(
                    @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+)(.*?)(?=\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}|\z)",
                    RegexOptions.Singleline);

                MatchCollection matches = blockRegex.Matches(output);

                if (matches.Count == 0)
                    throw new Exception("No timestamped blocks found in output.");

                // Find the most recent block that contains "mVoiceRegState"
                string targetBlock = null;

                RegistrationState regState = RegistrationState.GetTelephonyInfo(deviceId);

                if (regState.IMSRegisterationStatus.ToString() == "Registered")
                    return true;


                /*
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    string block = matches[i].Value;
                    string imsRegistertionStatus = Regex.IsMatch(output, @"ims.*?state:\s*CONNECTED", RegexOptions.IgnoreCase)
                   ? "Registered"
                   : "Not Registered";
                    if (block.Contains("T-Mobile IMS"))
                    {
                        targetBlock = block;
                        break;
                    }
                }
                
                if (targetBlock == null)
                    throw new Exception("No block with mVoiceRegState found.");
                */
                //UpdateOutput("Current block: " + targetBlock);

                bool onLte = ratOutput.Contains("lte");
                //bool voiceReady = lowerOutput.Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                //bool dataAttached = lowerOutput.Contains("mdataregstate=0"); // 0 means data attached
                //bool radioIsLte = lowerOutput.Contains("getrilvoiceradiotechnology=14"); // 14 means LTE
                bool imsStatus = lowerOutput.Contains("state: connected"); // true means IMS is registered

                if (onLte && imsStatus)
                    return true;

                //UpdateOutput($"Waiting for IMS registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 5 seconds before retrying
                attempt++;
            }

            return false;
        }

        public bool WaitForLTEAndVoLTERegistration(string deviceId)
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {

                string output = RunAdbCommand($"adb -s {deviceId} shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = RunAdbCommand($"adb  -s {deviceId} shell getprop gsm.network.type").ToLower();
                //UpdateOutput("Current RAT: " + ratOutput);

                // Use regex to match all timestamped blocks
                Regex blockRegex = new Regex(
                    @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+)(.*?)(?=\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}|\z)",
                    RegexOptions.Singleline);

                MatchCollection matches = blockRegex.Matches(output);

                if (matches.Count == 0)
                    throw new Exception("No timestamped blocks found in output.");

                // Find the most recent block that contains "mVoiceRegState"
                string targetBlock = null;

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    string block = matches[i].Value;

                    if (block.Contains("mVoiceRegState"))
                    {
                        targetBlock = block;
                        break;
                    }
                }

                if (targetBlock == null)
                    throw new Exception("No block with mVoiceRegState found.");

                //UpdateOutput("Current block: " + targetBlock);

                bool onLte = ratOutput.Contains("lte");
                bool voiceReady = targetBlock.ToLower().Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                bool dataAttached = targetBlock.ToLower().Contains("mdataregstate=0"); // 0 means data attached
                bool radioIsLte = targetBlock.ToLower().Contains("getrilvoiceradiotechnology=14"); // 14 means LTE
                bool voiceServicesAvaiable = targetBlock.ToLower().Contains("availableservices=[voice,sms,video]"); // true means voice services are available
                bool videoRegistrationAvaialble = targetBlock.ToLower().Contains("mvideoregstate=0"); // 0 means video registration is ready

                if (onLte && voiceReady && dataAttached && radioIsLte && voiceServicesAvaiable && videoRegistrationAvaialble)
                {
                    return true;
                }

                //UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 10 seconds before retrying
                attempt++;
            }

            return false;
        }

        public void UpdateOutput(string message, bool isError = false)
        {
            if (_outputRTB.InvokeRequired)
            {
                _outputRTB.Invoke(new Action(() => UpdateOutput(message, isError)));
            }
            else
            {
                _outputRTB.SelectionColor = isError
                             ? System.Drawing.Color.Red
                             : message.ToLower().Contains("pass") ? System.Drawing.Color.Green : System.Drawing.Color.Black;


                
                if ((message.Contains("Running ") && message.Contains("...")) || message.Contains("Processing test case ID: "))
                    _outputRTB.SelectionColor = System.Drawing.Color.Blue;

                if (!message.Contains("__________________________________________________"))
                {
                    if (message.Contains("Wi-Fi enabled on") || message.Contains("Wi-Fi disabled on") || message.Contains("XCAP/GBA-ME detected in logcat"))
                        message = "";
                    else if(message == "\n")
                        _outputRTB.AppendText("\n");
                    else
                        _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
                }
                else
                {
                    _outputRTB.SelectionColor = System.Drawing.Color.Blue;
                    _outputRTB.AppendText($"{message}\n");
                }

                _outputRTB.ScrollToCaret(); // Auto-scroll to the latest message
            }
        }

        public bool IsDeviceConnected(string deviceId)
        {
            string output = RunAdbCommand($"adb -s {deviceId} devices");
            return output.Contains(_deviceId);
        }

        public void SetAirplaneMode(string deviceId, bool enable)
        {
            string state = enable ? "1" : "0";
            RunAdbCommand($"adb -s {deviceId} shell settings put global airplane_mode_on {state}");
            RunAdbCommand($"adb -s {deviceId} shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
        }

        public bool IsAPNSet(string deviceId)
        {
            string output = RunAdbCommand($"adb -s {deviceId} shell content query --uri content://telephony/carriers/preferapn");
            return output.Contains("apn");
        }

        public void SendSMS(string deviceId, string mtPhoneNumber, string message)
        {
            //string command = $"adb shell am start -a android.intent.action.SENDTO -d sms:{mtPhoneNumber} --es sms_body \"{message}\" ";
            //gclass.RunAdbCommand(command);
            RunAdbCommand($"adb -s {deviceId} shell am start -a android.intent.action.SENDTO -d sms:{mtPhoneNumber} --es sms_body \"{message}\"");

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
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindNodeAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        public void CheckForReceivedSMS(string deviceId,string REFdeviceId)
        {
            int retryCount = 0;
            string targetNumber = ExtractPhoneNumber(deviceId);
            while (retryCount < 10)
            {
                string output = RunAdbCommand($"adb -s {REFdeviceId} shell content query --uri content://sms --projection address,body"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+{targetNumber}";
                string targetBody = "Hello";

                if(targetAddress.Contains("++"))
                   targetAddress = targetAddress.Replace("++", "+");
                

                string expectedRow = $"Row: 0 address={targetAddress}, body={targetBody}";
                if (output.Contains(expectedRow))
                {
                    IsSMSReceived = true;
                    RunAdbCommand($"adb -s {deviceId} shell content delete --uri content://sms");
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
            IsSMSReceived = false;
        }

        public void CheckForSentSMS(string deviceId, string REFdeviceId)
        {
            int retryCount = 0;
            string targetNumber = ExtractPhoneNumber(REFdeviceId);
            while (retryCount < 10)
            {
                string output = RunAdbCommand($"adb -s {deviceId} shell content query --uri content://sms --projection address,body"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+{targetNumber}";
                string targetBody = "Hello";

                if(targetAddress.Contains("++"))
                   targetAddress = targetAddress.Replace("++", "+");


                string expectedRow = $"Row: 0 address={targetAddress}, body={targetBody}";
                if (output.Contains(expectedRow))
                {
                    IsSMSSent = true;
                    RunAdbCommand($"adb -s {deviceId} shell content delete --uri content://sms");
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
            IsSMSSent = false;
        }

        public void SendMMS(string deviceId, string mtPhoneNumber, string message)
        {
            RunAdbCommand($"adb -s {deviceId} shell am start -a android.intent.action.SENDTO -d sms:{mtPhoneNumber} --es sms_body \"{message}\""); Thread.Sleep(3000);
            
            // Click on + attach button
            RunAdbCommand($"adb -s {deviceId} shell input tap 98.5 2198.3"); Thread.Sleep(3000);
            // Click on Gallery option
            RunAdbCommand($"adb -s {deviceId} shell input tap 136.1 1686.5"); Thread.Sleep(3000);
            // Click on center button to take photo
            RunAdbCommand($"adb -s {deviceId} shell input tap 549 1447"); Thread.Sleep(3000);
            // Click on Send mms button
            RunAdbCommand($"adb -s {deviceId} shell input tap 1005 1630"); Thread.Sleep(3000);

            Thread.Sleep(3000);
        }

        public void CheckForSentMMS(string deviceId, string REFdeviceId)
        {
            int retryCount = 0;
            string targetNumber = ExtractPhoneNumber(REFdeviceId);
            while (retryCount < 10)
            {
                string output = RunAdbCommand($"adb -s {deviceId} shell content query --uri content://mms/part --projection text"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+{targetNumber}";
                string targetBody = "MMSTEST";

                string expectedRow = $"text={targetBody}";

                if (output.Contains(expectedRow))
                {
                    IsMMSSent = true;

                    RunAdbCommand($"adb -s {deviceId} shell content delete --uri content://mms");
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
            IsMMSSent = false;
        }


        public void CaptureUIDump(string deviceId, string outputPath)
        {
            string command = $"adb -s {deviceId} shell uiautomator dump /sdcard/ui_dump.xml";
            RunAdbCommand(command);

            Thread.Sleep(2000); // Let the dump finish writing to disk

            string uiDumpPath = Path.Combine(outputPath, "ui_dump.xml");
            string pullCommand = $"adb -s {deviceId} pull /sdcard/ui_dump.xml {uiDumpPath}";
            RunAdbCommand(pullCommand);

            // Validate the root element manually (defensive coding)
            string firstLine = File.ReadLines(uiDumpPath).FirstOrDefault();
            if (firstLine == null || !firstLine.Contains("<?xml"))
                throw new Exception("UI dump file is invalid or missing root element.");
        }

        public static (int, int) FindNodeAndCalculateCenter(string uiDumpPath)
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

        public void SendTap(string deviceId, int x, int y)
        {
            string tapCommand = $"adb -s {deviceId} shell input tap {x} {y}";
            RunAdbCommand(tapCommand);
        }

        public bool ForwardCalls(string serial, string forwardToNumber)
        {
            UpdateOutput($"Forwarding calls on {serial} to {forwardToNumber}...");
            RunAdbCommand($"adb -s {serial} logcat -c"); // Clear logcat for XCAP detection

            string command = $"adb -s {serial} shell am start -a android.intent.action.CALL -d tel:*21*{forwardToNumber}%23";
            string output = RunAdbCommand(command);
            Thread.Sleep(1000);
            RunAdbCommand($"adb -s {serial} shell input keyevent 66"); // Press "Send" key

            Thread.Sleep(5000); // Wait for XCAP to be triggered

            // Check for XCAP/GBA-ME in logcat
            string xcapLog = RunAdbCommand($"adb -s {serial} logcat -d -b radio -v brief");
            if (Regex.IsMatch(xcapLog, @"xcap|gba-me", RegexOptions.IgnoreCase))
            {
                UpdateOutput("XCAP/GBA-ME detected in logcat. CFU set via XCAP.");
                return true;
            }
            else
            {
                UpdateOutput("XCAP/GBA-ME NOT detected in logcat. CFU may not be set via XCAP.", true);
                return false;
            }
        }

        public bool PlaceCall(string serial, string phoneNumber)
        {
            UpdateOutput($"Placing call from {serial} to {phoneNumber}...");
            string command = $"adb -s {serial} shell am start -a android.intent.action.CALL -d tel:{phoneNumber} --ez android.telecom.extra.START_CALL_WITH_SPEAKERPHONE true";
            string output = RunAdbCommand(command);

            if (output.Contains("Error") || string.IsNullOrEmpty(output))
            {
                UpdateOutput($"Failed to place a call from device {serial}.", true);
                return false;
            }
            else
            {
                UpdateOutput($"Call placed successfully from device {serial} to {phoneNumber}.");
                return true;
            }
        }

        public bool IsCallForwardingActive(string serial)
        {
            string command = $"adb -s {serial} shell dumpsys telephony.registry";
            string output = RunAdbCommand(command);
            // gclass.UpdateOutput("Call forwarding status output: " + output);

            // Check if call forwarding is active
            return output.Contains("mCallForwarding=true") || output.Contains("mCallForwardingIndicator=true");
        }

        public void EnableWiFi(string deviceId)
        {
            RunAdbCommand($"adb -s {deviceId} shell svc wifi enable");
            UpdateOutput($"Wi-Fi enabled on {deviceId}", true);
        }

        public void DisableWiFi(string deviceId)
        {
            RunAdbCommand($"adb -s {deviceId} shell svc wifi disable");
            UpdateOutput($"Wi-Fi disabled on {deviceId}", true);
        }

        public void ToggleWiFi(string deviceId, int times, int delayMilliseconds)
        {
            for (int i = 0; i < times; i++)
            {
                EnableWiFi(deviceId);
                Thread.Sleep(delayMilliseconds);
                DisableWiFi(deviceId);
                Thread.Sleep(delayMilliseconds);
            }

            UpdateOutput($"Toggled Wi-Fi {times} times on {deviceId}", true);
        }

        public void LogTestResultToCSV(string testCaseId, string deviceId, string result)
        {
            string csvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestResults.csv");

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string newLine = $"{testCaseId},{deviceId},{timestamp},{result}";

            // Create file with header if not exists
            if (!File.Exists(csvPath))
            {
                File.WriteAllText(csvPath, "TestCaseID,DeviceID,Timestamp,Result\n");
            }

            File.AppendAllText(csvPath, newLine + Environment.NewLine);
        }


        public static string Gstring { get; set; }
        public static int Gint { get; set; }


    }
}
