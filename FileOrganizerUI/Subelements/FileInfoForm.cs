using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using FileDBManager;

namespace FileOrganizerUI.Subelements
{
    public partial class FileInfoForm : Form
    {
        private ILogger logger;
        private GetFileMetadataType fileInfo;
        public FileInfoForm(ILogger logger)
        {
            InitializeComponent();
            this.logger = logger;
        }

        public void SetFileInfo(GetFileMetadataType file)
        {
            fileInfo = file;
        }

        
    }
}
