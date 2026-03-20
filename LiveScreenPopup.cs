using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace FIT_Automation
{
    public partial class LiveScreenPopup : Form
    {
        public PictureBox PictureBox;
        public LiveScreenPopup()
        {
            InitializeComponent();

            // Initialize the form
            this.Text = "Live Screen";
            this.Size = new Size(800, 600); // Set the default size of the popup window
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize the PictureBox
            PictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom // Scale the image proportionally
            };

            // Add the PictureBox to the form
            this.Controls.Add(PictureBox);
        }

        private void LiveScreenPopup_Load(object sender, EventArgs e)
        {

        }

    }
}
