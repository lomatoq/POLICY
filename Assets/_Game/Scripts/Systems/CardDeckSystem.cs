using System.Collections.Generic;
using UnityEngine;
using Policy.Data;

namespace Policy.Systems
{
    /// <summary>
    /// Manages the card queue: initial order, ignore-return logic, refill.
    /// </summary>
    public class CardDeckSystem : MonoBehaviour
    {
        [SerializeField] private CardData[] allCards;

        private readonly Queue<CardData> _queue       = new();
        private readonly List<(CardData card, int returnWeek)> _ignored = new();

        private Policy.Core.GameState State => Policy.Core.GameManager.Instance.state;

        private void Start()
        {
            Refill();
            Policy.Core.GameEvents.OnWeekEnded += OnWeekEnded;
        }

        private void OnDestroy()
        {
            Policy.Core.GameEvents.OnWeekEnded -= OnWeekEnded;
        }

        public CardData Peek()
        {
            TryReturnIgnored();
            return _queue.Count > 0 ? _queue.Peek() : null;
        }

        public CardData[] PeekTop(int count)
        {
            TryReturnIgnored();
            var arr = new List<CardData>();
            foreach (var c in _queue)
            {
                if (arr.Count >= count) break;
                arr.Add(c);
            }
            return arr.ToArray();
        }

        public void ConsumeTop()
        {
            if (_queue.Count > 0) _queue.Dequeue();
        }

        public void ReturnLater(CardData card, int inWeeks = 2)
        {
            _ignored.Add((card, State.week + inWeeks));
        }

        public int Count => _queue.Count;

        private void TryReturnIgnored()
        {
            for (int i = _ignored.Count - 1; i >= 0; i--)
            {
                if (State.week >= _ignored[i].returnWeek)
                {
                    _queue.Enqueue(_ignored[i].card);
                    _ignored.RemoveAt(i);
                }
            }
        }

        private void OnWeekEnded()
        {
            if (_queue.Count == 0) Refill();
        }

        public void Refill()
        {
            _queue.Clear();
            // Shuffle allCards into queue
            var shuffled = new List<CardData>(allCards);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            foreach (var c in shuffled) _queue.Enqueue(c);
        }
    }
}
