using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Policy.Core;
using Policy.Data;

namespace Policy.UI
{
    /// <summary>
    /// Dark overlay popup shown after each card resolution.
    /// Auto-dismisses after 2.2 seconds.
    /// </summary>
    public class OutcomeFlashOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup      canvasGroup;
        [SerializeField] private TextMeshProUGUI  fromLabel;
        [SerializeField] private TextMeshProUGUI  titleLabel;
        [SerializeField] private Transform        effectsContainer;
        [SerializeField] private TextMeshProUGUI  collateralLabel;
        [SerializeField] private GameObject       effectRowPrefab; // TMP label prefab

        [SerializeField] private float showDuration = 2.2f;

        private Tween _tween;

        private void OnEnable()  => GameEvents.OnCardResolved += Show;
        private void OnDisable() => GameEvents.OnCardResolved -= Show;

        private void Show(CardData card, CardChoice choice, SwipeDirection dir)
        {
            var dirLabels = new System.Collections.Generic.Dictionary<SwipeDirection, string>
            {
                { SwipeDirection.Approve,  "✓ Approved"  },
                { SwipeDirection.Decline,  "✗ Declined"  },
                { SwipeDirection.Escalate, "↑ Escalated" },
                { SwipeDirection.Ignore,   "↓ Filed"     },
            };

            fromLabel.text = $"{dirLabels[dir]} · {card.type}";
            titleLabel.text = card.title.Length > 80 ? card.title[..80] + "…" : card.title;

            // Clear effects
            foreach (Transform t in effectsContainer) Destroy(t.gameObject);

            AddEffect(choice.incomeDelta  != 0 ? $"{(choice.incomeDelta > 0 ? "+" : "")}${Mathf.Abs(choice.incomeDelta):N0} income {(choice.incomeDelta > 0 ? "gain" : "cost")}" : null, choice.incomeDelta > 0 ? "g" : "r");
            AddEffect(choice.legacyDelta  != 0 ? $"{(choice.legacyDelta > 0 ? "+" : "")}{choice.legacyDelta} legacy score" : null, choice.legacyDelta > 0 ? "g" : "r");
            AddEffect(choice.riskDelta    != 0 ? $"{(choice.riskDelta > 0 ? "+" : "")}{choice.riskDelta}% operational risk" : null, choice.riskDelta > 0 ? "r" : "g");
            if (choice.scopeDelta != 0) AddEffect($"+{Mathf.Abs(choice.scopeDelta * 12)} people affected", "w");
            if (!string.IsNullOrEmpty(choice.policy)) AddEffect("Policy created → see Policy tab", "y");

            collateralLabel.text = !string.IsNullOrEmpty(choice.collateral)
                ? choice.collateral
                : "Outcome pending — check back in 3 days.";

            _tween?.Kill();
            canvasGroup.alpha = 0f;
            gameObject.SetActive(true);

            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.15f));
            seq.AppendInterval(showDuration);
            seq.Append(canvasGroup.DOFade(0f, 0.2f));
            seq.OnComplete(() => gameObject.SetActive(false));
            _tween = seq;
        }

        private void AddEffect(string text, string colorClass)
        {
            if (string.IsNullOrEmpty(text)) return;
            var go  = Instantiate(effectRowPrefab, effectsContainer);
            var lbl = go.GetComponent<TextMeshProUGUI>();
            if (lbl == null) return;
            lbl.text = text;
            lbl.color = colorClass switch
            {
                "g" => new Color(0.13f, 0.77f, 0.37f),
                "r" => new Color(0.94f, 0.27f, 0.27f),
                "y" => new Color(0.97f, 0.62f, 0.07f),
                _   => new Color(0.47f, 0.47f, 0.47f),
            };
        }
    }
}
