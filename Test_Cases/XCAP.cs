using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class XCAP
    {
        public static void RunTest(DataGridView DeviceDataGridView, CheckedListBox deviceChkBox, CheckedListBox mtDeviceChkBox)
        {
            if (deviceChkBox.CheckedItems.Count == 0 || mtDeviceChkBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select both sending and receiving devices.");
                return;
            }

            string mtDeviceSerial = mtDeviceChkBox.CheckedItems[0].ToString();
            string moDeviceSerial = deviceChkBox.CheckedItems[0].ToString();
            string mtPhoneNumber = GetPhoneNumber(mtDeviceSerial, DeviceDataGridView);
            string moPhoneNumber = GetPhoneNumber(moDeviceSerial, DeviceDataGridView);

            if (string.IsNullOrEmpty(mtPhoneNumber))
            {
                MessageBox.Show("Could not find phone number for the selected MT device.");
                return;
            }
            //Forwarding Number
            string ForwardNumber = "6478376636";

            // Forward Calls on the MT Device
            ForwardCalls(mtDeviceSerial, ForwardNumber);

            Thread.Sleep(5000); // Wait 10 Seconds before checking

            // Check if call forwarding is active on the MT device
            if (IsCallForwardingActive(mtDeviceSerial))
            {
                MessageBox.Show("Calls of the MT device are forwarded.");
                // Place a call from MO to MT
                PlaceCall(moDeviceSerial, mtPhoneNumber);
                Thread.Sleep(15000);
                //End Call
                string command = "shell input keyevent KEYCODE_ENDCALL";
                RunADBCommand(moDeviceSerial, command);
            }
            else
            {
                MessageBox.Show("Call forwarding is not active on the MT device.");
            }

        }



        //FUNCTION CALLS>>>

        //Fucntion used to find the Phone Numbers of the Seleceted Devices.
        private static string GetPhoneNumber(string Serial, DataGridView dgv)
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

        //Function used to Toggle Airplane Mode on MO/MT Devices.
        private static void ToggleAirplaneMode(string serial, bool enable)
        {
            string state = enable ? "1" : "0";
            RunADBCommand(serial, $"shell settings put global airplane_mode_on {state}");
            RunADBCommand(serial, "shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state " + (enable ? "true" : "false"));
        }

        //Function used to run adb commands.
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


        //Function to Forward the calls on MT Devices.
        private static void ForwardCalls(string serial, string forwardToNumber)
        {
            // USSD code for call forwarding: *21*<number>#
            //string ussdCode = $"*21*{forwardToNumber";

            // Use ADB to input the USSD code
            string command = $"shell am start -a android.intent.action.CALL -d tel:*21*{forwardToNumber}%23";
            string output = RunADBCommand(serial, command);
            Thread.Sleep(1000);
            //ADB command to hit send
            command = $"shell input keyevent 66";
            RunADBCommand(serial, command);

            // Check if the command was successful
            if (output.Contains("Error") || string.IsNullOrEmpty(output))
            {
                MessageBox.Show($"Failed to forward calls on device {serial}.");
            }
            else
            {
                MessageBox.Show($"Calls forwarded successfully on device {serial} to {forwardToNumber}.");
            }
        }

        // Function to place a call from MO to MT
        private static void PlaceCall(string serial, string phoneNumber)
        {
            string command = $"shell am start -a android.intent.action.CALL -d tel:{phoneNumber} --ez android.telecom.extra.START_CALL_WITH_SPEAKERPHONE true";
            string output = RunADBCommand(serial, command);

            if (output.Contains("Error") || string.IsNullOrEmpty(output))
            {
                MessageBox.Show($"Failed to place a call from device {serial}.");
            }
            else
            {
                MessageBox.Show($"Call placed successfully from device {serial} to {phoneNumber}. Calls are being Forwarded. ");
            }
        }

        // Function to check if call forwarding is active on the MT device
        private static bool IsCallForwardingActive(string serial)
        {
            string command = "shell dumpsys telephony.registry";
            string output = RunADBCommand(serial, command);

            // Check if call forwarding is active
            return output.Contains("mCallForwarding=true") || output.Contains("mCallForwardingIndicator=true");
        }

    }
}
