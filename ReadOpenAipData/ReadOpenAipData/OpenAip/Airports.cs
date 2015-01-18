using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Xml.Linq;
using NLog;

namespace ReadOpenAipData.OpenAip
{
    public class Airports : List<Airport>
    {
        public Airports(SQLiteConnection connection)
        {
            log = LogManager.GetCurrentClassLogger();

            this.connection = connection;
            readAirportTypes();
            readRadioCategories();
            readRadioTypes();
            readRunwayOperations();
            readRunwaySurfaces();
        }

        private SQLiteConnection connection;
        private Logger log;

        private DefinedValues airportTypes;
        private DefinedValues radioCategories;
        private DefinedValues radioTypes;
        private DefinedValues runwayOperations;
        private DefinedValues runwaySurfaces;

        private void readAirportTypes()
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_AirportTypes", connection);
            airportTypes = new DefinedValues();
            airportTypes.ReadDatabase(cmd);
        }
        private Int32 getIDFromDefinedValue(DefinedValues v, string Value)
        {
            var a = from vv in v
                    where vv.Value == Value
                    select vv;

            if (a.Count() > 0)
                return a.First().ID;
            else
                return -1;
        }

        private void readRadioCategories()
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_RadioCategory", connection);
            radioCategories = new DefinedValues();
            radioCategories.ReadDatabase(cmd);
        }

        private void readRadioTypes()
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_RadioType", connection);
            radioTypes = new DefinedValues();
            radioTypes.ReadDatabase(cmd);
        }

        private void readRunwayOperations()
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_RunwayOperations", connection);
            runwayOperations = new DefinedValues();
            runwayOperations.ReadDatabase(cmd);
        }

        private void readRunwaySurfaces()
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tbl_RunwaySurface", connection);
            runwaySurfaces = new DefinedValues();
            runwaySurfaces.ReadDatabase(cmd);
        }


        private XDocument xml;
        public void OpenAirportFile(string Filename)
        {
            xml = XDocument.Load(Filename);

            var a = from aa in xml.Descendants("AIRPORT")
                    select new Airport(connection)
                    {
                        Type_ID = getIDFromDefinedValue(airportTypes, Xs.XsStringAttribute(aa,"TYPE")),
                        Country = Xs.XsStringElement(aa.Element("COUNTRY")),
                        Name = Xs.XsStringElement(aa.Element("NAME")),
                        Icao = Xs.XsStringElement(aa.Element("ICAO")),
                        geoLocation = new GeoLocation()
                        {
                            Lat = Xs.XsDoubleElement(aa.Element("GEOLOCATION").Element("LAT")),
                            Lon = Xs.XsDoubleElement(aa.Element("GEOLOCATION").Element("LON")),
                            Elev = Xs.XsDoubleElement(aa.Element("GEOLOCATION").Element("ELEV")),
                            Elev_Unit = Xs.XsStringAttribute(aa.Element("GEOLOCATION").Element("ELEV"), "UNIT")
                        }

                    };
            foreach (Airport aa in a)
            {
                log.Info(aa.Name);

                var b = from bb in xml.Descendants("AIRPORT")
                        where bb.Element("NAME").Value.ToString() == aa.Name
                        select bb;

                var radio = from rr in b.Descendants("RADIO")
                            select new Radio()
                            {
                                Category_ID = getIDFromDefinedValue(radioCategories, Xs.XsStringAttribute(rr,"CATEGORY")),
                                Type_ID = getIDFromDefinedValue(radioTypes, Xs.XsStringElement(rr.Element("TYPE"))),
                                Frequency = Xs.XsDoubleElement(rr.Element("FREQUENCY")),
                                TypeDesc = Xs.XsStringElement(rr.Element("TYPESPEC")),
                                Description = Xs.XsStringElement(rr.Element("DESCRIPTION"))
                            };

                aa.Radios = radio.ToList();

                var runways = from rr in b.Descendants("RWY")
                              select new Runway()
                              {
                                  Operations_ID = getIDFromDefinedValue(runwayOperations, Xs.XsStringAttribute(rr,"OPERATIONS")),
                                  Name = Xs.XsStringElement(rr.Element("NAME")),
                                  Sfc_ID = getIDFromDefinedValue(runwaySurfaces, Xs.XsStringElement(rr.Element("SFC"))),
                                  Length_Unit = Xs.XsStringAttribute(rr.Element("LENGTH"),"UNIT"),
                                  Length = Xs.XsDoubleElement(rr.Element("LENGTH")),
                                  Width_Unit = Xs.XsStringAttribute(rr.Element("WIDTH"),"UNIT"),
                                  Width = Xs.XsDoubleElement(rr.Element("WIDTH")),
                                  Strength = Xs.XsStringElement(rr.Element("STRENGTH")),
                                  Strength_Unit = Xs.XsStringAttribute(rr.Element("STRENGTH"),"UNIT"),
                                  
                              };

                aa.Runways = runways.ToList();

                foreach (Runway runway in aa.Runways)
                {

                    var dirs = from bb in b.Descendants("RWY")
                                     where bb.Element("NAME").Value.ToString() == runway.Name
                                     select bb.Elements("DIRECTION");

                    var directions = from dir in dirs.First()
                                     select new RunwayDirection()
                                     {
                                         Tc = Xs.XsInt32Attribute(dir, "TC"),
                                         Tora = (dir.Element("RUNS")==null) ? 0 : Xs.XsInt32Element(dir.Element("RUNS").Element("TORA")),
                                         Lda = (dir.Element("RUNS")==null) ? 0 : Xs.XsInt32Element(dir.Element("RUNS").Element("LDA")),
                                         Ils = (dir.Element("LANDINGAIDS") == null) ? "" : Xs.XsStringElement(dir.Element("LANDINGAIDS").Element("ILS")),
                                         Papi = (dir.Element("LANDINGAIDS") == null) ? "" : Xs.XsStringElement(dir.Element("LANDINGAIDS").Element("PAPI"))
                                     };

                    runway.RunwayDirections = directions.ToList();

                    log.Info("Runway: " + runway.Name);
                }

                this.Add(aa);
                StoreAirportInDB(aa);
            }

            log.Info("Airport file read!");
        }

        public void StoreAirportInDB(Airport a)
        {
            // INSERT INTO tbl_Airports (TYPE_ID, COUNTRY, NAME, ICAO) VALUES(@Type_ID, @Country, @Name, @Icao)
            // INSERT INTO tbl_GeoLocation (AIRPORT_ID, LAT, LON, ELEV_UNIT, ELEV) VALUES(@Airport_ID, @Lat, @Lon, @Elev_Unit, @Elev)
            // INSERT INTO tbl_Radio (AIRPORT_ID, CATEGORY_ID, FREQUENCY, TYPE_ID, TYPESPEC, DESCRIPTION) VALUES(@Airport_ID, @Category_ID, @Frequency, @Type_ID, @TypeSpec, @Description)
            //

            AirportSetTableAdapters.tbl_AirportsTableAdapter airportAdapter = new AirportSetTableAdapters.tbl_AirportsTableAdapter();
            airportAdapter.Connection = connection;
            airportAdapter.InsertQuery(a.Type_ID, a.Country, a.Name, a.Icao);
             
            Int32 airport_Id = Xs.GetLastID("tbl_Airports", connection);
            a.geoLocation.Airport_ID = airport_Id;
            //connection.Close();   

            AirportSetTableAdapters.tbl_GeoLocationTableAdapter geoAdapter = new AirportSetTableAdapters.tbl_GeoLocationTableAdapter();
            geoAdapter.Connection = connection;
            geoAdapter.InsertQuery(a.geoLocation.Airport_ID,
                a.geoLocation.Lat,
                a.geoLocation.Lon,
                a.geoLocation.Elev_Unit,
                a.geoLocation.Elev);

            AirportSetTableAdapters.tbl_RadioTableAdapter radioAdapter = new AirportSetTableAdapters.tbl_RadioTableAdapter();
            radioAdapter.Connection = connection;
            foreach (Radio ra in a.Radios)
            {
                radioAdapter.InsertQuery(airport_Id,
                    ra.Category_ID,
                    ra.Frequency,
                    ra.Type_ID,
                    ra.TypeDesc,
                    ra.Description);
            }


            AirportSetTableAdapters.tbl_RunwaysTableAdapter runwayAdapter = new AirportSetTableAdapters.tbl_RunwaysTableAdapter();
            runwayAdapter.Connection = connection;
            AirportSetTableAdapters.tbl_RunwayDirectionsTableAdapter dirAdapter = new AirportSetTableAdapters.tbl_RunwayDirectionsTableAdapter();
            dirAdapter.Connection = connection;
            
            foreach (Runway ru in a.Runways)
            {
                runwayAdapter.InsertQuery(airport_Id,
                    ru.Operations_ID,
                    ru.Name,
                    ru.Sfc_ID,
                    ru.Length_Unit,
                    ru.Length,
                    ru.Width_Unit,
                    ru.Width,
                    ru.Strength_Unit,
                    ru.Strength);

                Int32 runway_ID = Xs.GetLastID("tbl_Runways", connection);

                foreach (RunwayDirection rd in ru.RunwayDirections)
                {
                    dirAdapter.InsertQuery(runway_ID,
                        rd.Tc,
                        rd.Tora_Unit,
                        rd.Tora,
                        rd.Lda_Unit,
                        rd.Lda,
                        rd.Ils,
                        rd.Papi);
                }
            }

            log.Info("Inserted Airport: {0} with id {1}", a.Name, airport_Id);

            
        }

    }
}
