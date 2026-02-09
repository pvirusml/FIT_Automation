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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;


namespace FIT_Automation.Scripts
{
    public class GlobalVarClass
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
        public bool IsSMSSent { get; set; }
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

        public bool CheckIMSRegistrationWithDiagTrace(string deviceId)
        {
            RunAdbCommand($"adb -s {deviceId} shell monkey -p com.tmobile.echolocate -c android.intent.category.LAUNCHER 1");
            Thread.Sleep(3000); // Wait for airplane mode to apply
                                // Click on Accpet and Continue button
            SelectNodeWithTextFromUIDump(deviceId, "Accept and continue");
            Thread.Sleep(2000);

            EnableWiFi(deviceId);
            SelectNodeWithTextFromUIDump(deviceId, "Android Public API");
            Thread.Sleep(2000); 
            //Swipe up
            RunAdbCommand($"adb -s {deviceId} shell input swipe 783 1330 790 362");
            Thread.Sleep(4000);
            string imsRegButton = "IMSREGISTRATIONSTATELISTENER-REGISTER";
            SelectNodeWithTextFromUIDump(deviceId, imsRegButton);
            Thread.Sleep(5000);
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            Thread.Sleep(4000);
            if (!isInUIDump(uiDumpPath, "Registered IMS Registration State Listener. Check logs."))
            {
                return false;
            }
            Thread.Sleep(3000); // Wait for network stabilization

            outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            uiDumpPath = $"{outputPath}\\log_android_api.txt";
            // Look for file This PC\motorola edge (2022)\Internal shared storage\Android\data\com.tmobile.echolocate\cache\dia_debug\log_android_api.txt and search for "IMSRegistrationState: IMS Provider is registered to the IMS network"
            CaptureLogFileFromDevice(deviceId, "Internal shared storage/Android/data/com.tmobile.echolocate/cache/dia_debug/log_android_api.txt", outputPath);
            string logFilePath = $"{outputPath}\\log_android_api.txt";
            if (!isInLogFile(logFilePath, "IMSRegistrationState: IMS Provider is registered to the IMS network"))
            {
                return false;
            }
            Thread.Sleep(3000);

            //EndCommandResponse Diag Trace
            RunAdbCommand($"adb -s {deviceId} shell pm clear com.tmobile.echolocate");

            return true;
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

        public bool WaitVoLTERegistration(string deviceId)
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

                bool voiceReady = targetBlock.ToLower().Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                bool dataAttached = targetBlock.ToLower().Contains("mdataregstate=0"); // 0 means data attached
                bool radioIsLte = targetBlock.ToLower().Contains("getrilvoiceradiotechnology=14"); // 14 means LTE
                bool voiceServicesAvaiable = targetBlock.ToLower().Contains("availableservices=[voice,sms,video]"); // true means voice services are available
                bool videoRegistrationAvaialble = targetBlock.ToLower().Contains("mvideoregstate=0"); // 0 means video registration is ready

                if (voiceReady && dataAttached && radioIsLte && voiceServicesAvaiable && videoRegistrationAvaialble)
                {
                    return true;
                }

                //UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 10 seconds before retrying
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
                    else if (message == "\n")
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

        public void CheckForReceivedSMS(string deviceId, string REFdeviceId)
        {
            int retryCount = 0;
            string targetNumber = ExtractPhoneNumber(deviceId);
            while (retryCount < 10)
            {
                string output = RunAdbCommand($"adb -s {REFdeviceId} shell content query --uri content://sms --projection address,body"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+{targetNumber}";
                string targetBody = "Hello";

                if (targetAddress.Contains("++"))
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

        public void CheckForReceivedSMSWithTargetBodyInputCheck(string deviceId, string REFdeviceId, string targetBody)
        {
            int retryCount = 0;
            string targetNumber = ExtractPhoneNumber(deviceId);
            while (retryCount < 10)
            {
                string output = RunAdbCommand($"adb -s {REFdeviceId} shell content query --uri content://sms --projection address,body"); //("adb shell content query --uri content://sms/inbox --projection address,body");
                string targetAddress = $"+{targetNumber}";

                if (targetAddress.Contains("++"))
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

                if (targetAddress.Contains("++"))
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
            RunAdbCommand($"adb -s {deviceId} shell input tap 98.5 2198.3"); Thread.Sleep(4000);
            //SelectNodeWithTextFromUIDump(deviceId, "+"); Thread.Sleep(3000); 
            // Click on camera
            //RunAdbCommand($"adb -s {deviceId} shell input tap 383 1695.5"); Thread.Sleep(4000);
            SelectNodeWithTextFromUIDump(deviceId, "Camera"); Thread.Sleep(2000); Thread.Sleep(3000);
            // Click on Gallery option
            //RunAdbCommand($"adb -s {deviceId} shell input tap 136.1 1686.5"); Thread.Sleep(3000);
            // Click on center button to take photo
            RunAdbCommand($"adb -s {deviceId} shell input tap 540 1190"); Thread.Sleep(3000);
            // Click on Send mms button
            RunAdbCommand($"adb -s {deviceId} shell input tap 997 1620"); Thread.Sleep(3000);
            //SelectNodeWithTextFromUIDump(deviceId, "MMS");

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

                //string expectedRow = $"text={targetBody}";
                string expectedRow = "text=<smil><head><layout><root-layout/><region id=\"Image\" fit=\"meet\" top=\"0\" left=\"0\" height=\"80%\" width=\"100%\"/><region id=\"Text\" top=\"80%\" left=\"0\" height=\"20%\" width=\"100%\"/></layout></head><body><par dur=\"5000ms\"><img src=\"image000001.jpg\" region=\"Image\" /></par><par dur=\"5000ms\"><text src=\"text000002.txt\" region=\"Text\" /></par></body></smil>";

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

        public void SelectMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(5000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "More options" (3 vertical dots) button
        public static (int, int) FindMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@content-desc='More options']")
                     ?? doc.SelectSingleNode("//node[contains(@resource-id=\"com.android.dialer:id/main_options_menu_button\")");


            if (targetNode == null)
                throw new Exception("No three dotten lines or more options selection found");

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

        public void SelectSettingsInMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindSettingsInMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindSettingsInMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@text='Settings']");


            if (targetNode == null)
                throw new Exception("No Settings option found");

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
        }

        public void SelectCallsInSettingsInMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@text='Calls']");


            if (targetNode == null)
                throw new Exception("No Calls option found");

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
        }

        public void SelectWiFiCallingInCallsInSettingsInMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@text='Wi-Fi Calling']");


            if (targetNode == null)
                throw new Exception("No WiFi Calling option found");

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
        }

        public void SelectReadyForCallsInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindReadyForCallsInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindReadyForCallsInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@text='Ready for calls']");


            if (targetNode == null)
                throw new Exception("No Ready dor calls option found");

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
        }

        public void SelectOffInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerApp(string deviceId)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindOffInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(uiDumpPath);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindOffInWiFiCallingInCallsInSettingsInMoreOptionsOnDialerAppAndCalculateCenter(string uiDumpPath)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode("//node[@text='Off']");


            if (targetNode == null)
                throw new Exception("No Off option found");

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
        }

        public void SelectNodeWithResourceIdFromUIDump(string deviceId, string nodeText)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindStringFromUIDump(uiDumpPath, nodeText);
            SendTap(deviceId, composeX, composeY);

        }

        public void SelectNodeWithTextFromUIDump(string deviceId, string nodeText)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Thread.Sleep(1000); // Give UI time to load

            // Capture UI & find the compose message box
            string uiDumpPath = $"{outputPath}\\ui_dump.xml";
            CaptureUIDump(deviceId, outputPath);
            var (composeX, composeY) = FindStringFromUIDump(uiDumpPath, nodeText);
            SendTap(deviceId, composeX, composeY);

        }

        // Specifically for Dialer app "Settings" option in More options (3 vertical dots) menu
        public static (int, int) FindStringFromUIDump(string uiDumpPath, string nodeText)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            //XmlNode targetNode = doc.SelectSingleNode($"//node[@text='{nodeText}']");
            XmlNode targetNode = doc.SelectSingleNode($"//node[contains(@text, '{nodeText}')]");

            if (targetNode == null)
                throw new Exception($"No {nodeText} found");

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
        }

        public static (int, int) FindResourceIdFromUIDump(string uiDumpPath, string nodeText)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            //XmlNode targetNode = doc.SelectSingleNode($"//node[@text='{nodeText}']");
            XmlNode targetNode = doc.SelectSingleNode($"//node[contains(@resource-id, '{nodeText}')]");

            if (targetNode == null)
                throw new Exception($"No {nodeText} found");

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
        }

        public bool SelectNodeWithResourceId(string deviceId, string resourceId)
        {
            string dumpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ui_dump.xml");
            CaptureUIDump(deviceId, Path.GetDirectoryName(dumpPath));

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(dumpPath);
                XmlNode node = doc.SelectSingleNode($"//node[contains(@resource-id, '{resourceId}')]");
                if (node != null && node.Attributes["bounds"] != null)
                {
                    string bounds = node.Attributes["bounds"].Value;
                    // Example: [868,1537][931,1577]
                    string[] parts = bounds.Split(new[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    int x1 = int.Parse(parts[0]);
                    int y1 = int.Parse(parts[1]);
                    int x2 = int.Parse(parts[2]);
                    int y2 = int.Parse(parts[3]);

                    int centerX = (x1 + x2) / 2;
                    int centerY = (y1 + y2) / 2;

                    RunAdbCommand($"adb -s {deviceId} shell input tap {centerX} {centerY}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                UpdateOutput($"Error selecting node by resource-id: {ex.Message}");
            }

            return false;
        }

        public bool isInContentDescUIDump(string uiDumpPath, string nodeText)
        {
            bool isThere = true;
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode($"//node[@content-desc='{nodeText}']");


            if (targetNode == null)
            {
                isThere = false;
                throw new Exception($"No {nodeText} found");
            }

            string bounds = targetNode.Attributes["bounds"].Value;
            if (string.IsNullOrEmpty(bounds))
                throw new Exception("Bounds attribute is missing or empty.");

            //string[] coordinates = bounds.Replace("[", "").Replace("]", "").Split(',');
            var match = Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
            if (!match.Success)
                throw new Exception("Invalid bounds format: " + bounds);

            return isThere;
        }

        public bool isInUIDump(string uiDumpPath, string nodeText)
        {
            bool isThere = true;
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            XmlNode targetNode = doc.SelectSingleNode($"//node[@text='{nodeText}']");


            if (targetNode == null)
            {
                isThere = false;
                throw new Exception($"No {nodeText} found");
            }

            string bounds = targetNode.Attributes["bounds"].Value;
            if (string.IsNullOrEmpty(bounds))
                throw new Exception("Bounds attribute is missing or empty.");

            //string[] coordinates = bounds.Replace("[", "").Replace("]", "").Split(',');
            var match = Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
            if (!match.Success)
                throw new Exception("Invalid bounds format: " + bounds);

            return isThere;
        }

        public bool isInUIDumpWithExc(string deviceId, string uiDumpPath, string nodeText)
        {
            bool isThere = true;
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);



            XmlNode targetNode = doc.SelectSingleNode($"//node[contains(@text,'{nodeText}')]");


            if (targetNode == null)
            {
                isThere = false;
                return isThere;
            }

            string bounds = targetNode.Attributes["bounds"].Value;
            if (string.IsNullOrEmpty(bounds))
                throw new Exception("Bounds attribute is missing or empty.");

            //string[] coordinates = bounds.Replace("[", "").Replace("]", "").Split(',');
            var match = Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
            if (!match.Success)
                throw new Exception("Invalid bounds format: " + bounds);

            return isThere;
        }

        public bool IsInUiDumpBasedOnResourceIdAndIsChecked(string uiDumpPath, string resourceId)
        {
            var doc = new XmlDocument();
            doc.Load(uiDumpPath);

            // Use an XPath query to find the node with the specific resource-id attribute.
            // The query is resilient to the presence of namespaces.
            XmlNode targetNode = doc.SelectSingleNode($"//*[@resource-id='{resourceId}']");

            if (targetNode == null)
            {
                // If the node is not found, we can't determine its checked status.
                // This might be a scenario where you return false, or handle as an exception.
                // Returning false here means "not present or not checked".
                return false;
            }

            // Attempt to get the 'checked' attribute value.
            XmlAttribute checkedAttr = targetNode.Attributes["checked"];

            if (checkedAttr == null)
            {
                // If the attribute is missing, throw an exception or return false based on requirements.
                throw new Exception("The 'checked' attribute is missing from the target node.");
            }

            // Return the boolean value of the 'checked' attribute.
            // XmlConvert.ToBoolean handles the string "true" and "false".
            return XmlConvert.ToBoolean(checkedAttr.Value);
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

        // Helper: Wait for "Call failure" notification in UI dump
        public bool WaitForCallFailureNotification(string deviceId, int timeoutSeconds)
        {
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            for (int i = 0; i < timeoutSeconds; i++)
            {
                Thread.Sleep(1000);
                CaptureUIDump(deviceId, outputPath);
                var doc = new System.Xml.XmlDocument();
                string uiDumpPath = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                try
                {
                    doc.Load(uiDumpPath);
                    var failNode =
                        doc.SelectSingleNode("//node[contains(@text, 'Call ended')]") ??
                        doc.SelectSingleNode("//node[contains(@text, 'Call failure')]") ??
                        doc.SelectSingleNode("//node[contains(@content-desc, 'Call failure')]") ??
                        doc.SelectSingleNode("//node[contains(@text, 'Call ended')]") ??
                        doc.SelectSingleNode("//node[contains(@content-desc, 'Call ended')]") ??
                    doc.SelectSingleNode("//node[contains(@text, 'Mobile Network is not available')]") ??
                        doc.SelectSingleNode("//node[contains(@content-desc, 'Mobile Network is not available')]");
                    if (failNode != null)
                        return true;
                }
                catch { /* ignore parse errors, try again */ }
            }
            return false;
        }


        public static string Gstring { get; set; }
        public static int Gint { get; set; }

        public void resetAll(string deviceId)
        {
            // put all devices in airplane mode
            SetAirplaneMode(deviceId, true);
            DisableWiFi(deviceId);
            // go to home screen
            RunAdbCommand($"adb -s {deviceId} shell input keyevent KEYCODE_HOME");

        }

        public void CloseYouTubeVideoBrowser(string browserProcessName)
        {
            // Check for the browser process and kill it
            try
            {
                foreach (var process in Process.GetProcessesByName(browserProcessName))
                {
                    process.Kill();
                    UpdateOutput($"Closed browser process: {browserProcessName}");
                }
            }
            catch (Exception ex)
            {
                UpdateOutput($"Error closing browser process: {ex.Message}");
            }
        }

        public void CaptureLogFileFromDevice(string deviceId, string deviceFilePath, string outputPath)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
            }

            if (string.IsNullOrEmpty(deviceFilePath))
            {
                throw new ArgumentException("Device file path cannot be null or empty.", nameof(deviceFilePath));
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));
            }

            // Construct the ADB pull command to copy the file from the device to the local machine.
            string command = $"adb -s {deviceId} pull \"{deviceFilePath}\" \"{outputPath}\"";

            // Execute the command.
            string result = RunAdbCommand(command);

            // Check if the command execution was successful.
            if (string.IsNullOrWhiteSpace(result) || result.Contains("failed"))
            {
                throw new Exception($"Failed to capture log file from device. Command: {command}, Result: {result}");
            }
        }
        public bool isInLogFile(string logFilePath, string searchText)
        {
            if (string.IsNullOrEmpty(logFilePath) || string.IsNullOrEmpty(searchText))
            {
                throw new ArgumentException("Log file path and search text cannot be null or empty.");
            }

            if (!File.Exists(logFilePath))
            {
                throw new FileNotFoundException($"The log file at path '{logFilePath}' does not exist.");
            }

            string fileContent = File.ReadAllText(logFilePath);
            return fileContent.Contains(searchText);
        }
    

    }
}
