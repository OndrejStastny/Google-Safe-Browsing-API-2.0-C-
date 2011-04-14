using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace Google.SafeBrowsing
{
    /// <summary>
    /// Google Safe Browsing API v2. 
    /// Does not support MAC authentication.
    /// REKEY not supported.
    /// Only supports 32bit key sizes.
    /// http://code.google.com/intl/cs-CZ/apis/safebrowsing/developers_guide_v2.html
    /// </summary>
    public class API
    {
        /// <summary>
        /// API key issued by Google
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// Version identifer for this library
        /// </summary>
        public string ClientVersion
        {
            get
            {
                if (_ClientVersion == null)
                {
                    _ClientVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                return _ClientVersion;
            }
        }
        private string _ClientVersion;

        /// <summary>
        /// API URL with parameter placeholders
        /// </summary>
        public string ApiURL
        {
            get
            {
                return @"http://safebrowsing.clients.google.com/safebrowsing/{0}?client=api&apikey={1}&appver={2}&pver=2.2";
            }
        }


        public API(string apiKey)
        {
            ApiKey = apiKey;
        }



        /// <summary>
        /// Generate valid url combinations that have to be checked againts the database
        /// </summary>
        /// <param name="url">Canonical URL</param>
        /// <returns>List of url combinations to be checked</returns>
        public IEnumerable<string> GenerateCombinations(CanonicalURL url)
        {
            var list = new List<string>();


            var hostnames = new List<string>();

            hostnames.Add(url.Host);

            //split hostname to individual components
            if (!IsIpAddress(url.Host))
            {
                var res = SplitHost(url.Host).Skip(1);
                hostnames.AddRange(res.Take(res.Count() - 1).Skip(Math.Max(0, res.Count() - 5)));
            }


            var multipath = new List<string>();

            //split path to individual components
            multipath.Add(url.Path);

            multipath.AddRange(SplitPath(url.Path).Skip(1).Reverse().Take(4));

            string path = null;
            foreach (var hostname in hostnames)
            {
                path = multipath.First();

                if (!String.IsNullOrEmpty(url.Query))
                    list.Add(hostname + path + url.Query);

                list.Add(hostname + path);

                foreach (var subpath in multipath.Skip(1))
                {
                    list.Add(hostname + subpath);
                }

            }

            return list;
        }

        /// <summary>
        /// This is used by clients to discover the available list types.
        /// </summary>
        /// <returns>Lists</returns>
        public IEnumerable<string> GetLists()
        {
            var requestUrl = String.Format(ApiURL, "list", ApiKey, ClientVersion);
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);

            if (request.Proxy != null)
                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var sr = new StreamReader(response.GetResponseStream());

            var list = new List<string>();

            while (!sr.EndOfStream)
                list.Add(sr.ReadLine());

            return list;
        }

        /// <summary>
        /// This is used by clients who want to get new data for known list types.
        /// Step 1/2. Get chunk URLs redirects for ALL chunks
        /// Only supports data for one list at the time (which is not a problem for
        /// our implementation since we always only request from one list)
        /// </summary>
        /// <param name="list">list name</param>
        /// <param name="whitelist">all whitelist chunks already available localy</param>
        /// <param name="blacklist">all blacklist chunks already available localy</param>
        /// <returns>list of redirect URLs and chunks to be deleted from white&black list</returns>
        public ListData GetListData(string list, IEnumerable<Interval> whilelist, IEnumerable<Interval> blacklist)
        {
            var requestUrl = String.Format(ApiURL, "downloads", ApiKey, ClientVersion);
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);

            if (request.Proxy != null)
                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            request.Method = "POST";

            string strwlist = PrintIntervals(whilelist, "a"),
                strblist = PrintIntervals(blacklist, "s");

            var sb = new StringBuilder();

            sb.Append(list);
            sb.Append(";");

            if (strwlist.Length > 0)
            {
                sb.Append(strwlist);
                sb.Append(':');
            }
            if (strblist.Length > 0)
                sb.Append(strblist);

            sb.Append('\n');


            var s = request.GetRequestStream();

            var content = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(content, 0, content.Length);
            s.Close();

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            //read individual redirect addresses
            var data = ParseListData(response.GetResponseStream());

            return data;
        }

        /// <summary>
        /// This is used by clients who want to get new data for known list types.
        /// Step 2/2 - called for each Chunk redirect. Download chunk content.
        /// </summary>
        /// <param name="chunkURL">URL returned from GetListData</param>
        /// <returns>list of all chunks</returns>
        public IEnumerable<Chunk> GetChunkData(string chunkURL, string listName)
        {
            var request = (HttpWebRequest)WebRequest.Create(chunkURL);

            if (request.Proxy != null)
                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var chunks = ParseChunkData(response.GetResponseStream(), listName);

            return chunks;
        }

        /// <summary>
        /// A client may request the list of full-length hashes for a hash prefix. 
        /// This usually occurs when a client is about to download content from a url whose 
        /// calculated hash starts with a prefix listed in a blacklist.
        /// </summary>
        /// <param name="targetURL"></param>
        /// <returns>List of full length hashes</returns>
        public IEnumerable<byte[]> GetFullHashes(CanonicalURL targetURL)
        {
            var requestUrl = String.Format(ApiURL, "gethash", ApiKey, ClientVersion);

            var request = (HttpWebRequest)WebRequest.Create(requestUrl);

            if (request.Proxy != null)
                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            request.Method = "POST";
            //request.Timeout = 2000;

            var hash = ComputeHash(targetURL);

            var s = request.GetRequestStream();

            //we only send one hash and assume prefix size of 4
            var content = Encoding.ASCII.GetBytes("4:4\n");
            s.Write(content, 0, 4);
            s.Write(hash, 0, 4);

            s.Close();

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var hashes = ParseFullHashes(response.GetResponseStream());

            return hashes;
        }


        private string PrintIntervals(IEnumerable<Interval> list, string listName)
        {
            var sb = new StringBuilder();
            var enmrtr = list.GetEnumerator();

            if (enmrtr.MoveNext())
                sb.Append(listName + ':');

            while (true)
            {
                sb.Append(enmrtr.Current.Start);
                if (enmrtr.Current.End > enmrtr.Current.Start)  //if interval is larger than one unit
                {
                    sb.Append("-");
                    sb.Append(enmrtr.Current.End);
                }

                if (enmrtr.MoveNext())
                    sb.Append(",");
                else
                    break;
            }

            return sb.ToString();
        }

        private byte[] ComputeHash(CanonicalURL url)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(url.ToString());

            SHA256 algo = new SHA256Managed();

            return algo.ComputeHash(bytes);
        }

        private Int32 ReadNumber(byte[] buffer, int startPosition, out int endPosition)
        {
            endPosition = startPosition;

            int num = 0;
            while (endPosition < buffer.Length)
            {
                if (buffer[endPosition] < '0' || buffer[endPosition] > '9')
                    break;

                num = num * 10 + (int)(buffer[endPosition] - '0');
                ++endPosition;
            }

            return num;
        }

        private Int32 DecodeNumber(byte[] buffer, int startPosition, out int endPosition)
        {
            endPosition = startPosition + 4;

            int num = 0;
            while (startPosition < endPosition)
            {
                num = (num << 8) | (int)(buffer[startPosition]);
                ++startPosition;
            }

            return num;
        }

        private IEnumerable<Chunk> ParseChunkData(Stream stream, string listName)
        {
            var redirects = new List<string>();
            byte[] buffer = new byte[1048576]; //1MB

            //safe method to read the data
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        buffer = ms.ToArray();
                        break;
                    }
                    ms.Write(buffer, 0, read);
                }
            }

            var dict = new List<Chunk>();

            int mode = 0xA;
            int position = 0;
            bool error = false;
            int chunkLen = 0;
            int count = 0;
            int startPos = 0;
            bool isBlackList = false;
            int chunkId = 0;

            while (position < buffer.Length)
            {
                switch (buffer[position])
                {
                    case 0x61: //'a'
                        if (mode != 0xA)
                        {
                            error = true;
                            break;
                        }

                        isBlackList = false;
                        mode = 0xB;
                        ++position;
                        break;
                    case 0x73: //'s'
                        if (mode != 0xA)
                        {
                            error = true;
                            break;
                        }

                        isBlackList = true;
                        mode = 0xB;
                        ++position;
                        break;
                    case 0x3A: //':'
                        if (mode == 0xB)
                        {
                            chunkId = ReadNumber(buffer, ++position, out position);
                            mode = 0xC;
                        }
                        else if (mode == 0xC)
                        {
                            ReadNumber(buffer, ++position, out position);
                            //hashlength igonored, assumed 4
                            mode = 0xD;
                        }
                        else if (mode == 0xD)
                        {
                            chunkLen = ReadNumber(buffer, ++position, out position);
                            mode = 0xE;
                        }
                        else
                        {
                            error = true;
                        }
                        break;
                    case 0xA: //'\n'
                        if (mode != 0xE)
                        {
                            error = true;
                            break;
                        }

                        startPos = ++position;

                        while ((position - startPos) < chunkLen)
                        {
                            dict.Add(new Chunk()
                            {
                                ChunkId = chunkId,
                                IsBlackList = isBlackList,
                                Key = DecodeNumber(buffer, position, out position),
                                List = listName,
                                IsHost = true
                            });

                            count = (int)buffer[position++];

                            if (!isBlackList)
                            {
                                for (int i = 0; i < count; i++)
                                    dict.Add(new Chunk()
                                    {
                                        ChunkId = chunkId,
                                        IsBlackList = isBlackList,
                                        Key = DecodeNumber(buffer, position, out position),
                                        List = listName,
                                        IsHost = false
                                    });
                            }
                            else    //'s'
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    position += 4;  //skip chunknum
                                    dict.Add(new Chunk()
                                    {
                                        ChunkId = chunkId,
                                        IsBlackList = isBlackList,
                                        Key = DecodeNumber(buffer, position, out position),
                                        List = listName,
                                        IsHost = false
                                    });
                                }
                                if (count < 1)
                                    position += 4;  //skip chunknum
                            }
                        }

                        mode = 0xA;
                        break;
                    case 0:
                        if (mode != 0xA)
                            error = true;
                        position = buffer.Length;   //exit the FSM
                        break;
                    default:
                        error = true;
                        break;
                }

                if (error)
                    break;
            }

            return dict;
        }

        private ListData ParseListData(Stream stream)
        {
            var listData = new ListData();

            var redirects = new List<string>();
            Regex regex = new Regex(@"^(e|r|n|i|u|sd|ad)\:(.+)$");
            Match match = null;
            string line = null, type = null, value = null;
            int state = 0xA;
            bool error = false;
            var sr = new StreamReader(stream);

            //simple state machine
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                match = regex.Match(line);

                if (!match.Success)
                    continue;   //client must ignore lines it does not understand

                type = match.Groups[1].Value;
                value = match.Groups[2].Value;

                switch (type)
                {
                    case "n"://n: xxxx   delay in seconds before poling again
                        if (state != 0xA)
                        {
                            error = true;
                            break;
                        }
                        //delay gets ignored at the moment
                        state = 0xB;
                        break;
                    case "e":
                        if (state != 0xA)
                        {
                            error = true;
                            break;
                        }
                        //error gets ignored at the moment
                        break;
                    case "r":
                        if (state != 0xB)
                        {
                            error = true;
                            break;
                        }
                        //reset signal gets ignored at the moment
                        break;
                    case "i":   //i:xxxx   list name
                        //list name is ignored
                        state = 0xB;
                        break;
                    case "u":   //u:xxxx   redirect url
                        if (state != 0xB)
                        {
                            error = true;
                            break;
                        }
                        redirects.Add(value);
                        break;
                    case "sd":  //sd:x-y,z
                        if (state != 0xB)
                        {
                            error = true;
                            break;
                        }
                        listData.BlacklistDelete = ParseIntervals(value.Split(':').LastOrDefault());
                        break;
                    case "ad":  //ad:x-y,z
                        if (state != 0xB)
                        {
                            error = true;
                            break;
                        }
                        listData.WhitelistDelete = ParseIntervals(value.Split(':').LastOrDefault());
                        break;
                }

                if (error)
                    throw new Exception("Unable to parse data");
            }

            listData.Redirects = redirects;
            return listData;
        }

        private IEnumerable<byte[]> ParseFullHashes(Stream stream)
        {
            var hashes = new List<byte[]>();

            Regex regex = new Regex(@"^(.+)\:(\d+)\:(\d+)$");
            Match match = null;
            string list = null;
            int chunkNum = 0, hashLen;

            string line = null;

            var sr = new StreamReader(stream);

            //simple state machine
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                match = regex.Match(line);

                if (!match.Success)
                    throw new Exception("Unable to parse data");

                //list and chunk id is ignored
                list = match.Groups[1].Value;
                if (!Int32.TryParse(match.Groups[2].Value, out chunkNum))
                    chunkNum = 0;
                if (!Int32.TryParse(match.Groups[3].Value, out hashLen))
                    hashLen = 0;

                line = sr.ReadLine();

                hashes.Add(Encoding.ASCII.GetBytes(line));

            }

            return hashes;
        }

        private IEnumerable<string> SplitHost(string url)
        {
            var index = 0;
            var sub = url;

            while (index >= 0)
            {
                yield return sub;
                index = sub.IndexOf('.');
                sub = sub.Substring(index + 1);
            }
        }

        private IEnumerable<string> SplitPath(string url)
        {
            var index = Int32.MaxValue;
            var sub = url;

            while (index >= 0)
            {
                yield return sub;
                index = sub.Substring(0, sub.Length - 1).LastIndexOf('/');
                sub = sub.Substring(0, index + 1);
            }
        }

        private bool IsIpAddress(string hostname)
        {
            Regex reg = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

            return reg.IsMatch(hostname);
        }

        private IEnumerable<Interval> ParseIntervals(string input)
        {
            int mode = 0xA;
            int num = 0;
            bool error = false;
            Interval interval = null;
            var intervals = new List<Interval>();

            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '-':
                        if (mode != 0xB)
                        {
                            error = true;
                        }
                        interval = new Interval() { Start = num, End = num };
                        num = 0;
                        mode = 0xC;
                        break;
                    case ',':
                        if (mode == 0xB)
                            interval = new Interval() { Start = num, End = num };
                        else if (mode == 0xC)
                            interval.End = num;
                        else
                        {
                            error = true;
                            break;
                        }
                        intervals.Add(interval);
                        interval = null;
                        mode = 0xA;
                        num = 0;
                        break;
                    case '0':
                        goto case '9';
                    case '1':
                        goto case '9';
                    case '2':
                        goto case '9';
                    case '3':
                        goto case '9';
                    case '4':
                        goto case '9';
                    case '5':
                        goto case '9';
                    case '6':
                        goto case '9';
                    case '7':
                        goto case '9';
                    case '8':
                        goto case '9';
                    case '9':
                        num = num * 10 + (int)(input[i] - '0');
                        if (mode == 0xA) mode = 0xB;
                        else mode = 0xC;

                        break;
                }
                if (error)
                    throw new Exception("Parsing error.");
            }
            //append last interval in sequence
            if (mode == 0xB)
                interval = new Interval() { Start = num, End = num };
            else if (mode == 0xC)
                interval.End = num;
            else
                throw new Exception("Parsing error.");
            intervals.Add(interval);
            return intervals;
        }
    }
}
