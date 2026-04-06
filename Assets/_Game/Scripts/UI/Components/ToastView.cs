using TMPro;
using UnityEngine;
using Policy.Core;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Policy.UI
{
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float           holdDuration = 2.2f;

#if DOTWEEN
        private Tween _current;
#endif

        private void OnEnable()  => GameEvents.OnToast += Show;
        private void OnDisable() => GameEvents.OnToast -= Show;

        public void Show(string msg)
        {
            label.text        = msg;
            canvasGroup.alpha = 0f;

#if DOTWEEN
            _current?.Kill();
            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.2f));
            seq.AppendInterval(holdDuration);
            seq.Append(canvasGroup.DOFade(0f, 0.25f));
            _current = seq;
#else
            StopAllCoroutines();
            StartCoroutine(ShowCoroutine());
#endif
        }

#if !DOTWEEN
        private System.Collections.IEnumerator ShowCoroutine()
        {
            float t = 0f;
            while (t < 0.2f) { t += Time.deltaTime; canvasGroup.alpha = t / 0.2f; yield return null; }
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(holdDuration);
            t = 0f;
            while (t < 0.25f) { t += Time.deltaTime; canvasGroup.alpha = 1f - t / 0.25f; yield return null; }
            canvasGroup.alpha = 0f;
        }
#endif
    }
}
