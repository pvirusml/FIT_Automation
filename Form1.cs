using FIT_Automation.Scripts;
using FIT_Automation.Test_Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FIT_Automation
{

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }


        //FUNCTION CALLS>>>
        GlobalVarClass gclass = new GlobalVarClass(null, null, null);
        public void PopulateDeviceList()
        {
            try
            {
                // Run ADB command to get device list
                string output = gclass.RunAdbCommand("adb devices");
                gclass.RunAdbroot("adb root");
                
                //Timer to let the devices reconnect as root.
                Thread.Sleep(5000);

                // Clear existing data
                devicechkbxlst.Items.Clear();
                REFchekbx.Items.Clear();
                DeviceDataGridView.Rows.Clear();
                //DeviceContainer.Panel2.OutputRTB.Clear();

                // Split output into lines
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Skip the first line if it contains a header
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // Split the line into parts (assuming the format "device_serial\tdevice_status")
                    string[] parts = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2 && parts[1] == "device")
                    {
                        string deviceSerial = parts[0];

                        //RUN ADB root command
                        gclass.RunAdbroot($"adb -s {deviceSerial} root");

                        // Get the device name
                        string deviceName = gclass.RunAdbCommand($"adb -s {deviceSerial} shell getprop ro.boot.device").Trim();
                        string VONR = gclass.RunAdbCommand($"adb -s {deviceSerial} shell getprop persist.radio.is_vonr_enabled_0").Trim();
                        string prod_name = gclass.RunAdbCommand($"adb -s {deviceSerial} shell getprop ro.product.model").Trim();
                        string phoneNumber = gclass.ExtractPhoneNumber(deviceSerial);
                        string swver = gclass.RunAdbCommand($"adb -s {deviceSerial} shell getprop ro.build.id").Trim();
                        string build = gclass.RunAdbCommand($"adb -s {deviceSerial} shell getprop ro.soc.manufacturer").Trim().Equals("QTI", StringComparison.OrdinalIgnoreCase) ? "Qualcomm" : "Mediatek";
                        string code_name = gclass.GetCodeName(deviceSerial, deviceName);

                        // Add device serial to the checkbox list
                        devicechkbxlst.Items.Add(deviceSerial);

                        // Add device to DataGridView

                        //DeviceDataGridView.Rows.Add(deviceSerial, deviceName, VONR, phoneNumber, code_name, swver, build);
                        int rowIndex = DeviceDataGridView.Rows.Add(deviceSerial, deviceName, VONR, phoneNumber, code_name, swver, build);

                        // Set the background color of the 4th column (phoneNumber) to LightGreen
                        DeviceDataGridView.Rows[rowIndex].Cells[4].Style.BackColor = Color.LimeGreen;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        //public string ExtractPhoneNumber(string deviceId)
        //{
        //    ProcessStartInfo startInfo = new ProcessStartInfo
        //    {
        //        FileName = "adb",
        //        Arguments = $"-s {deviceId} shell service call iphonesubinfo 15",
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    Process process = new Process { StartInfo = startInfo };
        //    process.Start();
        //    string output = process.StandardOutput.ReadToEnd();
        //    process.WaitForExit();

        //    // Extract the data within the quotes (') using regex
        //    Regex regex = new Regex(@"'([^']*)'");
        //    MatchCollection matches = regex.Matches(output);

        //    // Join all extracted parts together
        //    string phoneNumber = "";
        //    foreach (Match match in matches)
        //    {
        //        phoneNumber += match.Groups[1].Value;
        //    }

        //    // Remove dots and any unwanted characters
        //    phoneNumber = phoneNumber.Replace(".", "").Trim();

        //    return phoneNumber;
        //}
        
        
        //public string RunAdbroot(string command)
        //{
        //    try
        //    {
        //        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //        process.StartInfo.FileName = "cmd.exe";
        //        process.StartInfo.Arguments = "/c " + command;
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.Start();

        //        string output = process.StandardOutput.ReadToEnd();
        //        process.WaitForExit();

        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error running ADB command: " + ex.Message);
        //        return string.Empty;
        //    }
        //}


        //public string RunAdbCommand(string command)
        //{
        //    try
        //    {
        //        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //        process.StartInfo.FileName = "cmd.exe";
        //        process.StartInfo.Arguments = "/c " + command;
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.Start();

        //        string output = process.StandardOutput.ReadToEnd();
        //        process.WaitForExit();

        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error running ADB command: " + ex.Message);
        //        return string.Empty;
        //    }
        //}




        //BUTTON CALL EVENTS

        private void PopulateBTN_Click(object sender, EventArgs e)
        {
            //RunAdbCommand("adb devices");
            PopulateDeviceList();
        }


        private void AddMTBTN_Click(object sender, EventArgs e)
        {
            for (int i = devicechkbxlst.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = devicechkbxlst.CheckedItems[i];

                //Add item i to  MT checkbox list
                REFchekbx.Items.Add(item);

                //Remove item from Device checkbox list
                devicechkbxlst.Items.Remove(item);
            }
        }

        // Add to DUT List
        private void RemoveMTBTN_Click(object sender, EventArgs e)
        {
            for (int i = devicechkbxlst.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = devicechkbxlst.CheckedItems[i];

                //Add item i to  MT checkbox list
                DUTchkbx.Items.Add(item);
                //devicechkbxlst.Items.Add(item);

                //Remove item from Device checkbox list
                //REFchekbx.Items.Remove(item);
                devicechkbxlst.Items.Remove(item);
            }
        }

        private void TC1BTN_Click(object sender, EventArgs e)
        {
            volteStatusgrid.Rows.Clear();
            if (volteStatusgrid.Columns.Count == 0)
            {
                volteStatusgrid.Columns.Add("Device", "Device");
                volteStatusgrid.Columns.Add("VoLTEStatus", "VoLTE Status");
                volteStatusgrid.Columns.Add("Network", "Network");
                volteStatusgrid.Columns.Add("Band", "Band");
                volteStatusgrid.Columns.Add("RSRP", "RSRP");
                volteStatusgrid.Columns.Add("DataState", "Data State");
                volteStatusgrid.Columns.Add("Emergency", "Emergency");
                volteStatusgrid.Columns.Add("Roaming", "Roaming");
                volteStatusgrid.Columns.Add("IMSRegisteration", "IMS Registeration");
            }
            foreach (var item in devicechkbxlst.CheckedItems)
            {
                string deviceId = item.ToString();
                RegistrationState state = RegistrationState.GetTelephonyInfo(deviceId);

                if (state != null)
                {
                    // Add row with telephony information
                    volteStatusgrid.Rows.Add(
                        state.DeviceId,
                        state.VoLTEStatus,
                        state.ConnectedNetwork,
                        state.BandInfo,
                        state.RSRP,
                        state.DataState,
                        state.RoamingStatus,
                        state.EmergencyState,
                        state.IMSRegisterationStatus
                    );
                }
                else
                {
                    MessageBox.Show($"Failed to fetch telephony info for device: {deviceId}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TC2BTN_Click_1(object sender, EventArgs e)
        {
            SMS.RunTest(DeviceDataGridView, devicechkbxlst, REFchekbx);
            if (gclass.IsSMSReceived == false)
            {
                tcsmsLBL.Visible = true;
                tcsmsLBL.Text = "FAIL";
                tcsmsLBL.BackColor= Color.Red;
            }
            else
            {
                tcsmsLBL.Visible = true;
                tcsmsLBL.Text = "PASS";
                tcsmsLBL.BackColor = Color.ForestGreen;
            }
        }

        private void TC3BTN_Click_1(object sender, EventArgs e)
        {
            XCAP.RunTest(DeviceDataGridView, devicechkbxlst, REFchekbx);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the selected device
            if(devicechkbxlst.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.3.");
                return;
            }

            string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            TC_1_3 test = new TC_1_3(deviceId, outputRTB);
            test.RunTest();
        }

        private void TC11BTN_Click(object sender, EventArgs e)
        {
            // Get the selected device
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.1.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            TC_1_1 test = new TC_1_1(deviceId, outputRTB, TC11BTN);
            test.RunTest();
        }

        private void TC12BTN_Click(object sender, EventArgs e)
        {
            // Get the selected device
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.2.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            TC_1_2 test = new TC_1_2(deviceId, outputRTB, TC12BTN);
            test.RunTest();

        }

        private void TC14BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.4.");
                return;
            }

            if (REFchekbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a reference device to run TC 1.4.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;

            TC_1_4 test = new TC_1_4(deviceId, outputRTB, TC14BTN, refDeviceId);
            test.RunTest();
        }

        private void DeviceDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void TC15BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.5.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_5 test = new TC_1_5(deviceId, outputRTB, TC15BTN, refDeviceId);
            test.RunTest();
        }

        private void TC16BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.6.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_6 test = new TC_1_6(deviceId, outputRTB, TC16BTN, refDeviceId);
            test.RunTest();
        }

        private void TC17BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.7.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;

            TC_1_7 test = new TC_1_7(deviceId, outputRTB, TC17BTN, refDeviceId);
            test.RunTest();

        }

        private void TC18BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.8.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;

            TC_1_8 test = new TC_1_8(deviceId, refDeviceId, moCallerId, outputRTB, TC18BTN);
            test.RunTest();

        }

        private void REFchekbx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ReturnDUTButton_Click(object sender, EventArgs e)
        {
            for (int i = DUTchkbx.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = DUTchkbx.CheckedItems[i];

                //Add item i to  MT checkbox list
               devicechkbxlst.Items.Add(item);
                //devicechkbxlst.Items.Add(item);

                //Remove item from Device checkbox list
                //REFchekbx.Items.Remove(item);
                DUTchkbx.Items.Remove(item);
            }

        }

        // REF RETURN BUTTON CLICK EVENT
        private void button1_Click_1(object sender, EventArgs e)
        {
            for (int i = REFchekbx.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = REFchekbx.CheckedItems[i];

                //Add item i to  MT checkbox list
                devicechkbxlst.Items.Add(item);
                //devicechkbxlst.Items.Add(item);

                //Remove item from Device checkbox list
                //REFchekbx.Items.Remove(item);
                REFchekbx.Items.Remove(item);
            }
        }

        private void DUTtoREFButton_Click(object sender, EventArgs e)
        {
            for (int i = DUTchkbx.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = DUTchkbx.CheckedItems[i];

                //Add item i to  MT checkbox list
                REFchekbx.Items.Add(item);
                //devicechkbxlst.Items.Add(item);

                //Remove item from Device checkbox list
                //REFchekbx.Items.Remove(item);
                DUTchkbx.Items.Remove(item);
            }

        }

        private void REFtoDUTButton_Click(object sender, EventArgs e)
        {
            for (int i = REFchekbx.CheckedItems.Count - 1; i >= 0; i--)
            {
                object item = REFchekbx.CheckedItems[i];

                //Add item i to  DUT Checkbox list
                DUTchkbx.Items.Add(item);
                //devicechkbxlst.Items.Add(item);

                //Remove item from REF checkbox list
                REFchekbx.Items.Remove(item);
            }
        }

        private void TC110BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.10.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_10 test = new TC_1_10(deviceId, outputRTB, TC110BTN, refDeviceId);
            test.RunTest();

        }

        private void TC111BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.11.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_11 test = new TC_1_11(deviceId, outputRTB, TC111BTN, refDeviceId);
            test.RunTest();

        }

        private void TCGRPBX_Enter(object sender, EventArgs e)
        {

        }

        private void TC112BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.12.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_12 test = new TC_1_12(deviceId, outputRTB, TC112BTN, refDeviceId);
            test.RunTest();

        }

        private void TC113BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.13.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_13 test = new TC_1_13(deviceId, outputRTB, TC113BTN, refDeviceId);
            test.RunTest();

        }

        private void TC114BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.14.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_14 test = new TC_1_14(deviceId, outputRTB, TC114BTN, refDeviceId);
            test.RunTest();
        }

        private void ProcessTCBatchButton_Click(object sender, EventArgs e)
        {
            // Validate DUT selection
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one DUT device.");
                return;
            }

            // Get DUT and REF devices
            string dutDeviceId = DUTchkbx.CheckedItems[0]?.ToString();
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0]?.ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;

            // Dictionary to map each check box to corresponding test case
            var testCases = new Dictionary<CheckBox, string>
            {
                { TC11CheckBox, "TC 1.1" },
                { TC12CheckBox, "TC 1.2" },
                { TC14CheckBox, "TC 1.4" },
                { TC15CheckBox, "TC 1.5" },
                { TC16CheckBox, "TC 1.6" },
                { TC17CheckBox, "TC 1.7" },
                { TC18CheckBox, "TC 1.8" },
                { TC110CheckBox, "TC 1.10" },
                { TC111CheckBox, "TC 1.11" },
                { TC112CheckBox, "TC 1.12" },
                { TC113CheckBox, "TC 1.13" },
                { TC114CheckBox, "TC 1.14"},
                { TC13CheckBox, "TC 1.3" } // 
            };

            // Validate REF selection for tests that require it
            if ((TC14CheckBox.Checked || TC15CheckBox.Checked || TC16CheckBox.Checked || TC17CheckBox.Checked
                || TC18CheckBox.Checked || TC110CheckBox.Checked || TC111CheckBox.Checked || TC112CheckBox.Checked || TC113CheckBox.Checked)
                && string.IsNullOrEmpty(refDeviceId))
            {
                MessageBox.Show("Please select a REF device for tests that require it.");
                return;
            }

            // Validate MO Caller ID for TC 1.8
            if (TC18CheckBox.Checked && string.IsNullOrEmpty(moCallerId))
            {
                MessageBox.Show("Please select a MO Caller ID device for TC 1.8.");
                return;
            }

            foreach (var pair in testCases)
            {
                if (pair.Key.Checked)
                {
                    string testCase = pair.Value;

                    switch (testCase)
                    {
                        case "TC 1.1":
                            new TC_1_1(dutDeviceId, outputRTB, TC11BTN).RunTest();
                            UpdateCheckBoxColor(TC11CheckBox, TC11BTN);
                            break;
                        case "TC 1.2":
                            new TC_1_2(dutDeviceId, outputRTB, TC12BTN).RunTest();
                            UpdateCheckBoxColor(TC12CheckBox, TC12BTN);
                            break;
                        case "TC 1.3":
                            new TC_1_3(dutDeviceId, outputRTB).RunTest();
                            break;
                        case "TC 1.4":
                            new TC_1_4(dutDeviceId, outputRTB, TC14BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC14CheckBox, TC14BTN);
                            break;
                        case "TC 1.5":
                            new TC_1_5(dutDeviceId, outputRTB, TC15BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC15CheckBox, TC15BTN);
                            break;
                        case "TC 1.6":
                            new TC_1_6(dutDeviceId, outputRTB, TC16BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC16CheckBox, TC16BTN);
                            break;
                        case "TC 1.7":
                            new TC_1_7(dutDeviceId, outputRTB, TC17BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC17CheckBox, TC17BTN);
                            break;
                        case "TC 1.8":
                            new TC_1_8(dutDeviceId, refDeviceId, moCallerId, outputRTB, TC18BTN).RunTest();
                            UpdateCheckBoxColor(TC18CheckBox, TC18BTN);
                            break;
                        case "TC 1.10":
                            new TC_1_10(dutDeviceId, outputRTB, TC110BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC110CheckBox, TC110BTN);
                            break;
                        case "TC 1.11":
                            new TC_1_11(dutDeviceId, outputRTB, TC111BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC111CheckBox, TC111BTN);
                            break;
                        case "TC 1.12":
                            new TC_1_12(dutDeviceId, outputRTB, TC112BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC112CheckBox, TC112BTN);
                            break;
                        case "TC 1.13":
                            new TC_1_13(dutDeviceId, outputRTB, TC113BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC113CheckBox, TC113BTN);
                            break;
                        case "TC 1.14":
                            new TC_1_14(dutDeviceId, outputRTB, TC114BTN, refDeviceId).RunTest();
                            UpdateCheckBoxColor(TC114CheckBox, TC114BTN);
                            break;
                        default:
                            MessageBox.Show($"Test case '{testCase}' is not implemented.");
                            break;
                    }

                    // Optional: Add delay if needed
                    // Thread.Sleep(2000);
                }

                void UpdateCheckBoxColor(CheckBox checkBox, Button button)
                {
                    if (button.BackColor == System.Drawing.Color.Green)
                        checkBox.ForeColor = System.Drawing.Color.Green;
                    else if (button.BackColor == System.Drawing.Color.Red)
                        checkBox.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void DUTchkbx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

      
    }
}