using System;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace GameTrakrSteamCollector
{
    internal class Program
    {
        string steamCommunityID;
        bool libraryXmlExists = false;

        static void Main(string[] args)
        {
            var instance = new Program();

            instance.Intro();
        }

        void Intro()
        {
            Console.WriteLine("This application grabs the Application ID for all games in your Steam Library for automatic import into the GameTrakr database.");
            Console.WriteLine("Your Steam Community page and game data must be public for this to work.");

            // WARNING
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: This pulls ALL Application IDs.  If you are part of a private testing group or are a game developer, ensure that your private " +
                "Application IDs do not get uploaded to GameTrakr by manually removing them from the outputted text file. If there is a match to SteamDB your application/game" +
                "WILl be imported to the GameTrakr database.");
            Console.ForegroundColor = ConsoleColor.White;

            string wantCount;
            do
            {
                Console.WriteLine("Do you wish to continue? Yes (y) or No (n): ");
                wantCount = Console.ReadLine();
                var wantCountLower = wantCount?.ToLower();
                if (wantCountLower == "y")
                {
                    break;
                }

                if (wantCountLower == "n")
                {
                    Environment.Exit(0);
                }
            } while (true);

            do
            {
                Console.WriteLine("What is your Steam Community ID? i.e /id/lyeuhm, we only need 'lyeuhm'");
                string userResult = Console.ReadLine();

                if(userResult.Length > 3)
                {
                    steamCommunityID = userResult;
                    break;
                }
            } while (true);

            Console.WriteLine($"Thank you {steamCommunityID}");
            Console.WriteLine($"Attempting to retrieve Game Library...");

            DownloadGamesXml();

            if(libraryXmlExists)
            {
                Console.WriteLine("File exists, we can proceed with pulling AppID data!");

                AppIDScraper();
            }
        }

        void DownloadGamesXml()
        {
            if(File.Exists("gameLibrary.xml"))
            {
                libraryXmlExists = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("gameLibrary.xml already exists, skipping...");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile($"https://steamcommunity.com/id/{steamCommunityID}/games?tab=all&xml=1", "gameLibrary.xml");
                    }

                    if(File.Exists("gameLibrary.xml"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("gameLibrary.xml downloaded successfully (I think)");
                        Console.ForegroundColor = ConsoleColor.White;
                        libraryXmlExists = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.ReadKey();
                }
            }
        }

        private void AppIDScraper()
        {
            if(File.Exists($"{steamCommunityID}.txt")) {
                File.Delete($"{steamCommunityID}.txt");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Deleted old import file for {steamCommunityID}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            XDocument xdoc = XDocument.Load("gameLibrary.xml");
            foreach(XElement element in xdoc.Descendants())
            {
                if(element.Name.ToString().ToLower() == "appid")
                {
                    Console.WriteLine($" >> {element.Value}");
                    File.AppendAllText($"{steamCommunityID}.txt", element.Value + Environment.NewLine);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("AppID data has been pulled and ready for import.");
            Console.ForegroundColor = ConsoleColor.White;

            if (File.Exists("gameLibrary.xml"))
            {
                File.Delete("gameLibrary.xml");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Deleted gameLibrary.xml");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.ReadLine();
        }
    }
}
