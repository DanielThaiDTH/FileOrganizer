﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using SymLinkMaker;
using FileDBManager;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Logging.Abstractions;
using FileDBManager.Entities;
using System.Linq;
using System.Drawing;

namespace FileOrganizerCore
{
    public partial class FileOrganizer
    {
        /// <summary>
        ///     Returns all files matching criteria set by the filter parameter. If it is 
        ///     not provided, all files will be returned.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ActionResult<List<GetFileMetadataType>> GetFileData(FileSearchFilter filter = null)
        {
            var res = new ActionResult<List<GetFileMetadataType>>();
            try {
                if (filter is null) {
                    var files = db.GetAllFileMetadata();
                    res.SetResult(files);
                } else {
                    var files = db.GetFileMetadata(filter);
                    res.SetResult(files);
                }
                activeFiles = res.Result;
            }
            catch (Exception ex) {
                string filterInfoStr = (filter is null) ? "" : filter.ToString();
                logger.LogError(ex, "Fatal error retrieving file data\n" + filterInfoStr);
                res.AddError(ErrorType.SQL, "Fatal error retrieving file data");
            }

            return res;
        }

        /// <summary>
        ///     Adds a file to the system. Should be an absolute path.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult<bool> AddFile(string filename, string altname = "")
        {
            var res = new ActionResult<bool>();

            if (!Path.IsPathRooted(filename)) {
                string msg = $"{filename} is not a rooted path";
                logger.LogWarning(msg);
                res.AddError(ErrorType.Path, "Internal path error");
            } else if (File.Exists(filename)) {
                var created = new DateTimeOptional(File.GetCreationTime(filename));
                string hash = AutoHash ? Utilities.Hasher.HashFile(filename, res) : "";
                string type = typeDet.FromFilename(filename);
                long size = GetFileSize(filename, res);
                bool status = db.AddFile(filename, type, hash, altname, size, created);
                res.SetResult(status);
                if (!status) {
                    res.AddError(ErrorType.SQL, $"Could not add {filename}");
                }
            } else {
                string msg = $"Could not add {filename} due to path issues";
                logger.LogWarning(msg);
                res.AddError(ErrorType.Path, msg);
            }

            return res;
        }

        /// <summary>
        ///     Adds files to the DB. Returns status of each 
        ///     add, which will be in the same order as the list
        ///     of files provided. Should be absolute paths. Files must 
        ///     be able to be found in the filesystem.
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public ActionResult<List<bool>> AddFiles(IEnumerable<string> filenames)
        {
            var res = new ActionResult<List<bool>>();
            List<bool> statuses = new List<bool>();

            db.StartTransaction();
            foreach (string filename in filenames) {
                var addRes = AddFile(filename);
                if (addRes.HasError()) ActionResult.AppendErrors(res, addRes);
                statuses.Add(addRes.Result);
            }
            db.FinishTransaction();

            res.SetResult(statuses);

            return res;
        }

        /// <summary>
        ///     Deletes a file and also removes it from collections.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<bool> DeleteFile(int id)
        {
            var res = new ActionResult<bool>();

            //Save positions of the file in each collection that has it, then 
            //delete it from that collection. Positions are saved in case it file deletion will fail
            var affectedCollections = new Dictionary<int, Dictionary<int, int>>();
            foreach (var collection in db.FileCollectionSearch("")) {
                if (collection.Files.Any(f => f.FileID == id)) {
                    logger.LogInformation($"Removing file from collection {collection.Name}");
                    var savedPosition = new Dictionary<int, int>();
                    savedPosition.Add(id, collection.Files.Find(f => f.FileID == id).Position);
                    affectedCollections.Add(collection.ID, savedPosition);
                    db.DeleteFileInCollection(collection.ID, id);
                }
            }

            bool status = db.DeleteFileMetadata(id);


            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed deleting {id}");
                logger.LogDebug("Reverting file removal from collections");
                foreach (var col_pair in affectedCollections) {
                    foreach (var pos_pair in col_pair.Value) {
                        db.AddFileToCollection(col_pair.Key, pos_pair.Key, pos_pair.Value);
                    }
                }
            }

            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateFileData(FileMetadata newInfo, FileSearchFilter filter)
        {
            logger.LogDebug("Updating with data\n" + newInfo.ToString());

            var res = new ActionResult<bool>();
            bool status = db.UpdateFileMetadata(newInfo, filter);

            if (!status) {
                res.AddError(ErrorType.SQL, "Failure updating files");
                logger.LogWarning($"Failure to update files using filter: {filter} \nand new info: {newInfo}");
            }

            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateFileData(FileMetadata newInfo, int id)
        {
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetIDFilter(id);
            return UpdateFileData(newInfo, filter);
        }

        public ActionResult<bool> UpdateFileData(FileMetadata newInfo, string filename)
        {
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter(filename);
            return UpdateFileData(newInfo, filter);
        }

        public ActionResult<int> GetPathID(string path)
        {
            ActionResult<int> res = new ActionResult<int>();
            res.SetResult(db.GetPathID(path));
            if (res.Result == -1) {
                res.AddError(ErrorType.SQL, $"Path {path} not found");
            }

            return res;
        }

        /// <summary>
        ///     Also updates the TagCategories property.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public ActionResult<bool> AddTagCategory(string category)
        {
            var res = new ActionResult<bool>();

            bool status = db.AddTagCategory(category);
            if (!status) res.AddError(ErrorType.SQL, "Failure adding tag category");
            res.SetResult(status);
            GetTagCategories();

            return res;
        }

        /// <summary>
        ///     Also updates the TagCategories property
        /// </summary>
        /// <returns></returns>
        public ActionResult<List<GetTagCategoryType>> GetTagCategories()
        {
            var res = new ActionResult<List<GetTagCategoryType>>();
            var catgories = db.GetAllTagCategories();
            res.SetResult(catgories);
            tagCategories = catgories;

            return res;
        }

        /// <summary>
        ///     Deletes a tag category. Returns a result that 
        ///     can possibly contain an <see cref="ErrorType.SQL"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult<bool> DeleteTagCategory(int id)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteTagCategory(id);

            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed to delete tag category {id}");
            } else {
                tagCategories = db.GetAllTagCategories();
            }

            res.SetResult(status);
            return res;
        }

        /// <summary>
        ///     Updates tag category name to a new name. 
        ///     If failed return result with error type of <see cref="ErrorType.SQL"/>.
        ///     All tag category names must be unique.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public ActionResult<bool> UpdateTagCategoryName(int id, string newName)
        {
            var res = new ActionResult<bool>();

            bool status = db.UpdateTagCategoryName(id, newName);

            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed to change tag category {id} name to {newName}");
            } else {
                tagCategories = db.GetAllTagCategories();
            }

            res.SetResult(status);
            return res;
        }

        public ActionResult<bool> UpdateTagCategoryColor(int id, Color color)
        {
            var res = new ActionResult<bool>();

            bool status = db.UpdateTagCategoryColor(id, color.ToArgb());

            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed to change tag category {id} color to #{color.ToArgb().ToString("X")}");
            } else {
                tagCategories = db.GetAllTagCategories();
            }

            res.SetResult(status);
            return res;
        }

        public ActionResult<bool> AddTag(string tag, string tagCategory = "")
        {
            var res = new ActionResult<bool>();

            bool status = db.AddTag(tag, tagCategory);

            if (!status) {
                res.AddError(ErrorType.SQL, "Failed to add tag");
            } else {
                AllTags = GetTags().Result;
            }
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> AddTagToFile(int fileID, string tag, string tagCategory = "")
        {
            var res = new ActionResult<bool>();

            try {
                bool status = db.AddTagToFile(fileID, tag, tagCategory);
                if (!status) res.AddError(ErrorType.SQL, $"Failed to add tag {tag} to file");
                res.SetResult(status);
            }
            catch (InvalidDataException ex) {
                logger.LogError(ex, "Failure to access DB to check tag category");
                res.SetResult(false);
                res.AddError(ErrorType.SQL, "DB access failure");
            }

            return res;
        }

        public ActionResult<List<GetTagType>> GetTags()
        {
            var res = new ActionResult<List<GetTagType>>();
            activeTags = db.GetAllTags();
            AllTags = new List<GetTagType>(activeTags);
            res.SetResult(activeTags);

            return res;
        }

        public ActionResult<List<GetTagType>> GetTagsForFile(int id)
        {
            var res = new ActionResult<List<GetTagType>>();
            activeTags = db.GetTagsForFile(id);
            res.SetResult(activeTags);

            return res;
        }

        public ActionResult<bool> DeleteTag(int id)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteTag(id);
            if (!status) res.AddError(ErrorType.SQL, "Could not delete tag");
            if (status) GetTags();
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> DeleteTagFromFile(int fileID, int tagID)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteTagFromFile(fileID, tagID);
            if (!status) res.AddError(ErrorType.SQL, "Could not delete tag from file");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateTagName(string name, string oldName)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagName(name, oldName);
            if (!status) res.AddError(ErrorType.SQL, $"Could not rename tag {oldName} to {name}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateTagName(string newName, int id)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagName(newName, id);
            if (!status) res.AddError(ErrorType.SQL, $"Could not rename tag #{id} to {newName}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateTagDescription(int tagID, string desc)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagDescription(tagID, desc);
            if (!status) res.AddError(ErrorType.SQL, "Could not update tag description");
            res.SetResult(status);

            return res;
        }

        /// <summary>
        ///     Updates the tag category for a tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public ActionResult<bool> UpdateTagCategory(string tagName, int categoryID)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagCategory(tagName, categoryID);
            if (!status) res.AddError(ErrorType.SQL,
                $"Could not change the category of tag {tagName} to category #{categoryID}");
            res.SetResult(status);

            return res;
        }

        /// <summary>
        ///     Updates tag category for tag. Category id of -1 means the category will be removed
        ///     from the tag.
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public ActionResult<bool> UpdateTagCategory(int tagID, int categoryID)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagCategory(tagID, categoryID);
            if (!status) res.AddError(ErrorType.SQL, $"Could not set the tag #{tagID} to category #{categoryID}");
            if (status) GetTags();
            res.SetResult(status);

            return res;
        }

        /// <summary>
        /// Updates tag parent. Parent ID of -1 will make so that there is no parent for the tag.
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public ActionResult<bool> UpdateTagParent(int tagID, int parentID)
        {
            var res = new ActionResult<bool>();

            if (parentID < 1) parentID = -1;
            string msg;
            bool status = db.UpdateTagParent(tagID, parentID, out msg);
            if (!status) res.AddError(ErrorType.SQL, msg);
            if (status) AllTags.Find(t => t.ID == tagID).ParentTagID = parentID;
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> AddCollection(string name, IEnumerable<int> fileSequence = null)
        {
            var res = new ActionResult<bool>();
            bool status = db.AddCollection(name, fileSequence);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to add collection {name}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> AddFileToCollection(int collectionID, int fileID, int insertindex = -1)
        {
            var res = new ActionResult<bool>();
            bool status = db.AddFileToCollection(collectionID, fileID, insertindex);
            if (status) {
                var collection = ActiveCollections.Find(c => c.ID == collectionID);
                if (collection != null) {
                    var updatedCollection = db.GetFileCollection(collectionID);
                    collection.Files = updatedCollection.Files;
                    logger.LogInformation("Updating file association of collection in cached list");
                } else {
                    logger.LogInformation("Collection not in cached list, not updating");
                }
            } else {
                res.AddError(ErrorType.SQL, "Could not add file to collection");
            }
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> DeleteFileInCollection(int collectionID, int fileID)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteFileInCollection(collectionID, fileID);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to remove file #{fileID} from #{collectionID}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<GetCollectionType> GetFileCollection(int id)
        {
            var res = new ActionResult<GetCollectionType>();
            var collection = db.GetFileCollection(id);
            if (collection is null) {
                res.AddError(ErrorType.SQL, $"Could not find collection #{id}");
            }
            res.SetResult(collection);
            activeCollection = collection;

            return res;
        }

        public ActionResult<GetCollectionType> GetFileCollection(string name)
        {
            var res = new ActionResult<GetCollectionType>();
            var collection = db.GetFileCollection(name);
            if (collection is null) {
                res.AddError(ErrorType.SQL, $"Could not find collection {name}");
            }
            res.SetResult(collection);
            activeCollection = collection;

            return res;
        }

        /// <summary>
        ///     Searches for multiple collections matching the query. Empty query will 
        ///     return all collections.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult<List<GetCollectionType>> SearchFileCollection(string query)
        {
            var res = new ActionResult<List<GetCollectionType>>();
            var collections = db.FileCollectionSearch(query);
            res.SetResult(collections);
            ActiveCollections = collections;

            return res;
        }

        public ActionResult<bool> UpdateCollectionName(int id, string newName)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateCollectionName(id, newName);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to change collection name to {newName} for #{id}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateCollectionName(string name, string newName)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateCollectionName(name, newName);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to rename collection {name} to {newName}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> DeleteCollection(int id)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteCollection(id);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to delete collection #{id}");
            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> DeleteCollection(string name)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteCollection(name);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to delete collection {name}");
            res.SetResult(status);

            return res;
        }


        /* Composite functions (calling multiple FileDBManager functions) */

        /// <summary>
        ///     Deletes a file. If the filename is not rooted, will 
        ///     assume it is relative to the current directory.
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public ActionResult<bool> DeleteFile(string fullname)
        {
            var res = new ActionResult<bool>();
            if (!Path.IsPathRooted(fullname)) {
                logger.LogDebug($"{fullname} is a relative path");
                fullname = Path.Combine(Directory.GetCurrentDirectory(), fullname);
            }
            logger.LogDebug($"Delete path: {fullname}");
            var filter = new FileSearchFilter().SetFullnameFilter(fullname);
            var file = db.GetFileMetadata(filter);
            if (file.Count == 1) {
                bool status = db.DeleteFileMetadata(file[0].ID);
                if (!status) res.AddError(ErrorType.SQL, $"Failed to delete {fullname}");
                res.SetResult(status);
            } else {
                string msg = $"No file named {fullname} to delete";
                res.AddError(ErrorType.SQL, msg);
                logger.LogWarning(msg);
                res.SetResult(false);
            }

            return res;
        }

        public ActionResult<bool> DeleteTagCategory(string name)
        {
            var res = new ActionResult<bool>();
            var category = tagCategories.Find(tc => tc.Name == name.ToLowerInvariant());
            if (category is null) {
                res.AddError(ErrorType.SQL, $"{name} is not a known category");
                res.SetResult(false);
            } else {
                bool status = db.DeleteTagCategory(category.ID);
                if (!status) res.AddError(ErrorType.SQL, $"Could not delete category {name}");
                res.SetResult(status);
            }

            return res;
        }

        public ActionResult<bool> UpdateTagCategoryName(string name, string newName)
        {
            var res = new ActionResult<bool>();
            var category = tagCategories.Find(tc => tc.Name == name.ToLowerInvariant());
            if (category is null) {
                res.AddError(ErrorType.SQL, $"{name} is not a known category");
                res.SetResult(false);
            } else {
                bool status = db.UpdateTagCategoryName(category.ID, newName);
                if (!status) res.AddError(ErrorType.SQL, $"Could not rename category {name} to {newName}");
                res.SetResult(status);
                GetTagCategories();
            }

            return res;
        }

        /// <summary>
        /// Adds tag to file. Also adds the category of the tag along with any ancestor tags.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="tag"></param>
        /// <param name="tagCategory"></param>
        /// <returns></returns>
        public ActionResult<bool> AddTagToFile(string filename, string tag, string tagCategory = "")
        {
            var res = new ActionResult<bool>();
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadata(filter);
            if (file.Count == 1) {
                bool status = db.AddTagToFile(file[0].ID, tag, tagCategory);
                if (!status) res.AddError(ErrorType.SQL, $"Failed to add tag {tag} to {filename}");
                res.SetResult(status);
            } else {
                string msg = $"No file {filename} to add tag {tag} to";
                res.AddError(ErrorType.SQL, msg);
                logger.LogWarning(msg);
                res.SetResult(false);
            }

            //Add parent tags as well
            if (res.Result) {
                GetTagType currTag = null;
                int tag_id = AllTags.Find(t => t.Name == tag).ID;
                do {
                    currTag = AllTags.Find(t => t.ID == tag_id);
                    if (currTag != null) {
                        db.AddTagToFile(file[0].ID, currTag.Name);
                        tag_id = currTag.ParentTagID;
                    }
                } while (currTag != null);
            }

            return res;
        }

        /// <summary>
        ///     Adds all tags to each file. Also adds ancestor tags.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public ActionResult<bool> AddTagsToFiles(List<string> files, List<string> tags)
        {
            ActionResult<bool> result = new ActionResult<bool>();
            result.SetResult(true);
            db.StartTransaction();

            foreach (string filename in files) {
                foreach (string tagname in tags) {
                    var file = ActiveFiles.Find(f => f.Fullname == filename);
                    var tag = AllTags.Find(t => t.Name == tagname);
                    var addRes = AddTagToFile(file.ID, tag.Name);
                    if (!addRes.Result) {
                        result.SetResult(false);
                        ActionResult.AppendErrors(result, addRes);
                    } else {
                        //Add parent tags as well
                        while (tag != null) {
                            db.AddTagToFile(file.ID, tag.Name);
                            tag = AllTags.Find(t => t.ID == tag.ParentTagID);
                        } 
                    }
                }
            }

            db.FinishTransaction();

            GetTags();

            return result;
        }

        public ActionResult<List<GetTagType>> GetTagsForFile(string filename)
        {
            var res = new ActionResult<List<GetTagType>>();
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadata(filter);
            if (file.Count == 1) {
                var tags = db.GetTagsForFile(file[0].ID);
                res.SetResult(tags);
            } else {
                string msg = $"Can't find tags for {filename} because it does not exist";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SQL, msg);
                res.SetResult(null);
            }

            return res;
        }

        public ActionResult<bool> DeleteTag(string name)
        {
            var res = new ActionResult<bool>();
            var file = db.GetAllTags().Find(t => t.Name == name);
            if (file is null) {
                string msg = $"Tag {name} cannot be deleted because it doesn't exist";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SQL, msg);
                res.SetResult(false);
            } else {
                bool status = db.DeleteTag(file.ID);
                if (!status) res.AddError(ErrorType.SQL, $"Tag {name} could not be deleted");
            }


            return res;
        }

        public ActionResult<bool> DeleteTagFromFile(string filename, string tagname)
        {
            var res = new ActionResult<bool>();
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadata(filter);
            var tag = db.GetAllTags().Find(t => t.Name == tagname);
            if (file.Count == 1 && tag != null) {
                bool status = db.DeleteTagFromFile(file[0].ID, tag.ID);
                if (!status) res.AddError(ErrorType.SQL, $"Could not remove {tag} from {filename}");
                res.SetResult(status);
            } else {
                if (file.Count != 1) res.AddError(ErrorType.SQL, $"File {filename} not found");
                if (tagname is null) res.AddError(ErrorType.SQL, $"Tag {tagname} not found");
                foreach (var msg in res.Messages) {
                    logger.LogWarning(msg);
                }
                res.SetResult(false);
            }

            return res;
        }

        public ActionResult<bool> UpdateTagCategory(string tagName, string categoryName)
        {
            var res = new ActionResult<bool>();
            var category = tagCategories.Find(tc => tc.Name == categoryName.ToLowerInvariant());
            if (category != null) {
                bool status = db.UpdateTagCategory(tagName, category.ID);
                if (!status) res.AddError(ErrorType.SQL, $"Could not change category of {tagName} to {categoryName}");
                res.SetResult(status);
            } else {
                string msg = $"Cannot change category of {tagName} because it cannot be found";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SQL, msg);
                res.SetResult(false);
            }

            return res;
        }

        public ActionResult<bool> AddFileToCollection(string collectionName, string filename, int index = -1)
        {
            var res = new ActionResult<bool>();
            var collection = db.GetFileCollection(collectionName);
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadata(filter);
            if (file.Count == 1 && collection != null) {
                bool status = db.AddFileToCollection(collection.ID, file[0].ID, index);
                if (!status) res.AddError(ErrorType.SQL, $"Could not add file {filename} " +
                    $"to collection {collectionName} at position {index}");
                res.SetResult(status);
            } else {
                if (file.Count != 1) res.AddError(ErrorType.SQL, $"File {filename} not found");
                if (collection is null) res.AddError(ErrorType.SQL, $"Collection {collectionName} not found");
                foreach (var msg in res.Messages) {
                    logger.LogWarning(msg);
                }
                res.SetResult(false);
            }

            return res;
        }

        public ActionResult<bool> DeleteFileInCollection(string collectionName, string filename)
        {
            var res = new ActionResult<bool>();
            var collection = db.GetFileCollection(collectionName);
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadata(filter);
            if (file.Count == 1 && collection != null) {
                bool status = db.DeleteFileInCollection(collection.ID, file[0].ID);
                if (!status) res.AddError(ErrorType.SQL, $"Could not delete file {filename} " +
                    $"in collection {collectionName}");
                res.SetResult(status);
            } else {
                if (file.Count != 1) res.AddError(ErrorType.SQL, $"File {filename} not found");
                if (collection is null) res.AddError(ErrorType.SQL, $"Collection {collectionName} not found");
                foreach (var msg in res.Messages) {
                    logger.LogWarning(msg);
                }
                res.SetResult(false);
            }

            return res;
        }

        /// <summary>
        ///     Queries files from a list of file ids. Will return an empty list of 
        ///     results if the id list is null or empty.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult<List<GetFileMetadataType>> GetFileDataFromIDs(IEnumerable<int> ids)
        {
            var result = new ActionResult<List<GetFileMetadataType>>();

            if (ids is null) {
                result.AddError(ErrorType.Misc, "List of ids is null");
                result.SetResult(new List<GetFileMetadataType>());
                return result;
            }

            FileSearchFilter idFilters = new FileSearchFilter();
            bool atLeastOne = false;
            foreach(int id in ids) {
                atLeastOne = true;
                var filter = new FileSearchFilter().SetIDFilter(id).SetOr(true);
                idFilters.AddSubfilter(filter);
            }

            if (atLeastOne) {
                try {
                    result.SetResult(db.GetFileMetadata(idFilters));
                } catch (Exception ex) {
                    logger.LogError(ex, "Fatal error retrieving file data");
                    result.AddError(ErrorType.SQL, "Fatal error retrieving file data");
                    result.SetResult(new List<GetFileMetadataType>());
                }
            } else {
                result.SetResult(new List<GetFileMetadataType>());
            }

            return result;
        }

        /// <summary>
        ///     Switches the positions of files A and B in a collection.
        /// </summary>
        /// <param name="collectionID"></param>
        /// <param name="fileAID"></param>
        /// <param name="fileBID"></param>
        /// <returns></returns>
        public ActionResult<bool> SwitchFilePositionInCollection(int collectionID, int fileAID, int fileBID)
        {
            var result = new ActionResult<bool>();

            if (fileAID == fileBID) result.SetResult(true);

            var collection = db.GetFileCollection(collectionID);
            int positionA = 0;
            int positionB = 0;
            try {
                positionA = collection.Files.Find(f => f.FileID == fileAID).Position;
                positionB = collection.Files.Find(f => f.FileID == fileBID).Position;
            } catch (Exception e) {
                string errMsg = "File not found in collection: " + e.Message;
                logger.LogWarning(errMsg);
                result.AddError(ErrorType.SQL, errMsg);
                result.SetResult(false);
            }
            
            if (!result.HasError() && positionA != positionB) {
                bool shiftRes = true;
                shiftRes = shiftRes && db.UpdateFilePositionInCollection(collectionID, fileAID, -1);
                shiftRes = shiftRes && db.UpdateFilePositionInCollection(collectionID, fileBID, positionA);
                shiftRes = shiftRes && db.UpdateFilePositionInCollection(collectionID, fileAID, positionB);
                if (!shiftRes) {
                    result.AddError(ErrorType.SQL, "Something went wrong when switching file positions");
                    logger.LogWarning($"Failed to switch file positions for files {fileAID} and {fileBID} " +
                        $"in collection {collection.Name}");
                } else {
                    result.SetResult(true);
                }
            }

            return result;
        }

        /// <summary>
        ///     Changes the path for a file. If the path does not currently exist in the DB, 
        ///     will add it.
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public ActionResult<bool> ChangePathForFile(int fileID, string newPath)
        {
            ActionResult<bool> result = new ActionResult<bool>();
            var file = db.GetFileMetadata(fileID);
            
            if (file is null) {
                result.SetResult(false);
                result.AddError(ErrorType.SQL, "File not found");
                return result;
            } else if (file.Path == newPath) {
                result.SetResult(false);
                result.AddError(ErrorType.Misc, "Path is already set, no changes made");
                return result;
            }

            bool isChanged = db.ChangeFilePath(file.ID, newPath);
            result.SetResult(isChanged);
            if (!isChanged) result.AddError(ErrorType.SQL, $"Unable to change path of {file.Filename} to {newPath}");

            return result;
        }

        /// <summary>
        ///     Changes all files with old path to the new path. In the DB, the 
        ///     path id remains the same, only the path is renamed. No verification is 
        ///     done to ensure the new path is actually valid.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public ActionResult<bool> ChangePathAll(string oldPath, string newPath)
        {
            var res = new ActionResult<bool>();
            if (oldPath == newPath) {
                res.SetResult(true);
                return res;
            }

            int id = db.GetPathID(oldPath);

            if (id != -1) {
                bool status = db.ChangePath(id, newPath);
                if (status) {
                    logger.LogInformation($"All files with path {oldPath} changed to {newPath}");
                    res.SetResult(true);
                } else {
                    logger.LogWarning($"Failure to update path {oldPath} to {newPath}");
                    res.AddError(ErrorType.SQL, "Failed to update path");
                    res.SetResult(false);
                }
            } else {
                string msg = $"{oldPath} not used by any file and not known, cannot rename";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SQL, msg);
                res.SetResult(false);
            }

            return res;
        }

    }
}
