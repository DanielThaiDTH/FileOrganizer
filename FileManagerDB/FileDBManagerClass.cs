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
            db = new SQLiteConnection("DataSource=" + dbLoc);
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
            ExecuteNonQuery("PRAGMA foreign_keys=ON");
        }

        private string createStatementString(string statment, params object[] args)
        {
            string filledQuery = "";
            if (statment.Count(c => c == '?') != args.Length) {
                logger.LogWarning("Argument mismatch - query: '" + statment + $"' does not have {args.Length} params");
                return null;
            }
            var queryParts = statment.Split('?');
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
            string statement = createStatementString("INSERT INTO FileTypes (Name) VALUES (?)", filetype);
            int num = ExecuteNonQuery(statement);

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

            statement = createStatementString("INSERT INTO FilePaths (Path) VALUES (?)", path);
            num = ExecuteNonQuery(statement);
            if (num == 0) {
                logger.LogInformation(path + " already exists in FilePath table");
            } else {
                logger.LogInformation(path + " added to FilePath table");
            }

            statement = createStatementString("SELECT ID FROM FilePaths WHERE Path = ?", path);
            var com = new SQLiteCommand(statement, db);
            var reader = com.ExecuteReader();
            reader.Read();
            int pathID = reader.GetInt32(0);
            reader.Close();
            com.Dispose();
            logger.LogDebug($"Found ID {pathID} for {path}");
            statement = createStatementString("SELECT ID FROM FileTypes WHERE Name = ?",
                            filetype.ToLowerInvariant());
            com = new SQLiteCommand(statement, db);
            reader = com.ExecuteReader();
            reader.Read();
            int typeID = reader.GetInt32(0);
            logger.LogDebug($"Found ID {typeID} for {filetype}");
            reader.Close();
            com.Dispose();

            statement = createStatementString("INSERT INTO Files " +
                            "(Filename, PathID, FileTypeID, Altname, Hash) VALUES (?,?,?,?,?)",
                            filename, pathID, typeID, altname, hash);
            logger.LogDebug("Insert Query: " + statement);
            result = ExecuteNonQuery(statement) == 1;
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
            string statement = "SELECT * FROM Files "
                            + "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID "
                            + "JOIN FilePaths ON Files.PathID = FilePaths.ID "
                            + $" WHERE Files.ID = {fileID}";
            logger.LogDebug("Query: " + statement);
            var com = new SQLiteCommand(statement, db).ExecuteReader();
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
            string statement = $"SELECT * FROM Files "
                            + "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID "
                            + "JOIN FilePaths ON Files.PathID = FilePaths.ID";
            var com = new SQLiteCommand(statement, db).ExecuteReader();
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
                logger.LogDebug("Could not find any files with query of " + statement);
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

            query = createStatementString(query, whereValues.ToArray());
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
                query = createStatementString("INSERT OR IGNORE INTO Filepaths (Path) Values (?)", newInfo.Path);
                ExecuteNonQuery(query);
                query = createStatementString("SELECT ID FROM Filepaths WHERE Path = ?", newInfo.Path);
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
                query = createStatementString("INSERT OR IGNORE INTO Filetypes (Name) Values (?)", newInfo.FileType);
                ExecuteNonQuery(query);
                query = createStatementString("SELECT ID FROM FileTypes WHERE Name = ?", newInfo.FileType);
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
            query = createStatementString($"UPDATE Files SET {assignmentstr} ", vals.ToArray());

            for (int i = 0; i < wheres.Count; i++) {
                if (i == 0) {
                    query += "WHERE ";
                } else {
                    query += " AND ";
                }

                query += wheres[i];
            }

            query = createStatementString(query, whereValues.ToArray());
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
                string query = createStatementString("INSERT INTO TagCategories (Name) VALUES (?)", tagCategory);
                logger.LogDebug("Using query: " + query);
                int added = ExecuteNonQuery(query);
                if (added == 1) {
                    logger.LogInformation("Added new tag category: " + tagCategory);
                    result = true;
                } else {
                    logger.LogInformation(tagCategory + " already exists, not adding");
                }
            }

            return result;
        }

        public List<GetTagCategoryType> GetAllTagCategories()
        {
            List<GetTagCategoryType> categories = new List<GetTagCategoryType>();

            string query = "SELECT * FROM TagCategories";
            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            while (read.HasRows && read.Read()) {
                categories.Add(new GetTagCategoryType()
                {
                    ID = read.GetInt32(read.GetOrdinal("ID")),
                    Name = read.GetString(read.GetOrdinal("Name"))
                });
            }

            logger.LogInformation($"Found {categories.Count} tag categories");
            return categories;
        }

        /// <summary>
        ///     Removes a tag category. Will set the category column value to null 
        ///     for previously associated tags.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteTagCategory(int id)
        {
            bool result;

            string query = createStatementString("DELETE FROM TagCategories WHERE ID = ?", id);
            logger.LogInformation("Deleting tag category with ID of " + id);

            result = ExecuteNonQuery(query) == 1;

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

            string query = createStatementString("SELECT ID FROM TagCategories WHERE Name = ?", tagCategory.ToLowerInvariant());
            logger.LogDebug("Searching tag category using: " + query);
            var com = new SQLiteCommand(query, db);
            int categoryID = -1;
            if (tagCategory.Length > 0) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    categoryID = read.GetInt32(0);
                    read.Close();
                } else {
                    read.Close();
                    com.Dispose();
                    logger.LogWarning("Failed to get ID for " + tagCategory);
                    throw new InvalidDataException("Error accessing tag category table");
                }
            }
            com.Dispose();

            logger.LogDebug($"Found ID {categoryID} for {tagCategory}");
            if (categoryID > -1) {
                query = createStatementString("INSERT OR IGNORE INTO Tags (Name, CategoryID) VALUES (?, ?)",
                        tag,
                        categoryID
                    );
            } else {
                query = createStatementString("INSERT OR IGNORE INTO Tags (Name, CategoryID) VALUES (?, NULL)",
                        tag
                    );
            }
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
        /// <returns>Add tag status. Will return false if it's already attached to the file.</returns>
        public bool AddTagToFile(int fileID, string tag, string tagCategory = "")
        {
            bool result;

            string query = createStatementString("SELECT COUNT(*) FROM Files WHERE ID = ?", fileID);
            logger.LogDebug("Querying file existence using: " + query);
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

            query = createStatementString("SELECT ID FROM Tags WHERE Name = ?", tag.ToLowerInvariant());
            logger.LogDebug("Querying tag id using: " + query);
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            read.Read();
            int tagID = read.GetInt32(0);
            read.Close();
            com.Dispose();

            query = createStatementString("SELECT COUNT(*) FROM FileTagAssociations " +
                "WHERE FileID = ? AND TagID = ?", fileID, tagID);
            logger.LogDebug("Checking file tag association existence using: " + query);
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            read.Read();
            result = read.GetInt32(0) == 0;
            read.Close();
            com.Dispose();

            if (result) {
                query = createStatementString("INSERT OR IGNORE INTO FileTagAssociations (FileID, TagID) VALUES (?, ?)",
                    fileID, tagID);
                result = ExecuteNonQuery(query) == 1;
                if (result) logger.LogInformation($"Tag {tag.ToLowerInvariant()} added to file #{fileID}");
            }

            return result;
        }

        /// <summary>
        ///     Gets all tags. If no tag category, the category ID will be -1 and the 
        ///     category column will be null
        /// </summary>
        /// <returns>Has the following columns [ID, Name, CategoryID, Category]</returns>
        public List<GetTagType> GetAllTags()
        {
            List<GetTagType> tags = new List<GetTagType>();
            logger.LogInformation("Getting all tags");

            string query = "SELECT * FROM Tags LEFT JOIN TagCategories ON CategoryID = TagCategories.ID";
            logger.LogDebug("Using query: " + query);
            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            while (read.HasRows && read.Read()) {
                var newTag = new GetTagType()
                {
                    ID = read.GetInt32(read.GetOrdinal("ID")),
                    Name = read.GetString(read.GetOrdinal("Name"))
                };

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("CategoryID")))) {
                    newTag.CategoryID = -1;
                    newTag.Category = null;
                    logger.LogDebug("No catagory for tag " + newTag.Name);
                } else {
                    newTag.CategoryID = read.GetInt32(read.GetOrdinal("CategoryID"));
                    newTag.Category = read.GetString(4);
                }
                tags.Add(newTag);
            }
            read.Close();
            com.Dispose();
            logger.LogInformation($"Found {tags.Count} tags");

            return tags;
        }

        public List<GetTagType> GetTagsForFile(int fileID)
        {
            List<GetTagType> fileTags = new List<GetTagType>();

            string query = createStatementString("SELECT * FROM FileTagAssociations JOIN Tags " +
                "ON FileTagAssociations.TagID = Tags.ID " +
                "LEFT JOIN TagCategories ON CategoryID = TagCategories.ID " +
                "WHERE FileTagAssociations.FileID = ?", fileID);

            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();

            while (read.HasRows && read.Read()) {
                var newTag = new GetTagType()
                {
                    ID = read.GetInt32(read.GetOrdinal("TagID")),
                    Name = read.GetString(read.GetOrdinal("Name"))
                };

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("CategoryID")))) {
                    newTag.CategoryID = -1;
                    newTag.Category = null;
                    logger.LogDebug("No catagory for tag " + newTag.Name);
                } else {
                    newTag.CategoryID = read.GetInt32(read.GetOrdinal("CategoryID"));
                    newTag.Category = read.GetString(6);
                }
                fileTags.Add(newTag);
            }

            logger.LogInformation($"Found {fileTags.Count} tags for file {fileID}");
            return fileTags;
        }

        /// <summary>
        ///     Deletes a tag. Will also remove that tag from all
        ///     files that have it.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteTag(int id)
        {
            bool result;

            string query = createStatementString("DELETE FROM Tags WHERE ID = ?", id);
            logger.LogInformation("Deleting tag with ID of " + id);

            result = ExecuteNonQuery(query) == 1;

            return result;
        }

        /* End Tag Section */

        /* Collection Section */

        /// <summary>
        ///     Adds a file collection. Items will be ordered in the provided sequence.
        ///     If a collection with the same name already exists, add will fail. If no 
        ///     sequence is provided or is null, an empty List is used.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileSequence"></param>
        /// <returns>Status of adding the new collection.</returns>
        /// <exception cref="SQLiteException">
        ///     When file sequence contains file id not in database or less than 1.
        /// </exception>
        public bool AddCollection(string name, IEnumerable<int> fileSequence = null)
        {
            bool result;

            if (string.IsNullOrEmpty(name)) {
                logger.LogWarning("Collection name is empty or null");
                return false;
            }

            if (fileSequence is null) {
                fileSequence = new List<int>();
            }

            var transaction = db.BeginTransaction();
            string statment = createStatementString("INSERT INTO Collections (Name) VALUES (?)", name);
            logger.LogDebug("Creating collection with query: " + statment);
            result = ExecuteNonQuery(statment) == 1;
            logger.LogInformation(name + " was" + (result ? " " : " not ") + "added to Collections table");

            if (result) {
                statment = createStatementString("SELECT ID FROM Collections WHERE Name = ?", name);
                var com = new SQLiteCommand(statment, db);
                var read = com.ExecuteReader();
                read.Read();
                int collectionID = read.GetInt32(0);
                read.Close();
                com.Dispose();
                logger.LogDebug($"Found ID {collectionID} for {name}");

                int index = 0;

                foreach (int fileID in fileSequence) {
                    statment = createStatementString("INSERT INTO FileCollectionAssociations (CollectionID,FileID,Position) " +
                        "VALUES (?,?,?)", collectionID, fileID, index + 1);
                    logger.LogDebug("Creating collection with query: " + statment);
                    bool insertResult = false;
                    
                    try {
                        insertResult = ExecuteNonQuery(statment) == 1;
                    } catch (SQLiteException ex) {
                        logger.LogWarning(ex, "Failed to insert file to collection");
                        transaction.Rollback();
                        transaction.Dispose();
                        throw ex;
                    }

                    logger.LogInformation($"File {fileID} in collection {name} was" + (insertResult ? " " : " not ")
                        + "added to FileCollectionAssociations table");
                    result = result && insertResult;
                    if (!result) {
                        logger.LogWarning($"Failure adding file {fileID} to collection {name}");
                        break;
                    }
                    index++;
                }
            }

            if (result) {
                transaction.Commit();
            } else {
                transaction.Rollback();
                logger.LogWarning("Failed adding files to collection, rolling back");
            }
            transaction.Dispose();

            return result;
        }

        /// <summary>
        ///     Adds a file to a collection. Defaults to adding to the 
        ///     end of a collection. Adding in the beginning or in the middle
        ///     will result in files coming after shifting forward by 1.
        /// </summary>
        /// <param name="collectionID"></param>
        /// <param name="fileID"></param>
        /// <param name="insertIndex"></param>
        /// <returns>Status of adding file to new collection.</returns>
        /// <exception cref="SQLiteException">
        ///     File id or collection id not in database or less than 1.
        /// </exception>
        public bool AddFileToCollection(int collectionID, int fileID, int insertIndex = -1)
        {
            bool result;
            
            string query = createStatementString("SELECT COUNT(*) FROM Files WHERE ID = ?", fileID);
            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();
            int files = 0;
            if (read.HasRows && read.Read()) {
                files = read.GetInt32(0);
            }
            read.Close();
            com.Dispose();
            if (files == 0) {
                logger.LogWarning($"File with ID of {fileID} was not found, " +
                    $"could not add to collection {collectionID}");
                return false;
            }

            query = createStatementString("SELECT COUNT(*) FROM Collections WHERE ID = ?", collectionID);
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            int collections = 0;
            if (read.HasRows && read.Read()) {
                collections = read.GetInt32(0);
            }
            read.Close();
            com.Dispose();
            if (collections == 0) {
                logger.LogWarning("Collection with ID of " + collectionID + " was not found, could not add to collection");
                return false;
            }

            query = createStatementString("SELECT MAX(Position) FROM FileCollectionAssociations " +
                "WHERE CollectionID = ?", collectionID);
            com = new SQLiteCommand(query, db);
            read = com.ExecuteReader();
            int maxPosition = -1;

            if (read.HasRows && read.Read() && !DBNull.Value.Equals(read.GetValue(0))) {
                maxPosition = read.GetInt32(0);
            }
            read.Close();
            com.Dispose();

            int idx = 0;
            if (insertIndex == -1 || insertIndex > maxPosition) {
                if (maxPosition > 0) {
                    idx += maxPosition + 1;
                } else {
                    //logger.LogWarning($"Could not find position for file {fileID} in collection {collectionID}");
                    //return false;
                    idx = 1;
                }
            } else if (insertIndex <= maxPosition && insertIndex > 0) {
                var transaction = db.BeginTransaction();
                result = true;
                for (int i = maxPosition; i >= insertIndex; i--) {
                    query = createStatementString("UPDATE FileCollectionAssociations SET Position = ? " +
                        "WHERE Position = ?", i + 1, i);
                    bool moveResult = ExecuteNonQuery(query) == 1;
                    result = result && moveResult;
                    if (!result) break;
                }
                if (result) {
                    transaction.Commit();
                    transaction.Dispose();
                    logger.LogDebug($"Moved {maxPosition - insertIndex + 1} file positions " +
                        $"in collection {collectionID}");
                    idx = insertIndex;
                } else {
                    transaction.Rollback();
                    transaction.Dispose();
                    logger.LogWarning($"Could not move files in collection {collectionID}");
                    return result;
                }
            } else {
                logger.LogWarning($"Unusable collection position of {insertIndex} was given");
                return false;
            }

            query = createStatementString("INSERT INTO FileCollectionAssociations (FileID, CollectionID, Position) " +
                "VALUES (?,?,?)", fileID, collectionID, idx);
            logger.LogDebug("Inserting with query: " + query);
            try {
                result = ExecuteNonQuery(query) == 1;
            } catch (SQLiteException ex) {
                logger.LogWarning(ex, "File add failure due to SQLite error");
                result = false;
            }
            logger.LogInformation($"File {fileID} in collection {collectionID} was" + (result ? " " : " not ")
                        + "added to FileCollectionAssociations table");

            return result;
        }

        /// <summary>
        ///     Returns a file collection with the name and list of files and 
        ///     their positions using the collection id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Collection object with the collection name, id and list of files</returns>
        /// 
        public GetCollectionType GetFileCollection(int id)
        {
            GetCollectionType collection = null;

            string statement = createStatementString("SELECT Name FROM Collections WHERE ID = ?", id);
            var com = new SQLiteCommand(statement, db);
            var read = com.ExecuteReader();
            if (read.HasRows && read.Read()) {
                collection = new GetCollectionType();
                collection.Name = read.GetString(0);
                collection.ID = id;
            } else {
                logger.LogWarning("Could not find collection with ID of " + id);
                read.Close();
                com.Dispose();
                return collection;
            }
            read.Close();
            com.Dispose();

            collection.Files = new List<GetFileCollectionAssociationType>();
            statement = createStatementString("SELECT * FROM FileCollectionAssociations WHERE CollectionID = ?", id);
            com = new SQLiteCommand(statement, db);
            read = com.ExecuteReader();
            while (read.HasRows && read.Read()) {
                collection.Files.Add(new GetFileCollectionAssociationType()
                {
                    FileID = read.GetInt32(read.GetOrdinal("FileID")),
                    CollectionID = id,
                    Position = read.GetInt32(read.GetOrdinal("Position"))
                });
            }

            return collection;
        }

        /// <summary>
        ///     Returns a file collection with the name and list of files and 
        ///     their positions using the collection name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Collection object with the collection name, id and list of files</returns>
        public GetCollectionType GetFileCollection(string name)
        {
            string statement = createStatementString("SELECT ID FROM Collections WHERE Name = ?", name);
            var com = new SQLiteCommand(statement, db);
            var read = com.ExecuteReader();
            if (read.HasRows && read.Read()) {
                int id = read.GetInt32(0);
                read.Close();
                com.Dispose();
                return GetFileCollection(id);
            } else {
                logger.LogWarning("Could not find collection with name of " + name);
                read.Close();
                com.Dispose();
                return null;
            }
        }

        /* End Collection Section */

        public void CloseConnection()
        {
            db.Close();
        }
    }
}
