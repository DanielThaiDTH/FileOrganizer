﻿using System;
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
    public partial class DeleteConfirmModal : Form
    {
        public DeleteConfirmModal()
        {
            InitializeComponent();
        }

        public void SetMessage(string msg)
        {
            DeleteConfirmLabel.Text = msg;
        }
    }
}
