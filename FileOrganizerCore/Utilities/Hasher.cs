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
    ///     Provides static utility functions for hashing files. 
    ///     Supports SHA256, SHA512, HMAC and MD5 hashing
    /// </summary>
    public class Hasher
    {
        static ILogger logger;

        public enum HashType
        {
            SHA256, 
            SHA512,
            HMAC,
            MD5
        }

        public static void SetLogger(ILogger logger)
        {
            Hasher.logger = logger;
        }

        private static string HashFileGeneric(string filename, HashAlgorithm hasher, in ActionResult<bool> result)
        {
            string hash = "";

            using (FileStream stream = new FileStream(filename, FileMode.Open)) {
                try {
                    stream.Position = 0;
                    var hashArr = hasher.ComputeHash(stream);
                    hash = BitConverter.ToString(hashArr);
                }
                catch (IOException ex) {
                    string msg = $"Error reading file {filename} for hashing";
                    logger.LogWarning(ex, msg);
                    result.AddError(ErrorType.Path, msg);
                }
                catch (UnauthorizedAccessException ex) {
                    string msg = $"Cannot access {filename} for hashing";
                    logger.LogWarning(ex, msg);
                    result.AddError(ErrorType.Access, msg);
                }
                catch (Exception ex) {
                    string msg = $"Could not hash {filename} due to unknown exception";
                    logger.LogWarning(ex, msg);
                    result.AddError(ErrorType.Path, msg);
                }
            }

            return hash;
        }

        public static string HashFile(string filename, in ActionResult<bool> result, HashType type = HashType.SHA256)
        {
            string hash = "";

            if (type == HashType.SHA256) {
                using (SHA256 hasher = SHA256.Create()) {
                    hash = HashFileGeneric(filename, hasher, in result);
                }
            } else if (type == HashType.SHA512) {
                using (SHA512 hasher = SHA512.Create()) {
                    hash = HashFileGeneric(filename, hasher, in result);
                }
            } else if (type == HashType.HMAC) {
                using (HMAC hasher = HMAC.Create()) {
                    hash = HashFileGeneric(filename, hasher, in result);
                }
            } else if (type == HashType.MD5) {
                using (MD5 hasher = MD5.Create()) {
                    hash = HashFileGeneric(filename, hasher, in result);
                }
            }

            result.SetResult(!result.HasError());

            return hash;
        }
    }
}
