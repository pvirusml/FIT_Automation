namespace FIT_Automation
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeviceContainer = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.DUTchkbx = new System.Windows.Forms.CheckedListBox();
            this.TCGRPBX = new System.Windows.Forms.GroupBox();
            this.TC13BTN = new System.Windows.Forms.Button();
            this.tcsmsLBL = new System.Windows.Forms.Label();
            this.TC3BTN = new System.Windows.Forms.Button();
            this.TC2BTN = new System.Windows.Forms.Button();
            this.TC1BTN = new System.Windows.Forms.Button();
            this.MTLBL = new System.Windows.Forms.Label();
            this.MOLBL = new System.Windows.Forms.Label();
            this.RemoveMTBTN = new System.Windows.Forms.Button();
            this.REFchekbx = new System.Windows.Forms.CheckedListBox();
            this.AddMTBTN = new System.Windows.Forms.Button();
            this.devicechkbxlst = new System.Windows.Forms.CheckedListBox();
            this.PopulateBTN = new System.Windows.Forms.Button();
            this.volteStatusgrid = new System.Windows.Forms.DataGridView();
            this.Deviceid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.voltests = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.networknm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.band = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rsrp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ImsStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.roam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.e911 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviceDataGridView = new System.Windows.Forms.DataGridView();
            this.Serial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Product_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VONR = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhoneNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Production_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SW_Version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Build_Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MasterRecordLBL = new System.Windows.Forms.Label();
            this.MtTTP = new System.Windows.Forms.ToolTip(this.components);
            this.MoTTP = new System.Windows.Forms.ToolTip(this.components);
            this.PopulateTTP = new System.Windows.Forms.ToolTip(this.components);
            this.UEinfoTTP = new System.Windows.Forms.ToolTip(this.components);
            this.outputRTB = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceContainer)).BeginInit();
            this.DeviceContainer.Panel1.SuspendLayout();
            this.DeviceContainer.Panel2.SuspendLayout();
            this.DeviceContainer.SuspendLayout();
            this.TCGRPBX.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.volteStatusgrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1142, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // DeviceContainer
            // 
            this.DeviceContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeviceContainer.Location = new System.Drawing.Point(0, 24);
            this.DeviceContainer.Name = "DeviceContainer";
            // 
            // DeviceContainer.Panel1
            // 
            this.DeviceContainer.Panel1.Controls.Add(this.label1);
            this.DeviceContainer.Panel1.Controls.Add(this.DUTchkbx);
            this.DeviceContainer.Panel1.Controls.Add(this.TCGRPBX);
            this.DeviceContainer.Panel1.Controls.Add(this.MTLBL);
            this.DeviceContainer.Panel1.Controls.Add(this.MOLBL);
            this.DeviceContainer.Panel1.Controls.Add(this.RemoveMTBTN);
            this.DeviceContainer.Panel1.Controls.Add(this.REFchekbx);
            this.DeviceContainer.Panel1.Controls.Add(this.AddMTBTN);
            this.DeviceContainer.Panel1.Controls.Add(this.devicechkbxlst);
            this.DeviceContainer.Panel1.Controls.Add(this.PopulateBTN);
            // 
            // DeviceContainer.Panel2
            // 
            this.DeviceContainer.Panel2.Controls.Add(this.outputRTB);
            this.DeviceContainer.Panel2.Controls.Add(this.volteStatusgrid);
            this.DeviceContainer.Panel2.Controls.Add(this.DeviceDataGridView);
            this.DeviceContainer.Panel2.Controls.Add(this.MasterRecordLBL);
            this.DeviceContainer.Size = new System.Drawing.Size(1142, 677);
            this.DeviceContainer.SplitterDistance = 380;
            this.DeviceContainer.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 229);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "DUT List";
            // 
            // DUTchkbx
            // 
            this.DUTchkbx.FormattingEnabled = true;
            this.DUTchkbx.Location = new System.Drawing.Point(30, 248);
            this.DUTchkbx.Name = "DUTchkbx";
            this.DUTchkbx.Size = new System.Drawing.Size(120, 94);
            this.DUTchkbx.TabIndex = 11;
            // 
            // TCGRPBX
            // 
            this.TCGRPBX.Controls.Add(this.TC13BTN);
            this.TCGRPBX.Controls.Add(this.tcsmsLBL);
            this.TCGRPBX.Controls.Add(this.TC3BTN);
            this.TCGRPBX.Controls.Add(this.TC2BTN);
            this.TCGRPBX.Controls.Add(this.TC1BTN);
            this.TCGRPBX.Location = new System.Drawing.Point(15, 367);
            this.TCGRPBX.Name = "TCGRPBX";
            this.TCGRPBX.Size = new System.Drawing.Size(292, 214);
            this.TCGRPBX.TabIndex = 10;
            this.TCGRPBX.TabStop = false;
            this.TCGRPBX.Text = "Test Cases";
            // 
            // TC13BTN
            // 
            this.TC13BTN.Location = new System.Drawing.Point(87, 18);
            this.TC13BTN.Name = "TC13BTN";
            this.TC13BTN.Size = new System.Drawing.Size(75, 23);
            this.TC13BTN.TabIndex = 11;
            this.TC13BTN.Text = "TC 1.3";
            this.TC13BTN.UseVisualStyleBackColor = true;
            this.TC13BTN.Click += new System.EventHandler(this.button1_Click);
            // 
            // tcsmsLBL
            // 
            this.tcsmsLBL.AutoSize = true;
            this.tcsmsLBL.Location = new System.Drawing.Point(110, 52);
            this.tcsmsLBL.Name = "tcsmsLBL";
            this.tcsmsLBL.Size = new System.Drawing.Size(0, 13);
            this.tcsmsLBL.TabIndex = 10;
            this.tcsmsLBL.Visible = false;
            // 
            // TC3BTN
            // 
            this.TC3BTN.Location = new System.Drawing.Point(6, 75);
            this.TC3BTN.Name = "TC3BTN";
            this.TC3BTN.Size = new System.Drawing.Size(75, 23);
            this.TC3BTN.TabIndex = 9;
            this.TC3BTN.Text = "XCAP TC";
            this.TC3BTN.UseVisualStyleBackColor = true;
            this.TC3BTN.Click += new System.EventHandler(this.TC3BTN_Click_1);
            // 
            // TC2BTN
            // 
            this.TC2BTN.Location = new System.Drawing.Point(6, 47);
            this.TC2BTN.Name = "TC2BTN";
            this.TC2BTN.Size = new System.Drawing.Size(75, 23);
            this.TC2BTN.TabIndex = 8;
            this.TC2BTN.Text = "SMS TC";
            this.TC2BTN.UseVisualStyleBackColor = true;
            this.TC2BTN.Click += new System.EventHandler(this.TC2BTN_Click_1);
            // 
            // TC1BTN
            // 
            this.TC1BTN.Location = new System.Drawing.Point(6, 18);
            this.TC1BTN.Name = "TC1BTN";
            this.TC1BTN.Size = new System.Drawing.Size(75, 23);
            this.TC1BTN.TabIndex = 7;
            this.TC1BTN.Text = "VOLTE TC";
            this.TC1BTN.UseVisualStyleBackColor = true;
            this.TC1BTN.Click += new System.EventHandler(this.TC1BTN_Click);
            // 
            // MTLBL
            // 
            this.MTLBL.AutoSize = true;
            this.MTLBL.Location = new System.Drawing.Point(184, 229);
            this.MTLBL.Name = "MTLBL";
            this.MTLBL.Size = new System.Drawing.Size(47, 13);
            this.MTLBL.TabIndex = 8;
            this.MTLBL.Text = "REF List";
            // 
            // MOLBL
            // 
            this.MOLBL.AutoSize = true;
            this.MOLBL.Location = new System.Drawing.Point(12, 77);
            this.MOLBL.Name = "MOLBL";
            this.MOLBL.Size = new System.Drawing.Size(61, 13);
            this.MOLBL.TabIndex = 7;
            this.MOLBL.Text = "MO Device";
            // 
            // RemoveMTBTN
            // 
            this.RemoveMTBTN.Location = new System.Drawing.Point(30, 201);
            this.RemoveMTBTN.Name = "RemoveMTBTN";
            this.RemoveMTBTN.Size = new System.Drawing.Size(89, 25);
            this.RemoveMTBTN.TabIndex = 6;
            this.RemoveMTBTN.Text = "Select as DUT";
            this.MoTTP.SetToolTip(this.RemoveMTBTN, "Click to Move Item to DUT");
            this.RemoveMTBTN.UseVisualStyleBackColor = true;
            this.RemoveMTBTN.Click += new System.EventHandler(this.RemoveMTBTN_Click);
            // 
            // REFchekbx
            // 
            this.REFchekbx.FormattingEnabled = true;
            this.REFchekbx.Location = new System.Drawing.Point(187, 248);
            this.REFchekbx.Name = "REFchekbx";
            this.REFchekbx.Size = new System.Drawing.Size(120, 94);
            this.REFchekbx.TabIndex = 5;
            // 
            // AddMTBTN
            // 
            this.AddMTBTN.Location = new System.Drawing.Point(187, 201);
            this.AddMTBTN.Name = "AddMTBTN";
            this.AddMTBTN.Size = new System.Drawing.Size(84, 23);
            this.AddMTBTN.TabIndex = 4;
            this.AddMTBTN.Text = "Select as REF";
            this.MtTTP.SetToolTip(this.AddMTBTN, "Click to Move Item to REF");
            this.AddMTBTN.UseVisualStyleBackColor = true;
            this.AddMTBTN.Click += new System.EventHandler(this.AddMTBTN_Click);
            // 
            // devicechkbxlst
            // 
            this.devicechkbxlst.FormattingEnabled = true;
            this.devicechkbxlst.Location = new System.Drawing.Point(13, 96);
            this.devicechkbxlst.Name = "devicechkbxlst";
            this.devicechkbxlst.Size = new System.Drawing.Size(120, 94);
            this.devicechkbxlst.TabIndex = 3;
            // 
            // PopulateBTN
            // 
            this.PopulateBTN.Location = new System.Drawing.Point(13, 20);
            this.PopulateBTN.Name = "PopulateBTN";
            this.PopulateBTN.Size = new System.Drawing.Size(106, 23);
            this.PopulateBTN.TabIndex = 0;
            this.PopulateBTN.Text = "Populate Devices";
            this.PopulateTTP.SetToolTip(this.PopulateBTN, "Click to Populate All ADB Devices");
            this.PopulateBTN.UseVisualStyleBackColor = true;
            this.PopulateBTN.Click += new System.EventHandler(this.PopulateBTN_Click);
            // 
            // volteStatusgrid
            // 
            this.volteStatusgrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.volteStatusgrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Deviceid,
            this.voltests,
            this.networknm,
            this.band,
            this.rsrp,
            this.ImsStatus,
            this.data,
            this.roam,
            this.e911});
            this.volteStatusgrid.Location = new System.Drawing.Point(3, 248);
            this.volteStatusgrid.Name = "volteStatusgrid";
            this.volteStatusgrid.Size = new System.Drawing.Size(742, 150);
            this.volteStatusgrid.TabIndex = 10;
            // 
            // Deviceid
            // 
            this.Deviceid.HeaderText = "Device ID";
            this.Deviceid.Name = "Deviceid";
            // 
            // voltests
            // 
            this.voltests.HeaderText = "VOLTE Status";
            this.voltests.Name = "voltests";
            // 
            // networknm
            // 
            this.networknm.HeaderText = "Network Name";
            this.networknm.Name = "networknm";
            // 
            // band
            // 
            this.band.HeaderText = "Band #";
            this.band.Name = "band";
            // 
            // rsrp
            // 
            this.rsrp.HeaderText = "RSRP";
            this.rsrp.Name = "rsrp";
            // 
            // ImsStatus
            // 
            this.ImsStatus.HeaderText = "IMS State";
            this.ImsStatus.Name = "ImsStatus";
            // 
            // data
            // 
            this.data.HeaderText = "Data State";
            this.data.Name = "data";
            // 
            // roam
            // 
            this.roam.HeaderText = "Roaming State";
            this.roam.Name = "roam";
            // 
            // e911
            // 
            this.e911.HeaderText = "Emergency State";
            this.e911.Name = "e911";
            // 
            // DeviceDataGridView
            // 
            this.DeviceDataGridView.AllowUserToDeleteRows = false;
            this.DeviceDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DeviceDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.DeviceDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.DeviceDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.RaisedVertical;
            this.DeviceDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Serial,
            this.Product_Name,
            this.VONR,
            this.PhoneNumber,
            this.Production_Name,
            this.SW_Version,
            this.Build_Type});
            this.DeviceDataGridView.EnableHeadersVisualStyles = false;
            this.DeviceDataGridView.Location = new System.Drawing.Point(3, 40);
            this.DeviceDataGridView.MultiSelect = false;
            this.DeviceDataGridView.Name = "DeviceDataGridView";
            this.DeviceDataGridView.ReadOnly = true;
            this.DeviceDataGridView.Size = new System.Drawing.Size(742, 150);
            this.DeviceDataGridView.TabIndex = 7;
            this.UEinfoTTP.SetToolTip(this.DeviceDataGridView, "This table shows all the information of connected UE\'s");
            // 
            // Serial
            // 
            this.Serial.HeaderText = "Serial";
            this.Serial.Name = "Serial";
            this.Serial.ReadOnly = true;
            // 
            // Product_Name
            // 
            this.Product_Name.HeaderText = "Product_Name";
            this.Product_Name.Name = "Product_Name";
            this.Product_Name.ReadOnly = true;
            // 
            // VONR
            // 
            this.VONR.HeaderText = "VONR";
            this.VONR.Name = "VONR";
            this.VONR.ReadOnly = true;
            // 
            // PhoneNumber
            // 
            this.PhoneNumber.HeaderText = "Phone Number";
            this.PhoneNumber.Name = "PhoneNumber";
            this.PhoneNumber.ReadOnly = true;
            // 
            // Production_Name
            // 
            this.Production_Name.HeaderText = "Production_Name";
            this.Production_Name.Name = "Production_Name";
            this.Production_Name.ReadOnly = true;
            // 
            // SW_Version
            // 
            this.SW_Version.HeaderText = "SW Version";
            this.SW_Version.Name = "SW_Version";
            this.SW_Version.ReadOnly = true;
            // 
            // Build_Type
            // 
            this.Build_Type.HeaderText = "Chipset Type";
            this.Build_Type.Name = "Build_Type";
            this.Build_Type.ReadOnly = true;
            // 
            // MasterRecordLBL
            // 
            this.MasterRecordLBL.AutoSize = true;
            this.MasterRecordLBL.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MasterRecordLBL.Location = new System.Drawing.Point(3, 20);
            this.MasterRecordLBL.Name = "MasterRecordLBL";
            this.MasterRecordLBL.Size = new System.Drawing.Size(112, 15);
            this.MasterRecordLBL.TabIndex = 9;
            this.MasterRecordLBL.Text = "Master Record Table:";
            // 
            // outputRTB
            // 
            this.outputRTB.Location = new System.Drawing.Point(3, 416);
            this.outputRTB.Name = "outputRTB";
            this.outputRTB.Size = new System.Drawing.Size(743, 238);
            this.outputRTB.TabIndex = 11;
            this.outputRTB.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1142, 701);
            this.Controls.Add(this.DeviceContainer);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "FIT Automation";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.DeviceContainer.Panel1.ResumeLayout(false);
            this.DeviceContainer.Panel1.PerformLayout();
            this.DeviceContainer.Panel2.ResumeLayout(false);
            this.DeviceContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceContainer)).EndInit();
            this.DeviceContainer.ResumeLayout(false);
            this.TCGRPBX.ResumeLayout(false);
            this.TCGRPBX.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.volteStatusgrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.SplitContainer DeviceContainer;
        private System.Windows.Forms.Button PopulateBTN;
        private System.Windows.Forms.CheckedListBox devicechkbxlst;
        private System.Windows.Forms.Button AddMTBTN;
        private System.Windows.Forms.CheckedListBox REFchekbx;
        private System.Windows.Forms.Button RemoveMTBTN;
        private System.Windows.Forms.Label MOLBL;
        private System.Windows.Forms.Label MTLBL;
        private System.Windows.Forms.Label MasterRecordLBL;
        private System.Windows.Forms.ToolTip MoTTP;
        private System.Windows.Forms.ToolTip MtTTP;
        private System.Windows.Forms.ToolTip PopulateTTP;
        private System.Windows.Forms.ToolTip UEinfoTTP;
        private System.Windows.Forms.DataGridView DeviceDataGridView;
        private System.Windows.Forms.GroupBox TCGRPBX;
        private System.Windows.Forms.Button TC3BTN;
        private System.Windows.Forms.Button TC2BTN;
        private System.Windows.Forms.Button TC1BTN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox DUTchkbx;
        private System.Windows.Forms.DataGridViewTextBoxColumn Serial;
        private System.Windows.Forms.DataGridViewTextBoxColumn Product_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn VONR;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhoneNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn Production_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn SW_Version;
        private System.Windows.Forms.DataGridViewTextBoxColumn Build_Type;
        private System.Windows.Forms.Label tcsmsLBL;
        private System.Windows.Forms.DataGridView volteStatusgrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Deviceid;
        private System.Windows.Forms.DataGridViewTextBoxColumn voltests;
        private System.Windows.Forms.DataGridViewTextBoxColumn networknm;
        private System.Windows.Forms.DataGridViewTextBoxColumn band;
        private System.Windows.Forms.DataGridViewTextBoxColumn rsrp;
        private System.Windows.Forms.DataGridViewTextBoxColumn ImsStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn data;
        private System.Windows.Forms.DataGridViewTextBoxColumn roam;
        private System.Windows.Forms.DataGridViewTextBoxColumn e911;
        private System.Windows.Forms.Button TC13BTN;
        private System.Windows.Forms.RichTextBox outputRTB;
    }
}

