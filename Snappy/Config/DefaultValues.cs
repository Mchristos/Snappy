using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Config
{
    public class DefaultValues
    {
        public const double GPS_Error_In_Meters = 20;

        /****************************  map matching parameters ****************************/
        public const double Sigma_Value_In_Meters_For_Emissions = GPS_Error_In_Meters;
        public const double Nearby_Road_Radius_In_Meters = 60;

        public const double Beta_For_Transitions_In_Meters = 5;
        public const double Difference_Threshold_For_Transitions_In_Meters = 1000;
        /****************************  _______________________ ****************************/

        /****************************  threshold values ****************************/
        public const double Dijstra_Upper_Search_Limit_In_Meters                    = 1000;
        public const double Too_Fast_In_Kilometres_Per_Hour_For_Coordinate_Cleaning = 200;
        /****************************  ________________ ****************************/
    }
}
