using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Structure2D
{
    /// <summary>
    /// The ChunkLoader handles the Loading/Unloading of Chunks based on the current Viewer Position
    /// </summary>
    [AddComponentMenu("Structure 2D/Framework Base/Chunk Loader")]
    public class ChunkLoader : MonoBehaviour
    {
        public static ChunkLoader SingleTon { get; private set; }
        
        /// <summary>
        /// Width of the Viewport in Chunks.
        /// </summary>
        public static int DesiredViewPortSizeX { get; private set; }
        
        /// <summary>
        /// Height of the Viewport in Chunks.
        /// </summary>
        public static int DesiredViewPortSizeY { get; private set; }

        /// <summary>
        /// This gets called when a chunk gets loaded,
        /// with the chunk offset of the unloaded chunk.
        /// </summary>
        public static Action<Vector2Int> ChunkLoaded;
        
        /// <summary>
        /// This gets called when a chunk gets unloaded,
        /// with the chunk offset of the unloaded chunk.
        /// </summary>
        public static Action<Vector2Int> ChunkUnloaded;
        
        /// <summary>
        /// Viewer on which position we base the viewport on
        /// </summary>
        public GameObject Viewer;
    
        [SerializeField] 
        private int _viewDistanceX;
    
        [SerializeField] 
        private int _viewDistanceY;

        private Dictionary<Vector2Int, Chunk> _activeChunks => Chunk.LoadedChunks;// new Dictionary<Vector2Int, Chunk>();

        private HashSet<Vector2Int> _chunksActiveThisFrame = new HashSet<Vector2Int> ();
    
        private HashSet<Vector2Int> _chunksActiveLastFrame = new HashSet<Vector2Int> ();

        private Vector3 _lastViewerPosition = Vector3.zero;
    
        private int MaxHorizontalChunks => CellMap.MapWidth / CellMetrics.ChunkSize;
    
        private int MaxDownChunks => (CellMap.MapHeight) / CellMetrics.ChunkSize;

        private void Awake()
        {
            if (SingleTon != null)
            {
                Debug.LogWarning("There's already a ChunkLoader active");
                this.enabled = false;
                return;
            }

            CellMap.MapInitialized += RefreshActiveChunks;
            
            SingleTon = this;
            
            DesiredViewPortSizeX = _viewDistanceX * 2;
            DesiredViewPortSizeY = _viewDistanceY * 2;

            if (_activeChunks == null)
            {
                Chunk.LoadedChunks = new Dictionary<Vector2Int, Chunk>();
            }

            if (Viewer == null)
                Viewer = GameObject.FindObjectOfType<Camera>()?.gameObject;
        }

        /// <summary>
        /// Refresh all chunks, this is useful when a new Map got generated.
        /// </summary>
        private void RefreshActiveChunks()
        {
            foreach (var activeChunk in _activeChunks)
            {
                //If the active chunk is not in the current map bounds we continue
                if(activeChunk.Key.x > CellMap.MapWidth / CellMetrics.ChunkSize || 
                   activeChunk.Key.y > CellMap.MapHeight / CellMetrics.ChunkSize)
                    continue;
                
                SetChunk(activeChunk.Value, activeChunk.Key.x, activeChunk.Key.y);
            }   
        }

        private void OnValidate()
        {
            _viewDistanceX = Mathf.Clamp(_viewDistanceX, 1, int.MaxValue);
            _viewDistanceY = Mathf.Clamp(_viewDistanceY, 1, int.MaxValue);
        }
    
        public void Update()
        {
            if (CellMap.IsMapHidden)
                return;
            
            if(Viewer.transform.position == _lastViewerPosition)
                return;
        
            _lastViewerPosition = Viewer.transform.position;

            var viewerChunk = GetViewerChunk();
        
            UpdateChunks(viewerChunk.x, viewerChunk.y);
        }

        private void CalculateViewportSize()
        {
            var start = GetStartCoordinateOfViewport();
            var end = GetEndCoordinateOfViewport();

            var _viewport = new Viewport();
            
            _viewport.BottomLeft = start;
            _viewport.Width = end.x - start.x;
            _viewport.Height = end.y - start.y;

            Viewport.CurrentViewport = _viewport;
        }

        private Vector2Int GetViewerChunk()
        {
            float chunkSize = CellMetrics.ChunkSize * CellMetrics.CellSize;

            int viewerChunkX = Mathf.RoundToInt(Viewer.transform.position.x / chunkSize );
            int viewerChunkY = Mathf.RoundToInt(Viewer.transform.position.y / chunkSize);
        
            return new Vector2Int(viewerChunkX, viewerChunkY);
        }

        private void UpdateChunks(int xOffset, int yOffset)
        {
            _chunksActiveThisFrame.Clear();

            List<Vector2Int> newChunks = ListPool<Vector2Int>.Get();

            for (int x = -_viewDistanceX; x < _viewDistanceX; ++x)
            {
                for (int y = -_viewDistanceY; y < _viewDistanceY; ++y)
                {
                    var coordinate = new Vector2Int(x + xOffset, y + yOffset);

                    if (coordinate.x > (MaxHorizontalChunks - 1) || coordinate.x < -MaxHorizontalChunks)
                        continue;

                    else if (coordinate.y < (-MaxDownChunks - 1) || coordinate.y > (MaxDownChunks - 1))
                        continue;

                    if (coordinate.x < 0 || coordinate.y < 0)
                        continue;
                    
                    //If all the upper conditions are met, we want this chunk to be active
                    //For this we check if this chunk is already in the active chunks
                    //If it's not we add it to the chunks that want to be set to active,
                    if (!_activeChunks.ContainsKey(coordinate))
                    {
                        newChunks.Add(coordinate);
                    }

                    _chunksActiveThisFrame.Add(coordinate);
                }
            }

            //Now we compare the chunks that have been active the last frame with the ones that are active this frame
            //This way we clean up all the not used chunks and add them back to the pool, so we can reuse them for the chunks
            //that came in the viewer position this frame
            foreach (var chunkDrawnLastFrame in _chunksActiveLastFrame)
            {
                //This chunk doesn't get drawn this frame so we have to add it to the pool so it can be reused
                if (!_chunksActiveThisFrame.Contains(chunkDrawnLastFrame))
                {
                    ChunkUnloaded?.Invoke(_activeChunks[chunkDrawnLastFrame].Offset);
                    _activeChunks[chunkDrawnLastFrame].AddToPool();
                }
            }

            foreach (var chunk in newChunks)
            {
                var chunkPool = ChunkPool.Get();
                _activeChunks.Add(new Vector2Int(chunk.x, chunk.y), chunkPool);
                SetChunk(chunkPool, chunk.x, chunk.y);
            }

            ListPool<Vector2Int>.Add(newChunks);

            _chunksActiveLastFrame.Clear();

            foreach (var chunk in _chunksActiveThisFrame)
            {
                _chunksActiveLastFrame.Add(chunk);
            }
        }

        private void SetChunk(Chunk chunk, int coordinateX, int coordinateY)
        {
            chunk.SetToOffset(coordinateX, coordinateY);
            
            ChunkLoaded?.Invoke(chunk.Offset);
        }
        
        private void LateUpdate()
        {
            if (CellMap.IsMapHidden)
                return;
                
            //Only rebuilt one chunk a frame till we have multithreading
            while (Chunk.ChunksThatRequestsRebuildsOfTheirColliders.Count > 0)
            {
                var chunk = Chunk.ChunksThatRequestsRebuildsOfTheirColliders[0];
            
                chunk.GenerateColliders();
                Chunk.ChunksThatRequestsRebuildsOfTheirColliders.RemoveAt(0);
            }
            
            CalculateViewportSize();

            Shader.SetGlobalVector("_ViewerPos", new Vector4(Mathf.FloorToInt(Viewport.CurrentViewport.BottomLeft.x) + Mathf.FloorToInt(Viewport.CurrentViewport.BottomLeft.y) * (ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize), 0, 0));
    }

        private  Coordinate GetStartCoordinateOfViewport()
        {
            int viewerCellX = Mathf.FloorToInt(SingleTon.Viewer.transform.position.x / CellMetrics.CellSize);
            int viewerCellY = Mathf.FloorToInt(SingleTon.Viewer.transform.position.y / CellMetrics.CellSize);

            //To-Do: Im not sure if the -3 etc will be correct when we change the viewdistance from 7 to something else
            int startX = viewerCellX - (SingleTon._viewDistanceX) * CellMetrics.ChunkSize;
            int startY = viewerCellY - (SingleTon._viewDistanceY ) * CellMetrics.ChunkSize;

            int minHorizontalCell = 0;
            int maxHorizontalCell = SingleTon.MaxHorizontalChunks * CellMetrics.ChunkSize - 1;

            int minVerticalCell = 0;
            int maxVerticalCell = SingleTon.MaxDownChunks * CellMetrics.ChunkSize - 1;

            startX = Mathf.Clamp(startX, minHorizontalCell, maxHorizontalCell);
            startY = Mathf.Clamp(startY, minVerticalCell, maxVerticalCell);
        
            return new Coordinate(startX, startY);
        }

        private Coordinate GetEndCoordinateOfViewport()
        {
            int viewerCellX = Mathf.FloorToInt(SingleTon.Viewer.transform.position.x / CellMetrics.CellSize);
            int viewerCellY = Mathf.FloorToInt(SingleTon.Viewer.transform.position.y / CellMetrics.CellSize);

            int endX = viewerCellX + (SingleTon._viewDistanceX) * CellMetrics.ChunkSize;
            int endY = viewerCellY + (SingleTon._viewDistanceY) * CellMetrics.ChunkSize;

            int minHorizontalCell = 0;
            int maxHorizontalCell = SingleTon.MaxHorizontalChunks * CellMetrics.ChunkSize;

            int minVerticalCell = 0;
            int maxVerticalCell = SingleTon.MaxDownChunks * CellMetrics.ChunkSize;
        
            endX = Mathf.Clamp(endX, minHorizontalCell, maxHorizontalCell);
            endY = Mathf.Clamp(endY, minVerticalCell, maxVerticalCell);
        
            return new Coordinate(endX, endY );
        }
        
        private void OnDestroy()
        {
            if (SingleTon == this)
            {
                SingleTon = null;
                CellMap.MapInitialized -= RefreshActiveChunks;
            }
        }
    }

    public struct Viewport
    {
        /// <summary>
        /// The current Viewport.
        /// This is set by the ChunkLoader.
        /// </summary>
        public static Viewport CurrentViewport { get; internal set; }
        
        /// <summary>
        /// Bottom left coordinate of the Viewport.
        /// </summary>
        public Coordinate BottomLeft;
        
        /// <summary>
        /// Width of the Viewport.
        /// </summary>
        public int Width;
        
        /// <summary>
        /// Height of the Viewport.
        /// </summary>
        public int Height;

        /// <summary>
        /// Checks if the given coordinate is in the Viewport.
        /// </summary>
        /// <param name="coordinate">Coordinate to check.</param>
        /// <returns>Returns whether the coordinate is in the viewport.</returns>
        public bool ContainsCoordinate(Coordinate coordinate)
        {
            if (coordinate.x < BottomLeft.x || coordinate.x >= BottomLeft.x + Width)
                return false;
            else if (coordinate.y < BottomLeft.y || coordinate.y >= BottomLeft.y + Height)
                return false;

            return true;
        }
        
        
    }
}