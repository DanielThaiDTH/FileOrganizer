﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using FileOrganizerCore;


namespace FileOrganizerUI.CodeBehind
{

    public class ThumbnailProxy
    {

        private ILogger logger;
        string fileroot;
        string root;
        FileTypeDeterminer det;
        Bitmap blankThumbnail;
        public static Color TransparentColor = Color.FromArgb(0, 1, 0);
        public static Color BackgroundColor = Color.White;

        public ThumbnailProxy(ILogger logger)
        {
            this.logger = logger;
            fileroot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
            root = fileroot;
            det = new FileTypeDeterminer();
            blankThumbnail = new Bitmap(48, 48);

            using (Graphics g = Graphics.FromImage(blankThumbnail)) {
                g.Clear(BackgroundColor);
                g.DrawImage(
                    blankThumbnail,
                    new Rectangle(Point.Empty, blankThumbnail.Size),
                    0, 0, blankThumbnail.Width, blankThumbnail.Height,
                    GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        ///     Throws argument exception if path provided is not rooted.
        /// </summary>
        /// <param name="root"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetRoot(string root)
        {
            if (!Path.IsPathRooted(root)) {
                throw new ArgumentException($"Path {root} is not rooted");
            }
            logger.LogInformation("Setting thumbnail root to " + root);
            this.root = root;
        }

        public void ResetRoot()
        {
            logger.LogInformation("Resetting thumbnail root to " + fileroot);
            root = fileroot;
        }

        private Bitmap CleanBackground(Bitmap original)
        {
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetRemapTable(new ColorMap[]
            {
                new ColorMap()
                {
                    OldColor = Color.Black,
                    NewColor = BackgroundColor,
                }
            }, ColorAdjustType.Bitmap);

            using (Graphics g = Graphics.FromImage(original)) {
                g.DrawImage(
                    original,
                    new Rectangle(Point.Empty, original.Size),
                    0, 0, original.Width, original.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return original;
        }

        /// <summary>
        ///     Returns a thumbnail for a file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="getIcon"></param>
        /// <returns></returns>
        public Image GetThumbnail(string file, bool getIcon = true)
        {
            string filepath = file;
            if (!Path.IsPathRooted(file)) {
                logger.LogInformation($"Using file root of {root} to locate {file}");
                filepath = Path.Combine(root, file);
            }

            Bitmap thumbnailData = null;
            try {
                ShellFile shellfile = ShellFile.FromFilePath(filepath);
                var thumbnail = shellfile.Thumbnail;
                if (getIcon) thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                thumbnailData = thumbnail.LargeBitmap;
                if (getIcon) thumbnailData.MakeTransparent(Color.Black);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Thumbnail load error: " + ex.GetType().ToString());
                logger.LogError(ex.Message);
                logger.LogDebug(ex.StackTrace);
                logger.LogInformation("Using blank thumbnail");
                thumbnailData = blankThumbnail;
            }

            return thumbnailData;
        }

        /// <summary>
        ///     Returns thumbnail images in a dictionary for a list of files. The list 
        ///     should be rooted or if not will use the proxy class root. The dictionary 
        ///     uses the full path of the file as an image key.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public Dictionary<string, Image> GetThumbnails(IEnumerable<string> files, HashSet<string> imageFiles = null)
        {
            if (imageFiles is null) imageFiles = new HashSet<string>();
            var imageList = new Dictionary<string, Image>();
            foreach (string file in files) {
                string filepath = file;
                if (!Path.IsPathRooted(file)) {
                    logger.LogInformation($"Using file root of {root} to locate {file}");
                    filepath = Path.Combine(root, file);
                }

                Image thumbnailData = GetThumbnail(filepath, !imageFiles.Contains(filepath));

                if (thumbnailData != null) {
                    imageList.Add(filepath, thumbnailData);
                }
            }

            return imageList;
        }

        /// <summary>
        ///     Returns a list of tasks to get thumbnails async. Returns a list of tasks that 
        ///     will return a kv pair with the filepath as the key and the Image as a value.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="imageFiles"></param>
        /// <returns></returns>
        public List<Task<KeyValuePair<string, Image>>> GetThumbnailsAsync(IEnumerable<string> files, HashSet<string> imageFiles = null)
        {
            if (imageFiles is null) imageFiles = new HashSet<string>();
            var taskList = new List<Task<KeyValuePair<string, Image>>>();

            foreach (string file in files) {
                string filepath = file;
                if (!Path.IsPathRooted(file)) {
                    logger.LogInformation($"Using file root of {root} to locate {file}");
                    filepath = Path.Combine(root, file);
                }

                taskList.Add(Task.Factory.StartNew(() => { 
                    string f = filepath;
                    Image thumbnailData = GetThumbnail(f, !imageFiles.Contains(f));
                    return new KeyValuePair<string, Image>(f, thumbnailData);
                }));
            }

            return taskList;
        }
    }
}
