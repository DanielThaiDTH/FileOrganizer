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
    public class FileOrganizer
    {
        ILogger logger;
        SymLinkMaker.SymLinkMaker symlinkmaker;
        FileDBManagerClass db;
        FileTypeDeterminer typeDet;
        ConfigLoader configLoader;
        private readonly string configFilename = "config.xml";

        //Tags, tag categories and searched files are kept in memory
        List<GetTagCategoryType> tagCategories;
        public List<GetTagCategoryType> TagCategories { get { return tagCategories; } }
        List<GetFileMetadataType> activeFiles;
        public List<GetFileMetadataType> ActiveFiles { get { return activeFiles; } }

        /* WIN32 API imports/definitions section */

        /* End of win32 imports*/

        public FileOrganizer(ILogger logger)
        {
            this.logger = logger;
            configLoader = new ConfigLoader(configFilename, logger);
            typeDet = new FileTypeDeterminer();
            string dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                configLoader.GetNodeValue("DB"));
            db = new FileDBManagerClass(dbPath, logger);
            try {
                symlinkmaker = new SymLinkMaker.SymLinkMaker(configLoader.GetNodeValue("DefaultFolder"), logger);
            } catch (ArgumentException ex) {
                logger.LogError(ex, "Path is not rooted");
                throw ex;
            }
        }

        /// <summary>
        ///     Initializes file organizer by loading relevant information.
        /// </summary>
        /// <returns></returns>
        public ActionResult<bool> StartUp()
        {
            var res = new ActionResult<bool>();

            var categoryRes = GetTagCategories();
            tagCategories = categoryRes.Result ?? new List<GetTagCategoryType>();

            ActionResult.AppendErrors(res, categoryRes);

            return res;
        }

        /// <summary>
        ///     Updates the symlink folder. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult<bool> SetSymLinkFolder(string path)
        {
            var res = new ActionResult<bool>();
            symlinkmaker.ClearExisting();
            try {
                symlinkmaker = new SymLinkMaker.SymLinkMaker(path, logger);
                res.SetResult(true);
            } catch (ArgumentException ex) {
                logger.LogWarning(ex, $"Error accessing path ${path} for storing symlinks");
                res.AddError(ErrorType.Path, "Symlink folder error: " + path);
            }

            return res;
        }

        /// <summary>
        ///     Creates symlinks using a files passing a file filter or all known files if filter 
        ///     is null. The symlinks will use the same filename as the original file. If names are 
        ///     repeated, the symlink will have _# appended, where # is a number that will increment 
        ///     by 1 if another number was already used for that name.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ActionResult<bool> CreateSymLinks(FileSearchFilter filter = null)
        {
            var res = new ActionResult<bool>();
            var fileDataRes = GetFileData(filter);
            if (!fileDataRes.HasError()) {
                List<GetFileMetadataType> fileData = fileDataRes.Result;
                List<string> symlinkNames = new List<string>(symlinkmaker.GetSymLinks());
                HashSet<string> usedNames = new HashSet<string>(symlinkNames);

                foreach (var data in fileData) {
                    string filepath = Path.Combine(data.Path, data.Filename);
                    string linkname = data.Filename;

                    //Fix reused names in symlinks
                    int count = 0;
                    while (usedNames.Contains(linkname)) {
                        linkname = data.Filename + "_" + count.ToString();
                        count++;
                    }

                    if (!symlinkmaker.Make(linkname, filepath, false)) {
                        string msg = $"Could not create symlink {linkname} for file {filepath}";
                        res.AddError(ErrorType.SymLinkCreate, msg);
                        logger.LogWarning(msg);
                    }

                    usedNames.Add(linkname);
                }
                res.SetResult(!res.HasError());
            } else {
                res.SetResult(false);
            }

            return res;
        }

        /// <summary>
        ///     Creates symlinks from a list of files. The filenames must be 
        ///     a full path.
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public ActionResult<bool> CreateSymLinksFilenames(IEnumerable<string> filenames)
        {
            var res = new ActionResult<bool>();
            symlinkmaker.ClearExisting();
            foreach (string filename in filenames) {
                string name = Path.GetFileName(filename);
                bool result = symlinkmaker.Make(name, filename, false);
                if (!result) {
                    res.AddError(ErrorType.SymLinkCreate, $"Error creating symlink for {filename}");
                }
            }

            if (!res.HasError()) res.SetResult(true);

            return res;
        }

        public ActionResult<bool> ClearSymLinks()
        {
            var res = new ActionResult<bool>();
            int count = symlinkmaker.GetSymLinks().Length;
            int delCount = symlinkmaker.ClearExisting();
            if (delCount != count) {
                string msg = $"Removed {delCount} symlinks, expecting {count}";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SymLinkDelete, msg);
            }

            if (!res.HasError()) res.SetResult(true);

            return res;
        }

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
            } catch (Exception ex) {
                string filterInfoStr = (filter is null) ? "" : filter.ToString();
                logger.LogError(ex, "Fatal error retrieving file data\n" + filterInfoStr);
                res.AddError(ErrorType.SQL, "Fatal error retrieving file data");
            }

            return res;
        }

        private long GetFileSize(string filename, in ActionResult<bool> res)
        {
            long size = 0;
            try {
                FileInfo info = new FileInfo(filename);
                size = info.Length;
            } catch(Exception ex) {
                string msg = $"Could not get size information for {filename}";
                logger.LogWarning(ex, msg);
                res.AddError(ErrorType.Path, msg);
            }

            res.SetResult(!res.HasError());

            return size;
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

        /// <summary>
        ///     Checks for file metadata mismatches between the actual file and the 
        ///     metadata in the db. Full filepath is used.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>
        ///     An ActionResult containing a set of column names with mismatching values.
        ///     Set will be null if errors were encountered.
        /// </returns>
        public ActionResult<HashSet<string>> CheckFileDataMismatch(string filename)
        {
            var res = new ActionResult<HashSet<string>>();
            var filter = new FileSearchFilter().SetFullnameFilter(filename);
            var queryResult = db.GetFileMetadataFiltered(filter);
            if (queryResult.Count != 1) {
                string msg = ((queryResult.Count == 0) ? "No" : "Duplicate") + $" data found for {filename}";
                res.AddError(ErrorType.Path, msg);
                logger.LogWarning("Failed metadata check because: " + msg);
            } else {
                var fileData = queryResult[0];
                var mismatchedColumns = new HashSet<string>();
                var hashRes = new ActionResult<bool>();
                var sizeRes = new ActionResult<bool>();
                var created = new DateTimeOptional(File.GetCreationTime(filename));
                string hash = Utilities.Hasher.HashFile(filename, hashRes);
                string type = typeDet.FromFilename(filename);
                long size = GetFileSize(filename, sizeRes);
                if (hashRes.Result && sizeRes.Result) {
                    if (hash != fileData.Hash) mismatchedColumns.Add("hash");
                    if (type != fileData.FileType) mismatchedColumns.Add("type");
                    if (size != fileData.Size) mismatchedColumns.Add("size");
                    if (created.Date != fileData.Created) mismatchedColumns.Add("created");
                    res.SetResult(mismatchedColumns);
                } else {
                    ActionResult.AppendErrors(res, hashRes);
                    ActionResult.AppendErrors(res, sizeRes);
                }
            }

            return res;
        }

        /// <summary>
        ///     Checks for file metadata mismatches between the actual file and the 
        ///     metadata in the db. File id is used.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>
        ///     An ActionResult containing a set of column names with mismatching values.
        ///     Set will be null if errors were encountered.
        /// </returns>
        public ActionResult<HashSet<string>> CheckFileDataMismatch(int id)
        {
            var res = new ActionResult<HashSet<string>>();
            var fileData = db.GetFileMetadata(id);

            if (fileData is null) {
                string msg = $"No data found for file with id of {id}";
                res.AddError(ErrorType.Path, msg);
                logger.LogWarning("Failed metadata check because: " + msg);
            } else {
                string filename = Path.Combine(fileData.Path, fileData.Filename);
                var mismatchedColumns = new HashSet<string>();
                var hashRes = new ActionResult<bool>();
                var sizeRes = new ActionResult<bool>();
                var created = new DateTimeOptional(File.GetCreationTime(filename));
                string hash = Utilities.Hasher.HashFile(filename, hashRes);
                string type = typeDet.FromFilename(filename);
                long size = GetFileSize(filename, sizeRes);
                if (hashRes.Result && sizeRes.Result) {
                    if (hash != fileData.Hash) mismatchedColumns.Add("hash");
                    if (type != fileData.FileType) mismatchedColumns.Add("type");
                    if (size != fileData.Size) mismatchedColumns.Add("size");
                    if (created.Date != fileData.Created) mismatchedColumns.Add("created");
                    res.SetResult(mismatchedColumns);
                } else {
                    ActionResult.AppendErrors(res, hashRes);
                    ActionResult.AppendErrors(res, sizeRes);
                }
            }

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

            return res;
        }

        public ActionResult<List<GetTagCategoryType>> GetTagCategories()
        {
            var res = new ActionResult<List<GetTagCategoryType>>();

            var catgories = db.GetAllTagCategories();
            res.SetResult(catgories);
            
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

        public ActionResult<bool> AddTag(string tag, string tagCategory="")
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
            } catch(InvalidDataException ex) {
                logger.LogError(ex, "Failure to access DB to check tag category");
                res.SetResult(false);
                res.AddError(ErrorType.SQL, "DB access failure");
            }

            return res;
        }

        public ActionResult<List<GetTagType>> GetTags()
        {
            var res = new ActionResult<List<GetTagType>>();
            res.SetResult(db.GetAllTags());

            return res;
        }

        public ActionResult<List<GetTagType>> GetTagsForFile(int id)
        {
            var res = new ActionResult<List<GetTagType>>();
            res.SetResult(db.GetTagsForFile(id));

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
    }
}
