using System;
using Policy.Data;

namespace Policy.Core
{
    public enum SwipeDirection { Approve, Decline, Escalate, Ignore }

    public static class GameEvents
    {
        public static event Action OnStateChanged;
        public static event Action<CardData, CardChoice, SwipeDirection> OnCardResolved;
        public static event Action<string> OnPolicyAdded;
        public static event Action OnWeekEnded;
        public static event Action<string> OnToast;
        public static event Action OnGameStarted;
        public static event Action OnScreenSwipe;
        public static event Action OnScreenGame;

        public static void StateChanged()                                          => OnStateChanged?.Invoke();
        public static void CardResolved(CardData card, CardChoice choice, SwipeDirection dir) => OnCardResolved?.Invoke(card, choice, dir);
        public static void PolicyAdded(string text)                                => OnPolicyAdded?.Invoke(text);
        public static void WeekEnded()                                             => OnWeekEnded?.Invoke();
        public static void Toast(string msg)                                       => OnToast?.Invoke(msg);
        public static void GameStarted()                                           => OnGameStarted?.Invoke();
        public static void ScreenSwipe()                                           => OnScreenSwipe?.Invoke();
        public static void ScreenGame()                                            => OnScreenGame?.Invoke();
    }
}
