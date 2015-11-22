using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BiisControllers;

namespace Utility
{
    /// <summary>
    ///     The class incapsulate main BL of working 
    /// with html files on disk and parsing request
    /// </summary>
    public class HtmlTools
    {
        private const string HttpHeader = "HTTP/1.1 200 OK\r\nServer: Biis" +
                                      "\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n";

        /// <summary>
        /// Parses request string and creating HttpRequest object
        /// </summary>
        /// <param name="content">Http request string</param>
        /// <returns>HttpRequest - representation of request</returns>
        public HttpRequest ParseRequest(string content)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            HttpRequest request = new HttpRequest
            {
                PostRequestParams = new Dictionary<string, string>()
            };
            string[] rows = content.Split('\r', '\n');

            //Regex to find pairs name: value in http request header
            Regex requestRegex = new Regex("[\\s]*(?<name>[\\w-._]*):[\\s]*(?<value>[\\w\\S]*)[\\s]*");
            //Regex to parse first line of request
            Regex firstLineRegex = new Regex("(?<verb>[A-Z]*)[\\s]*(?<path>[\\/\\.\\-\\w]*)[\\s\\S]*");
            //Regex to parse post parameters
            Regex postParametersRegex = new Regex("(?<name>[\\w\\W]*)=(?<value>[\\S\\s]*)");

            Match lineMatch;
            string propertyName;
            string propertyValue;

            try
            {
                //Parsing the first line to fing VERB and URI PATH
                var firstLineMatch = firstLineRegex.Match(rows[0]);
                request.Verb = firstLineMatch.Groups["verb"].Value;
                request.Path = firstLineMatch.Groups["path"].Value;

                //Parsing other lines to find other properties
                foreach (var line in rows.Where(line => requestRegex.IsMatch(line)))
                {
                    lineMatch = requestRegex.Match(line);
                    propertyName = lineMatch.Groups["name"].Value;
                    propertyValue = lineMatch.Groups["value"].Value;
                    properties.Add(propertyName, propertyValue);
                }

                if (properties.ContainsKey("Accept")) request.Accept = properties["Accept"];
                if (properties.ContainsKey("Connection")) request.Connection = properties["Connection"];
                if (properties.ContainsKey("Host")) request.Host = properties["Host"];

                //Parsing POST request string
                if (string.Equals(request.Verb, "POST", StringComparison.CurrentCultureIgnoreCase))
                {
                    string[] postParmeters = rows[rows.Length - 1].Split('&');

                    foreach (string postParmeter in postParmeters)
                    {
                        lineMatch = postParametersRegex.Match(postParmeter);
                        propertyName = lineMatch.Groups["name"].Value;
                        propertyValue = lineMatch.Groups["value"].Value;
                        request.PostRequestParams.Add(propertyName, propertyValue);
                    }
                }
            }
            catch (Exception exc)
            {
                return null;
            }
            return request;
        }

        /// <summary>
        /// Generate response with 404 code
        /// </summary>
        /// <returns>Html representation of not found page</returns>
        public string GetNotFoundResponse()
        {
            return string.Format("HTTP/1.0 404 Not Found\r\nServer: Biis" +
                                 "\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n" +
                                 "<html><body><h1>404 Page not found!</h1></body></html>");
        }

        /// <summary>
        /// Generate response with 500 code
        /// </summary>
        /// <returns>Html representation of internal server error page</returns>
        public string GetInternalServerErrorResponse()
        {
            return string.Format("HTTP/1.0 500 Internal Server Error\r\nServer: Biis" +
                                 "\r\nConnection: close\r\nContent-Type: text/html\r\n\r\n" +
                                 "<html><body><h1>500 Internal Server Error</h1></body></html>");
        }

        /// <summary>
        /// Parses html page and creates collection of biis controls from it
        /// </summary>
        /// <param name="path">URI path</param>
        /// <param name="directory">Directory on disk</param>
        /// <param name="request">Http request</param>
        /// <returns>Collection of all biis controls frm document</returns>
        public Dictionary<string, IBiisControl> GetControlsCollection(string path, string directory, HttpRequest request)
        {
            Dictionary<string, IBiisControl> controls = new Dictionary<string, IBiisControl>();
            
            //Regex to find <biis: tag, find control type, its attributes and content
            Regex biisTagRegex = new Regex(
                    "[\\s\\S]*<biis:(?<controll>[\\w]*)\\s((?:[\\w]*)=[\"\'](?:[^\"]*?['\"]|[^']*['\"])\\s*)(>(?<content>[\\s\\S]*?)<.biis:\\k<controll>>|.>)[\\s\\S]*",
                    RegexOptions.IgnoreCase);
            //Regex to parse attrib="value" html attributes
            Regex attributeNameValueRegex = new Regex("(?<name>[\\w]*)=\"(?<value>[\\S\\s]*)\"");

            try
            {
                using (StreamReader streamReader =
                        new StreamReader(new FileStream(@".\" + directory + path, FileMode.Open)))
                {
                    var rawString = streamReader.ReadToEnd();
                    var strings = rawString.Split('\r', '\n');

                    foreach (var line in strings.Where(s => biisTagRegex.IsMatch(s)))
                    {
                        var regexMatch = biisTagRegex.Match(line);
                        var controlType = regexMatch.Groups["controll"].Value;
                        var content = regexMatch.Groups["content"].Value;
                        var parameters = new Dictionary<string, string>();

                        var properties = regexMatch.Groups[1].Value.Split(' ');
                        foreach (var property in properties)
                        {
                            regexMatch = attributeNameValueRegex.Match(property);
                            var name = regexMatch.Groups["name"].Value;
                            var value = regexMatch.Groups["value"].Value.Trim('\"', '\'');
                            parameters.Add(name, value);
                        }

                        IBiisControl control;
                        switch (controlType)
                        {
                            case "Button":
                                control = new Button(parameters["value"]);
                                if (parameters.ContainsKey("onclick"))
                                    control.HandlerFunction = parameters["onclick"];
                                break;
                            case "Label":
                                control = string.IsNullOrEmpty(content) ? new Label(parameters["value"]) : new Label(content);
                                break;
                            case "TextBox":
                                control = new TextBox(content);
                                string name = parameters["name"];
                                if (request.PostRequestParams.ContainsKey(name))
                                    control.Value = request.PostRequestParams[name];
                                break;
                            default:
                                throw new Exception();
                        }

                        if (parameters.ContainsKey("id")) control.Id = parameters["id"];
                        if (parameters.ContainsKey("name")) control.Name = parameters["name"];
                        if (parameters.ContainsKey("style")) control.Style = parameters["style"];
                        control.RawHtmlView = line;
                        controls.Add(control.Name, control);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Internal exception occured");
                return null;
            }
            return controls;
        }

        /// <summary>
        /// Checks whether requested page is static
        /// </summary>
        /// <param name="request">Http request</param>
        /// <returns></returns>
        public bool IsStaticPage(HttpRequest request)
        {
            return request.Path.Contains(".html");
        }

        /// <summary>
        /// Cheks whether the page exists on disk
        /// </summary>
        /// <param name="path">URI path</param>
        /// <param name="directory">Directory on disk</param>
        /// <returns></returns>
        public bool PageExist(string path, string directory)
        {
            FileInfo fileInfo = new FileInfo(@".\" + directory + path);
            return fileInfo.Exists;
        }

        /// <summary>
        /// Create http OK summary response with static html body
        /// </summary>
        /// <param name="path">URI path</param>
        /// <param name="directory">Directory on disk</param>
        /// <returns>Summary http 200 OK response</returns>
        public string ResponseForStaticPage(string path, string directory)
        {
            string htmlString;
            using (StreamReader streamReader =
                new StreamReader(new FileStream(@".\" + directory + path, FileMode.Open)))
                htmlString = streamReader.ReadToEnd();
            return HttpHeader + htmlString;
        }

        /// <summary>
        /// Create http OK summary response for .biis page
        /// </summary>
        /// <param name="path">URI path</param>
        /// <param name="directory">Directory on disk</param>
        /// <param name="request">Http request</param>
        /// <returns>Summary http 200 OK response</returns>
        public string ResponseForDynamicPage(string path, string directory, HttpRequest request)
        {
            string htmlString;

            var controlsCollection = GetControlsCollection(path, directory, request);

            if (string.Equals(request.Verb, "POST"))
                controlsCollection = Compiler.ExecuteBiisCode(controlsCollection, path, directory);

            using (StreamReader streamReader =
                new StreamReader(new FileStream(@".\" + directory + path, FileMode.Open)))
                htmlString = streamReader.ReadToEnd();

            htmlString = controlsCollection.Values.Aggregate(htmlString, (current, biisControl) 
                => current.Replace(biisControl.RawHtmlView, biisControl.ToHtmlString()));

            return HttpHeader + htmlString;
        }
    }
}