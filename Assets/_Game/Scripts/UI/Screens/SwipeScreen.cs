using UnityEngine;
using UnityEngine.UIElements;
using Policy.Core;
using Policy.Systems;

namespace Policy.UI
{
    /// <summary>
    /// Swipe screen topbar + meters (UI Toolkit side).
    /// The actual card deck lives on the Canvas layer (CardDeckView.cs).
    /// </summary>
    public class SwipeScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private Label         _stbSub, _stbBal;
        private VisualElement _mIncome, _mLegacy, _mRisk, _mScope;

        private GameState State => GameManager.Instance != null ? GameManager.Instance.state : null;

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;
            _stbSub  = root.Q<Label>("stb-sub");
            _stbBal  = root.Q<Label>("stb-bal");
            _mIncome = root.Q("m-income");
            _mLegacy = root.Q("m-legacy");
            _mRisk   = root.Q("m-risk");
            _mScope  = root.Q("m-scope");

            var backBtn = root.Q<Button>("btn-back");
            if (backBtn != null) backBtn.clicked += () => ScreenManager.Instance.ShowGame();

            GameEvents.OnStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnStateChanged -= Refresh;
        }

        private void Refresh()
        {
            var s = State;
            if (s == null) return;
            if (_stbSub != null) _stbSub.text = $"Week {s.week} · {CardDeckSystem_Pending()} pending";
            if (_stbBal != null) _stbBal.text = $"${Mathf.RoundToInt(s.incomePerHour)}/hr";

            SetMeter(_mIncome, Mathf.Min(100f, s.incomePerHour / 3f));
            SetMeter(_mLegacy, s.legacy);
            SetMeter(_mRisk,   s.risk);
            SetMeter(_mScope,  Mathf.Min(100f, s.scope));
        }

        private static void SetMeter(VisualElement fill, float pct)
        {
            if (fill != null) fill.style.width = Length.Percent(Mathf.Clamp(pct, 0, 100));
        }

        private int CardDeckSystem_Pending()
        {
            var sys = FindFirstObjectByType<CardDeckSystem>();
            return sys != null ? Mathf.Min(3, sys.Count) : 0;
        }
    }
}
