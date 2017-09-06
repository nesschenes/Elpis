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

            Debug.NessLog("EventSystem 產出");
            GameObject InputHelper = new GameObject("EventSystem", typeof(StandaloneInputModule));
            InputHelper.isStatic = true;
            Object.DontDestroyOnLoad(InputHelper);
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
