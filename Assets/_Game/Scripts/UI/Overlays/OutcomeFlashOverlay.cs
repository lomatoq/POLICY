using TMPro;
using UnityEngine;
using Policy.Core;
using Policy.Data;
#if DOTWEEN
using DG.Tweening;
#endif

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

#if DOTWEEN
        private Tween _tween;
#endif

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
            if (titleLabel != null) titleLabel.text  = card.title.Length > 80 ? card.title[..80] + "…" : card.title;

            foreach (Transform t in effectsContainer) Destroy(t.gameObject);

            AddEffect(choice.incomeDelta  != 0 ? $"{(choice.incomeDelta  > 0 ? "+" : "")}${Mathf.Abs(choice.incomeDelta):N0} income {(choice.incomeDelta  > 0 ? "gain" : "cost")}" : null, choice.incomeDelta  > 0 ? "g" : "r");
            AddEffect(choice.legacyDelta  != 0 ? $"{(choice.legacyDelta  > 0 ? "+" : "")}{choice.legacyDelta} legacy score" : null,                                                        choice.legacyDelta  > 0 ? "g" : "r");
            AddEffect(choice.riskDelta    != 0 ? $"{(choice.riskDelta    > 0 ? "+" : "")}{choice.riskDelta}% operational risk" : null,                                                     choice.riskDelta    > 0 ? "r" : "g");
            if (choice.scopeDelta != 0)                   AddEffect($"+{Mathf.Abs(choice.scopeDelta * 12)} people affected", "w");
            if (!string.IsNullOrEmpty(choice.policy))     AddEffect("Policy created → see Policy tab", "y");

            if (collateralLabel != null)
                collateralLabel.text = !string.IsNullOrEmpty(choice.collateral)
                    ? choice.collateral : "Outcome pending — check back in 3 days.";

            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

#if DOTWEEN
            _tween?.Kill();
            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.15f));
            seq.AppendInterval(showDuration);
            seq.Append(canvasGroup.DOFade(0f, 0.2f));
            seq.OnComplete(() => gameObject.SetActive(false));
            _tween = seq;
#else
            StopAllCoroutines();
            StartCoroutine(ShowCoroutine());
#endif
        }

        private void AddEffect(string text, string colorClass)
        {
            if (string.IsNullOrEmpty(text) || effectRowPrefab == null) return;
            var go  = Instantiate(effectRowPrefab, effectsContainer);
            var lbl = go.GetComponent<TextMeshProUGUI>();
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

#if !DOTWEEN
        private System.Collections.IEnumerator ShowCoroutine()
        {
            float t = 0f;
            while (t < 0.15f) { t += Time.deltaTime; canvasGroup.alpha = t / 0.15f; yield return null; }
            yield return new WaitForSeconds(showDuration);
            t = 0f;
            while (t < 0.2f) { t += Time.deltaTime; canvasGroup.alpha = 1f - t / 0.2f; yield return null; }
            gameObject.SetActive(false);
        }
#endif
    }
}
