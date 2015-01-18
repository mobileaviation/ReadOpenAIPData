using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using NLog;

namespace ReadOpenAipData.OpenAip
{
    public class DefinedValues : List<DefinedValue>
    {
        private Logger log;

        public DefinedValues()
        {
            log = LogManager.GetCurrentClassLogger();
        }

        public void ReadDatabase(SQLiteCommand cmd)
        {
            try
            {
                cmd.Connection.Open();
                SQLiteDataReader read = cmd.ExecuteReader();

                while (read.Read())
                {
                    DefinedValue v = new DefinedValue();
                    v.ID = Convert.ToInt32(read["_id"]);
                    v.Value = read["Value"].ToString();
                    v.Description = (read["Description"] == DBNull.Value) ? "" : read["Description"].ToString();
                    this.Add(v);
                }
            }
            catch (Exception ee)
            {
                log.Error("Database open Exception: " + ee.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
    }
}
