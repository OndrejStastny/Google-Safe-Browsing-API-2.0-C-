using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Google.SafeBrowsing.Model
{
    /// <summary>
    /// Default Chunk Repository implementation using ADO.NET for MS SQL
    /// </summary>
    public class SqlChunkRepository : IChunkRepository
    {
        public IEnumerable<Chunk> GetByKey(int key, bool isHost, bool isBlacklist)
        {
            var list = new List<Chunk>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Google.SafeBrowsing"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    @"SELECT ChunkId, List, IsBlackList, IsHost, Key 
                      FROM Chunks 
                      WHERE Key = @Key 
                        AND IsHost = @IsHost 
                        AND IsBlacklist = @IsBlacklist"
                    , conn);
                cmd.Parameters.Add("@Key", SqlDbType.Int);
                cmd.Parameters.Add("@IsHost", SqlDbType.Bit);
                cmd.Parameters.Add("@IsWhitelist", SqlDbType.Bit);

                cmd.Parameters["@Key"].Value = key;
                cmd.Parameters["@IsHost"].Value = isHost;
                cmd.Parameters["@IsBlacklist"].Value = isBlacklist;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Chunk()
                        {
                            ChunkId = reader.GetInt32(0),
                            List = reader.GetString(1),
                            IsBlackList = reader.GetBoolean(2),
                            IsHost = reader.GetBoolean(3),
                            Key = reader.GetInt32(4)
                        });
                    }
                }
            }

            return list;
        }

        public void Save(IEnumerable<Chunk> Chunks)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Google.SafeBrowsing"].ConnectionString))
            {
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    using (var rdr = new SqlChunkReader(Chunks))
                    {
                        using (var inserter = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity, tran))
                        {
                            inserter.BatchSize = rdr.RecordsAffected;
                            inserter.DestinationTableName = "Chunks";
                            inserter.NotifyAfter = 0;

                            inserter.ColumnMappings.Add("ChunkId", "ChunkId");
                            inserter.ColumnMappings.Add("List", "List");
                            inserter.ColumnMappings.Add("IsBlackList", "IsBlackList");
                            inserter.ColumnMappings.Add("IsHostKey", "IsHostKey");
                            inserter.ColumnMappings.Add("Key", "Key");

                            inserter.WriteToServer(rdr);
                            inserter.Close();

                            tran.Commit();
                        }
                    }
                }
            }
        }

        public void Delete(string list, IEnumerable<Interval> chunkIds)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Google.SafeBrowsing"].ConnectionString))
            {
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand("DELETE FROM Chunks WHERE List = @List AND ChunkId >= @From AND @ChunkId <= @To", conn);

                    cmd.Parameters.Add("@List", SqlDbType.NVarChar, 4);
                    cmd.Parameters.Add("@From", SqlDbType.Int);
                    cmd.Parameters.Add("@To", SqlDbType.Int);

                    cmd.Parameters["@List"].Value = list.Substring(0, 4);

                    foreach (var interval in chunkIds)
                    {
                        cmd.Parameters["@From"].Value = interval.Start;
                        cmd.Parameters["@To"].Value = interval.End;

                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
            }
        }

        public IEnumerable<Interval> ListChunks(string list, bool isBlackList)
        {
            var Chunks = new List<Interval>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Google.SafeBrowsing"].ConnectionString))
            {
                var cmd = new SqlCommand("SELECT DISTINCT ChunkId FROM Chunks WHERE List = @List AND isWhitelist = @IsBlackList ORDER BY ChunkId ASC", conn);
                cmd.Parameters.Add("@List", SqlDbType.NVarChar, 4);
                cmd.Parameters.Add("@IsBlackList", SqlDbType.Bit);

                cmd.Parameters["@List"].Value = list.Substring(0, 4);
                cmd.Parameters["@IsBlackList"].Value = isBlackList;


                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return Chunks;   //no Chunks, terminate

                    int fromId = reader.GetInt32(0), toId = reader.GetInt32(0);

                    while (reader.Read())
                    {
                        if (reader.GetInt32(0) - toId > 1)
                        {
                            Chunks.Add(new Interval() { Start = fromId, End = toId });
                            fromId = toId = reader.GetInt32(0);
                            continue;
                        }

                        ++toId;
                    }

                    Chunks.Add(new Interval() { Start = fromId, End = toId });
                }
            }

            return Chunks;
        }
    }
}
