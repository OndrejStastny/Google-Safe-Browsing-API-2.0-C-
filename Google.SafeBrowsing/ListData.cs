using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google.SafeBrowsing
{
    public class ListData
    {
        public IEnumerable<Interval> WhitelistDelete { get; set; }

        public IEnumerable<Interval> BlacklistDelete { get; set; }

        public IEnumerable<string> Redirects { get; set; }
    }
}
