using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Google.SafeBrowsing
{
    /// <summary>
    /// Canonical url according to http://code.google.com/intl/cs-CZ/apis/safebrowsing/developers_guide_v2.html#Canonicalization
    /// </summary>
    public class CanonicalURL
    {
        public string Host { get; set; }

        public string Schema { get; set; }

        public string Path { get; set; }

        public string Query { get; set; }


        public override string ToString()
        {
            return Schema + Host + Path + Query;
        }


        private CanonicalURL() { }


        /// <summary>
        /// Get canonical url according to http://code.google.com/intl/cs-CZ/apis/safebrowsing/developers_guide_v2.html#Canonicalization
        /// </summary>
        public static CanonicalURL Get(string url)
        {
            //remove escape characters
            Regex escChars = new Regex(@"\r|\t|\n|\v");
            url = escChars.Replace(url, String.Empty);

            //remove leading and trailing whitespace
            url = url.Trim(' ');

            //remove fragment
            Regex frag = new Regex(@"#.*");
            url = frag.Replace(url, String.Empty);

            //repeatedly unescape
            url = Unescape(url);

            //remove all leading and trailing dots
            Regex urlReg = new Regex(@"^((?:http|https|ftp)\://)?(.+?)(?:(/.*?)|)(\?.+)?$");
            Match urlMatch = urlReg.Match(url);

            if (!urlMatch.Success)
                throw new ArgumentException("Supplied URL was not in valid format " + url);

            var schema = urlMatch.Groups[1].Value;
            if (String.IsNullOrEmpty(schema))
                schema = "http://";

            var host = urlMatch.Groups[2].Value;
            host = host.TrimStart('.').TrimEnd('.');

            //replace consecutive dots with a single dot
            Regex dots = new Regex(@"\.\.+");
            host = dots.Replace(host, String.Empty);

            //lower case
            host = host.ToLowerInvariant();

            long intHost = -1;
            if (Int64.TryParse(host, out intHost))
            {
                host = String.Format("{0}.{1}.{2}.{3}", (intHost >> 24) & 255,
                                                        (intHost >> 16) & 255,
                                                        (intHost >> 8) & 255,
                                                        (intHost) & 255);
            }

            var path = urlMatch.Groups[3].Value;

            //replace path sequence
            Regex seq1 = new Regex(@"(?:/\./|//)");
            path = seq1.Replace(path, @"/");

            Regex seq2 = new Regex(@"/.+?/\.\./?");
            path = seq2.Replace(path, String.Empty);

            if (String.IsNullOrEmpty(path))
                path = "/";

            var query = urlMatch.Groups[4].Value;

            var curl = new CanonicalURL()
            {
                Schema = Encode(schema),
                Host = Encode(host),
                Path = Encode(path),
                Query = Encode(query)
            };

            return curl;
        }

        private static string Encode(string url)
        {
            var sb = new StringBuilder();
            var cha = url.ToCharArray();
            for (int i = 0; i < url.Length; i++)
            {
                if (cha[i] <= 32 || cha[i] >= 127 || cha[i] == '#' || cha[i] == '%')
                    sb.Append("%" + ((int)cha[i] < 16 ? "0" : "") + ((int)cha[i]).ToString("X"));
                else
                    sb.Append(cha[i]);
            }

            return sb.ToString();
        }

        private static string Unescape(string url)
        {
            Regex unescape = new Regex(@"%([0-9a-fA-F]{2})");
            MatchCollection matches = unescape.Matches(url);
            StringBuilder sb = null;
            int prev = 0;
            byte hex;
            while (matches.Count > 0)
            {
                sb = new StringBuilder();

                prev = 0;
                foreach (Match match in matches)
                {
                    sb.Append(url.Substring(prev, match.Index - prev));
                    hex = Byte.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    sb.Append((char)hex);
                    prev = match.Index + match.Length;
                }
                sb.Append(url.Substring(prev));
                url = sb.ToString();
                matches = unescape.Matches(url);
            }

            return url;
        }
    }
}
