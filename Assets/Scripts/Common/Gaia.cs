using UnityEngine;
using UnityEngine.EventSystems;

namespace Elpis
{
    internal static class Gaia
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeScene()
        {
            Debug.NessLog("Gaia 誕生");

            GameObject inputHelper = new GameObject("EventSystem", typeof(StandaloneInputModule));
            inputHelper.isStatic = true;
            Object.DontDestroyOnLoad(inputHelper);
            Debug.NessLog("EventSystem 已被產出");

            GameObject undeadMono = new GameObject("UndeadMono", typeof(UndeadMono));
            undeadMono.isStatic = true;
            Object.DontDestroyOnLoad(undeadMono);
            Debug.NessLog("UndeadMono 已被產出");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterScene()
        {

        }

        #if UNITY_EDITOR

        static Gaia()
        {
            UnityEditor.EditorApplication.playmodeStateChanged -= UnityEditor_OnPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += UnityEditor_OnPlayModeChanged;
        }

        private static void UnityEditor_OnPlayModeChanged()
        {
            if (UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Global.Instance.Socket.DisconnectImmediately();
            }
        }

        #endif

    }
}
