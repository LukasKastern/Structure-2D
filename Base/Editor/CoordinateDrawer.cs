#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    /// <summary>
    /// Property drawer used to display the Coordinate struct.
    /// </summary>
    [CustomPropertyDrawer(typeof(Coordinate))]
    public class CoordinateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Label(position, label);

            var coordinates = "(" + property.FindPropertyRelative("X").intValue + "," + property.FindPropertyRelative("Y").intValue + ")";
        
            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, coordinates);
        }
    }
}

#endif