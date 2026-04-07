using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Policy.Editor
{
    public static class FixInputSystem
    {
        [MenuItem("POLICY/Fix Input System")]
        public static void Fix()
        {
            // Fix EventSystem module
            var es = Object.FindFirstObjectByType<EventSystem>();
            if (es != null)
            {
                var old = es.GetComponent<StandaloneInputModule>();
                if (old != null) Object.DestroyImmediate(old);

                var t = System.Type.GetType(
                    "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (t != null && es.GetComponent(t) == null)
                {
                    es.gameObject.AddComponent(t);
                    Debug.Log("[POLICY] ✓ Added InputSystemUIInputModule.");
                }
                else if (t == null && es.GetComponent<StandaloneInputModule>() == null)
                {
                    es.gameObject.AddComponent<StandaloneInputModule>();
                }
                EditorUtility.SetDirty(es.gameObject);
            }

            // Force PlayerSettings to Both (activeInputHandler=2)
            // This must be done via SerializedObject since there's no public API
            var ps = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            var psPath = "ProjectSettings/ProjectSettings.asset";
            var psObj = AssetDatabase.LoadAllAssetsAtPath(psPath);
            if (psObj.Length > 0)
            {
                var so = new SerializedObject(psObj[0]);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue != 2)
                {
                    prop.intValue = 2;
                    so.ApplyModifiedProperties();
                    Debug.Log("[POLICY] ✓ Set activeInputHandler to Both (2).");
                }
                else
                {
                    Debug.Log($"[POLICY] activeInputHandler already = {prop?.intValue}");
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("POLICY", "Input System fixed!\nRestart Unity Editor to apply fully.", "OK");
        }
    }
}
