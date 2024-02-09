using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Structure2D.Lighting;
using Structure2D.Utility;
using UnityEngine;
using Profiler = UnityEngine.Profiling.Profiler;

namespace Structure2D
{
    /// <summary>
    /// This is the storage class for all the Cells.
    /// It also provides convenient methods of fetching Cells.
    /// </summary>
    public static class CellMap
    {
        /// <summary>
        /// This gets called when a new Map Gets Generated.
        /// </summary>
        public static Action MapUnloaded;
        
        /// <summary>
        /// Current Map Height In Cells.
        /// </summary>
        public static int MapHeight { get; private set; }
    
        /// <summary>
        /// Current Map Width in Cells.
        /// </summary>
        public static int MapWidth { get; private set; }
        
        internal static TerrainShaderData ShaderData => TerrainShaderData.ShaderData;
       
        /// <summary>
        /// This is used to pause Updating components which are dependent on the CellMap.
        /// This is useful when you want to access Cells from a separate thread,
        /// so you don't have to worry about race conditions.
        /// </summary>
        public static bool IsMapHidden { get; private set; }

        /// <summary>
        /// This gets called when the Map leaves the hidden state.
        /// </summary>
        public static Action MapInitialized;

        //This is the data of the the active map
        private static Cell[,] _cells;

        /// <summary>
        /// Creates a map, call SetMapVisible to show the Map.
        /// doesn't initialize the ShadeData or the BlockLighting.
        /// Call SetMapVisible to initialize the ShaderData and the BlockLighting.
        /// </summary>
        internal static void CreateMap(int chunksInWidth, int chunksInHeight)
        {
            if (_cells != null)
            {
                MapUnloaded?.Invoke();
            }
                
            
            _cells = new Cell[chunksInWidth * CellMetrics.ChunkSize, chunksInHeight * CellMetrics.ChunkSize];
            IsMapHidden = true;
            TerrainShaderData.InitializeSingleTon();
            CreateCellData();
        }
        
        /// <summary>
        /// Sets the current Map Visible.
        /// This also initializes the current ShaderTexture + BlockLighting.
        /// </summary>
        internal static void SetMapVisible()
        {
            IsMapHidden = false;
            MapWidth = _cells.GetLength(0);
            MapHeight = _cells.GetLength(1);
            
            ShaderData.CreateTexture();
            BlockLighting.Initialize();

            for (int x = 0; x < MapWidth; ++x)
            {
                for (int y = 0; y < MapHeight; ++y)
                {
                    var cell = GetCell(x, y);
                    TerrainShaderData.ShaderData.RefreshShaderDataAtCell(cell);
                }
            }
            
            MapInitialized?.Invoke();
        }

        /// <summary>
        /// Creates Cells based upon the current MapWidth and MapHeight
        /// This should be used after resizing the Map.
        /// </summary>
        private static void CreateCellData()
        {
            for (int x = 0; x < _cells.GetLength(0); ++x)
            {
                for (int y = 0; y < _cells.GetLength(1); ++y)
                {
                    _cells[x, y] = new Cell() {Coordinate = new Coordinate(x, y), Block = 2, Background = 0};
                }
            }
        }
        
        #region FunctionsToRetrieveCells

        /// <summary>
        /// This function returns the cell at the given index.
        /// Before returning this functions checks if the given coordinate was in bounds of the CellMap.
        /// If it wasn't it returns null
        /// </summary>
        /// <param name="x">X coordinate of the desired Cell</param>
        /// <param name="y">Y coordinate of the desired Cell</param>
        public static Cell GetCell(int x, int y)
        {
            if (!IsInCellDataBounds(x, y))
            {
                return null;
            }
        
            return GetCellUnsafe(x, y);
        }
        
        /// <summary>
        /// Returns the cell at the given coordinate
        /// </summary>
        /// <param name="coordinate">Coordinate of the desired Cell.</param>
        /// <returns></returns>
        public static Cell GetCell(Coordinate coordinate) => GetCell(coordinate.x, coordinate.y);

        /// <summary>
        /// Returns the Cell at the given index.
        /// The difference to GetCell is that this call doesn't check if the index is in bounds of the CellMap.
        /// </summary>
        /// <returns></returns>
        public static Cell GetCellUnsafe(int x, int y)
        {
            return _cells[x, y];
        }
        
        /// <summary>
        /// This function checks if the given index is in bounds of the CellMap.
        /// </summary>
        private static bool IsInCellDataBounds(int x, int y)
        {
            if (_cells == null)
                return false;
            
            if (x < 0 || x > _cells.GetLength(0) - 1)
                return false;
            else if (y < 0 || y > _cells.GetLength(1) - 1)
                return false;
        
            return true;
        }

        /// <summary>
        /// Fetches the chunk at the given offset.
        /// If there is no CHunk at the offset it returns null.
        /// </summary>
        /// <param name="offset">Offset of the desired Chunk.</param>
        /// <returns></returns>
        public static Chunk GetChunkAtOffset(Vector2Int offset)
        {
            return !Chunk.LoadedChunks.ContainsKey(offset) ? null : Chunk.LoadedChunks[offset];
        }
        
        
        /// <summary>
        /// Returns the chunk at the given point in world space.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static Chunk WorldPointToChunk(Vector2 worldPoint)
        {
            var chunkX = Mathf.FloorToInt(worldPoint.x / (CellMetrics.ChunkSize * CellMetrics.CellSize));
            var chunkY = Mathf.FloorToInt(worldPoint.y / (CellMetrics.ChunkSize * CellMetrics.CellSize));
        
            return GetChunkAtOffset(new Vector2Int(chunkX, chunkY));
        }
    
        
        public static Vector2Int WorldPointToChunkOffset(Vector3 worldPoint)
        {            
            var chunkX = Mathf.FloorToInt(worldPoint.x / (CellMetrics.ChunkSize * CellMetrics.CellSize));
            
            var chunkY = Mathf.FloorToInt(worldPoint.y / (CellMetrics.ChunkSize * CellMetrics.CellSize));

            return new Vector2Int(chunkX, chunkY);
        }
        
        /// <summary>
        /// Returns the chunk at the given mouse position
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        public static Chunk MousePositionToChunk(Vector2 mousePosition)
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        
            var chunk = WorldPointToChunk(worldPoint);

            return chunk;
        }

        /// <summary>
        /// Returns the Cell at the given world point.
        /// </summary>
        /// <param name="worldPoint">Point in world space.</param>
        public static Cell GetCellAtWorldPoint(Vector2 worldPoint)
        {
            var xCoordinate = Mathf.FloorToInt(worldPoint.x / CellMetrics.CellSize);
            var yCoordinate = Mathf.FloorToInt(worldPoint.y / CellMetrics.CellSize);

            return GetCell(xCoordinate, yCoordinate);
        }
        
        public static List<Cell> GetCellsInBounds(Vector2 screenPosition, int bounds)
        {
            if (bounds < 0)
            {
                throw new ArgumentException("Bounds have to be positive");
            }
            
            List<Cell> cells = ListPool<Cell>.Get();
        
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPosition, Camera.MonoOrStereoscopicEye.Mono);

            for (int y = 0; y <= bounds; ++y)
            {
                var yCellSize = (float) (CellMetrics.CellSize * y);
                var upCoordinate = new Vector2(worldPoint.x, worldPoint.y + yCellSize);
                var downCoordinate = new Vector2(worldPoint.x, worldPoint.y + -yCellSize);
            
                cells.Add(GetCellAtWorldPoint(upCoordinate));
                cells.Add(GetCellAtWorldPoint(downCoordinate));
            
                for (int x = 0; x <= bounds; ++x)
                {
                    var xCellSize = x * (float) (CellMetrics.CellSize);
                
                    var bottomLeft = new Vector2(worldPoint.x - xCellSize, worldPoint.y - yCellSize);
                    cells.Add(GetCellAtWorldPoint(bottomLeft));
                
                    var bottomRight  = new Vector2(worldPoint.x + xCellSize, worldPoint.y - yCellSize);

                    var topLeft = new Vector2(worldPoint.x - xCellSize, worldPoint.y + yCellSize);
                    var topRight = new Vector2(worldPoint.x + xCellSize, worldPoint.y + yCellSize);
                
                    cells.Add(GetCellAtWorldPoint(topLeft));
                    cells.Add(GetCellAtWorldPoint(topRight));

                    cells.Add(GetCellAtWorldPoint(bottomRight));
                }
            }

            return cells;
        }

        /// <summary>
        /// Fetches the Cell at the given position in Screen Space.
        /// </summary>
        /// <param name="screenPosition">Position in Screen Space of the desired Cell.</param>
        public static Cell ScreenPositionToCell(Vector2 screenPosition)
        {
            return GetCell(Coordinate.FromScreenPoint(screenPosition));
        }
        
        #endregion

        internal static void SetCellAtIndex(int x, int y, Cell cell)
        {
            _cells[x, y] = cell;
        }
    }
}
