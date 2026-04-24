using FIT_Automation.Scripts;
using OpenQA.Selenium.BiDi.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing;
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

        GlobalVarClass gclass;
        public LiveScreenPopup(string deviceId, int deviceScreenWidth, int deviceScreenHeight)
        {
            InitializeComponent();
            this.KeyPreview = true; // Enable key events for the form
            _deviceId = deviceId;
            _deviceScreenWidth = deviceScreenWidth;
            _deviceScreenHeight = deviceScreenHeight;
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

    }
}
