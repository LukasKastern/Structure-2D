#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    public partial class Structure2DEditor
    {
        private class Toolbar
        {
            public bool HasSelectedChanged = false;

            private Texture[] _icons;

            private int selected;
        
            public Toolbar(Texture[] icons)
            {
                _icons = icons;
            
                selected = EditorPrefs.HasKey("Structure2DToolbarSelected")
                    ? EditorPrefs.GetInt("Structure2DToolbarSelected")
                    : 0;
            }

            public void Draw()
            {
                GUILayout.Space(15f);
        
                var layout = new GUIStyle(EditorStyles.toolbarButton);

                layout.onFocused = layout.normal;
        
                layout.fixedWidth = 60;

                //layout.stretchWidth = true;
                layout.fixedHeight = 30;
            
                layout.overflow = new RectOffset(0, 0, 6, 6);
                layout.stretchHeight = true;
                layout.stretchWidth = false;

                using (var space = new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(5);
                    var tempSelected = GUILayout.Toolbar(selected, _icons, layout);
                
                    if (selected != tempSelected)
                        HasSelectedChanged = true;

                    selected = tempSelected;
                }

                EditorGUILayout.Space();
            
                DrawUILine(Color.gray);
            }

            public SelectedTab GetSelected()
            {
                return (SelectedTab) selected;
            }

            public void Save()
            {
                EditorPrefs.SetInt("Structure2DToolbarSelected", selected);
            }
        }
    }
}

#endif