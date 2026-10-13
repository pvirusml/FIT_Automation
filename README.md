This is used to Create automated testing applicaiton.
Where the User can connect n number of devices to the Software and then run the test cases.

The Application runs the test cases autmatically on the selected Device Under Test (DUT) and then dispays the results. Which can be later saved to Excel format.

Functional Documentation of the Test cases the SW can do:
1. Volte Registration Check
2. Send SMS between UE's and check if received correctly or not
3. XCAP/Call Forwarding
4. MOS score in Call (Work in progress)


The main C# application, the global device-control helper, test-case implementations, live-screen code, project dependencies, and unit-test code. I also compared these with our recent troubleshooting.

These are file counts, not a claim that all 111 tests are fully implemented or passing.
Existing project architecture
File / component	What I found
Form1.cs	Main Windows GUI, DUT/REF selection, batch execution, test dispatching, network monitoring and screen controls
GlobalVarClass.cs	ADB commands, telephony checks, SMS/MMS, call control, UI dump parsing, Wi-Fi, logging and SQL access
Test_Cases/	Individual procedures including voice calls, IMS, SMS, MMS, VoWiFi, forwarding and supplementary services
Registration_State.cs	Parsing telephony status and radio measurements
LiveScreenPopup.cs	Interactive remote-screen control, screenshot capture and network detail export
UnitTest1.cs	Automated tests primarily covering GUI operations, selection controls and parsing
FIT_Automation.csproj	Windows Forms application targeting .NET Framework 4.8
Your existing application already provides several things OpenHTF cannot provide without additional development: interactive phone screens, device-role management, telecom-specific workflows and your customized GUI.
Areas where the current code is strong
Multi-device test architecture: Your batch runner distinguishes DUT-only, DUT/REF and three-device test cases. It creates tasks for device groups, executes the selected test cases in sequence, and uses Task.WhenAll to wait for each batch of synchronous tasks.
Telecom-specific logic: The code already includes registration checks, UI-based dialer actions, SMS/MMS operations, call forwarding, VoWiFi and call waiting tests. TC 1.29, for example, manages a three-device call waiting, hold and swap procedure.
Useful operator interface: You have device discovery, test case selection, Excel/CSV test-plan import, network status tables, live-screen interaction, screenshots and visual pass/fail indicators.
