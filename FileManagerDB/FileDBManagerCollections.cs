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
            } else if (InTransaction) {
                logger.LogWarning("Cannot add collection because transaction is currently active");
                return false;
            }
            
            if (fileSequence is null) {
                fileSequence = new List<int>();
            }

            var transaction = db.BeginTransaction();
            string statment = createStatement("INSERT INTO Collections (Name) VALUES (?)", name);
            logger.LogDebug("Creating collection with query: " + statment);
            result = ExecuteNonQuery(statment) == 1;
            logger.LogInformation(name + " was" + (result ? " " : " not ") + "added to Collections table");

            if (result) {
                statment = createStatement("SELECT ID FROM Collections WHERE Name = ?", name);
                var com = new SQLiteCommand(statment, db);
                var read = com.ExecuteReader();
                read.Read();
                int collectionID = read.GetInt32(0);
                read.Close();
                com.Dispose();
                logger.LogDebug($"Found ID {collectionID} for {name}");

                int index = 0;

                foreach (int fileID in fileSequence) {
                    statment = createStatement("INSERT INTO FileCollectionAssociations (CollectionID,FileID,Position) " +
                        "VALUES (?,?,?)", collectionID, fileID, index + 1);
                    logger.LogDebug("Creating collection with query: " + statment);
                    bool insertResult = false;

                    try {
                        insertResult = ExecuteNonQuery(statment) == 1;
                    }
                    catch (SQLiteException ex) {
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
            if (InTransaction) {
                logger.LogWarning("Cannot add file to collection, currently in transaction");
                return false;
            }

            string statement = createStatement("SELECT COUNT(*) FROM Files WHERE ID = ?", fileID);
            int files = 0;
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    files = read.GetInt32(0);
                }
                read.Close();
            }
            
            if (files == 0) {
                logger.LogWarning($"File with ID of {fileID} was not found, " +
                    $"could not add to collection {collectionID}");
                return false;
            }

            statement = createStatement("SELECT COUNT(*) FROM Collections WHERE ID = ?", collectionID);
            int collections = 0;
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    collections = read.GetInt32(0);
                }
                read.Close();
            }
            if (collections == 0) {
                logger.LogWarning("Collection with ID of " + collectionID + " was not found, could not add to collection");
                return false;
            }

            statement = createStatement("SELECT MAX(Position) FROM FileCollectionAssociations " +
                "WHERE CollectionID = ?", collectionID);
            int maxPosition = -1;
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();

                if (read.HasRows && read.Read() && !DBNull.Value.Equals(read.GetValue(0))) {
                    maxPosition = read.GetInt32(0);
                }
                read.Close();
            }

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

                for (int i = maxPosition; i >= insertIndex && result; i--) {
                    statement = createStatement("UPDATE FileCollectionAssociations SET Position = ? " +
                        "WHERE Position = ? AND CollectionID = ?", i + 1, i, collectionID);
                    result = ExecuteNonQuery(statement) == 1;
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

            statement = createStatement("INSERT INTO FileCollectionAssociations (FileID, CollectionID, Position) " +
                "VALUES (?,?,?)", fileID, collectionID, idx);
            logger.LogDebug("Inserting with query: " + statement);
            try {
                result = ExecuteNonQuery(statement) == 1;
            }
            catch (SQLiteException ex) {
                logger.LogWarning(ex, "File add failure due to SQLite error");
                result = false;
            }
            logger.LogInformation($"File {fileID} in collection {collectionID} was" + (result ? " " : " not ")
                        + "added to FileCollectionAssociations table");

            return result;
        }

        /// <summary>
        ///     Deletes and reorganizes files in a collection 
        ///     (i.e. positions will shift accordingly if deleted file is not at the end)
        /// </summary>
        /// <param name="collectionID"></param>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public bool DeleteFileInCollection(int collectionID, int fileID)
        {
            bool result;

            if (InTransaction) {
                logger.LogWarning("Could not remove file from collection, DB is in a transaction");
                return false;
            }

            int position;
            string statement = createStatement("SELECT Position FROM FileCollectionAssociations " +
                "WHERE CollectionID = ? AND FileID = ?", collectionID, fileID);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    position = read.GetInt32(0);
                    read.Close();
                } else {
                    read.Close();
                    logger.LogInformation($"No file {fileID} in collection {collectionID} to delete");
                    return false;
                }
            }

            int maxPosition;
            statement = createStatement("SELECT MAX(Position) FROM FileCollectionAssociations " +
                "WHERE CollectionID = ?", collectionID);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    maxPosition = read.GetInt32(0);
                    read.Close();
                } else {
                    read.Close();
                    logger.LogWarning("Could not find a max position");
                    return false;
                }
            }

            statement = createStatement("DELETE FROM FileCollectionAssociations " +
                "WHERE CollectionID = ? AND FileID = ?", collectionID, fileID);
            logger.LogDebug("Deleting with statement: " + statement);
            result = ExecuteNonQuery(statement) == 1;

            if (result) {
                var transaction = db.BeginTransaction();

                for (int i = position + 1; i <= maxPosition && result; i++) {
                    statement = createStatement("UPDATE FileCollectionAssociations SET Position = ? " +
                        "WHERE Position = ? AND CollectionID = ?", i - 1, i, collectionID);
                    result = ExecuteNonQuery(statement) == 1;
                }

                if (result) {
                    transaction.Commit();
                    transaction.Dispose();
                } else {
                    transaction.Rollback();
                    transaction.Dispose();
                }
            }

            logger.LogInformation($"File {fileID} was" + (result ? " " : " not ")
                        + $" removed from collection {collectionID}");

            return result;
        }

        /// <summary>
        ///     Returns a file collection with the name and list of files and 
        ///     their positions using the collection id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        ///     Collection object with the collection name, id and list of files.
        ///     Null if nothing found.
        /// </returns>
        /// 
        public GetCollectionType GetFileCollection(int id)
        {
            GetCollectionType collection = null;

            string statement = createStatement("SELECT Name FROM Collections WHERE ID = ?", id);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    collection = new GetCollectionType();
                    collection.Name = read.GetString(0);
                    collection.ID = id;
                    read.Close();
                } else {
                    logger.LogWarning("Could not find collection with ID of " + id);
                    read.Close();
                    return collection;
                }
            }

            collection.Files = new List<GetFileCollectionAssociationType>();
            statement = createStatement("SELECT * FROM FileCollectionAssociations WHERE CollectionID = ?", id);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                while (read.HasRows && read.Read()) {
                    collection.Files.Add(new GetFileCollectionAssociationType()
                    {
                        FileID = read.GetInt32(read.GetOrdinal("FileID")),
                        CollectionID = id,
                        Position = read.GetInt32(read.GetOrdinal("Position"))
                    });
                }
            }

            return collection;
        }

        /// <summary>
        ///     Returns a file collection with the name and list of files and 
        ///     their positions using the collection name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        ///     Collection object with the collection name, id and list of files.
        ///     Null if nothing found.
        /// </returns>
        public GetCollectionType GetFileCollection(string name)
        {
            string statement = createStatement("SELECT ID FROM Collections WHERE Name = ?", name);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows && read.Read()) {
                    int id = read.GetInt32(0);
                    read.Close();
                    return GetFileCollection(id);
                } else {
                    logger.LogWarning("Could not find collection with name of " + name);
                    read.Close();
                    return null;
                }
            }
        }

        /// <summary>
        ///     Searches for file collections using a non exact query. Empty string will 
        ///     return all collections.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<GetCollectionType> FileCollectionSearch(string query)
        {
            var results = new List<GetCollectionType>();
            string statement = createStatement("SELECT * FROM Collections WHERE Name LIKE ?", "%" + query + "%");

            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                if (read.HasRows) {
                    while (read.Read()) {
                        var collection = new GetCollectionType
                        {
                            Name = read.GetString(read.GetOrdinal("Name")),
                            ID = read.GetInt32(read.GetOrdinal("ID"))
                        };
                        results.Add(collection);
                    }
                    read.Close();
                } else {
                    logger.LogWarning($"Could not find collection with query: {statement}");
                    read.Close();
                    return results;
                }
            }

            foreach (var collection in results) {
                var files = new List<GetFileCollectionAssociationType>();
                statement = createStatement("SELECT * FROM FileCollectionAssociations WHERE CollectionID = ?", collection.ID);
                using (var com = new SQLiteCommand(statement, db)) {
                    var read = com.ExecuteReader();

                    while (read.HasRows && read.Read()) {
                        files.Add(new GetFileCollectionAssociationType()
                        {
                            FileID = read.GetInt32(read.GetOrdinal("FileID")),
                            CollectionID = collection.ID,
                            Position = read.GetInt32(read.GetOrdinal("Position"))
                        });
                    }

                    read.Close();
                    collection.Files = files;
                }
            }

            return results;
        }

        public bool UpdateCollectionName(int id, string newName)
        {
            string statement = createStatement("UPDATE Collections SET Name=? WHERE ID = ?", newName, id);
            logger.LogDebug("Updating collection name with statement: " + statement);
            bool result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Collection {id} was" + (result ? " " : " not ")
                        + "renamed to " + newName);

            return result;
        }

        public bool UpdateCollectionName(string name, string newName)
        {
            bool result;
            string statement = createStatement("SELECT ID FROM Collections WHERE Name = ?", name);
            var com = new SQLiteCommand(statement, db);
            var read = com.ExecuteReader();
            if (read.HasRows && read.Read()) {
                int id = read.GetInt32(0);
                read.Close();
                com.Dispose();
                result = UpdateCollectionName(id, newName);
            } else {
                read.Close();
                com.Dispose();
                logger.LogWarning("No collection with name of " + name);
                result = false;
            }

            logger.LogInformation($"Collection {name} was" + (result ? " " : " not ")
                        + "renamed to " + newName);

            return result;
        }

        public bool UpdateFilePositionInCollection(int collectionID, int fileID, int position)
        {
            bool result;
            string statement = createStatement("UPDATE FileCollectionAssociations SET Position = ? WHERE CollectionID = ? AND FileID = ?", 
                position, collectionID, fileID);
            logger.LogDebug("Updating position of file in collection with: " + statement);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Position of file {fileID} in collection {collectionID} was set {(result ? " " : " not ")}" +
                $"to {position}");

            return result;
        }

        public bool DeleteCollection(int id)
        {
            bool result;

            string statement = createStatement("DELETE From Collections WHERE ID = ?", id);
            result = ExecuteNonQuery(statement) == 1;

            if (result) {
                logger.LogInformation($"Deleted file collection");
            } else {
                logger.LogInformation($"Could not delete file collection");
            }

            return result;
        }

        public bool DeleteCollection(string name)
        {
            bool result = false;

            string statement = createStatement("SELECT ID FROM Collections WHERE Name = ?", name);
            var com = new SQLiteCommand(statement, db);
            var read = com.ExecuteReader();
            if (read.HasRows && read.Read()) {
                int id = read.GetInt32(0);
                logger.LogDebug($"Found ID of {id} for collection {name}");
                result = DeleteCollection(id);
            }

            if (result) {
                logger.LogInformation($"Deleted file collection " + name);
            } else {
                logger.LogInformation($"Could not delete file collection " + name);
            }

            return result;
        }
    }
}
