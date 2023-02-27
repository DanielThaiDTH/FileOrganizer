using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace FileOrganizerCore.Utilities
{
    /// <summary>
    ///     Provides static utility functions for hashing files. Only 
    ///     does SHA256 hashing so far.
    /// </summary>
    public class Hasher
    {
        static ILogger logger;
        public static void SetLogger(ILogger logger)
        {
            Hasher.logger = logger;
        }

        public static string HashFile(string filename, in ActionResult<bool> result)
        {
            string hash = "";
            using (SHA256 hasher = SHA256.Create()) {
                using (FileStream stream = new FileStream(filename, FileMode.Open)) {
                    try {
                        stream.Position = 0;
                        var hashArr = hasher.ComputeHash(stream);
                        hash = BitConverter.ToString(hashArr);
                    } catch (IOException ex) {
                        string msg = $"Error reading file {filename} for hashing";
                        logger.LogWarning(ex, msg);
                        result.AddError(ErrorType.Path, msg);
                    } catch (UnauthorizedAccessException ex) {
                        string msg = $"Cannot access {filename} for hashing";
                        logger.LogWarning(ex, msg);
                        result.AddError(ErrorType.Access, msg);
                    } catch (Exception ex) {
                        string msg = $"Could not hash {filename} due to unknown exception";
                        logger.LogWarning(ex, msg);
                        result.AddError(ErrorType.Path, msg);
                    }
                }
            }

            result.SetResult(!result.HasError());

            return hash;
        }
    }
}
