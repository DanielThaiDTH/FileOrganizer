﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using FileDBManager.Entities;
using SQLitePCL;

namespace FileDBManager
{
    public partial class FileDBManagerClass
    {
        public SQLiteConnection db;
        public ILogger logger;

        public FileDBManagerClass(string dbLoc, ILogger logger)
        {
            this.logger = logger;
            logger.LogInformation("Creating or opening DB at " + dbLoc);
            db = new SQLiteConnection("DataSource="+dbLoc);
            db.Open();
            CreateTable(FilePath.TableName, FilePath.Columns);
            CreateTable(FileType.TableName, FileType.Columns);
            CreateTable(FileMetadata.TableName, FileMetadata.Columns, FileMetadata.Constraint);
            CreateTable(TagCategory.TableName, TagCategory.Columns);
            CreateTable(Tag.TableName, Tag.Columns);
            CreateTable(FileTagAssociation.TableName, FileTagAssociation.Columns, FileTagAssociation.Constraint);
            CreateTable(FileCollection.TableName, FileCollection.Columns);
            CreateTable(FileCollectionAssociation.TableName, 
                FileCollectionAssociation.Columns, 
                FileCollectionAssociation.Constraint);
        }

        private string createQueryString(string query, params object[] args)
        {
            string filledQuery = "";
            if (query.Count(c => c == '?') != args.Length) {
                logger.LogWarning("Argument mismatch - query: '" + query + $"' does not have {args.Length} params");
                return null;
            }
            var queryParts = query.Split('?');
            int count = 0;
            filledQuery += queryParts[0];
            foreach (object arg in args) {
                string fixArg = "'" + arg.ToString().Replace("'", "''") + "'";
                filledQuery += fixArg + queryParts[count + 1];
                count++;
            }

            logger.LogDebug("Created query: " + filledQuery);

            return filledQuery;
        }

        private void CreateTable(string name, Dictionary<string, string> cols, string constraint = null)
        {
            string query = "CREATE TABLE IF NOT EXISTS " + name + "\n (";
            int count = 0;
            foreach (var col in cols) {
                query += col.Key + " " + col.Value;
                if (count + 1 < cols.Count || (count + 1 == cols.Count && constraint != null)) {
                    query += ",";
                }
                count++;
                query += "\n";
            }
            if (constraint != null) {
                query += constraint;
            }
            query += ")";
            logger.LogInformation($"QUERY: \n{query}");
            var com = new SQLiteCommand(query, db);
            count = com.ExecuteNonQuery();
            logger.LogInformation($"Affecting {count} row(s).");
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
        public bool AddFile(string filepath, string filetype, string hash, string altname = "")
        {
            bool result;
            string queryString = createQueryString("INSERT INTO FileTypes (Name) VALUES (?)", filetype);
            int num = new SQLiteCommand(queryString, db).ExecuteNonQuery();

            if (num == 0) {
                logger.LogInformation(filetype + " already exists in FileType table");
            } else {
                logger.LogInformation(filetype + " added to FileType table");
            }

            int idx = filepath.LastIndexOf('\\');
            string path = filepath.Substring(0, idx);
            string filename = filepath.Substring(idx + 1);
            if (!Path.IsPathRooted(path)) {
                logger.LogWarning(path + " is not a proper full path");
                return false;
            }

            queryString = createQueryString("INSERT INTO FilePaths (Path) VALUES (?)", path);
            num = new SQLiteCommand(queryString, db).ExecuteNonQuery();
            if (num == 0) {
                logger.LogInformation(path + " already exists in FilePath table");
            } else {
                logger.LogInformation(path + " added to FilePath table");
            }

            queryString = createQueryString("SELECT ID FROM FilePaths WHERE Path = ?", path);
            var com = new SQLiteCommand(queryString, db).ExecuteReader();
            com.Read();
            int pathID = com.GetInt32(0);
            logger.LogDebug($"Found ID {pathID} for {path}");
            queryString = createQueryString("SELECT ID FROM FileTypes WHERE Name = ?",
                            filetype.ToLowerInvariant());
            com = new SQLiteCommand(queryString, db).ExecuteReader();
            com.Read();
            int typeID = com.GetInt32(0);
            logger.LogDebug($"Found ID {typeID} for {filetype}");

            queryString = createQueryString("INSERT INTO Files " +
                            "(Filename, PathID, FileTypeID, Altname, Hash) VALUES (?,?,?,?,?)",
                            filename, pathID, typeID, altname, hash);
            result = new SQLiteCommand(queryString, db).ExecuteNonQuery() == 1;
            logger.LogInformation($"{filepath} was" + (result ? " " : " not ") + "added to Files table");

            return result;
        }

        public void CloseConnection()
        {
            db.Close();
        }
    }
}

//namespace FileDBManager
//{
//    public class FileDBManagerClass
//    {
//        /* File Section */

//        /// <summary>
//        ///     Returns full metadata of a file from a file ID.
//        /// </summary>
//        /// <param name="fileID"></param>
//        /// <returns>
//        ///     Dynamic type with the following format: <br/>
//        ///         <list type="bullet">
//        ///         <item>
//        ///             FileMetadata: <br/>
//        ///             <list type="bullet">
//        ///                 <item>
//        ///                     FileMetadata: <br/>
//        ///                         <list type="bullet">
//        ///                             FileMeta columns... <br/>
//        ///                         </list>
//        ///                 </item>
//        ///                 <item>
//        ///                     FileType: <br/>
//        ///                     <list type="bullet">
//        ///                         ID, Name <br/>
//        ///                     </list>
//        ///                 </item>
//        ///             </list>
//        ///         </item>
//        ///         <item>
//        ///             Path: <br/>
//        ///                 ID, Path <br/>
//        ///         </item>
//        ///         </list>
//        /// </returns>
//        public dynamic GetFileMetadata(int fileID)
//        {
//            var results = db.Table<FileMetadata>().Where(t => t.ID == fileID).ToList()
//                            .Join(db.Table<FileType>().ToList(), 
//                                    file => file.FileTypeID, 
//                                    filetype => filetype.ID, 
//                                    (file, filetype) => new {
//                                        FileMetadata = file,
//                                        FileType = filetype
//                                    })
//                            .ToList()
//                            .Join(db.Table<FilePath>().ToList(), 
//                                    a => a.FileMetadata.PathID, 
//                                    path => path.ID,
//                                    (a, path) => new { 
//                                        FileMetadata = a,
//                                        Path = path
//                                    })
//                            .ToList();

//            if (results.Count == 0) {
//                logger.LogInformation("No file found with ID of " + fileID);
//                return null;
//            }

//            return results[0];
//        }

//        /// <summary>
//        ///     Gets all metadata for all files.
//        /// </summary>
//        /// <returns>
//        /// Dynamic type with the following format: <br/>
//        ///         <list type="bullet">
//        ///         <item>
//        ///             FileMetadata: <br/>
//        ///             <list type="bullet">
//        ///                 <item>
//        ///                     FileMetadata: <br/>
//        ///                         <list type="bullet">
//        ///                             FileMeta columns... <br/>
//        ///                         </list>
//        ///                 </item>
//        ///                 <item>
//        ///                     FileType: <br/>
//        ///                     <list type="bullet">
//        ///                         ID, Name <br/>
//        ///                     </list>
//        ///                 </item>
//        ///             </list>
//        ///         </item>
//        ///         <item>
//        ///             Path: <br/>
//        ///                 ID, Path <br/>
//        ///         </item>
//        ///         </list>
//        /// </returns>
//        public List<dynamic> GetAllFileMetadata()
//        {
//            var results = db.Table<FileMetadata>()
//                            .Join(db.Table<FileType>(),
//                                    file => file.FileTypeID,
//                                    filetype => filetype.ID,
//                                    (file, filetype) => new {
//                                        FileMetadata = file,
//                                        FileType = filetype
//                                    })
//                            .Join(db.Table<FilePath>(),
//                                    a => a.FileMetadata.PathID,
//                                    path => path.ID,
//                                    (a, path) => new {
//                                        FileMetadata = a,
//                                        Path = path
//                                    })
//                            .ToList<dynamic>();

//            if (results.Count == 0) {
//                logger.LogInformation("No files found");
//                return null;
//            }

//            return results;
//        }

//        /// <summary>
//        ///     Applies standard match or equality filters to file metadata search.
//        /// </summary>
//        /// <param name="filter"></param>
//        /// <returns>
//        /// Dynamic type with the following format: <br/>
//        ///         <list type="bullet">
//        ///         <item>
//        ///             FileMetadata: <br/>
//        ///             <list type="bullet">
//        ///                 <item>
//        ///                     FileMetadata: <br/>
//        ///                         <list type="bullet">
//        ///                             FileMeta columns... <br/>
//        ///                         </list>
//        ///                 </item>
//        ///                 <item>
//        ///                     FileType: <br/>
//        ///                     <list type="bullet">
//        ///                         ID, Name <br/>
//        ///                     </list>
//        ///                 </item>
//        ///             </list>
//        ///         </item>
//        ///         <item>
//        ///             Path: <br/>
//        ///                 ID, Path <br/>
//        ///         </item>
//        ///         </list>
//        /// </returns>
//        public List<dynamic> GetFileMetadataFiltered(FileSearchFilter filter)
//        {
//            var result = db.Table<FileMetadata>();
//            if (filter.UsingID) {
//                result = result.Where(t => t.ID == filter.ID);
//            }
//            if (filter.UsingPathID) {
//                result = result.Where(t => t.PathID == filter.PathID);
//            }
//            if (filter.UsingFilename) {
//                if (filter.FilenameFilterExact) {
//                    result = result.Where(t => t.Filename == filter.Filename);
//                } else {
//                    result = result.Where(t => t.Filename.Contains(filter.Filename));
//                }
//            }
//            if (filter.UsingFullname) {
//                if (filter.FullnameFilterExact) {
//                    result = result.Where(t => t.Fullname == filter.Fullname);
//                } else {
//                    result = result.Where(t => t.Fullname.Contains(filter.Fullname));
//                }
//            }
//            if (filter.UsingAltname) {
//                if (filter.AltnameFilterExact) {
//                    result = result.Where(t => t.AltName == filter.Altname);
//                } else {
//                    result = result.Where(t => t.AltName.Contains(filter.Altname));
//                }
//            }
//            if (filter.UsingFileTypeID) {
//                result = result.Where(t => t.FileTypeID == filter.FileTypeID);
//            }
//            if (filter.UsingHash) {
//                if (filter.hashFilterExact) {
//                    result = result.Where(t => t.Hash == filter.Hash);
//                } else {
//                    result = result.Where(t => t.Hash.Contains(filter.Hash));
//                }
//            }

//            Func<FilePath, bool> PathFunc = (FilePath t) => 
//            {
//                if (!filter.UsingPath) return true;
//                if (filter.PathFilterExact) {
//                    return t.PathString == filter.Path;
//                } else {
//                    return t.PathString.Contains(filter.Path);
//                }
//            };

//            Func<FileType, bool> TypeFunc = (FileType t) =>
//            {
//                if (!filter.UsingFileType) return true;
//                if (filter.FileTypeFilterExact) {
//                    return t.Name == filter.FileType;
//                } else {
//                    return t.Name.Contains(filter.FileType);
//                }
//            };

//            var resultA = result.Join(db.Table<FileType>().Where(TypeFunc),
//                                    file => file.FileTypeID,
//                                    filetype => filetype.ID,
//                                    (file, filetype) => new
//                                    {
//                                        FileMetadata = file,
//                                        FileType = filetype
//                                    })
//                                .Join(db.Table<FilePath>().Where(PathFunc),
//                                    a => a.FileMetadata.PathID,
//                                    path => path.ID,
//                                    (a, path) => new {
//                                        FileMetadata = a,
//                                        Path = path
//                                    });

//            return resultA.ToList<dynamic>();
//        }

//        /// <summary>
//        ///     Deletes a file and all dependendent entities.
//        /// </summary>
//        /// <param name="id">The file ID</param>
//        /// <returns>Result of deletion</returns>
//        public bool DeleteFileMetadata(int id)
//        {
//            bool result;

//            logger.LogInformation("Delete file metadata with id of " + id);
//            result = db.Delete<FileMetadata>(id) == 1;

//            if (result) {
//                db.Execute("DELETE FROM FileCollectionAssociations WHERE FileID = ?", id);
//                db.Execute("DELETE FROM FileTagAssociations WHERE FileID = ?", id);
//                logger.LogInformation("File metadata and dependencies removed");
//            } else {
//                logger.LogWarning("Could not delete file metadata with id of " + id);
//            }

//            return result;
//        }

//        /// <summary>
//        ///     Updates a file's metadata. The info object must 
//        ///     have the file id.
//        /// </summary>
//        /// <param name="info"></param>
//        /// <returns>Status of update</returns>
//        public bool UpdateFileMetadata(FileMetadataUpdateObj info)
//        {
//            if (info.ID < 0) {
//                logger.LogWarning("No or negative ID provided");
//                return false;
//            }

//            List<string> cols = new List<string>();
//            List<object> vals = new List<object>();
//            if (info.Path != null) {
//                db.Execute("INSERT OR IGNORE INTO FilePaths (Path) VALUES (?)", info.Path);
//                var results = db.ExecuteScalar<int>("SELECT ID FROM FilePaths WHERE Path = ?", info.Path);
//                cols.Add("PathID");
//                vals.Add(results);
//            } 
//            if (info.FileType != null) {
//                db.Execute("INSERT OR IGNORE INTO FileTypes (Name) VALUES (?)", info.FileType);
//                var results = db.ExecuteScalar<int>("SELECT ID FROM FileTypes WHERE Name = ?", info.FileType);
//                cols.Add("FileTypeID");
//                vals.Add(results);
//            }
//            if (info.Filename != null) {
//                cols.Add("Filename");
//                vals.Add(info.Filename);
//            }
//            if (info.Altname != null) {
//                cols.Add("Altname");
//                vals.Add(info.Altname);
//            }
//            if (info.Hash != null) {
//                cols.Add("Hash");
//                vals.Add(info.Hash);
//            }

//            string assignmentStr = "";
//            for (int i = 0; i < cols.Count; i++) {
//                assignmentStr += cols[i] + " = ?";
//                if (i + 1 < cols.Count) {
//                    assignmentStr += ", ";
//                }
//            }

//            vals.Add(info.ID);

//            bool result = db.Execute($"UPDATE Files SET {assignmentStr} WHERE ID = ?", vals.ToArray()) == 1;

//            if (result) {
//                var fileResult = db.Table<FileMetadata>().Where(t => t.ID == info.ID)
//                                    .Join(db.Table<FilePath>(), fm => fm.PathID, p => p.ID, (fm, p) => new { FileMetadata = fm, Path = p.PathString })
//                                    .ToList()[0];
//                string fullname = fileResult.Path + "\\" + fileResult.FileMetadata.Filename;
//                db.Execute($"UPDATE Files SET Fullname = ? WHERE ID = ?", fullname, info.ID);
//                logger.LogInformation("Updated file metadata with the following data: \n" + info.ToString());
//            } else {
//                logger.LogWarning("Could not update with the following information: \n" + info.ToString());
//            }

//            return result;
//        }

//        /* End File Section */

//        /* Tag Section */

//        /// <summary>
//        ///     Adds a tag to a file. Can optionally add a tag category to the tag.
//        /// </summary>
//        /// <param name="fileID"></param>
//        /// <param name="tag"></param>
//        /// <param name="tagCategory"></param>
//        /// <returns>Add tag status. Will return false if it already exists.</returns>
//        public bool AddTagToFile(int fileID, string tag, string tagCategory = "")
//        {
//            bool result;

//            result = db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"Files\" WHERE \"ID\" = ?", fileID) == 1;
//            if (!result) {
//                logger.LogWarning(fileID + " does not exist in DB, cannot add tag");
//                return result;
//            }

//            AddTag(tag, tagCategory);

//            int tagID = db.ExecuteScalar<int>("SELECT \"ID\" FROM \"Tags\" WHERE \"Name\" = ?", tag.ToLowerInvariant());
//            result = db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"FileTagAssociations\" " +
//                "WHERE \"FileID\" = ? AND \"TagID\" = ?", fileID, tagID) == 0;

//            if (result) {
//                var association = new FileTagAssociation
//                {
//                    FileID = fileID,
//                    TagID = tagID
//                };
//                result = db.Insert(association) == 1;
//                if (result) logger.LogInformation($"Tag {tag.ToLowerInvariant()} added to file #{fileID}");
//            }

//            return result;
//        }

//        /// <summary>
//        ///     Adds a tag category to the DB
//        /// </summary>
//        /// <param name="tagCategory"></param>
//        /// <returns>Add tag category status. Will return false if it already exists.</returns>
//        public bool AddTagCategory(string tagCategory)
//        {
//            bool result = false;

//            if (tagCategory.Length > 0) {
//                tagCategory = tagCategory.ToLowerInvariant();
//                var category = new TagCategory
//                {
//                    Name = tagCategory
//                };
//                int added = db.Insert(category);
//                if (added == 0) logger.LogInformation("Added new tag category: " + tagCategory);
//            }

//            return result;
//        }

//        /// <summary>
//        ///     Adds a tag. Can also add an optional category to the tag.
//        /// </summary>
//        /// <param name="tag"></param>
//        /// <param name="tagCategory"></param>
//        /// <returns>Status of adding new tag. Will return if it already exists</returns>
//        public bool AddTag(string tag, string tagCategory = "")
//        {
//            bool result;

//            AddTagCategory(tagCategory);

//            int categoryID = db.ExecuteScalar<int>("SELECT \"ID\" FROM \"TagCategories\" WHERE \"Name\" = ?", tagCategory);
//            logger.LogDebug($"Found ID {categoryID} for {tagCategory}");

//            var tagObj = new Tag
//            {
//                Name = tag.ToLowerInvariant(),
//                CategoryID = categoryID
//            };
//            result = db.Insert(tagObj) == 1;
//            logger.LogInformation(tag + " was" + (result ? " " : " not ") + "added to Tags table");

//            return result;
//        }

//        /* End Tag Section */

//        /* Collection Section*/

//        /// <summary>
//        ///     Adds a file collection. Items will be ordered in the provided sequence.
//        ///     If a collection with the same name already exists, add will fail.
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="fileSequence"></param>
//        /// <returns>Status of adding the new collection.</returns>
//        public bool AddCollection(string name, IEnumerable<int> fileSequence)
//        {
//            bool result;

//            if (name.Length == 0) return false;

//            var collection = new FileCollection
//            {
//                Name = name
//            };
//            result = db.Insert(collection) == 1;
//            logger.LogInformation(name + " was" + (result ? " " : " not ") + "added to Collections table");

//            if (result) {
//                int collectionID = db.ExecuteScalar<int>("SELECT \"ID\" FROM \"Collections\" WHERE \"Name\" = ?", name);
//                logger.LogDebug($"Found ID {collectionID} for {name}");
                
//                int index = 0;
                
//                foreach (int fileID in fileSequence) {
//                    var fileCollectionAssociation = new FileCollectionAssociation
//                    {
//                        CollectionID = collectionID,
//                        FileID = fileID,
//                        Position = index
//                    };
//                    bool insertResult = db.Insert(fileCollectionAssociation) == 1;
//                    logger.LogInformation($"File {fileID} in collection {name} was" + (result ? " " : " not ") 
//                        + "added to FileCollectionAssociations table");
//                    result = result && insertResult;
//                }
//            }
            

//            return result;
//        }

//        /// <summary>
//        ///     Adds a file to a collection. Can only add to the end of the collection.
//        /// </summary>
//        /// <param name="collectionID"></param>
//        /// <param name="fileID"></param>
//        /// <returns>Status of adding file to new collection.</returns>
//        public bool AddFileToCollection(int collectionID, int fileID)
//        {
//            bool result;

//            int files = db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"Files\" WHERE \"ID\" = ?", fileID);
//            int collections = db.ExecuteScalar<int>("SELECT COUNT(*) FROM \"Collections\" WHERE \"ID\" = ?", collectionID);
//            if (files == 0) {
//                logger.LogWarning("File with ID of" + fileID + " was not found, could not add to collection");
//                return false;
//            }
//            if (collections == 0) {
//                logger.LogWarning("Collection with ID of" + fileID + " was not found, could not add to collection");
//                return false;
//            }

//            var positionList = db.ExecuteScalar<int>("SELECT MAX(Position) FROM FileCollectionAssociations " +
//                "WHERE FileID = ? AND CollectionID = ?", fileID, collectionID);

//            int idx = 0;
//            if (positionList > 0) {
//                idx += positionList + 1;
//            }

//            var fileCollectionAssociation = new FileCollectionAssociation
//            {
//                FileID = fileID,
//                CollectionID = collectionID,
//                Position = idx
//            };
//            result = db.Insert(fileCollectionAssociation) == 1;
//            logger.LogInformation($"File {fileID} in collection {collectionID} was" + (result ? " " : " not ")
//                        + "added to FileCollectionAssociations table");

//            return result;
//        }

//        /* End Collection Section */
//    }
//}
