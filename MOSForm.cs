using FIT_Automation.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FIT_Automation
{
    public partial class MOSForm : Form
    {
        private readonly AndroidDeviceManager _deviceManager;
        private readonly MosCalculator _mosCalculator;
        private readonly Timer _monitoringTimer;
        private bool _isMonitoring;
        private Label _lblMosScore;
        private Label _lblJitter;
        private Label _lblPacketLoss;
        private Label _lblLatency;
        private Label _lblStatus;
        private Button _btnStartStop;
        private Chart _mosChart;
        private TextBox _txtCallNumber;
        private Label _lblCodec;
        private Label _lblCodecName;
        private Label _lblCodecDescription;
        private Label _lblRtpPackets;
        private Label _lblRtcpPackets;
        public MOSForm()
        {
            InitializeComponent();
            InitializeUIComponents();
            InitialiseChart();

            _deviceManager = new AndroidDeviceManager();
            _mosCalculator = new MosCalculator();
            _monitoringTimer = new Timer { Interval = 2000 }; // Update every 2 seconds

            _monitoringTimer.Tick += MonitoringTimer_Tick;
            this.FormClosing += MOSForm_FormClosing;
        }

        private void InitializeUIComponents()
        {
            // Form setup
            this.Text = "Voice Call MOS Monitor";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            _lblStatus = new Label
            {
                Text = "Ready to connect",
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Call number input
            var lblCallNumber = new Label
            {
                Text = "Call Number:",
                Location = new Point(20, 60),
                AutoSize = true
            };

            _txtCallNumber = new TextBox
            {
                Location = new Point(120, 60),
                Width = 200,
                Text = GlobalVarClass.MOcallnumber // Use your global number
            };

            // Start/Stop button
            _btnStartStop = new Button
            {
                Text = "Start Monitoring",
                Location = new Point(20, 100),
                Size = new Size(150, 40),
                BackColor = Color.LightGreen
            };
            _btnStartStop.Click += BtnStartStop_Click;

            // MOS Score display
            var lblMosTitle = new Label
            {
                Text = "Current MOS Score:",
                Location = new Point(20, 160),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true
            };

            _lblMosScore = new Label
            {
                Text = "0.00",
                Location = new Point(180, 160),
                Font = new Font("Arial", 24, FontStyle.Bold),
                Size = new Size(150, 40),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Quality metrics
            var lblMetricsTitle = new Label
            {
                Text = "Call Quality Metrics:",
                Location = new Point(20, 220),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };

            _lblJitter = new Label
            {
                Text = "Jitter: 0.00 ms",
                Location = new Point(20, 250),
                AutoSize = true
            };

            _lblPacketLoss = new Label
            {
                Text = "Packet Loss: 0.00%",
                Location = new Point(20, 280),
                AutoSize = true
            };

            _lblLatency = new Label
            {
                Text = "Latency: 0.00 ms",
                Location = new Point(20, 310),
                AutoSize = true
            };

            //MOSGraph
            _lblCodec = new Label { Text = "Codec: N/A", Location = new Point(20, 340), AutoSize = true };
            _lblRtpPackets = new Label { Text = "RTP Packets: 0", Location = new Point(20, 370), AutoSize = true };
            _lblRtcpPackets = new Label { Text = "RTCP Packets: 0", Location = new Point(20, 400), AutoSize = true };


            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                _lblStatus,
                lblCallNumber, _txtCallNumber,
                _btnStartStop,
                lblMosTitle, _lblMosScore,
                lblMetricsTitle, _lblJitter, _lblPacketLoss, _lblLatency,
                _mosChart,
                _lblCodec, _lblRtpPackets, _lblRtcpPackets
            });
        }

        private void InitialiseChart()
        {
            _mosChart = new Chart
            {
                Location = new Point(350, 60),
                Size = new Size(400, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Create chart area
            var chartArea = new ChartArea
            {
                Name = "ChartArea1",
                AxisX = { Title = "Time", IntervalAutoMode = IntervalAutoMode.FixedCount },
                AxisY = { Title = "MOS Score", Minimum = 1, Maximum = 5 }
            };
            _mosChart.ChartAreas.Add(chartArea);

            // Create series
            var series = new Series
            {
                Name = "MOSSeries",
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 2,
                XValueType = ChartValueType.DateTime,
                YValueType = ChartValueType.Double
            };
            _mosChart.Series.Add(series);

            // Add annotations for quality ranges
            AddQualityAnnotations();

            this.Controls.Add(_mosChart);
        }
        private void AddQualityAnnotations()
        {
            // Add horizontal lines for MOS quality ranges
            AddAnnotation("Excellent (4.3-5)", 4.3, Color.Green);
            AddAnnotation("Good (4.0-4.3)", 4.0, Color.LightGreen);
            AddAnnotation("Fair (3.6-4.0)", 3.6, Color.Yellow);
            AddAnnotation("Poor (3.1-3.6)", 3.1, Color.Orange);
            AddAnnotation("Bad (1.0-3.1)", 1.0, Color.Red);
        }

        private void AddAnnotation(string text, double yPosition, Color color)
        {
            var annotation = new HorizontalLineAnnotation
            {
                AxisXName = "ChartArea1\\rX",
                AxisYName = "ChartArea1\\rY",
                Y = yPosition,
                LineColor = color,
                LineWidth = 1,
                LineDashStyle = ChartDashStyle.Dash,
                IsSizeAlwaysRelative = false,
                ClipToChartArea = "ChartArea1"
            };
            _mosChart.Annotations.Add(annotation);
        }


        private async void BtnStartStop_Click(object sender, EventArgs e)
        {
            if (!_isMonitoring)
            {
                // Start monitoring
                if (!_deviceManager.ConnectToDevice())
                {
                    MessageBox.Show("Failed to connect to device. Ensure USB debugging is enabled.");
                    return;
                }

                // Initiate call
                await InitiateCall(_txtCallNumber.Text);

                _btnStartStop.Text = "Stop Monitoring";
                _btnStartStop.BackColor = Color.LightCoral;
                _lblStatus.Text = "Monitoring call quality...";
                _isMonitoring = true;
                _monitoringTimer.Start();
            }
            else
            {
                // Stop monitoring
                _monitoringTimer.Stop();
                await EndCall();

                _btnStartStop.Text = "Start Monitoring";
                _btnStartStop.BackColor = Color.LightGreen;
                _lblStatus.Text = "Monitoring stopped";
                _isMonitoring = false;
            }
        }

        private async Task InitiateCall(string number)
        {
            try
            {
                string command = $"am start -a android.intent.action.CALL -d tel:{number}";
                await Task.Run(() => _deviceManager.ExecuteShellCommand(command));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating call: {ex.Message}");
            }
        }

        private async Task EndCall()
        {
            try
            {
                await Task.Run(() => _deviceManager.ExecuteShellCommand("input keyevent KEYCODE_ENDCALL"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error ending call: {ex.Message}");
            }
        }

        private async void MonitoringTimer_Tick(object sender, EventArgs e)
        {
            if (!_isMonitoring) return;

            try
            {
                var metrics = await Task.Run(() => new CallQualityMetrics().GetMetrics(_deviceManager));
                double mosScore = _mosCalculator.CalculateMos(metrics);

                // Update UI thread safely
                this.Invoke((MethodInvoker)delegate
                {
                    _lblMosScore.Text = mosScore.ToString("0.00");
                    _lblJitter.Text = $"Jitter: {metrics.Jitter.ToString("0.00")} ms";
                    _lblPacketLoss.Text = $"Packet Loss: {(metrics.PacketLoss * 100).ToString("0.00")}%";
                    _lblLatency.Text = $"Latency: {metrics.Latency.ToString("0.00")} ms";
                    _lblCodec.Text = $"Codec: {metrics.Codec}";
                    _lblRtpPackets.Text = $"RTP Packets: {metrics.RTPPackets}";
                    _lblRtcpPackets.Text = $"RTCP Packets: {metrics.RTCPPackets}";
                    // Update graph (you would implement this)
                    UpdateMosGraph(mosScore);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    _lblStatus.Text = $"Error: {ex.Message}";
                });
            }
        }

        private void UpdateMosGraph(double mosScore)
        {
            if (_mosChart.InvokeRequired)
            {
                _mosChart.Invoke(new Action<double>(UpdateMosGraph), mosScore);
                return;
            }

            var series = _mosChart.Series["MOSSeries"];
            series.Points.AddXY(DateTime.Now, mosScore);

            // Keep only the last 30 points for better visibility
            if (series.Points.Count > 30)
            {
                series.Points.RemoveAt(0);
            }

            // Auto-scroll the X axis
            _mosChart.ChartAreas[0].AxisX.Minimum = series.Points[0].XValue;
            _mosChart.ChartAreas[0].AxisX.Maximum = series.Points[series.Points.Count - 1].XValue;
        }
        private void MOSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _monitoringTimer.Stop();
            _monitoringTimer.Dispose();
        }
    }
}
