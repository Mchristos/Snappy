using Snappy.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snappy.DataStructures
{
    //Warning : data of type T should not have colliding hashes
    public class SearchGrid<T> : Dictionary<int, Dictionary<int, List<T>>>
    {
        public double Left { get; set; }
        public double Bottom { get; set; }

        public double GridSizeX { get; set; }
        public double GridSizeY { get; set; }

        public int CellCountX { get; set; }
        public int CellCountY { get; set; }

        public SearchGrid(double left, double bottom, double gridSizeX, double gridSizeY, int cellCountX, int cellCountY) : base()
        {
            Left = left;
            Bottom = bottom;
            GridSizeX = gridSizeX;
            GridSizeY = gridSizeY;
            CellCountX = cellCountX;
            CellCountY = cellCountY;

            //Initialize dictionary entries
            for (int i = 0; i < cellCountX; i++)
            {
                this[i] = new Dictionary<int, List<T>>();
                for (int j = 0; j < cellCountY; j++)
                {
                    this[i][j] = new List<T>();
                }
            }

        }

        public int[] GetGridCellOfPoint(double x, double y)
        {
            if (!(this.Bottom < y) || !(y < (this.Bottom + CellCountY * GridSizeY)) ||
                !(this.Left < x) || !(x < (this.Left + CellCountX * GridSizeX)))
            {
                throw new ArgumentException("Values outside of grid range");
            }

            int gridCellY = (int)((y - Bottom) / GridSizeY);
            int gridCellX = (int)((x - Left) / GridSizeX);
            return new int[2] { gridCellX, gridCellY };
        }

        public List<T> GetNearbyValues(double queryX, double queryY)
        {
            // TO DO: Assumes hashes don't collide for type T. Should implement IEquatable?
            var result = new HashSet<T>();
            int[] gridcell = GetGridCellOfPoint(queryX, queryY);
            List<int[]> cells = gridcell.GetSurroundingCells(CellCountX, CellCountY);
            foreach (var cell in cells)
            {
                var valuesInCell = this[cell[0]][cell[1]];
                result.UnionWith(valuesInCell);
            }
            return result.ToList();
        }

        public SearchGrid<T> Clone()
        {
            var result = new SearchGrid<T>(Left, Bottom, GridSizeX, GridSizeY, CellCountX, CellCountY);
            foreach (var int1 in this.Keys)
            {
                var dict = new Dictionary<int, List<T>>();
                foreach (var int2 in this[int1].Keys)
                {
                    dict[int2] = this[int1][int2];
                }
                result[int1] = dict;
            }
            return result;
        }
    }
}