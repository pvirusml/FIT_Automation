using FIT_Automation.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{

    public class RegistrationState
    {
        public string DeviceId { get; set; }
        public string VoLTEStatus { get; set; }
        public string ConnectedNetwork { get; set; }
        public string BandInfo { get; set; }
        public string RSRP { get; set; }
        public string DataState { get; set; }
        public string EmergencyState { get; set; }
        public string RoamingStatus { get; set; }

        public static RegistrationState GetTelephonyInfo(string deviceId)
        {
            GlobalVarClass gclass = new GlobalVarClass();
            try
            {
                // Run ADB command
                string output = gclass.RunAdbCommand($"adb -s {deviceId} shell dumpsys telephony.registry");
                //MessageBox.Show(output);
                // Parse required values
                string volteStatus = Regex.Match(output, @"mVoiceRegState=(\d+)").Groups[1].Value;
                string network = Regex.Match(output, @"mOperatorAlphaLong=([\w\s]+)").Groups[1].Value;
                string band = Regex.Match(output, @"mBands=\[(\d+)\]").Groups[1].Value;
                string rsrp = Regex.Match(output, @"CellSignalStrengthLte:.*?rsrp=(-?\d+)").Groups[1].Value;
                //MessageBox.Show(rsrp);
                string dataState = Regex.Match(output, @"mDataConnectionState=(\d+)").Groups[1].Value;
                string emergencyState = Regex.Match(output, @"mIsEmergencyOnly=(\w+)").Groups[1].Value;
                string roamingStatus = Regex.Match(output, @"roamingType=(\w+)").Groups[1].Value;

                // Convert values
                volteStatus = (volteStatus == "0") ? "IN_SERVICE" : "POWER_OFF";
                dataState = (dataState == "2") ? "Connected" : "Not Connected";
                emergencyState = (emergencyState == "true") ? "Yes" : "No";
                roamingStatus = (roamingStatus == "NOT_ROAMING") ? "Home" : "Roaming";

                // Return as an object
                return new RegistrationState
                {
                    DeviceId = deviceId,
                    VoLTEStatus = volteStatus,
                    ConnectedNetwork = network,
                    BandInfo = band,
                    RSRP = rsrp,
                    DataState = dataState,
                    EmergencyState = emergencyState,
                    RoamingStatus = roamingStatus
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        //private static string RunAdbCommand(string command)
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo
        //    {
        //        FileName = "adb",
        //        Arguments = command,
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using (Process process = new Process { StartInfo = psi })
        //    {
        //        process.Start();
        //        return process.StandardOutput.ReadToEnd();
        //    }
        //}
    }
}
