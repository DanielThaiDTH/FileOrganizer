using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileOrganizerCore
{
    /// <summary>
    ///     Loads configuration from a file in the same path as the 
    ///     executable.
    /// </summary>
    public class ConfigLoader
    {
        XPathDocument configFile;
        ILogger logger;

        public ConfigLoader(string filename, ILogger logger)
        {
            this.logger = logger;
            logger.LogInformation("Loading config from " + filename);
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
            logger.LogDebug("Location: " + path);
            configFile = new XPathDocument(Path.Combine(path, filename));
        }

        /// <summary>
        ///     Returns the value of of the first node with the given name.
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public string GetNodeValue(string nodeName)
        {
            logger.LogDebug($"Loading {nodeName} in configuration");
            var XMLNav = configFile.CreateNavigator();
            var nodeList = XMLNav.Select("//" + nodeName);
            foreach (XPathNavigator n in nodeList) {
                logger.LogDebug($"Loading value {n.Value} for {nodeName}");
                return n.Value;
            }
            return null;
        }
    }
}
