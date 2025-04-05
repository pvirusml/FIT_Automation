using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIT_Automation.Scripts
{
    public class MosCalculator
    {
        public double CalculateMos(CallQualityMetrics metrics)
        {
            // E-model based MOS calculation
            double R = CalculateRFactor(metrics);

            // Convert R-factor to MOS
            if (R < 0) return 1;
            if (R > 100) return 4.5;

            return 1 + 0.035 * R + R * (R - 60) * (100 - R) * 7 * Math.Pow(10, -6);
        }

        private double CalculateRFactor(CallQualityMetrics metrics)
        {
            // Base R-factor
            double R = 94.2;

            // Subtract impairments
            R -= CalculateEquipmentImpairment(metrics);
            R -= CalculateDelayImpairment(metrics.Latency);
            R -= CalculatePacketLossImpairment(metrics.PacketLoss);

            return R;
        }

        private double CalculateEquipmentImpairment(CallQualityMetrics metrics)
        {
            return 10 * Math.Log10(1 + 10 * metrics.Jitter / 1000);
        }

        private double CalculateDelayImpairment(double latency)
        {
            double Ta = latency / 2;
            if (Ta < 100) return 0;
            return 25 * (1 + Math.Pow(Math.Log(Ta / 100) / Math.Log(2), 6));
        }

        private double CalculatePacketLossImpairment(double packetLoss)
        {
            return 20 * (1 + Math.Pow(packetLoss / 5, 2));
        }
    }
}
