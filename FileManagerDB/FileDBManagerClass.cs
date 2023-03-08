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
                if (filter.HashFilterExact) {
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
            if (filter.UsingSize) {
                string comp = filter.IsSizeLesser ? "<=" : ">=";
                wheres.Add("Size " + comp + " ?");
                whereValues.Add(filter.Size);
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
            var com = new SQLiteCommand(statement, db);
            var reader = com.ExecuteReader();
            reader.Read();
            int pathID = reader.GetInt32(0);
            reader.Close();
            com.Dispose();
            logger.LogDebug($"Found ID {pathID} for {path}");
            statement = createStatement("SELECT ID FROM FileTypes WHERE Name = ?",
                            filetype.ToLowerInvariant());
            com = new SQLiteCommand(statement, db);
            reader = com.ExecuteReader();
            reader.Read();
            int typeID = reader.GetInt32(0);
            logger.LogDebug($"Found ID {typeID} for {filetype}");
            reader.Close();
            com.Dispose();

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
            string statementPart1 = "SELECT * FROM Files ";
            string statementPart2 = "JOIN FileTypes ON Files.FileTypeID = FileTypes.ID ";
            string statementPart3 = "JOIN FilePaths ON Files.PathID = FilePaths.ID ";

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
        ///     Updates a file's metadata. The info object must 
        ///     have the file ID. Tags and created dates are updated 
        ///     in another method.
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
                query = createStatement("INSERT OR IGNORE INTO Filepaths (Path) Values (?)", newInfo.Path);
                ExecuteNonQuery(query);
                query = createStatement("SELECT ID FROM Filepaths WHERE Path = ?", newInfo.Path);
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
                query = createStatement("INSERT OR IGNORE INTO Filetypes (Name) Values (?)", newInfo.FileType);
                ExecuteNonQuery(query);
                query = createStatement("SELECT ID FROM FileTypes WHERE Name = ?", newInfo.FileType);
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
                cols.Add("Hash");
                vals.Add(newInfo.Hash);
            }
            if (newInfo.UsingSize) {
                cols.Add("Size");
                vals.Add(newInfo.Size);
            }

            string assignmentstr = "";
            for (int i = 0; i < cols.Count; i++) {
                assignmentstr += cols[i] + " = ?";
                if (i + 1 < cols.Count) {
                    assignmentstr += ", ";
                }
            }

            BuildWhereArrays(filter, ref wheres, ref whereValues);
            query = createStatement($"UPDATE Files SET {assignmentstr} ", vals.ToArray());

            for (int i = 0; i < wheres.Count; i++) {
                if (i == 0) {
                    query += "WHERE ";
                } else {
                    query += " AND ";
                }

                query += wheres[i];
            }

            query = createStatement(query, whereValues.ToArray());
            bool result = ExecuteNonQuery(query) == 1;

            if (result) {
                logger.LogInformation("Updated file metadata with the following data: \n" + newInfo);
            } else {
                logger.LogWarning("Could not update with the following information: \n" + newInfo);
            }

            return result;
        }

        /* End File Section */

        public void CloseConnection()
        {
            db.Close();
        }
    }
}
