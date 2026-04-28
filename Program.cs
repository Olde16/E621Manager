namespace E621Manager
{
    using e621lib;
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.FileIO;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Reflection;
    using System.Reflection.PortableExecutable;
    using System.Text;
    using System.Text.Unicode;

    // class magic ( extending class Post by this to make life easier )
    public class Post:e621lib.Post
    {
        public object? this[string name] // set and get values by their string representation
        {
            get {
                Type t = GetType();
                PropertyInfo? i = t.GetProperty(name);
                if (i == null) return null;
                return i.GetValue(this, null);
            }
            set
            {
                Type t = GetType();
                PropertyInfo? i = t.GetProperty(name);
                if ( i == null) return;
                i.SetValue(this, value, null);
            }
        }
    }
    internal class Program
    {
        static HttpClient httpClient = new HttpClient();
        static HttpRequestMessage requestMessage = new HttpRequestMessage();
        static UriBuilder uriBuilder = new UriBuilder();

        static void Main(string[] args)
        {
            // preperations
            if (args.Length == 0)
            {
                errorHandler(0, null);
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

            // implement authorization test and error handling

            // code Main
            while (true)
            {
                printText_Start();
                Console.Write("Select the number of your choice: ");
                string? input = Console.ReadLine();

                if (input != null)
                {
                    input = input.Trim();
                    switch (input)
                    {
                        case "1": // get
                            enter_Context1();
                            break;
                        case "2": // set
                            enter_Context2();
                            break;
                        case "3": // compare
                            enter_Context3();
                            break;
                        case "4": // stats
                            enter_Context4();
                            break;
                        case "68": // sort 
                            enter_Context68();
                            break;
                        case "69": // download
                            enter_Context69();
                            break;
                        case "99": // delete
                            enter_Context99();
                            break;
                        default:
                            Console.WriteLine();
                            Console.WriteLine("Thats not an option!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Provide a valid number please!");
                }

                // end
                Console.ReadKey();
                Console.Clear();
            }
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
                if (id <= 0) { errorHandler(1, null); } // requires an id be provided
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
                        errorHandler(3, null);
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
                        if (id <= 0) { errorHandler(1, null); break; } // requires id (why the hell not use Put??)
                        rebuildUri(string.Format("/posts/{0}/votes.json",id), httpm, query_strings);
                        break;
                    default:
                        errorHandler(3, null);
                        break;
                }
            } else // method not supported
            {
                errorHandler(2, null);
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

        #region Encoding_and_compression

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

        public static void decompressGZ(FileStream fs,  string fileTo)
        {
            FileStream decomp = File.Create(fileTo);
            GZipStream gZip = new GZipStream(fs, CompressionMode.Decompress);
            gZip.CopyTo(decomp);

            gZip.Flush();
            gZip.Dispose();
            gZip.Close();
            decomp.Flush();
            decomp.Dispose();
            decomp.Close();
        }

        #endregion

        #region ErrorHandling

        public static void errorHandler(int err, Exception? inner)
        {
            if (err > 0)
            {
                if (inner == null) inner = new Exception("NoInner");
                switch (err)
                {
                    case 0:
                        throw new Exception("Provide at least a username and API key!", inner);
                    case 1:
                        throw new Exception("The requested HttpMethod requires an id to be provided!", inner);
                    case 2:
                        throw new Exception("The requested HttpMethod is not supported!", inner);
                    case 3:
                        throw new Exception("The provided path is invalid!", inner);
                    case 4:
                        throw new Exception("The HTTP Request failed with an unexpected status code", inner);
                    default: 
                        throw new Exception("Unhandled Error Exception", inner);
                }
            }
            else throw new Exception("Unknown Error Exception");
        }

        #endregion

        #region Context
        public static void printText_Start() // main menu
        {
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
        }

        public static void enter_Context1() // get
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("###### --- Get --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void enter_Context2() // set
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("###### --- Set --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void enter_Context3() // compare
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("###### --- Compare --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void enter_Context4() // stats
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("###### --- E621 Statistics --- ######");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.WriteLine("Displaying statistics... (this may take a while)");
            Console.WriteLine();

            // requires db dump for fast processing, api limits dont allow plain iteration across all posts
            // this is using the latest dump and a local temp file
            string currentDB = string.Empty;
            DateTime currentDateTime = DateTime.Now;
        IfDateFailed: // iterates back when the currentDateTime variable has lead to an error - changes the date to one day before
            currentDB = "posts-" + currentDateTime.ToString("yyyy-MM-dd") + ".csv.gz";
            uriBuilder.Path = "/db_export/" + currentDB;

            // add check for existing db

            Console.Write("Establishing connection... ");
            Task<Stream> respStream = httpClient.GetStreamAsync(uriBuilder.Uri);
            respStream.Wait();
            if (respStream.IsCompletedSuccessfully)
            {
                Console.WriteLine("Done!");
            } else
            {
                currentDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day - 1);
                if (!(currentDateTime.Day < DateTime.Now.Day - 5))
                {
                    goto IfDateFailed;
                }
                else
                    errorHandler(4, new ArgumentException("The provided download string returned an error.", new HttpRequestException(HttpRequestError.HttpProtocolError, "Resource not found!")));
            }

            if (respStream.Result != null)
            {
                if (respStream.Result.CanRead)
                {
                    Post[] posts = [];

                    string tempFilePath = Path.Combine(Path.GetTempPath(), "e621.csv.gz");
                    string tempFileDecompPath = Path.Combine(Path.GetTempPath(), "e621.csv");

                    Console.Write(string.Format( "Downloading {0} to {1}: ", uriBuilder.Uri.ToString(), tempFilePath) );
                    FileStream fs = File.OpenWrite(tempFilePath);
                    long len = fs.Length;
                    if (len < 0) errorHandler(4, new HttpIOException(HttpRequestError.InvalidResponse, "Content has length of 0 Bytes"));
                    Console.Write("expect up to " + Math.Ceiling((decimal)len / 1000000000) + " GB... ");
                    respStream.Result.CopyTo(fs);
                    respStream.Result.Flush();
                    respStream.Result.Dispose();
                    respStream.Result.Close();
                    fs.Flush();
                    fs.Dispose();
                    Console.WriteLine("Done!");
                    Console.Write(string.Format( "Decompressing archive {0}... ", tempFilePath) );
                    fs = File.OpenRead(tempFilePath);
                    decompressGZ(fs, tempFileDecompPath);
                    fs.Dispose();
                    fs.Close();
                    Console.WriteLine("Done!");

                    Console.Write("Reading Post DB... ");
                    posts = readPostsFromCSV(tempFileDecompPath);
                    Console.WriteLine("Done!");
                    Console.WriteLine();
                    Console.WriteLine("Statistics:");
                    Console.WriteLine(posts.Length);
                    foreach (var post in posts)
                    {
                        Debug.WriteLine(post.fav_count);
                    }
                    Console.WriteLine("Done!");
                } else
                {
                    errorHandler(4, new HttpIOException(HttpRequestError.InvalidResponse,"Not able to read stream."));
                }
            }
        }
        public static void enter_Context68() // sort
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("###### --- Sort --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void enter_Context69() // download
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("###### --- Download --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void enter_Context99() // delete
        {
            Console.Clear(); // clear for new context
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("###### --- DELETE --- ######");
            Console.ForegroundColor = ConsoleColor.White;
        }
        #endregion

        #region CSV
        public static Post[] readPostsFromCSV(string csvLocation)
        {
            List<Post> posts = new List<Post>();
            Post post = new Post();
            string[] descriptors = [];
            uint i = 0;

            var fs = File.OpenRead(csvLocation);

            foreach (var row in ReadCsv(fs))
            {
                if (i == 0)
                {
                    descriptors = row;
                    i++;
                    continue;
                }



                if (i == 200) break;

                i++;
            }

            return posts.ToArray();
        }

        public static IEnumerable<string[]> ReadCsv(Stream stream)
        {
            using var reader = new StreamReader(stream);

            var field = new StringBuilder();
            var row = new List<string>();

            bool inQuotes = false;

            while (true)
            {
                int raw = reader.Read();
                if (raw == -1) break;

                char c = (char)raw;

                if (c == '"')
                {
                    if (inQuotes && reader.Peek() == '"')
                    {
                        field.Append('"');
                        reader.Read();
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (c == '\r' && reader.Peek() == '\n')
                        reader.Read();

                    row.Add(field.ToString());
                    field.Clear();

                    yield return row.ToArray();
                    row.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                yield return row.ToArray();
            }
        }
        #endregion
    }
}
