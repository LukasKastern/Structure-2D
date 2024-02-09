#if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Structure2D
{
    public class FirstTimeLaunchPopup : EditorWindow
    {

        private string _readmePath => Application.dataPath + "/Structure2D/ReadMe.pdf";
        
        [InitializeOnLoadMethod]
        private static void ShowFirstTimeInstallPopup()
        {
#if (UNITY_2018 || UNITY_2018_1_OR_NEWER)
            EditorApplication.projectChanged += DisplayIfNeeded;
            EditorApplication.hierarchyChanged += DisplayIfNeeded;
#else
            EditorApplication.projectWindowChanged += DisplayIfNeeded;
            EditorApplication.hierarchyWindowChanged += DisplayIfNeeded;
#endif
            EditorApplication.update += DisplayIfNeeded;
        }

        private static void DisplayIfNeeded()
        {
            bool isFirstTimeLaunch = !EditorPrefs.HasKey("Initialized Structure2D");

            isFirstTimeLaunch = !EditorPrefs.GetBool("Initialized Structure2D");
            
            if(!isFirstTimeLaunch)
                return;
            
            EditorPrefs.SetBool("Initialized Structure2D", true);
            
            var window = (FirstTimeLaunchPopup)EditorWindow.GetWindow(typeof(FirstTimeLaunchPopup), false, "Welcome");
            
            window.maxSize = new Vector2(250, 80);
            window.minSize = new Vector2(250, 80);
        }
        

        protected void OnGUI()
        {
            GUIStyle CenterLableBold = new GUIStyle(EditorStyles.largeLabel);
            CenterLableBold.richText = true;

            var centerLable = new GUIStyle(EditorStyles.largeLabel);
            centerLable.richText = true;

            centerLable.alignment = TextAnchor.MiddleCenter;
            CenterLableBold.alignment = TextAnchor.MiddleCenter;

            using (var verticalScope = new GUILayout.VerticalScope())
            {
                GUILayout.Space(15);
                
                using (var scope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(40);
                    
                    EditorGUILayout.LabelField("<b>Welcome to Structure 2D</b>", CenterLableBold);
                }

                GUILayout.Space(10);
                
                using (var scope = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Version: 1.0", centerLable);
                }
                
                
                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("Open Readme"), EditorStyles.miniButtonLeft))
                {
                    Process.Start(_readmePath);
                }
            }
        }
    }
}
#endif