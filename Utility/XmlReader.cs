using System.IO;
using System.Text.RegularExpressions;

namespace Utility
{
    /// <summary>
    /// Allows to read content from xml files
    /// </summary>
    public static class XmlParser
    {
        /// <summary>
        /// Returns xml content from file
        /// </summary>
        /// <param name="directory">Path to xml file</param>
        /// <returns>Xml context from file</returns>
        public static string GetXmlContent(string directory)
        {
            string xmlContent;
            using (StreamReader xmlReader = new StreamReader(directory))
                xmlContent = xmlReader.ReadToEnd();
            return xmlContent;
        }

        /// <summary>
        /// Returns port value from xml
        /// </summary>
        /// <param name="xml">Xml content</param>
        /// <returns>Port from xml file</returns>
        public static int GetPort(string xml)
        {
            Regex portRegex = new Regex("[\\s\\S]*<port>(?<port>\\d*?)<.port>[\\s\\S]*");
            int port = int.Parse(portRegex.Match(xml).Groups["port"].Value ?? "10800");
            return port;
        }

        /// <summary>
        /// Returns path value from xml
        /// </summary>
        /// <param name="xml">Xml content</param>
        /// <returns>Path from xml file</returns>
        public static string GetPath(string xml)
        {
            Regex pathRegex = new Regex("[\\s\\S]*<directory>(?<directory>[\\s\\S]*?)<.directory>[\\s\\S]*");
            string path = pathRegex.Match(xml).Groups["directory"].Value;
            return path;
        }

        /// <summary>
        /// Returns host value from xml
        /// </summary>
        /// <param name="xml">Xml content</param>
        /// <returns>Host value of the server</returns>
        public static string GetHost(string xml)
        {
            Regex hostRegex = new Regex("[\\s\\S]*<host>(?<host>[\\s\\S]*?)<.host>[\\s\\S]*");
            string host = hostRegex.Match(xml).Groups["host"].Value;
            return host;
        }
    }
}
