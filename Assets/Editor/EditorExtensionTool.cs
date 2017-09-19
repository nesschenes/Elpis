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
            GUI.backgroundColor = new Color32(0, 255, 255, 255);
            GUI.contentColor = Color.white;

            EditorGUILayout.BeginVertical();

            GUIStyle guiStyle = new GUIStyle();
            GUIStyleState styleState = new GUIStyleState();

            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.4f);

            styleState.background = Texture2D.whiteTexture;

            styleState.textColor = Color.green;

            guiStyle.normal = styleState;

            GUI.Label(new Rect(0, 0, position.width, 20), "", guiStyle);

            GUI.backgroundColor = new Color32(0, 255, 255, 255);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Scene", GUILayout.Width(60f));
                GUILayout.Label("Path", GUILayout.Width(200f));

                GUILayout.FlexibleSpace();

                GUILayout.Label("Action", GUILayout.Width(100f));
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            foreach (KeyValuePair<string, string> kvp in mScenes)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(60f));

                    EditorGUILayout.LabelField(kvp.Value, GUILayout.Width(200f));

                    GUILayout.FlexibleSpace();

                    if (SceneManager.GetActiveScene().name == kvp.Key)
                    {
                        GUI.backgroundColor = new Color32(255, 125, 130, 255);

                        if (GUILayout.Button("Save", GUILayout.Width(100f), GUILayout.Height(25f)))
                        {
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), kvp.Value);
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color32(0, 255, 255, 255);

                        if (GUILayout.Button("Load", GUILayout.Width(100f), GUILayout.Height(25f)))
                        {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            EditorSceneManager.OpenScene(kvp.Value);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = new Color32(0, 255, 255, 255);

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", GUILayout.Width(position.width - 20), GUILayout.Height(25f)))
                Refresh();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
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
