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
using System.Net.Http;
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
        private System.Windows.Forms.TabControl tc;
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
            addToolTips();

        }


        void addToolTips()
        {
            this.toolTip1.SetToolTip(this.TC11BTN, "1. Power ON the device in LTE coverage.\r\n2. Verify the device can camp on LTE using LTE attach apn followed by sucessful VoLTE registration.");
            this.toolTip1.SetToolTip(this.TC12BTN, "1. Verify the device can camp on LTE using LTE attach apn followed by sucessful VoLTE registration.");
            this.toolTip1.SetToolTip(this.TC13BTN, "1. Ensure the device is camped on 3G and registered on IMS.\r\n2. Verify the Device can can sucessfully re register on IMS before the Registration timer expires.");
            this.toolTip1.SetToolTip(this.TC14BTN, "1. Place a MO call from device registered on VoLTE to another device registered on VoLTE\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC15BTN, "1. Place a MO call from device registered on VoLTE to another ICS WB AMR capable device \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC16BTN, "1. Receive SMS from a VoLTE device to another VoLTE device. TURN OFF RCS");
            this.toolTip1.SetToolTip(this.TC17BTN, "1. Receive SMS on a VoLTE device (ICS) from a VoLTE device.");
            this.toolTip1.SetToolTip(this.TC18BTN, "1. Ensure device is IMS registered on LTE.\r\n2. Initiate CFU to DUT 2.\r\n3. Ensure Ensure the device uses XCAP (GBA-ME) to set up CFU.\r\n4. Call DUT 1 from any device except DUT 2, ensure call is forwarded to DUT 2.\r\n5. Maintain the call for 1min and end the call");
            //this.toolTip1.SetToolTip(this.TC19BTN, "1. Place a MO call from device registered on VoLTE to another ICS WB AMR capable device (data turned off) \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC110BTN, "1. Send SMS from a VoLTE device to another VoLTE device.");
            this.toolTip1.SetToolTip(this.TC111BTN, "1. Send SMS from a VoWiFi attached device camping on Cellular to a Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC112BTN, "1. Initiate a call between the devices.\r\n2.  Send SMS from a VoWiFi attached device camping on Cellular to the Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC113BTN, "1. Send MMS from a VoWiFi attached device camping on Cellular to a Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC114BTN, "1. Initiate a call between the devices.\r\n2.  Send MMS from a VoWiFi attached device camping on Cellular to the Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC115BTN, "1. Call from a VoWiFi attached device camping on cellular to VoWiFi device camping on Cellular. \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC116BTN, "1. Call from a VoWiFi attached device camping on cellular to VoLTE device. \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC117BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC118BTN, "1. Place a MO call from device registered on VoLTE to another device registered on VoLTE (data turned Off)\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC119BTN, "1. Receive a MT call on device registered on VoLTE From another device registered on VoLTE (data turned Off)\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC120BTN, "1) Place a MO video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC121BTN, "1) Place a MO video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC122BTN, "Wi-Fi > Wi-Fi >> Wi-Fi");
            this.toolTip1.SetToolTip(this.TC123BTN, "1. Ensure DUT 1 is IMS registered on LTE.\r\n2. On DUT 1, set up CFU to its own CTN.\r\n3. Verify the device does not show CFU as succesful.");
            this.toolTip1.SetToolTip(this.TC124BTN, "1. Setup Call forwarding on DUT 2\r\n2. Place a MO video call from DUT 1 to DUT 2\r\n3. Ensure video call is downgraded and connected as audio\r\n4. Ensure audio and is ok\r\n5. Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC125BTN, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) While call is ongoing, disable \"Wi-Fi\" switch on DUT 1\r\n4) DUT 1 will show \"Call failure\" UI notification\r\n5) On DUT 2, call should fail after RTP/RTCP timeout value. (for ex: if RTP/RTCP timeout is set to 10secs, then DUT will show 'Call failure\" notification after 10secs)");
            this.toolTip1.SetToolTip(this.TC126BTN, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) On DUT 1, while call is ongoing, enable Airplane Mode\r\n4) A pop-up should come up informing that enabling Airplane mode will end your Phone call. Tap on \"Enable Airplane mode\"\r\n5) DUT 1 will end the call, ensure DUT 2 also ends the call immediately (Note: there should not be call failure notifications as UE sends BYE with \"d-registering\" when AP mode is enabled; so NW should 200 Ok and end the call gracefully on DUT 2)\r\n6) Turn off \"Airplane mode\" and ensure DUT is IMS registered over Wi-Fi for Voice and other supported services");
            this.toolTip1.SetToolTip(this.TC127BTN, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) On DUT 1, while call is ongoing, disable \"Wi-Fi Calling\" switch\r\n4) DUT 1 will HO to VoLTE and maintain the call on VoLTE\r\n5) Turn ON \"WiFi calling switch\" and ensure DUT is IMS registered over Wi-Fi for Voice and other supported services");
            this.toolTip1.SetToolTip(this.TC128BTN, "1. Intiate a call from DUT 1 to DUT 2.\r\n2. Receive call from a VOLTE device while being on active call.");
            this.toolTip1.SetToolTip(this.TC129BTN, "1) Place a call from DUT 1 to DUT 2. \r\n2) While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 and validate that the call waiting notification is received on DUT 1\r\n3) Hold current call and answer incoming call from DUT 3\r\n4) DUT2 is on hold, keep it on hold for 1 minute\r\n4) Ensure audio is ok between DUT1 and DUT3\r\n5) Now swap calls so DUT 3 is kept on hold, keep DUT 3 on hold for 1 minute\r\n6) Ensure audio is ok between DUT1 and DUT2\r\n7) Again swap calls so DUT2 goes on hold, keep DUT 2 on hold for 1 minute\r\n8) Ensure audio is ok between DUT1 and DUT3");
            this.toolTip1.SetToolTip(this.TC130BTN, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC131BTN, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC132BTN, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC133BTN, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT  1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT  1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC134BTN, "1) Place a call from DUT  2 to DUT  1 and reject the call on DUT  1 \r\n2) Ensure that DUT  2 is re-directed to DUT 1's voicemail\r\n3) Leave voicemail to DUT  1\r\n4) Verify DUT  1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC135BTN, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC136BTN, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 1 on hold\r\n3) Verify Call hold tone is heard on DUT 1\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC137BTN, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC138BTN, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 1 on hold\r\n3) Verify Call hold tone is heard on DUT 1\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC139BTN, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC140BTN, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC141BTN, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Switch call to audio, verify Audio is Okay.\r\n4) Upgrade call to Video and Verify the Audio and Video is Okay.\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC142BTN, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Accept the call as Audio.\r\n3) Ensure audio call is connected, audio is ok\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC143BTN, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Switch call to audio, verify Audio is Okay.\r\n4) Upgrade call to Video and Verify the Audio and Video is Okay.\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC144BTN, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Accept the call as Audio.\r\n3) Ensure audio call is connected, audio is ok\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC145BTN, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT 1 receives SMS\r\n4) DUT 1 should be redirected to DUT 2's voicemail\r\n5) Leave voicemail\r\n6) On DUT 2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC146BTN, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT 1 receives SMS\r\n4) DUT 1 should be redirected to DUT 2's voicemail\r\n5) Leave voicemail\r\n6) On DUT 2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC147BTN, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT  1 receives SMS\r\n4) DUT  1 should be redirected to DUT  2's voicemail\r\n5) Leave voicemail\r\n6) On DUT  2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC148BTN, "1) Turn off \"Show my caller ID\" on DUT 2\r\n2) Place a call from DUT 2 to DUT 1\r\n3) Called ID should be shown as \"Unknown Caller\"\r\n4) Accept the call, check audio is ok\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC149BTN, "1) Place a call from DUT 1 to DUT  2\r\n2) Let DUT  2 alert the call\r\n3) Cancel the call on DUT  1 before call is answered on DUT  2\r\n4) Ensure call is ended; no errors on DUT  1 and DUT  2\r\n4) Place another call from DUT  1 to DUT  2; answer the call; ensure audio is ok");
            this.toolTip1.SetToolTip(this.TC150BTN, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected, audio is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC151BTN, "1) Place a call from DUT 1 to DUT 2\r\n2) Verify the right Country Code is sent in PANI Header\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC152BTN, "1) Turn of Location Services on DUT 1. \r\n2)Place a call from DUT 1 to DUT 2\r\n3) Verify the right Country Code is sent in PANI Header\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC153BTN, "1) Turn on Airplane mode and then turn on WiFi on DUT 1. \r\n2)Place a call from DUT 1 to DUT 2\r\n3) Verify the right Country Code is sent in PANI Header\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC154BTN, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE");
            this.toolTip1.SetToolTip(this.TC155BTN, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE.\r\n4. Place a MO VolTE call from DUT 1 to DUT 2.\r\n5. Ensure call is sucessful over VoLTE. End the Call.");
            this.toolTip1.SetToolTip(this.TC156BTN, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE.\r\n4. Place a MO video call from DUT 1 to DUT 2.\r\n5. Ensure call is sucessful over VoLTE and Audio/Video are fine.\r\n6. End the call.");
            this.toolTip1.SetToolTip(this.TC157BTN, "1) Start FTP DL and browse the internet\r\n2) Make MO VoWiFi Call\r\n3) While FTP/browsing and VoWiFi call running, do the following:\r\n  - Receive a SMS\r\n - Send a SMS\r\n4) End VoWiFi Call and Reoriginate after 30 sec\r\n - Incoming MMS\r\n - Outgoing MMS\r\n5) End VoWiFi Call and Reoriginate after 30 sec\r\n - Download Apps\r\n - Web Browsing \r\n6) End FTP ");
            this.toolTip1.SetToolTip(this.TC158BTN, "1) Start FTP DL and browse the internet\r\n2) Receive MT VoWiFi Call\r\n3) While FTP/browsing and VoWiFi call running, do the following:\r\n  - Receive a SMS\r\n - Send a SMS\r\n4) End VoWiFi Call and Reoriginate after 30 sec\r\n - Incoming MMS\r\n - Outgoing MMS\r\n5) End VoWiFi Call and Reoriginate after 30 sec\r\n - Download Apps\r\n - Web Browsing \r\n6) End FTP ");
            this.toolTip1.SetToolTip(this.TC159BTN, "1) Place a call from DUT 1 to DUT 2. While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 \r\n2) Tap on \"Hold & Accept\", to accept the incoming call from DUT3.\r\n3) Once the call is established with DUT 3 make sure that audio is heard in both directions.\r\n4) Ensure DUT2 is on Hold\r\n5) Now on DUT1, tap on \"Merge\" to merge all 3 calls.\r\n6) Make sure that all parties can send and receive audio on the conference. \r\n7) End the Conference call on DUT 1 and make sure that the conference call is ended successfully.");
            this.toolTip1.SetToolTip(this.TC160BTN, "1) Place a call from DUT 1 to DUT 2. While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 \r\n2) Tap on \"Hold & Accept\", to accept the incoming call from DUT3.\r\n3) Once the call is established with DUT 3 make sure that audio is heard in both directions.\r\n4) Ensure DUT2 is on Hold\r\n5) Wait for 30 secs and on DUT1, tap on \"Merge\" to merge all 3 calls.\r\n6) Make sure that all parties can send and receive audio on the conference. \r\n7) End the Conference call on DUT 1 and make sure that the conference call is ended successfully.");
            this.toolTip1.SetToolTip(this.TC161BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC162BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC163BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC164BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC165BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC166BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC167BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC168BTN, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC169BTN, "1. Check device connections.\r\n2. Set Airplane mode ON, then OFF for both devices.\r\n3. Wait for LTE/VoLTE registration.\r\n4. Send SMS from REF to DUT.\r\n5. Check for SMS reception.\r\n6. Reset device state.");
            this.toolTip1.SetToolTip(this.TC170BTN, "1. Check device connections.\r\n2. Set Airplane mode ON, then OFF for both devices.\r\n3. Wait for LTE/VoLTE registration.\r\n4. Send SMS from REF to DUT.\r\n5. Check for SMS reception.\r\n6. Reset device state.");
            this.toolTip1.SetToolTip(this.TC171BTN, "1. Check device connections.\r\n2. Set Airplane mode ON, enable WiFi for both devices.\r\n3. Wait for LTE/VoWiFi registration.\r\n4. Place and answer call.\r\n5. Send MMS during call.\r\n6. End call and reset device state.");
            this.toolTip1.SetToolTip(this.TC172BTN, "1. Verify device supports both Cellular and WiFi preferred polices.");
            this.toolTip1.SetToolTip(this.TC173BTN, "1. Verify by default the WiFi preferred policy is used  by the device");
            this.toolTip1.SetToolTip(this.TC174BTN, "1) Connect to any Wi-Fi Access Point (AP)\r\n2 )Go to Settings and enable \"Wi-Fi Calling switch\"");
            this.toolTip1.SetToolTip(this.TC175BTN, "1)DUT is IMS registered for Voice and other supported services over Wi-Fi\r\n2) Enable AirPlane mode (ON).\r\n3) Enable Wi-Fi (ON).\r\n4) Verify Wi-Fi calling and SMS works.");


            //for check boxes
            this.toolTip1.SetToolTip(this.TC11CheckBox, "1. Power ON the device in LTE coverage.\r\n2. Verify the device can camp on LTE using LTE attach apn followed by sucessful VoLTE registration.");
            this.toolTip1.SetToolTip(this.TC12CheckBox, "1. Verify the device can camp on LTE using LTE attach apn followed by sucessful VoLTE registration. ");
            this.toolTip1.SetToolTip(this.TC13CheckBox, "1. Ensure the device is camped on 3G and registered on IMS.\r\n2. Verify the Device can can sucessfully re register on IMS before the Registration timer expires.");
            this.toolTip1.SetToolTip(this.TC14CheckBox, "1. Place a MO call from device registered on VoLTE to another device registered on VoLTE\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC15CheckBox, "1. Place a MO call from device registered on VoLTE to another ICS WB AMR capable device \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC16CheckBox, "1. Receive SMS from a VoLTE device to another VoLTE device. TURN OFF RCS");
            this.toolTip1.SetToolTip(this.TC17CheckBox, "1. Receive SMS on a VoLTE device (ICS) from a VoLTE device.");
            this.toolTip1.SetToolTip(this.TC18CheckBox, "1. Ensure device is IMS registered on LTE.\r\n2. Initiate CFU to DUT 2.\r\n3. Ensure Ensure the device uses XCAP (GBA-ME) to set up CFU.\r\n4. Call DUT 1 from any device except DUT 2, ensure call is forwarded to DUT 2.\r\n5. Maintain the call for 1min and end the call");
            //this.toolTip1.SetToolTip(this.TC19BTN, "1. Place a MO call from device registered on VoLTE to another ICS WB AMR capable device (data turned off) \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC110CheckBox, "1. Send SMS from a VoLTE device to another VoLTE device.");
            this.toolTip1.SetToolTip(this.TC111CheckBox, "1. Send SMS from a VoWiFi attached device camping on Cellular to a Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC112CheckBox, "1. Initiate a call between the devices.\r\n2.  Send SMS from a VoWiFi attached device camping on Cellular to the Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC113CheckBox, "1. Send MMS from a VoWiFi attached device camping on Cellular to a Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC114CheckBox, "1. Initiate a call between the devices.\r\n2.  Send MMS from a VoWiFi attached device camping on Cellular to the Unison (VoWiFi) device");
            this.toolTip1.SetToolTip(this.TC115CheckBox, "1. Call from a VoWiFi attached device camping on cellular to VoWiFi device camping on Cellular. \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC116CheckBox, "1. Call from a VoWiFi attached device camping on cellular to VoLTE device. \r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC117CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC118CheckBox, "1. Place a MO call from device registered on VoLTE to another device registered on VoLTE (data turned Off)\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC119CheckBox, "1. Receive a MT call on device registered on VoLTE From another device registered on VoLTE (data turned Off)\r\n2. Maintain call for one minute.");
            this.toolTip1.SetToolTip(this.TC120CheckBox, "1) Place a MO video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC121CheckBox, "1) Place a MO video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC122CheckBox, "Wi-Fi > Wi-Fi >> Wi-Fi");
            this.toolTip1.SetToolTip(this.TC123CheckBox, "1. Ensure DUT 1 is IMS registered on LTE.\r\n2. On DUT 1, set up CFU to its own CTN.\r\n3. Verify the device does not show CFU as succesful.");
            this.toolTip1.SetToolTip(this.TC124CheckBox, "1. Setup Call forwarding on DUT 2\r\n2. Place a MO video call from DUT 1 to DUT 2\r\n3. Ensure video call is downgraded and connected as audio\r\n4. Ensure audio and is ok\r\n5. Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC125CheckBox, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) While call is ongoing, disable \"Wi-Fi\" switch on DUT 1\r\n4) DUT 1 will show \"Call failure\" UI notification\r\n5) On DUT 2, call should fail after RTP/RTCP timeout value. (for ex: if RTP/RTCP timeout is set to 10secs, then DUT will show 'Call failure\" notification after 10secs)");
            this.toolTip1.SetToolTip(this.TC126CheckBox, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) On DUT 1, while call is ongoing, enable Airplane Mode\r\n4) A pop-up should come up informing that enabling Airplane mode will end your Phone call. Tap on \"Enable Airplane mode\"\r\n5) DUT 1 will end the call, ensure DUT 2 also ends the call immediately (Note: there should not be call failure notifications as UE sends BYE with \"d-registering\" when AP mode is enabled; so NW should 200 Ok and end the call gracefully on DUT 2)\r\n6) Turn off \"Airplane mode\" and ensure DUT is IMS registered over Wi-Fi for Voice and other supported services");
            this.toolTip1.SetToolTip(this.TC127CheckBox, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected and audio is ok\r\n3) On DUT 1, while call is ongoing, disable \"Wi-Fi Calling\" switch\r\n4) DUT 1 will HO to VoLTE and maintain the call on VoLTE\r\n5) Turn ON \"WiFi calling switch\" and ensure DUT is IMS registered over Wi-Fi for Voice and other supported services");
            this.toolTip1.SetToolTip(this.TC128CheckBox, "1. Intiate a call from DUT 1 to DUT 2.\r\n2. Receive call from a VOLTE device while being on active call.");
            this.toolTip1.SetToolTip(this.TC129CheckBox, "1) Place a call from DUT 1 to DUT 2. \r\n2) While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 and validate that the call waiting notification is received on DUT 1\r\n3) Hold current call and answer incoming call from DUT 3\r\n4) DUT2 is on hold, keep it on hold for 1 minute\r\n4) Ensure audio is ok between DUT1 and DUT3\r\n5) Now swap calls so DUT 3 is kept on hold, keep DUT 3 on hold for 1 minute\r\n6) Ensure audio is ok between DUT1 and DUT2\r\n7) Again swap calls so DUT2 goes on hold, keep DUT 2 on hold for 1 minute\r\n8) Ensure audio is ok between DUT1 and DUT3");
            this.toolTip1.SetToolTip(this.TC130CheckBox, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC131CheckBox, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC132CheckBox, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT 1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT 1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC133CheckBox, "1) Place a call from DUT 2 to DUT 1 and reject the call on DUT  1 \r\n2) Ensure that DUT 2 is re-directed to DUT1's voicemail\r\n3) Leave voicemail to DUT 1\r\n4) Verify DUT  1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC134CheckBox, "1) Place a call from DUT  2 to DUT  1 and reject the call on DUT  1 \r\n2) Ensure that DUT  2 is re-directed to DUT 1's voicemail\r\n3) Leave voicemail to DUT  1\r\n4) Verify DUT  1 receives Voicemail notification and play voicemail\r\n5) Ensure audio is ok\r\n6) 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok");
            this.toolTip1.SetToolTip(this.TC135CheckBox, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC136CheckBox, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 1 on hold\r\n3) Verify Call hold tone is heard on DUT 1\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC137CheckBox, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC138CheckBox, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 1 on hold\r\n3) Verify Call hold tone is heard on DUT 1\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC139CheckBox, "1) Make call from DUT 1 to DUT 2\r\n2) Once call is connected, put DUT 2 on hold\r\n3) Verify Call hold tone is heard on DUT 2\r\n4) Wait for 1 minute and then Un hold\r\n5) Ensure audio is ok after unholding the call\r\n6) End the call after 10 secs");
            this.toolTip1.SetToolTip(this.TC140CheckBox, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC141CheckBox, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Switch call to audio, verify Audio is Okay.\r\n4) Upgrade call to Video and Verify the Audio and Video is Okay.\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC142CheckBox, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Accept the call as Audio.\r\n3) Ensure audio call is connected, audio is ok\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC143CheckBox, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Ensure video call is connected, audio and video is ok\r\n3) Switch call to audio, verify Audio is Okay.\r\n4) Upgrade call to Video and Verify the Audio and Video is Okay.\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC144CheckBox, "1) Place a Video call from DUT 1 to DUT 2\r\n2) Accept the call as Audio.\r\n3) Ensure audio call is connected, audio is ok\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC145CheckBox, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT 1 receives SMS\r\n4) DUT 1 should be redirected to DUT 2's voicemail\r\n5) Leave voicemail\r\n6) On DUT 2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC146CheckBox, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT 1 receives SMS\r\n4) DUT 1 should be redirected to DUT 2's voicemail\r\n5) Leave voicemail\r\n6) On DUT 2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC147CheckBox, "1) Make a call from DUT 1 to DUT 2\r\n2) On DUT 2, decline call with SMS\r\n3) Ensure DUT  1 receives SMS\r\n4) DUT  1 should be redirected to DUT  2's voicemail\r\n5) Leave voicemail\r\n6) On DUT  2, verify Voicemail notification is received and play voicemail");
            this.toolTip1.SetToolTip(this.TC148CheckBox, "1) Turn off \"Show my caller ID\" on DUT 2\r\n2) Place a call from DUT 2 to DUT 1\r\n3) Called ID should be shown as \"Unknown Caller\"\r\n4) Accept the call, check audio is ok\r\n5) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC149CheckBox, "1) Place a call from DUT 1 to DUT  2\r\n2) Let DUT  2 alert the call\r\n3) Cancel the call on DUT  1 before call is answered on DUT  2\r\n4) Ensure call is ended; no errors on DUT  1 and DUT  2\r\n4) Place another call from DUT  1 to DUT  2; answer the call; ensure audio is ok");
            this.toolTip1.SetToolTip(this.TC150CheckBox, "1) Place a call from DUT 1 to DUT 2\r\n2) Ensure call is connected, audio is ok\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC151CheckBox, "1) Place a call from DUT 1 to DUT 2\r\n2) Verify the right Country Code is sent in PANI Header\r\n3) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC152CheckBox, "1) Turn of Location Services on DUT 1. \r\n2)Place a call from DUT 1 to DUT 2\r\n3) Verify the right Country Code is sent in PANI Header\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC153CheckBox, "1) Turn on Airplane mode and then turn on WiFi on DUT 1. \r\n2)Place a call from DUT 1 to DUT 2\r\n3) Verify the right Country Code is sent in PANI Header\r\n4) Maintain the call for 1min and end the call");
            this.toolTip1.SetToolTip(this.TC154CheckBox, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE");
            this.toolTip1.SetToolTip(this.TC155CheckBox, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE.\r\n4. Place a MO VolTE call from DUT 1 to DUT 2.\r\n5. Ensure call is sucessful over VoLTE. End the Call.");
            this.toolTip1.SetToolTip(this.TC156CheckBox, "1. Ensure Dut 1 is IMS registered on LTE.\r\n2. Turn on WiFi and connect to WiFi.\r\n3. Ensure the device stays on VoLTE and does not de-register from VoLTE.\r\n4. Place a MO video call from DUT 1 to DUT 2.\r\n5. Ensure call is sucessful over VoLTE and Audio/Video are fine.\r\n6. End the call.");
            this.toolTip1.SetToolTip(this.TC157CheckBox, "1) Start FTP DL and browse the internet\r\n2) Make MO VoWiFi Call\r\n3) While FTP/browsing and VoWiFi call running, do the following:\r\n  - Receive a SMS\r\n - Send a SMS\r\n4) End VoWiFi Call and Reoriginate after 30 sec\r\n - Incoming MMS\r\n - Outgoing MMS\r\n5) End VoWiFi Call and Reoriginate after 30 sec\r\n - Download Apps\r\n - Web Browsing \r\n6) End FTP ");
            this.toolTip1.SetToolTip(this.TC158CheckBox, "1) Start FTP DL and browse the internet\r\n2) Receive MT VoWiFi Call\r\n3) While FTP/browsing and VoWiFi call running, do the following:\r\n  - Receive a SMS\r\n - Send a SMS\r\n4) End VoWiFi Call and Reoriginate after 30 sec\r\n - Incoming MMS\r\n - Outgoing MMS\r\n5) End VoWiFi Call and Reoriginate after 30 sec\r\n - Download Apps\r\n - Web Browsing \r\n6) End FTP ");
            this.toolTip1.SetToolTip(this.TC159CheckBox, "1) Place a call from DUT 1 to DUT 2. While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 \r\n2) Tap on \"Hold & Accept\", to accept the incoming call from DUT3.\r\n3) Once the call is established with DUT 3 make sure that audio is heard in both directions.\r\n4) Ensure DUT2 is on Hold\r\n5) Now on DUT1, tap on \"Merge\" to merge all 3 calls.\r\n6) Make sure that all parties can send and receive audio on the conference. \r\n7) End the Conference call on DUT 1 and make sure that the conference call is ended successfully.");
            this.toolTip1.SetToolTip(this.TC160CheckBox, "1) Place a call from DUT 1 to DUT 2. While the call between DUT 1 and DUT 2 is in progress, place a call from DUT 3 to DUT 1 \r\n2) Tap on \"Hold & Accept\", to accept the incoming call from DUT3.\r\n3) Once the call is established with DUT 3 make sure that audio is heard in both directions.\r\n4) Ensure DUT2 is on Hold\r\n5) Wait for 30 secs and on DUT1, tap on \"Merge\" to merge all 3 calls.\r\n6) Make sure that all parties can send and receive audio on the conference. \r\n7) End the Conference call on DUT 1 and make sure that the conference call is ended successfully.");
            this.toolTip1.SetToolTip(this.TC161CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC162CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (VoWiFi)");
            this.toolTip1.SetToolTip(this.TC163CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC164CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC165CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (CS)");
            this.toolTip1.SetToolTip(this.TC166CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoWiFi).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC167CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (VoLTE).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC168CheckBox, "1. Setup CFU ON DUT 1 (VoWiFi) to DUT 3 (CS).\r\n2. Call DUT 1 from DUT 2 (VoLTE)");
            this.toolTip1.SetToolTip(this.TC169CheckBox, "1. Check device connections.\r\n2. Set Airplane mode ON, then OFF for both devices.\r\n3. Wait for LTE/VoLTE registration.\r\n4. Send SMS from REF to DUT.\r\n5. Check for SMS reception.\r\n6. Reset device state.");
            this.toolTip1.SetToolTip(this.TC170CheckBox, "1. Check device connections.\r\n2. Set Airplane mode ON, then OFF for both devices.\r\n3. Wait for LTE/VoLTE registration.\r\n4. Send SMS from REF to DUT.\r\n5. Check for SMS reception.\r\n6. Reset device state.");
            this.toolTip1.SetToolTip(this.TC171CheckBox, "1. Check device connections.\r\n2. Set Airplane mode ON, enable WiFi for both devices.\r\n3. Wait for LTE/VoWiFi registration.\r\n4. Place and answer call.\r\n5. Send MMS during call.\r\n6. End call and reset device state.");
            this.toolTip1.SetToolTip(this.TC172CheckBox, "1. Verify device supports both Cellular and WiFi preferred polices.");
            this.toolTip1.SetToolTip(this.TC173CheckBox, "1. Verify by default the WiFi preferred policy is used  by the device");
            this.toolTip1.SetToolTip(this.TC174CheckBox, "1) Connect to any Wi-Fi Access Point (AP)\r\n2 )Go to Settings and enable \"Wi-Fi Calling switch\"");
            this.toolTip1.SetToolTip(this.TC175CheckBox, "1)DUT is IMS registered for Voice and other supported services over Wi-Fi\r\n2) Enable AirPlane mode (ON).\r\n3) Enable Wi-Fi (ON).\r\n4) Verify Wi-Fi calling and SMS works.");


            this.toolTip1.SetToolTip(this.CheckAllDUTOnlyBoxes, "Select all DUT only TCs");
            this.toolTip1.SetToolTip(this.CheckAllDUTAndREFOnlyBoxes, "Select all DUT & REF only TCs");
            this.toolTip1.SetToolTip(this.CheckAllDUTREFAndMOOnlyBoxes, "Select all DUT, REF, & AD only TCs");
        }


        private void PreRequisitesButton_Click(object sender, EventArgs e)
        {
            // You can initialize things here if needed
            StartUpPopUpForm popUpForm = new StartUpPopUpForm();

            popUpForm.ShowDialog();
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

        public void PopulateDeviceListInitial()
        {
            try
            {
                // Run ADB command to get device list
                string output = gclass.RunAdbCommand("adb devices");
                gclass.RunAdbroot("adb root");

                //Timer to let the devices reconnect as root.
                Thread.Sleep(3);

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
            // force async test to tun frist with l=mutex lock

            test.RunTestAsync();
        }


        private void TC131BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.31.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_31 test = new TC_1_31(deviceId, refDeviceId, outputRTB, TC131BTN);
                test.RunTestAsync();
        }

        private void TC132BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.32.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_32 test = new TC_1_32(deviceId, refDeviceId, outputRTB, TC132BTN);
            test.RunTestAsync();
        }

        private void TC133BTN_Click(object sender, EventArgs e)
        {
if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.33.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_33 test = new TC_1_33(deviceId, refDeviceId, outputRTB, TC133BTN);
            test.RunTestAsync();
        }

        private void TC134BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.34.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_34 test = new TC_1_34(deviceId, refDeviceId, outputRTB, TC134BTN);
            test.RunTestAsync();

        }

        private void TC135BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.35.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_35 test = new TC_1_35(deviceId, refDeviceId, outputRTB, TC135BTN);
            test.RunTestAsync();
        }

        private void TC136BTN_Click(object sender, EventArgs e)
        {
        if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.36.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_36 test = new TC_1_36(deviceId, refDeviceId, outputRTB, TC136BTN);
                test.RunTestAsync();
        }

        private void TC137BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.37.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_37 test = new TC_1_37(deviceId, refDeviceId, outputRTB, TC137BTN);
            test.RunTestAsync();
        }

        private void TC138BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.38.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_38 test = new TC_1_38(deviceId, refDeviceId, outputRTB, TC138BTN);
            test.RunTestAsync();
        }

        private void TC139BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.39.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_39 test = new TC_1_39(deviceId, refDeviceId, outputRTB, TC139BTN);
            test.RunTestAsync();

        }

        private void TC140BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.40.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_40 test = new TC_1_40(deviceId, refDeviceId, outputRTB, TC140BTN);
            test.RunTest();

        }

        private void TC141BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.41.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_41 test = new TC_1_41(deviceId, refDeviceId, outputRTB, TC141BTN);
            test.RunTest();

        }

        private void TC142BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.42.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_42 test = new TC_1_42(deviceId, refDeviceId, outputRTB, TC142BTN);
            test.RunTest();
        }

        private void TC143BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.43.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_43 test = new TC_1_43(deviceId, refDeviceId, outputRTB, TC143BTN);
            test.RunTest();
        }

        private void TC144BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.44.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_44 test = new TC_1_44(deviceId, refDeviceId, outputRTB, TC144BTN);
            test.RunTest();

        }

        private void TC145BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.45.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_45 test = new TC_1_45(deviceId, refDeviceId, outputRTB, TC145BTN);
            test.RunTestAsync();
        }

        private void TC146BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.46.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_46 test = new TC_1_46(deviceId, refDeviceId, outputRTB, TC146BTN);
            test.RunTestAsync();
        }

        private void TC147BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.47.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_47 test = new TC_1_47(deviceId, refDeviceId, outputRTB, TC147BTN);
                test.RunTestAsync();
        }

        private void TC148BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.48.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_48 test = new TC_1_48(deviceId, refDeviceId, outputRTB, TC148BTN);
            test.RunTestAsync();
        }

        private void TC149BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.49.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_49 test = new TC_1_49(deviceId, refDeviceId, outputRTB, TC149BTN);
            test.RunTestAsync();
        }

        private void TC150BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.50.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_50 test = new TC_1_50(deviceId, refDeviceId, outputRTB, TC150BTN);
            test.RunTestAsync();
        }

        private void TC151BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.51.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_51 test = new TC_1_51(deviceId, refDeviceId, outputRTB, TC151BTN);
            test.RunTestAsync();
        }

        private void TC152BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.52.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_52 test = new TC_1_52(deviceId, refDeviceId, outputRTB, TC152BTN);
            test.RunTestAsync();
        }


        private void TC153BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.53.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_53 test = new TC_1_53(deviceId, refDeviceId, outputRTB, TC153BTN);
            test.RunTestAsync();
        }

        private void TC154BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.54.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            TC_1_54 test = new TC_1_54(deviceId, outputRTB, TC154BTN);
            test.RunTestAsync();

        }


        private void TC155BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.55.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_55 test = new TC_1_55(deviceId, refDeviceId, outputRTB, TC155BTN);
            test.RunTest();
        }

        private void TC156BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.56.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_56 test = new TC_1_56(deviceId, refDeviceId, outputRTB, TC156BTN);
            test.RunTest();
        }

        private void TC157BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.57.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_57 test = new TC_1_57(deviceId, refDeviceId, outputRTB, TC157BTN);
            test.RunTest();
        }

        private void TC158BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.58.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_58 test = new TC_1_58(deviceId, refDeviceId, outputRTB, TC158BTN);
            test.RunTest();
        }

        private void TC159BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.59.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_59 test = new TC_1_59(deviceId, refDeviceId, moCallerId, outputRTB, TC159BTN);
            test.RunTest();
        }

        private void TC160BTN_Click(object sender, EventArgs e)
        {
if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.60.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_60 test = new TC_1_60(deviceId, refDeviceId, moCallerId, outputRTB, TC160BTN);
            test.RunTest();
        }

        private void TC161BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.61.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_61 test = new TC_1_61(deviceId, refDeviceId, moCallerId, outputRTB, TC161BTN);
            test.RunTest();
        }

        private void TC162BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.62.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_62 test = new TC_1_62(deviceId, refDeviceId, moCallerId, outputRTB, TC162BTN);
            test.RunTest();
        }

        private void TC163BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.63.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
                TC_1_63 test = new TC_1_63(deviceId, refDeviceId, moCallerId, outputRTB, TC163BTN);
                test.RunTest();
        }

        private void TC164BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.64.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_64 test = new TC_1_64(deviceId, refDeviceId, moCallerId, outputRTB, TC164BTN);
            test.RunTest();
        }

        private void TC165BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.65.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_65 test = new TC_1_65(deviceId, refDeviceId, moCallerId, outputRTB, TC165BTN);
            test.RunTest();
        }

        private void TC166BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.66.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_66 test = new TC_1_66(deviceId, refDeviceId, moCallerId, outputRTB, TC166BTN);
            test.RunTest();
        }

        private void TC167BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.67.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_67 test = new TC_1_67(deviceId, refDeviceId, moCallerId, outputRTB, TC167BTN);
            test.RunTest();
        }

        private void TC168BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.68.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            string moCallerId = devicechkbxlst.CheckedItems.Count > 0 ? devicechkbxlst.CheckedItems[0].ToString() : null;
            TC_1_68 test = new TC_1_68(deviceId, refDeviceId, moCallerId, outputRTB, TC168BTN);
            test.RunTest();
        }


        private void TC169BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.69.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_69 test = new TC_1_69(deviceId,  outputRTB, TC169BTN, refDeviceId);
            test.RunTest();
        }

        private void TC170BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.70.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_70 test = new TC_1_70(deviceId,  outputRTB, TC170BTN, refDeviceId);
            test.RunTest();
        }

        private void TC171BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.71.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_71 test = new TC_1_71(deviceId,  outputRTB, TC171BTN, refDeviceId);
            test.RunTest();
        }

        private void TC172BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.72.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_72 test = new TC_1_72(deviceId,  outputRTB, TC172BTN);
            test.RunTestAsync();
        }

        private void TC173BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.73.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_73 test = new TC_1_73(deviceId,  outputRTB, TC173BTN);
            test.RunTestAsync();
        }

        private void TC174BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.74.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_74 test = new TC_1_74(deviceId,  outputRTB, TC174BTN);
            test.RunTestAsync();
        }

        private void TC175BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.75.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_75 test = new TC_1_75(deviceId,  outputRTB, TC175BTN);
            test.RunTestAsync();
        }


        private void DeviceContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TC176BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.76.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_76 test = new TC_1_76(deviceId, outputRTB, TC176BTN);
            test.RunTestAsync();
        }

        private void TC177BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.77.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_77 test = new TC_1_77(deviceId, outputRTB, TC177BTN);
            test.RunTestAsync();
        }

        private void TC178BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.78.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_78 test = new TC_1_78(deviceId, outputRTB, TC178BTN);
            test.RunTestAsync();
        }

        private void TC179BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.79.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_79 test = new TC_1_79(deviceId, outputRTB, TC179BTN);
            test.RunTestAsync();
        }

        private void TC180BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.80.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_80 test = new TC_1_80(deviceId, outputRTB, TC180BTN);
            test.RunTestAsync();
        }

        private void TC181BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.81.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_81 test = new TC_1_81(deviceId, outputRTB, TC181BTN);
            test.RunTestAsync();
        }

        private void TC182BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.82.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_82 test = new TC_1_82(deviceId, outputRTB, TC182BTN);
            test.RunTestAsync();
        }

        private void TC183BTN_Click(object sender, EventArgs e)
        {
                if (DUTchkbx.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select a device to run TC 1.83.");
                    return;
                }
                //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
                string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
                string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
                TC_1_83 test = new TC_1_83(deviceId, outputRTB, TC183BTN);
                test.RunTestAsync();
        }

        private void TC184BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.84.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_84 test = new TC_1_84(deviceId, outputRTB, TC184BTN);
            test.RunTestAsync();
        }

        private void TC185BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.85.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_85 test = new TC_1_85(deviceId, outputRTB, TC185BTN);
            test.RunTestAsync();
        }

        private void TC186BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.86.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_86 test = new TC_1_86(deviceId, outputRTB, TC186BTN);
            test.RunTestAsync();
        }

        private void TC187BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.87.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_87 test = new TC_1_87(deviceId, outputRTB, TC187BTN);
            test.RunTestAsync();
        }

        private void TC188BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.88.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_88 test = new TC_1_88(deviceId, outputRTB, TC188BTN);
            test.RunTestAsync();
        }

        private void TC189BTN_Click(object sender, EventArgs e)
        {
            if (DUTchkbx.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to run TC 1.89.");
                return;
            }
            //string deviceId = devicechkbxlst.CheckedItems[0].ToString();
            string deviceId = DUTchkbx.CheckedItems.Count > 0 ? DUTchkbx.CheckedItems[0].ToString() : null;
            string refDeviceId = REFchekbx.CheckedItems.Count > 0 ? REFchekbx.CheckedItems[0].ToString() : null;
            TC_1_89 test = new TC_1_89(deviceId, outputRTB, TC189BTN);
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
                if (TC131CheckBox.Checked) testCases.Add("TC 1.31");
                if (TC132CheckBox.Checked) testCases.Add("TC 1.32");
                if (TC133CheckBox.Checked) testCases.Add("TC 1.33");
                if (TC134CheckBox.Checked) testCases.Add("TC 1.34");
                if (TC135CheckBox.Checked) testCases.Add("TC 1.35");
                if (TC136CheckBox.Checked) testCases.Add("TC 1.36");
                if (TC137CheckBox.Checked) testCases.Add("TC 1.37");
                if (TC138CheckBox.Checked) testCases.Add("TC 1.38");
                if (TC139CheckBox.Checked) testCases.Add("TC 1.39");
                if (TC140CheckBox.Checked) testCases.Add("TC 1.40");
                if (TC141CheckBox.Checked) testCases.Add("TC 1.41");
                if (TC142CheckBox.Checked) testCases.Add("TC 1.42");
                if (TC143CheckBox.Checked) testCases.Add("TC 1.43");
                if (TC144CheckBox.Checked) testCases.Add("TC 1.44");
                if (TC145CheckBox.Checked) testCases.Add("TC 1.45");
                if (TC146CheckBox.Checked) testCases.Add("TC 1.46");
                if (TC147CheckBox.Checked) testCases.Add("TC 1.47");
                if (TC148CheckBox.Checked) testCases.Add("TC 1.48");
                if (TC149CheckBox.Checked) testCases.Add("TC 1.49");
                if (TC150CheckBox.Checked) testCases.Add("TC 1.50");
                if (TC151CheckBox.Checked) testCases.Add("TC 1.51");
                if (TC152CheckBox.Checked) testCases.Add("TC 1.52");
                if (TC153CheckBox.Checked) testCases.Add("TC 1.53");
                if (TC154CheckBox.Checked) testCases.Add("TC 1.54");
                if (TC155CheckBox.Checked) testCases.Add("TC 1.55");
                if (TC156CheckBox.Checked) testCases.Add("TC 1.56");
                if (TC157CheckBox.Checked) testCases.Add("TC 1.57");
                if (TC158CheckBox.Checked) testCases.Add("TC 1.58");
                if (TC159CheckBox.Checked) testCases.Add("TC 1.59");
                if (TC160CheckBox.Checked) testCases.Add("TC 1.60");
                if (TC161CheckBox.Checked) testCases.Add("TC 1.61");
                if (TC162CheckBox.Checked) testCases.Add("TC 1.62");
                if (TC163CheckBox.Checked) testCases.Add("TC 1.63");
                if (TC164CheckBox.Checked) testCases.Add("TC 1.64");
                if (TC165CheckBox.Checked) testCases.Add("TC 1.65");
                if (TC166CheckBox.Checked) testCases.Add("TC 1.66");
                if (TC167CheckBox.Checked) testCases.Add("TC 1.67");
                if (TC168CheckBox.Checked) testCases.Add("TC 1.68");
                if (TC169CheckBox.Checked) testCases.Add("TC 1.69");
                if (TC170CheckBox.Checked) testCases.Add("TC 1.70");
                if (TC171CheckBox.Checked) testCases.Add("TC 1.71");
                if (TC172CheckBox.Checked) testCases.Add("TC 1.72");
                if (TC173CheckBox.Checked) testCases.Add("TC 1.73");
                if (TC174CheckBox.Checked) testCases.Add("TC 1.74");
                if (TC175CheckBox.Checked) testCases.Add("TC 1.75");
                if (TC176CheckBox.Checked) testCases.Add("TC 1.76");
                if (TC177CheckBox.Checked) testCases.Add("TC 1.77");
                if (TC178CheckBox.Checked) testCases.Add("TC 1.78");
                if (TC179CheckBox.Checked) testCases.Add("TC 1.79");
                if (TC180CheckBox.Checked) testCases.Add("TC 1.80");
                if (TC181CheckBox.Checked) testCases.Add("TC 1.81");
                if (TC182CheckBox.Checked) testCases.Add("TC 1.82");
                if (TC183CheckBox.Checked) testCases.Add("TC 1.83");
                if (TC184CheckBox.Checked) testCases.Add("TC 1.84");
                if (TC185CheckBox.Checked) testCases.Add("TC 1.85");
                if (TC186CheckBox.Checked) testCases.Add("TC 1.86");
                if (TC187CheckBox.Checked) testCases.Add("TC 1.87");
                if (TC188CheckBox.Checked) testCases.Add("TC 1.88");
                if (TC189CheckBox.Checked) testCases.Add("TC 1.89");

                foreach (var testCase in testCases)
                {
                    var tasks = new List<Task>();

                    // DUT-only test cases
                    if (testCase == "TC 1.1" || testCase == "TC 1.2" || testCase == "TC 1.3" || testCase == "TC 1.23" || testCase == "TC 1.54" ||
                        testCase == "TC 1.72" || testCase == "TC 1.73" || testCase == "TC 1.74" || testCase == "TC 1.75" || testCase == "TC 1.76" ||
                        testCase == "TC 1.77" || testCase == "TC 1.78" || testCase == "TC 1.79" || testCase == "TC 1.80" || testCase == "TC 1.81" ||
                        testCase == "TC 1.82" || testCase == "TC 1.83" || testCase == "TC 1.84" || testCase == "TC 1.85" || testCase == "TC 1.86" ||
                        testCase == "TC 1.87" || testCase == "TC 1.88" || testCase == "TC 1.89")
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
                                    case "TC 1.54":
                                        new TC_1_54(dut, outputRTB, TC154BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC154CheckBox, TC154BTN);
                                        break;
                                    case "TC 1.72":
                                        new TC_1_72(dut, outputRTB, TC172BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC172CheckBox, TC172BTN);
                                        break;
                                    case "TC 1.73":
                                        new TC_1_73(dut, outputRTB, TC173BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC173CheckBox, TC173BTN);
                                        break;
                                    case "TC 1.74":
                                        new TC_1_74(dut, outputRTB, TC174BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC174CheckBox, TC174BTN);
                                        break;
                                    case "TC 1.75":
                                        new TC_1_75(dut, outputRTB, TC175BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC175CheckBox, TC175BTN);
                                        break;
                                        case "TC 1.76":
                                        new TC_1_76(dut, outputRTB, TC176BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC176CheckBox, TC176BTN);
                                        break;
                                        case "TC 1.77":
                                        new TC_1_77(dut, outputRTB, TC177BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC177CheckBox, TC177BTN);
                                        break;
                                        case "TC 1.78":
                                        new TC_1_78(dut, outputRTB, TC178BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC178CheckBox, TC178BTN);
                                        break;
                                        case "TC 1.79":
                                        new TC_1_79(dut, outputRTB, TC179BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC179CheckBox, TC179BTN);
                                        break;
                                        case "TC 1.80":
                                        new TC_1_80(dut, outputRTB, TC180BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC180CheckBox, TC180BTN);
                                        break;
                                        case "TC 1.81":
                                        new TC_1_81(dut, outputRTB, TC181BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC181CheckBox, TC181BTN);
                                        break;
                                        case "TC 1.82":
                                        new TC_1_82(dut, outputRTB, TC182BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC182CheckBox, TC182BTN);
                                        break;
                                        case "TC 1.83":
                                        new TC_1_83(dut, outputRTB, TC183BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC183CheckBox, TC183BTN);
                                        break;
                                        case "TC 1.84":
                                        new TC_1_84(dut, outputRTB, TC184BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC184CheckBox, TC184BTN);
                                        break;
                                        case "TC 1.85":
                                        new TC_1_85(dut, outputRTB, TC185BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC185CheckBox, TC185BTN);
                                        break;
                                        case "TC 1.86":
                                        new TC_1_86(dut, outputRTB, TC186BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC186CheckBox, TC186BTN);
                                        break;
                                        case "TC 1.87":
                                        new TC_1_87(dut, outputRTB, TC187BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC187CheckBox, TC187BTN);
                                        break;
                                        case "TC 1.88":
                                        new TC_1_88(dut, outputRTB, TC188BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC188CheckBox, TC188BTN);
                                        break;
                                        case "TC 1.89":
                                        new TC_1_89(dut, outputRTB, TC189BTN).RunTestAsync();
                                        UpdateCheckBoxColor(TC189CheckBox, TC189BTN);
                                        break;

                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF paired test cases
                    else if (new[] {
                "TC 1.4","TC 1.5","TC 1.6","TC 1.7","TC 1.10","TC 1.11","TC 1.12","TC 1.13","TC 1.14","TC 1.15",
                "TC 1.16","TC 1.18","TC 1.19","TC 1.20","TC 1.21", "TC 1.25", "TC 1.26", "TC 1.27", "TC 1.30", "TC 1.31",
                        "TC 1.32", "TC 1.33", "TC 1.34", "TC 1.35", "TC 1.36", "TC 1.37", "TC 1.38", "TC 1.39", "TC 1.40", "TC 1.41",
                        "TC 1.42", "TC 1.43", "TC 1.44", "TC 1.45", "TC 1.46", "TC 1.47", "TC 1.48", "TC 1.49", "TC 1.50", "TC 1.51", "TC 1.52", "TC 1.53", 
                        "TC 1.55", "TC 1.56", "TC 1.57", "TC 1.58", "TC 1.69", "TC 1.70", "TC 1.71"
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
                            tasks.Add(Task.Run(async () =>
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
                                        await Task.Run(() => new TC_1_30(dut, refDev, outputRTB, TC130BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC130CheckBox, TC130BTN);
                                        break;
                                    case "TC 1.31":
                                        await Task.Run(() => new TC_1_31(dut, refDev, outputRTB, TC131BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC131CheckBox, TC131BTN);
                                        break;
                                    case "TC 1.32":
                                        await Task.Run(() => new TC_1_32(dut, refDev, outputRTB, TC132BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC132CheckBox, TC132BTN);
                                        break;
                                        case "TC 1.33":
                                        await Task.Run(() => new TC_1_33(dut, refDev, outputRTB, TC133BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC133CheckBox, TC133BTN);
                                        break;
                                    case "TC 1.34":
                                        await Task.Run(() => new TC_1_34(dut, refDev, outputRTB, TC134BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC134CheckBox, TC134BTN);
                                        break;
                                    case "TC 1.35":
                                        await Task.Run(() => new TC_1_35(dut, refDev, outputRTB, TC135BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC135CheckBox, TC135BTN);
                                        break;
                                    case "TC 1.36":
                                        await Task.Run(() => new TC_1_36(dut, refDev, outputRTB, TC136BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC136CheckBox, TC136BTN);
                                        break;
                                    case "TC 1.37":
                                                await Task.Run(() => new TC_1_37(dut, refDev, outputRTB, TC137BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC137CheckBox, TC137BTN);
                                        break;
                                    case "TC 1.38":
                                        await Task.Run(() => new TC_1_38(dut, refDev, outputRTB, TC138BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC138CheckBox, TC138BTN);
                                        break;
                                    case "TC 1.39":
                                        await Task.Run(() => new TC_1_39(dut, refDev, outputRTB, TC139BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC139CheckBox, TC139BTN);
                                        break;
                                    case "TC 1.40":
                                        new TC_1_40(dut, refDev, outputRTB, TC140BTN).RunTest();
                                        UpdateCheckBoxColor(TC140CheckBox, TC140BTN);
                                        break;
                                    case "TC 1.41":
                                        new TC_1_41(dut, refDev, outputRTB, TC141BTN).RunTest();
                                        UpdateCheckBoxColor(TC141CheckBox, TC141BTN);
                                        break;
                                    case "TC 1.42":
                                        new TC_1_42(dut, refDev, outputRTB, TC142BTN).RunTest();
                                        UpdateCheckBoxColor(TC142CheckBox, TC142BTN);
                                        break;
                                    case "TC 1.43":
                                        new TC_1_43(dut, refDev, outputRTB, TC143BTN).RunTest();
                                        UpdateCheckBoxColor(TC143CheckBox, TC143BTN);
                                        break;
                                    case "TC 1.44":
                                        new TC_1_44(dut, refDev, outputRTB, TC144BTN).RunTest();
                                        UpdateCheckBoxColor(TC144CheckBox, TC144BTN);
                                        break;
                                    case "TC 1.45":
                                        await Task.Run(() => new TC_1_45(dut, refDev, outputRTB, TC145BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC145CheckBox, TC145BTN);
                                        break;
                                    case "TC 1.46":
                                        await Task.Run(() => new TC_1_46(dut, refDev, outputRTB, TC146BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC146CheckBox, TC146BTN);
                                        break;
                                    case "TC 1.47":
                                        await Task.Run(() => new TC_1_47(dut, refDev, outputRTB, TC147BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC147CheckBox, TC147BTN);
                                        break;
                                    case "TC 1.48":
                                        await Task.Run(() => new TC_1_48(dut, refDev, outputRTB, TC148BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC148CheckBox, TC148BTN);
                                        break;
                                    case "TC 1.49":
                                        await Task.Run(() => new TC_1_49(dut, refDev, outputRTB, TC149BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC149CheckBox, TC149BTN);
                                        break;
                                    case "TC 1.50":
                                        await Task.Run(() => new TC_1_50(dut, refDev, outputRTB, TC150BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC150CheckBox, TC150BTN);
                                        break;
                                    case "TC 1.51":
                                        await Task.Run(() => new TC_1_51(dut, refDev, outputRTB, TC151BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC151CheckBox, TC151BTN);
                                        break;
                                    case "TC 1.52":
                                        await Task.Run(() => new TC_1_52(dut, refDev, outputRTB, TC152BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC152CheckBox, TC152BTN);
                                        break;
                                    case "TC 1.53":
                                        await Task.Run(() => new TC_1_53(dut, refDev, outputRTB, TC153BTN).RunTestAsync());
                                        UpdateCheckBoxColor(TC153CheckBox, TC153BTN);
                                        break;
                                    case "TC 1.55":
                                        new TC_1_55(dut, refDev, outputRTB, TC155BTN).RunTest();
                                        UpdateCheckBoxColor(TC155CheckBox, TC155BTN);
                                        break;
                                        case "TC 1.56":
                                        new TC_1_56(dut, refDev, outputRTB, TC156BTN).RunTest();
                                        UpdateCheckBoxColor(TC156CheckBox, TC156BTN);
                                        break;
                                        case "TC 1.57":
                                        new TC_1_57(dut, refDev, outputRTB, TC157BTN).RunTest();
                                        UpdateCheckBoxColor(TC157CheckBox, TC157BTN);
                                        break;
                                        case "TC 1.58":
                                        new TC_1_58(dut, refDev, outputRTB, TC158BTN).RunTest();
                                        UpdateCheckBoxColor(TC158CheckBox, TC158BTN);
                                        break;
                                        case "TC 1.69":
                                        new TC_1_69(dut, outputRTB, TC169BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC169CheckBox, TC169BTN);
                                        break;
                                        case "TC 1.70":
                                        new TC_1_70(dut, outputRTB, TC170BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC170CheckBox, TC170BTN);
                                        break;
                                        case "TC 1.71":
                                        new TC_1_71(dut, outputRTB, TC171BTN, refDev).RunTest();
                                        UpdateCheckBoxColor(TC171CheckBox, TC171BTN);
                                        break;

                                }
                            }, _runCts.Token));
                        }
                    }
                    // DUT/REF/MO paired test cases
                    else if (new[] { "TC 1.8", "TC 1.17", "TC 1.22", "TC 1.24", "TC 1.28", "TC 1.29", "TC 1.59", "TC 1.60", "TC 1.61", "TC 1.62", "TC 1.63", "TC 1.64",
                    "TC 1.65", "TC 1.66", "TC 1.67", "TC 1.68"}.Contains(testCase))
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
                                    case "TC 1.59":
                                        new TC_1_59(dut, refDev, moDev, outputRTB, TC159BTN).RunTest();
                                        UpdateCheckBoxColor(TC159CheckBox, TC159BTN);
                                        break;
                                    case "TC 1.60":
                                        new TC_1_60(dut, refDev, moDev, outputRTB, TC160BTN).RunTest();
                                        UpdateCheckBoxColor(TC160CheckBox, TC160BTN);
                                        break;
                                    case "TC 1.61":
                                        new TC_1_61(dut, refDev, moDev, outputRTB, TC161BTN).RunTest();
                                        UpdateCheckBoxColor(TC161CheckBox, TC161BTN);
                                        break;
                                    case "TC 1.62":
                                        new TC_1_62(dut, refDev, moDev, outputRTB, TC162BTN).RunTest();
                                        UpdateCheckBoxColor(TC162CheckBox, TC162BTN);
                                        break;
                                    case "TC 1.63":
                                        new TC_1_63(dut, refDev, moDev, outputRTB, TC163BTN).RunTest();
                                        UpdateCheckBoxColor(TC163CheckBox, TC163BTN);
                                        break;

                                    case "1.64":
                                        new TC_1_64(dut, refDev, moDev, outputRTB, TC164BTN).RunTest();
                                        UpdateCheckBoxColor(TC164CheckBox, TC164BTN);
                                        break;
                                    case "1.65":
                                        new TC_1_65(dut, refDev, moDev, outputRTB, TC165BTN).RunTest();
                                        UpdateCheckBoxColor(TC165CheckBox, TC165BTN);
                                        break;
                                    case "1.66":
                                        new TC_1_66(dut, refDev, moDev, outputRTB, TC166BTN).RunTest();
                                        UpdateCheckBoxColor(TC166CheckBox, TC166BTN);
                                        break;
                                    case "1.67":
                                        new TC_1_67(dut, refDev, moDev, outputRTB, TC167BTN).RunTest();
                                        UpdateCheckBoxColor(TC167CheckBox, TC167BTN);
                                        break;
                                    case "1.68":
                                        new TC_1_68(dut, refDev, moDev, outputRTB, TC168BTN).RunTest();
                                        UpdateCheckBoxColor(TC168CheckBox, TC168BTN);
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

                    await Task.Run(async () =>
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
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                        }
                                    case "1.31":
                                        {
                                        var t = new TC_1_31(dutId, refId, outputRTB, TC131BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.32":
                                        {
                                        var t = new TC_1_32(dutId, refId, outputRTB, TC132BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.33":
                                        {
                                        var t = new TC_1_33(dutId, refId, outputRTB, TC133BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.34":
                                    {
                                        var t = new TC_1_34(dutId, refId, outputRTB, TC134BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    
                                    case "1.35":
                                        {
                                        var t = new TC_1_35(dutId, refId, outputRTB, TC135BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                        }
                                    case "1.36":
                                        {
                                        var t = new TC_1_36(dutId, refId, outputRTB, TC136BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.37":
                                        {
                                        var t = new TC_1_37(dutId, refId, outputRTB, TC137BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.38":
                                        {
                                        var t = new TC_1_38(dutId, refId, outputRTB, TC138BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.39":
                                        {
                                        var t = new TC_1_39(dutId, refId, outputRTB, TC139BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.40+":
                                        {
                                        var t = new TC_1_40(dutId, refId, outputRTB, TC140BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.41":
                                        {
                                        var t = new TC_1_41(dutId, refId, outputRTB, TC141BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.42":
                                        {
                                        var t = new TC_1_42(dutId, refId, outputRTB, TC142BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.43":
                                        {
                                        var t = new TC_1_43(dutId, refId, outputRTB, TC143BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.44":
                                        {
                                        var t = new TC_1_44(dutId, refId, outputRTB, TC144BTN);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.45":
                                    {
                                        var t = new TC_1_45(dutId, refId, outputRTB, TC138BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                case "1.46":
                                    {
                                        var t = new TC_1_46(dutId, refId, outputRTB, TC139BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.47":
                                        {
                                        var t = new TC_1_47(dutId, refId, outputRTB, TC147BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.48":
                                        {
                                        var t = new TC_1_48(dutId, refId, outputRTB, TC148BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.49":
                                        {
                                        var t = new TC_1_49(dutId, refId, outputRTB, TC149BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.50+":
                                        {
                                        var t = new TC_1_50(dutId, refId, outputRTB, TC150BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.51":
                                        {
                                        var t = new TC_1_51(dutId, refId, outputRTB, TC151BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.52":
                                        {
                                        var t = new TC_1_52(dutId, refId, outputRTB, TC152BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.53":
                                        {
                                        var t = new TC_1_53(dutId, refId, outputRTB, TC153BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.54":
                                        {
                                        var t = new TC_1_54(dutId, outputRTB, TC154BTN);
                                        await Task.Run(() => t.RunTestAsync());
                                        break;
                                    }
                                    case "1.55":
                                        {
                                        var t = new TC_1_55(dutId, refId, outputRTB, TC155BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.56":
                                        {
                                        var t = new TC_1_56(dutId, refId, outputRTB, TC156BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.57":
                                        {
                                        var t = new TC_1_57(dutId, refId, outputRTB, TC157BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.58":
                                        {
                                        var t = new TC_1_58(dutId, refId, outputRTB, TC158BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.59":
                                        {
                                        var t = new TC_1_59(dutId, refId, moCallerId, outputRTB, TC159BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.60+":
                                        {
                                        var t = new TC_1_60(dutId, refId, moCallerId, outputRTB, TC160BTN);
                                        await Task.Run(() => t.RunTest());
                                        break;
                                    }
                                    case "1.61":
                                        {
                                        var t = new TC_1_61(dutId, refId, moCallerId, outputRTB, TC161BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.62":
                                    {
                                        var t = new TC_1_62(dutId, refId, moCallerId, outputRTB, TC162BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.63":
                                    {
                                        var t = new TC_1_63(dutId, refId, moCallerId, outputRTB, TC163BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.64":
                                    {
                                        var t = new TC_1_64(dutId, refId, moCallerId, outputRTB, TC164BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.65":
                                    {
                                        var t = new TC_1_65(dutId, refId, moCallerId, outputRTB, TC165BTN);
                                        t.RunTest();
                                        break;
                                    }
                                    case "1.66":
                                    {
                                        var t = new TC_1_66(dutId, refId, moCallerId, outputRTB, TC166BTN);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.67":
                                    {
                                        var t = new TC_1_67(dutId, refId, moCallerId, outputRTB, TC167BTN);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.68":
                                    {
                                        var t = new TC_1_68(dutId, refId, moCallerId, outputRTB, TC168BTN);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.69":
                                    {
                                        var t = new TC_1_69(dutId, outputRTB, TC169BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.70+":
                                    {
                                        var t = new TC_1_70(dutId, outputRTB, TC170BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.71":
                                    {
                                        var t = new TC_1_71(dutId, outputRTB, TC171BTN, refId);
                                        t.RunTest();
                                        break;
                                    }
                                case "1.72":
                                    {
                                        var t = new TC_1_72(dutId, outputRTB, TC172BTN);
                                        t.RunTestAsync();
                                        break;
                                    }
                                case "1.73":
                                    {
                                        var t = new TC_1_73(dutId, outputRTB, TC173BTN);
                                        t.RunTestAsync();
                                        break;
                                    }
                                case "1.74":
                                    {
                                        var t = new TC_1_74(dutId, outputRTB, TC174BTN);
                                        t.RunTestAsync();
                                        break;
                                    }
                                case "1.75":
                                    {
                                        var t = new TC_1_75(dutId, outputRTB, TC175BTN);
                                        t.RunTestAsync();
                                        break;
                                    }
                                    case "1.76":
                                    {
                                        var t = new TC_1_76(dutId, outputRTB, TC176BTN);
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
            Upload.SelectTab(1);

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
            TC131CheckBox.Checked = true;
            TC132CheckBox.Checked = true;
            TC133CheckBox.Checked = true;
            TC134CheckBox.Checked = true;
            TC135CheckBox.Checked = true;
            TC136CheckBox.Checked = true;
            TC137CheckBox.Checked = true;
            TC138CheckBox.Checked = true;
            TC139CheckBox.Checked = true;
            TC140CheckBox.Checked = true;
            TC141CheckBox.Checked = true;
            TC142CheckBox.Checked = true;
            TC143CheckBox.Checked = true;
            TC144CheckBox.Checked = true;
            TC145CheckBox.Checked = true;
            TC146CheckBox.Checked = true;
            TC147CheckBox.Checked = true;
            TC148CheckBox.Checked = true;
            TC149CheckBox.Checked = true;
            TC150CheckBox.Checked = true;
            TC151CheckBox.Checked = true;
            TC152CheckBox.Checked = true;
            TC153CheckBox.Checked = true;
            TC154CheckBox.Checked = true;
            TC155CheckBox.Checked = true;
            TC156CheckBox.Checked = true;
            TC157CheckBox.Checked = true;
            TC158CheckBox.Checked = true;
            TC159CheckBox.Checked = true;
            TC160CheckBox.Checked = true;
            TC161CheckBox.Checked = true;
            TC162CheckBox.Checked = true;
            TC163CheckBox.Checked = true;
            TC164CheckBox.Checked = true;
            TC165CheckBox.Checked = true;
            TC166CheckBox.Checked = true;
            TC167CheckBox.Checked = true;
            TC168CheckBox.Checked = true;
            TC169CheckBox.Checked = true;
            TC170CheckBox.Checked = true;
            TC171CheckBox.Checked = true;
            TC172CheckBox.Checked = true;
            TC173CheckBox.Checked = true;
            TC174CheckBox.Checked = true;
            TC175CheckBox.Checked = true;
            TC176CheckBox.Checked = true;
            TC177CheckBox.Checked = true;
            TC178CheckBox.Checked = true;
            TC179CheckBox.Checked = true;
            TC180CheckBox.Checked = true;
            TC181CheckBox.Checked = true;
            TC182CheckBox.Checked = true;
            TC183CheckBox.Checked = true;
            TC184CheckBox.Checked = true;
            TC185CheckBox.Checked = true;
            TC186CheckBox.Checked = true;
            TC187CheckBox.Checked = true;
            TC188CheckBox.Checked = true;
            TC189CheckBox.Checked = true;
        }

        private void clearAllTCCheckBoxes()
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
            TC131CheckBox.Checked = false;
            TC132CheckBox.Checked = false;
            TC133CheckBox.Checked = false;
            TC134CheckBox.Checked = false;
            TC135CheckBox.Checked = false;
            TC136CheckBox.Checked = false;
            TC137CheckBox.Checked = false;
            TC138CheckBox.Checked = false;
            TC139CheckBox.Checked = false;
            TC140CheckBox.Checked = false;
            TC141CheckBox.Checked = false;
            TC142CheckBox.Checked = false;
            TC143CheckBox.Checked = false;
            TC144CheckBox.Checked = false;
            TC145CheckBox.Checked = false;
            TC146CheckBox.Checked = false;
            TC147CheckBox.Checked = false;
            TC148CheckBox.Checked = false;
            TC149CheckBox.Checked = false;
            TC150CheckBox.Checked = false;
            TC151CheckBox.Checked = false;
            TC152CheckBox.Checked = false;
            TC153CheckBox.Checked = false;
            TC154CheckBox.Checked = false;
            TC155CheckBox.Checked = false;
            TC156CheckBox.Checked = false;
            TC157CheckBox.Checked = false;
            TC158CheckBox.Checked = false;
            TC159CheckBox.Checked = false;
            TC160CheckBox.Checked = false;
            TC161CheckBox.Checked = false;
            TC162CheckBox.Checked = false;
            TC163CheckBox.Checked = false;
            TC164CheckBox.Checked = false;
            TC165CheckBox.Checked = false;
            TC166CheckBox.Checked = false;
            TC167CheckBox.Checked = false;
            TC168CheckBox.Checked = false;
            TC169CheckBox.Checked = false;
            TC170CheckBox.Checked = false;
            TC171CheckBox.Checked = false;
            TC172CheckBox.Checked = false;
            TC173CheckBox.Checked = false;
            TC174CheckBox.Checked = false;
            TC175CheckBox.Checked = false;
            TC176CheckBox.Checked = false;
            TC177CheckBox.Checked = false;
            TC178CheckBox.Checked = false;
            TC179CheckBox.Checked = false;
            TC180CheckBox.Checked = false;
            TC181CheckBox.Checked = false;
            TC182CheckBox.Checked = false;
            TC183CheckBox.Checked = false;
            TC184CheckBox.Checked = false;
            TC185CheckBox.Checked = false;
            TC186CheckBox.Checked = false;
            TC187CheckBox.Checked = false;
            TC188CheckBox.Checked = false;
            TC189CheckBox.Checked = false;
        }

        private void ClearAllTCsBTN_Click(object sender, EventArgs e)
        {
            clearAllTCCheckBoxes();
        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void TC139CheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        // Make certain test cases checked for sanity check
        private void SanityChkBTN_Click(object sender, EventArgs e)
        {
            Upload.SelectTab(1);

            TC12CheckBox.Checked = true;
            TC14CheckBox.Checked = true;
            TC16CheckBox.Checked = true;
            TC111CheckBox.Checked = true;
            TC113CheckBox.Checked = true;
            TC115CheckBox.Checked = true;
            TC120CheckBox.Checked = true;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void CheckAllDUTOnlyBoxes_Click(object sender, EventArgs e)
        {
            Upload.SelectTab(1);

            TC11CheckBox.Checked = true;
            TC12CheckBox.Checked = true;
            TC13CheckBox.Checked = true;
            TC123CheckBox.Checked = true;
            TC154CheckBox.Checked = true;
            TC172CheckBox.Checked = true;
            TC173CheckBox.Checked = true;
            TC174CheckBox.Checked = true;
            TC175CheckBox.Checked = true;
            TC176CheckBox.Checked = true;
            TC177CheckBox.Checked = true;
            TC178CheckBox.Checked = true;
            TC179CheckBox.Checked = true;
            TC180CheckBox.Checked = true;
            TC181CheckBox.Checked = true;
            TC182CheckBox.Checked = true;
            TC183CheckBox.Checked = true;
            TC184CheckBox.Checked = true;
            TC185CheckBox.Checked = true;
            TC186CheckBox.Checked = true;
            TC187CheckBox.Checked = true;
            TC188CheckBox.Checked = true;
            TC189CheckBox.Checked = true;
        }
        /*
         *    "TC 1.4","TC 1.5","TC 1.6","TC 1.7","TC 1.10","TC 1.11","TC 1.12","TC 1.13","TC 1.14","TC 1.15",
                "TC 1.16","TC 1.18","TC 1.19","TC 1.20","TC 1.21", "TC 1.25", "TC 1.26", "TC 1.27", "TC 1.30", "TC 1.31",
                        "TC 1.32", "TC 1.33", "TC 1.34", "TC 1.35", "TC 1.36", "TC 1.37", "TC 1.38", "TC 1.39", "TC 1.40", "TC 1.41",
                        "TC 1.42", "TC 1.43", "TC 1.44", "TC 1.45", "TC 1.46", "TC 1.47", "TC 1.48", "TC 1.49", "TC 1.50", "TC 1.51", "TC 1.52", "TC 1.53", 
                        "TC 1.55", "TC 1.56", "TC 1.57", "TC 1.58"
         */
        private void CheckAllDUTAndREFOnlyBoxes_Click(object sender, EventArgs e)
        {
            Upload.SelectTab(1);

            TC14CheckBox.Checked = true;
            TC15CheckBox.Checked = true;
            TC16CheckBox.Checked = true;
            TC17CheckBox.Checked = true;
            TC110CheckBox.Checked = true;
            TC111CheckBox.Checked = true;
            TC112CheckBox.Checked = true;
            TC113CheckBox.Checked = true;
            TC114CheckBox.Checked = true;
            TC115CheckBox.Checked = true;
            TC116CheckBox.Checked = true;
            TC118CheckBox.Checked = true;
            TC119CheckBox.Checked = true;
            TC120CheckBox.Checked = true;
            TC121CheckBox.Checked = true;
            TC125CheckBox.Checked = true;
            TC126CheckBox.Checked = true;
            TC127CheckBox.Checked = true;
            TC130CheckBox.Checked = true;
            TC131CheckBox.Checked = true;
            TC132CheckBox.Checked = true;
            TC133CheckBox.Checked = true;
            TC134CheckBox.Checked = true;
            TC135CheckBox.Checked = true;
            TC136CheckBox.Checked = true;
            TC137CheckBox.Checked = true;
            TC138CheckBox.Checked = true;
            TC139CheckBox.Checked = true;
            TC140CheckBox.Checked = true;
            TC141CheckBox.Checked = true;
            TC142CheckBox.Checked = true;
            TC143CheckBox.Checked = true;
            TC144CheckBox.Checked = true;
            TC145CheckBox.Checked = true;
            TC146CheckBox.Checked = true;
            TC147CheckBox.Checked = true;
            TC148CheckBox.Checked = true;
            TC149CheckBox.Checked = true;
            TC150CheckBox.Checked = true;
            TC151CheckBox.Checked = true;
            TC152CheckBox.Checked = true;
            TC153CheckBox.Checked = true;
            TC155CheckBox.Checked = true;
            TC156CheckBox.Checked = true;
            TC157CheckBox.Checked = true;
            TC158CheckBox.Checked = true;
            TC169CheckBox.Checked = true;
            TC170CheckBox.Checked = true;
            TC171CheckBox.Checked = true;

        }

        /*
         * "TC 1.8", "TC 1.17", "TC 1.22", "TC 1.24", "TC 1.28", "TC 1.29", "TC 1.59", "TC 1.60"
         */
        private void CheckAllDUTREFAndMOOnlyBoxes_Click(object sender, EventArgs e)
        {
            Upload.SelectTab(1);

            TC18CheckBox.Checked = true;
            TC117CheckBox.Checked = true;
            TC122CheckBox.Checked = true;
            TC124CheckBox.Checked = true;
            TC128CheckBox.Checked = true;
            TC129CheckBox.Checked = true;
            TC159CheckBox.Checked = true;
            TC160CheckBox.Checked = true;
            TC161CheckBox.Checked = true;
            TC162CheckBox.Checked = true;
            TC163CheckBox.Checked = true;
            TC164CheckBox.Checked = true;
            TC165CheckBox.Checked = true;
            TC166CheckBox.Checked = true;
            TC167CheckBox.Checked = true;
            TC168CheckBox.Checked = true;
        }

        /*
        private void SelectTCsCategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            if(SelectTCsCategoryComboBox.SelectedItem == "CAF")
            {
                TC117CheckBox.Checked = true;
                TC122CheckBox.Checked = true;
            }
            else if(SelectTCsCategoryComboBox.SelectedItem == "CAH")
            {
                TC135CheckBox.Checked = true;
                TC136CheckBox.Checked = true;
                TC137CheckBox.Checked = true;
                TC138CheckBox.Checked = true;
                TC139CheckBox.Checked = true;
            }
            else if(SelectTCsCategoryComboBox.SelectedItem == "CAL")
            {
                TC14CheckBox.Checked = true;
                TC15CheckBox.Checked = true;
                TC116CheckBox.Checked = true;
                TC118CheckBox.Checked = true;
                TC119CheckBox.Checked = true;
                TC145CheckBox.Checked = true;
                TC146CheckBox.Checked = true;
                TC147CheckBox.Checked = true;
                TC148CheckBox.Checked = true;
                TC149CheckBox.Checked = true;
                TC150CheckBox.Checked = true;
                TC151CheckBox.Checked = true;
                TC152CheckBox.Checked = true;
                TC153CheckBox.Checked = true;
            }
        }
        */
        private void SelectTCsCategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearAllTCCheckBoxes();
            Upload.SelectTab(1);
            if (SelectTCsCategoryComboBox.SelectedItem == "CAF")
            {
                TC117CheckBox.Checked = true;
                TC122CheckBox.Checked = true;
                TC161CheckBox.Checked = true;
                TC162CheckBox.Checked = true;
                TC163CheckBox.Checked = true;
                TC164CheckBox.Checked = true;
                TC165CheckBox.Checked = true;
                TC166CheckBox.Checked = true;
                TC167CheckBox.Checked = true;
                TC168CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CAH")
            {
                TC135CheckBox.Checked = true;
                TC136CheckBox.Checked = true;
                TC137CheckBox.Checked = true;
                TC138CheckBox.Checked = true;
                TC139CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CAL")
            {
                TC14CheckBox.Checked = true;
                TC15CheckBox.Checked = true;
                TC116CheckBox.Checked = true;
                TC118CheckBox.Checked = true;
                TC119CheckBox.Checked = true;
                TC145CheckBox.Checked = true;
                TC146CheckBox.Checked = true;
                TC147CheckBox.Checked = true;
                TC148CheckBox.Checked = true;
                TC149CheckBox.Checked = true;
                TC150CheckBox.Checked = true;
                TC151CheckBox.Checked = true;
                TC152CheckBox.Checked = true;
                TC153CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CAS")
            {
                TC129CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CAW")
            {
                TC128CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CNT")
            {
                TC157CheckBox.Checked = true;
                TC158CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "CON")
            {
                TC159CheckBox.Checked = true;
                TC160CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "IRD")
            {
                TC172CheckBox.Checked = true;
                TC173CheckBox.Checked = true;
                TC174CheckBox.Checked = true;
                TC175CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "MIS")
            {
                TC154CheckBox.Checked = true;
                TC155CheckBox.Checked = true;
                TC156CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "MMS")
            {
                TC113CheckBox.Checked = true;
                TC114CheckBox.Checked = true;
                TC171CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "REG")
            {
                TC11CheckBox.Checked = true;
                TC12CheckBox.Checked = true;
                TC13CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "SMS")
            {
                TC16CheckBox.Checked = true;
                TC17CheckBox.Checked = true;
                TC110CheckBox.Checked = true;
                TC111CheckBox.Checked = true;
                TC112CheckBox.Checked = true;
                TC169CheckBox.Checked = true;
                TC170CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "TOG")
            {
                TC125CheckBox.Checked = true;
                TC126CheckBox.Checked = true;
                TC127CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "VCL")
            {
                TC120CheckBox.Checked = true;
                TC121CheckBox.Checked = true;
                TC124CheckBox.Checked = true;
                TC140CheckBox.Checked = true;
                TC141CheckBox.Checked = true;
                TC142CheckBox.Checked = true;
                TC143CheckBox.Checked = true;
                TC144CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "VOM")
            {
                TC130CheckBox.Checked = true;
                TC131CheckBox.Checked = true;
                TC132CheckBox.Checked = true;
                TC133CheckBox.Checked = true;
                TC134CheckBox.Checked = true;
            }
            else if (SelectTCsCategoryComboBox.SelectedItem == "XCAP")
            {
                TC18CheckBox.Checked = true;
                TC123CheckBox.Checked = true;
            }
        }

    }
}