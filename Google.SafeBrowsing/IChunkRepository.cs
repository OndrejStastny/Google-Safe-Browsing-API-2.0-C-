using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google.SafeBrowsing.Model
{
    public interface IChunkRepository
    {
        /// <summary>
        /// Get items by Host or URL key for any list
        /// </summary>
        IEnumerable<Chunk> GetByKey(int key, bool isHost, bool isBlacklist);

        /// <summary>
        /// Save items
        /// </summary>
        void Save(IEnumerable<Chunk> items);

        /// <summary>
        /// Delete item
        /// </summary>
        void Delete(string list, bool isBlacklist, IEnumerable<Interval> chunkIds);

        /// <summary>
        /// List all chunk IDs for given black/white list
        /// </summary>
        IEnumerable<Interval> ListChunks(string list, bool isBlackList);

    }
}
