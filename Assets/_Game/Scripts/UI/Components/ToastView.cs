using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Policy.Core;

namespace Policy.UI
{
    /// <summary>
    /// Floating toast notification. Listens to GameEvents.OnToast.
    /// Fades in, holds, fades out.
    /// </summary>
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup   canvasGroup;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float          holdDuration = 2.2f;

        private Tween _current;

        private void OnEnable()  => GameEvents.OnToast += Show;
        private void OnDisable() => GameEvents.OnToast -= Show;

        public void Show(string msg)
        {
            label.text = msg;
            _current?.Kill();
            canvasGroup.alpha = 0f;

            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.2f));
            seq.AppendInterval(holdDuration);
            seq.Append(canvasGroup.DOFade(0f, 0.25f));
            _current = seq;
        }
    }
}
