using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elpis
{
    public sealed class SceneSwitcher : EditorWindow
    {
        private Dictionary<string, string> mScenes = new Dictionary<string, string>();

        [MenuItem("Window/SceneSwitcher")]
        public static void ShowSceneSwitcher()
        {
            EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            Type hierarchyType = typeof(EditorWindow);

            // 抓不到 SceneHierarchyWindow 的 Reference?，暫解
            for (int i = 0; i < allWindows.Length; i++)
                if (allWindows[i].GetType().Name == "SceneHierarchyWindow")
                    hierarchyType = allWindows[i].GetType();

            var window = GetWindow<SceneSwitcher>("SceneSwitcher", hierarchyType);
            window.minSize = new Vector2(400f, 150f);
        }

        void OnEnable()
        {
            Refresh();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", GUILayout.Width(350f), GUILayout.Height(25f)))
            {
                Refresh();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            foreach (KeyValuePair<string, string> kvp in mScenes)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(60f));

                    EditorGUILayout.LabelField(kvp.Value, GUILayout.Width(200f));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Load", GUILayout.Width(100f), GUILayout.Height(25f)))
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        EditorSceneManager.OpenScene(kvp.Value);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void Refresh()
        {
            mScenes.Clear();

            var scenes = AssetDatabase.FindAssets("t:Scene");
            for (int i = 0; i < scenes.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(scenes[i]);
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                mScenes.Add(scene.name, scenePath);
            }
        }
    }
}
