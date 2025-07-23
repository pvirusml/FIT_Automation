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
            this.TC14BTN = new System.Windows.Forms.Button();
            this.TC12BTN = new System.Windows.Forms.Button();
            this.TC11BTN = new System.Windows.Forms.Button();
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
            this.outputRTB = new System.Windows.Forms.RichTextBox();
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
            this.TC15BTN = new System.Windows.Forms.Button();
            this.TC16BTN = new System.Windows.Forms.Button();
            this.TC17BTN = new System.Windows.Forms.Button();
            this.TC18BTN = new System.Windows.Forms.Button();
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
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1713, 33);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // DeviceContainer
            // 
            this.DeviceContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeviceContainer.Location = new System.Drawing.Point(0, 33);
            this.DeviceContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.DeviceContainer.Size = new System.Drawing.Size(1713, 1017);
            this.DeviceContainer.SplitterDistance = 570;
            this.DeviceContainer.SplitterWidth = 6;
            this.DeviceContainer.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 352);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "DUT List";
            // 
            // DUTchkbx
            // 
            this.DUTchkbx.FormattingEnabled = true;
            this.DUTchkbx.Location = new System.Drawing.Point(45, 382);
            this.DUTchkbx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DUTchkbx.Name = "DUTchkbx";
            this.DUTchkbx.Size = new System.Drawing.Size(178, 142);
            this.DUTchkbx.TabIndex = 11;
            // 
            // TCGRPBX
            // 
            this.TCGRPBX.Controls.Add(this.TC18BTN);
            this.TCGRPBX.Controls.Add(this.TC17BTN);
            this.TCGRPBX.Controls.Add(this.TC16BTN);
            this.TCGRPBX.Controls.Add(this.TC15BTN);
            this.TCGRPBX.Controls.Add(this.TC14BTN);
            this.TCGRPBX.Controls.Add(this.TC12BTN);
            this.TCGRPBX.Controls.Add(this.TC11BTN);
            this.TCGRPBX.Controls.Add(this.TC13BTN);
            this.TCGRPBX.Controls.Add(this.tcsmsLBL);
            this.TCGRPBX.Controls.Add(this.TC3BTN);
            this.TCGRPBX.Controls.Add(this.TC2BTN);
            this.TCGRPBX.Controls.Add(this.TC1BTN);
            this.TCGRPBX.Location = new System.Drawing.Point(22, 565);
            this.TCGRPBX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TCGRPBX.Name = "TCGRPBX";
            this.TCGRPBX.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TCGRPBX.Size = new System.Drawing.Size(438, 329);
            this.TCGRPBX.TabIndex = 10;
            this.TCGRPBX.TabStop = false;
            this.TCGRPBX.Text = "Test Cases";
            // 
            // TC14BTN
            // 
            this.TC14BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC14BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC14BTN.Location = new System.Drawing.Point(156, 72);
            this.TC14BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC14BTN.Name = "TC14BTN";
            this.TC14BTN.Size = new System.Drawing.Size(78, 35);
            this.TC14BTN.TabIndex = 14;
            this.TC14BTN.Text = "TC 1.4";
            this.TC14BTN.UseVisualStyleBackColor = false;
            this.TC14BTN.Click += new System.EventHandler(this.TC14BTN_Click);
            // 
            // TC12BTN
            // 
            this.TC12BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC12BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC12BTN.Location = new System.Drawing.Point(247, 28);
            this.TC12BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC12BTN.Name = "TC12BTN";
            this.TC12BTN.Size = new System.Drawing.Size(78, 35);
            this.TC12BTN.TabIndex = 13;
            this.TC12BTN.Text = "TC 1.2";
            this.TC12BTN.UseVisualStyleBackColor = false;
            this.TC12BTN.Click += new System.EventHandler(this.TC12BTN_Click);
            // 
            // TC11BTN
            // 
            this.TC11BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC11BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC11BTN.Location = new System.Drawing.Point(156, 28);
            this.TC11BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC11BTN.Name = "TC11BTN";
            this.TC11BTN.Size = new System.Drawing.Size(78, 35);
            this.TC11BTN.TabIndex = 12;
            this.TC11BTN.Text = "TC 1.1";
            this.TC11BTN.UseVisualStyleBackColor = false;
            this.TC11BTN.Click += new System.EventHandler(this.TC11BTN_Click);
            // 
            // TC13BTN
            // 
            this.TC13BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC13BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC13BTN.Location = new System.Drawing.Point(337, 28);
            this.TC13BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC13BTN.Name = "TC13BTN";
            this.TC13BTN.Size = new System.Drawing.Size(78, 35);
            this.TC13BTN.TabIndex = 11;
            this.TC13BTN.Text = "TC 1.3";
            this.TC13BTN.UseVisualStyleBackColor = false;
            this.TC13BTN.Click += new System.EventHandler(this.button1_Click);
            // 
            // tcsmsLBL
            // 
            this.tcsmsLBL.AutoSize = true;
            this.tcsmsLBL.Location = new System.Drawing.Point(165, 80);
            this.tcsmsLBL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tcsmsLBL.Name = "tcsmsLBL";
            this.tcsmsLBL.Size = new System.Drawing.Size(0, 20);
            this.tcsmsLBL.TabIndex = 10;
            this.tcsmsLBL.Visible = false;
            // 
            // TC3BTN
            // 
            this.TC3BTN.Location = new System.Drawing.Point(9, 115);
            this.TC3BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC3BTN.Name = "TC3BTN";
            this.TC3BTN.Size = new System.Drawing.Size(112, 35);
            this.TC3BTN.TabIndex = 9;
            this.TC3BTN.Text = "XCAP TC";
            this.TC3BTN.UseVisualStyleBackColor = true;
            this.TC3BTN.Click += new System.EventHandler(this.TC3BTN_Click_1);
            // 
            // TC2BTN
            // 
            this.TC2BTN.Location = new System.Drawing.Point(9, 72);
            this.TC2BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC2BTN.Name = "TC2BTN";
            this.TC2BTN.Size = new System.Drawing.Size(112, 35);
            this.TC2BTN.TabIndex = 8;
            this.TC2BTN.Text = "SMS TC";
            this.TC2BTN.UseVisualStyleBackColor = true;
            this.TC2BTN.Click += new System.EventHandler(this.TC2BTN_Click_1);
            // 
            // TC1BTN
            // 
            this.TC1BTN.Location = new System.Drawing.Point(9, 28);
            this.TC1BTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TC1BTN.Name = "TC1BTN";
            this.TC1BTN.Size = new System.Drawing.Size(112, 35);
            this.TC1BTN.TabIndex = 7;
            this.TC1BTN.Text = "VOLTE TC";
            this.TC1BTN.UseVisualStyleBackColor = true;
            this.TC1BTN.Click += new System.EventHandler(this.TC1BTN_Click);
            // 
            // MTLBL
            // 
            this.MTLBL.AutoSize = true;
            this.MTLBL.Location = new System.Drawing.Point(276, 352);
            this.MTLBL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MTLBL.Name = "MTLBL";
            this.MTLBL.Size = new System.Drawing.Size(71, 20);
            this.MTLBL.TabIndex = 8;
            this.MTLBL.Text = "REF List";
            // 
            // MOLBL
            // 
            this.MOLBL.AutoSize = true;
            this.MOLBL.Location = new System.Drawing.Point(18, 118);
            this.MOLBL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MOLBL.Name = "MOLBL";
            this.MOLBL.Size = new System.Drawing.Size(86, 20);
            this.MOLBL.TabIndex = 7;
            this.MOLBL.Text = "MO Device";
            // 
            // RemoveMTBTN
            // 
            this.RemoveMTBTN.Location = new System.Drawing.Point(45, 309);
            this.RemoveMTBTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RemoveMTBTN.Name = "RemoveMTBTN";
            this.RemoveMTBTN.Size = new System.Drawing.Size(134, 38);
            this.RemoveMTBTN.TabIndex = 6;
            this.RemoveMTBTN.Text = "Select as DUT";
            this.MoTTP.SetToolTip(this.RemoveMTBTN, "Click to Move Item to DUT");
            this.RemoveMTBTN.UseVisualStyleBackColor = true;
            this.RemoveMTBTN.Click += new System.EventHandler(this.RemoveMTBTN_Click);
            // 
            // REFchekbx
            // 
            this.REFchekbx.FormattingEnabled = true;
            this.REFchekbx.Location = new System.Drawing.Point(280, 382);
            this.REFchekbx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.REFchekbx.Name = "REFchekbx";
            this.REFchekbx.Size = new System.Drawing.Size(178, 142);
            this.REFchekbx.TabIndex = 5;
            // 
            // AddMTBTN
            // 
            this.AddMTBTN.Location = new System.Drawing.Point(280, 309);
            this.AddMTBTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.AddMTBTN.Name = "AddMTBTN";
            this.AddMTBTN.Size = new System.Drawing.Size(126, 35);
            this.AddMTBTN.TabIndex = 4;
            this.AddMTBTN.Text = "Select as REF";
            this.MtTTP.SetToolTip(this.AddMTBTN, "Click to Move Item to REF");
            this.AddMTBTN.UseVisualStyleBackColor = true;
            this.AddMTBTN.Click += new System.EventHandler(this.AddMTBTN_Click);
            // 
            // devicechkbxlst
            // 
            this.devicechkbxlst.FormattingEnabled = true;
            this.devicechkbxlst.Location = new System.Drawing.Point(20, 148);
            this.devicechkbxlst.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.devicechkbxlst.Name = "devicechkbxlst";
            this.devicechkbxlst.Size = new System.Drawing.Size(178, 142);
            this.devicechkbxlst.TabIndex = 3;
            // 
            // PopulateBTN
            // 
            this.PopulateBTN.Location = new System.Drawing.Point(20, 31);
            this.PopulateBTN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PopulateBTN.Name = "PopulateBTN";
            this.PopulateBTN.Size = new System.Drawing.Size(159, 35);
            this.PopulateBTN.TabIndex = 0;
            this.PopulateBTN.Text = "Populate Devices";
            this.PopulateTTP.SetToolTip(this.PopulateBTN, "Click to Populate All ADB Devices");
            this.PopulateBTN.UseVisualStyleBackColor = true;
            this.PopulateBTN.Click += new System.EventHandler(this.PopulateBTN_Click);
            // 
            // outputRTB
            // 
            this.outputRTB.Location = new System.Drawing.Point(3, 593);
            this.outputRTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.outputRTB.Name = "outputRTB";
            this.outputRTB.Size = new System.Drawing.Size(1112, 322);
            this.outputRTB.TabIndex = 11;
            this.outputRTB.Text = "";
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
            this.volteStatusgrid.Location = new System.Drawing.Point(3, 332);
            this.volteStatusgrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.volteStatusgrid.Name = "volteStatusgrid";
            this.volteStatusgrid.RowHeadersWidth = 62;
            this.volteStatusgrid.Size = new System.Drawing.Size(1113, 231);
            this.volteStatusgrid.TabIndex = 10;
            // 
            // Deviceid
            // 
            this.Deviceid.HeaderText = "Device ID";
            this.Deviceid.MinimumWidth = 8;
            this.Deviceid.Name = "Deviceid";
            this.Deviceid.Width = 150;
            // 
            // voltests
            // 
            this.voltests.HeaderText = "VOLTE Status";
            this.voltests.MinimumWidth = 8;
            this.voltests.Name = "voltests";
            this.voltests.Width = 150;
            // 
            // networknm
            // 
            this.networknm.HeaderText = "Network Name";
            this.networknm.MinimumWidth = 8;
            this.networknm.Name = "networknm";
            this.networknm.Width = 150;
            // 
            // band
            // 
            this.band.HeaderText = "Band #";
            this.band.MinimumWidth = 8;
            this.band.Name = "band";
            this.band.Width = 150;
            // 
            // rsrp
            // 
            this.rsrp.HeaderText = "RSRP";
            this.rsrp.MinimumWidth = 8;
            this.rsrp.Name = "rsrp";
            this.rsrp.Width = 150;
            // 
            // ImsStatus
            // 
            this.ImsStatus.HeaderText = "IMS State";
            this.ImsStatus.MinimumWidth = 8;
            this.ImsStatus.Name = "ImsStatus";
            this.ImsStatus.Width = 150;
            // 
            // data
            // 
            this.data.HeaderText = "Data State";
            this.data.MinimumWidth = 8;
            this.data.Name = "data";
            this.data.Width = 150;
            // 
            // roam
            // 
            this.roam.HeaderText = "Roaming State";
            this.roam.MinimumWidth = 8;
            this.roam.Name = "roam";
            this.roam.Width = 150;
            // 
            // e911
            // 
            this.e911.HeaderText = "Emergency State";
            this.e911.MinimumWidth = 8;
            this.e911.Name = "e911";
            this.e911.Width = 150;
            // 
            // DeviceDataGridView
            // 
            this.DeviceDataGridView.AllowUserToDeleteRows = false;
            this.DeviceDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DeviceDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.DeviceDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.DeviceDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.RaisedVertical;
            this.DeviceDataGridView.ColumnHeadersHeight = 34;
            this.DeviceDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Serial,
            this.Product_Name,
            this.VONR,
            this.PhoneNumber,
            this.Production_Name,
            this.SW_Version,
            this.Build_Type});
            this.DeviceDataGridView.EnableHeadersVisualStyles = false;
            this.DeviceDataGridView.Location = new System.Drawing.Point(4, 62);
            this.DeviceDataGridView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DeviceDataGridView.MultiSelect = false;
            this.DeviceDataGridView.Name = "DeviceDataGridView";
            this.DeviceDataGridView.ReadOnly = true;
            this.DeviceDataGridView.RowHeadersWidth = 62;
            this.DeviceDataGridView.Size = new System.Drawing.Size(1113, 231);
            this.DeviceDataGridView.TabIndex = 7;
            this.UEinfoTTP.SetToolTip(this.DeviceDataGridView, "This table shows all the information of connected UE\'s");
            this.DeviceDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DeviceDataGridView_CellContentClick);
            // 
            // Serial
            // 
            this.Serial.HeaderText = "Serial";
            this.Serial.MinimumWidth = 8;
            this.Serial.Name = "Serial";
            this.Serial.ReadOnly = true;
            // 
            // Product_Name
            // 
            this.Product_Name.HeaderText = "Product_Name";
            this.Product_Name.MinimumWidth = 8;
            this.Product_Name.Name = "Product_Name";
            this.Product_Name.ReadOnly = true;
            // 
            // VONR
            // 
            this.VONR.HeaderText = "VONR";
            this.VONR.MinimumWidth = 8;
            this.VONR.Name = "VONR";
            this.VONR.ReadOnly = true;
            // 
            // PhoneNumber
            // 
            this.PhoneNumber.HeaderText = "Phone Number";
            this.PhoneNumber.MinimumWidth = 8;
            this.PhoneNumber.Name = "PhoneNumber";
            this.PhoneNumber.ReadOnly = true;
            // 
            // Production_Name
            // 
            this.Production_Name.HeaderText = "Production_Name";
            this.Production_Name.MinimumWidth = 8;
            this.Production_Name.Name = "Production_Name";
            this.Production_Name.ReadOnly = true;
            // 
            // SW_Version
            // 
            this.SW_Version.HeaderText = "SW Version";
            this.SW_Version.MinimumWidth = 8;
            this.SW_Version.Name = "SW_Version";
            this.SW_Version.ReadOnly = true;
            // 
            // Build_Type
            // 
            this.Build_Type.HeaderText = "Chipset Type";
            this.Build_Type.MinimumWidth = 8;
            this.Build_Type.Name = "Build_Type";
            this.Build_Type.ReadOnly = true;
            // 
            // MasterRecordLBL
            // 
            this.MasterRecordLBL.AutoSize = true;
            this.MasterRecordLBL.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MasterRecordLBL.Location = new System.Drawing.Point(4, 31);
            this.MasterRecordLBL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MasterRecordLBL.Name = "MasterRecordLBL";
            this.MasterRecordLBL.Size = new System.Drawing.Size(163, 22);
            this.MasterRecordLBL.TabIndex = 9;
            this.MasterRecordLBL.Text = "Master Record Table:";
            // 
            // TC15BTN
            // 
            this.TC15BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC15BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC15BTN.Location = new System.Drawing.Point(247, 72);
            this.TC15BTN.Name = "TC15BTN";
            this.TC15BTN.Size = new System.Drawing.Size(78, 35);
            this.TC15BTN.TabIndex = 15;
            this.TC15BTN.Text = "TC 1.5";
            this.TC15BTN.UseVisualStyleBackColor = false;
            this.TC15BTN.Click += new System.EventHandler(this.TC15BTN_Click);
            // 
            // TC16BTN
            // 
            this.TC16BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC16BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC16BTN.Location = new System.Drawing.Point(337, 72);
            this.TC16BTN.Name = "TC16BTN";
            this.TC16BTN.Size = new System.Drawing.Size(78, 35);
            this.TC16BTN.TabIndex = 16;
            this.TC16BTN.Text = "TC 1.6";
            this.TC16BTN.UseVisualStyleBackColor = false;
            this.TC16BTN.Click += new System.EventHandler(this.TC16BTN_Click);
            // 
            // TC17BTN
            // 
            this.TC17BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC17BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC17BTN.Location = new System.Drawing.Point(156, 115);
            this.TC17BTN.Name = "TC17BTN";
            this.TC17BTN.Size = new System.Drawing.Size(78, 35);
            this.TC17BTN.TabIndex = 17;
            this.TC17BTN.Text = "TC 1.7";
            this.TC17BTN.UseVisualStyleBackColor = false;
            this.TC17BTN.Click += new System.EventHandler(this.TC17BTN_Click);
            // 
            // TC18BTN
            // 
            this.TC18BTN.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TC18BTN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TC18BTN.Location = new System.Drawing.Point(247, 115);
            this.TC18BTN.Name = "TC18BTN";
            this.TC18BTN.Size = new System.Drawing.Size(78, 35);
            this.TC18BTN.TabIndex = 18;
            this.TC18BTN.Text = "TC 1.8";
            this.TC18BTN.UseVisualStyleBackColor = false;
            this.TC18BTN.Click += new System.EventHandler(this.TC18BTN_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1713, 1050);
            this.Controls.Add(this.DeviceContainer);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
        private System.Windows.Forms.Button TC11BTN;
        private System.Windows.Forms.Button TC12BTN;
        private System.Windows.Forms.Button TC14BTN;
        private System.Windows.Forms.Button TC15BTN;
        private System.Windows.Forms.Button TC16BTN;
        private System.Windows.Forms.Button TC17BTN;
        private System.Windows.Forms.Button TC18BTN;
    }
}

