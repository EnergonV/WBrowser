using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroFramework.Forms;

namespace WBrowser
{
    class MetroWBrowser:MetroForm
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MetroWBrowser
            // 
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MetroWBrowser";
            this.Text = "MetroWBrowser";
            this.ResumeLayout(false);

        }
    }
}
