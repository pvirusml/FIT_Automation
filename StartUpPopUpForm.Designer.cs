using System.Linq;

namespace FIT_Automation
{
    partial class StartUpPopUpForm
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
            this.PopUpHeadline = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.OpenAppButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PopUpHeadline
            // 
            this.PopUpHeadline.AutoSize = true;
            this.PopUpHeadline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PopUpHeadline.Font = new System.Drawing.Font("Myanmar Text", 9F, System.Drawing.FontStyle.Bold);
            this.PopUpHeadline.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.PopUpHeadline.Location = new System.Drawing.Point(12, 9);
            this.PopUpHeadline.Name = "PopUpHeadline";
            this.PopUpHeadline.Size = new System.Drawing.Size(682, 34);
            this.PopUpHeadline.TabIndex = 0;
            this.PopUpHeadline.Text = "These are the pre-requisites that need to be met before any testing can begin!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Font = new System.Drawing.Font("Myanmar Text", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label1.Location = new System.Drawing.Point(49, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(291, 194);
            this.label1.TabIndex = 1;
            this.label1.Text = "1. Keep phone on LTE Only\r\n2. Turn WiFi Off\r\n3. Turn off RCS \r\n4. Keep screen ope" +
    "n\r\n5. Keep Laptop Volume up\r\n6. Keep Phone(s) close to laptop\r\n7. Manually enable VoWiFi Caliing before beginning" +
    "\r\n8. Make sure your devices are already registered to the Wi-Fi";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // OpenAppButton
            // 
            this.OpenAppButton.Location = new System.Drawing.Point(583, 369);
            this.OpenAppButton.Name = "OpenAppButton";
            this.OpenAppButton.Size = new System.Drawing.Size(111, 54);
            this.OpenAppButton.TabIndex = 2;
            this.OpenAppButton.Text = "Open App";
            this.OpenAppButton.UseVisualStyleBackColor = true;
            this.OpenAppButton.Click += new System.EventHandler(this.OpenAppButton_Click);
            // 
            // StartUpPopUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.OpenAppButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PopUpHeadline);
            this.Name = "StartUpPopUpForm";
            this.Text = "StartUpPopUpForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PopUpHeadline;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OpenAppButton;
    }
}