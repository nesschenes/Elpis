using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Elpis
{
    [InitializeOnLoad]
    public static class EditorSceneEventCallback
    {
        public static Action<Scene, NewSceneSetup, NewSceneMode> OnSceneCreated = delegate { };
        public static Action<Scene> OnSceneClosed = delegate { };
        public static Action<Scene, bool> OnSceneClosing = delegate { };
        public static Action<Scene, OpenSceneMode> OnSceneOpened = delegate { };
        public static Action<string, OpenSceneMode> OnSceneOpening = delegate { };
        public static Action<Scene> OnSceneSaved = delegate { };
        public static Action<Scene, string> OnSceneSaving = delegate { };

        static EditorSceneEventCallback()
        {
            EditorSceneManager.newSceneCreated += (scene, setup, mode) =>
            {
                OnSceneCreated(scene, setup, mode);
            };

            EditorSceneManager.sceneClosed += (scene) =>
            {
                OnSceneClosed(scene);
            };

            EditorSceneManager.sceneClosing += (scene, isRemovingScene) =>
            {
                OnSceneClosing(scene, isRemovingScene);
            };

            EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                OnSceneOpened(scene, mode);
            };

            EditorSceneManager.sceneOpening += (path, mode) =>
            {
                OnSceneOpening(path, mode);
            };

            EditorSceneManager.sceneSaved += (scene) =>
            {
                OnSceneSaved(scene);
            };

            EditorSceneManager.sceneSaving += (scene, path) =>
            {
                OnSceneSaving(scene, path);
            };
        }
    }
}
