namespace E621Manager
{
    using e621lib;
    using System.Net;
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
                errorHandler(0);
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
            requestMessage.Headers.Remove("Authorization");
            string? encodedCreds = encodeBase(user + ":" + apiKey);
            if (encodedCreds != null)
            {
                requestMessage.Headers.Add("Authorization", "Basic " + encodedCreds);
            }
            else requestMessage.Headers.Add("Authorization", "Basic Missing");
            requestMessage.Headers.Remove("User-Agent");
            requestMessage.Headers.Add("User-Agent", "E621 helper programm . Contact " + user + " for using this . development by Olde16");
            requestMessage.Version = httpClient.DefaultRequestVersion;
            requestMessage.VersionPolicy = httpClient.DefaultVersionPolicy;

            // code
            Console.WriteLine("Thanks for using this! What to do now?");
            Console.WriteLine("All operations use the E621.net API. Therefore it is required that you are connected to the Internet!");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Be aware: All operations are irreversable! Do not use a command of which you are not sure of the consequences!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("###### --- Standard Operations --- ######");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("These options do not do anything to your local machine:");
            Console.WriteLine("1  - Get something... (number of posts favorited, number of posts on E621, etc.) [Not implemented yet]");
            Console.WriteLine("2  - Set something... [Not implemeted yet]");
            Console.WriteLine("3  - Compare something... (you have more of, less of, etc.) [Not implemeted yet]");
            Console.WriteLine("4  - Statistics overview... (E621 statistics that let your jaw drop) [Not implemented yet]");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("###### --- Affecting Operations --- ######");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("These might alter your file system depending on what you choose. Use with caution:");
            Console.WriteLine("68 - Sort something... (for artist, character, etc.) [Not implemented yet]");
            Console.WriteLine("69 - Download something... (posts, collections, all of something) [Not implemeted yet]");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("###### --- DANGER ZONE --- ######");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("These options should be used with caution and thorough consideration:");
            Console.WriteLine("99 - Delete something... (something that you're the owner of, favorites, upvotes, etc) [Not implemented yet]");
            Console.WriteLine();

            Console.Write("Select the number of your choice: ");
            string? input = Console.ReadLine();



            //e621_Api(E621_PATHS.FAVORITES, HttpMethod.Get, 0, ["q=dragon"]); // finally got it working
            //HttpResponseMessage respMsg = await httpClient.SendAsync(requestMessage);
            //if (respMsg != null)
            //{
            //    Console.WriteLine(requestMessage.ToString());
            //    Console.WriteLine(HttpStatusCode.OK == respMsg.StatusCode);
            //    Console.WriteLine(respMsg.ToString());
            //}

            // end
            Console.ReadKey();
        }


        #region e621_API

        public static void rebuildUri(string path, HttpMethod method, string[]? vals = null)
        {
            if (vals != null)
            {
                uriBuilder.Query = string.Join('&', vals);
            }
            uriBuilder.Path = path;
            requestMessage.RequestUri = uriBuilder.Uri;
            requestMessage.Method = method;
        }
        public static void e621_Api(E621_PATHS path, HttpMethod httpm, long id = 0, string[]? query_strings = null) 
        {
            if (httpm == HttpMethod.Put || httpm == HttpMethod.Delete || httpm == HttpMethod.Patch)
            {
                if (id <= 0) { errorHandler(1); } // requires an id be provided
                switch (path)
                {
                    case E621_PATHS.POST:
                        rebuildUri(string.Format("/posts/{0}.json",id), httpm, query_strings);
                        break;
                    case E621_PATHS.FAVORITES:
                        rebuildUri(string.Format("/favorites/{0}.json", id), httpm, query_strings);
                        break;
                    case E621_PATHS.NOTES:
                        rebuildUri(string.Format("/notes/{0}.json", id), httpm, query_strings);
                        break;
                    case E621_PATHS.POOLS:
                        rebuildUri(string.Format("/pools/{0}.json", id), httpm, query_strings);
                        break;
                    default:
                        errorHandler(3);
                        break;
                }
            } else if (httpm == HttpMethod.Post || httpm == HttpMethod.Get)
            {
                switch (path)
                {
                    case E621_PATHS.POST:
                        rebuildUri("/posts.json", httpm, query_strings);
                        break;
                    case E621_PATHS.FAVORITES:
                        rebuildUri("/favorites.json", httpm, query_strings);
                        break;
                    case E621_PATHS.NOTES:
                        rebuildUri("/notes.json", httpm, query_strings);
                        break;
                    case E621_PATHS.POOLS:
                        rebuildUri("/pools.json", httpm, query_strings);
                        break;
                    case E621_PATHS.UPLOADS:
                        rebuildUri("/posts.json", httpm, query_strings);
                        break;
                    case E621_PATHS.FLAGS:
                        rebuildUri("/post_flags.json", httpm, query_strings);
                        break;
                    case E621_PATHS.VOTES:
                        if (id <= 0) { errorHandler(1); break; } // requires id (why the hell not use Put??)
                        rebuildUri(string.Format("/posts/{0}/votes.json",id), httpm, query_strings);
                        break;
                    default:
                        errorHandler(3);
                        break;
                }
            } else // method not supported
            {
                errorHandler(2);
            }
        }
        public enum E621_PATHS : uint
        {
            POST = 0,
            FAVORITES = 1,
            NOTES = 2,
            POOLS = 3,
            UPLOADS = 4,
            FLAGS = 5,
            VOTES = 6,
        }

        #endregion

        #region Encoding

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

        #endregion

        #region ErrorHandling

        public static void errorHandler(int err)
        {
            if (err > 0)
            {
                switch (err)
                {
                    case 0:
                        throw new Exception("Provide at least a username and API key!");
                    case 1:
                        throw new Exception("The requested HttpMethod requires an id to be provided!");
                    case 2:
                        throw new Exception("The requested HttpMethod is not supported!");
                    case 3:
                        throw new Exception("The provided path is invalid!");
                    default: 
                        throw new Exception("Unhandled Error Exception");
                }
            }
            else throw new Exception("Unknown Error Exception");
        }

        #endregion

    }
}
