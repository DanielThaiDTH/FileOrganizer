using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SymLinkMaker
{
    public class SymLinkMaker
    {
        ILogger logger;
        /* WIN32 API imports/definitions section */
        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        enum FileAttribute
        {
            FILE_ATTRIBUTE_REPARSE_POINT = 0x400
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
        
        [DllImport("kernel32.dll", EntryPoint = "GetFileAttributesW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "DeleteFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool DeleteFile(string lpFileName);

        /* End of win32 imports*/

        
        static HashSet<Regex> forbiddenRootPattern = new HashSet<Regex>() { 
            new Regex(@"^\w:\\$", RegexOptions.Compiled|RegexOptions.IgnoreCase),
            new Regex(@".*\\system32", RegexOptions.Compiled|RegexOptions.IgnoreCase),
            new Regex(@".*\\Windows", RegexOptions.Compiled|RegexOptions.IgnoreCase)
        };

        string root;
        public string Root { get { return root; } }
        HashSet<string> symLinks;

        static bool allowableRoot(string path)
        {
            bool match = false;

            foreach (Regex r in forbiddenRootPattern) {
                if (match) break;
                match = r.IsMatch(path);
            }

            return !match;
        }


        /// <summary>
        ///     Creates class using given root as the location of where the symbolic links go.
        ///     Will throw ArguementException if the root string is not an absolute path.
        /// </summary>
        /// <param name="root">
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public SymLinkMaker(string root, ILogger logger)
        {
            if (!Path.IsPathRooted(root)) {
                logger.LogError("{0} is not an absolute path", root);
                throw new ArgumentException(root + " is not an absolute path");
            } else if (!allowableRoot(root)) {
                logger.LogError("{0} is not an allowable path", root);
                throw new ArgumentException(root + " is not an allowable root");
            }

            logger.LogInformation("Creating SymLinkMaker with root {0}", root);
            symLinks = new HashSet<string>();
            this.logger = logger;
            this.root = root;
        }

        /// <summary>
        ///     Creates a symlink at the path variable, using the target source in the source variable.
        ///     Set if it is a directory or not in the third parameter.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="source"></param>
        /// <param name="isDirectory"></param>
        /// <returns>
        ///     Boolean indicating symlink creation status
        ///</returns>
        public bool Make(string path, string source, bool isDirectory)
        {
            if (path is null || source is null) {
                logger.LogWarning("Symlink path or file source is null");
                return false;
            } else if (!Path.IsPathRooted(source)) {
                logger.LogWarning("{0} is not a rooted path, cannot use as source", source);
                return false;
            } else if (Path.IsPathRooted(path)) {
                logger.LogWarning("{0} is a rooted path, cannot use as symlink folder", path);
                return false;
            }

            logger.LogInformation("Creating symbolic link at {0} with source {1}", Path.Combine(root, path), source);
            int result = CreateSymbolicLink(Path.Combine(root, path), source, isDirectory ? SymbolicLink.Directory : SymbolicLink.File);
            if (result == 1) {
                logger.LogInformation("Symbolic link created at {0}", Path.Combine(root, path));
                symLinks.Add(path);
                return true;
            }

            int err = Marshal.GetLastWin32Error();
            logger.LogWarning("CreateSymbolicLink failed with error code of {0}", err);
            return false;
        }

        /// <summary>
        ///     Returns an array of symlink strings. 
        /// </summary>
        /// <returns>Array of symlinks (relative to root)</returns>
        public string[] GetSymLinks()
        {
            string[] values = new string[symLinks.Count];
            symLinks.CopyTo(values);
            logger.LogDebug("Returning list of known symlinks in {0}, count={1}", root, symLinks.Count);

            return values;
        }

        /// <summary>
        ///     Checks if a given file is a sym link. Returns false if not a relative path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Boolean indicating if the given path is a symlink or not</returns>
        public bool IsSymLink(string path)
        {
            if (path is null) {
                logger.LogWarning("IsSymLink called with null path");
                return false;
            } else if (Path.IsPathRooted(path)) {
                logger.LogWarning("IsSymLink failed because a root path was given");
                return false;
            }

            uint result = GetFileAttributes(Path.Combine(root, path));
            if ((result & (uint) FileAttribute.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
                logger.LogInformation("{0} is a symlink", path);
                return true;
            } else {
                logger.LogInformation("{0} is not a symlink", path);
                return false;
            }
        }

        /// <summary>
        ///     Clears all symlinks from the root folder. Only deletes symlinks
        ///     an instance of this class knows.
        /// </summary>
        /// <returns></returns>
        public int ClearExisting()
        {
            int count = 0;
            logger.LogInformation("Clearing all symlinks in {0}", root);

            foreach (string sl in symLinks) {
                logger.LogDebug("Deleting {0}", sl);
                if (DeleteFile(Path.Combine(root, sl))) {
                    logger.LogDebug("Deleted {0}", sl);
                    count++;
                    logger.LogTrace("{0} items deleted", count);
                } else {
                    int err = Marshal.GetLastWin32Error();
                    logger.LogWarning("Could not delete {0} due to error code {1}", sl, err);
                }
            }

            symLinks.Clear();
            logger.LogInformation("{0} symlinks cleared", count);

            return count;
        }

        /// <summary>
        ///     Deletes a sym link in the folder managed by an instance of this class.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Boolean indicating if the symlink at the given path was deleted</returns>
        public bool ClearLink(string path)
        {
            bool result = false;
            if (IsSymLink(path)) {
                logger.LogDebug("{0} is a symlink, deleting", path);
                result = DeleteFile(Path.Combine(root, path));
            }

            if (result && symLinks.Contains(path)) {
                symLinks.Remove(path);
                logger.LogInformation("{0} in {1} deleted", path, root);
            } else {
                int err = Marshal.GetLastWin32Error();
                logger.LogWarning("Delete of {0} failed due to error code {1}", path, err);
            }

            return result;
        }
    }
}
