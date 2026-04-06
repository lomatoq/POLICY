using UnityEngine;
using Policy.Core;
using Policy.Data;
using Policy.Systems;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Policy.UI
{
    /// <summary>
    /// Manages the 3-card visual deck on Canvas.
    /// Instantiates SwipeCardView prefabs and promotes the deck after each resolution.
    /// </summary>
    public class CardDeckView : MonoBehaviour
    {
        [SerializeField] private SwipeCardView cardPrefab;
        [SerializeField] private CardDeckSystem deckSystem;
        [SerializeField] private RectTransform  deckParent;

        private static readonly Vector2 BackOffset1   = new(0f,  8f);
        private static readonly Vector2 BackOffset2   = new(0f, 16f);
        private static readonly Vector3 BackScale1    = new(0.96f, 0.96f, 1f);
        private static readonly Vector3 BackScale2    = new(0.92f, 0.92f, 1f);
        private const           float   PromoteDur    = 0.2f;

        private SwipeCardView[] _cards = new SwipeCardView[3];

        private void Start()
        {
            BuildDeck();
            GameEvents.OnGameStarted += BuildDeck;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStarted -= BuildDeck;
        }

        private void BuildDeck()
        {
            foreach (var c in _cards)
                if (c != null) Destroy(c.gameObject);

            var top = deckSystem.PeekTop(3);
            for (int i = 0; i < 3 && i < top.Length; i++)
            {
                if (top[i] != null) _cards[i] = SpawnCard(top[i], i);
            }
        }

        private SwipeCardView SpawnCard(CardData data, int depth)
        {
            var card = Instantiate(cardPrefab, deckParent);
            card.Bind(data);
            ApplyDepth(card, depth);
            if (depth == 0)
                card.OnResolved += dir => OnCardResolved(card, data, dir);
            return card;
        }

        private static void ApplyDepth(SwipeCardView card, int depth)
        {
            switch (depth)
            {
                case 0: card.SetDepth(2, Vector2.zero, Vector3.one); break;
                case 1: card.SetDepth(1, BackOffset1,  BackScale1);  break;
                case 2: card.SetDepth(0, BackOffset2,  BackScale2);  break;
            }
        }

        private void OnCardResolved(SwipeCardView card, CardData data, SwipeDirection dir)
        {
            var choice = data.GetChoice(dir);
            if (choice != null) CardResolveSystem.Apply(data, choice, dir);

            deckSystem.ConsumeTop();
            if (dir == SwipeDirection.Ignore) deckSystem.ReturnLater(data);

            Destroy(card.gameObject);
            PromoteBackCards();
        }

        private void PromoteBackCards()
        {
            _cards[0] = _cards[1];
            _cards[1] = _cards[2];
            _cards[2] = null;

            if (_cards[0] != null)
            {
                var rt = _cards[0].GetComponent<RectTransform>();
                rt.SetSiblingIndex(2);
#if DOTWEEN
                rt.DOAnchorPos(Vector2.zero, PromoteDur).SetEase(Ease.OutCubic);
                rt.DOScale(Vector3.one,      PromoteDur);
#else
                rt.anchoredPosition = Vector2.zero;
                rt.localScale       = Vector3.one;
#endif
                var next = deckSystem.PeekTop(1);
                if (next.Length > 0)
                    _cards[0].OnResolved += d => OnCardResolved(_cards[0], next[0], d);
            }

            if (_cards[1] != null)
            {
                var rt = _cards[1].GetComponent<RectTransform>();
                rt.SetSiblingIndex(1);
#if DOTWEEN
                rt.DOAnchorPos(BackOffset1, PromoteDur).SetEase(Ease.OutCubic);
                rt.DOScale(BackScale1,      PromoteDur);
#else
                rt.anchoredPosition = BackOffset1;
                rt.localScale       = BackScale1;
#endif
            }

            // Spawn new back card
            var upcoming = deckSystem.PeekTop(3);
            if (upcoming.Length >= 3)
                _cards[2] = SpawnCard(upcoming[2], 2);

            if (deckSystem.Count == 0 && _cards[0] == null)
                deckSystem.Refill();
        }
    }
}
