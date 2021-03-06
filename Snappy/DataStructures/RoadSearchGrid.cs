﻿using Snappy.Functions;
using Snappy.ValueObjects;
using System.Collections.Generic;

namespace Snappy.DataStructures
{
    public class RoadSearchGrid : SearchGrid<DirectedRoad>
    {
        public RoadSearchGrid(double left, double bottom, double gridSizeX, double gridSizeY, int cellCountX, int cellCountY) : base(left, bottom, gridSizeX, gridSizeY, cellCountX, cellCountY)
        {
        }
        
        public List<DirectedRoad> GetNearbyValues(Coord query, double radiusInMeters )
        {
            List<DirectedRoad> searchResult = GetNearbyValues(query.Longitude, query.Latitude);
            //return searchResult;
            var result = new List<DirectedRoad>();
            foreach (var road in searchResult)
            {
                int pos;
                var projectedDistance = query.HaversineDistance(query.SnapToPolyline(road.Geometry, out pos));
                if (projectedDistance.DistanceInMeters < radiusInMeters)
                {
                    result.Add(road);
                }
            }
            if (result.Count == 0)
            {
                return searchResult;
            }
            return result;
        }

        public void Populate(IEnumerable<DirectedRoad> roads)
        {
            foreach (var road in roads)
            {
                AddRoad(road);
            }
        }

        public void AddRoad(DirectedRoad road)
        {
            for (int i = 1; i < road.Geometry.Count; i++)
            {
                int[] startCell = this.GetGridCellOfPoint(road.Geometry[i - 1].Longitude, road.Geometry[i - 1].Latitude);
                int[] endCell = this.GetGridCellOfPoint(road.Geometry[i].Longitude, road.Geometry[i].Latitude);
                List<int[]> intersectedCells = GridAlgorithms.Bresenham(startCell[0], startCell[1], endCell[0], endCell[1]);
                foreach (var cell in intersectedCells)
                {
                    int x = cell[0];
                    int y = cell[1];
                    if (!this.ContainsKey(x))
                    {
                        this[x] = new Dictionary<int, List<DirectedRoad>>();
                    }
                    if (!this[x].ContainsKey(y))
                    {
                        this[x][y] = new List<DirectedRoad>();
                    }
                    this[x][y].Add(road);
                }
            }
        }

        // This should be inplemented for the base class too :( 
        public RoadSearchGrid Clone()
        {
            var result = new RoadSearchGrid(Left, Bottom, GridSizeX, GridSizeY, CellCountX, CellCountY);
            foreach (var int1 in this.Keys)
            {
                var dict = new Dictionary<int, List<DirectedRoad>>();
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