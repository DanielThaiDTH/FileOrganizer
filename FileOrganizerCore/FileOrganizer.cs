using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
        ConfigLoader configLoader;
        private readonly string configFilename = "config.xml";

        /* WIN32 API imports/definitions section */

        /* End of win32 imports*/

        public FileOrganizer(ILogger logger)
        {
            this.logger = logger;
            configLoader = new ConfigLoader(configFilename, logger);
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
        ///     Updates the symlink folder. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult SetSymLinkFolder(string path)
        {
            var res = new ActionResult();
            symlinkmaker.ClearExisting();
            try {
                symlinkmaker = new SymLinkMaker.SymLinkMaker(path, logger);
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
        public ActionResult CreateSymLinks(FileSearchFilter filter = null)
        {
            var res = new ActionResult();
            var fileDataRes = GetFileData(filter);
            if (!fileDataRes.HasError()) {
                List<GetFileMetadataType> fileData = (List<GetFileMetadataType>)fileDataRes.Result;
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
            } else {
                res = fileDataRes;
            }

            return res;
        }

        /// <summary>
        ///     Creates symlinks from a list of files. The filenames must be 
        ///     a full path.
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public ActionResult CreateSymLinksFilenames(IEnumerable<string> filenames)
        {
            var res = new ActionResult();
            symlinkmaker.ClearExisting();
            foreach (string filename in filenames) {
                string name = Path.GetFileName(filename);
                bool result = symlinkmaker.Make(name, filename, false);
                if (!result) {
                    res.AddError(ErrorType.SymLinkCreate, $"Error creating symlink for {filename}");
                }
            }

            return res;
        }

        public ActionResult ClearSymLinks()
        {
            var res = new ActionResult();
            int count = symlinkmaker.GetSymLinks().Length;
            int delCount = symlinkmaker.ClearExisting();
            if (delCount != count) {
                string msg = $"Removed {delCount} symlinks, expecting {count}";
                logger.LogWarning(msg);
                res.AddError(ErrorType.SymLinkDelete, msg);
            }

            return res;
        }

        public ActionResult GetFileData(FileSearchFilter filter = null)
        {
            var res = new ActionResult();
            try {
                if (filter is null) {
                    var files = db.GetAllFileMetadata();
                    res.SetResult(typeof(List<GetFileMetadataType>), files);
                } else {
                    var files = db.GetFileMetadataFiltered(filter);
                    res.SetResult(typeof(List<GetFileMetadataType>), files);
                }
            } catch (Exception ex) {
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
        public ActionResult AddFile(string filename)
        {
            var res = new ActionResult();

            if (!Path.IsPathRooted(filename)) {
                string msg = $"{filename} is not a rooted path";
                logger.LogWarning(msg);
                res.AddError(ErrorType.Path, "Internal path error");
            } else if (File.Exists(filename)) {
                var created = File.GetCreationTime(filename);

            } else {
                string msg = $"Could not find {filename}";
                logger.LogWarning(msg);
                res.AddError(ErrorType.Path, msg);
            }
            
            return res;
        }
    }
}
