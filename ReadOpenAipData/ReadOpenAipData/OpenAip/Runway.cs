using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadOpenAipData.OpenAip
{
    public class Runway
    {
        public Int32 ID;
        public Int32 Operations_ID;
        public string Name;
        public Int32 Sfc_ID;
        public string Length_Unit;
        public double Length;
        public string Width_Unit;
        public double Width;
        public string Strength_Unit;
        public string Strength;
        public List<RunwayDirection> RunwayDirections;
    }
}
