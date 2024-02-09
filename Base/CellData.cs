using System;
using System.Collections.Generic;
using System.Linq;
using Structure2D;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace Structure2D
{
    /// <summary>
    /// Class which handles the creation of the Texture Arrays and Meta Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Structure2D/BlockData")]
    public class CellData : ScriptableObject
    {
            private MetaDataBaseClass[] _metaDataInitializationClasses = {
            new LightingMetaData(),
            new BlockSolidStateMetaData(),  
        };

        [SerializeField]
        public bool IsActive;
        
        public Block EmptyBlock;
        public Background EmptyBackground;
        
        public Vector2 TextureSize;

        /// <summary>
        /// This is the block with which the map gets filled initially 
        /// </summary>
        public Block DefaultBlock;

        /// <summary>
        /// This is the placeholder background with which the map gets filled initially 
        /// </summary>
        public Background DefaultBackground;
        
        /// <summary>
        /// This blocks is a solid unbreakable block 
        /// </summary>
        public Block BaseBlock;

        /// <summary>
        /// Here you can add your own blocks
        /// </summary>
        public Block[] CustomBlocks;

        /// <summary>
        /// Here you can add all the Background CustomBackgrounds
        /// </summary>
        public Background[] CustomBackgrounds;

        public Material BlockMaterial { get; set; }
    
        private Texture2DArray _cellTexture2DArray;
        private Texture2DArray _backgroundTextureArray;

        private Block[] _blocks;

        private Background[] _backgrounds;

        private static CellData _cellData;

        public static Texture2D EmptyTexture
        {
            get
            {
                if (_emptyTexture == null)
                {
                    _emptyTexture = new Texture2D((int)_cellData.TextureSize.x, (int)_cellData.TextureSize.y, TextureFormat.RGBA32 , false);
                    for (int x = 0; x < _cellData.TextureSize.x; ++x)
                    {
                        for (int y = 0; y < _cellData.TextureSize.y; ++y)
                        {
                            _emptyTexture.SetPixel(x, y, new Color(0, 0, 0, 1));
                        }
                    }

                    _emptyTexture.Apply();
                }
                
                return _emptyTexture;
            }
        }

        private static Texture2D _emptyTexture;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeActiveBlockData()
        {
            ChunkMaterial material;
            
            try
            {
                _cellData = Resources.LoadAll<CellData>("").First(i => i.IsActive);
                
                if (_cellData == null)
                    throw new Exception();
            }
            
            catch (Exception)
            {
                throw new Exception("There is no active BlockData instance in your resources folder");
            }

            try
            {
                material = Resources.Load<ChunkMaterial>("ChunkMaterial");

                if (material == null)
                    throw new Exception();
            }
            
            catch (Exception)
            {
                throw new Exception("There is no Chunk Material inside your resources folder");
            }

            if(material.Material == null)
                throw new Exception("The Chunk material has no material assigned to it");
            
            _cellData.BlockMaterial = material.Material;
            _cellData.Initialize();
        }

        /// <summary>
        /// Initializes the texture array for the Background Material and returns all Blocks in this BlockData
        /// </summary>
        /// <returns></returns>
        private void Initialize()
        {
            string errorCode;
            
            if (HasError(out errorCode))
            {
                throw new Exception("Failed to initialize BlockData: " + errorCode);
            }

            InitializeBlocks();
            InitializeBackgrounds();
            
            CreateBlockMetaData();
            
            _cellTexture2DArray = GenerateBlockTextureMap();
            _backgroundTextureArray = GenerateBackgroundTextureMap();
        
            BlockMaterial.SetTexture("_BackgroundTexArray", _backgroundTextureArray);
            BlockMaterial.SetTexture("_CellTexArray", _cellTexture2DArray);

            Chunk.ChunkMaterial = BlockMaterial;
        }

        public bool HasError(out string errorCode)
        {
            errorCode = String.Empty;
            
            if (BaseBlock == null)
            {
                errorCode = "Background Data doesn't have a Base Background";
                return true;
            }
            
            else if (!IsBlockUsable(BaseBlock))
            {
                errorCode = "Texture of Base Block is null or doesn't have the right size";
                return true;
            }
            
            else if (DefaultBlock == null)
            {
                errorCode = "Background Data doesn't have a Default Background";
                return true;
            }
            
            else if (!IsBlockUsable(DefaultBlock))
            {
                errorCode = "Texture of Default Block is null or doesn't have the right size";
                return true;
            }
            
            return false;
        }
        
        private void CreateBlockMetaData()
        {
            foreach (var metaDataClass in _metaDataInitializationClasses)
            {
                MetaDataManager.PrepareBlockInitialization += metaDataClass.RegisterForBlockInitialization;
                MetaDataManager.PrepareBackgroundInitialization += metaDataClass.RegisterForBackgroundInitialization;
                MetaDataManager.BlockInitializationEnumerator += metaDataClass.EnumerateBlockInitialization;    
                MetaDataManager.BackgroundInitializationEnumerator += metaDataClass.EnumerateBackgroundInitialization;
            }
            
            MetaDataManager.InitializeBlocks(_blocks);
            MetaDataManager.InitializeBackgrounds(_backgrounds);
            
            foreach (var metaDataClass in _metaDataInitializationClasses)
            {
                MetaDataManager.PrepareBlockInitialization -= metaDataClass.RegisterForBlockInitialization;
                MetaDataManager.PrepareBackgroundInitialization -= metaDataClass.RegisterForBackgroundInitialization;
                MetaDataManager.BlockInitializationEnumerator -= metaDataClass.EnumerateBlockInitialization;    
                MetaDataManager.BackgroundInitializationEnumerator -= metaDataClass.EnumerateBackgroundInitialization;
            }
        }

        /// <summary>
        /// Fills the Blocks array with all custom + default blocks and assigns the IDs of the blocks
        /// </summary>
        private void InitializeBlocks()
        {
            List<Block> blocks = new List<Block>(CustomBlocks.Length + 3);

            EmptyBlock.ID = 0;

            EmptyBlock.IsSolid = false;
            EmptyBlock.LightBlockAmount = 0;
            EmptyBlock.Texture = EmptyTexture;
            
            
            BaseBlock.ID = 1;
            DefaultBlock.ID = 2;
            
            blocks.Add(EmptyBlock);
            blocks.Add(BaseBlock);
            blocks.Add(DefaultBlock);
            
            //We iterate over the custom blocks instead of just adding it by range so we can give every block its ID
            for (int i = 0; i < CustomBlocks.Length; ++i)
            {
                if(!IsBlockUsable(CustomBlocks[i]))
                    continue;
                        
                CustomBlocks[i].ID = 3 + i;
                blocks.Add(CustomBlocks[i]);
            }

            _blocks = blocks.ToArray();
        }

        private bool IsBlockUsable(Block block)
        {
            return block.Texture != null && block.Texture.width == (int)TextureSize.x &&
                   block.Texture.height == (int)TextureSize.y;
        }

        private bool IsBackgroundUsable(Background background)
        {
            return background.Texture != null && background.Texture.width == (int)TextureSize.x &&
                   background.Texture.height == (int)TextureSize.y;        
        }
        
        /// <summary>
        /// Fills the Blocks array with all custom + default blocks and assigns the IDs of the blocks
        /// </summary>
        private void InitializeBackgrounds()
        {
            List<Background> backgrounds = new List<Background>(CustomBackgrounds.Length + 1);

            EmptyBackground.ID = 0;

            EmptyBackground.BlocksSunLight = false;
            EmptyBackground.LightBlockAmount = 0;
            EmptyBackground.Texture = EmptyTexture;
            backgrounds.Add(EmptyBackground);

            DefaultBackground.BlocksSunLight = true;

            DefaultBackground.ID = 1;
            backgrounds.Add(DefaultBackground);
            
            //We iterate over the custom blocks instead of just adding it by range so we can give every block its ID
            for (int i = 0; i < CustomBackgrounds.Length; ++i)
            {
                if(!IsBackgroundUsable(CustomBackgrounds[i]))
                    continue;
                
                CustomBackgrounds[i].ID = 2 + i;
                backgrounds.Add(CustomBackgrounds[i]);
            }

            _backgrounds = backgrounds.ToArray();
        }

        private Texture2DArray GenerateBlockTextureMap()
        {
            var blocksWithMissingTexture = _blocks.Where(i => i.Texture == null).ToArray();

            foreach (var blockWithoutTexture in blocksWithMissingTexture)
            {
//                Debug.LogErrorFormat("{0} needs to have a Background Texture", blockWithoutTexture.name);
            }
        
            if(blocksWithMissingTexture.Length > 0)
                throw new Exception("Couldn't generate Texture Array");
        
            if(_blocks.Length == 0)
                Debug.LogError("Couldn't create texture array, there are no blocks to create an array from");

            return GenerateTextureMap(_blocks.Select(i => i.Texture).ToArray());
        }
    
        private Texture2DArray GenerateBackgroundTextureMap()
        {
            var blockWithMissingBackground = _backgrounds.Where(i => i.Texture == null).ToArray();

            foreach (var blockWithMissingTexture in blockWithMissingBackground)
            {
                Debug.LogErrorFormat("{0} needs to have a Texture", blockWithMissingTexture.name);
            }
        
            if(blockWithMissingBackground.Length > 0)
                throw new Exception("Couldn't generate Texture Array");
        
            if(_backgrounds.Length == 0)
                Debug.LogError("Couldn't create texture array, there are no backgrounds to create an array from");

            return GenerateTextureMap(_backgrounds.Select(i => i.Texture).ToArray());
        }

        /// <summary>
        /// Generates a texture map from the given textures
        /// </summary>
        /// <param name="textures"></param>
        /// <returns></returns>
        private Texture2DArray GenerateTextureMap(Texture2D[] textures)
        {
            if (textures.Length == 0)
                return null;
        
            var texture = textures[0];
        
            Texture2DArray texture2DArray = new Texture2DArray(texture.width, texture.height, textures.Length, texture.format, texture.mipmapCount > 1);
            texture2DArray.anisoLevel = texture.anisoLevel;
            texture2DArray.filterMode = texture.filterMode;
            texture2DArray.wrapMode = texture.wrapMode;
        
            for (int i = 0; i < textures.Length; i++) {
                for (int m = 0; m < texture.mipmapCount; m++) {
                    Graphics.CopyTexture(textures[i], 0, m, texture2DArray, i, m);
                }
            }

            return texture2DArray;
        }

        public static Vector2Int CellDimension { get; private set; }
    }
}
