﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Specialized;
using SymLinkMaker;
using FileDBManager;
using FileDBManager.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileOrganizerCore
{
    /// <summary>
    /// Core of the file organizer. Configuration done in the config.xml. This file must have 
    /// a node tagged DB with the db name and a node named DefaultFolder with the path to 
    /// a default location to store symlinks.
    /// </summary>
    public partial class FileOrganizer
    {
        ILogger logger;
        SymLinkMaker.SymLinkMaker symlinkmaker;
        FileDBManagerClass db;
        FileTypeDeterminer typeDet;
        ConfigLoader configLoader;
        private string root;
        private readonly string configFilename = "config.xml";

        //Recently searched tags, tag categories and searched files are kept in memory
        List<GetTagCategoryType> tagCategories;
        public List<GetTagCategoryType> TagCategories { get { return tagCategories; } }
        bool tagCategoriesClean;
        List<GetFileMetadataType> activeFiles;
        public List<GetFileMetadataType> ActiveFiles { get { return activeFiles; } }
        List<GetTagType> activeTags;
        public List<GetTagType> ActiveTags { get { return activeTags; } }
        GetCollectionType activeCollection;
        public GetCollectionType ActiveCollection { get { return activeCollection; } }

        /* WIN32 API imports/definitions section */

        /* End of win32 imports*/

        public FileOrganizer(ILogger logger, NameValueCollection config = null)
        {
            this.logger = logger;
            if (config is null) {
                configLoader = new ConfigLoader(configFilename, logger);
            }
            
            typeDet = new FileTypeDeterminer();
            root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
            logger.LogDebug("Executable root: " + root);
            
            string dbPath;
            if (config is null) {
                dbPath = Path.Combine(root, configLoader.GetNodeValue("DB"));
            } else {
                dbPath = Path.Combine(root, config.Get("DB"));
            }

            db = new FileDBManagerClass(dbPath, logger);
            try {
                
                string symlinkPath;
                if (config is null) {
                    symlinkPath = configLoader.GetNodeValue("DefaultFolder");
                } else {
                    symlinkPath = config.Get("DefaultFolder");
                }

                if (!Path.IsPathRooted(symlinkPath)) symlinkPath = Path.Combine(root, symlinkPath);
                if (!Directory.Exists(symlinkPath)) {
                    Directory.CreateDirectory(symlinkPath);
                }
                symlinkmaker = new SymLinkMaker.SymLinkMaker(symlinkPath, logger);
            } catch (ArgumentException ex) {
                logger.LogError(ex, "Path is not rooted or not found");
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

            try {
                var categoryRes = GetTagCategories();
                tagCategories = categoryRes.Result ?? new List<GetTagCategoryType>();
                tagCategoriesClean = true;
                if (categoryRes.Result != null) res.SetResult(true);
            } catch {
                res.AddError(ErrorType.SQL, "Error accessing DB");
            }

            return res;
        }

        public void Stop()
        {
            tagCategories = null;
            activeFiles = null;
            activeCollection = null;
            activeTags = null;
            db.CloseConnection();
        }

        /// <summary>
        ///     Updates the symlink folder. Fails if folder not found.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult<bool> SetSymLinkFolder(string path)
        {
            var res = new ActionResult<bool>();
            symlinkmaker.ClearExisting();
            try {
                if (!Path.IsPathRooted(path)) path = Path.Combine(root, path);
                if (Directory.Exists(path)) {
                    symlinkmaker = new SymLinkMaker.SymLinkMaker(path, logger);
                    res.SetResult(true);
                } else {
                    throw new ArgumentException("Folder not found");
                }
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
        ///     Creates symlinks from the most recent search result.
        /// </summary>
        /// <returns></returns>
        public ActionResult<bool> CreateSymLinksFromActiveFiles()
        {
            var res = new ActionResult<bool>();
            if (activeFiles != null && activeFiles.Count > 0) {
                List<string> filenames = activeFiles.ConvertAll(f => f.Fullname);
                res = CreateSymLinksFilenames(filenames);
            } else {
                res.AddError(ErrorType.Misc, "No file search results to create symlinks from");
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

        public string GetSymLinksRoot()
        {
            return symlinkmaker.Root;
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
            var queryResult = db.GetFileMetadata(filter);
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

        
    }
}
