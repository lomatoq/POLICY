using DG.Tweening;
using TMPro;
using UnityEngine;
using Policy.Core;

namespace Policy.UI
{
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float           holdDuration = 2.2f;

        private Tween _current;

        private void OnEnable()  => GameEvents.OnToast += Show;
        private void OnDisable() => GameEvents.OnToast -= Show;

        public void Show(string msg)
        {
            label.text        = msg;
            canvasGroup.alpha = 0f;
            _current?.Kill();
            _current = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, 0.2f))
                .AppendInterval(holdDuration)
                .Append(canvasGroup.DOFade(0f, 0.25f));
        }
    }
}
