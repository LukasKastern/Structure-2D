using System;
using System.IO;
using Structure2D;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.AI;

namespace Structure2D
{
    /// <summary>
    /// Representation of a Block/Background inside the Map.
    /// </summary>
    [System.Serializable]
    public class Cell
    {
        /// <summary>
        /// Block ID of this Cell.
        /// </summary>
        public int Block
        {
            get => _block;
            set
            {
                //If the solid status of this block has changed the collider of the Chunk which we are currently mapped to has to be rebuilt
                if (BlockSolidStateMetaData.IsBlockSolid[value] != BlockSolidStateMetaData.IsBlockSolid[_block])
                    RequestRebuildCollider();
                
                _block = value;
                CellMap.ShaderData.RefreshShaderDataAtCell(this);
            }
        }

        /// <summary>
        /// Background ID of this Cell.
        /// </summary>
        public int Background
        {
            get => _background;
            set
            {
                _background = value;
                CellMap.ShaderData.RefreshShaderDataAtCell(this);
            }
        }

        internal void RequestRebuildCollider()
        {
            Chunk?.RequestRebuildCollider();
        }

        /// <summary>
        /// Coordinate of this Cell.
        /// </summary>
        public Coordinate Coordinate;

        /// <summary>
        /// Chunk to which this cell is mapped currently.
        /// Do not set this directly, this gets set by the Chunk.
        /// </summary>
        public Chunk Chunk;
        
        private int _background;
        private int _block;
    }

    public static class CellExtensions
    {
        /// <summary>
        /// Fetches the Neighbor at the given direction.
        /// </summary>
        /// <param name="direction">Direction from which we should fetch the Neighbor from</param>
        /// <returns>Returns the neighbor at the given direction, if there is none it returns null</returns>
        public static Cell GetNeighbor(this Cell cell, Direction direction)
        {
            var neighborCoordinate = cell.Coordinate;
          
            switch (direction)
            {
                case Direction.Left:
                    neighborCoordinate += new Coordinate(-1, 0);
                    break;
                case Direction.Right:
                    neighborCoordinate += new Coordinate(1, 0);
                    break;
                case Direction.Up:
                    neighborCoordinate += new Coordinate(0, 1);
                    break;
                case Direction.Down:
                    neighborCoordinate += new Coordinate(0, -1);
                    break;
                case Direction.UpRight:
                    neighborCoordinate += new Coordinate(1, 1);
                    break;
                
                case Direction.DownRight:
                    neighborCoordinate += new Coordinate(1, -1);
                    break;
                
                case Direction.DownLeft:
                    neighborCoordinate += new Coordinate(-1, -1);
                    break;
                case Direction.UpLeft:
                    neighborCoordinate += new Coordinate(-1, 1);
                    break;
                
                default:
                    return null;
            }

            return CellMap.GetCell(neighborCoordinate);
        }
    }
}