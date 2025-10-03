using ExcelDataReader;
using FIT_Automation.Scripts;
using FIT_Automation.Test_Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
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
        private readonly System.Windows.Forms.Timer _netRefreshTimer = new System.Windows.Forms.Timer();
        // Replace the following line:
        // private readonly System.Windows.Forms.Timer _netTimer = new() { Interval = 5000 }; // 5 seconds interval

        // With this line to ensure compatibility with C# 7.3:
        private readonly System.Windows.Forms.Timer _netTimer = new System.Windows.Forms.Timer { Interval = 2000 }; // 5 seconds interval
        private CancellationTokenSource _runCts;
        private bool _isRunningBatch = false;
        // Replace the following line:
        // private readonly Dictionary<string, RegistrationState> _networkInfoCache = new();

        // With this line to ensure compatibility with C# 7.3:
        private readonly Dictionary<string, RegistrationState> _networkInfoCache = new Dictionary<string, RegistrationState>();
        // Map your grid columns. Adjust indexes to match your DataGridView.
        private enum Col
        {
            DeviceId = 0,
            VoLTEStatus,
            ConnectedNetwork,
            BandInfo,
            RATStatus,
            RSRP,
            RSRQ,
            SINR,
            IMSRegistrationStatus,
            DataState,
            RoamingStatus,
            EmergencyState
        }

        public MainForm()
        {
            InitializeComponent();
            gclass = new GlobalVarClass(null, outputRTB, null);
            //networkUpdateTimer = new System.Windows.Forms.Timer();
            //networkUpdateTimer.Interval = 5000; // 5 secs
            //_netRefreshTimer.Interval = 5000;
            //_netRefreshTimer.Tick += async (s, e) => await RefreshNetworkInfoDiffAsync();
            _netTimer.Tick += async (s, e) => await RefreshNetworkInfoDiffAsync();
            // networkUpdateTimer.Tick += NetworkUpdateTimer_Tick;
            volteStatusgrid.RowHeadersVisible = false;
            volteStatusgrid.Font = new Font("Tahoma", 7); // Set font and size
            DeviceDataGridView.Font = new Font("Tahoma", 7); // Set font and size
            _netTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _netTimer.Stop();
            _netRefreshTimer.Stop();
            //networkUpdateTimer.Stop();
            _runCts?.Cancel();
        }


        //FUNCTION CALLS>>>
        GlobalVarClass gclass;
        System.Windows.Forms.Timer networkUpdateTimer;
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

                        //networkUpdateTimer.Start();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //BUTTON CALL EVENTS

        private void PopulateBTN_Click(object sender, EventArgs e)
        {
            //RunAdbCommand("adb devices");
            // Clear all lists and grids before populating
            devicechkbxlst.Items.Clear();
            REFchekbx.Items.Clear();
            DUTchkbx.Items.Clear();
            DeviceDataGridView.Rows.Clear();
            volteStatusgrid.Rows.Clear();
            _networkInfoCache.Clear();
            gclass.IsSMSReceived = false;
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
                tcsmsLBL.BackColor = Color.Red;
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
            if (devicechkbxlst.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.3.");
                return;
            }

            string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            TC_1_3 test = new TC_1_3(deviceId, outputRTB);
            test.RunTest();
        }

        #region Test Case Buttons
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


        private void TC115BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.15.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_15 test = new TC_1_15(deviceId, outputRTB, TC115BTN, refDeviceId);
            test.RunTest();
        }

        private void TC116BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.16.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_16 test = new TC_1_16(deviceId, outputRTB, TC116BTN, refDeviceId);
            test.RunTest();
        }

        private void TC117BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.17.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_17 test = new TC_1_17(deviceId, refDeviceId, moCallerId, outputRTB, TC117BTN);
            test.RunTest();
        }

        private void TC118BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.18.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_18 test = new TC_1_18(deviceId, outputRTB, TC118BTN, refDeviceId);
            test.RunTest();
        }

        private void TC119BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.19.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_19 test = new TC_1_19(deviceId, outputRTB, TC119BTN, refDeviceId);
            test.RunTest();
        }

        private void TC120BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.20.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_20 test = new TC_1_20(deviceId, outputRTB, TC120BTN, refDeviceId);
            test.RunTest();
        }

        private void TC121BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.21.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_21 test = new TC_1_21(deviceId, outputRTB, TC121BTN, refDeviceId);
            test.RunTest();
        }

        private void TC122BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.22.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_22 test = new TC_1_22(deviceId, refDeviceId, moCallerId, outputRTB, TC122BTN);
            test.RunTest();
        }

        private void TC123BTN_Click(object sender, EventArgs e)
        {
            // Get the selected device
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.23.");
                return;
            }

            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            TC_1_23 test = new TC_1_23(deviceId, outputRTB, TC123BTN);
            test.RunTest();
        }

        private void TC124BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.24.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_24 test = new TC_1_24(deviceId, refDeviceId, moCallerId, outputRTB, TC124BTN);
            test.RunTest();
        }


        private void TC125BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.25.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_25 test = new TC_1_25(deviceId, outputRTB, TC125BTN, refDeviceId);
                test.RunTest();
        }

        private void TC126BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.26.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_26 test = new TC_1_26(deviceId, outputRTB, TC126BTN, refDeviceId);
            test.RunTest();

        }

        private void TC127BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.27.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_27 test = new TC_1_27(deviceId, outputRTB, TC127BTN, refDeviceId);
                test.RunTest();

        }

        private void TC128BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.28.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_28 test = new TC_1_28(deviceId, refDeviceId, moCallerId, outputRTB, TC128BTN);
            test.RunTest();

        }

        private void TC129BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.29.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_29 test = new TC_1_29(deviceId, refDeviceId, moCallerId, outputRTB, TC129BTN);
            test.RunTest();
        }

        private void TC130BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.30.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_30 test = new TC_1_30(deviceId, refDeviceId, outputRTB, TC130BTN);
            test.RunTestAsync();
        }

        #endregion

        #region Switching between Lists
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

        #endregion

        private void DeviceDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void REFchekbx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void ProcessTCBatchButton_Click(object sender, EventArgs e)
        {
            if (_isRunningBatch)
                return;
            _isRunningBatch = true;
            _runCts = new CancellationTokenSource();
            _netRefreshTimer.Start();

            try
            {
                // Validate DUT selection
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select at least one DUT device.");
                    return;
                }

                // Gather device lists
                var dutDevices = DUTchkbx.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();
                var refDevices = REFchekbx.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();
                var moDevices = devicechkbxlst.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();

                // List of test cases to run, in order
                var testCases = new List<string>();
                if (TC11CheckBox.Checked) testCases.Add("TC 1.1");
                if (TC12CheckBox.Checked) testCases.Add("TC 1.2");
                if (TC13CheckBox.Checked) testCases.Add("TC 1.3");
                if (TC123CheckBox.Checked) testCases.Add("TC 1.23");
                if (TC14CheckBox.Checked) testCases.Add("TC 1.4");
                if (TC15CheckBox.Checked) testCases.Add("TC 1.5");
                if (TC16CheckBox.Checked) testCases.Add("TC 1.6");
                if (TC17CheckBox.Checked) testCases.Add("TC 1.7");
                if (TC18CheckBox.Checked) testCases.Add("TC 1.8");
                if (TC110CheckBox.Checked) testCases.Add("TC 1.10");
                if (TC111CheckBox.Checked) testCases.Add("TC 1.11");
                if (TC112CheckBox.Checked) testCases.Add("TC 1.12");
                if (TC113CheckBox.Checked) testCases.Add("TC 1.13");
                if (TC114CheckBox.Checked) testCases.Add("TC 1.14");
                if (TC115CheckBox.Checked) testCases.Add("TC 1.15");
                if (TC116CheckBox.Checked) testCases.Add("TC 1.16");
                if (TC117CheckBox.Checked) testCases.Add("TC 1.17");
                if (TC118CheckBox.Checked) testCases.Add("TC 1.18");
                if (TC119CheckBox.Checked) testCases.Add("TC 1.19");
                if (TC120CheckBox.Checked) testCases.Add("TC 1.20");
                if (TC121CheckBox.Checked) testCases.Add("TC 1.21");
                if (TC122CheckBox.Checked) testCases.Add("TC 1.22");
                if (TC124CheckBox.Checked) testCases.Add("TC 1.24");
                if (TC125CheckBox.Checked) testCases.Add("TC 1.25");
                if (TC126CheckBox.Checked) testCases.Add("TC 1.26");
                if (TC127CheckBox.Checked) testCases.Add("TC 1.27");
                if (TC128CheckBox.Checked) testCases.Add("TC 1.28");
                if (TC129CheckBox.Checked) testCases.Add("TC 1.29");
                if (TC130CheckBox.Checked) testCases.Add("TC 1.30");

                foreach (var testCase in testCases)
                {
                    var tasks = new List<Task>();

                    // DUT-only test cases
                    if (testCase == "TC 1.1" || testCase == "TC 1.2" || testCase == "TC 1.3" || testCase == "TC 1.23")
                    {
                        foreach (var dut in dutDevices)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.1":
                                        new TC_1_1(dut, outputRTB, TC11BTN).RunTest();
                                        UpdateCheckBoxColor(TC11CheckBox, TC11BTN);
                                        break;
                                    case "TC 1.2":
                                        new TC_1_2(dut, outputRTB, TC12BTN).RunTest();
                                        UpdateCheckBoxColor(TC12CheckBox, TC12BTN);
                                        break;
                                    case "TC 1.3":
                                        new TC_1_3(dut, outputRTB).RunTest();
                                        break;
                                    case "TC 1.23":
                                        new TC_1_23(dut, outputRTB, TC123BTN).RunTest();
                                        UpdateCheckBoxColor(TC123CheckBox, TC123BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF paired test cases
                    else if (new[] {
                "TC 1.4","TC 1.5","TC 1.6","TC 1.7","TC 1.10","TC 1.11","TC 1.12","TC 1.13","TC 1.14","TC 1.15",
                "TC 1.16","TC 1.18","TC 1.19","TC 1.20","TC 1.21", "TC 1.25", "TC 1.26", "TC 1.27", "TC 1.30"
            }.Contains(testCase))
                    {
                        int pairCount = Math.Min(dutDevices.Count, refDevices.Count);
                        if (pairCount == 0)
                        {
                            MessageBox.Show($"Please select matching DUT and REF devices for {testCase}.");
                            continue;
                        }
                        for (int i = 0; i < pairCount; i++)
                        {
                            var dut = dutDevices[i];
                            var refDev = refDevices[i];
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.4":
                                        new TC_1_4(dut, outputRTB, TC14BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC14CheckBox, TC14BTN);
                                        break;
                                    case "TC 1.5":
                                        new TC_1_5(dut, outputRTB, TC15BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC15CheckBox, TC15BTN);
                                        break;
                                    case "TC 1.6":
                                        new TC_1_6(dut, outputRTB, TC16BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC16CheckBox, TC16BTN);
                                        break;
                                    case "TC 1.7":
                                        new TC_1_7(dut, outputRTB, TC17BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC17CheckBox, TC17BTN);
                                        break;
                                    case "TC 1.10":
                                        new TC_1_10(dut, outputRTB, TC110BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC110CheckBox, TC110BTN);
                                        break;
                                    case "TC 1.11":
                                        new TC_1_11(dut, outputRTB, TC111BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC111CheckBox, TC111BTN);
                                        break;
                                    case "TC 1.12":
                                        new TC_1_12(dut, outputRTB, TC112BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC112CheckBox, TC112BTN);
                                        break;
                                    case "TC 1.13":
                                        new TC_1_13(dut, outputRTB, TC113BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC113CheckBox, TC113BTN);
                                        break;
                                    case "TC 1.14":
                                        new TC_1_14(dut, outputRTB, TC114BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC114CheckBox, TC114BTN);
                                        break;
                                    case "TC 1.15":
                                        new TC_1_15(dut, outputRTB, TC115BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC115CheckBox, TC115BTN);
                                        break;
                                    case "TC 1.16":
                                        new TC_1_16(dut, outputRTB, TC116BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC116CheckBox, TC116BTN);
                                        break;
                                    case "TC 1.18":
                                        new TC_1_18(dut, outputRTB, TC118BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC118CheckBox, TC118BTN);
                                        break;
                                    case "TC 1.19":
                                        new TC_1_19(dut, outputRTB, TC119BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC119CheckBox, TC119BTN);
                                        break;
                                    case "TC 1.20":
                                        new TC_1_20(dut, outputRTB, TC120BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC120CheckBox, TC120BTN);
                                        break;
                                    case "TC 1.21":
                                        new TC_1_21(dut, outputRTB, TC121BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC121CheckBox, TC121BTN);
                                        break;
                                    case "TC 1.25":
                                        new TC_1_25(dut, outputRTB, TC125BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC125CheckBox, TC125BTN);
                                        break;
                                    case "TC 1.26":
                                        new TC_1_26(dut, outputRTB, TC126BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC126CheckBox, TC126BTN);
                                        break;
                                    case "TC 1.27":
                                        new TC_1_27(dut, outputRTB, TC127BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC127CheckBox, TC127BTN);
                                        break;
                                    case "TC 1.30":
                                        new TC_1_30(dut, refDev, outputRTB, TC130BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC130CheckBox, TC130BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF/MO paired test cases
                    else if (new[] { "TC 1.8", "TC 1.17", "TC 1.22", "TC 1.24", "TC 1.28", "TC 1.29"}.Contains(testCase))
                    {
                        int pairCount = Math.Min(Math.Min(dutDevices.Count, refDevices.Count), moDevices.Count);
                        if (pairCount == 0)
                        {
                            MessageBox.Show($"Please select matching DUT, REF, and MO devices for {testCase}.");
                            continue;
                        }
                        for (int i = 0; i < pairCount; i++)
                        {
                            var dut = dutDevices[i];
                            var refDev = refDevices[i];
                            var moDev = moDevices[i];
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.8":
                                        new TC_1_8(dut, refDev, moDev, outputRTB, TC18BTN).RunTest();
                                        UpdateCheckBoxColor(TC18CheckBox, TC18BTN);
                                        break;
                                    case "TC 1.17":
                                        new TC_1_17(dut, refDev, moDev, outputRTB, TC117BTN).RunTest();
                                        UpdateCheckBoxColor(TC117CheckBox, TC117BTN);
                                        break;
                                    case "TC 1.22":
                                        new TC_1_22(dut, refDev, moDev, outputRTB, TC122BTN).RunTest();
                                        UpdateCheckBoxColor(TC122CheckBox, TC122BTN);
                                        break;
                                    case "TC 1.24":
                                        new TC_1_24(dut, refDev, moDev, outputRTB, TC124BTN).RunTest();
                                        UpdateCheckBoxColor(TC124CheckBox, TC124BTN);
                                        break;
                                    case "TC 1.28":
                                        new TC_1_28(dut, refDev, moDev, outputRTB, TC128BTN).RunTest();
                                        UpdateCheckBoxColor(TC128CheckBox, TC128BTN);
                                        break;
                                    case "TC 1.29":
                                        new TC_1_29(dut, refDev, moDev, outputRTB, TC129BTN).RunTest();
                                        UpdateCheckBoxColor(TC129CheckBox, TC129BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }

                    // Wait for all device tasks for this test case to finish before moving to the next test case
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Batch run error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isRunningBatch = false;
                _runCts.Dispose();
                _runCts = null;
                _netRefreshTimer.Stop();
            }

            void UpdateCheckBoxColor(CheckBox checkBox, Button button)
            {
                if (button.BackColor == System.Drawing.Color.Green)
                    checkBox.ForeColor = System.Drawing.Color.Green;
                else if (button.BackColor == System.Drawing.Color.Red)
                    checkBox.ForeColor = System.Drawing.Color.Red;
            }
        }

        /*private async void ProcessTCBatchButton_Click(object sender, EventArgs e)
        {
            if (_isRunningBatch)
                return;
            _isRunningBatch = true;
            _runCts = new CancellationTokenSource();
            _netRefreshTimer.Start();

            try
            {
                // Validate DUT selection
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select at least one DUT device.");
                    return;
                }

                // Gather device lists
                var dutDevices = DUTchkbx.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();
                var refDevices = REFchekbx.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();
                var moDevices = devicechkbxlst.CheckedItems.Cast<object>().Select(o => o.ToString()).ToList();

                // Dictionary to map each check box to corresponding test case
                var testCases = new Dictionary<CheckBox, string>
        {
            { TC11CheckBox, "TC 1.1" },
            { TC12CheckBox, "TC 1.2" },
            { TC13CheckBox, "TC 1.3" },
            { TC123CheckBox, "TC 1.23" },
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
            { TC115CheckBox, "TC 1.15" },
            { TC116CheckBox, "TC 1.16" },
            { TC117CheckBox, "TC 1.17" },
            { TC118CheckBox, "TC 1.18" },
            { TC119CheckBox, "TC 1.19" },
            { TC120CheckBox, "TC 1.20" },
            { TC121CheckBox, "TC 1.21" },
            { TC122CheckBox, "TC 1.22" },
            { TC124CheckBox, "TC 1.24" }
        };

                // Build tasks for parallel execution
                var tasks = new List<Task>();

                foreach (var pair in testCases)
                {
                    if (!pair.Key.Checked)
                        continue;

                    string testCase = pair.Value;

                    // DUT-only test cases (run for all DUTs)
                    if (testCase == "TC 1.1" || testCase == "TC 1.2" || testCase == "TC 1.3" || testCase == "TC 1.23")
                    {
                        foreach (var dut in dutDevices)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.1":
                                        new TC_1_1(dut, outputRTB, TC11BTN).RunTest();
                                        UpdateCheckBoxColor(TC11CheckBox, TC11BTN);
                                        break;
                                    case "TC 1.2":
                                        new TC_1_2(dut, outputRTB, TC12BTN).RunTest();
                                        UpdateCheckBoxColor(TC12CheckBox, TC12BTN);
                                        break;
                                    case "TC 1.3":
                                        new TC_1_3(dut, outputRTB).RunTest();
                                        break;
                                    case "TC 1.23":
                                        new TC_1_23(dut, outputRTB, TC123BTN).RunTest();
                                        UpdateCheckBoxColor(TC123CheckBox, TC123BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF paired test cases
                    else if (new[] {
                "TC 1.4","TC 1.5","TC 1.6","TC 1.7","TC 1.10","TC 1.11","TC 1.12","TC 1.13","TC 1.14","TC 1.15",
                "TC 1.16","TC 1.18","TC 1.19","TC 1.20","TC 1.21"
            }.Contains(testCase))
                    {
                        int pairCount = Math.Min(dutDevices.Count, refDevices.Count);
                        if (pairCount == 0)
                        {
                            MessageBox.Show($"Please select matching DUT and REF devices for {testCase}.");
                            continue;
                        }
                        for (int i = 0; i < pairCount; i++)
                        {
                            var dut = dutDevices[i];
                            var refDev = refDevices[i];
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.4":
                                        new TC_1_4(dut, outputRTB, TC14BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC14CheckBox, TC14BTN);
                                        break;
                                    case "TC 1.5":
                                        new TC_1_5(dut, outputRTB, TC15BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC15CheckBox, TC15BTN);
                                        break;
                                    case "TC 1.6":
                                        new TC_1_6(dut, outputRTB, TC16BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC16CheckBox, TC16BTN);
                                        break;
                                    case "TC 1.7":
                                        new TC_1_7(dut, outputRTB, TC17BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC17CheckBox, TC17BTN);
                                        break;
                                    case "TC 1.10":
                                        new TC_1_10(dut, outputRTB, TC110BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC110CheckBox, TC110BTN);
                                        break;
                                    case "TC 1.11":
                                        new TC_1_11(dut, outputRTB, TC111BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC111CheckBox, TC111BTN);
                                        break;
                                    case "TC 1.12":
                                        new TC_1_12(dut, outputRTB, TC112BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC112CheckBox, TC112BTN);
                                        break;
                                    case "TC 1.13":
                                        new TC_1_13(dut, outputRTB, TC113BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC113CheckBox, TC113BTN);
                                        break;
                                    case "TC 1.14":
                                        new TC_1_14(dut, outputRTB, TC114BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC114CheckBox, TC114BTN);
                                        break;
                                    case "TC 1.15":
                                        new TC_1_15(dut, outputRTB, TC115BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC115CheckBox, TC115BTN);
                                        break;
                                    case "TC 1.16":
                                        new TC_1_16(dut, outputRTB, TC116BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC116CheckBox, TC116BTN);
                                        break;
                                    case "TC 1.18":
                                        new TC_1_18(dut, outputRTB, TC118BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC118CheckBox, TC118BTN);
                                        break;
                                    case "TC 1.19":
                                        new TC_1_19(dut, outputRTB, TC119BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC119CheckBox, TC119BTN);
                                        break;
                                    case "TC 1.20":
                                        new TC_1_20(dut, outputRTB, TC120BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC120CheckBox, TC120BTN);
                                        break;
                                    case "TC 1.21":
                                        new TC_1_21(dut, outputRTB, TC121BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC121CheckBox, TC121BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF/MO paired test cases
                    else if (new[] { "TC 1.8", "TC 1.17", "TC 1.22", "TC 1.24" }.Contains(testCase))
                    {
                        int pairCount = Math.Min(Math.Min(dutDevices.Count, refDevices.Count), moDevices.Count);
                        if (pairCount == 0)
                        {
                            MessageBox.Show($"Please select matching DUT, REF, and MO devices for {testCase}.");
                            continue;
                        }
                        for (int i = 0; i < pairCount; i++)
                        {
                            var dut = dutDevices[i];
                            var refDev = refDevices[i];
                            var moDev = moDevices[i];
                            tasks.Add(Task.Run(() =>
                            {
                                switch (testCase)
                                {
                                    case "TC 1.8":
                                        new TC_1_8(dut, refDev, moDev, outputRTB, TC18BTN).RunTest();
                                        UpdateCheckBoxColor(TC18CheckBox, TC18BTN);
                                        break;
                                    case "TC 1.17":
                                        new TC_1_17(dut, refDev, moDev, outputRTB, TC117BTN).RunTest();
                                        UpdateCheckBoxColor(TC117CheckBox, TC117BTN);
                                        break;
                                    case "TC 1.22":
                                        new TC_1_22(dut, refDev, moDev, outputRTB, TC122BTN).RunTest();
                                        UpdateCheckBoxColor(TC122CheckBox, TC122BTN);
                                        break;
                                    case "TC 1.24":
                                        new TC_1_24(dut, refDev, moDev, outputRTB, TC124BTN).RunTest();
                                        UpdateCheckBoxColor(TC124CheckBox, TC124BTN);
                                        break;
                                }
                            }, _runCts.Token));
                        }
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Batch run error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isRunningBatch = false;
                _runCts.Dispose();
                _runCts = null;
                _netRefreshTimer.Stop();
            }

            void UpdateCheckBoxColor(CheckBox checkBox, Button button)
            {
                if (button.BackColor == System.Drawing.Color.Green)
                    checkBox.ForeColor = System.Drawing.Color.Green;
                else if (button.BackColor == System.Drawing.Color.Red)
                    checkBox.ForeColor = System.Drawing.Color.Red;
            }
        }
        */

        /*
        private async void ProcessTCBatchButton_Click(object sender, EventArgs e)
        {
            if (_isRunningBatch)
                return;
            _isRunningBatch = true;
            _runCts = new CancellationTokenSource();
            _netRefreshTimer.Start();

            try
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
            { TC115CheckBox, "TC 1.15" },
            { TC116CheckBox, "TC 1.16" },
            { TC117CheckBox, "TC 1.17" },
            { TC118CheckBox, "TC 1.18" },
            { TC119CheckBox, "TC 1.19" },
            { TC120CheckBox, "TC 1.20" },
            { TC121CheckBox, "TC 1.21" },
            { TC122CheckBox, "TC 1.22" },
            { TC123CheckBox, "TC 1.23" },
            { TC124CheckBox, "TC 1.24" },
            { TC13CheckBox, "TC 1.3" }
        };

                // Validate REF selection for tests that require it
                if ((TC14CheckBox.Checked || TC15CheckBox.Checked || TC16CheckBox.Checked || TC17CheckBox.Checked
                    || TC18CheckBox.Checked || TC110CheckBox.Checked || TC111CheckBox.Checked || TC112CheckBox.Checked ||
                    TC113CheckBox.Checked || TC114CheckBox.Checked || TC115CheckBox.Checked || TC116CheckBox.Checked ||
                    TC117CheckBox.Checked || TC118CheckBox.Checked || TC119CheckBox.Checked || TC120CheckBox.Checked ||
                    TC121CheckBox.Checked || TC122CheckBox.Checked || TC124CheckBox.Checked)
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

                // Validate MO Caller ID for TC 1.17
                if (TC117CheckBox.Checked && string.IsNullOrEmpty(moCallerId))
                {
                    MessageBox.Show("Please select a MO Caller ID device for TC 1.17.");
                    return;
                }

                // Validate MO Caller ID for TC 1.22
                if (TC122CheckBox.Checked && string.IsNullOrEmpty(moCallerId))
                {
                    MessageBox.Show("Please select a MO Caller ID device for TC 1.22.");
                    return;
                }

                await Task.Run(() =>
                {
                    foreach (var pair in testCases)
                    {
                        if (_runCts.IsCancellationRequested)
                            break;

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
                                case "TC 1.15":
                                    new TC_1_15(dutDeviceId, outputRTB, TC115BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC115CheckBox, TC115BTN);
                                    break;
                                case "TC 1.16":
                                    new TC_1_16(dutDeviceId, outputRTB, TC116BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC116CheckBox, TC116BTN);
                                    break;
                                case "TC 1.17":
                                    new TC_1_17(dutDeviceId, refDeviceId, moCallerId, outputRTB, TC117BTN).RunTest();
                                    UpdateCheckBoxColor(TC117CheckBox, TC117BTN);
                                    break;
                                case "TC 1.18":
                                    new TC_1_18(dutDeviceId, outputRTB, TC118BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC118CheckBox, TC118BTN);
                                    break;
                                case "TC 1.19":
                                    new TC_1_19(dutDeviceId, outputRTB, TC119BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC119CheckBox, TC119BTN);
                                    break;
                                case "TC 1.20":
                                    new TC_1_20(dutDeviceId, outputRTB, TC120BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC120CheckBox, TC120BTN);
                                    break;
                                case "TC 1.21":
                                    new TC_1_21(dutDeviceId, outputRTB, TC121BTN, refDeviceId).RunTest();
                                    UpdateCheckBoxColor(TC121CheckBox, TC121BTN);
                                    break;
                                case "TC 1.22":
                                    new TC_1_22(dutDeviceId, refDeviceId, moCallerId, outputRTB, TC122BTN).RunTest();
                                    UpdateCheckBoxColor(TC122CheckBox, TC122BTN);
                                    break;
                                case "TC 1.23":
                                    new TC_1_23(dutDeviceId, outputRTB, TC123BTN).RunTest();
                                    UpdateCheckBoxColor(TC123CheckBox, TC123BTN);
                                    break;
                                case "TC 1.24":
                                    new TC_1_24(dutDeviceId, refDeviceId, moCallerId, outputRTB, TC124BTN).RunTest();
                                    UpdateCheckBoxColor(TC124CheckBox, TC124BTN);
                                    break;
                                default:
                                    MessageBox.Show($"Test case '{testCase}' is not implemented.");
                                    break;
                            }
                        }
                    }

                    void UpdateCheckBoxColor(CheckBox checkBox, Button button)
                    {
                        if (button.BackColor == System.Drawing.Color.Green)
                            checkBox.ForeColor = System.Drawing.Color.Green;
                        else if (button.BackColor == System.Drawing.Color.Red)
                            checkBox.ForeColor = System.Drawing.Color.Red;
                    }
                }, _runCts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Batch run error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isRunningBatch = false;
                _runCts.Dispose();
                _runCts = null;
                _netRefreshTimer.Stop();
            }
        }
        */

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void DUTchkbx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void UploadTCsBTN_Click(object sender, EventArgs e)
        {
            if (_isRunningBatch)
                return;
            _isRunningBatch = true;
            _runCts = new CancellationTokenSource();
            _netRefreshTimer.Start();
            using (var dialog = new OpenFileDialog
            {
                Title = "Select Test Case file",
                Filter = "CSV or Excel (*.csv;*.xlsx)|*.csv;*.xlsx|All files (*.*)|*.*",
                Multiselect = false
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var filePath = dialog.FileName;
                    var tcIds = ParseTestCaseIds(filePath)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                    if (tcIds.Count == 0)
                    {
                        MessageBox.Show("No Test Case IDs were found in the file.", "Nothing to run",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Confirm
                    var preview = string.Join(", ", tcIds.Take(12));
                    if (tcIds.Count > 12) preview += $" … (+{tcIds.Count - 12} more)";
                    var dr = MessageBox.Show($"Found {tcIds.Count} test(s):\n{preview}\n\nRun now?",
                                             "Confirm",
                                             MessageBoxButtons.OKCancel,
                                             MessageBoxIcon.Question);

                    if (dr == DialogResult.OK)
                    {
                        await RunTestsById(tcIds);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file:\n{ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _isRunningBatch = false;
                    _runCts.Dispose();
                    _runCts = null;
                    _netRefreshTimer.Stop();
                }
            }
        }

        // Parse Test Case IDs from either CSV or XLSX
        private List<string> ParseTestCaseIds(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".csv")
                return ParseFromCsv(filePath);
            if (ext == ".xlsx" || ext == ".xls")
                return ParseFromXlsx(filePath);
            throw new NotSupportedException("Only .csv and .xlsx are supported.");
        }

        private List<string> ParseFromCsv(string filePath)
        {
            var ids = new List<string>();
            using (var sr = new StreamReader(filePath))
            {

                string header = sr.ReadLine();
                if (header == null) return ids;

                var headers = SplitCsvLine(header);
                int tcCol = FindHeaderIndex(headers, "Test Case ID", "TestCaseID", "Test CaseId", "TCID");
                if (tcCol < 0) throw new Exception("Couldn't find a 'Test Case ID' column in the CSV.");

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var cells = SplitCsvLine(line);
                    if (tcCol < cells.Count)
                    {
                        var raw = cells[tcCol]?.Trim();
                        if (!string.IsNullOrWhiteSpace(raw)) ids.Add(NormalizeTcId(raw));
                    }
                }
            }
            return ids;
        }

        // very light CSV split (handles commas in quotes)
        private List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var cell = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (c == ',' && !inQuotes)
                {
                    result.Add(cell.ToString());
                    cell.Clear();
                }
                else
                {
                    cell.Append(c);
                }
            }
            result.Add(cell.ToString());
            return result;
        }

        private int FindHeaderIndex(IList<string> headers, params string[] candidates)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                var h = headers[i].Trim();
                foreach (var cand in candidates)
                {
                    if (string.Equals(h, cand, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            // try contains “Test Case”
            for (int i = 0; i < headers.Count; i++)
                if (headers[i].IndexOf("Test Case", StringComparison.OrdinalIgnoreCase) >= 0)
                    return i;
            return -1;
        }

        // XLSX parser (ExcelDataReader)
        private List<string> ParseFromXlsx(string filePath)
        {
            var ids = new List<string>();
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(fs))
            {
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });
                if (ds.Tables.Count == 0) return ids;

                // Pick the first worksheet that contains a Test Case column
                foreach (DataTable table in ds.Tables)
                {
                    int tcCol = -1;
                    foreach (DataColumn col in table.Columns)
                    {
                        if (col.ColumnName.IndexOf("Test Case", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            string.Equals(col.ColumnName, "TestCaseID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(col.ColumnName, "TCID", StringComparison.OrdinalIgnoreCase))
                        {
                            tcCol = col.Ordinal;
                            break;
                        }
                    }
                    if (tcCol < 0) continue;

                    foreach (DataRow row in table.Rows)
                    {
                        var raw = row[tcCol]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(raw)) ids.Add(NormalizeTcId(raw));
                    }
                    if (ids.Count > 0) break;
                }
            }
            return ids;
        }

        // Normalize “TC 1.12” → “TC1.12” to unify comparisons
        private string NormalizeTcId(string raw)
        {
            // keep digits and dot; remove extra spaces
            raw = raw.Trim();
            if (raw.StartsWith("TC ", StringComparison.OrdinalIgnoreCase))
                raw = "TC" + raw.Substring(3);
            return raw.Replace(" ", "");
        }

        private async Task RunTestsById(IEnumerable<string> tcIds)
        {
            // Pick DUT/REF from your lists (first checked item, or parallel if you prefer)
            string dutId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;

            if (string.IsNullOrWhiteSpace(dutId))
            {
                MessageBox.Show("Please select at least one DUT device.");
                return;
            }

            foreach (var id in tcIds.Select(NormalizeTcId))
            {
                try
                {
                    gclass.UpdateOutput($"Running {id} ...", true);
                    gclass.UpdateOutput($"Processing test case ID: '{id}'", true);

                    await Task.Run(() =>
                        { 
                    switch (id.ToUpperInvariant())
                    {
                        case "1.1":
                            {
                                var t = new TC_1_1(dutId, outputRTB, TC11BTN);
                                t.RunTest();
                                break;
                            }
                        case "1.2":
                            {
                                var t = new TC_1_2(dutId, outputRTB, TC12BTN);
                                t.RunTest();
                                break;
                            }
                        case "1.3":
                            {
                                //var t = new TC_1_3(dutId, outputRTB);
                                //t.RunTest();    
                                break;
                            }
                        case "1.4":
                            {
                                var t = new TC_1_4(dutId, outputRTB, TC14BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.5":
                            {
                                var t = new TC_1_5(dutId, outputRTB, TC15BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.6":
                            {
                                var t = new TC_1_6(dutId, outputRTB, TC16BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.7":
                            {
                                var t = new TC_1_7(dutId, outputRTB, TC17BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.8":
                            {
                                var t = new TC_1_8(dutId, refId, moCallerId, outputRTB, TC18BTN);
                                t.RunTest();
                                break;
                            }
                        case "1.10+":
                            {
                                var t = new TC_1_10(dutId, outputRTB, TC110BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.11":
                            {
                                var t = new TC_1_11(dutId, outputRTB, TC111BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.12":
                            {
                                var t = new TC_1_12(dutId, outputRTB, TC112BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.13":
                            {
                                var t = new TC_1_13(dutId, outputRTB, TC113BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.14":
                            {
                                var t = new TC_1_14(dutId, outputRTB, TC114BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.15":
                            {
                                var t = new TC_1_15(dutId, outputRTB, TC115BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.16":
                            {
                                var t = new TC_1_16(dutId, outputRTB, TC116BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.17":
                            {
                                var t = new TC_1_17(dutId, refId, moCallerId, outputRTB, TC117BTN);
                                t.RunTest();
                                break;
                            }
                        case "1.18":
                            {
                                var t = new TC_1_18(dutId, outputRTB, TC118BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.19":
                            {
                                var t = new TC_1_19(dutId, outputRTB, TC119BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.20+":
                            {
                                var t = new TC_1_20(dutId, outputRTB, TC120BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.21":
                            {
                                var t = new TC_1_21(dutId, outputRTB, TC121BTN, refId);
                                t.RunTest();
                                break;
                            }
                        case "1.22":
                            {
                                var t = new TC_1_22(dutId, refId, moCallerId, outputRTB, TC122BTN);
                                t.RunTest();
                                break;
                        }
                         case "1.23":
                             {
                                var t = new TC_1_23(dutId, outputRTB, TC123BTN);
                                t.RunTest();
                                break;
                         }
                                    case "1.24":
                                        {
                                        var t = new TC_1_24(dutId, refId, moCallerId, outputRTB, TC124BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.25":
                                        {
                                        var t = new TC_1_25(dutId, outputRTB, TC125BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.26":
                                        {
                                        var t = new TC_1_26(dutId, outputRTB, TC126BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.27":
                                        {
                                        var t = new TC_1_27(dutId, outputRTB, TC127BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.28":
                                        {
                                        var t = new TC_1_28(dutId, refId, moCallerId, outputRTB, TC128BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.29":
                                    {
                                        var t = new TC_1_29(dutId, refId, moCallerId, outputRTB, TC129BTN);
                                        break;
                                    }
                                    case "1.30":
                                        {
                                        var t = new TC_1_30(dutId, refId, outputRTB, TC130BTN);
                                        t.RunTestAsync();
                                        break;
                                        }
                                default:
                            gclass.UpdateOutput($"No runner mapped for {id}. Skipping.", true);
                            break;
                    }
                });
                }
                catch (Exception ex)
                {
                    gclass.UpdateOutput($"{id}: Exception - {ex.Message}", true);
                }
            }
        }

        private async void NetworkUpdateTimer_Tick(object sender, EventArgs e)
        {
            // 1) Snapshot lists on the UI thread
            List<string> moIds = null;
            List<string> dutIds = null;

            if (!IsHandleCreated || IsDisposed) return;

            this.Invoke((MethodInvoker)delegate
            {
                // Clear the grid safely on UI thread
                volteStatusgrid.Rows.Clear();

                // Take immutable snapshots so later modifications won't affect enumeration
                moIds = devicechkbxlst.Items.Cast<object>()
                                             .Select(o => o.ToString())
                                             .ToList();
                dutIds = DUTchkbx.Items.Cast<object>()
                                       .Select(o => o.ToString())
                                       .ToList();
            });

            // 2) Do the work off the UI thread
            await Task.Run(() =>
            {
                // Combine both lists; handle nulls defensively
                var allDeviceIds = Enumerable.Empty<string>()
                                             .Concat(moIds ?? Enumerable.Empty<string>())
                                             .Concat(dutIds ?? Enumerable.Empty<string>());

                foreach (var deviceId in allDeviceIds)
                {
                    // Get telephony/registration info (no UI access here)
                    var state = RegistrationState.GetTelephonyInfo(deviceId);
                    if (state == null) continue;

                    // 3) Marshal UI updates back to the UI thread
                    if (!IsHandleCreated || IsDisposed) return;

                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        // Guard again in case form is closing
                        if (IsDisposed) return;

                        volteStatusgrid.Rows.Add(
                            state.DeviceId,
                            state.VoLTEStatus,
                            state.ConnectedNetwork,
                            state.BandInfo,
                            state.RATStatus,
                            state.RSRP,
                            state.RSRQ,
                            state.SINR,
                            state.IMSRegisterationStatus,
                            state.DataState,
                            state.RoamingStatus,
                            state.EmergencyState
                        );
                    });
                }
            });
        }

        private void Ui(Action a)
        {
            if (InvokeRequired) BeginInvoke(a); else a();
        }

     
        private async Task RefreshNetworkInfoDiffAsync()
        {
            if (!IsHandleCreated || IsDisposed) return;

            var token = _runCts?.Token ?? CancellationToken.None;

            // 1) Take immutable snapshots of current device IDs on the UI thread
            List<string> deviceIds = null;
            await this.InvokeAsync(() =>
            {
                // Build from whatever lists you maintain (MO, DUT, REF). Examples:
                var moIds = devicechkbxlst.Items.Cast<object>().Select(o => o.ToString());
                var dutIds = DUTchkbx.Items.Cast<object>().Select(o => o.ToString());
                var refIds = REFchekbx.Items.Cast<object>().Select(o => o.ToString());

                deviceIds = moIds.Concat(dutIds).Concat(refIds)
                                 .Distinct()
                                 .Where(id => !string.IsNullOrWhiteSpace(id))
                                 .ToList();
            });

            if (deviceIds.Count == 0) return;

            try
            {
                // The issue is caused because the method `Task.Run` is being used incorrectly. 
                // The lambda passed to `Task.Run` is expected to return a value, but the code is using a `void` method.
                // To fix this, ensure that the lambda returns the appropriate value (e.g., `RegistrationState`).

                var tasks = deviceIds.Select(id => Task.Run(() =>
                {
                    try
                    {
                        return RegistrationState.GetTelephonyInfo(id);
                    }
                    catch
                    {
                        return null; // Ensure the lambda returns a value even in case of an exception.
                    }
                }, token));

                var newStates = (await Task.WhenAll(tasks)) // Ensure `tasks` is awaited properly.
                               .Where(s => s != null)       // This will now work because `Task.WhenAll` returns a collection of `RegistrationState`.
                               .ToDictionary(s => s.DeviceId, s => s);

                if (token.IsCancellationRequested) return;

                // 3) Apply diffs on the UI thread
                await this.InvokeAsync(() =>
                {
                    volteStatusgrid.SuspendLayout();

                    // Add/update rows for devices we have now
                    foreach (var kv in newStates)
                    {
                        var id = kv.Key;
                        var cur = kv.Value;
                        var row = EnsureRowForDevice(id);

                        // Compare to last, update only changed cells
                        _ = UpdateCellIfChanged(row, Col.VoLTEStatus, cur.VoLTEStatus);
                        _ = UpdateCellIfChanged(row, Col.ConnectedNetwork, cur.ConnectedNetwork);
                        _ = UpdateCellIfChanged(row, Col.BandInfo, cur.BandInfo);
                        _ = UpdateCellIfChanged(row, Col.RATStatus, cur.RATStatus);
                        _ = UpdateCellIfChanged(row, Col.RSRP, cur.RSRP);
                        _ = UpdateCellIfChanged(row, Col.RSRQ, cur.RSRQ);
                        _ = UpdateCellIfChanged(row, Col.SINR, cur.SINR);
                        _ = UpdateCellIfChanged(row, Col.IMSRegistrationStatus, cur.IMSRegisterationStatus);
                        _ = UpdateCellIfChanged(row, Col.DataState, cur.DataState);
                        _ = UpdateCellIfChanged(row, Col.RoamingStatus, cur.RoamingStatus);
                        _ = UpdateCellIfChanged(row, Col.EmergencyState, cur.EmergencyState);

                        // Update last snapshot
                        _networkInfoCache[id] = cur;
                    }

                    // Remove rows for devices that disappeared
                    var nowIds = new HashSet<string>(newStates.Keys);
                    for (int i = volteStatusgrid.Rows.Count - 1; i >= 0; i--)
                    {
                        var rid = Convert.ToString(volteStatusgrid.Rows[i].Cells[(int)Col.DeviceId].Value);
                        if (!string.IsNullOrEmpty(rid) && !nowIds.Contains(rid))
                        {
                            volteStatusgrid.Rows.RemoveAt(i);
                            _networkInfoCache.Remove(rid);
                        }
                    }

                    volteStatusgrid.ResumeLayout();
                });
            }
            catch (OperationCanceledException) { /* closing */ }
        }


        private int EnsureRowForDevice(string deviceId)
        {
            // Find existing
            for (int i = 0; i < volteStatusgrid.Rows.Count; i++)
            {
                if (Equals(volteStatusgrid.Rows[i].Cells[(int)Col.DeviceId].Value, deviceId))
                    return i;
            }

            // Create new
            var idx = volteStatusgrid.Rows.Add();
            volteStatusgrid.Rows[idx].Cells[(int)Col.DeviceId].Value = deviceId;
            return idx;
        }

        private bool UpdateCellIfChanged(int rowIndex, Col col, object newValue)
        {
            var cell = volteStatusgrid.Rows[rowIndex].Cells[(int)col];
            var oldValue = cell.Value;

            if ((oldValue == null && newValue == null) ||
                (oldValue != null && oldValue.Equals(newValue)))
                return false;

            cell.Value = newValue;

            // Optional: briefly highlight changed cells
            // cell.Style.BackColor = Color.LightYellow;  // and later fade/clear if you want

            return true;
        }

        // Nice Invoke helper
        private Task InvokeAsync(Action a)
        {
            var tcs = new TaskCompletionSource<object>(); // Specify the type argument as 'object'
            if (IsDisposed) { tcs.TrySetResult(null); return tcs.Task; } // Pass 'null' as the result for 'object'
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(() =>
                {
                    try
                    {
                        a();
                        tcs.TrySetResult(null); // Pass 'null' as the result for 'object'
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }));
            else
            {
                try
                {
                    a();
                    tcs.TrySetResult(null); // Pass 'null' as the result for 'object'
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
            return tcs.Task;
        }

        private void SelectAllAvailableDevicesBTN_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < devicechkbxlst.Items.Count; i++)
                devicechkbxlst.SetItemChecked(i, true);
        }

        private void SelectAllDUTDevicesBTN_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DUTchkbx.Items.Count; i++)
                DUTchkbx.SetItemChecked(i, true);
        }

        private void SelectAllREFDevicesBTN_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < REFchekbx.Items.Count; i++)
                REFchekbx.SetItemChecked(i, true);
        }

        private void SelectAllTCsBTN_Click(object sender, EventArgs e)
        {
            TC11CheckBox.Checked = true;
            TC12CheckBox.Checked = true;
            TC13CheckBox.Checked = true;
            TC14CheckBox.Checked = true;
            TC15CheckBox.Checked = true;
            TC16CheckBox.Checked = true;
            TC17CheckBox.Checked = true;
            TC18CheckBox.Checked = true;
            TC110CheckBox.Checked = true;
            TC111CheckBox.Checked = true;
            TC112CheckBox.Checked = true;
            TC113CheckBox.Checked = true;
            TC114CheckBox.Checked = true;
            TC115CheckBox.Checked = true;
            TC116CheckBox.Checked = true;
            TC117CheckBox.Checked = true;
            TC118CheckBox.Checked = true;
            TC119CheckBox.Checked = true;
            TC120CheckBox.Checked = true;
            TC121CheckBox.Checked = true;
            TC122CheckBox.Checked = true;
            TC123CheckBox.Checked = true;
            TC124CheckBox.Checked = true;
            TC125CheckBox.Checked = true;
            TC126CheckBox.Checked = true;
            TC127CheckBox.Checked = true;
            TC128CheckBox.Checked = true;
            TC129CheckBox.Checked = true;
            TC130CheckBox.Checked = true;
        }

        private void ClearAllTCsBTN_Click(object sender, EventArgs e)
        {
            TC11CheckBox.Checked = false;
            TC12CheckBox.Checked = false;
            TC13CheckBox.Checked = false;
            TC14CheckBox.Checked = false;
            TC15CheckBox.Checked = false;
            TC16CheckBox.Checked = false;
            TC17CheckBox.Checked = false;
            TC18CheckBox.Checked = false;
            TC110CheckBox.Checked = false;
            TC111CheckBox.Checked = false;
            TC112CheckBox.Checked = false;
            TC113CheckBox.Checked = false;
            TC114CheckBox.Checked = false;
            TC115CheckBox.Checked = false;
            TC116CheckBox.Checked = false;
            TC117CheckBox.Checked = false;
            TC118CheckBox.Checked = false;
            TC119CheckBox.Checked = false;
            TC120CheckBox.Checked = false;
            TC121CheckBox.Checked = false;
            TC122CheckBox.Checked = false;
            TC123CheckBox.Checked = false;
            TC124CheckBox.Checked = false;
            TC125CheckBox.Checked = false;
            TC126CheckBox.Checked = false;
            TC127CheckBox.Checked = false;
            TC128CheckBox.Checked = false;
            TC129CheckBox.Checked = false;
            TC130CheckBox.Checked = false;
        }

    }
}