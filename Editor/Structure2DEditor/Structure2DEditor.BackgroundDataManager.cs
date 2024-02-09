#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    public partial class Structure2DEditor
    {
        private class BackgroundDataManager
        {
            private CellData _activeCellData;
        
            private List<BackgroundDisplayData> _backgrounds;
        
            private Vector2 _scrollPos;

            private void FetchBlocks()
            {
                if(_backgrounds == null)
                    _backgrounds = new List<BackgroundDisplayData>();
        
                var blockGUIDS = AssetDatabase.FindAssets("t: " + typeof(Background).FullName);
        
                foreach (var blockGUID in blockGUIDS)
                {
                    var backgorund = AssetDatabase.LoadAssetAtPath<Background>(AssetDatabase.GUIDToAssetPath(blockGUID));
            
                    if(_backgrounds.FirstOrDefault(i => i.Background == backgorund) == null)
                        _backgrounds.Add(new BackgroundDisplayData(backgorund, AssetDatabase.GUIDToAssetPath(blockGUID), DoesBlockDataUseBlock(backgorund)));
                }
            
                SortBlocks();
            }

            private bool DoesBlockDataUseBlock(Background block)
            {
                if (IsUsedAsSpecialBackground(block))
                    return true;

                return _activeCellData.CustomBackgrounds.Contains(block);
            }

            public void Save()
            {
                if(_backgrounds == null || _activeCellData == null)
                    return;
            
                var activeBlocks = new List<Background>();

                for (int i = 0; i < _backgrounds.Count; ++i)
                {
                    _backgrounds[i].ChangeNameIfNeeded();
                
                    if (_backgrounds[i].ErrorMessage != string.Empty || !_backgrounds[i].IsActive)
                        continue;

                    if (!IsUsedAsSpecialBackground(_backgrounds[i].Background))
                        activeBlocks.Add(_backgrounds[i].Background);
                }

                _activeCellData.CustomBackgrounds = activeBlocks.ToArray();

                EditorUtility.SetDirty(_activeCellData);
            
            }

            private bool IsUsedAsSpecialBackground(Background background)
            {
                return _activeCellData.EmptyBackground == background || _activeCellData.DefaultBackground == background;
            }

            private void CreateNewBlock()
            {
                var path = "";
                var background = CreateAsset<Background>(out path);
                background.BlocksSunLight = true;
                background.LightBlockAmount = 40;
        
                _backgrounds.Add(new BackgroundDisplayData(background, path, false));
    
                SortBlocks();
            }

            private void SortBlocks()
            {
                _backgrounds.Sort(BackgroundDisplayDataSorting);
            }

            private int BackgroundDisplayDataSorting(BackgroundDisplayData a, BackgroundDisplayData b) 
            {
                var value = IsUsedAsSpecialBackground(b.Background).CompareTo(IsUsedAsSpecialBackground(a.Background));

                if (value == 0)
                    return a.Background.name.CompareTo(b.Background.name);

                return value;
            }

            public void SetTo(CellData activeCellData)
            {
                _activeCellData = activeCellData;
                FetchBlocks();
            }
        
            private void ProcessBlockInput()
            {
                List<BackgroundDisplayData> blocksThatRequestTheirDestruction = new List<BackgroundDisplayData>();
        
                for (int i = 0; i < _backgrounds.Count; ++i)
                {
                    if(!_backgrounds[i].RequestDestroyThis)
                        continue;
            
                    blocksThatRequestTheirDestruction.Add(_backgrounds[i]);
                }

                foreach (var block in blocksThatRequestTheirDestruction)
                {
                    _backgrounds.Remove(block);

                    AssetDatabase.DeleteAsset(block.Path);
                }
            
                if(blocksThatRequestTheirDestruction.Count > 0)
                    SortBlocks();
            }
        
            public void DisplayBackgrounds(Rect viewport)
            {
                if(_activeCellData == null)
                    return;
            
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.Width(viewport.width), GUILayout.Height(viewport.height));

                using (GUILayout.VerticalScope scope = new GUILayout.VerticalScope(GUILayout.Width(viewport.width - 20)))
                {
                    foreach (var block in _backgrounds)
                    {
                        DisplayBackground(block, viewport);
                    }
            
                    using (GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope())
                    {
                        if(EditorGUILayout.DropdownButton(new GUIContent("Create new Background"), FocusType.Passive, EditorStyles.miniButtonRight))
                            CreateNewBlock();
            
                        if(EditorGUILayout.DropdownButton(new GUIContent("Refresh"), FocusType.Keyboard, EditorStyles.miniButtonLeft))
                            FetchBlocks();
                    }
                }
        
                EditorGUILayout.EndScrollView();
        
                ProcessBlockInput();
            }

            private void DisplayBackground(BackgroundDisplayData blockToDisplay, Rect viewport)
            {
                blockToDisplay.CanBeDisabled = !IsUsedAsSpecialBackground(blockToDisplay.Background);
                blockToDisplay.CanDrawContent = _activeCellData.EmptyBackground != blockToDisplay.Background;
                
                blockToDisplay.Display(viewport);
            }

            
            
        }
    }
}

#endif