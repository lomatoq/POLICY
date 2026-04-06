using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Policy.Core;
using Policy.Systems;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Policy.UI
{
    public class WeeklyReportOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TextMeshProUGUI weekLabel;
        [SerializeField] private TextMeshProUGUI netWorthLabel;
        [SerializeField] private TextMeshProUGUI weekChangeLabel;
        [SerializeField] private TextMeshProUGUI legacyLabel;
        [SerializeField] private TextMeshProUGUI decisionsLabel;
        [SerializeField] private TextMeshProUGUI affectedLabel;
        [SerializeField] private Transform       collateralContainer;
        [SerializeField] private TextMeshProUGUI collateralRowPrefab;
        [SerializeField] private Button          closeButton;

        private GameState State => GameManager.Instance.state;

        private void OnEnable()
        {
            GameEvents.OnWeekEnded += Show;
            closeButton?.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            GameEvents.OnWeekEnded -= Show;
            closeButton?.onClick.RemoveListener(Close);
        }

        private void Show()
        {
            var s = State;
            if (weekLabel       != null) weekLabel.text       = $"WEEKLY STATEMENT · WEEK {s.week}";
            if (netWorthLabel   != null) netWorthLabel.text   = $"${Mathf.RoundToInt(s.balance):N0}";
            if (weekChangeLabel != null) weekChangeLabel.text = $"+${Mathf.RoundToInt(s.weekEarned):N0}";
            if (legacyLabel     != null) legacyLabel.text     = $"{s.legacy}/100";
            if (decisionsLabel  != null) decisionsLabel.text  = s.decisionsThisWeek.ToString();
            if (affectedLabel   != null) affectedLabel.text   = s.affected.ToString("N0");

            if (collateralContainer != null)
            {
                foreach (Transform t in collateralContainer) Destroy(t.gameObject);
                var log    = s.collateralLog;
                var recent = log.Count > 3 ? log.GetRange(log.Count - 3, 3) : new List<string>(log);
                if (recent.Count == 0 && collateralRowPrefab != null)
                {
                    Instantiate(collateralRowPrefab, collateralContainer).text = "No collateral yet.";
                }
                else
                {
                    for (int i = 0; i < recent.Count; i++)
                    {
                        if (collateralRowPrefab == null) break;
                        var row = Instantiate(collateralRowPrefab, collateralContainer);
                        row.text = $"{i + 1}. {(recent[i].Length > 90 ? recent[i][..90] : recent[i])}";
                    }
                }
            }

            s.weekEarned = 0f;
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

#if DOTWEEN
            canvasGroup.DOFade(1f, 0.2f);
#else
            StartCoroutine(FadeIn());
#endif
        }

        private void Close()
        {
#if DOTWEEN
            canvasGroup.DOFade(0f, 0.15f).OnComplete(() => gameObject.SetActive(false));
#else
            StartCoroutine(FadeOut());
#endif
        }

#if !DOTWEEN
        private System.Collections.IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.2f) { t += Time.deltaTime; canvasGroup.alpha = t / 0.2f; yield return null; }
            canvasGroup.alpha = 1f;
        }
        private System.Collections.IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < 0.15f) { t += Time.deltaTime; canvasGroup.alpha = 1f - t / 0.15f; yield return null; }
            gameObject.SetActive(false);
        }
#endif
    }
}
