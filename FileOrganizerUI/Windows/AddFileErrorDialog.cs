using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOrganizerUI.Windows
{
    public partial class AddFileErrorDialog : Form
    {
        public AddFileErrorDialog(List<string> errorMsgs)
        {
            InitializeComponent();
            foreach (string msg in errorMsgs) {
                Label lbl = new Label();
                lbl.AutoSize = true;
                lbl.Text = msg;
                ErrorLayoutPanel.Controls.Add(lbl);
            }
        }
    }
}
