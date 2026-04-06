using DG.Tweening;
using TMPro;
using UnityEngine;
using Policy.Core;
using Policy.Data;

namespace Policy.UI
{
    public class OutcomeFlashOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TextMeshProUGUI fromLabel;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private Transform       effectsContainer;
        [SerializeField] private TextMeshProUGUI collateralLabel;
        [SerializeField] private GameObject      effectRowPrefab;
        [SerializeField] private float           showDuration = 2.2f;

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

            if (fromLabel  != null) fromLabel.text  = $"{dirLabels[dir]} · {card.type}";
            if (titleLabel != null) titleLabel.text = card.title.Length > 80 ? card.title[..80] + "…" : card.title;

            foreach (Transform t in effectsContainer) Destroy(t.gameObject);

            TryAddEffect(choice.incomeDelta  != 0, $"{(choice.incomeDelta  > 0 ? "+" : "")}${Mathf.Abs(choice.incomeDelta):N0} income {(choice.incomeDelta  > 0 ? "gain" : "cost")}", choice.incomeDelta  > 0 ? "g" : "r");
            TryAddEffect(choice.legacyDelta  != 0, $"{(choice.legacyDelta  > 0 ? "+" : "")}{choice.legacyDelta} legacy score",       choice.legacyDelta  > 0 ? "g" : "r");
            TryAddEffect(choice.riskDelta    != 0, $"{(choice.riskDelta    > 0 ? "+" : "")}{choice.riskDelta}% operational risk",    choice.riskDelta    > 0 ? "r" : "g");
            TryAddEffect(choice.scopeDelta   != 0, $"+{Mathf.Abs(choice.scopeDelta * 12)} people affected",                          "w");
            TryAddEffect(!string.IsNullOrEmpty(choice.policy), "Policy created → see Policy tab",                                    "y");

            if (collateralLabel != null)
                collateralLabel.text = !string.IsNullOrEmpty(choice.collateral)
                    ? choice.collateral : "Outcome pending — check back in 3 days.";

            _tween?.Kill();
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            _tween = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, 0.15f))
                .AppendInterval(showDuration)
                .Append(canvasGroup.DOFade(0f, 0.2f))
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void TryAddEffect(bool condition, string text, string colorClass)
        {
            if (!condition || effectRowPrefab == null) return;
            var lbl = Instantiate(effectRowPrefab, effectsContainer).GetComponent<TextMeshProUGUI>();
            if (lbl == null) return;
            lbl.text  = text;
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
