using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data.SQLite;
using System.Globalization;

namespace ReadOpenAipData.OpenAip
{
    public class Xs
    {
        public static string XsStringElement(XElement o)
        { return (o == null) ? "" : o.Value.ToString(); }
        public static string XsStringAttribute(XElement o, string Attribute)
        { 
            return (o == null) ? "" : o.Attribute(Attribute).Value.ToString(); 
        }
        public static double XsDoubleElement(XElement o)
        {
            try { return (o == null) ? 0 : Double.Parse(o.Value.ToString().Replace(',', '.'), CultureInfo.InvariantCulture); }
            catch { return 0; }
        }
        public static Int32 XsInt32Attribute(XElement o, string Attribute)
        {
            try { return (o == null) ? 0 : Convert.ToInt32(o.Attribute(Attribute).Value); }
            catch { return 0; }
        }
        public static Int32 XsInt32Element(XElement o)
        {
            try { return (o == null) ? 0 : Convert.ToInt32(o.Value); }
            catch { return 0; }
        }

        public static Int32 GetLastID(string Tablename, SQLiteConnection connection)
        {
            SQLiteCommand cmd = new SQLiteCommand("select seq from sqlite_sequence where name=@TableName", connection);
            cmd.Parameters.AddWithValue("@TableName", Tablename);
            try
            {
                Object r = cmd.ExecuteScalar();
                return Convert.ToInt32(r);
            }
            catch
            {
                return -1;
            }
        }
    }
}
