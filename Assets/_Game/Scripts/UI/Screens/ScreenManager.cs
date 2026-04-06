using UnityEngine;
using UnityEngine.UIElements;
using Policy.Core;

namespace Policy.UI
{
    /// <summary>
    /// Controls which top-level screen is visible in the UI Toolkit document.
    /// Screens are VisualElements with class names: "screen--splash", "screen--game", "screen--swipe".
    /// Active screen gets USS class "screen--active".
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private VisualElement _splash;
        private VisualElement _game;
        private VisualElement _swipe;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            _root   = uiDocument.rootVisualElement;
            _splash = _root.Q("screen-splash");
            _game   = _root.Q("screen-game");
            _swipe  = _root.Q("screen-swipe");

            ShowScreen(_splash);
        }

        public void ShowSplash() => ShowScreen(_splash);
        public void ShowGame()   => ShowScreen(_game);
        public void ShowSwipe()  => ShowScreen(_swipe);

        private void ShowScreen(VisualElement target)
        {
            foreach (var s in new[] { _splash, _game, _swipe })
            {
                if (s == null) continue;
                s.RemoveFromClassList("screen--active");
                s.AddToClassList("screen--hidden");
            }
            target?.RemoveFromClassList("screen--hidden");
            target?.AddToClassList("screen--active");
        }
    }
}
