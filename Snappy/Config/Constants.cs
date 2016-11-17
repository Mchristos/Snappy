using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Config
{
    public static class Constants
    {
        //public const double Earth_Radius_In_Meters = 6372800;
        public const double Earth_Radius_In_Meters = 6371000;

        public const double Search_Grid_Grid_Size_In_Meters = 50;

        public const double Sigma_Value_In_Meters_For_Emissions = GPS_Error_In_Meters;

        public const double Beta_For_Transitions_In_Meters = 1;

        // don't try pathfind further than 1km!
        public const double Dijstra_Upper_Search_Limit_In_Meters = 2000;

        public const double GPS_Error_In_Meters = 20;

        public const double Too_Fast_In_Kilometres_Per_Hour_For_Coordinate_Cleaning = 200;
    }
}
