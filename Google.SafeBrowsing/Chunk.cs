using System;
using System.Collections.Generic;

namespace Google.SafeBrowsing
{
    /// <summary>
    /// Chunk of data 
    /// </summary>
    public class Chunk
    {
       /* public int Id { get; set; }

        public Int32 HostKey { get; set; }

        public List<Int32> UrlKeys { get; set; }

        public char Mode { get; set; }*/
        
        public int ChunkId { get; set; }

        public string List { get; set; }

        public bool IsHost { get; set; }

        public bool IsBlackList { get; set; }

        public int Key { get; set; }

    }
}
