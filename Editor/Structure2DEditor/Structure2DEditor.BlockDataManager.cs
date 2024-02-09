

using System.IO;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Structure2D.MapGeneration.BasePasses;
using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    public partial class Structure2DEditor
    {
        private class CellDataManager
        {
            private CellData _activeCellData;
        
            private List<BlockDisplayData> _blocks;
        
            private Vector2 _scrollPos;

            private void FetchBlocks()
            {
                if(_blocks == null)
                    _blocks = new List<BlockDisplayData>();
        
                var blockGUIDS = AssetDatabase.FindAssets("t: " + typeof(Block).FullName);

                foreach (var blockGUID in blockGUIDS)
                {
                    var block = AssetDatabase.LoadAssetAtPath<Block>(AssetDatabase.GUIDToAssetPath(blockGUID));

                    if(_blocks.FirstOrDefault(i => i.Block == block) == null)
                        _blocks.Add(new BlockDisplayData(block, AssetDatabase.GUIDToAssetPath(blockGUID), DoesBlockDataUseBlock(block)));
                }

                SortBlocks();
            }

            private bool DoesBlockDataUseBlock(Block block)
            {
                if (IsBlockUsedAsSpecialBlock(block))
                    return true;

                return _activeCellData.CustomBlocks.Contains(block);
            }

            public void Save()
            {
                if(_blocks == null || _activeCellData == null)
                    return;
            
                var activeBlocks = new List<Block>();

                for (int i = 0; i < _blocks.Count; ++i)
                {
                    _blocks[i].ChangeNameIfNeeded();
                
                    if (_blocks[i].ErrorMessage != string.Empty || !_blocks[i].IsActive)
                        continue;

                    if (!IsBlockUsedAsSpecialBlock(_blocks[i].Block))
                        activeBlocks.Add(_blocks[i].Block);
                }

                _activeCellData.CustomBlocks = activeBlocks.ToArray();
            
                EditorUtility.SetDirty(_activeCellData);
            
            }

            private bool IsBlockUsedAsSpecialBlock(Block block)
            {
                return _activeCellData.BaseBlock == block || _activeCellData.EmptyBlock == block || _activeCellData.DefaultBlock == block;
            }

            private void CreateNewBlock()
            {
                var path = "";
                var block = CreateAsset<Block>(out path);
                block.IsSolid = true;
                block.LightBlockAmount = 40;
        
                _blocks.Add(new BlockDisplayData(block, path, false));
    
                SortBlocks();
            }

            private void SortBlocks()
            {
                _blocks.Sort(BlockDisplaySorting);
            }


            private int BlockDisplaySorting(BlockDisplayData a, BlockDisplayData b)
            {
                var value = IsBlockUsedAsSpecialBlock(b.Block).CompareTo(IsBlockUsedAsSpecialBlock(a.Block));

                if (value == 0)
                    return a.Block.name.CompareTo(b.Block.name);

                else return value;
            }

        
            public void SetTo(CellData activeCellData)
            {
                _activeCellData = activeCellData;
                FetchBlocks();
            }
        
        
            private void ProcessBlockInput()
            {
                List<BlockDisplayData> blocksThatRequestTheirDestruction = new List<BlockDisplayData>();
        
                for (int i = 0; i < _blocks.Count; ++i)
                {
                    if(!_blocks[i].RequestDestroyThis)
                        continue;
            
                    blocksThatRequestTheirDestruction.Add(_blocks[i]);
                }

                foreach (var block in blocksThatRequestTheirDestruction)
                {
                    _blocks.Remove(block);

                    AssetDatabase.DeleteAsset(block.Path);
                }
            
                if(blocksThatRequestTheirDestruction.Count > 0)
                    SortBlocks();
            }
        
            public void DisplayBlocks(Rect viewport)
            {
                if(_activeCellData == null)
                    return;
            
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.Width(viewport.width), GUILayout.Height(viewport.height));

                using (GUILayout.VerticalScope scope = new GUILayout.VerticalScope(GUILayout.Width(viewport.width - 20)))
                {
                    foreach (var block in _blocks)
                    {
                        DisplayBlock(block, viewport);
                    }
            
                    using (GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope())
                    {
                        if(EditorGUILayout.DropdownButton(new GUIContent("Create new Block"), FocusType.Passive, EditorStyles.miniButtonRight))
                            CreateNewBlock();
            
                        if(EditorGUILayout.DropdownButton(new GUIContent("Refresh"), FocusType.Keyboard, EditorStyles.miniButtonLeft))
                            FetchBlocks();
                    }
                
                }
        
                EditorGUILayout.EndScrollView();
        
                ProcessBlockInput();
            }

            private void DisplayBlock(BlockDisplayData blockToDisplay, Rect viewport)
            {
                blockToDisplay.CanBeDisabled = !IsBlockUsedAsSpecialBlock(blockToDisplay.Block);

                blockToDisplay.CanDrawContent = _activeCellData.EmptyBlock != blockToDisplay.Block;
                
                blockToDisplay.Display(viewport);
            }

        }
    }
}

#endif