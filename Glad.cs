using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.IO;

namespace Global.Apiary.Documentation
{
    public class Glad
    {
        private static XDocument swaggerXmlSource { get; set; }
        private static string blueprintDocument;

        /// <summary>
        /// Configurations if needed 
        /// Most likely something like this will be used in the final implementation. 
        /// This is not implemented here and a fake key is provided for reference
        /// </summary>
        public static IConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json",
                optional: true,
                reloadOnChange: true);

            return builder.Build();
        }
        /// <summary>
        /// Entry Point - Checking to see if the source is a URL or YAML
        /// </summary>
        /// <param name="sourceUrl"></param>
        /// <param name="destinationUrl"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public string BeginRequests(string source, string destinationUrl, string title, string description, string auth)
        {
            bool isYaml = source.EndsWith(".yaml");
            
            if (isYaml)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(source);
                // execute the request
                HttpWebResponse response = (HttpWebResponse) req.GetResponse();
                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();
                var yamlString = new StreamReader(resStream).ReadToEnd();
                swaggerXmlSource = XDocument.Parse(JsonConvert.DeserializeXmlNode(YamlConverter.YamlToJson(yamlString), "root").OuterXml);
            }
            else
            {
                swaggerXmlSource = Request.getSwaggerJson(source);
            }
            ConvertToBlueprintDoc(title, description);
            return Request.PostToApiary(destinationUrl, blueprintDocument, auth);
        }

        /// <summary>
        /// Starting conversion - top level items like title and description
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        private void ConvertToBlueprintDoc(string title, string description)
        {
            // start construction of BluePrint API string
            blueprintDocument = @"FORMAT: 1A\n";
            blueprintDocument += $@"HOST:{swaggerXmlSource.Descendants("host").First().Value}\n\n# {title}\n\n {description}\n\n";

            // getting grouped level information
            var groups = swaggerXmlSource.Descendants("tags").GroupBy(g => g.Value).ToList();
            createCollections(groups);          
        }
        /// <summary>
        /// Creates collections of (groups) of items to be processed
        /// </summary>
        /// <param name="groups"></param>
        private void createCollections(List<IGrouping<string, XElement>> groups)
        {
            // get collection from grouped items and create Blueprint
            foreach (var g in groups)
            {
                blueprintDocument += $@"# Group {g.Key}\n\n";
                blueprintDocument += $@"## Collection [{g.Key}]\n\n";
                // iterate individual items
                foreach (var i in g)
                {
                    var _parent = i.Parent;
                    // method
                    var _method = _parent.Name.ToString().ToUpper();
                    var _path = XmlConvert.DecodeName(i.Parent.Parent.Name.ToString());
                    var _summary = ((XElement)i.NextNode).Value;
                    var _description = _parent.Element("description")?.Value.ToString();
                    var _parameters = _parent.Descendants("parameters").ToList();

                    // getting response obj reference and code
                    // loading all definitions so that i don't have to re convert them to json since i have them already
                    var _responseContentType = _parent.Descendants("produces").First().Value;
                    var _responseNameSpace = XName.Get("ref", "http://james.newtonking.com/projects/json");
                    var _responseCode = XmlConvert.DecodeName(((XElement)_parent.Descendants("responses").First().FirstNode).Name.ToString());
                    string[] _responseNodeNameArray = getResponseTypeName(_parent, _responseNameSpace);
                    var  _response = i.Document.Root.Descendants(_responseNodeNameArray[0]).Descendants(_responseNodeNameArray[1]);
                    // creating json object for either response or request use
                    var jsonResult = createJsonFromXElement(_response);
                    blueprintDocument += $@"### {_summary} [{_method} {_path}]\n\n";
                    // adding optional description
                    if (!string.IsNullOrEmpty(_description))
                    {
                        blueprintDocument += $@">{_description}\n\n";
                    }
                    // building parameter list
                    if (_parameters.Any())
                    {
                        blueprintDocument += $@"+ Parameters\n";
                        var paramType = _parameters.First().Element("in")?.Value;

                        foreach (var p in _parameters)
                        {
                            if (paramType != "body")
                            {
                                var _type = p.Descendants("type").First().Value;
                                var _value = p.Descendants("name").First().Value;

                                string _example;
                                if (_type == "integer")
                                {
                                    _example = "123";
                                }
                                else if (_type == "bool")
                                {
                                    _example = "false";
                                }
                                else
                                {
                                    _example = "abc";
                                }

                                blueprintDocument += $@"    + {_value}: {_example} ({_type})\n";
                            }
                            else
                            {
                                blueprintDocument += $@"+ Request {_responseCode} ({_responseContentType})\n\n{jsonResult}\n\n";
                            }
                        }
                    }
                    blueprintDocument += $@"+ Response {_responseCode} ({_responseContentType})\n\n{jsonResult}\n\n";
                }
            }
        }
        #region REQEST & RESPONSE OBJECT CONVERSIONS
        /// <summary>
        /// Gets the referenced response types names
        /// </summary>
        /// <param name="element"></param>
        /// <param name="responseNamespace"></param>
        /// <returns></returns>
        private string[] getResponseTypeName(XElement element, XName responseNamespace)
        {
            var schemaHasAttributes = element.Descendants("schema").Attributes(responseNamespace).Any();
            var itemHasAttributes = element.Descendants("items").Attributes(responseNamespace).Any();
            if (schemaHasAttributes)
            {
                return element.Descendants("schema").Attributes(responseNamespace)
                                            .First().Value.Replace("#/", "").Split('/');
            }
            else if (itemHasAttributes)
            {
                return element.Descendants("items").Attributes(responseNamespace)
                                            .First().Value.Replace("#/", "").Split('/');
            }
            else
            {
                return new string[] { };
            }
        }
        /// <summary>
        /// Creating the individual response objects (JSON examples in Apiary)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string createJsonFromXElement(IEnumerable<XElement> element)
        {
            string twelveSpace = "            ";
            string sixteenSpaces = "                ";
            string result = $"{twelveSpace}{{ \\n";
            foreach (var el in element)
            {
                foreach (var e in el.Descendants("properties").Elements())
                {
                        string name = e.Name.LocalName;
                        string type = e.Descendants("type").Select(t => t.Value.ToString()).FirstOrDefault();
                    if (type != "array")
                    {
                        result += $"{sixteenSpaces}\\\"{name}\\\" : ";
                        switch (type)
                        {
                            case "integer":
                                result += "0,\\n";
                                break;
                            case "boolean":
                                result += $"true,\\n";
                                break;
                            default:
                                result += "\\\"sample\\\",\\n";
                                break;
                        }
                    }
                }
                result = result.Substring(0, result.LastIndexOf(",")); 
            }           
            result += $"\\n{twelveSpace}}}";
            return result;
        }
        #endregion

    
    }
}
