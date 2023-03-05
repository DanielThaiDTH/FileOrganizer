using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using SymLinkMaker;
using FileDBManager;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Logging.Abstractions;
using FileDBManager.Entities;

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
                    var files = db.GetFileMetadataFiltered(filter);
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
                string hash = Utilities.Hasher.HashFile(filename, res);
                string type = typeDet.FromFilename(filename);
                long size = GetFileSize(filename, res);
                bool status = db.AddFile(filename, type, hash, altname, size, created);
                if (status) {
                    res.SetResult(status);
                }
            } else {
                string msg = $"Could not find {filename}";
                logger.LogWarning(msg);
                res.AddError(ErrorType.Path, msg);
            }

            return res;
        }

        /// <summary>
        ///     Adds files to the DB. Returns status of each 
        ///     add, which will be in the same order as the list
        ///     of files provided.
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public ActionResult<List<bool>> AddFiles(IEnumerable<string> filenames)
        {
            var res = new ActionResult<List<bool>>();
            List<bool> statuses = new List<bool>();

            foreach (string filename in filenames) {
                var addRes = AddFile(filename);
                if (addRes.HasError()) ActionResult.AppendErrors(res, addRes);
                statuses.Add(addRes.Result);
            }

            res.SetResult(statuses);

            return res;
        }

        public ActionResult<bool> DeleteFile(int id)
        {
            var res = new ActionResult<bool>();

            bool status = db.DeleteFileMetadata(id);
            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed deleting {id}");
            }

            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> UpdateFileData(FileSearchFilter newInfo, FileSearchFilter filter)
        {
            var res = new ActionResult<bool>();

            bool status = db.UpdateFileMetadata(newInfo, filter);

            if (!status) {
                res.AddError(ErrorType.SQL, "Failure updating files");
                logger.LogWarning($"Failure to update files using filter: {filter} \nand new info: {newInfo}");
            }

            res.SetResult(status);

            return res;
        }

        public ActionResult<bool> AddTagCategory(string category)
        {
            var res = new ActionResult<bool>();

            bool status = db.AddTagCategory(category);
            if (!status) res.AddError(ErrorType.SQL, "Failure adding tag category");
            res.SetResult(status);
            tagCategoriesClean = false;

            return res;
        }

        public ActionResult<List<GetTagCategoryType>> GetTagCategories()
        {
            var res = new ActionResult<List<GetTagCategoryType>>();
            var catgories = db.GetAllTagCategories();
            res.SetResult(catgories);
            tagCategories = catgories;
            tagCategoriesClean = true;

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
            tagCategoriesClean = false;

            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed to delete tag category {id}");
            } else {
                tagCategories = db.GetAllTagCategories();
                tagCategoriesClean = true;
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
            tagCategoriesClean = false;

            if (!status) {
                res.AddError(ErrorType.SQL, $"Failed to change tag category {id} name to {newName}");
            } else {
                tagCategories = db.GetAllTagCategories();
                tagCategoriesClean = true;
            }

            res.SetResult(status);
            return res;
        }

        public ActionResult<bool> AddTag(string tag, string tagCategory = "")
        {
            var res = new ActionResult<bool>();

            bool status = db.AddTag(tag, tagCategory);

            if (!status) res.AddError(ErrorType.SQL, "Failed to add tag");
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

        public ActionResult<bool> UpdateTagCategory(int tagID, int categoryID)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateTagCategory(tagID, categoryID);
            if (!status) res.AddError(ErrorType.SQL, $"Could not set the tag #{tagID} to category #{categoryID}");
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
            if (!status) res.AddError(ErrorType.SQL, "Could not add file to collection");

            return res;
        }

        public ActionResult<bool> DeleteFileInCollection(int collectionID, int fileID)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteFileInCollection(collectionID, fileID);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to remove file #{fileID} from #{collectionID}");

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

        public ActionResult<bool> UpdateCollectionName(int id, string newName)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateCollectionName(id, newName);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to change collection name to {newName} for #{id}");

            return res;
        }

        public ActionResult<bool> UpdateCollectionName(string name, string newName)
        {
            var res = new ActionResult<bool>();
            bool status = db.UpdateCollectionName(name, newName);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to rename collection {name} to {newName}");

            return res;
        }

        public ActionResult<bool> DeleteCollection(int id)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteCollection(id);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to delete collection #{id}");

            return res;
        }

        public ActionResult<bool> DeleteCollection(string name)
        {
            var res = new ActionResult<bool>();
            bool status = db.DeleteCollection(name);
            if (!status) res.AddError(ErrorType.SQL, $"Failed to delete collection {name}");

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
            var filter = new FileSearchFilter().SetFilenameFilter(fullname);
            var file = db.GetFileMetadataFiltered(filter);
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
            }

            return res;
        }

        public ActionResult<bool> AddTagToFile(string filename, string tag, string tagCategory = "")
        {
            var res = new ActionResult<bool>();
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadataFiltered(filter);
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

            return res;
        }

        public ActionResult<List<GetTagType>> GetTagsForFile(string filename)
        {
            var res = new ActionResult<List<GetTagType>>();
            if (!Path.IsPathRooted(filename)) {
                filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var file = db.GetFileMetadataFiltered(filter);
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
            var file = db.GetFileMetadataFiltered(filter);
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
            var file = db.GetFileMetadataFiltered(filter);
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
            var file = db.GetFileMetadataFiltered(filter);
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
    }
}
