using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIT_Automation.Scripts
{
    public class AndroidDeviceManager
    {
        private AdbClient adbClient;
        private DeviceData device;

        public bool ConnectToDevice(bool usbConnection = true, string deviceIP = null)
        {
            adbClient = new AdbClient();

            // Start ADB server if not running
            var server = new AdbServer();
            var result = server.StartServer(@"C:\adb\adb.exe", false);

            if (!usbConnection && deviceIP != null)
            {
                // For WiFi connection
                adbClient.Connect(deviceIP);
            }

            var devices = adbClient.GetDevices();

            if (devices.Any())
            {
                device = devices.First();
                return true;
            }
            return false;
        }

        public string ExecuteShellCommand(string command)
        {
            var receiver = new ConsoleOutputReceiver();
            adbClient.ExecuteRemoteCommand(command, device, receiver);
            return receiver.ToString();
        }

        public DeviceData GetFirstConnectedDevice()
        {
            var devices = adbClient.GetDevices();
            return devices.FirstOrDefault();
        }
    }
}
