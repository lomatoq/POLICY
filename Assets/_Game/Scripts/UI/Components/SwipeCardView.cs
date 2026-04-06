using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Policy.Core;
using Policy.Data;

namespace Policy.UI
{
    /// <summary>
    /// One swipe card on the Canvas. Handles drag, rotate, overlay reveal, and fly-out.
    /// After the fly-out tween completes it calls OnResolved(direction).
    /// </summary>
    public class SwipeCardView : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI typeLabel;
        [SerializeField] private TextMeshProUGUI fromLabel;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI contextLabel;

        [Header("Overlays")]
        [SerializeField] private CanvasGroup overlayLeft;
        [SerializeField] private CanvasGroup overlayRight;
        [SerializeField] private CanvasGroup overlayUp;
        [SerializeField] private CanvasGroup overlayDown;

        [Header("Swipe Config")]
        [SerializeField] private float showOverlayThreshold  = 40f;
        [SerializeField] private float resolveThreshold      = 80f;
        [SerializeField] private float flyOutDistance        = 1200f;
        [SerializeField] private float flyOutDuration        = 0.35f;
        [SerializeField] private float snapBackDuration      = 0.25f;

        public Action<SwipeDirection> OnResolved;

        private CardData _data;
        private Vector2  _pointerStart;
        private Vector2  _dragDelta;
        private bool     _dragging;
        private bool     _resolved;

        // ── Public API ──────────────────────────────────────────────

        public void Bind(CardData data)
        {
            _data    = data;
            _resolved = false;

            var typeNames = new System.Collections.Generic.Dictionary<CardType, string>
            {
                { CardType.Decision,   "Decision"        },
                { CardType.Investment, "Investment"       },
                { CardType.Policy,     "Policy Proposal" },
                { CardType.Event,      "Breaking Event"  },
                { CardType.Prestige,   "Prestige Offer"  },
            };

            if (typeLabel   != null) typeLabel.text    = typeNames.GetValueOrDefault(data.type, "Card");
            if (fromLabel   != null) fromLabel.text    = data.from;
            if (titleLabel  != null) titleLabel.text   = data.title;
            if (contextLabel != null) contextLabel.text = data.context;

            // Accent color on type label
            if (typeLabel != null) typeLabel.color = data.accentColor;
        }

        public void SetDepth(int siblingIndex, Vector2 offset, Vector3 scale)
        {
            transform.SetSiblingIndex(siblingIndex);
            rectTransform.anchoredPosition = offset;
            rectTransform.localScale       = scale;
        }

        // ── Drag ────────────────────────────────────────────────────

        public void OnPointerDown(PointerEventData e)
        {
            if (_resolved) return;
            _pointerStart = rectTransform.anchoredPosition;
            _dragDelta    = Vector2.zero;
            _dragging     = true;
            DOTween.Kill(rectTransform);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging || _resolved) return;
            _dragDelta += e.delta;
            rectTransform.anchoredPosition = _pointerStart + _dragDelta;
            rectTransform.localEulerAngles = new Vector3(0f, 0f, -_dragDelta.x * 0.08f);
            UpdateOverlays(_dragDelta);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_dragging || _resolved) return;
            _dragging = false;

            float absX = Mathf.Abs(_dragDelta.x);
            float absY = Mathf.Abs(_dragDelta.y);

            if (_dragDelta.x > resolveThreshold)                          Resolve(SwipeDirection.Approve);
            else if (_dragDelta.x < -resolveThreshold)                    Resolve(SwipeDirection.Decline);
            else if (_dragDelta.y > resolveThreshold && absY > absX)      Resolve(SwipeDirection.Ignore);
            else if (_dragDelta.y < -resolveThreshold && absY > absX)     Resolve(SwipeDirection.Escalate);
            else                                                           SnapBack();
        }

        // ── Resolve ─────────────────────────────────────────────────

        private void Resolve(SwipeDirection dir)
        {
            _resolved = true;
            HideAllOverlays();

            Vector2 target = dir switch
            {
                SwipeDirection.Approve  => Vector2.right * flyOutDistance,
                SwipeDirection.Decline  => Vector2.left  * flyOutDistance,
                SwipeDirection.Escalate => Vector2.up    * flyOutDistance * 1.5f,
                SwipeDirection.Ignore   => Vector2.down  * flyOutDistance,
                _                       => Vector2.right * flyOutDistance
            };

            float rotation = target.x * 0.05f;

            var seq = DOTween.Sequence();
            seq.Append(rectTransform.DOAnchorPos(target, flyOutDuration).SetEase(Ease.InCubic));
            seq.Join(rectTransform.DOLocalRotate(new Vector3(0, 0, rotation), flyOutDuration));
            seq.Join(canvasGroup.DOFade(0f, flyOutDuration * 0.8f));
            seq.OnComplete(() => OnResolved?.Invoke(dir));
        }

        private void SnapBack()
        {
            HideAllOverlays();
            var seq = DOTween.Sequence();
            seq.Append(rectTransform.DOAnchorPos(Vector2.zero,   snapBackDuration).SetEase(Ease.OutBack));
            seq.Join(rectTransform.DOLocalRotate(Vector3.zero,    snapBackDuration));
        }

        // ── Helpers ─────────────────────────────────────────────────

        private void UpdateOverlays(Vector2 delta)
        {
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);
            float t    = showOverlayThreshold;
            float rng  = 60f;

            SetOverlay(overlayRight, delta.x > t    ? Mathf.Clamp01((absX - t) / rng) : 0f);
            SetOverlay(overlayLeft,  delta.x < -t   ? Mathf.Clamp01((absX - t) / rng) : 0f);
            SetOverlay(overlayUp,    delta.y < -t && absY > absX ? Mathf.Clamp01((absY - t) / rng) : 0f);
            SetOverlay(overlayDown,  delta.y > t  && absY > absX ? Mathf.Clamp01((absY - t) / rng) : 0f);
        }

        private static void SetOverlay(CanvasGroup cg, float alpha)
        {
            if (cg != null) cg.alpha = alpha;
        }

        private void HideAllOverlays()
        {
            SetOverlay(overlayLeft,  0f);
            SetOverlay(overlayRight, 0f);
            SetOverlay(overlayUp,    0f);
            SetOverlay(overlayDown,  0f);
        }

        private void OnDestroy() => DOTween.Kill(rectTransform);
    }
}
