using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace Global.Apiary.Documentation
{
    public static class Request
    {
        #region GENERIC CALLS (REQUETS & RESPONSES)
        /// <summary>
        /// Calling for source Swagger JSON
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static XDocument getSwaggerJson(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var s = client.GetStringAsync(url);
                    var response = client.GetStringAsync(url).Result;
                    string withRoot = $@"{{ swaggerXml : '{response}'}}";
                    return XDocument.Parse(JsonConvert.DeserializeXmlNode(response, "root").OuterXml);
                }
                catch (HttpRequestException ex)
                {
                    return XDocument.Parse($"{{ \"message\" : \"{ex.Message}\" }}");
                }
            }
        }
        /// <summary>
        /// Posting completed Blueprint API document
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string PostToApiary(string url, string blueprintDocument, string auth)
        {
           
            using (var client = new HttpClient())
            {
                var s = $"{{ \"code\": \"{blueprintDocument}\" }}";
                var content = new StringContent($"{{ \"code\": \"{blueprintDocument}\" }}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.Add("Authentication", $"Token {auth}"); // this is to be replaced with a proper Apiary API key
                var response = client.PostAsync(url, content).Result.StatusCode;
                var message = "";
                switch (response)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                        message = $"{{ \"code\" : {(int)System.Net.HttpStatusCode.BadRequest},\n\"message\" : \"{System.Net.HttpStatusCode.BadRequest}\",\n \"reason\" : \"There maybe something wrong with your Blueprint.\" }}";
                        break;
                    case System.Net.HttpStatusCode.Created:
                        message = $"{{ \"code\" : {(int)System.Net.HttpStatusCode.Created},\n\"message\" : \"{System.Net.HttpStatusCode.Created}\",\n \"reason\" : \"Your request was successful and the document was updated.\" }}";
                        break;
                    case System.Net.HttpStatusCode.InternalServerError:
                        message = $"{{ \"code\" : {(int)System.Net.HttpStatusCode.InternalServerError},\n\"message\" : \"{System.Net.HttpStatusCode.InternalServerError}\",\n \"reason\" : \"Something went wrong on your target server.\" }}";
                        break;
                    case System.Net.HttpStatusCode.OK:
                        message = $"{{ \"code\" : {(int)System.Net.HttpStatusCode.OK},\n\"message\" : \"{System.Net.HttpStatusCode.OK}\",\n \"reason\" : \"The request was successful but no changes where made because the document has no differences.\" }}";
                        break;
                    default:
                        break;
                }
                return message;
            }
        }
        #endregion
    }
}
