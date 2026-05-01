namespace FIT_Automation
{
    partial class LiveScreenPopup
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
            this.CaptureButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CaptureButton
            // 
            this.CaptureButton.BackColor = System.Drawing.SystemColors.Highlight;
            this.CaptureButton.Font = new System.Drawing.Font("Ubuntu Mono", 7.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CaptureButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.CaptureButton.Location = new System.Drawing.Point(9, 9);
            this.CaptureButton.Margin = new System.Windows.Forms.Padding(0);
            this.CaptureButton.Name = "CaptureButton";
            this.CaptureButton.Size = new System.Drawing.Size(133, 51);
            this.CaptureButton.TabIndex = 0;
            this.CaptureButton.TabStop = false;
            this.CaptureButton.Text = "Screenshot";
            this.CaptureButton.UseMnemonic = false;
            this.CaptureButton.UseVisualStyleBackColor = false;
            this.CaptureButton.Click += new System.EventHandler(this.CaptureButton_Click);
            // 
            // LiveScreenPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.CaptureButton);
            this.Name = "LiveScreenPopup";
            this.Text = "LiveScreenPopup";
            this.Load += new System.EventHandler(this.LiveScreenPopup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button CaptureButton;
    }
}