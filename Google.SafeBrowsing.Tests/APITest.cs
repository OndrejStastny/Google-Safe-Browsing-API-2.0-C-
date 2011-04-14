using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Google.SafeBrowsing;

namespace Google.SafeBrowsing.Tests
{
/// <summary>
///This is a test class for APITest and is intended
///to contain all APITest Unit Tests
///</summary>
    [TestClass]
    public class SafeBrowsingAPI
    {
        [TestMethod]
        public void t01()
        {
            string[] addresses = { "a.b.c/1/2.html?param=1", 
                             "a.b.c/1/2.html",
                             "a.b.c/",
                             "a.b.c/1/",
                             "b.c/1/2.html?param=1",
                             "b.c/1/2.html",
                             "b.c/",
                             "b.c/1/" };

            var api = new Google.SafeBrowsing.API("1234");

            var url = Google.SafeBrowsing.CanonicalURL.Get("http://a.b.c/1/2.html?param=1");

            var res = api.GenerateCombinations(url);

            CollectionAssert.AreEqual(addresses, res.ToArray());
        }

        [TestMethod]
        public void t02()
        {
            string[] addresses = { "a.b.c.d.e.f.g/1.html",
                                    "a.b.c.d.e.f.g/",
                                    "c.d.e.f.g/1.html",
                                     "c.d.e.f.g/",
                                     "d.e.f.g/1.html",
                                     "d.e.f.g/",
                                     "e.f.g/1.html",
                                     "e.f.g/",
                                     "f.g/1.html",
                                     "f.g/" };

            var api = new Google.SafeBrowsing.API("1234");

            var url = Google.SafeBrowsing.CanonicalURL.Get("http://a.b.c.d.e.f.g/1.html");

            var res = api.GenerateCombinations(url);

            CollectionAssert.AreEqual(addresses, res.ToArray());
        }

        [TestMethod]
        public void t03()
        {
            string[] addresses = { "1.2.3.4/1/",
                                    "1.2.3.4/" };

            var api = new Google.SafeBrowsing.API("1234");

            var url = Google.SafeBrowsing.CanonicalURL.Get("http://1.2.3.4/1/");

            var res = api.GenerateCombinations(url);

            CollectionAssert.AreEqual(addresses, res.ToArray());
        }

        [TestMethod]
        public void ListData01()
        {
            string[] addresses = { "abc.d/efgh" };
            string data = "n:123\ni:list-name\nu:abc.d/efgh\nsd:1-2,3\n";

            Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);

            var res = api.ParseListData(stream);

            CollectionAssert.AreEqual(addresses, res.Redirects.ToArray());
        }

        [TestMethod]
        public void ListData02()
        {
            string[] addresses = { };
            string data = "r:pleasereset\n";

            Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);

            ListData res = null;

            try
            {
                res = api.ParseListData(stream);

                Assert.Fail("Malformed data should throw exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Unable to parse data", ex.Message);
            }

        }

        [TestMethod]
        public void ListData03()
        {
            string[] addresses = { };
            string data = "n:123\nr:pleasereset\n";

            Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);

            ListData res = null;

            try
            {
                res = api.ParseListData(stream);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ListData04()
        {
            string[] addresses = { "abc.d/efgh",
                                   "123.4/5/6" };
            string data = "n:123\ni:list-name\nu:abc.d/efgh\nsd:1-2,3\ni:list-name\nu:123.4/5/6\n";

            Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);

            var res = api.ParseListData(stream);

            CollectionAssert.AreEqual(addresses, res.Redirects.ToArray());
        }


        /* [TestMethod]
         public void ChunkData01()
         {
             Chunk[] chunks = new Chunk[] { 
                 new Chunk() {
                         Id = 4,
                         Mode = 'a',
                         Hash = UnicodeEncoding.ASCII.GetBytes("abcdef"),
                         Length = 48
                  }
             };
             string data = "a:4:48:6\nabcdef";

             Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

             byte[] byteArray = Encoding.ASCII.GetBytes(data);
             MemoryStream stream = new MemoryStream(byteArray);

             var res = api.ParseChunkData(stream);

             CollectionAssert.AreEqual(chunks, res.Select(x => x.Value).ToArray());
         }

         [TestMethod]
         public void ChunkData02()
         {
             Chunk[] chunks = new Chunk[] { 
                 new Chunk() {
                         Id = 4,
                         Mode = 'a',
                         Hash = UnicodeEncoding.ASCII.GetBytes("abcdef"),
                         Length = 48
                  },
                  new Chunk() {
                         Id = 5,
                         Mode = 's',
                         Hash = UnicodeEncoding.ASCII.GetBytes("1234"),
                         Length = 32
                  }
             };
             string data = "a:4:48:6\nabcdefs:5:32:4\n1234";

             Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

             byte[] byteArray = Encoding.ASCII.GetBytes(data);
             MemoryStream stream = new MemoryStream(byteArray);

             var res = api.ParseChunkData(stream);

             CollectionAssert.AreEqual(chunks, res.Select(x => x.Value).ToArray());
         }

         [TestMethod]
         public void ChunkData03()
         {
             Chunk[] chunks = new Chunk[] { };
             string data = "a:4:48:";

             Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

             byte[] byteArray = Encoding.ASCII.GetBytes(data);
             MemoryStream stream = new MemoryStream(byteArray);

             Dictionary<byte[], Chunk> res = null;

             try
             {
                 res = api.ParseChunkData(stream);

                 Assert.Fail("Malformed data should throw exception");
             }
             catch (Exception ex) 
             {
                 Assert.AreEqual("Unable to parse data", ex.Message);
             }
         }

         [TestMethod]
         public void ChunkData04()
         {
             Chunk[] chunks = new Chunk[] {  
                 new Chunk() {
                         Id = 4,
                         Mode = 'a',
                         Hash = UnicodeEncoding.ASCII.GetBytes(""),
                         Length = 48
                  }};
             string data = "a:4:48:0\n";

             Google.SafeBrowsing.API_Accessor api = new Google.SafeBrowsing.API_Accessor("1234");

             byte[] byteArray = Encoding.ASCII.GetBytes(data);
             MemoryStream stream = new MemoryStream(byteArray);

             var res = api.ParseChunkData(stream);

             CollectionAssert.AreEqual(chunks, res.Select(x => x.Value).ToArray());
         }*/
    }
}
