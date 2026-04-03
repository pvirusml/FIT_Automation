using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using FIT_Automation; // Ensure the correct namespace for MainForm is included

namespace FIT_AutomationUnitTests
{
    [TestClass]
    public class Form1Tests
    {
        private MainForm _form;

        [TestInitialize]
        public void Setup()
        {
            _form = new MainForm();

            // Initialize allCheckBoxes if not already initialized in MainForm
            var allCheckBoxesField = typeof(MainForm).GetField("allCheckBoxes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (allCheckBoxesField != null && allCheckBoxesField.GetValue(_form) == null)
            {
                allCheckBoxesField.SetValue(_form, new List<CheckBox>
                {
                    new CheckBox { Checked = false },
                    new CheckBox { Checked = false },
                    new CheckBox { Checked = false }
                });
            }
        }

        [TestMethod]
        public void PopulateDeviceList_InitializesDeviceList()
        {
            InvokePrivate("PopulateDeviceList");
            var allCheckBoxes = (List<CheckBox>)typeof(MainForm).GetField("allCheckBoxes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_form);
            Assert.IsNotNull(allCheckBoxes, "Device list should be initialized.");
            Assert.IsTrue(allCheckBoxes.Count > 0, "Device list should contain checkboxes.");
        }

        [TestMethod]
        public void SelectAllAvailableDevicesBTN_Click_SelectsAllDevices()
        {
            InvokePrivate("SelectAllAvailableDevicesBTN_Click", EventArgs.Empty);
            var allCheckBoxes = (List<CheckBox>)typeof(MainForm).GetField("allCheckBoxes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_form);
            Assert.IsTrue(allCheckBoxes.TrueForAll(cb => cb.Checked), "All checkboxes should be selected.");
        }

        [TestMethod]
        public void ClearAllTCsBTN_Click_ClearsAllSelections()
        {
            var allCheckBoxes = (List<CheckBox>)typeof(MainForm).GetField("allCheckBoxes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_form);
            foreach (var checkBox in allCheckBoxes)
            {
                checkBox.Checked = true;
            }
            InvokePrivate("ClearAllTCsBTN_Click", EventArgs.Empty);
            Assert.IsTrue(allCheckBoxes.TrueForAll(cb => !cb.Checked), "All checkboxes should be cleared.");
        }

        [TestMethod]
        public void NormalizeTcId_RemovesSpacesAndPrefix()
        {
            string result = InvokePrivate<string>("NormalizeTcId", "TC 1.12");
            Assert.AreEqual("TC1.12", result);
        }

        [TestMethod]
        public void FindHeaderIndex_ReturnsCorrectIndex()
        {
            var headers = new List<string> { "Test Case ID", "Other" };
            int index = InvokePrivate<int>("FindHeaderIndex", headers, "Test Case ID");
            Assert.AreEqual(0, index);
        }

        [TestMethod]
        public void SplitCsvLine_SplitsSimpleLine()
        {
            var result = InvokePrivate<List<string>>("SplitCsvLine", "a,b,c");
            CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, result);
        }

        [TestMethod]
        public void SplitCsvLine_SplitsQuotedComma()
        {
            var result = InvokePrivate<List<string>>("SplitCsvLine", "\"a,b\",c");
            CollectionAssert.AreEqual(new List<string> { "a,b", "c" }, result);
        }

        [TestMethod]
        public void ParseFromCsv_ParsesTestCaseIds()
        {
            string tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, "Test Case ID,Other\nTC 1.1,foo\nTC 1.2,bar");
            var result = InvokePrivate<List<string>>("ParseFromCsv", tempFile);
            System.IO.File.Delete(tempFile);
            CollectionAssert.AreEqual(new List<string> { "TC1.1", "TC1.2" }, result);
        }

        [TestMethod]
        public void AddToolTips_AddsToolTipsToControls()
        {
            InvokePrivate("addToolTips");
            // Assert that tooltips are added (requires exposing controls or using reflection)
        }

        [TestMethod]
        public void PreRequisitesButton_Click_ExecutesWithoutError()
        {
            InvokePrivate("PreRequisitesButton_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC1BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC1BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC2BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC2BTN_Click_1", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC3BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC3BTN_Click_1", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC11BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC11BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC12BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC12BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC14BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC14BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC15BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC15BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC16BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC16BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC17BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC17BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC18BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC18BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC110BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC110BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC111BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC111BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC112BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC112BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC113BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC113BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC114BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC114BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC115BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC115BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC116BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC116BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC117BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC117BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC118BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC118BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC119BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC119BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC120BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC120BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void AddMTBTN_Click_AddsNewDevice()
        {
            InvokePrivate("AddMTBTN_Click", _form, EventArgs.Empty);
            // Assert that a new device is added to the list (requires exposing the device list or using reflection)
        }

        [TestMethod]
        public void RemoveMTBTN_Click_RemovesDevice()
        {
            InvokePrivate("RemoveMTBTN_Click", _form, EventArgs.Empty);
            // Assert that a device is removed from the list (requires exposing the device list or using reflection)
        }

        [TestMethod]
        public void TC121BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC121BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC122BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC122BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC123BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC123BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC124BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC124BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC125BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC125BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC126BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC126BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC127BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC127BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC128BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC128BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC129BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC129BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC130BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC130BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC131BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC131BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC132BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC132BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC133BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC133BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC134BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC134BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC135BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC135BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC136BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC136BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC137BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC137BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC138BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC138BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC139BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC139BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC140BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC140BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC141BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC141BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC142BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC142BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC143BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC143BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC144BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC144BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC145BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC145BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC146BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC146BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC147BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC147BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC148BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC148BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC149BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC149BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        [TestMethod]
        public void TC150BTN_Click_ExecutesWithoutError()
        {
            InvokePrivate("TC150BTN_Click", _form, EventArgs.Empty);
            // Assert no exceptions are thrown
        }

        // Helper to invoke private methods
        private T InvokePrivate<T>(string methodName, params object[] args)
        {
            var method = typeof(MainForm).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(_form, args);
        }

        private void InvokePrivate(string methodName, params object[] args)
        {
            var method = typeof(MainForm).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_form, args);
        }
    }
}