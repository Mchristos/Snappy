using Snappy.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.XunitTests.Helpers
{
    public static class DataHelpers
    {
        public static RoadGraph BuildGraph1()
        {
            RoadGraph result = new RoadGraph();

            DirectedRoad roadAB = new DirectedRoad(1000, 1001, GeometryAB().ToCoordList(), "AB");
            DirectedRoad roadAC = new DirectedRoad(1000, 1002, GeometryAC().ToCoordList(), "AC");
            DirectedRoad roadAD = new DirectedRoad(1000, 1003, GeometryAD().ToCoordList(), "AD");
            DirectedRoad roadDB = new DirectedRoad(1003, 1001, GeometryDB().ToCoordList(), "DB");

            result.AddRoad(roadAB);
            result.AddRoad(roadAC);
            result.AddRoad(roadAD);
            result.AddRoad(roadDB);

            return result;
        }
        public static double[,] GeometryAB()
        {
            return new double[,]
            {
                 { -33.9444992073946, 18.4820938110352 },
                 { -33.9513344520844, 18.4869003295898 },
                 { -33.9607320171703, 18.4961700439453 },
                 { -33.9826556320584, 18.489990234375 },
                 { -33.9983118959444, 18.4910202026367 },
                 { -34.0062812494092, 18.4834671020508 },
                 { -34.0219331594475, 18.4855270385742 },
                 { -34.0333145541668, 18.4910202026367 },
                 { -34.0466857421595, 18.4882736206055 }
            };
        }

        public static double[,] GeometryAC()
        {
            return new double[,]
            {
                 { -33.9447484186656, 18.4829521179199 },
                 { -33.9348150044721, 18.4613227844238 },
                 { -33.9315392159552, 18.4443283081055 },
                 { -33.9296876275766, 18.4307670593262 },
                 { -33.9305422118304, 18.4189224243164 },
                 { -33.9333907640784, 18.4108543395996 },
                 { -33.9343877348552, 18.4084510803223 },
                 { -33.9358119585743, 18.4062194824219 },
                 { -33.9369513203983, 18.4034729003906 },
                 { -33.9385179180127, 18.3990097045898 },
                 { -33.9489136990368, 18.3832168579102 },
                 { -33.9598777359629, 18.3746337890625 }
            };
        }

        public static double[,] GeometryAD()
        {
            return new double[,]
            {
                 { -33.9443211988973, 18.4820508956909 },
                 { -33.9510496611821, 18.4947967529297 },
                 { -33.9487712996543, 18.5105895996094 },
                 { -33.9504800765184, 18.5284423828125 },
                 { -33.9601624973185, 18.5541915893555 },
                 { -33.965572781968, 18.5696411132813 },
                 { -33.980662809736, 18.585090637207 },
                 { -33.9954655170726, 18.6183929443359 }
            };
        }

        public static double[,] GeometryDB()
        {
            return new double[,]
            {
                 { -33.9954833072367, 18.6184304952621 },
                 { -34.0014428025519, 18.6338424682617 },
                 { -34.0159573161614, 18.6177062988281 },
                 { -34.022786817002, 18.5737609863281 },
                 { -34.0654587451934, 18.599853515625 },
                 { -34.0796779489294, 18.5456085205078 },
                 { -34.0569260774104, 18.5305023193359 },
                 { -34.0545083320242, 18.5143661499023 },
                 { -34.0451211375776, 18.4894752502441 }
            };
        }


        ////////////////


        // Graph where A->B, B->C is shorter than going A->C 
        public static RoadGraph BuildGraphTwoShortRoadsOneLong()
        {
            RoadGraph result = new RoadGraph();

            // Two short roads
            DirectedRoad roadAB = new DirectedRoad(1000, 1001, StraightAB().ToCoordList(), "AB");
            DirectedRoad roadBC = new DirectedRoad(1001, 1002, StraightBC().ToCoordList(), "BC");

            //Long road
            DirectedRoad roadAC = new DirectedRoad(1000, 1002, BigTriangleAC().ToCoordList(), "AC");

            result.AddRoad(roadAB);
            result.AddRoad(roadBC);
            result.AddRoad(roadAC);


            return result;
        }
        public static double[,] BigTriangleAC()
        {
            return new double[,]
            {
                { -32.1570124860701, 19.324951171875 }, //A
                { -28.2753582818171, 20.423583984375 },
                { -32.0732655510424, 20.98388671875 }  //C
            };
        }
        public static double[,] StraightAB()
        {
            return new double[,]
            {
                { -32.1570124860701, 19.324951171875 }, // A
                { -32.0732655510424, 20.0 }             // B
            };
        }
        public static double[,] StraightBC()
        {
            return new double[,]
            {
                { -32.0732655510424, 20.0 },          //B
                { -32.0732655510424, 20.98388671875 } //C
            };
        }

    }
}
