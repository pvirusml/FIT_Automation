/*
 * TC_1_47: Message Call rejection with Voicemail Notification: DUT1(VoWiFi) to DUT2(VOLTE)
 * -----------------------------------------------------
 * 
1) Make a call from DUT 1 to DUT 2
2) On DUT 2, decline call with SMS
3) Ensure DUT 1 receives SMS
4) DUT 1 should be redirected to DUT 2's voicemail
5) Leave voicemail
6) On DUT 2, verify Voicemail notification is received and play voicemail
 */

using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_47
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_47(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
        {
            _dut1Id = dut1Id;
            _dut2Id = dut2Id;
            _outputRTB = outputRTB;
            _testButton = testButton;
            gclass = new GlobalVarClass(_dut1Id, _outputRTB, _testButton);
        }

        public async Task RunTestAsync()
        {
            result = "FAIL";

            lock (_lockObject)
            {
                if (!headerLogged)
                {
                    gclass.UpdateOutput("\n");
                    gclass.UpdateOutput("==================================================");
                    gclass.UpdateOutput("Starting TC 1.47: Message Call rejection with Voicemail Notification: DUT1(VoWiFi) to DUT2(VOLTE)");
                    gclass.UpdateOutput("==================================================\n");
                    headerLogged = true;
                }
            }

            try
            {
                // 1. Place a call from DUT 2 to DUT 1 and reject the call on DUT 1
                if (!gclass.IsDeviceConnected(_dut1Id) || !gclass.IsDeviceConnected(_dut2Id))
                {
                    throw new Exception($"DUT 1 or DUT 2 is not connected. [{_dut1Id}, {_dut2Id}]");
                }

                string dut2Number = gclass.ExtractPhoneNumber(_dut2Id);
                if (string.IsNullOrWhiteSpace(dut2Number))
                {
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut2Id}]");
                }

                // Properly handle airplane mode toggle and network stabilization
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000); // Wait for airplane mode to apply
                gclass.SetAirplaneMode(_dut1Id, false);
                gclass.SetAirplaneMode(_dut2Id, false);

                // Wait longer for both devices to fully reconnect to the network
                gclass.UpdateOutput("Waiting for devices to reconnect to network...");
                await Task.Delay(15000);

                gclass.EnableWiFi(_dut1Id);

                Thread.Sleep(15000); // Wait for network stabilization

                if (!gclass.PlaceCall(_dut1Id, dut2Number))
                {
                    throw new Exception($"Failed to place call from DUT 1 [{_dut1Id}] to DUT 2 [{_dut2Id}]");
                }

                await Task.Delay(4000); // Wait longer for the call to be registered

                // Swipe Down to access incoming call controls
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 300 0 300 1000 500");
                await Task.Delay(2000); // Wait for the notification shade to open

                // Select Incoming call from text ui dump
                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Incoming");
                await Task.Delay(10000); // Wait for the call screen to be ready

                // Swipe form bottom left to top right to reject with SMS
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 100 2374 919 331");
                // gclass.RunAdbCommand($"adb -s {_dut2Id} input swipe 50 2374 919 331");
                // gclass.RunAdbCommand($"adb -s {_dut2Id} input swipe 35 2395 919 331");
                //gclass.RunAdbCommand($"adb -s {_dut2Id} input swipe 100 2374 919 331");

                // Reject the call on DUT 1 using KEYCODE_ENDCALL
                //gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                // 2. Ensure that DUT 2 is redirected to DUT 1's voicemail
                await Task.Delay(3000); // Wait for voicemail system to answer

                gclass.SelectNodeWithTextFromUIDump(_dut2Id, "up?");



                // 3. Leave voicemail to DUT 1 
                gclass.UpdateOutput("Leaving voicemail for DUT 2...");

                // Use Task.Run to run the blocking Process.Start and timed WaitForExit on a background thread.
                Process youtubeProcess = null;
                await Task.Run(() =>
                {
                    string youtubeUrl = "https://www.youtube.com/watch?v=nbPBmNRH9KU";
                    youtubeProcess = Process.Start(youtubeUrl);

                    if (youtubeProcess != null)
                    {
                        // Wait for 15 seconds, or until the user closes the browser.
                        // The timeout is in milliseconds, so 15 seconds is 15000.
                        youtubeProcess.WaitForExit(25000);

                        // After the wait, check if the process is still running.
                        if (!youtubeProcess.HasExited)
                        {
                            gclass.UpdateOutput("YouTube video playback time elapsed. Closing browser window.");

                            // This is a graceful way to close the window.
                            youtubeProcess.CloseMainWindow();
                            youtubeProcess.Close();
                            youtubeProcess.Dispose();
                            youtubeProcess.Exited += (s, e) => youtubeProcess.Dispose();

                        }
                    }

                    youtubeProcess.Dispose();
                });

                // The YouTube video approach is unreliable and blocking. A better approach is to
                // simulate the voicemail tone and dialer interaction with timed ADB commands.
                // The following section simulates ending the call after leaving a message.
                // Consider replacing the above YouTube code with more reliable ADB commands.
                await Task.Delay(20000); // Simulate leaving a voicemail
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");



                await Task.Delay(3000); // Wait for the notification shade to open
                // Swipe down
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 300 0 300 1000 500");
                await Task.Delay(2000); // Wait for the notification shade to open

                gclass.CloseYouTubeVideoBrowser("msedge"); // Or "msedge", "firefox", etc.



                // 4. Verify DUT 2 receives Voicemail notification and play voicemail
                bool voicemailNotification = false;
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string uiDumpPath = $"{outputPath}\\ui_dump.xml";
                gclass.CaptureUIDump(_dut2Id, outputPath);
                voicemailNotification = gclass.isInUIDump(uiDumpPath, "Voicemail");

                if (!voicemailNotification)
                {
                    gclass.UpdateOutput("Voicemail notification NOT detected on DUT 2.");
                    throw new Exception("Voicemail notification not detected on DUT 2.");

                }
                else
                {
                    gclass.UpdateOutput("Voicemail notification detected on DUT 2.");
                    await Task.Delay(3000);

                    // Swipe down
                    //gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 584 983 574 1590");
                    //await Task.Delay(2000); // Wait for the notification shade to open

                    // Open Voicemail app
                    gclass.SelectNodeWithTextFromUIDump(_dut2Id, "Voicemail");
                    await Task.Delay(4000); // Wait for the voicemail app to open

                    // Click on 1 to play voicemal
                    gclass.SelectNodeWithTextFromUIDump(_dut2Id, "1");

                    await Task.Delay(3000); // Wait for voicemail to play

                    // Click on 7 to delete voicemail
                    gclass.SelectNodeWithTextFromUIDump(_dut2Id, "7");
                    await Task.Delay(3000); // Wait for voicemail to be deleted

                    // end call
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");

                    // Check sms and delete
                    gclass.CheckForReceivedSMSWithTargetBodyInputCheck(_dut1Id, _dut2Id, "Can't talk now. What's up?");
                    await Task.Delay(3000); // Wait for SMS to be sent

                    // Return to home screen
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");

                    gclass.UpdateOutput($"TC 1.47: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.47: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {
                // go to home screen
                gclass.resetAll(_dut2Id);
                gclass.resetAll(_dut1Id);
            }

            gclass.LogTestResultToCSV("TC1.47", _dut1Id, result);
        }
    }
}
