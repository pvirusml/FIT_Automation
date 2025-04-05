using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace FIT_Automation.Scripts
{
    public  class GlobalVarClass
    {
        public static string Gdevices = "adb devices";
        public static string MOcallnumber = "6478376636";
        public string bit = "";

        // MOS Related properties
        public static double CurrentMOS { get; set; }
        public static CallQualityMetrics CurrentCallMetrics { get; set; }

        // Method to start MOS measurement
        public static void StartMOSMeasurement(string deviceIP = null)
        {
            var mosForm = new MOSForm();
            mosForm.Show();
        }
        //        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\FIT_Inventory.mdf;Integrated Security=True";
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PulkitPatel\source\repos\FIT_Automation\FIT_Inventory.mdf;Integrated Security=True";
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
        public static string Gstring { get; set; }
        public static int Gint { get; set; }


    }
}
