using System;
using System.Collections.Generic;

namespace Snappy.Functions
{
    public static class GridAlgorithms
    {
        /// <summary>
        /// Finds all cells lying on the straight line connecting a pair of cells in a grid.
        /// [ I stole this code online ]
        /// </summary>
        /// <param name="x0"> x co-ordinate of the first cell </param>
        /// <param name="y0"> y co-ordinate of the first cell </param>
        /// <param name="x1"> x co-ordinate of the second cell</param>
        /// <param name="y1"> y co-ordinate of the second cell</param>
        /// <remarks> Increasing y goes from top to bottom. Increasing x goes from left to right.
        /// </remarks>
        /// <returns> List of cells lying on the straight line connecting the two cells. </returns>
        public static List<int[]> Bresenham(int x0, int y0, int x1, int y1)
        {
            //TODO: verify that parameters specify valid cell
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            List<int[]> returnList = new List<int[]>();
            for (int x = x0; x <= x1; x++)
            {
                //Point((steep ? y : x), (steep ? x : y));
                returnList.Add(new int[2] { steep ? y : x, steep ? x : y });
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            return returnList;
        }

        /// <summary>
        /// Gets all cells (inclusive) surrounding a cell in a grid of given dimensions
        /// </summary>
        /// <param name="cell"> cell in grid in form [x,y] </param>
        /// <param name="gridDimensionX"></param>
        /// <param name="gridDimensionY"></param>
        /// <returns></returns>
        public static List<int[]> GetSurroundingCells(this int[] cell, int gridDimensionX, int gridDimensionY)
        {
            List<int[]> result = new List<int[]>();

            int x = cell[0];
            int y = cell[1];

            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (-1 < i && i < gridDimensionX && -1 < j && j < gridDimensionY)
                    {
                        result.Add(new int[] { i, j });
                    }
                }
            }
            return result;
        }
    }
}