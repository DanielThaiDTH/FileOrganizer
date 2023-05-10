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
using System.Reactive.Linq;
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
        SortedList<int, GetFileCollectionAssociationType> sortedFiles;
        DeleteConfirmModal DeleteModal;
        IObservable<object> editObservable;
        IDisposable editDispose;

        public bool IsDeleted { get; private set; }
        public bool IsUpdated { get; private set; }
        public string NewName { get; private set; }


        public CollectionInfoForm(ILogger logger, FileOrganizer core)
        {
            this.logger = logger;
            this.core = core;
            sortedFiles = new SortedList<int, GetFileCollectionAssociationType>();
            InitializeComponent();

            CollectionFileListView.View = View.List;
            CollectionFileListView.AllowDrop = true;
            CollectionFileListView.ItemDrag += CollectionListView_ItemDrag;
            CollectionFileListView.DragOver += CollectionListView_DragOver;
            CollectionFileListView.DragDrop += CollectionListView_DragDrop;

            UpdateButton.Click += UpdateButton_Click;

            DeleteButton.Click += DeleteButton_Click;

            DeleteModal = new DeleteConfirmModal();

            editObservable = Observable.FromEventPattern(handler =>
            {
                NameBox.TextChanged += handler;
            },
            handler =>
            {
                NameBox.TextChanged -= handler;
                //CollectionFileListView.DragDrop += handler;
            });
        }

        public void SetCollection(GetCollectionType collection)
        {
            this.collection = collection;
            NameBox.Text = collection.Name;
            NewName = collection.Name;

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

            IsDeleted = false;
            IsUpdated = false;
            UpdateButton.Enabled = false;

            editDispose = editObservable.Subscribe(args =>
            {
                bool changed = NameBox.Text != collection.Name;
                UpdateButton.Enabled = changed || IsFileOrderChanged();
            });
        }

        public void ClearCollection()
        {
            NameBox.Text = "";
            MessageText.Text = "";
            sortedFiles.Clear();
            CollectionFileListView.Clear();
            editDispose.Dispose();
        }

        #region Handlers
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ActionResult<bool> nameRes = new ActionResult<bool>();
            if (NameBox.Text != collection.Name) {
                nameRes = core.UpdateCollectionName(collection.ID, NameBox.Text);
            }

            if (nameRes.Result) {
                UpdateMessage("Updated collection name", Color.Black);
                collection.Name = NameBox.Text;
                NewName = NameBox.Text;
                IsUpdated = true;
            } else {
                UpdateMessage("Failed to update collection name", MainForm.ErrorMsgColor);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DeleteModal.Text = "Delete " + collection.Name;
            DeleteModal.SetMessage("          Confirm deletion of the collection?");
            var result = DeleteModal.ShowDialog(this);
            if (result == DialogResult.Yes) {
                var delRes = core.DeleteCollection(collection.ID);
                if (delRes.Result) {
                    logger.LogInformation("Closing info screen for deleted collection " + collection.Name);
                    IsDeleted = true;
                    DialogResult = DialogResult.No;
                    Close();
                    DialogResult = DialogResult.No;
                } else {
                    UpdateMessage("Failed to delete collection", MainForm.ErrorMsgColor);
                }
            }
        }

        private void CollectionListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem item = (ListViewItem) e.Item;
            DoDragDrop(item, DragDropEffects.Move);
        }

        private void CollectionListView_DragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = CollectionFileListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem item = CollectionFileListView.HitTest(clientPoint).Item;
            ListViewItem lastItem = CollectionFileListView.Items[CollectionFileListView.Items.Count - 1];

            if (item != null) {
                e.Effect = DragDropEffects.Move;
                CollectionFileListView.InsertionMark.Index = CollectionFileListView.Items.IndexOf(item);
            } else if (item != lastItem && lastItem.Bounds.Bottom < e.Y) {
                e.Effect = DragDropEffects.Move;
                CollectionFileListView.InsertionMark.Index = CollectionFileListView.Items.IndexOf(lastItem) + 1;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void CollectionListView_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = CollectionFileListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            ListViewItem lastItem = CollectionFileListView.Items[CollectionFileListView.Items.Count - 1];
            ListViewItem targetItem = CollectionFileListView.HitTest(clientPoint).Item;
            if (targetItem != null) {
                int targetIndex = targetItem.Index;
                int draggedIndex = draggedItem.Index;
                if (targetIndex != draggedIndex) {
                    CollectionFileListView.Items.RemoveAt(draggedIndex);
                    if (targetIndex > draggedIndex) targetIndex--;
                    CollectionFileListView.Items.Insert(targetIndex, draggedItem);
                }
            } else if (draggedItem != lastItem && lastItem.Bounds.Bottom < e.Y) {
                CollectionFileListView.Items.RemoveAt(draggedItem.Index);
                CollectionFileListView.Items.Add(draggedItem);
            }

            CollectionFileListView.InsertionMark.Index = -1;

            UpdateButton.Enabled = NameBox.Text != collection.Name || IsFileOrderChanged();
        }
        #endregion

        #region Functionality
        private void UpdateMessage(string msg, Color color)
        {
            MessageText.Text = msg;
            MessageText.ForeColor = color;
        }

        private bool IsFileOrderChanged()
        {
            bool result = false;

            FileSearchFilter filter = new FileSearchFilter();
            for (int i = 0; i < collection.Files.Count && !result; i++) {
                filter.Reset().SetIDFilter(sortedFiles[i+1].FileID);
                string name = core.GetFileData(filter).Result[0].Fullname;
                result = name != CollectionFileListView.Items[i].Text;
            }

            return result;
        }

        private void UpdateFilePositions()
        {

        }
        #endregion
    }
}
