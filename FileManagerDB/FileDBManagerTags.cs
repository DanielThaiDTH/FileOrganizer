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
                string query = createStatement("INSERT INTO TagCategories (Name) VALUES (?)", tagCategory);
                logger.LogDebug("Using query: " + query);
                int added = ExecuteNonQuery(query);
                if (added == 1) {
                    logger.LogInformation("Added new tag category: " + tagCategory);
                    result = true;
                } else {
                    logger.LogDebug(tagCategory + " already exists, not adding");
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
                    Name = read.GetString(read.GetOrdinal("Name")),
                    Color = read.GetInt32(read.GetOrdinal("Color"))
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

            string query = createStatement("DELETE FROM TagCategories WHERE ID = ?", id);
            logger.LogInformation("Deleting tag category with ID of " + id);

            result = ExecuteNonQuery(query) == 1;

            if (!result) logger.LogWarning("Could not delete tag category " + id);

            return result;
        }

        public bool UpdateTagCategoryName(int id, string newName)
        {
            bool result;

            string statement = createStatement("UPDATE TagCategories SET Name = ? WHERE ID = ?", newName, id);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag category {id} was {(result ? "" : "not")} renamed to {newName}");

            return result;
        }

        public bool UpdateTagCategoryColor(int id, int argb)
        {
            bool result;

            string statement = createStatement("UPDATE TagCategories SET Color = ? WHERE ID = ?", argb, id);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag category {id} color was {(result ? "" : "not")} changed to #{argb.ToString("X")}");

            return result;
        }



        /// <summary>
        ///     Adds a tag. Can also add an optional category to the tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagCategory"></param>
        /// <returns>Status of adding new tag. Will return if it already exists</returns>
        /// <exception cref="InvalidDataException">If fetching from tag category table fails</exception>
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

            string query = createStatement("SELECT ID FROM TagCategories WHERE Name = ?", tagCategory.ToLowerInvariant());
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
                query = createStatement("INSERT OR IGNORE INTO Tags (Name, CategoryID) VALUES (?, ?)",
                        tag,
                        categoryID
                    );
            } else {
                query = createStatement("INSERT OR IGNORE INTO Tags (Name, CategoryID) VALUES (?, NULL)",
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

            string statement = createStatement("SELECT COUNT(*) FROM Files WHERE ID = ?", fileID);
            logger.LogDebug("Querying file existence using: " + statement);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                read.Read();
                result = read.GetInt32(0) == 1;
                read.Close();
            }

            if (!result) {
                logger.LogWarning(fileID + " does not exist in DB, cannot add tag");
                return result;
            }

            AddTag(tag, tagCategory);

            statement = createStatement("SELECT ID FROM Tags WHERE Name = ?", tag);
            logger.LogDebug("Querying tag id using: " + statement);
            int tagID;
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                read.Read();
                tagID = read.GetInt32(0);
                read.Close();
            }

            statement = createStatement("SELECT COUNT(*) FROM FileTagAssociations " +
                "WHERE FileID = ? AND TagID = ?", fileID, tagID);
            logger.LogDebug("Checking file tag association existence using: " + statement);
            using (var com = new SQLiteCommand(statement, db)) {
                var read = com.ExecuteReader();
                read.Read();
                result = read.GetInt32(0) == 0;
                read.Close();
            }

            if (result) {
                statement = createStatement("INSERT OR IGNORE INTO FileTagAssociations (FileID, TagID) VALUES (?, ?)",
                    fileID, tagID);
                result = ExecuteNonQuery(statement) == 1;
                if (result) logger.LogInformation($"Tag {tag} added to file #{fileID}");
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
                    Name = read.GetString(read.GetOrdinal("Name")),
                    Description = read.GetString(read.GetOrdinal("Description"))
                };

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("ParentID")))) {
                    newTag.ParentTagID = -1;
                } else {
                    newTag.ParentTagID = read.GetInt32(read.GetOrdinal("ParentID"));
                }

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("CategoryID")))) {
                    newTag.CategoryID = -1;
                    newTag.Category = null;
                    logger.LogDebug("No category for tag " + newTag.Name);
                } else {
                    newTag.CategoryID = read.GetInt32(read.GetOrdinal("CategoryID"));
                    newTag.Category = read.GetString(6);
                }
                tags.Add(newTag);
            }
            read.Close();
            com.Dispose();
            logger.LogInformation($"Found {tags.Count} tags");

            return tags;
        }

        /// <summary>
        ///     Returns all tags a given file has.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public List<GetTagType> GetTagsForFile(int fileID)
        {
            List<GetTagType> fileTags = new List<GetTagType>();

            string query = createStatement("SELECT * FROM FileTagAssociations JOIN Tags " +
                "ON FileTagAssociations.TagID = Tags.ID " +
                "LEFT JOIN TagCategories ON CategoryID = TagCategories.ID " +
                "WHERE FileTagAssociations.FileID = ?", fileID);

            var com = new SQLiteCommand(query, db);
            var read = com.ExecuteReader();

            while (read.HasRows && read.Read()) {
                var newTag = new GetTagType()
                {
                    ID = read.GetInt32(read.GetOrdinal("TagID")),
                    Name = read.GetString(read.GetOrdinal("Name")),
                    Description = read.GetString(read.GetOrdinal("Description")),
                };

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("ParentID")))) {
                    newTag.ParentTagID = -1;
                    logger.LogTrace("No parent for tag " + newTag.Name);
                } else {
                    newTag.ParentTagID = read.GetInt32(read.GetOrdinal("ParentID"));
                }

                if (DBNull.Value.Equals(read.GetValue(read.GetOrdinal("CategoryID")))) {
                    newTag.CategoryID = -1;
                    newTag.Category = null;
                    logger.LogTrace("No category for tag " + newTag.Name);
                } else {
                    newTag.CategoryID = read.GetInt32(read.GetOrdinal("CategoryID"));
                    newTag.Category = read.GetString(8);
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

            string query = createStatement("DELETE FROM Tags WHERE ID = ?", id);
            logger.LogInformation("Deleting tag with ID of " + id);

            result = ExecuteNonQuery(query) == 1;

            if (!result) logger.LogWarning("Could not delete tag " + id);

            return result;
        }

        public bool DeleteTagFromFile(int fileID, int tagID)
        {
            bool result;

            string statement = createStatement("DELETE FROM FileTagAssociations " +
                "WHERE FileID = ? AND TagID = ?", fileID, tagID);
            logger.LogInformation($"Removing tag {tagID} from file {fileID}");

            result = ExecuteNonQuery(statement) == 1;
            if (!result) logger.LogInformation($"Failed to remove tag {tagID} from file {fileID}");

            return result;
        }

        public bool UpdateTagName(string name, string oldName)
        {
            bool result;

            string statement = createStatement("UPDATE Tags SET Name = ? WHERE Name = ?", name, oldName);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag {oldName} was {(result ? "" : "not")} renamed to {name}");

            return result;
        }

        public bool UpdateTagName(string name, int id)
        {
            bool result;

            string statement = createStatement("UPDATE Tags SET Name = ? WHERE ID = ?", name, id);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag {id} was {(result ? "" : "not")} renamed to {name}");

            return result;
        }

        public bool UpdateTagDescription(int tagID, string desc)
        {
            bool result;

            if (desc is null) desc = "";
            string statement = createStatement("UPDATE Tags SET Description = ? WHERE ID = ?", desc, tagID);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag {tagID} description was updated");
            logger.LogDebug("New description: " + desc);

            return result;
        }

        /// <summary>
        ///     Updates tag category for a given tag.
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public bool UpdateTagCategory(int tagID, int categoryID)
        {
            bool result;
            string statement;

            if (categoryID < 1) {
                statement = createStatement("UPDATE Tags SET CategoryID = ? WHERE ID = ?", null, tagID);
            } else {
                statement = createStatement("UPDATE Tags SET CategoryID = ? WHERE ID = ?", categoryID, tagID);
            }

            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag {tagID} {(result? "changed" : "did not change ")} category to {categoryID}");

            return result;
        }

        /// <summary>
        ///     Updates tag category for a given tag selected using the tag name.
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public bool UpdateTagCategory(string tagName, int categoryID)
        {
            bool result;

            string statement = createStatement("UPDATE Tags SET CategoryID = ? WHERE Name = ?", categoryID, tagName);
            result = ExecuteNonQuery(statement) == 1;

            logger.LogInformation($"Tag {tagName} {(result ? "changed" : "did not change ")} category to {categoryID}");

            return result;
        }

        /// <summary>
        /// Updates the tag parent. Checks if there is a loop in ancestry tree, fails if 
        /// so. Can return an error message in the errMsg parameter.
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="parentTagID"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool UpdateTagParent(int tagID, int parentTagID, out string errMsg)
        {
            bool result;

            string statement = "";
            errMsg = "";
            if (parentTagID > 0) {
                List<GetTagType> tags = GetAllTags();
                int ancestorID = parentTagID;
                HashSet<int> seenIDs = new HashSet<int>();
                seenIDs.Add(tagID);

                while (!seenIDs.Contains(ancestorID) && ancestorID != -1) {
                    var ancestor = tags.Find(t => t.ID == ancestorID);
                    if (ancestor is null) {
                        ancestorID = -1;
                        break;
                    }
                    seenIDs.Add(ancestorID);
                    ancestorID = ancestor.ParentTagID;
                }

                logger.LogDebug("Exiting loop with ancestor ID of " + ancestorID);

                if (ancestorID == -1 && tags.Any(t => t.ID == parentTagID)) {
                    statement = createStatement("UPDATE Tags Set ParentID = ? WHERE ID = ?", parentTagID, tagID);
                } else if (tags.Any(t => t.ID == ancestorID)) {
                    errMsg = $"Failed to make {tags.Find(t => t.ID == parentTagID).Name} " +
                        $"parent of {tags.Find(t => t.ID == tagID).Name} because it would create an ancestry loop";
                    logger.LogWarning(errMsg);

                    return false;
                } else {
                    errMsg = $"Failed to add parent to {tags.Find(t => t.ID == tagID).Name} because " +
                        $"the parent ({parentTagID}) does not exist.";
                    logger.LogWarning(errMsg);
                    
                    return false;
                }
            } else {
                statement = createStatement("UPDATE Tags Set ParentID = ? WHERE ID = ?", null, tagID);
            }

            result = ExecuteNonQuery(statement) == 1;
            logger.LogInformation($"Tag {tagID} {(result ? "changed" : "did not change ")} parent tag to {parentTagID}");

            if (!result) errMsg = "Failed to update tag parent";

            return result;
        }
    }
}
