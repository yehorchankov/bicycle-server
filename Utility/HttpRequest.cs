using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// Incapsulates main fields from HTTP request
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Verb word which determs the type of request
        /// </summary>
        public string Verb { get; set; }
        /// <summary>
        /// URI path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Request's host
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// State of connection
        /// </summary>
        public string Connection { get; set; }
        /// <summary>
        /// Acceptable types of content
        /// </summary>
        public string Accept { get; set; }
        /// <summary>
        /// Post request parameters
        /// </summary>
        public Dictionary<string, string> PostRequestParams { get; set; }
    }
}