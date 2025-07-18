using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FIT_Automation.Scripts
{
    public  class GlobalVarClass
    {
        public static string Gdevices = "adb devices";
        public static string MOcallnumber = "6478376636";
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

        public bool WaitForIMSRegisteration()
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                string output = RunAdbCommand("adb shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = RunAdbCommand("adb shell getprop gsm.network.type").ToLower();
                UpdateOutput("Current RAT: " + ratOutput);

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

                    if (block.Contains("T-Mobile IMS"))
                    {
                        targetBlock = block;
                        break;
                    }
                }

                if (targetBlock == null)
                    throw new Exception("No block with mVoiceRegState found.");

                UpdateOutput("Current block: " + targetBlock);

                bool onLte = ratOutput.Contains("lte");
                //bool voiceReady = lowerOutput.Contains("mvoiceregstate=0"); // 0 means voice/VOLTE ready
                //bool dataAttached = lowerOutput.Contains("mdataregstate=0"); // 0 means data attached
                //bool radioIsLte = lowerOutput.Contains("getrilvoiceradiotechnology=14"); // 14 means LTE
                bool imsStatus = lowerOutput.Contains("state: connected"); // true means IMS is registered

                if (onLte && imsStatus)
                    return true;

                UpdateOutput($"Waiting for IMS registration... Attempt {attempt + 1}/{maxAttempts}");
                Thread.Sleep(10000); // Wait for 5 seconds before retrying
                attempt++;
            }

            return false;
        }

        public bool WaitForLTEAndVoLTERegistration()
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {

                string output = RunAdbCommand("adb shell dumpsys telephony.registry");
                string lowerOutput = output.ToLower();

                string ratOutput = RunAdbCommand("adb shell getprop gsm.network.type").ToLower();
                UpdateOutput("Current RAT: " + ratOutput);

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

                UpdateOutput("Current block: " + targetBlock);
                /*
  * mVoiceRegState=0 indicates VOLTE- ready voice 
  * mDataRegState=0 indicates data is attached
  * getRilVoiceRadioTechnology=14 indicates LTE
  */

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

                UpdateOutput($"Waiting for LTE and VoLTE registration... Attempt {attempt + 1}/{maxAttempts}");
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

                _outputRTB.AppendText($"{DateTime.Now}: {message}\n");
                _outputRTB.ScrollToCaret(); // Auto-scroll to the latest message
            }
        }

        public bool IsDeviceConnected()
        {
            string output = RunAdbCommand("adb devices");
            return output.Contains(_deviceId);
        }

        public void SetAirplaneMode(bool enable)
        {
            string state = enable ? "1" : "0";
            RunAdbCommand($"adb shell settings put global airplane_mode_on {state}");
            RunAdbCommand("adb shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + enable);
        }

        public bool IsAPNSet()
        {
            string output = RunAdbCommand("adb shell content query --uri content://telephony/carriers/preferapn");
            return output.Contains("apn");
        }


        public static string Gstring { get; set; }
        public static int Gint { get; set; }


    }
}
