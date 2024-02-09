

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Structure2D
{
    public partial class Structure2DEditor : EditorWindow
    {
        public CellData ActiveCellData
        {
            get => _activeCellData;
            set
            {
                if (value == null)
                {
                    _activeCellData = null;
                    return;
                }
                
                value.IsActive = true;
                EditorUtility.SetDirty(value);
                
                if (_activeCellData != null && _activeCellData != value)
                {
                     _activeCellData.IsActive = false;
                    EditorUtility.SetDirty(_activeCellData);
                }
                
                TextureDimensions = new Vector2Int((int)value.TextureSize.x, (int)value.TextureSize.y);

           
                CellManager.SetTo(value);
                BackgroundManager.SetTo(value);
                

                _activeCellData = value;

            }
        }

        private CellData _activeCellData;

        private string _relativeIconPath = "Assets/Structure2D/Editor/Icons/";

        private Toolbar _toolbar;
        private CellDataManager _cellDataManager;
        private BackgroundDataManager _backgroundManager;

        private CellDataManager CellManager
        {
            get
            {
                if (_cellDataManager == null)
                    _cellDataManager = new CellDataManager();

                return _cellDataManager;
            }
            set
            {
                _cellDataManager = value;
               // BlockManager.SetTo(ActiveBlockData);
            }
        }

        private BackgroundDataManager BackgroundManager
        {
            get
            {
                if (_backgroundManager == null)
                    _backgroundManager = new BackgroundDataManager();

                return _backgroundManager;
            }
            set
            {
                _backgroundManager = value;
                //_backgroundManager.SetTo(ActiveBlockData);
            }
        }

        public static Vector2Int TextureDimensions;

        [MenuItem("Window/Structure2D/Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(Structure2DEditor), false, "Structure2D Editor");
        }

        private void OnEnable()
        {
            CellManager = new CellDataManager();
            BackgroundManager = new BackgroundDataManager();
            _toolbar = new Toolbar(LoadIcons());

            LoadCellData();
        }

        private Texture[] LoadIcons()
        {
            var blockTexture = AssetDatabase.LoadAssetAtPath<Texture>(_relativeIconPath + "Block.png");
            var backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture>(_relativeIconPath + "background.png");
            var blockDataTexture = AssetDatabase.LoadAssetAtPath<Texture>(_relativeIconPath + "configuration.png");

            return new Texture[] {blockDataTexture, blockTexture, backgroundTexture};
        }

        private void OnDisable()
        {
            Save();

            _toolbar.Save();
        }

        private void Save()
        {
            if(ActiveCellData == null)
                return;
            
            VerifyBlockDataPath(ActiveCellData);
        
            CellManager.Save();
            BackgroundManager.Save();
        
            EditorUtility.SetDirty(ActiveCellData);
        }

        private void OnGUI()
        {
            if(_toolbar == null)
                _toolbar = new Toolbar(LoadIcons());
        
            _toolbar.Draw();

            if (_toolbar.HasSelectedChanged)
            {
                Save();
                _toolbar.HasSelectedChanged = false;
            }
        
            var typeToDisplay = _toolbar.GetSelected();

            if (typeToDisplay == SelectedTab.Blocks)
                CellManager.DisplayBlocks(position);
            else if (typeToDisplay == SelectedTab.Backgrounds)
                BackgroundManager.DisplayBackgrounds(position);
            else if (typeToDisplay == SelectedTab.CellData)
                DrawBlockData();
        }

        private void VerifyBlockDataPath(CellData data)
        {
            var path = AssetDatabase.GetAssetPath(data);

            var targetPath = path;
            
            if (path.Contains("Resources")) return;
            
            var fileName = Path.GetFileName(path);
            targetPath = "Assets/Resources/" + fileName;
                        
            Debug.LogWarningFormat("{0} is not inside a resources folder, {0} will get moved to {1}", fileName, targetPath);
                        
            AssetDatabase.MoveAsset(path, targetPath);
        }
        
        private void LoadCellData()
        {
            var blockDataGuids = AssetDatabase.FindAssets("t: " + typeof(CellData).Name);

            List<CellData> _blockDatas = new List<CellData>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            
            foreach (var guid in blockDataGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var targetPath = path;
                if (!path.Contains("Resources"))
                {
                    var fileName = Path.GetFileName(path);
                    targetPath = "Assets/Resources/" + fileName;
                        
                    Debug.LogWarningFormat("{0} is not inside a resources folder, {0} will get moved to {1}", fileName, targetPath);
                        
                    AssetDatabase.MoveAsset(path, targetPath);   
                }

                _blockDatas.Add(AssetDatabase.LoadAssetAtPath<CellData>(targetPath));
            }
            
            
            CellData cellData;

            var activeData = _blockDatas.Where(i => i.IsActive);

            cellData = activeData.FirstOrDefault();

            if (cellData == null)
                cellData = _blockDatas.FirstOrDefault();
            
            if (cellData == null)
            {
                cellData = CreateAsset<CellData>();

                var path = AssetDatabase.GetAssetPath(cellData);
                
                var fileName = Path.GetFileName(path);
                var targetPath = "Assets/Resources/" + fileName;
                
                AssetDatabase.MoveAsset(path, targetPath);
            }

            else
            {
                foreach (var dataToSetInactive in activeData.Where(i => i != cellData))
                {
                    dataToSetInactive.IsActive = false;
                    EditorUtility.SetDirty(dataToSetInactive);
                }
            }
            

            ActiveCellData = cellData;
        }

        private void DrawBlockData()
        {
            if (ActiveCellData != null)
            {
                string error;
                if (ActiveCellData.HasError(out error))
                    EditorGUILayout.LabelField(error, BlockDisplayData.ErrorStyle);
            }

            var maxWidth = GUILayout.MaxWidth(position.width - 50);

            ActiveCellData =
                (CellData) EditorGUILayout.ObjectField("Block Data", ActiveCellData, typeof(CellData), false, maxWidth);

            if (ActiveCellData == null)
                LoadCellData();

            DrawUILine(Color.gray);

            EditorGUIUtility.wideMode = true;
            ActiveCellData.TextureSize = EditorGUILayout.Vector2Field("Texture Size", ActiveCellData.TextureSize, GUILayout.Width(position.width + EditorGUIUtility.labelWidth - 32));
            EditorGUIUtility.wideMode = false;

            ActiveCellData.TextureSize.x = (int)Mathf.Clamp(ActiveCellData.TextureSize.x, 1, int.MaxValue);
            ActiveCellData.TextureSize.y = (int)Mathf.Clamp(ActiveCellData.TextureSize.y, 1, int.MaxValue);

            TextureDimensions = new Vector2Int((int)ActiveCellData.TextureSize.x, (int)ActiveCellData.TextureSize.y);
        
            DrawUILine(Color.gray);

            ActiveCellData.EmptyBlock = (Block) EditorGUILayout.ObjectField("Empty Block", ActiveCellData.EmptyBlock,
                typeof(Block), false, maxWidth);
            ActiveCellData.BaseBlock = (Block) EditorGUILayout.ObjectField("Base Block", ActiveCellData.BaseBlock,
                typeof(Block), false, maxWidth);
            ActiveCellData.DefaultBlock = (Block) EditorGUILayout.ObjectField("Default Block",
                ActiveCellData.DefaultBlock, typeof(Block), false, maxWidth);
     
            DrawUILine(Color.gray);

            ActiveCellData.EmptyBackground = (Background) EditorGUILayout.ObjectField("Empty Background",
                ActiveCellData.EmptyBackground, typeof(Background), false, maxWidth);
            
            ActiveCellData.DefaultBackground = (Background) EditorGUILayout.ObjectField("Default Background",
                ActiveCellData.DefaultBackground, typeof(Background), false, maxWidth);
        }

        public static T CreateAsset<T>(out string path) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            path = "Assets";

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).Name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();

            return asset;
        }

        public static T CreateAsset<T>() where T : ScriptableObject
        {
            string path = "";
            return CreateAsset<T>(out path);
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private enum SelectedTab
        {
            CellData,
            Blocks,
            Backgrounds
        }
    }
}

#endif