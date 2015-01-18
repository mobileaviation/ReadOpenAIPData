using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadOpenAipData.OpenAip;
using System.Data.SQLite;
using NLog;

namespace ReadOpenAipData
{
    public class Airport
    {
        private Logger log;
        public Int32 ID;
        public Int32 Type_ID;
        public string Country;
        public string Name;
        public string Icao;
        public List<Radio> Radios;
        public GeoLocation geoLocation;
        public List<Runway> Runways;

        public Airport(SQLiteConnection connection)
        {
            log = LogManager.GetCurrentClassLogger();
            this.connection = connection;

            Radios = new List<Radio>();
            geoLocation = new GeoLocation();
            Runways = new List<Runway>();
        }

        private SQLiteConnection connection;

        
    }
}
