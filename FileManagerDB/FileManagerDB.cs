using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FileManagerDB.Entities;

namespace FileManagerDB
{
    public class FileManagerDB
    {
        private SQLiteConnection db;
        private ILogger logger;

        public FileManagerDB(string dbLoc, ILogger logger)
        {
            db = new SQLiteConnection(dbLoc);
            this.logger = logger;
            logger.LogInformation("FileDB manager started");
            db.CreateTable<FileInfo>();
            db.CreateTable<FilePath>();
        }
    }
}
