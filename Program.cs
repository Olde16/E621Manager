namespace E621Manager
{
    using e621lib;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Reflection.Metadata;
    using System.Text;

    internal class Program
    {
        static HttpClient httpClient = new HttpClient();
        static HttpRequestMessage requestMessage = new HttpRequestMessage();
        static UriBuilder uriBuilder = new UriBuilder();

        static async Task Main(string[] args)
        {
            // preperations
            if (args.Length == 0)
            {
                
            }

            string? user = args[0];
            string? apiKey = args[1];
            string? dir_source = string.Empty;
            string? dir_target = string.Empty;
            if (args.Length > 2)
            {
                dir_source = args[2];
            }
            if (args.Length > 3)
            {
                dir_target = args[3];
            }
            User e621User = new User();
            Page page = new Page();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = "e621.net";
            httpClient.BaseAddress = uriBuilder.Uri;
            httpClient.Timeout = new TimeSpan(0, 0, 6);
            httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
            requestMessage.Headers.Clear();
            requestMessage.Headers.Concat(httpClient.DefaultRequestHeaders);
            requestMessage.Headers.Remove("Authorisation");
            string? encodedCreds = encodeBase(user + ":" + apiKey);
            if (encodedCreds != null)
            {
                requestMessage.Headers.Add("Authorisation", "Basic " + encodedCreds);
            }
            else requestMessage.Headers.Add("Authorisation", "Basic Missing");
            requestMessage.Headers.Remove("User-Agent");
            requestMessage.Headers.Add("User-Agent","E621 helper programm -- contact " + user + " for using this -- development by Olde16");
            requestMessage.Version = httpClient.DefaultRequestVersion;
            requestMessage.VersionPolicy = httpClient.DefaultVersionPolicy;

            // code

            //test

            rebuildUri("/favorites.json", HttpMethod.Delete, ["post_id=6321858"]);
            HttpResponseMessage respMsg = await httpClient.SendAsync(requestMessage);
            if (respMsg != null)
            {
                Console.WriteLine(requestMessage.ToString());
                Console.WriteLine(HttpStatusCode.OK == respMsg.StatusCode);
                Console.WriteLine(respMsg.ToString());
            }

            // end
            Console.ReadKey();
        }
        public static void rebuildUri(string uri, HttpMethod method, string[]? vals = null)
        {
            if (vals != null)
            {
                uriBuilder.Query = string.Join('&', vals);
            }
            uriBuilder.Path = uri;
            requestMessage.RequestUri = uriBuilder.Uri;
            requestMessage.Method = method;
        }
        public static string? encodeBase(string inp)
        {
            if (inp != null)
            {
                if (inp.Trim() != string.Empty)
                {
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(inp));
                }
            }
            return null;
        }
        public static void errorHandler(int err)
        {
            if (err > 0)
            {
                switch (err)
                {
                    case 0:
                        throw new Exception("Provide at least a username and API key!");
                    default: 
                        throw new Exception("Unhandled Error Exception");
                }
            }
            else throw new Exception("Unknown Error Exception");
        }
    }
}
