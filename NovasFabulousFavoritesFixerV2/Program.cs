using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NovasFabulousFavoritesFixerV2
{
    class Program
    {
        public const string baseurl = "https://synthriderz.com";
        public const string beatmapsurl = "/api/beatmaps";
        public const string osturl = "https://synthriderz.com/api/beatmaps?s=%7B%22published%22:%7B%22$or%22:[true,false]%7D,%22ost%22:true%7D";

        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------------------------------------------------------------------------------");
            Console.WriteLine("                          ***** Novas Fabulous Favorites Fixer ******");
            Console.WriteLine("                          Fixes all your Problems with a Single Click");
            Console.WriteLine("---------------------------------------------------------------------------------------------------");
            Console.WriteLine("");

            if (File.Exists("favorites.bin"))
            {
                int errors = 0;

                List<SongInfo> items = new List<SongInfo>();

                Console.WriteLine("Downloading map info");

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string content = client.DownloadString(new Uri(baseurl + beatmapsurl));
                    items = JsonConvert.DeserializeObject<List<SongInfo>>(content);

                    content = client.DownloadString(new Uri(osturl));
                    items.AddRange(JsonConvert.DeserializeObject<List<SongInfo>>(content));
                }

                Console.WriteLine("Loading old favorites");

                Favorites favorites = JsonConvert.DeserializeObject<Favorites>(File.ReadAllText("favorites.bin"));

                List<string> tmp = new List<string>();

                foreach (string favorite in favorites.Favorite)
                {

                    string[] split = favorite.Trim('-').Split('-');
                    string res = "";

                    if (split.Length == 1)
                    {
                        var found = items.Where(x => x.title.ToLower() == split[0].ToLower()).ToArray();
                        if (found.Length == 1)
                        {
                            tmp.Add(found[0].title + "-" + found[0].artist + "-" + found[0].hash);
                        }
                        else
                        {
                            Console.WriteLine("Found " + found.Length + " maps with the title " + split[0]);
                            errors++;
                        }
                    }
                    else if (split.Length == 2)
                    {
                        var found = items.Where(x => x.title.ToLower() == split[0].ToLower() && x.artist.ToLower() == split[1].ToLower()).ToArray();
                        if (found.Length == 1)
                        {
                            tmp.Add(found[0].title + "-" + found[0].artist + "-" + found[0].hash);
                        }
                        else
                        {
                            Console.WriteLine("Found " + found.Length + " maps with the title " + split[0] + " and the artist " + split[1]);
                            errors++;
                        }
                    }
                    else if (split.Length == 3)
                    {
                        //Console.WriteLine(split[0] + " is already fixed");
                    }
                    else
                    {
                        Console.WriteLine("No valid format found for " + favorite);
                        errors++;
                    }
                }

                favorites.Favorite.AddRange(tmp);

                favorites.Favorite = favorites.Favorite.Distinct().ToList();//remove duplicates

                Console.WriteLine("Writing new favorites");

                using (StreamWriter sr = new StreamWriter("favorites.bin"))
                {
                    sr.Write(JsonConvert.SerializeObject(favorites, Formatting.Indented));
                }

                Console.WriteLine("All done");
                Console.WriteLine(errors + " favorites were not fixed");
            }
            else
            {
                Console.WriteLine("No favorites.bin found in the current directory");
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }

    class Favorites
    {
        public List<string> Favorite = new List<string>();
    }

    class SongInfo
    {
        public string title = "";
        public string artist = "";
        public string hash = "";
    }
}
