namespace E621Manager
{
    using e621lib;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    internal class Program
    {
        static void Main(string[] args)
        {
            // preperations
            if (args.Length == 0)
            {
                throw new Exception("Provide at least a username and API key!");
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
            HttpClient httpClient = new HttpClient();
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = "e621.net";
            httpClient.BaseAddress = uriBuilder.Uri;
            httpClient.Timeout = new TimeSpan(0, 0, 6);
            httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
            HttpRequestMessage reqMsg = new HttpRequestMessage();
            reqMsg.Headers.Clear();
            reqMsg.Headers.Concat(httpClient.DefaultRequestHeaders);
            reqMsg.Headers.Remove("Authorisation");
            string? encodedCreds = encodeBase(user + ":" + apiKey);
            if (encodedCreds != null)
            {
                reqMsg.Headers.Add("Authorisation", "Basic " + encodedCreds);
            }
            else reqMsg.Headers.Add("Authorisation", "Basic Missing");
            reqMsg.Headers.Remove("User-Agent");
            reqMsg.Headers.Add("User-Agent","E621 helper programm -- contact " + user + " for using this -- development by Olde16");


            // code

            // end
            Console.ReadKey();
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
    }
}
