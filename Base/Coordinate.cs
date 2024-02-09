using System;
using System.Collections.Generic;
using UnityEngine;

namespace Structure2D
{
    [System.Serializable]
    public struct Coordinate
    {
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y;
    
        /// <summary>
        /// Returns the distance from this coordinate to the given one
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int DistanceTo(Coordinate other)
        {
            var xDistance = x < other.x ? other.x - x : x - other.x;
            var yDistance = y < other.y ? other.y - y : y - other.y;

            return (xDistance + yDistance);
        }

        
        public static Vector3 ToWorldPoint(Coordinate coordinate)
        {
            return ToWorldPoint(coordinate, CoordinateAnchor.MiddleCenter);
        } 

        public static Vector3 ToWorldPoint(Coordinate coordinate, CoordinateAnchor anchor)
        {
            var worldPoint = new Vector3(coordinate.x * CellMetrics.CellSize  + CellMetrics.CellSize / 2, coordinate.y * CellMetrics.CellSize + CellMetrics.CellSize / 2);

            Vector3 _offset;
            
            switch (anchor)
            {
                case CoordinateAnchor.UpperLeft:
                    _offset = new Vector2(-CellMetrics.CellSize / 2, CellMetrics.CellSize / 2);
                    break;
                case CoordinateAnchor.UpperCenter:
                    _offset = new Vector2(0, CellMetrics.CellSize / 2);
                    break;
                case CoordinateAnchor.UpperRight:
                    _offset = new Vector2(CellMetrics.CellSize / 2, CellMetrics.CellSize / 2);
                    break;
                case CoordinateAnchor.MiddleLeft:
                    _offset = new Vector2(-CellMetrics.CellSize / 2, 0);
                    break;
                case CoordinateAnchor.MiddleCenter:
                    _offset = new Vector2(0, 0);
                    break;
                case CoordinateAnchor.MiddleRight:
                    _offset = new Vector2(CellMetrics.CellSize / 2, 0);
                    break;
                case CoordinateAnchor.LowerLeft:
                    _offset = new Vector2(-CellMetrics.CellSize / 2, -CellMetrics.CellSize / 2);
                    break;
                case CoordinateAnchor.LowerCenter:
                    _offset = new Vector2(0, -CellMetrics.CellSize / 2);
                    break;
                case CoordinateAnchor.LowerRight:
                    _offset = new Vector2(CellMetrics.CellSize / 2, -CellMetrics.CellSize / 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }

            return worldPoint + _offset;
        }
        
        /// <summary>
        /// Creates a coordinate from the given ScreenPoint
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Coordinate FromScreenPoint(Vector2 screenPoint)
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPoint, Camera.MonoOrStereoscopicEye.Mono);

            return FromWorldPoint(worldPoint);
        }

        public static Coordinate FromWorldPoint(Vector2 worldPoint)
        {
            var cellX = (Mathf.CeilToInt(worldPoint.x / CellMetrics.CellSize) - 1);
            var cellY = (Mathf.CeilToInt(worldPoint.y / CellMetrics.CellSize) - 1);
        
            return new Coordinate(cellX, cellY);
        }

        /// <summary>
        /// Returns the index of this coordinate as it's 1D representation
        /// </summary>
        /// <returns></returns>
        public int GetIndex()
        {
            return ToIndex(x, y);
        }

        /// <summary>
        /// This converts the given 2D index to a 1D index
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ToIndex(int x, int y)
        {
            return x + CellMap.MapWidth * y;
        }
    
        public override string ToString()
        {
            return x + "  " + y;
        }
    
        #region Operators
    
        public static Coordinate operator+ (Coordinate a, Coordinate b)
        {
            return new Coordinate(a.x + b.x, a.y + b.y);
        }

        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.x - b.x, a.y - b.y);
        }

        public static bool operator ==(Coordinate a, Coordinate b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !(a == b);
        }

        public static Coordinate operator *(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.x * b.x, a.y * b.y);
        }

        public static Coordinate operator *(Coordinate a, int b)
        {
            return a * new Coordinate(b, b);
        }

        public static implicit operator Coordinate(Vector2Int vectorToTransform)
        {
            return new Coordinate(vectorToTransform.x, vectorToTransform.y);
        }
    
        #endregion

        public static Coordinate Up => new Coordinate(0, 1);
        public static Coordinate Down => new Coordinate(0, -1);
        public static Coordinate Left => new Coordinate(-1, 0);
        public static Coordinate Right => new Coordinate(1, 0);
        public static Coordinate One => new Coordinate(1, 1);
    }

    /// <summary>
    /// Used to transform a coordinate into WorldSpace.
    /// </summary>
    public enum CoordinateAnchor 
    {
        UpperLeft,

        UpperCenter,

        UpperRight,

        MiddleLeft,

        MiddleCenter,

        MiddleRight,

        LowerLeft,
 
        LowerCenter,
    
        LowerRight,
    }
}