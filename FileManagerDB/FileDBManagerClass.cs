using System;
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
                string fixArg;
                if (arg.GetType() == typeof(int) || 
                    arg.GetType() == typeof(float) || 
                    arg.GetType() == typeof(long) ||
                    arg.GetType() == typeof(double)) {
                    fixArg = arg.ToString();
                } else {
                    fixArg = "'" + arg.ToString().Replace("'", "''") + "'";
                }
                filledQuery += fixArg + queryParts[count + 1];
                count++;
            }

            logger.LogDebug("Created query: " + filledQuery);

            return filledQuery;
        }

        private int ExecuteNonQuery(string query)
        {
            var com = new SQLiteCommand(query, db);
            int result = com.ExecuteNonQuery();
            com.Dispose();
            return result;
        }

        private void BuildWhereArrays(FileSearchFilter filter, ref List<string> wheres, ref List<object> whereValues)
        {
            if (filter.UsingID) {
                wheres.Add("Files.ID = ? ");
                whereValues.Add(filter.ID);
            }
            if (filter.UsingPathID) {
                wheres.Add("PathID = ? ");
                whereValues.Add(filter.PathID);
            }
            if (filter.UsingFilename) {
                if (filter.FilenameFilterExact) {
                    wheres.Add("Filename = ?");
                    whereValues.Add(filter.Filename);
                } else {
                    wheres.Add("Filename LIKE ?");
                    whereValues.Add("%" + filter.Filename + "%");
                }
            }
            if (filter.UsingFullname) {
                if (filter.FullnameFilterExact) {
                    wheres.Add("Path || '\\' || Filename = ?");
                    whereValues.Add(filter.Fullname);
                } else {
                    wheres.Add("Path || '\\' || Filename LIKE ?");
                    whereValues.Add("%" + filter.Fullname + "%");
                }
            }
            if (filter.UsingAltname) {
                if (filter.AltnameFilterExact) {
                    wheres.Add("Altname = ?");
                    whereValues.Add(filter.Altname);
                } else {
                    wheres.Add("Altname LIKE ?");
                    whereValues.Add("%" + filter.Altname + "%");
                }
            }
            if (filter.UsingFileTypeID) {
                wheres.Add("FileTypeID = ?");
                whereValues.Add(filter.FileTypeID);
            }
            if (filter.UsingHash) {
                if (filter.hashFilterExact) {
                    wheres.Add("Hash = ?");
                    whereValues.Add(filter.Hash);
                } else {
                    wheres.Add("Hash LIKE ?");
                    whereValues.Add("%" + filter.Hash + "%");
                }
            }
            if (filter.UsingPath) {
                if (filter.PathFilterExact) {
                    wheres.Add("Path = ?");
                    whereValues.Add(filter.Path);
                } else {
                    wheres.Add("Path LIKE ?");
                    whereValues.Add("%" + filter.Path + "%");
                }
            }
            if (filter.UsingFileType) {
                if (filter.FileTypeFilterExact) {
                    wheres.Add("Name = ?");
                    whereValues.Add(filter.FileType);
                } else {
                    wheres.Add("Name LIKE ?");
                    whereValues.Add("%" + filter.FileType + "%");
                }
            }
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
            ExecuteNonQuery(query);
        }

        /* File Section */

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
            int num = ExecuteNonQuery(queryString);

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
            num = ExecuteNonQuery(queryString);
            if (num == 0) {
                logger.LogInformation(path + " already exists in FilePath table");
            } else {
                logger.LogInformation(path + " added to FilePath table");
            }

            queryString = createQueryString("SELECT ID FROM FilePaths WHERE Path = ?", path);
            var com = new SQLiteCommand(queryString, db);
            var reader = com.ExecuteReader();
            reader.Read();
            int pathID = reader.GetInt32(0);
            reader.Close();
            com.Dispose();
            logger.LogDebug($"Found ID {pathID} for {path}");
            queryString = createQueryString("SELECT ID FROM FileTypes WHERE Name = ?",
                            filetype.ToLowerInvariant());
            com = new SQLiteCommand(queryString, db);
            reader = com.ExecuteReader();
            reader.Read();
            int typeID = reader.GetInt32(0);
            logger.LogDebug($"Found ID {typeID} for {filetype}");
            reader.Close();
            com.Dispose();

            queryString = createQueryString("INSERT INTO Files " +
                            "(Filename, PathID, FileTypeID, Altname, Hash) VALUES (?,?,?,?,?)",
                            filename, pathID, typeID, altname, hash);
            logger.LogDebug("Insert Query: " + queryString);
            result = ExecuteNonQuery(queryString) == 1;
            logger.LogInformation($"{filepath} was" + (result ? " " : " not ") + "added to Files table");

            return result;
        }

        /// <summary>
        ///     Returns full metadata of a file from a file ID.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns>
        ///     Dynamic type with the following format: <br/>
        ///         <list type="bullet">
        ///         <item>
        ///             FileMetadata: <br/>
        ///             <list type="bullet">
        ///                 <item>
        ///                     FileMetadata: <br/>
        ///                         <list type="bullet">
        ///                             FileMeta columns... <br/>
        ///                         </list>
        ///                 </item>
        ///                 <item>
        ///                     FileType: <br/>
        ///                     <list type="bullet">
        ///                         ID, Name <br/>
        ///                     </list>
        ///                 </item>
        ///             </list>
        ///         </item>
        ///         <item>
        ///             Path: <br/>
        ///                 ID, Path <br/>
        ///         </item>
        ///         </list>
        /// </returns>
        public dynamic GetFileMetadata(int fileID)
        {
            string queryString = "SELECT * FROM Files " 
                            + "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID "
                            + "JOIN FilePaths ON Files.PathID = FilePaths.ID "
                            + $" WHERE Files.ID = {fileID}";
            logger.LogDebug("Query: " + queryString);
            var com = new SQLiteCommand(queryString, db).ExecuteReader();
            List<dynamic> results = new List<dynamic>(); 
            if (com.Read()) {
                results.Add(new { 
                    ID = fileID,
                    PathID = com.GetInt32(com.GetOrdinal("PathID")),
                    Path = com.GetString(com.GetOrdinal("Path")),
                    Filename = com.GetString(com.GetOrdinal("Filename")),
                    Altname = com.GetString(com.GetOrdinal("Altname")),
                    FileTypeID = com.GetInt32(com.GetOrdinal("FileTypeID")),
                    FileType = com.GetString(com.GetOrdinal("Name")),
                    Hash = com.GetString(com.GetOrdinal("Hash"))
                });
            }

            com.Close();

            if (results.Count == 0) {
                logger.LogInformation("No file found with ID of " + fileID);
                return null;
            }
            logger.LogInformation("File found with ID of " + fileID);

            return results[0];
        }

        /// <summary>
        ///     Gets all metadata for all files.
        /// </summary>
        /// <returns>
        /// Dynamic type with the following format: <br/>
        ///         <list type="bullet">
        ///         <item>
        ///             FileMetadata: <br/>
        ///             <list type="bullet">
        ///                 <item>
        ///                     FileMetadata: <br/>
        ///                         <list type="bullet">
        ///                             FileMeta columns... <br/>
        ///                         </list>
        ///                 </item>
        ///                 <item>
        ///                     FileType: <br/>
        ///                     <list type="bullet">
        ///                         ID, Name <br/>
        ///                     </list>
        ///                 </item>
        ///             </list>
        ///         </item>
        ///         <item>
        ///             Path: <br/>
        ///                 ID, Path <br/>
        ///         </item>
        ///         </list>
        /// </returns>
        public List<GetFileMetadataType> GetAllFileMetadata()
        {
            string queryString = $"SELECT * FROM Files "
                            + "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID "
                            + "JOIN FilePaths ON Files.PathID = FilePaths.ID";
            var com = new SQLiteCommand(queryString, db).ExecuteReader();
            List<GetFileMetadataType> results = new List<GetFileMetadataType>();
            while (com.HasRows && com.Read()) {
                results.Add(new GetFileMetadataType()
                {
                    ID = com.GetInt32(com.GetOrdinal("ID")),
                    PathID = com.GetInt32(com.GetOrdinal("PathID")),
                    Path = com.GetString(com.GetOrdinal("Path")),
                    Filename = com.GetString(com.GetOrdinal("Filename")),
                    Altname = com.GetString(com.GetOrdinal("Altname")),
                    FileTypeID = com.GetInt32(com.GetOrdinal("FileTypeID")),
                    FileType = com.GetString(com.GetOrdinal("Name")),
                    Hash = com.GetString(com.GetOrdinal("Hash"))
                });
            }

            com.Close();

            if (results.Count == 0) {
                logger.LogInformation("No files found");
                logger.LogDebug("Could not find any files with query of " + queryString);
                return results;
            }

            return results;
        }

        /// <summary>
        ///     Applies standard match or equality filters to file metadata search.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>
        /// Dynamic type with the following format: <br/>
        ///         <list type="bullet">
        ///         <item>
        ///             FileMetadata: <br/>
        ///             <list type="bullet">
        ///                 <item>
        ///                     FileMetadata: <br/>
        ///                         <list type="bullet">
        ///                             FileMeta columns... <br/>
        ///                         </list>
        ///                 </item>
        ///                 <item>
        ///                     FileType: <br/>
        ///                     <list type="bullet">
        ///                         ID, Name <br/>
        ///                     </list>
        ///                 </item>
        ///             </list>
        ///         </item>
        ///         <item>
        ///             Path: <br/>
        ///                 ID, Path <br/>
        ///         </item>
        ///         </list>
        /// </returns>
        public List<GetFileMetadataType> GetFileMetadataFiltered(FileSearchFilter filter)
        {
            string queryStringPart1 = "SELECT * FROM Files ";
            string queryStringPart2 = "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID ";
            string queryStringPart3 = "JOIN FilePaths ON Files.PathID = FilePaths.ID ";

            List<string> wheres = new List<string>();
            List<object> whereValues = new List<object>();
            logger.LogInformation("Querying with file metadata with filters.");

            BuildWhereArrays(filter, ref wheres, ref whereValues);

            string query = queryStringPart1 + queryStringPart2 + queryStringPart3;
            for (int i = 0; i < wheres.Count; i++) {
                if (i == 0) {
                    query += "WHERE ";
                } else {
                    query += " AND ";
                }

                query += wheres[i];
            }

            query = createQueryString(query, whereValues.ToArray());
            logger.LogDebug("Using filtered query: " + query);

            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            var results = new List<GetFileMetadataType>();
            while (read.HasRows && read.Read()) {
                results.Add(new GetFileMetadataType()
                {
                    ID = read.GetInt32(read.GetOrdinal("ID")),
                    PathID = read.GetInt32(read.GetOrdinal("PathID")),
                    Path = read.GetString(read.GetOrdinal("Path")),
                    Filename = read.GetString(read.GetOrdinal("Filename")),
                    Altname = read.GetString(read.GetOrdinal("Altname")),
                    FileTypeID = read.GetInt32(read.GetOrdinal("FileTypeID")),
                    FileType = read.GetString(read.GetOrdinal("Name")),
                    Hash = read.GetString(read.GetOrdinal("Hash"))
                });
            }
            read.Close();
            com.Dispose();
            logger.LogInformation($"Returning {results.Count} result(s)");

            return results;
        }

        /// <summary>
        ///     Deletes a file and all dependendent entities.
        /// </summary>
        /// <param name="id">The file ID</param>
        /// <returns>Result of deletion</returns>
        public bool DeleteFileMetadata(int id)
        {
            bool result;

            logger.LogInformation("Delete file metadata with id of " + id);
            string query = $"DELETE FROM Files WHERE ID = {id}";
            logger.LogDebug("Delete Query: " + query);

            result = ExecuteNonQuery(query) == 1;

            if (result) {
                //db.Execute("DELETE FROM FileCollectionAssociations WHERE FileID = ?", id);
                //db.Execute("DELETE FROM FileTagAssociations WHERE FileID = ?", id);
                logger.LogInformation("File metadata and dependencies removed");
            } else {
                logger.LogWarning("Could not delete file metadata with id of " + id);
            }

            return result;
        }

        /// <summary>
        ///     Updates a file's metadata. The info object must 
        ///     have the file ID.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>Status of update</returns>
        public bool UpdateFileMetadata(FileSearchFilter newInfo, FileSearchFilter filter)
        {
            List<string> cols = new List<string>();
            List<object> vals = new List<object>();
            List<string> wheres = new List<string>();
            List<object> whereValues = new List<object>();
            string query;

            if (newInfo.IsEmpty || filter.IsEmpty) {
                if (filter.IsEmpty) logger.LogInformation("Filter is empty");
                if (newInfo.IsEmpty) logger.LogInformation("Update information is empty");
                return false;
            }

            if (newInfo.UsingPath) {
                query = createQueryString("INSERT OR IGNORE INTO Filepaths (Path) Values (?)", newInfo.Path);
                ExecuteNonQuery(query);
                query = createQueryString("SELECT ID FROM Filepaths WHERE Path = ?", newInfo.Path);
                var com = new SQLiteCommand(query, db);
                var read = com.ExecuteReader();
                int results = -1;
                if (read.HasRows) {
                    read.Read();
                    results = read.GetInt32(0);
                    read.Close();
                    com.Dispose();
                } else {
                    read.Close();
                    com.Dispose();
                    throw new InvalidDataException("Path could not be found or inserted");
                }
                cols.Add("PathID");
                vals.Add(results);
            }

            if (newInfo.UsingFileType) {
                query = createQueryString("INSERT OR IGNORE INTO Filetypes (Name) Values (?)", newInfo.FileType);
                ExecuteNonQuery(query);
                query = createQueryString("SELECT ID FROM FileTypes WHERE Name = ?", newInfo.FileType);
                var com = new SQLiteCommand(query, db);
                var read = com.ExecuteReader();
                int results;
                if (read.HasRows) {
                    read.Read();
                    results = read.GetInt32(0);
                    read.Close();
                    com.Dispose();
                } else {
                    read.Close();
                    com.Dispose();
                    throw new InvalidDataException("Path could not be found or inserted");
                }
                cols.Add("FileTypeID");
                vals.Add(results);
            }
            if (newInfo.UsingFilename) {
                cols.Add("Filename");
                vals.Add(newInfo.Filename);
            }
            if (newInfo.UsingAltname) {
                cols.Add("Altname");
                vals.Add(newInfo.Altname);
            }
            if (newInfo.UsingHash) {
                cols.Add("hash");
                vals.Add(newInfo.Hash);
            }

            string assignmentstr = "";
            for (int i = 0; i < cols.Count; i++) {
                assignmentstr += cols[i] + " = ?";
                if (i + 1 < cols.Count) {
                    assignmentstr += ", ";
                }
            }

            BuildWhereArrays(filter, ref wheres, ref whereValues);
            query = createQueryString($"UPDATE Files SET {assignmentstr} ", vals.ToArray());

            for (int i = 0; i < wheres.Count; i++) {
                if (i == 0) {
                    query += "WHERE ";
                } else {
                    query += " AND ";
                }

                query += wheres[i];
            }

            query = createQueryString(query, whereValues.ToArray());
            bool result = ExecuteNonQuery(query) == 1;

            if (result) {
                logger.LogInformation("Updated file metadata with the following data: \n" + newInfo);
            } else {
                logger.LogWarning("Could not update with the following information: \n" + newInfo);
            }

            return result;
        }

        /* End File Section */

        /* Tag Section */

        /// <summary>
        ///     Adds a tag category to the DB
        /// </summary>
        /// <param name="tagCategory"></param>
        /// <returns>Add tag category status. Will return false if it already exists.</returns>
        public bool AddTagCategory(string tagCategory)
        {
            bool result = false;

            if (tagCategory != null && tagCategory.Length > 0) {
                tagCategory = tagCategory.ToLowerInvariant();
                string query = createQueryString("INSERT INTO TagCategories (Name) VALUES (?)", tagCategory);
                int added = ExecuteNonQuery(query);
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

            if (tag is null || tag.Length == 0) {
                logger.LogInformation("Failure adding tag, no tag name provided");
                return false;
            }

            if (tagCategory is null) {
                logger.LogInformation("Failure adding tag, tag category was null");
                return false;
            }

            AddTagCategory(tagCategory);

            string query = createQueryString("SELECT ID FROM TagCategories WHERE Name = ?", tagCategory);
            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            int categoryID;
            if (read.Read()) {
                categoryID = read.GetInt32(0);
                read.Close();
                com.Dispose();
            } else {
                read.Close();
                com.Dispose();
                throw new InvalidDataException("Error accessing tag category table");
            }
            
            logger.LogDebug($"Found ID {categoryID} for {tagCategory}");

            query = createQueryString("INSERT OR IGNORE INTO Tags (Name, CategoryID) VALUES (?, ?)",
                    tag.ToLowerInvariant(),
                    categoryID
                );
            result = ExecuteNonQuery(query) == 1;
            logger.LogInformation(tag + " was" + (result ? " " : " not ") + "added to Tags table");

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

            string query = createQueryString("SELECT COUNT(*) FROM Files WHERE ID = ?", fileID);
            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            read.Read();
            result = read.GetInt32(0) == 1;
            read.Close();
            com.Dispose();
            if (!result) {
                logger.LogWarning(fileID + " does not exist in DB, cannot add tag");
                return result;
            }

            AddTag(tag, tagCategory);

            query = createQueryString("SELECT ID FROM Tags WHERE Name = ?", tag.ToLowerInvariant());
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            read.Read();
            int tagID = read.GetInt32(0);
            read.Close();
            com.Dispose();

            query = createQueryString("SELECT COUNT(*) FROM FileTagAssociations " +
                "WHERE FileID = ? AND TagID = ?", fileID, tagID);
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            read.Read();
            result = read.GetInt32(0) == 0;
            read.Close();
            com.Dispose();

            if (result) {
                query = createQueryString("INSERT OR IGNORE INTO FileTagAssociations (FileID, TagID) VALUES (?, ?)",
                    fileID, tagID);
                result = ExecuteNonQuery(query) == 1;
                if (result) logger.LogInformation($"Tag {tag.ToLowerInvariant()} added to file #{fileID}");
            }

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
