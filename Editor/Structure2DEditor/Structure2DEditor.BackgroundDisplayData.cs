#if UNITY_EDITOR

using System;
using Structure2D;
using Structure2D.Lighting;
using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    public partial class Structure2DEditor
    {
        internal class BackgroundDisplayData
        {
            public Vector2Int TextureDimensions => Structure2DEditor.TextureDimensions;

            public Background Background;

            public string ErrorMessage;

            public string Path;

            public bool RequestDestroyThis;

            public BackgroundDisplayData(Background background, string Path, bool isActive)
            {
                IsActive = isActive;
                _name = background.name;
                this.Path = Path;

                Background = background;

                ErrorMessage = string.Empty;
            }

            public static GUIStyle _errorStyle;

            private static GUIStyle _disabledStyle;

            private static GUIStyle _foldoutErrorStyle;

            private bool _foldOutState;

            private string _name;

            private GUIStyle FoldOutStyle
            {
                get
                {
                    if (!IsActive)
                    {
                        return DisabledStyle;
                    }

                    else if (ErrorMessage != "")
                    {
                        return FoldoutErrorStyle;
                    }

                    else
                    {
                        return EditorStyles.foldout;
                    }
                }
            }

            public static GUIStyle ErrorStyle
            {
                get
                {
                    if (_errorStyle == null)
                    {
                        _errorStyle = new GUIStyle();
                        _errorStyle.normal.textColor = new Color(0.75f, 0, 0, 1);
                    }

                    return _errorStyle;
                }
            }

            private static GUIStyle DisabledStyle
            {
                get
                {
                    if (_disabledStyle == null)
                    {

                        _disabledStyle = new GUIStyle(EditorStyles.foldout);
                        _disabledStyle.normal.textColor = Color.gray;
                    }

                    return _disabledStyle;
                }
            }

            public static GUIStyle FoldoutErrorStyle
            {
                get
                {
                    if (_foldoutErrorStyle == null)
                    {
                        _foldoutErrorStyle = new GUIStyle(EditorStyles.foldout);
                        _foldoutErrorStyle.normal.textColor = new Color(0.75f, 0, 0, 1);
                        _foldoutErrorStyle.active.textColor = new Color(0.75f, 0, 0, 1);
                        _foldoutErrorStyle.focused.textColor = new Color(0.75f, 0, 0, 1);
                    }

                    return _foldoutErrorStyle;
                }

            }

            public bool IsActive;

            public bool CanBeDisabled = true;

            public void Display(Rect viewRect)
            {
                if (CanDrawContent)
                    ErrorMessage = GetErrorMessage();
                else
                    ErrorMessage = "";
                
                DrawContent(viewRect);
            }

            private void DrawButtonStateButton()
            {
                if (!CanBeDisabled)
                {
                    IsActive = true;
                    return;
                }

                string enableDisable = IsActive ? "Disable" : "Enable";

                if (GUILayout.Button(new GUIContent(enableDisable), EditorStyles.miniButton, GUILayout.MaxWidth(100)))
                {
                    IsActive = !IsActive;
                }
            }

            private void DrawContent(Rect viewRect)
            {
                if (Background == null)
                    return;

                using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope())
                {
                    _foldOutState = EditorGUILayout.Foldout(_foldOutState, _name, true, FoldOutStyle);

                    DrawButtonStateButton();

                    if (GUILayout.Button(new GUIContent("Delete"), EditorStyles.miniButton, GUILayout.MaxWidth(100)))
                    {
                        RequestDestroyThis = true;
                    }
                }


                if (!_foldOutState)
                    return;

                ++EditorGUI.indentLevel;

                float minTextureWidth = 200;
                var actualTextureWidth = viewRect.width / 2.5f;

                if (actualTextureWidth < minTextureWidth)
                    actualTextureWidth = minTextureWidth;

                _name = EditorGUILayout.TextField("Name", _name);

                if (CanDrawContent)
                {
                    Background.BlocksSunLight = EditorGUILayout.Toggle("Blocks Sunlight", Background.BlocksSunLight);

                    var currentBlockAmount = Background.LightBlockAmount / 255f;

                    var minLightBlockAmount = (255f / BlockLighting.MaxLightDistance) / 255;

                    if (currentBlockAmount < minLightBlockAmount)
                        currentBlockAmount = minLightBlockAmount;

                    var blockAmount =
                        EditorGUILayout.Slider("Light block amount", currentBlockAmount, minLightBlockAmount, 1);

                    Background.LightBlockAmount = (byte) (blockAmount * 255f);

                    Background.Texture = (Texture2D) EditorGUILayout.ObjectField("Texture", Background.Texture,
                        typeof(Texture2D), false, GUILayout.MaxWidth(210));
                }
                
                if (ErrorMessage != String.Empty)
                {
                    EditorGUILayout.LabelField(ErrorMessage, ErrorStyle);
                }

                --EditorGUI.indentLevel;

                EditorUtility.SetDirty(Background);
            }

            public bool CanDrawContent { get; set; }

            private string GetErrorMessage()
            {
                if (!CanDrawContent)
                    return "";
                
                if (Background.Texture == null)
                    return "Background has no texture assigned to it";

                else if ((Background.Texture.width != TextureDimensions.x ||
                         Background.Texture.height != TextureDimensions.y))
                {
                    return $"Background Texture doesn't have the correct size, the size of the given texture is " +
                           $"{new Vector2Int(Background.Texture.width, Background.Texture.height)} but the set size is {TextureDimensions}";}
                else
                    return "";
            }

            public void ChangeNameIfNeeded()
            {
                if (_name != Background.name)
                {
                    AssetDatabase.RenameAsset(Path, _name);

                    Path = AssetDatabase.GetAssetPath(Background);
                }
            }
        }
    }
}

#endif