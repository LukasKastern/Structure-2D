using System;
using System.Diagnostics;
using Structure2D.Lighting;
using Structure2D.MapGeneration.BasePasses;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Structure2D
{
    /// <summary>
    /// This class handles the texture which represents the block data inside our Shader.
    /// It also draws the Chunks.
    /// </summary>
    [AddComponentMenu("Structure 2D/Framework Base/Terrain Shader Data")]
    internal class TerrainShaderData : MonoBehaviour
    {
        public static bool IsLightingEnabled { get; set; }
        
        internal static TerrainShaderData ShaderData
        {
            get
            {
                if (_shaderData != null) return _shaderData;
                
                InitializeSingleTon();

                return _shaderData;
            }
        }

        private static TerrainShaderData _shaderData;
        
        private Color32[] _textureData;

        private Texture2D _shaderTexture;
        private Texture2D _lightingTexture;
        
        internal void CreateTexture()
        {
            if (_shaderTexture == null)
            {
                _shaderTexture = new Texture2D(ChunkLoader.DesiredViewPortSizeX* CellMetrics.ChunkSize, ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize,
                    TextureFormat.RGBA32, false, true);
            }

            else
                _shaderTexture.Resize(ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize, ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize);

            if (_lightingTexture == null)
            {
                _lightingTexture = new Texture2D(ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize, ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize, TextureFormat.RGBA32, false, true);
            }

            else
            {
                _lightingTexture.Resize(ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize,
                    ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize);
            }

            
            _lightingTexture.filterMode = FilterMode.Point;
            _lightingTexture.wrapMode = TextureWrapMode.Clamp;
            
            _shaderTexture.filterMode = FilterMode.Point;
            _shaderTexture.wrapMode = TextureWrapMode.Clamp;
            
            Shader.SetGlobalTexture("_CellData", _shaderTexture);   
            Shader.SetGlobalTexture("_LightingData", _lightingTexture);

            Shader.SetGlobalVector(
                "_CellData_TexelSize",
                new Vector4(1f / (ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize), 1f / (ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize), ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize, ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize)
            );

            FillTextureData();
        }
        
        /// <summary>
        /// Initializes the texture with empty colors
        /// </summary>
        private void FillTextureData()
        {
            if(_textureData == null || _textureData.Length != CellMap.MapWidth * CellMap.MapHeight)
                _textureData = new Color32[CellMap.MapWidth * CellMap.MapHeight];
            else
            {
                for(int i = 0; i < _textureData.Length; ++i)
                    _textureData[i] = new Color32(0, 0, 0, 0);
            }


            var array = _lightingTexture.GetRawTextureData<Color32>();
            for (int x = 0; x < ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize; ++x)
            {
                for (int y = 0; y < ChunkLoader.DesiredViewPortSizeY * CellMetrics.ChunkSize; ++y)
                {
                    array[x + ChunkLoader.DesiredViewPortSizeX * y] = new Color32(255, 0, 0, 0);
                }
            }
            _lightingTexture.Apply();
        }
        
        /// <summary>
        /// Refreshes the shader data for the given cell
        /// </summary>
        /// <param name="cell"></param>
        internal void RefreshShaderDataAtCell(Cell cell)
        {
            if(cell == null || CellMap.IsMapHidden)
                return;

            var blockIndex = (byte) (cell.Block);
            var backgroundIndex = (byte) (cell.Background);

            /*
            var alpha = (byte)255;

            var isCellEmpty = cell.Block == 0;
            var isBackgroundEmpty = cell.Background == 0;
        
            if (isCellEmpty && isBackgroundEmpty)
            {
                alpha = 0;
            }

            if (isCellEmpty)
            {
                blockIndex = 255;
            }

*/
            var index = cell.Coordinate.GetIndex();
    
            _textureData[index].SetBlock(blockIndex);
            _textureData[index].SetBackground(backgroundIndex);

            /*
            _textureData[index].a = blockIndex;
            _textureData[index].g = alpha;
            _textureData[index].b = backgroundIndex;
    
        */
            
        }
        
        private void LateUpdate()
        {
            if(CellMap.IsMapHidden)
                return;
            
            RefreshShaderTexture();
            FetchLightData();

            Chunk.Draw();
        }

        /// <summary>
        /// Sets the ShaderTexture values to those of the TextureData of the current viewport
        /// </summary>
        private void RefreshShaderTexture()
        {
            var shaderTexture = _shaderTexture.GetRawTextureData<Color32>();
            
            var viewport = Viewport.CurrentViewport;

            for (int y = 0; y < viewport.Height; ++y)
            {
                var textureDataIndex = y * CellMap.MapWidth + viewport.BottomLeft.x + viewport.BottomLeft.y * CellMap.MapWidth;
                var shaderTextureIndex = y * ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize;
                
                NativeArray<Color32>.Copy(_textureData, textureDataIndex, shaderTexture, shaderTextureIndex, viewport.Width);
            }
            
            _shaderTexture.Apply();
        }
        
        private void FetchLightData()
        {
            var viewport = Viewport.CurrentViewport;

            if(viewport.Width == 0 || viewport.Height == 0)
                return;

            var map = LightMap.GetLightMap();

            var lightingData = _lightingTexture.GetRawTextureData<Color32>();

            var desiredViewportWidth = ChunkLoader.DesiredViewPortSizeX * CellMetrics.ChunkSize;
            
            for (int y = 0; y < viewport.Height; ++y)
            {
                var yIndex = viewport.BottomLeft.y + y;
                var xIndex = viewport.BottomLeft.x;
                
                var lightMapOffset = CellMap.MapWidth *  yIndex + xIndex;
                var lightDataOffset = desiredViewportWidth * y;

                NativeArray<Color32>.Copy(map, lightMapOffset, lightingData, lightDataOffset, viewport.Width) ;
            }
            
            _lightingTexture.Apply();
        }

        /// <summary>
        /// This tries to fetch the texture data of the active TerrainShaderData.
        /// </summary>
        internal static Color32[] FetchCellTextureData()
        {
            if (_shaderData == null)
                return null;

            return _shaderData._textureData;
        }

        internal static void InitializeSingleTon()
        {
            _shaderData = GameObject.FindObjectOfType<TerrainShaderData>();

            if (_shaderData == null)
            {
                _shaderData = new GameObject("Shader Data", typeof(TerrainShaderData))
                    .GetComponent<TerrainShaderData>();   
            }
        }
    }
    
    public static class TerrainShaderExtensions
    {
        public static void SetBlock(ref this Color32 color, byte block)
        {
            color.a = block;
            color.UpdateAlpha();
        }

        public static byte GetBlock(this Color32 color) => color.a;

        public static void SetBackground(ref this Color32 color, byte background)
        {
            color.b = background;
            color.UpdateAlpha();
        }

        private static void UpdateAlpha(ref this Color32 color)
        {
            var alpha = (byte)255;

            var isCellEmpty = color.GetBlock() == 0;
            var isBackgroundEmpty = color.GetBackground() == 0;
        
            if (isCellEmpty && isBackgroundEmpty)
            {
                alpha = 0;
            }

            color.g = alpha;
        }
        
        public static byte GetBackground(this Color32 color) => color.b;
    }
    
}