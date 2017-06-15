using UnityEditor;
using UnityEngine;

namespace Elpis
{
    internal static class Gaia
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeScene()
        {
            Debug.Log("Gaia 誕生");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterScene()
        {

        }

#if UNITY_EDITOR

        static Gaia()
        {
            EditorApplication.playmodeStateChanged -= UnityEditor_OnPlayModeChanged;
            EditorApplication.playmodeStateChanged += UnityEditor_OnPlayModeChanged;
        }

        private static void UnityEditor_OnPlayModeChanged()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Global.Instance.Socket.DisconnectImmediately();
            }
        }

#endif

    }
}
