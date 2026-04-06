using UnityEngine;

namespace Policy.Data
{
    [CreateAssetMenu(menuName = "POLICY/MarketItem", fileName = "Market_New")]
    public class MarketItemData : ScriptableObject
    {
        public string assetId;
        public string displayName;
        public string sector;
        [TextArea(1, 3)] public string description;
        public int    price;
        public string incomeLabel;
        public Color  iconBackground = Color.white;
        public string iconEmoji;

        [Header("Lock Condition")]
        public bool  hasLockCondition;
        public float unlockAtBalance;
    }
}
