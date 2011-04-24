using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.SafeBrowsing.Model;
using System.Configuration;

namespace Google.SafeBrowsing.Example
{
    class Program
    {
        public API Api { get; private set; }

        public IChunkRepository ChunkRepository { get; set; }

        public Program(string apiKey, IChunkRepository chunkRepositiory)
        {
            Api = new API(apiKey);
            ChunkRepository = chunkRepositiory;
        }

        public void PrintAvailableLists()
        {
            var lists = Api.GetLists();

            foreach (var item in lists)
            {
                Console.WriteLine(item);
            }
        }

        public void GetMalwareDataChunks()
        {
            //change GetChunkData parameter to CanonicalURL ??????


            //step 1 of 2
            //var data = Api.GetListData("googpub-phish-shavar",
            //           new List<Interval>() { new Interval() { Start = 1, End = 2 }, new Interval() { Start = 3, End = 3 } },
            //           new List<Interval>() { new Interval() { Start = 2, End = 5 }, new Interval() { Start = 7, End = 7 } });

            var data = Api.GetListData("googpub-phish-shavar",
                                        ChunkRepository.ListChunks("phish", false), 
                                        ChunkRepository.ListChunks("phish", true));

            //add-del and sub-del chunks
            ChunkRepository.Delete("phish", true, data.BlacklistDelete);
            ChunkRepository.Delete("phish", false, data.WhitelistDelete);

            //step 2 of 2
            foreach (var url in data.Redirects)
                ChunkRepository.Save(Api.GetChunkData("http://" + url, "phish"));

        }

        public void GetFullLengthHash()
        {
            var url = CanonicalURL.Get("http://kresko-group.net/");

            var hashes = Api.GetFullHashes(url);

        }
            
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Usage: Google.SafeBrowsing.Example.exe google_api_key connection_string");
                return;
            }

            //chunk repository
            IChunkRepository repo = new SqlChunkRepository(args[1]);

            //get API key from the console
            var program = new Program(args[0], repo);


            //only two lists are used for actual operation
            //googpub-phish-shavar: A list of hashed suffix/prefix expressions representing sites 
            //                      that should be blocked because they are hosting phishing pages.
            //goog-malware-shavar: A list of suffix/prefix regular expressions representing sites 
            //                      that should be blocked because they are hosting malware pages.
            program.PrintAvailableLists();


            //get sample chunk of list goog-malware-shavar data
            //assume that we already have some chunks for both white and black lists
            program.GetMalwareDataChunks();


            //...
            program.GetFullLengthHash();


            Console.ReadLine();
        }
    }
}
