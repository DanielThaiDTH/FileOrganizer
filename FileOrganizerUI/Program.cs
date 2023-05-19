using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using FileOrganizerCore;

namespace FileOrganizerUI
{
    static class Program
    {
        public static Microsoft.Extensions.Logging.ILogger logger;
        private static int logKeepDate = 7;
        private static string dbLocation;
        private static FileOrganizer core;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.File("log.log", rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            Log.Logger = GetLogConfig().CreateLogger();
            logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<IServiceProvider>();
            bool hasLogsDurationSetting = int.TryParse(ConfigurationManager.AppSettings.Get("LogsKeepDuration"), out logKeepDate);
            if (!hasLogsDurationSetting) logKeepDate = 7;
            ClearOldLogs();

            dbLocation = ConfigurationManager.AppSettings.Get("DB");
            logger.LogInformation("DB file: " + dbLocation);
            core = new FileOrganizer(logger, ConfigurationManager.AppSettings);
            core.StartUp();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(logger, core));
        }

        static void ClearOldLogs()
        {
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
            logger.LogDebug("Root: " + root);
            var dir = new DirectoryInfo(root);
            var logFiles = dir.GetFiles("log*.log");
            DateTime today = DateTime.Today;
            CultureInfo culture = (CultureInfo) CultureInfo.CurrentCulture.Clone();
            DateTimeFormatInfo dtInfo = culture.DateTimeFormat;
            dtInfo.ShortDatePattern = "yyyyMMdd";

            foreach (var logFile in logFiles) {
                logger.LogDebug("Checking log file " + logFile.Name);
                string logDate = logFile.Name.Replace(".log", "").Replace("log", "");
                logger.LogDebug("Checking log date " + logDate);
                try {
                    DateTime logDT = DateTime.ParseExact(logDate, "yyyyMMdd", dtInfo);
                    if (today.Subtract(logDT).TotalDays > logKeepDate) {
                        logger.LogInformation("Removing log file " + logFile.Name);
                        logFile.Delete();
                    }
                }
                catch (Exception ex) {
                    logger.LogInformation("Failed to remove log due to: " + ex.Message);
                    logger.LogDebug(ex.StackTrace);
                }
            }
            
        }

        static LoggerConfiguration GetLogConfig()
        {
            LoggerConfiguration config = new LoggerConfiguration();
            string level = ConfigurationManager.AppSettings.Get("LogLevel");
            if (string.IsNullOrWhiteSpace(level) || level.Trim().ToLowerInvariant() == "debug") {
                config = config.MinimumLevel.Debug();
            } else if (level.Trim().ToLowerInvariant() == "info" || level.Trim().ToLowerInvariant() == "information") {
                config = config.MinimumLevel.Information();
            } else if (level.Trim().ToLowerInvariant() == "warning") {
                config = config.MinimumLevel.Warning();
            } else if (level.Trim().ToLowerInvariant() == "error") {
                config = config.MinimumLevel.Error();
            } else {
                config = config.MinimumLevel.Debug();
            }

            return config.WriteTo.File("log.log", rollingInterval: RollingInterval.Day);
        }
    }
}
