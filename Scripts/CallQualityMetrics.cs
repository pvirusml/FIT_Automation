using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FIT_Automation.Scripts
{
    public class CallQualityMetrics
    {
        public double Jitter { get; set; }
        public double PacketLoss { get; set; }
        public double Latency { get; set; }
        public double SNR { get; set; }
        public string Codec { get; set; }
        public int RTPPackets { get; set; }
        public int RTCPPackets { get; set; }

        public CallQualityMetrics GetMetrics(AndroidDeviceManager deviceManager)
        {
            string output = deviceManager.ExecuteShellCommand("dumpsys telephony.registry");
            string netstatOutput = deviceManager.ExecuteShellCommand("netstat -w 1");
            string pingOutput = deviceManager.ExecuteShellCommand("ping -c 4 8.8.8.8");

            return new CallQualityMetrics
            {
                Jitter = CalculateJitter(netstatOutput),
                PacketLoss = CalculatePacketLoss(output),
                Latency = CalculateLatency(pingOutput),
                SNR = CalculateSNR(output),
                Codec = ParseCodec(output),
                RTPPackets = ParseRtpPackets(output),
                RTCPPackets = ParseRtcpPackets(output)
            };
        }

        private double CalculateJitter(string netstatOutput)
        {
            // Parse jitter from netstat output
            var matches = Regex.Matches(netstatOutput, @"time=(\d+)\.?(\d+)? ms");
            if (matches.Count < 2) return 0;

            var times = matches.Cast<Match>()
                .Select(m => double.Parse(m.Groups[1].Value + "." + (m.Groups[2].Success ? m.Groups[2].Value : "0")))
                .ToArray();

            double sum = 0;
            for (int i = 1; i < times.Length; i++)
            {
                sum += Math.Abs(times[i] - times[i - 1]);
            }

            return sum / (times.Length - 1);
        }

        private double CalculatePacketLoss(string telephonyOutput)
        {
            // Parse packet loss from telephony registry
            var match = Regex.Match(telephonyOutput, @"packetLoss=(\d+)");
            if (match.Success)
            {
                return double.Parse(match.Groups[1].Value) / 100.0;
            }
            return 0;
        }

        private double CalculateLatency(string pingOutput)
        {
            // Parse average ping latency
            var match = Regex.Match(pingOutput, @"min/avg/max/mdev = [\d.]+/([\d.]+)/");
            if (match.Success)
            {
                return double.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        private double CalculateSNR(string telephonyOutput)
        {
            // Parse signal-to-noise ratio
            var match = Regex.Match(telephonyOutput, @"snr=(-?\d+)");
            if (match.Success)
            {
                return double.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        private string ParseCodec(string telephonyOutput)
        {
            var match = Regex.Match(telephonyOutput, @"codec=([A-Z0-9_]+)");
            return match.Success ? match.Groups[1].Value : "UNKNOWN";
        }

        private int ParseRtpPackets(string telephonyOutput)
        {
            var match = Regex.Match(telephonyOutput, @"rtpPackets=(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private int ParseRtcpPackets(string telephonyOutput)
        {
            var match = Regex.Match(telephonyOutput, @"rtcpPackets=(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}
