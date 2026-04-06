using DG.Tweening;
using UnityEngine;
using Policy.Core;
using Policy.Data;
using Policy.Systems;

namespace Policy.UI
{
    /// <summary>
    /// Manages the 3-card visual deck on Canvas.
    /// Instantiates SwipeCardView prefabs, positions back cards,
    /// and promotes the deck forward after each resolution.
    /// </summary>
    public class CardDeckView : MonoBehaviour
    {
        [SerializeField] private SwipeCardView cardPrefab;
        [SerializeField] private CardDeckSystem deckSystem;
        [SerializeField] private RectTransform  deckParent;

        // Layout
        private static readonly Vector2  BackOffset1  = new(0f, 8f);
        private static readonly Vector2  BackOffset2  = new(0f, 16f);
        private static readonly Vector3  BackScale1   = new(0.96f, 0.96f, 1f);
        private static readonly Vector3  BackScale2   = new(0.92f, 0.92f, 1f);
        private static readonly float    PromoteDuration = 0.2f;

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
            for (int i = 0; i < 3; i++)
            {
                if (i >= top.Length || top[i] == null) continue;
                _cards[i] = SpawnCard(top[i], i);
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
                case 0: card.SetDepth(2, Vector2.zero,  Vector3.one);   break;
                case 1: card.SetDepth(1, BackOffset1,   BackScale1);    break;
                case 2: card.SetDepth(0, BackOffset2,   BackScale2);    break;
            }
        }

        private void OnCardResolved(SwipeCardView card, CardData data, SwipeDirection dir)
        {
            var choice = data.GetChoice(dir);
            if (choice != null)
                CardResolveSystem.Apply(data, choice, dir);

            deckSystem.ConsumeTop();
            if (dir == SwipeDirection.Ignore)
                deckSystem.ReturnLater(data);

            Destroy(card.gameObject);

            // Promote back cards with DOTween
            PromoteBackCards();
        }

        private void PromoteBackCards()
        {
            // Shift _cards[1] → [0], _cards[2] → [1]
            _cards[0] = _cards[1];
            _cards[1] = _cards[2];
            _cards[2] = null;

            if (_cards[0] != null)
            {
                _cards[0].OnResolved = null;
                var rt0 = _cards[0].GetComponent<RectTransform>();
                rt0.DOAnchorPos(Vector2.zero, PromoteDuration).SetEase(Ease.OutCubic);
                rt0.DOScale(Vector3.one,      PromoteDuration);
                rt0.SetSiblingIndex(2);
                var next = deckSystem.PeekTop(1);
                if (next.Length > 0) _cards[0].OnResolved += d => OnCardResolved(_cards[0], next[0], d);
            }

            if (_cards[1] != null)
            {
                var rt1 = _cards[1].GetComponent<RectTransform>();
                rt1.DOAnchorPos(BackOffset1, PromoteDuration).SetEase(Ease.OutCubic);
                rt1.DOScale(BackScale1,      PromoteDuration);
                rt1.SetSiblingIndex(1);
            }

            // Spawn new back card
            var upcoming = deckSystem.PeekTop(3);
            if (upcoming.Length >= 3)
            {
                _cards[2] = SpawnCard(upcoming[2], 2);
            }

            if (deckSystem.Count == 0 && _cards[0] == null)
                deckSystem.Refill();
        }
    }
}
