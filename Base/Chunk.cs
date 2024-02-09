using System.Collections.Generic;
using Structure2D.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Structure2D
{
    /// <summary>
    /// Chunks handle the drawing and collider generation of Cells.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// The Material with which the Chunks get drawn.
        /// </summary>
        internal static Material ChunkMaterial;
        
        internal static List<Chunk> ChunksThatRequestsRebuildsOfTheirColliders = new List<Chunk>();
        
        /// <summary>
        /// Chunks that are currently loaded.
        /// </summary>
        internal static Dictionary<Vector2Int, Chunk> LoadedChunks;

        /// <summary>
        /// Is the Chunk currently Visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
            
                return _isVisible && _hasData;
            }

            set => _isVisible = value;
        }
    
        /// <summary>
        /// Offset of this Chunk.
        /// </summary>
        public Vector2Int Offset
        {
            get => _offset;
            private set
            {
                _offset = value;

                var newPosition = new Vector3(_offset.x * CellMetrics.ChunkSize * CellMetrics.CellSize,
                    _offset.y * CellMetrics.ChunkSize * CellMetrics.CellSize);
                _transformMatrix = Matrix4x4.TRS(newPosition, Quaternion.identity, Vector3.one);
            }
        }
    
        /// <summary>
        /// Offset of this Chunk in Cells.
        /// </summary>
        public Vector2Int CellOffset { get; private set; }
    
        private MeshCollider _meshCollider;
    
        private PolygonCollider2D _polygonCollider2D;
    
        private Mesh _mesh;

        private bool _isVisible;
    
        private static List<Vector3> _vertices;
        private static List<int> _triangles;
        private static List<Vector2> _uvs;
        private static Bounds _meshBounds;
        
        private Matrix4x4 _transformMatrix;
    
        private bool _hasData;
    
        private Vector2Int _offset;

        /// <summary>
        /// This is the list of the colliders which this Chunk currently uses
        /// </summary>
        private readonly List<EdgeCollider2D> _activeColliders = new List<EdgeCollider2D>();

        internal Chunk()
        {
            if (LoadedChunks == null)
                LoadedChunks = new Dictionary<Vector2Int, Chunk>();
        }

        /// <summary>
        /// Creates a mesh from the default Mesh.
        /// </summary>
        internal void Initialize()
        {
            Offset = new Vector2Int(int.MaxValue, int.MaxValue);

            _mesh = CreateMesh();
        }

        /// <summary>
        /// This initializes the default mesh data, if the mesh data already exists this function will just exit without recreating it.
        /// </summary>
        private static void InitializeMeshData()
        {
            if(_vertices != null && _uvs != null && _triangles != null)
                return;
            
            _vertices = new List<Vector3>();
            _uvs = new List<Vector2>();
            _triangles = new List<int>();
            
            
            _meshBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(CellMetrics.CellSize * CellMetrics.ChunkSize, CellMetrics.CellSize * CellMetrics.ChunkSize));
    
            for (int x = 0; x < CellMetrics.ChunkSize; ++x)
            {
                for (int y = 0; y < CellMetrics.ChunkSize; ++y)
                {
                    var relativePosition = new Vector3(x * CellMetrics.CellSize, y* CellMetrics.CellSize);
                
                    //Bottom Left
                    _vertices.Add(new Vector3(0, 0, 0) + relativePosition);
                    _vertices.Add(new Vector3(CellMetrics.CellSize, 0, 0) + relativePosition);                
                    _vertices.Add(new Vector3(CellMetrics.CellSize, CellMetrics.CellSize, 0) + relativePosition);
                    _vertices.Add(new Vector3(0, CellMetrics.CellSize, 0) + relativePosition);
                
                    int verticesCounter = _vertices.Count - 1;
        
                    _triangles.Add(verticesCounter - 3);
                    _triangles.Add(verticesCounter - 1);
                    _triangles.Add(verticesCounter - 2);
        
                    _triangles.Add(verticesCounter - 3);
                    _triangles.Add(verticesCounter);
                    _triangles.Add(verticesCounter - 1);
                
                    _uvs.Add(new Vector2(1, 1));
                    _uvs.Add(new Vector2(0, 1));
                    _uvs.Add(new Vector2(0, 0));
                    _uvs.Add(new Vector2(1, 0));
                }
            }
        }
    
        /// <summary>
        /// This creates and returns a mesh from the default Mesh Data
        /// </summary>
        private static Mesh CreateMesh()
        {
            //This doesn't add a noticeable overhead so we can just use it every frame
            InitializeMeshData();
     
            var mesh = new Mesh();

            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);
            mesh.SetUVs(0, _uvs);
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// Sets this Chunk to a given Offset.
        /// This remaps the Block UVs of this chunk and generates new colliders.
        /// </summary>
        /// <param name="offsetX">new X offset of this Chunk</param>
        /// <param name="offsetY">new Y offset of this Chunk</param>
        internal void SetToOffset(int offsetX, int offsetY)
        {
            Offset = new Vector2Int(offsetX, offsetY);
            
            CellOffset = Offset * CellMetrics.ChunkSize;
            
            MapUvs();
            
            GenerateColliders();
        }

        /// <summary>
        /// Generates colliders for this Chunk.
        /// </summary>
        public void GenerateColliders()
        {
            ColliderGenerator.GenerateColliders(this, _activeColliders);
        }

        private void DrawCells()
        {
            if(!IsVisible)
                return;
            
            Graphics.DrawMesh(_mesh, _transformMatrix, ChunkMaterial, 0);
        }
    
        internal static void Draw()
        {
            foreach (var chunk in LoadedChunks)
            {
                chunk.Value.DrawCells();
            }
        }

        private static Vector3[] Uvs = new Vector3[4];
        
        /// <summary>
        /// Maps the second UV channel to the block indices.
        /// We also map the chunks of the cells to this one.
        /// </summary>
        private void MapUvs()
        {
            Profiler.BeginSample("Mapping uvs");
            var blockIndices = ListPool<Vector3>.Get();
        
            for (int x = 0; x < CellMetrics.ChunkSize; ++x)
            {
                for (int y = 0; y < CellMetrics.ChunkSize; ++y)
                {
                    var blockIndex = Coordinate.ToIndex(x + CellOffset.x, y + CellOffset.y);
                    
                    blockIndex = (CellOffset.x + x) + (CellOffset.y + y) * (ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize);
                    
                    var blockIndexVector = new Vector3(0, 0, blockIndex);
                    
                    CellMap.GetCellUnsafe(x + CellOffset.x, y + CellOffset.y).Chunk = this;
                    
                    blockIndices.Add(blockIndexVector);               
                    blockIndices.Add(blockIndexVector);
                    blockIndices.Add(blockIndexVector);               
                    blockIndices.Add(blockIndexVector);
                }
            }
        
            _mesh.SetUVs(2, blockIndices);

            IsVisible = true;
            ListPool<Vector3>.Add(blockIndices);
            _hasData = true;
            
            Profiler.EndSample();

        }

        /// <summary>
        /// Requests the rebuilt of the collider for this Chunk.
        /// </summary>
        internal void RequestRebuildCollider()
        {
            if(!ChunksThatRequestsRebuildsOfTheirColliders.Contains(this))
                ChunksThatRequestsRebuildsOfTheirColliders.Add(this);
        }

        /// <summary>
        /// Adds this Chunk to the Chunk Pool.
        /// </summary>
        internal void AddToPool()
        {
            LoadedChunks.Remove(Offset);
            ChunkPool.Add(this);
        }

        /// <summary>
        /// Adds the colliders that are currently active on this chunk to the ColliderPool and clears the collider list.
        /// </summary>
        internal void ClearColliders()
        {
            for (var index = 0; index < _activeColliders.Count; index++)
            {
                var collider = _activeColliders[index];
                collider.offset = Vector2.zero;

                ColliderPool.Add(collider);
            }

            _activeColliders.Clear(); 
        }
    }
}