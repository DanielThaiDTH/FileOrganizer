using SQLite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FileDBManager.Entities;

namespace FileDBManager
{
    public class FileDBManagerClass
    {
        private SQLiteConnection db;
        private ILogger logger;

        public FileDBManagerClass(string dbLoc, ILogger logger)
        {
            db = new SQLiteConnection(dbLoc);
            this.logger = logger;
            logger.LogInformation("FileDB manager started");
            db.CreateTable<FilePath>();
            db.CreateTable<FileType>();
            db.CreateTable<FileMetadata>();
            db.CreateTable<TagCategory>();
            db.CreateTable<Tag>();
            db.CreateTable<FileTagAssociation>();
        }

        /// <summary>
        ///     Adds filemeta data to the database. Be consistent with casing in
        ///     the filepath and hash, otherwise duplicate entries may occur.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filetype"></param>
        /// <param name="hash"></param>
        /// <param name="altname"></param>
        /// <returns></returns>
        public bool AddFile(string filepath, string filetype, string hash, string altname="")
        {
            bool result;

            var fileType = new FileType
            {
                Name = filetype.ToLowerInvariant()
            };
            int num = db.Insert(fileType);
            if (num == 0) {
                logger.LogInformation(fileType + " already exists in FileType table");
            } else {
                logger.LogInformation(fileType + " added to FileType table");
            }

            int idx = filepath.LastIndexOf('\\');
            string path = filepath.Substring(0, idx);
            string filename = filepath.Substring(idx + 1);
            if (!Path.IsPathRooted(path)) {
                logger.LogWarning(path + " is not a proper full path");
                return false;
            }

            var filePath = new FilePath
            {
                PathString = path
            };
            num = db.Insert(filePath);
            if (num == 0) {
                logger.LogInformation(filePath + " already exists in FilePath table");
            } else {
                logger.LogInformation(filePath + " added to FilePath table");
            }

            int pathID = db.Query<int>("SELECT \"ID\" FROM \"FilePaths\" WHERE \"Path\" = ?", path)[0];
            logger.LogDebug($"Found ID {pathID} for {path}");
            int typeID = db.Query<int>("SELECT \"ID\" FROM \"FileTypes\" WHERE \"Name\" = ?", filetype.ToLowerInvariant())[0];
            logger.LogDebug($"Found ID {typeID} for {filetype}");

            var fileInfo = new FileMetadata
            {
                Filename = filename,
                Fullname = filepath,
                PathID = pathID,
                FileTypeID = typeID,
                AltName = altname,
                Hash = hash
            };
            result = db.Insert(fileInfo) == 1;
            logger.LogInformation($"{filepath} was" + (result ? " " : " not ") + "added to Files table");

            return result;
        }

        /// <summary>
        ///     Adds a tag to a file. Can optionally add a tag category to the tag.
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="tag"></param>
        /// <param name="tagCategory"></param>
        /// <returns>Add tag status. Will return false if it already exists.</returns>
        public bool AddTagToFile(int fileID, string tag, string tagCategory = "")
        {
            bool result;

            result = db.Query<int>("SELECT COUNT(*) FROM \"Files\" WHERE \"ID\" = ?", fileID)[0] == 1;
            if (!result) {
                logger.LogWarning(fileID + " does not exist in DB, cannot add tag");
                return result;
            }

            AddTag(tag, tagCategory);

            int tagID = db.Query<int>("SELECT \"ID\" FROM \"Tags\" WHERE \"Name\" = ?", tag.ToLowerInvariant())[0];
            result = db.Query<int>("SELECT COUNT(*) FROM \"FileTagAssociations\" " +
                "WHERE \"FileID\" = ? AND \"TagID\" = ?", fileID, tagID)[0] == 0;

            if (result) {
                var association = new FileTagAssociation
                {
                    FileID = fileID,
                    TagID = tagID
                };
                result = db.Insert(association) == 1;
                if (result) logger.LogInformation($"Tag {tag.ToLowerInvariant()} added to file #{fileID}");
            }

            return result;
        }

        /// <summary>
        ///     Adds a tag category to the DB
        /// </summary>
        /// <param name="tagCategory"></param>
        /// <returns>Add tag category status. Will return false if it already exists.</returns>
        public bool AddTagCategory(string tagCategory)
        {
            bool result = false;

            if (tagCategory.Length > 0) {
                tagCategory = tagCategory.ToLowerInvariant();
                var category = new TagCategory
                {
                    Name = tagCategory
                };
                int added = db.Insert(category);
                if (added == 0) logger.LogInformation("Added new tag category: " + tagCategory);
            }

            return result;
        }

        /// <summary>
        ///     Adds a tag. Can also add an optional category to the tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagCategory"></param>
        /// <returns>Status of adding new tag. Will return if it already exists</returns>
        public bool AddTag(string tag, string tagCategory = "")
        {
            bool result;

            AddTagCategory(tagCategory);

            int categoryID = db.Query<int>("SELECT \"ID\" FROM \"TagCategories\" WHERE \"Name\" = ?", tagCategory)[0];
            logger.LogDebug($"Found ID {categoryID} for {tagCategory}");

            var tagObj = new Tag
            {
                Name = tag.ToLowerInvariant(),
                CategoryID = categoryID
            };
            result = db.Insert(tagObj) == 1;
            logger.LogInformation(tag + " was" + (result ? " " : " not ") + "added to Tags table");

            return result;
        }
    }
}
