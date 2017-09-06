using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Elpis
{
    public class MenuItemTool : Editor
    {
        [MenuItem("Elpis/QuickLoadLoginScene %#l")]
        static void QuickToLogin()
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            EditorSceneManager.OpenScene("Assets/Scenes/Login.unity");
        }

        [MenuItem("Elpis/QuickLoadToolScene %#t")]
        static void QuickToTool()
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            EditorSceneManager.OpenScene("Assets/Scenes/Tool.unity");
        }
    }
}

