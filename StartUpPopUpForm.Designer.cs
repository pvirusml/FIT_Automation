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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartUpPopUpForm));
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
            this.PopUpHeadline.Location = new System.Drawing.Point(11, 7);
            this.PopUpHeadline.Name = "PopUpHeadline";
            this.PopUpHeadline.Size = new System.Drawing.Size(560, 29);
            this.PopUpHeadline.TabIndex = 0;
            this.PopUpHeadline.Text = "These are the pre-requisites that need to be met before any testing can begin!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Font = new System.Drawing.Font("Myanmar Text", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label1.Location = new System.Drawing.Point(44, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(437, 245);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // OpenAppButton
            // 
            this.OpenAppButton.Location = new System.Drawing.Point(518, 295);
            this.OpenAppButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OpenAppButton.Name = "OpenAppButton";
            this.OpenAppButton.Size = new System.Drawing.Size(99, 43);
            this.OpenAppButton.TabIndex = 2;
            this.OpenAppButton.Text = "Open App";
            this.OpenAppButton.UseVisualStyleBackColor = true;
            this.OpenAppButton.Click += new System.EventHandler(this.OpenAppButton_Click);
            // 
            // StartUpPopUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 360);
            this.Controls.Add(this.OpenAppButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PopUpHeadline);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "StartUpPopUpForm";
            this.Text = "ML Automation Pre-requisites";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PopUpHeadline;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OpenAppButton;
    }
}