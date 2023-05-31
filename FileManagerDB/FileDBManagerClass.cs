using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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

        SQLiteTransaction transaction;
        public bool InTransaction { get; private set; } = false;

        public FileDBManagerClass(string dbLoc, ILogger logger)
        {
            this.logger = logger;
            logger.LogDebug("Creating or opening DB at " + dbLoc);
            db = new SQLiteConnection("DataSource=" + dbLoc);
            db.Open();

            //Creates
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

            //Updates
            UpdateTable(Tag.TableName, Tag.Columns, new List<string> { "Description" });
            UpdateTable(TagCategory.TableName, TagCategory.Columns, new List<string> { "Color" });

            ExecuteNonQuery("PRAGMA foreign_keys=ON");
        }

        private string createStatement(string statment, params object[] args)
        {
            var s = new SQLiteParameter(System.Data.DbType.Date);
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
                if (arg is null) {
                    fixArg = "NULL";
                } else if (arg.GetType() == typeof(int) ||
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
            int result;
            using (var com = new SQLiteCommand(query, db)) {
                result = com.ExecuteNonQuery();
            }
            return result;
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
            logger.LogDebug($"QUERY: \n{query}");
            ExecuteNonQuery(query);
        }

        /// <summary>
        /// Call this to update tables in databases created in an older version.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="colDefs"></param>
        /// <param name="colsToAdd"></param>
        private void UpdateTable(string name, Dictionary<string, string> colDefs, List<string> colsToAdd)
        {
            logger.LogInformation("Updating " + name);
            foreach (var colName in colsToAdd) {
                var com = new SQLiteCommand($"SELECT 1 FROM pragma_table_info('{name}') WHERE Name='{colName}'", db);
                var res = com.ExecuteReader();
                bool colMissing = !res.HasRows;
                res.Close();
                com.Dispose();
                
                if (colMissing) {
                    try {
                        string updateStmt = $"ALTER TABLE {name} ADD COLUMN {colName} {colDefs[colName]}";
                        logger.LogDebug("Update statement: " + updateStmt);
                        ExecuteNonQuery(updateStmt);
                    } catch {
                        logger.LogError($"Failed to add column {colName} to table {name}");
                        if (!colDefs.ContainsKey(colName)) logger.LogError($"{colName} not defined");
                    }
                }
            }
        }

        /// <summary>
        ///     Starts a transaction. You can check if a transaction is active by looking at the 
        ///     InTransaction member.
        /// </summary>
        public void StartTransaction()
        {
            transaction = db.BeginTransaction();
            InTransaction = true;
            logger.LogInformation("Transaction started");
        }

        /// <summary>
        ///     Commits a transaction, finishes it and closes the transaction. Will always commit, 
        ///     even if errors occured, so keep aware.
        /// </summary>
        public void FinishTransaction()
        {
            if (InTransaction) {
                transaction.Commit();
                transaction.Dispose();
                InTransaction = false;
                logger.LogInformation("Transaction commited and completed");
            } else {
                logger.LogInformation("Unnecessary attempt to close a transaction");
            }
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
        public bool AddFile(string filepath, string filetype, string hash, string altname = "", 
            long size = 0, DateTimeOptional created = null)
        {
            bool result;
            string statement = createStatement("INSERT INTO FileTypes (Name) VALUES (?)", filetype);
            int num = ExecuteNonQuery(statement);

            if (num == 0) {
                logger.LogInformation(filetype + " already exists in FileType table");
            } else {
                logger.LogInformation(filetype + " added to FileType table");
            }

            int idx = filepath.LastIndexOf('\\');
            string path = filepath.Substring(0, idx);
            string filename = filepath.Substring(idx + 1);
            long createdTime = 0;
            int pathID, typeID;

            if (created != null) {
                createdTime = DateTimeOptional.ToUnixTime(created);
            }

            if (!Path.IsPathRooted(path)) {
                logger.LogWarning(path + " is not a proper full path");
                return false;
            }

            statement = createStatement("INSERT INTO FilePaths (Path) VALUES (?)", path);
            num = ExecuteNonQuery(statement);
            if (num == 0) {
                logger.LogInformation(path + " already exists in FilePath table");
            } else {
                logger.LogInformation(path + " added to FilePath table");
            }

            statement = createStatement("SELECT ID FROM FilePaths WHERE Path = ?", path);
            using (var com = new SQLiteCommand(statement, db)) {
                var reader = com.ExecuteReader();
                reader.Read();
                pathID = reader.GetInt32(0);
                reader.Close();
                logger.LogDebug($"Found ID {pathID} for {path}");
            }
            statement = createStatement("SELECT ID FROM FileTypes WHERE Name = ?",
                            filetype.ToLowerInvariant());
            using (var com = new SQLiteCommand(statement, db)) {
                var reader = com.ExecuteReader();
                reader.Read();
                typeID = reader.GetInt32(0);
                logger.LogDebug($"Found ID {typeID} for {filetype}");
                reader.Close();
            }

            if (created is null) { 
                statement = createStatement("INSERT INTO Files " +
                                "(Filename, PathID, FileTypeID, Altname, Hash, Size) VALUES (?,?,?,?,?,?)",
                                filename, pathID, typeID, altname, hash, size);
            } else {
                statement = createStatement("INSERT INTO Files " +
                                "(Filename, PathID, FileTypeID, Altname, Hash, Size, Created) VALUES " +
                                "(?,?,?,?,?,?,?)",
                                filename, pathID, typeID, altname, hash, size, createdTime);
            }
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
        public GetFileMetadataType GetFileMetadata(int fileID)
        {
            string statement = "SELECT * FROM Files "
                            + "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID "
                            + "JOIN FilePaths ON Files.PathID = FilePaths.ID "
                            + $" WHERE Files.ID = {fileID}";
            logger.LogDebug("Query: " + statement);
            var com = new SQLiteCommand(statement, db).ExecuteReader();
            List<GetFileMetadataType> results = new List<GetFileMetadataType>();

            Func<long, DateTime> convertDT = (unixTime) =>
            {
                logger.LogDebug("Converting Unix time " + unixTime);
                return DateTimeOptional.FromUnixTime(unixTime);
            };

            if (com.Read()) {
                results.Add(new GetFileMetadataType
                {
                    ID = fileID,
                    PathID = com.GetInt32(com.GetOrdinal("PathID")),
                    Path = com.GetString(com.GetOrdinal("Path")),
                    Filename = com.GetString(com.GetOrdinal("Filename")),
                    Altname = com.GetString(com.GetOrdinal("Altname")),
                    FileTypeID = com.GetInt32(com.GetOrdinal("FileTypeID")),
                    FileType = com.GetString(com.GetOrdinal("Name")),
                    Hash = com.GetString(com.GetOrdinal("Hash")),
                    Size = com.GetInt64(com.GetOrdinal("Size")),
                    Created = convertDT(com.GetInt64(com.GetOrdinal("Created")))
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
            Func<long, DateTime> convertDT = (unixTime) =>
            {
                logger.LogDebug("Converting Unix time " + unixTime);
                return DateTimeOptional.FromUnixTime(unixTime);
            };
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
                    Hash = com.GetString(com.GetOrdinal("Hash")),
                    Size = com.GetInt64(com.GetOrdinal("Size")),
                    Created = convertDT(com.GetInt64(com.GetOrdinal("Created")))
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
        ///     Applies filters to file metadata search.
        /// </summary>
        /// <param name="filters"></param>
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
        public List<GetFileMetadataType> GetFileMetadata(FileSearchFilter filter)
        {
            string statementPart1 = "SELECT * FROM Files ";
            string statementPart2 = "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID ";
            string statementPart3 = "JOIN FilePaths ON Files.PathID = FilePaths.ID ";

            if (filter is null) {
                logger.LogWarning("Filter is null");
                return null;
            }

            List<object> whereValues = new List<object>();
            logger.LogInformation("Querying with file metadata with filters.");

            string statement = statementPart1 + statementPart2 + statementPart3;
            filter.BuildWhereStatementPart(ref statement, ref whereValues);
            statement = createStatement(statement, whereValues.ToArray());
            logger.LogDebug("Using filtered query: " + statement);

            var com = new SQLiteCommand(statement, db);
            var read = com.ExecuteReader();
            var results = new List<GetFileMetadataType>();
            Func<long, DateTime> convertDT = (unixTime) =>
            {
                logger.LogDebug("Converting Unix time " + unixTime);
                return DateTimeOptional.FromUnixTime(unixTime);
            };
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
                    Hash = read.GetString(read.GetOrdinal("Hash")),
                    Size = read.GetInt64(read.GetOrdinal("Size")),
                    Created = convertDT(read.GetInt64(read.GetOrdinal("Created")))
                });
            }
            read.Close();
            com.Dispose();
            
            if (filter.UsingCustomFilter) {
                logger.LogInformation("Applying custom filter");
                int oldCount = results.Count;
                results = filter.CustomFilter(results);
                logger.LogInformation($"Results reduced from {oldCount} to {results.Count}");
            }

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
        ///     Updates a file's metadata. 
        /// </summary>
        /// <param name="newInfo"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public bool UpdateFileMetadata(FileMetadata newInfo, FileSearchFilter filter)
        {
            logger.LogDebug(MethodBase.GetCurrentMethod().Name);
            List<string> cols = new List<string>();
            List<object> vals = new List<object>();
            List<string> wheres = new List<string>();
            List<object> whereValues = new List<object>();
            string statement;
            bool result;

            if (newInfo.IsEmpty || filter is null || filter.IsEmpty) {
                if (filter is null) logger.LogInformation("Missing filter");
                if (filter.IsEmpty) logger.LogInformation("Empty filter");
                if (newInfo.IsEmpty) logger.LogInformation("Update information is empty");
                return false;
            }

            if (newInfo.UsingPath) {
                statement = createStatement("INSERT OR IGNORE INTO Filepaths (Path) Values (?)", newInfo.Path);
                ExecuteNonQuery(statement);
                statement = createStatement("SELECT ID FROM Filepaths WHERE Path = ?", newInfo.Path);
                var com = new SQLiteCommand(statement, db);
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

                logger.LogDebug("Updating file path");
                cols.Add("PathID");
                vals.Add(results);
            }

            if (newInfo.UsingFileType) {
                statement = createStatement("INSERT OR IGNORE INTO Filetypes (Name) Values (?)", newInfo.FileType);
                ExecuteNonQuery(statement);
                statement = createStatement("SELECT ID FROM FileTypes WHERE Name = ?", newInfo.FileType);
                var com = new SQLiteCommand(statement, db);
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

                logger.LogDebug("Updating file type");
                cols.Add("FileTypeID");
                vals.Add(results);
            }
            if (newInfo.UsingFilename) {
                logger.LogDebug("Updating filename");
                cols.Add("Filename");
                vals.Add(newInfo.Filename);
            }
            if (newInfo.UsingAltname) {
                logger.LogDebug("Updating file altname");
                cols.Add("Altname");
                vals.Add(newInfo.Altname);
            }
            if (newInfo.UsingHash) {
                logger.LogDebug("Updating file hash");
                cols.Add("Hash");
                vals.Add(newInfo.Hash);
            }
            if (newInfo.UsingSize) {
                logger.LogDebug("Updating file size");
                cols.Add("Size");
                vals.Add(newInfo.Size);
            }

            logger.LogInformation(newInfo.UsingCreated.ToString());
            logger.LogInformation(newInfo.Created.ToString());
            if (newInfo.UsingCreated) {
                logger.LogDebug("Updating file date");
                cols.Add("Created");
                vals.Add(DateTimeOptional.ToUnixTime(newInfo.Created));
            }

            logger.LogDebug("Number of columns to update: " + cols.Count);

            string assignmentstr = "";
            for (int i = 0; i < cols.Count; i++) {
                assignmentstr += cols[i] + " = ?";
                if (i + 1 < cols.Count) {
                    assignmentstr += ", ";
                }
            }

            statement = createStatement($"UPDATE Files SET {assignmentstr} ", vals.ToArray());
            string whereStr = "";
            filter.BuildWhereStatementPart(ref whereStr, ref whereValues);

            statement = createStatement(statement + whereStr, whereValues.ToArray());
            try {
                result = ExecuteNonQuery(statement) == 1;
            } catch (SQLiteException ex) {
                logger.LogError("SQLiteException: " + ex.Message);
                result = false;
            }  catch (Exception ex2) {
                logger.LogError("Exception: " + ex2.Message);
                result = false;
            }

            if (result) {
                logger.LogInformation("Updated file metadata with the following data: \n" + newInfo.ToString());
            } else {
                logger.LogWarning("Could not update with the following information: \n" + newInfo);
            }

            return result;
        }

        /* End File Section */

        /* Misc Section */

        /// <summary>
        ///     Changes the path of all files using the path with 
        ///     the given ID.
        /// </summary>
        /// <param name="pathID"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public bool ChangePath(int pathID, string newPath)
        {
            if (string.IsNullOrWhiteSpace(newPath) || !Path.IsPathRooted(newPath)) {
                logger.LogWarning($"Could not change the path of {pathID} to " + newPath);
                return false;
            }

            string statment = createStatement("UPDATE FilePaths SET Path=? WHERE ID=?", newPath, pathID);
            bool result = ExecuteNonQuery(statment) == 1;

            if (!result) logger.LogWarning($"Failed to change path value of {pathID} to {newPath}");

            return result;
        }

        /// <summary>
        ///     Returns the path ID. THe path must be an exact match. If not found, 
        ///     returns -1.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int GetPathID(string path)
        {
            int pathID = -1;
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path)) {
                logger.LogWarning($"{path} not found because it is invalid.");
                return pathID;
            }

            string statement = createStatement("SELECT ID FROM FilePaths WHERE Path = ?", path);
            logger.LogDebug($"Searching for path with query: {statement}");

            var com = new SQLiteCommand(statement, db);
            var reader = com.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                pathID = reader.GetInt32(reader.GetOrdinal("ID"));
            }

            reader.Close();
            com.Dispose();
            
            if (pathID != -1) {
                logger.LogInformation($"ID of {pathID} found for path {path}");
            } else {
                logger.LogInformation($"Path {path} not found");
            }

            return pathID;
        }

        /// <summary>
        ///     Changes the file path of a file. If the new path does not exist, 
        ///     will add it to the DB first.
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public bool ChangeFilePath(int fileID, string newPath)
        {
            if (string.IsNullOrWhiteSpace(newPath) || !Path.IsPathRooted(newPath)) {
                logger.LogWarning($"Could not change the path of file {fileID} to {newPath} because it is an invalid path");
                return false;
            }

            int pathID;

            //Get Path ID segment, adding new path if necessary
            string statement = createStatement("SELECT * FROM FilePaths WHERE Path = ?", newPath);
            using (var com = new SQLiteCommand(statement, db)) {
                var reader = com.ExecuteReader();
                if (reader.HasRows) {
                    reader.Read();
                    pathID = reader.GetInt32(reader.GetOrdinal("ID"));
                    reader.Close();
                } else {
                    logger.LogInformation($"Path {newPath} not in DB, adding");
                    reader.Close();
                    statement = createStatement("INSERT INTO FilePaths (Path) VALUES (?)", newPath);
                    ExecuteNonQuery(statement);
                    statement = createStatement("SELECT * FROM FilePaths WHERE Path = ?", newPath);
                    using (var com2 = new SQLiteCommand(statement, db)) {
                        var reader2 = com2.ExecuteReader();
                        reader2.Read();
                        pathID = reader2.GetInt32(reader2.GetOrdinal("ID"));
                        reader2.Close();
                    }
                }
            }

            statement = createStatement("UPDATE Files SET PathID = ? WHERE ID = ?", pathID, fileID);
            bool result = ExecuteNonQuery(statement) == 1;
            logger.LogInformation($"Path for file {fileID} was {(result ? "" : " not")} updated to {newPath}");
            
            return result;
        }

        /* End Misc Section */

        public void CloseConnection()
        {
            FinishTransaction();
            db.Close();
        }
    }
}
