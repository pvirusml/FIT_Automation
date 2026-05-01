using FIT_Automation.Scripts;
using OpenQA.Selenium.BiDi.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation
{
    public partial class LiveScreenPopup : Form
    {
        public PictureBox PictureBox;
        private readonly string _deviceId;
        private readonly int _deviceScreenWidth;
        private readonly int _deviceScreenHeight;
        private readonly DataGridView _networkDetailsGrid;
        GlobalVarClass gclass;
        public LiveScreenPopup(string deviceId, int deviceScreenWidth, int deviceScreenHeight, DataGridView networkDetailsGrid)
        {
            InitializeComponent();
            this.KeyPreview = true; // Enable key events for the form
            _deviceId = deviceId;
            _deviceScreenWidth = deviceScreenWidth;
            _deviceScreenHeight = deviceScreenHeight;
            _networkDetailsGrid = networkDetailsGrid;
            // Initialize the form
            this.Text = "Live Screen";
            this.Size = new Size(800, 600); // Set the default size of the popup window
            this.StartPosition = FormStartPosition.CenterScreen;


            // Initialize the PictureBox
            PictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom // Scale the image proportionally
            };

            // Add the PictureBox to the form
            this.Controls.Add(PictureBox);
            gclass = new GlobalVarClass(deviceId, null, null);
            _networkDetailsGrid = networkDetailsGrid;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.KeyDown += LiveScreenPopup_KeyDown;

            // Attach MouseClick event to the PictureBox
            PictureBox.MouseClick += PictureBox_MouseClick;
        }

        private void LiveScreenPopup_KeyDown(object sender, KeyEventArgs e)
        {
            string deviceId = _deviceId;
            string adbKey = null;
            if (e.KeyCode == Keys.Up)
                gclass.RunAdbCommand($"adb -s {deviceId} shell input swipe 500 500 500 1500");
            // Swipe Up: ADB command for swipe up (startX, startY, endX, endY, duration)
            //adbKey = "19";      // KEYCODE_DPAD_UP
            if (e.KeyCode == Keys.Down)
                gclass.RunAdbCommand($"adb -s {deviceId} shell input swipe 500 1500 500 500");
            //adbKey = "20";    // KEYCODE_DPAD_DOWN
            if (e.KeyCode == Keys.Left)
                gclass.RunAdbCommand($"adb -s {deviceId} shell input swipe 200 1000 800 1000");
            //adbKey = "21";    // KEYCODE_DPAD_LEFT
            if (e.KeyCode == Keys.Right)
                gclass.RunAdbCommand($"adb -s {deviceId} shell input swipe 800 1000 300 1103");
            //adbKey = "22";   // KEYCODE_DPAD_RIGHT

            if (adbKey != null)
            {
                // Call your ADB command runner (e.g., GlobalVarClass.RunAdbCommand)
                gclass.RunAdbCommand($"adb -s {deviceId} shell input keyevent {adbKey}");
                e.Handled = true;
            }
        }
        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            // Get the PictureBox dimensions
            int pictureBoxWidth = PictureBox.ClientSize.Width;
            int pictureBoxHeight = PictureBox.ClientSize.Height;

            // Calculate the aspect ratio of the device screen and PictureBox
            float deviceAspectRatio = (float)_deviceScreenWidth / _deviceScreenHeight;
            float pictureBoxAspectRatio = (float)pictureBoxWidth / pictureBoxHeight;

            // Initialize offsets for padding
            int offsetX = 0, offsetY = 0;

            // Adjust for letterboxing or pillarboxing
            if (pictureBoxAspectRatio > deviceAspectRatio)
            {
                // PictureBox is wider than the device screen
                int adjustedWidth = (int)(pictureBoxHeight * deviceAspectRatio);
                offsetX = (pictureBoxWidth - adjustedWidth) / 2;
                pictureBoxWidth = adjustedWidth;
            }
            else if (pictureBoxAspectRatio < deviceAspectRatio)
            {
                // PictureBox is taller than the device screen
                int adjustedHeight = (int)(pictureBoxWidth / deviceAspectRatio);
                offsetY = (pictureBoxHeight - adjustedHeight) / 2;
                pictureBoxHeight = adjustedHeight;
            }

            // Calculate the scaling factors
            float scaleX = (float)_deviceScreenWidth / pictureBoxWidth;
            float scaleY = (float)_deviceScreenHeight / pictureBoxHeight;

            // Map the mouse click coordinates to the device screen
            int deviceX = (int)((e.X - offsetX) * scaleX);
            int deviceY = (int)((e.Y - offsetY) * scaleY);

            // Ensure the coordinates are within bounds
            deviceX = Math.Max(0, Math.Min(_deviceScreenWidth, deviceX));
            deviceY = Math.Max(0, Math.Min(_deviceScreenHeight, deviceY));

            // Send the ADB tap command
            gclass.RunAdbCommand($"adb -s {_deviceId} shell input tap {deviceX} {deviceY}");
        }
        private void LiveScreenPopup_Load(object sender, EventArgs e)
        {

        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (PictureBox.Image == null)
                {
                    MessageBox.Show("No image to capture!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fitAutomationFolder = Path.Combine(desktopPath, "FIT_AUTOMATION Screenshots");

                Directory.CreateDirectory(fitAutomationFolder);

                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string currentDut = _deviceId.ToString();
                int sessionNumber;

                var existingSessions = Directory.GetDirectories(fitAutomationFolder)
                    .Select(Path.GetFileName)
                    .Where(name => name.StartsWith("Session "))
                    .Select(name =>
                    {
                        // Expected: Session 1 - 2026-04-30 - DUT 12345
                        string[] parts = name.Split(new[] { " - " }, StringSplitOptions.None);

                        sessionNumber = 0;
                        string date = "";
                        string dut = "";

                        if (parts.Length >= 3)
                        {
                            string numberText = parts[0].Replace("Session", "").Trim();
                            int.TryParse(numberText, out sessionNumber);

                            date = parts[1].Trim();
                            dut = parts[2].Replace("DUT", "").Trim();
                        }

                        return new
                        {
                            Number = sessionNumber,
                            Date = date,
                            Dut = dut,
                            FolderName = name
                        };
                    })
                    .Where(s => s.Number > 0)
                    .OrderByDescending(s => s.Number)
                    .ToList();

                var latestSession = existingSessions.FirstOrDefault();


                if (latestSession != null &&
                    latestSession.Date == currentDate &&
                    latestSession.Dut == currentDut)
                {
                    // Same date and same DUT: reuse same session
                    sessionNumber = latestSession.Number;
                }
                else
                {
                    // Date changed OR DUT changed: create next session number
                    int highestSessionNumber = existingSessions.Any()
                        ? existingSessions.Max(s => s.Number)
                        : 0;

                    sessionNumber = highestSessionNumber + 1;
                }

                /*
               string sessionFolder = Path.Combine(
                fitAutomationFolder,
                    $"Session {sessionNumber} - {currentDate} - DUT {currentDut}"
                );
                */
                string sessionFolder = GetCurrentSessionFolder();

                Directory.CreateDirectory(sessionFolder);

                string fileName = $"Screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
                string filePath = Path.Combine(sessionFolder, fileName);

                PictureBox.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show($"Screenshot saved to: {filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to capture screenshot: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetCurrentSessionFolder()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fitAutomationFolder = Path.Combine(desktopPath, "FIT_AUTOMATION Screenshots");

            Directory.CreateDirectory(fitAutomationFolder);

            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string currentDut = _deviceId.ToString();

            var existingSessions = Directory.GetDirectories(fitAutomationFolder)
                .Select(Path.GetFileName)
                .Where(name => name.StartsWith("Session "))
                .Select(name =>
                {
                    string[] parts = name.Split(new[] { " - " }, StringSplitOptions.None);

                    int number = 0;
                    string date = "";
                    string dut = "";

                    if (parts.Length >= 3)
                    {
                        string numberText = parts[0].Replace("Session", "").Trim();
                        int.TryParse(numberText, out number);

                        date = parts[1].Trim();
                        dut = parts[2].Replace("DUT", "").Trim();
                    }

                    return new
                    {
                        Number = number,
                        Date = date,
                        Dut = dut,
                        FolderName = name
                    };
                })
                .Where(s => s.Number > 0)
                .OrderByDescending(s => s.Number)
                .ToList();

            var latestSession = existingSessions.FirstOrDefault();

            int sessionNumber;

            if (latestSession != null &&
                latestSession.Date == currentDate &&
                latestSession.Dut == currentDut)
            {
                sessionNumber = latestSession.Number;
            }
            else
            {
                int highestSessionNumber = existingSessions.Any()
                    ? existingSessions.Max(s => s.Number)
                    : 0;

                sessionNumber = highestSessionNumber + 1;
            }

            string sessionFolder = Path.Combine(
                fitAutomationFolder,
                $"Session {sessionNumber} - {currentDate} - DUT {currentDut}"
            );

            Directory.CreateDirectory(sessionFolder);

            return sessionFolder;
        }

        private void DownloadLogsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_networkDetailsGrid == null || _networkDetailsGrid.Rows.Count == 0)
                {
                    MessageBox.Show("No network details available to download.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string sessionFolder = GetCurrentSessionFolder();

                string fileName = $"NetworkDetails_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
                string filePath = Path.Combine(sessionFolder, fileName);

                string[] headers =
                {
    "Device",
    "VoLTEStatus",
    "Network",
    "Band",
    "RSRP",
    "DataState",
    "Emergency",
    "Roaming",
    "IMSRegisteration"
};

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(string.Join(",", headers));

                    foreach (DataGridViewRow row in _networkDetailsGrid.Rows)
                    {
                        if (row.IsNewRow)
                            continue;

                        List<string> values = new List<string>();

                        for (int i = 0; i < headers.Length; i++)
                        {
                            string value = row.Cells[i].Value?.ToString() ?? "";
                            values.Add(EscapeCsvValue(value));
                        }

                        writer.WriteLine(string.Join(",", values));
                    }
                }

                MessageBox.Show($"Network details saved to: {filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download network details: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        private void BothButton_Click(object sender, EventArgs e)
        {
            CaptureButton_Click(sender, e);
            DownloadLogsButton_Click(sender, e);
        }
    }
}
