using UnityEngine;
using UnityEngine.UIElements;
using Policy.Core;

namespace Policy.UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private void OnEnable()
        {
            var root   = uiDocument.rootVisualElement;
            var btn    = root.Q<Button>("btn-start");
            if (btn != null)
                btn.clicked += OnStartClicked;
        }

        private void OnDisable()
        {
            var root = uiDocument.rootVisualElement;
            var btn  = root.Q<Button>("btn-start");
            if (btn != null)
                btn.clicked -= OnStartClicked;
        }

        private void OnStartClicked()
        {
            GameManager.Instance.StartGame();
            ScreenManager.Instance.ShowGame();
        }
    }
}
