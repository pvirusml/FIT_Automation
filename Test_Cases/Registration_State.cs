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
        public string RSRQ { get; set; }
        public string SINR { get; set; }
        public string DataState { get; set; }
        public string EmergencyState { get; set; }
        public string RoamingStatus { get; set; }
        public string IMSRegisterationStatus { get; set; }
        public string RATStatus { get; set; }
        

        public static RegistrationState GetTelephonyInfo(string deviceId)
        {
            GlobalVarClass gclass = new GlobalVarClass(null, null, null);
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
                string rsrq = Regex.Match(output, @"CellSignalStrengthLte:.*?rsrq=(-?\d+)").Groups[1].Value;
                string sinr = Regex.Match(output, @"ssSinr\s*=\s*(\d+)").Groups[1].Value;

                //MessageBox.Show(rsrp);
                string dataState = Regex.Match(output, @"mDataConnectionState=(\d+)").Groups[1].Value;
                string emergencyState = Regex.Match(output, @"mIsEmergencyOnly=(\w+)").Groups[1].Value;
                string roamingStatus = Regex.Match(output, @"roamingType=(\w+)").Groups[1].Value;
                string imsRegistertionStatus = Regex.Match(output, @"mImsRegistrationOnOff=(\w+)").Groups[1].Value;
                string ratStatus = gclass.RunAdbCommand($"adb  -s {deviceId} shell getprop gsm.network.type").ToLower();
                if (ratStatus.Contains(",unknown"))
                {
                    ratStatus = ratStatus.Replace(",unknown", "");
                }
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
                    RSRQ = rsrq,
                    SINR = sinr,
                    DataState = dataState,
                    EmergencyState = emergencyState,
                    RoamingStatus = roamingStatus,
                    IMSRegisterationStatus = imsRegistertionStatus == "true" ? "Registered" : "Not Registered",
                    RATStatus = ratStatus
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public static bool IsDeviceRegisteredOnVOLTE(string deviceId)
        {
            GlobalVarClass gclass = new GlobalVarClass(null, null, null);
            // Run ADB command
            string output = gclass.RunAdbCommand($"adb -s {deviceId} shell dumpsys telephony.registry");

            // Check LTE Registeration
            bool isLTERegistered = Regex.IsMatch(output, @"mDataConnectionState\s*2"); // 2 mean connected
            // Check VoLTE Registeration
            bool isVoLTERegistered = Regex.IsMatch(output, @"mVolteState=\s*CONNECTED", RegexOptions.IgnoreCase);

            return isLTERegistered && isVoLTERegistered;

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
