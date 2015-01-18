using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using NLog;
using ReadOpenAipData.OpenAip;

namespace ReadOpenAipData
{
    class Program
    {
        static void Main(string[] args)
        {
            log = LogManager.GetCurrentClassLogger();
            connection = new SQLiteConnection(@"data source=C:\Users\Rob Verhoef\Documents\NavAirDataFiles\openaip\airnav-airports.db" + ";Version=3;");
            //airport_connection = new SQLiteConnection(@"data source=C:\Users\Rob Verhoef\Documents\NavAirDataFiles\openaip\airnav-airports.db" + ";Version=3;");

            log.Info("Starting OpenAip Data importer");

            airports = new Airports(connection);

            log.Info("Defined Values read!");

            //string Filename = @"C:\Users\Rob Verhoef\Documents\NavAirDataFiles\openaip\openaip_airports_netherlands_nl.aip";
            string Path = @"C:\Users\Rob Verhoef\Documents\NavAirDataFiles\openaip\";

            ReadAirports(Path);

            //airports.OpenAirportFile(Filename);

            log.Info("Read airports!");

            Console.ReadKey();
        }

        static private Logger log;
        static private SQLiteConnection connection;
        //static private SQLiteConnection airport_connection;
        static private Airports airports;

        private static void ReadAirports(string Path)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_Files", connection);
            try
            {
                connection.Open();
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string Filename = reader["AirportsFile"].ToString();
                    if (Convert.ToBoolean(reader["Active"]))
                    {
                        log.Info("Try to read Airports File: " + Filename);

                        airports.OpenAirportFile(Path + Filename);

                        log.Info("read airport file successfully!");
                    }

                }

            }
            catch (Exception ee)
            {
                log.Error(ee.Message);
            }
            finally
            {
                connection.Close();
            }
        }

    }
}
