using UnityEngine;

namespace Policy.Data
{
    public enum CardType { Decision, Investment, Policy, Event, Prestige }

    [CreateAssetMenu(menuName = "POLICY/Card", fileName = "Card_New")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public CardType type;
        public string   from;
        [TextArea(2, 4)] public string title;
        [TextArea(2, 4)] public string context;
        public Color accentColor = Color.white;

        [Header("Choices")]
        public CardChoice approve;
        public CardChoice decline;
        public CardChoice escalate;
        public CardChoice ignore;

        public CardChoice GetChoice(Policy.Core.SwipeDirection dir) => dir switch
        {
            Policy.Core.SwipeDirection.Approve  => approve,
            Policy.Core.SwipeDirection.Decline  => decline,
            Policy.Core.SwipeDirection.Escalate => escalate,
            Policy.Core.SwipeDirection.Ignore   => ignore,
            _                                   => null
        };
    }
}
