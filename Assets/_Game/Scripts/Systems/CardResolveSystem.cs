using UnityEngine;
using Policy.Core;
using Policy.Data;

namespace Policy.Systems
{
    /// <summary>
    /// Applies a card choice's deltas to GameState.
    /// Called by SwipeCardView after the fly-out animation completes.
    /// </summary>
    public static class CardResolveSystem
    {
        public static void Apply(CardData card, CardChoice choice, SwipeDirection dir)
        {
            var s = GameManager.Instance.state;

            s.incomePerHour = Mathf.Max(10f, s.incomePerHour + choice.incomeDelta);
            s.legacy        = Mathf.Clamp(s.legacy + choice.legacyDelta,  0, 100);
            s.risk          = Mathf.Clamp(s.risk   + choice.riskDelta,    0, 100);
            s.scope         = Mathf.Clamp(s.scope  + choice.scopeDelta,   0, 100);
            s.affected     += Mathf.Abs(choice.scopeDelta) * 12;
            s.decisionsThisWeek++;

            if (!string.IsNullOrEmpty(choice.unlocksAssetId)
                && !s.ownedAssetIds.Contains(choice.unlocksAssetId))
            {
                s.ownedAssetIds.Add(choice.unlocksAssetId);
            }

            if (!string.IsNullOrEmpty(choice.collateral))
                s.collateralLog.Add(choice.collateral);

            if (!string.IsNullOrEmpty(choice.policy))
                GameEvents.PolicyAdded(choice.policy);

            if (dir == SwipeDirection.Ignore && !string.IsNullOrEmpty(choice.returnNote))
                GameEvents.Toast(choice.returnNote);

            GameEvents.CardResolved(card, choice, dir);
            GameEvents.StateChanged();
        }
    }
}
