using DG.Tweening;
using TMPro;
using UnityEngine;
using Policy.Core;
using Policy.Systems;

namespace Policy.UI
{
    /// <summary>
    /// Weekly report modal. Shown on GameEvents.OnWeekEnded.
    /// </summary>
    public class WeeklyReportOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TextMeshProUGUI weekLabel;
        [SerializeField] private TextMeshProUGUI netWorthLabel;
        [SerializeField] private TextMeshProUGUI weekChangeLabel;
        [SerializeField] private TextMeshProUGUI returnLabel;
        [SerializeField] private TextMeshProUGUI legacyLabel;
        [SerializeField] private TextMeshProUGUI decisionsLabel;
        [SerializeField] private TextMeshProUGUI affectedLabel;
        [SerializeField] private Transform       collateralContainer;
        [SerializeField] private TextMeshProUGUI collateralRowPrefab;
        [SerializeField] private UnityEngine.UI.Button closeButton;

        private GameState State => GameManager.Instance.state;

        private void OnEnable()
        {
            GameEvents.OnWeekEnded += Show;
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            GameEvents.OnWeekEnded -= Show;
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        private void Show()
        {
            var s = State;
            if (weekLabel      != null) weekLabel.text      = $"WEEKLY STATEMENT · WEEK {s.week}";
            if (netWorthLabel  != null) netWorthLabel.text  = $"${Mathf.RoundToInt(s.balance):N0}";
            if (weekChangeLabel != null) weekChangeLabel.text = $"+${Mathf.RoundToInt(s.weekEarned):N0}";
            if (returnLabel    != null) returnLabel.text    = $"+${Mathf.RoundToInt(s.weekEarned):N0}";
            if (legacyLabel    != null) legacyLabel.text    = $"{s.legacy}/100";
            if (decisionsLabel != null) decisionsLabel.text = s.decisionsThisWeek.ToString();
            if (affectedLabel  != null) affectedLabel.text  = s.affected.ToString("N0");

            if (collateralContainer != null)
            {
                foreach (Transform t in collateralContainer) Destroy(t.gameObject);
                var recent = s.collateralLog.Count > 3 ? s.collateralLog.GetRange(s.collateralLog.Count - 3, 3) : s.collateralLog;
                if (recent.Count == 0 && collateralRowPrefab != null)
                {
                    var row = Instantiate(collateralRowPrefab, collateralContainer);
                    row.text = "No collateral yet.";
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
            canvasGroup.DOFade(1f, 0.2f);
        }

        private void Close()
        {
            canvasGroup.DOFade(0f, 0.15f).OnComplete(() => gameObject.SetActive(false));
        }
    }
}
