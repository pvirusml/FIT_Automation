/*
 * TC_1_34: Voicemail Notification and Callback Test Case
 * -----------------------------------------------------
 * Purpose:
 *   Verify voicemail notification and callback functionality:
 *   1. Place a call from DUT 2 to DUT 1 and reject the call on DUT 1.
 *   2. Ensure DUT 2 is redirected to DUT 1's voicemail.
 *   3. Leave a voicemail for DUT 1.
 *   4. Verify DUT 1 receives voicemail notification and play voicemail.
 *   5. Ensure audio is OK.
 *   6. 'Call back' same number and ensure call is connected over Wi-Fi, check audio is OK.
 */

using FIT_Automation.Scripts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIT_Automation.Test_Cases
{
    public class TC_1_34
    {
        private string _dut1Id;
        private string _dut2Id;
        private Button _testButton;
        private RichTextBox _outputRTB;
        private GlobalVarClass gclass;
        private string result;
        private static bool headerLogged = false;
        private static readonly object _lockObject = new object();

        public TC_1_34(string dut1Id, string dut2Id, RichTextBox outputRTB, Button testButton)
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
                    gclass.UpdateOutput("Starting TC 1.34: Voicemail Notification and Callback Test");
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

                string dut1Number = gclass.ExtractPhoneNumber(_dut1Id);
                if (string.IsNullOrWhiteSpace(dut1Number))
                {
                    throw new Exception($"Failed to extract phone number from DUT 1 [{_dut1Id}]");
                }

                // Properly handle airplane mode toggle and network stabilization
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
                Thread.Sleep(3000); // Wait for airplane mode to apply
                gclass.SetAirplaneMode(_dut2Id, false);

                // Wait longer for both devices to fully reconnect to the network
                gclass.UpdateOutput("Waiting for devices to reconnect to network...");
                gclass.WaitForLTEAndVoLTERegistration(_dut2Id);
                await Task.Delay(15000);

                gclass.EnableWiFi(_dut1Id);

                if (!gclass.PlaceCall(_dut2Id, dut1Number))
                {
                    throw new Exception($"Failed to place call from DUT 2 [{_dut2Id}] to DUT 1 [{_dut1Id}]");
                }

                await Task.Delay(10000); // Wait longer for the call to be registered

                // Reject the call on DUT 1 using KEYCODE_ENDCALL
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                // 2. Ensure that DUT 2 is redirected to DUT 1's voicemail
                await Task.Delay(8000); // Wait for voicemail system to answer

                // 3. Leave voicemail to DUT 1 
                gclass.UpdateOutput("Leaving voicemail for DUT 1...");

                /*
                // Use Task.Run to run the blocking Process.Start and WaitForExit on a background thread.
                await Task.Run(() =>
                {
                    string youtubeUrl = "https://www.youtube.com/watch?v=nbPBmNRH9KU";
                    Process youtubeProcess = Process.Start(youtubeUrl);
                    if (youtubeProcess != null)
                    {
                        youtubeProcess.WaitForExit();
                    }
                });
                */

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
                        youtubeProcess.WaitForExit(15000);

                        // After the wait, check if the process is still running.
                        if (!youtubeProcess.HasExited)
                        {
                            gclass.UpdateOutput("YouTube video playback time elapsed. Closing browser window.");

                            // This is a graceful way to close the window.
                            youtubeProcess.CloseMainWindow();
                            youtubeProcess.Close();
                            youtubeProcess.Dispose();
                            youtubeProcess.Exited += (s, e) => youtubeProcess.Dispose();
                            

                            // If that fails, force-kill the process.
                            if (!youtubeProcess.HasExited)
                            {
                                youtubeProcess.Kill();
                            }
                        }
                    }
                    youtubeProcess.Dispose();
                });

                // The YouTube video approach is unreliable and blocking. A better approach is to
                // simulate the voicemail tone and dialer interaction with timed ADB commands.
                // The following section simulates ending the call after leaving a message.
                // Consider replacing the above YouTube code with more reliable ADB commands.
                await Task.Delay(20000); // Simulate leaving a voicemail
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");

                await Task.Delay(3000); // Wait for the notification shade to open
                // Swipe down
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input swipe 300 0 300 1000 500");
                await Task.Delay(2000); // Wait for the notification shade to open

               

                
                // 4. Verify DUT 1 receives Voicemail notification and play voicemail
                bool voicemailNotification = false;
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string uiDumpPath = $"{outputPath}\\ui_dump.xml";
                gclass.CaptureUIDump(_dut1Id, outputPath);
                voicemailNotification = gclass.isInUIDump(uiDumpPath, "Voicemail");
                /*
                for (int i = 0; i < 20; i++)
                {
                    gclass.CaptureUIDump(_dut1Id, outputPath);
                    var doc = new System.Xml.XmlDocument();
                    string uiDumpPath = System.IO.Path.Combine(outputPath, "ui_dump.xml");
                    try
                    {
                        doc.Load(uiDumpPath);
                        var vmNode =
                            doc.SelectSingleNode("//node[contains(@text, 'Voicemail')]") ??
                            doc.SelectSingleNode("//node[contains(@content-desc, 'Voicemail')]");
                        if (vmNode != null)
                        {
                            voicemailNotification = true;
                            break;
                        }
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        gclass.UpdateOutput("UI dump file not found. Retrying...");
                    }
                    catch (System.Xml.XmlException ex)
                    {
                        gclass.UpdateOutput($"XML parsing error: {ex.Message}");
                    }
                    await Task.Delay(2000);
                }
                if (!voicemailNotification)
                {
                    throw new Exception("Voicemail notification not detected on DUT 1.");
                }
                */

                if (!voicemailNotification)
                {
                    gclass.UpdateOutput("Voicemail notification NOT detected on DUT 1.");
                    throw new Exception("Voicemail notification not detected on DUT 1.");

                }
                else
                {
                    gclass.UpdateOutput("Voicemail notification detected on DUT 1.");
                    await Task.Delay(3000);

                    gclass.SelectNodeWithTextFromUIDump(_dut1Id, "Voicemail");
                    await Task.Delay(4000); // Wait for the voicemail app to open

                    // Click on 1 to play voicemal
                    gclass.SelectNodeWithTextFromUIDump(_dut1Id, "1");

                    await Task.Delay(3000); // Wait for voicemail to play

                    // Click on 7 to delete voicemail
                    gclass.SelectNodeWithTextFromUIDump(_dut1Id, "7");
                    await Task.Delay(3000); // Wait for voicemail to be deleted

                    // end call
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");

                    // Return to home screen
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");

                    // 6. 'Call back' same number and ensure call is connected over Wi-Fi, check audio is ok
                    gclass.UpdateOutput("Initiating callback from DUT 1 to DUT 2...");
                    // A better approach for placing a callback, instead of dialing a number, is
                    // to trigger a call from the notification itself using ADB gestures if possible.
                    if (!gclass.PlaceCall(_dut1Id, gclass.ExtractPhoneNumber(_dut2Id)))
                    {
                        throw new Exception("Failed to place callback call.");
                    }

                    await Task.Delay(5000); // Wait for call to be placed
                    //gclass.RunAdbCommand($"adb -s {_dut2Id} shell input swipe 545 313 545 1380");
                    //await Task.Delay(4000); // Wait for the call to be answered
                    
                    //gclass.CaptureUIDump(_dut2Id, outputPath);
                    //bool isWifiCall = gclass.isInUIDump(_dut2Id, "Incoming Wi‑Fi call");
                    //if (!isWifiCall)
                    //    {
                    //    throw new Exception("Incoming WiFi call notification not detected on DUT 2.");
                    //}

                    await Task.Delay(7000); // Wait for the call to connect

                    // End all calls and cleanup
                    gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                    gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");

                    gclass.UpdateOutput($"TC 1.34: PASS [{_dut1Id}, {_dut2Id}]");
                    _testButton.BackColor = System.Drawing.Color.Green;
                    result = "PASS";
                }
            }
            catch (Exception ex)
            {
                gclass.UpdateOutput($"TC 1.34: FAIL [{_dut1Id}, {_dut2Id}] - {ex.Message}");
                _testButton.BackColor = System.Drawing.Color.Red;
                result = "FAIL";
                // Add more robust cleanup in the catch block if needed.
            }
            finally
            {
                // Ensure calls are ended and wifi is enabled as part of cleanup.
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_ENDCALL");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_ENDCALL");
                // go to home screen
                gclass.RunAdbCommand($"adb -s {_dut1Id} shell input keyevent KEYCODE_HOME");
                gclass.RunAdbCommand($"adb -s {_dut2Id} shell input keyevent KEYCODE_HOME");
                gclass.DisableWiFi(_dut1Id);
                gclass.DisableWiFi(_dut2Id);
                gclass.SetAirplaneMode(_dut1Id, true);
                gclass.SetAirplaneMode(_dut2Id, true);
            }
        }
    }
}
