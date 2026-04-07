using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Policy.Editor
{
    /// <summary>
    /// POLICY → Fix Input System
    /// Replaces StandaloneInputModule with InputSystemUIInputModule if New Input System is active.
    /// Run once — fixes the "You are trying to read Input using UnityEngine.Input" error.
    /// </summary>
    public static class FixInputSystem
    {
        [MenuItem("POLICY/Fix Input System")]
        public static void Fix()
        {
            var es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                Debug.LogWarning("[POLICY] No EventSystem found. Run POLICY → Build Scene first.");
                return;
            }

            // Remove old module
            var old = es.GetComponent<StandaloneInputModule>();
            if (old != null) Object.DestroyImmediate(old);

            // Try to add InputSystemUIInputModule
            var t = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null)
            {
                if (es.GetComponent(t) == null) es.gameObject.AddComponent(t);
                Debug.Log("[POLICY] ✓ Replaced StandaloneInputModule with InputSystemUIInputModule.");
            }
            else
            {
                // Fallback: re-add StandaloneInputModule
                if (es.GetComponent<StandaloneInputModule>() == null)
                    es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.LogWarning("[POLICY] InputSystemUIInputModule not found — kept StandaloneInputModule. " +
                                 "In Player Settings → Active Input Handling → set to 'Both' or 'Input Manager'.");
            }

            EditorUtility.SetDirty(es.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
