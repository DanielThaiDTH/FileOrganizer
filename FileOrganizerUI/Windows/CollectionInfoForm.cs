using FileOrganizerCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileDBManager;
using FileDBManager.Entities;

namespace FileOrganizerUI.Windows
{
    public partial class CollectionInfoForm : Form
    {
        ILogger logger;
        FileOrganizer core;
        GetCollectionType collection;

        public CollectionInfoForm(ILogger logger, FileOrganizer core)
        {
            this.logger = logger;
            this.core = core;
            InitializeComponent();

            CollectionFileListView.View = View.List;
        }

        public void SetCollection(GetCollectionType collection)
        {
            this.collection = collection;
            NameBox.Text = collection.Name;

            var sortedFiles = new SortedList<int, GetFileCollectionAssociationType>();
            foreach (var file in collection.Files) {
                sortedFiles.Add(file.Position, file);
            }

            var filter = new FileSearchFilter();
            foreach (var filePair in sortedFiles) {
                var subfilter = new FileSearchFilter();
                subfilter.SetIDFilter(filePair.Value.FileID).SetOr(true);
                filter.AddSubfilter(subfilter);
            }

            var files = core.GetFileData(filter).Result;

            foreach (var filePair in sortedFiles) {
                string name = files.Find(f => f.ID == filePair.Value.FileID).Fullname;
                CollectionFileListView.Items.Add(new ListViewItem(name));
            }
        }

        #region Handlers

        #endregion

        #region Functionality
        
        #endregion
    }
}
